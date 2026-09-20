using CoreMcp.Protocol.Exceptions;
using CoreMcp.Protocol.Tools;
using CoreMcp.Tools.Transcript.TranscriptsSearch;
using Microsoft.Data.Sqlite;
using Xunit;

namespace CoreMcp.Tools.Transcript.Tests;

public sealed class TranscriptRecoveryTests : IDisposable
{
    private readonly string _path = Path.Combine(Path.GetTempPath(), $"CoreMcp.Recovery.{Guid.NewGuid():N}.db");
    private TranscriptRepository Repository => new(new($"Data Source={_path};Default Timeout=1"));
    private const string Schema = "CREATE TABLE transcripts(created_at TEXT, source TEXT, start_time REAL, end_time REAL, language TEXT, confidence REAL, text TEXT);";
    private const string Insert = "INSERT INTO transcripts VALUES('2026-08-21T21:30:00.000000+00:00','microphone',0,1,'en',0.9,'budget');";

    private void Execute(string sql)
    {
        using var connection = new SqliteConnection($"Data Source={_path};Pooling=False");
        connection.Open();
        using var command = connection.CreateCommand();
        command.CommandText = sql;
        command.ExecuteNonQuery();
    }

    private async Task<int> Search(string word, TranscriptRepository? repository = null) =>
        (await (repository ?? Repository).SearchAsync(new(Query: word))).Conversations.Count;

    [Fact]
    public async Task Provisions_populated_database_and_synchronizes_all_mutations()
    {
        Execute(Schema + Insert);
        Assert.Equal(1, await Search("budget"));
        Execute("UPDATE transcripts SET text='forecast';");
        Assert.Equal(0, await Search("budget"));
        Assert.Equal(1, await Search("forecast"));
        Execute("DELETE FROM transcripts;");
        Assert.Equal(0, await Search("forecast"));
        Execute(Insert);
        Assert.Equal(1, await Search("budget"));
    }

    [Fact]
    public async Task Repairs_missing_trigger_and_resynchronizes_existing_rows()
    {
        Execute(Schema);
        await Repository.ValidateAsync();
        Execute("DROP TRIGGER transcripts_fts_after_insert;" + Insert);
        Assert.Equal(1, await Search("budget"));
    }

    [Fact]
    public async Task Repairs_missing_fts_table_even_when_triggers_remain()
    {
        Execute(Schema + Insert);
        await Repository.ValidateAsync();
        Execute("DROP TABLE transcripts_fts;");
        Assert.Equal(1, await Search("budget"));
        Execute("UPDATE transcripts SET text='forecast';");
        Assert.Equal(1, await Search("forecast"));
    }

    [Fact]
    public async Task Healthy_index_is_not_rebuilt_and_needs_no_write_lock()
    {
        Execute(Schema + Insert);
        await Repository.ValidateAsync();
        // Deliberately remove an index entry while leaving the schema healthy.
        Execute("INSERT INTO transcripts_fts(transcripts_fts,rowid,text) VALUES('delete',1,'budget');");
        using var writer = new SqliteConnection($"Data Source={_path};Pooling=False");
        writer.Open();
        using var transaction = writer.BeginTransaction();
        Assert.Equal(0, await Search("budget"));
    }

    [Fact]
    public async Task Same_repository_recovers_after_file_deletion_and_recreation()
    {
        Execute(Schema + Insert);
        var repository = Repository;
        Assert.Equal(1, await Search("budget", repository));
        File.Delete(_path);
        await Assert.ThrowsAsync<TranscriptDatabaseUnavailableException>(() => repository.ValidateAsync());
        Assert.False(File.Exists(_path));
        Execute(Schema + Insert);
        Assert.Equal(1, await Search("budget", repository));
    }

    [Fact]
    public async Task Concurrent_initializers_provision_once_safely()
    {
        Execute(Schema + Insert);
        await Task.WhenAll(Enumerable.Range(0, 6).Select(_ => Task.Run(() => Repository.ValidateAsync())));
        Assert.Equal(1, await Search("budget"));
    }

    [Theory]
    [InlineData("CREATE TABLE transcripts_fts(text TEXT);")]
    [InlineData("CREATE TRIGGER transcripts_fts_after_insert AFTER INSERT ON transcripts BEGIN SELECT 1; END;")]
    [InlineData("CREATE VIRTUAL TABLE transcripts_fts USING fts5(text, content='trans cripts', content_rowid='rowid');")]
    public async Task Rejects_conflicting_objects_without_modifying_them(string conflict)
    {
        Execute(Schema + conflict);
        var error = await Assert.ThrowsAsync<TranscriptDatabaseUnavailableException>(() => Repository.ValidateAsync());
        Assert.Contains("incompatible", error.Message);
        Assert.DoesNotContain(_path, error.Message);
    }

    [Fact]
    public async Task Failed_provisioning_rolls_back_schema_and_can_be_retried()
    {
        Execute(Schema + "CREATE VIEW transcripts_fts_data AS SELECT 1;");
        await Assert.ThrowsAsync<TranscriptDatabaseUnavailableException>(() => Repository.ValidateAsync());
        using (var connection = new SqliteConnection($"Data Source={_path};Pooling=False"))
        {
            connection.Open();
            using var command = connection.CreateCommand();
            command.CommandText = "SELECT count(*) FROM sqlite_master WHERE name IN ('transcripts_fts','transcripts_fts_after_insert');";
            Assert.Equal(0L, command.ExecuteScalar());
        }
        Execute("DROP VIEW transcripts_fts_data;");
        await Repository.ValidateAsync();
    }

    [Fact]
    public async Task Lock_timeout_returns_tool_error_and_next_call_recovers()
    {
        Execute(Schema);
        using (var writer = new SqliteConnection($"Data Source={_path};Pooling=False"))
        {
            writer.Open();
            using var transaction = writer.BeginTransaction();
            var response = await new TranscriptsSearchTool(Repository).ExecuteAsync(new(), default);
            Assert.True(response.IsError);
            Assert.Contains("busy", Assert.IsType<TextContent>(Assert.Single(response.Content)).Text);
        }
        await Repository.ValidateAsync();
    }

    [Fact]
    public async Task Missing_database_is_tool_error_but_invalid_arguments_and_cancellation_propagate()
    {
        var tool = new TranscriptsSearchTool(Repository);
        Assert.True((await tool.ExecuteAsync(new(), default)).IsError);
        Assert.False(File.Exists(_path));
        await Assert.ThrowsAsync<InvalidParamsException>(() => tool.ExecuteAsync(new(Source: "invalid"), default));
        Execute(Schema);
        using var cancellation = new CancellationTokenSource();
        cancellation.Cancel();
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => tool.ExecuteAsync(new(), cancellation.Token));
    }

    [Fact]
    public async Task Readonly_file_can_be_queried_but_cannot_be_repaired()
    {
        Execute(Schema + Insert);
        File.SetAttributes(_path, FileAttributes.ReadOnly);
        try
        {
            var response = await new TranscriptsSearchTool(Repository).ExecuteAsync(new(), default);
            Assert.True(response.IsError);
        }
        finally { File.SetAttributes(_path, FileAttributes.Normal); }
        await Repository.ValidateAsync();
        File.SetAttributes(_path, FileAttributes.ReadOnly);
        try { Assert.Equal(1, await Search("budget")); }
        finally { File.SetAttributes(_path, FileAttributes.Normal); }
    }

    [Theory]
    [InlineData("CREATE TABLE unrelated(id INTEGER);")]
    [InlineData("CREATE TABLE transcripts(text TEXT);")]
    public async Task Missing_or_incompatible_source_is_not_modified(string schema)
    {
        Execute(schema);
        await Assert.ThrowsAsync<TranscriptDatabaseUnavailableException>(() => Repository.ValidateAsync());
    }

    public void Dispose()
    {
        if (File.Exists(_path))
        {
            File.SetAttributes(_path, FileAttributes.Normal);
            File.Delete(_path);
        }
    }
}
