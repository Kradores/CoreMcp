using System.Text.RegularExpressions;
using Microsoft.Data.Sqlite;

namespace CoreMcp.Tools.Transcript;

internal sealed class TranscriptDatabaseInitializer(TranscriptDatabaseOptions options)
{
    private static readonly string Script = ReadScript();
    private static readonly string[] RequiredColumns =
        ["created_at", "source", "start_time", "end_time", "language", "confidence", "text"];
    private static readonly string[] IndexObjects =
        ["transcripts_fts", "transcripts_fts_after_insert", "transcripts_fts_after_delete", "transcripts_fts_after_update_text"];

    public async Task<SqliteConnection> OpenAsync(CancellationToken cancellationToken)
    {
        var builder = CreateBuilder();
        try
        {
            var connection = await OpenReadAsync(builder, cancellationToken);
            try
            {
                if (await IsCompleteAsync(connection, null, cancellationToken))
                    return connection;
            }
            catch
            {
                await connection.DisposeAsync();
                throw;
            }
            await connection.DisposeAsync();

            builder.Mode = SqliteOpenMode.ReadWrite;
            await using (var writer = new SqliteConnection(builder.ToString()))
            {
                await writer.OpenAsync(cancellationToken);
                cancellationToken.ThrowIfCancellationRequested();
                using var transaction = writer.BeginTransaction(deferred: false);
                if (!await IsCompleteAsync(writer, transaction, cancellationToken))
                {
                    await using var command = writer.CreateCommand();
                    command.Transaction = transaction;
                    command.CommandText = Script;
                    await command.ExecuteNonQueryAsync(cancellationToken);
                }
                cancellationToken.ThrowIfCancellationRequested();
                transaction.Commit();
            }

            connection = await OpenReadAsync(builder, cancellationToken);
            try
            {
                if (!await IsCompleteAsync(connection, null, cancellationToken))
                    throw new TranscriptDatabaseUnavailableException("The transcript database changed during search-index repair. Retry the request.");
                return connection;
            }
            catch
            {
                await connection.DisposeAsync();
                throw;
            }
        }
        catch (SqliteException error)
        {
            cancellationToken.ThrowIfCancellationRequested();
            throw TranscriptDatabaseUnavailableException.FromSqlite(error);
        }
    }

    private SqliteConnectionStringBuilder CreateBuilder()
    {
        if (string.IsNullOrWhiteSpace(options.ConnectionString))
            throw new TranscriptDatabaseUnavailableException($"Set {TranscriptDatabaseOptions.ConnectionStringEnvironmentVariable} to the existing transcript database location.");
        try
        {
            var builder = new SqliteConnectionStringBuilder(options.ConnectionString)
            {
                Pooling = false,
                Mode = SqliteOpenMode.ReadOnly
            };
            if (string.IsNullOrWhiteSpace(builder.DataSource) || builder.DataSource == ":memory:")
                throw new ArgumentException();
            if (builder.DefaultTimeout <= 0)
                builder.DefaultTimeout = 30;
            return builder;
        }
        catch (ArgumentException)
        {
            throw new TranscriptDatabaseUnavailableException("The transcript database connection configuration is invalid. Configure an existing database file.");
        }
    }

    private static async Task<SqliteConnection> OpenReadAsync(SqliteConnectionStringBuilder builder, CancellationToken token)
    {
        builder.Mode = SqliteOpenMode.ReadOnly;
        var connection = new SqliteConnection(builder.ToString());
        try
        {
            await connection.OpenAsync(token);
            return connection;
        }
        catch
        {
            await connection.DisposeAsync();
            throw;
        }
    }

    private static async Task<bool> IsCompleteAsync(SqliteConnection connection, SqliteTransaction? transaction, CancellationToken token)
    {
        await using var command = connection.CreateCommand();
        command.Transaction = transaction;
        command.CommandText = "SELECT type FROM sqlite_master WHERE name = 'transcripts';";
        if (await command.ExecuteScalarAsync(token) is not string type || type != "table")
            throw new TranscriptDatabaseUnavailableException("The transcripts table is missing. Start the audio transcription service to initialize the database, then retry.");

        command.CommandText = "PRAGMA table_info(transcripts);";
        var columns = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        await using (var reader = await command.ExecuteReaderAsync(token))
            while (await reader.ReadAsync(token))
                columns.Add(reader.GetString(1));
        if (RequiredColumns.Any(column => !columns.Contains(column)))
            throw new TranscriptDatabaseUnavailableException("The transcripts table has an incompatible schema. Check the audio transcription service database configuration.");
        command.CommandText = "SELECT rowid FROM transcripts LIMIT 0;";
        await command.ExecuteNonQueryAsync(token);

        var complete = true;
        foreach (var name in IndexObjects)
        {
            command.CommandText = "SELECT type, tbl_name, sql FROM sqlite_master WHERE name = $name;";
            command.Parameters.Clear();
            command.Parameters.AddWithValue("$name", name);
            await using var reader = await command.ExecuteReaderAsync(token);
            if (!await reader.ReadAsync(token))
            {
                complete = false;
                continue;
            }
            var isTable = name == "transcripts_fts";
            var expected = Regex.Match(Script, isTable
                ? @"CREATE VIRTUAL TABLE IF NOT EXISTS transcripts_fts\b[^;]+;"
                : $@"CREATE TRIGGER IF NOT EXISTS {name}\b.*?END;", RegexOptions.Singleline).Value;
            if (reader.GetString(0) != (isTable ? "table" : "trigger") ||
                reader.GetString(1) != (isTable ? name : "transcripts") ||
                reader.IsDBNull(2) || Normalize(reader.GetString(2)) != Normalize(expected))
                throw new TranscriptDatabaseUnavailableException($"The search-index object {name} has an incompatible definition. CoreMcp cannot safely repair it automatically.");
        }
        return complete;
    }

    // Ignore formatting and SQLite's removal of IF NOT EXISTS, but reject unknown definitions.
    private static string Normalize(string sql)
    {
        sql = Regex.Replace(sql.Trim(), @"^(CREATE\s+(?:VIRTUAL\s+TABLE|TRIGGER))\s+IF\s+NOT\s+EXISTS\b",
            "$1", RegexOptions.IgnoreCase);
        // Preserve quoted text: whitespace or case inside a literal can change its meaning.
        return Regex.Replace(sql, """'(?:''|[^'])*'|"(?:""|[^"])*"|\[[^\]]*\]|`(?:``|[^`])*`|\s+|.""",
            match => char.IsWhiteSpace(match.Value[0]) ? "" :
                "'\"[`".Contains(match.Value[0]) ? match.Value : match.Value.ToLowerInvariant())
            .TrimEnd(';');
    }

    private static string ReadScript()
    {
        using var stream = typeof(TranscriptDatabaseInitializer).Assembly.GetManifestResourceStream(
            "CoreMcp.Tools.Transcript.Sql.transcripts-fts5.sql")
            ?? throw new InvalidOperationException("Embedded transcript index script is missing.");
        using var reader = new StreamReader(stream);
        return reader.ReadToEnd();
    }
}
