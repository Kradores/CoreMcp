using CoreMcp.Protocol.Exceptions;
using CoreMcp.Tools.Transcript;
using CoreMcp.Tools.Transcript.TranscriptsReadConversation;
using CoreMcp.Tools.Transcript.TranscriptsSearch;
using Microsoft.Data.Sqlite;
using Xunit;

namespace CoreMcp.Tools.Transcript.Tests;

public sealed class TranscriptRepositoryTests : IDisposable
{
    private readonly string _databasePath = Path.Combine(
        Path.GetTempPath(),
        $"CoreMcp.TranscriptTests.{Guid.NewGuid():N}.db");

    public TranscriptRepositoryTests()
    {
        CreateDatabase();
    }

    [Fact]
    public async Task Search_groups_mixed_channels_until_the_five_minute_boundary()
    {
        var repository = CreateRepository();

        var result = await repository.SearchAsync(new TranscriptsSearchArguments(
            Query: "budget"));

        var conversation = Assert.Single(result.Conversations);
        Assert.Equal("2026-08-21T21:30:00.000000+00:00", conversation.StartedAt);
        Assert.Equal("2026-08-21T21:34:00.000000+00:00", conversation.EndedAt);
        Assert.Equal(["microphone", "system_audio"], conversation.Sources);
    }

    [Fact]
    public async Task Search_uses_madrid_calendar_dates_and_utc_bounds()
    {
        var repository = CreateRepository();

        var result = await repository.SearchAsync(new TranscriptsSearchArguments(
            Date: "2026-08-22"));

        var conversation = Assert.Single(result.Conversations);
        Assert.Equal("2026-08-21T22:30:00.000000+00:00", conversation.StartedAt);
    }

    [Fact]
    public async Task Empty_search_returns_the_latest_conversation()
    {
        var repository = CreateRepository();

        var result = await repository.SearchAsync(new TranscriptsSearchArguments());

        var conversation = Assert.Single(result.Conversations);
        Assert.Equal("2026-08-21T22:30:00.000000+00:00", conversation.StartedAt);
    }

    [Fact]
    public async Task Read_pages_a_selected_conversation()
    {
        var repository = CreateRepository();
        var found = await repository.SearchAsync(new TranscriptsSearchArguments(
            Query: "budget"));

        var firstPage = await repository.ReadConversationAsync(
            new TranscriptsReadConversationArguments(
                Assert.Single(found.Conversations).ConversationRef,
                MaxCharacters: 15));

        Assert.Single(firstPage.Segments);
        Assert.NotNull(firstPage.NextCursor);

        var secondPage = await repository.ReadConversationAsync(
            new TranscriptsReadConversationArguments(
                firstPage.ConversationRef,
                firstPage.NextCursor,
                500));

        Assert.Equal(2, secondPage.Segments.Count);
        Assert.Null(secondPage.NextCursor);
    }

    [Fact]
    public async Task Search_rejects_unsafe_or_invalid_filter_values()
    {
        var repository = CreateRepository();

        await Assert.ThrowsAsync<InvalidParamsException>(() => repository.SearchAsync(
            new TranscriptsSearchArguments(Source: "other")));
        await Assert.ThrowsAsync<InvalidParamsException>(() => repository.SearchAsync(
            new TranscriptsSearchArguments(Date: "2026/08/21")));
        await Assert.ThrowsAsync<InvalidParamsException>(() => repository.SearchAsync(
            new TranscriptsSearchArguments(Query: "!!!")));
    }

    [Fact]
    public async Task Provisions_a_missing_fts_index_for_an_empty_database()
    {
        var path = Path.Combine(Path.GetTempPath(), $"CoreMcp.NoFts.{Guid.NewGuid():N}.db");

        try
        {
            await using (var connection = new SqliteConnection($"Data Source={path};Pooling=False"))
            {
                await connection.OpenAsync();
                await using var command = connection.CreateCommand();
                command.CommandText =
                    "CREATE TABLE transcripts (created_at TEXT, source TEXT, start_time REAL, end_time REAL, language TEXT, confidence REAL, text TEXT);";
                await command.ExecuteNonQueryAsync();
            }

            var repository = new TranscriptRepository(
                new TranscriptDatabaseOptions($"Data Source={path};Pooling=False"));

            var result = await repository.SearchAsync(new TranscriptsSearchArguments());
            Assert.Empty(result.Conversations);
        }
        finally
        {
            if (File.Exists(path))
                File.Delete(path);
        }
    }

    public void Dispose()
    {
        if (File.Exists(_databasePath))
            File.Delete(_databasePath);
    }

    private TranscriptRepository CreateRepository() =>
        new(new TranscriptDatabaseOptions($"Data Source={_databasePath};Pooling=False"));

    private void CreateDatabase()
    {
        using var connection = new SqliteConnection($"Data Source={_databasePath};Pooling=False");
        connection.Open();

        using var setup = connection.CreateCommand();
        setup.CommandText =
            """
            CREATE TABLE transcripts (
                created_at TEXT NOT NULL,
                source TEXT NOT NULL,
                start_time REAL,
                end_time REAL,
                language TEXT,
                confidence REAL,
                text TEXT NOT NULL
            );
            CREATE VIRTUAL TABLE transcripts_fts
            USING fts5(text, content = 'transcripts', content_rowid = 'rowid');
            """;
        setup.ExecuteNonQuery();

        AddRow(connection, "2026-08-21T21:30:00.000000+00:00", "microphone", "Let's discuss the budget.");
        AddRow(connection, "2026-08-21T21:31:00.000000+00:00", "system_audio", "The budget needs approval.");
        AddRow(connection, "2026-08-21T21:34:00.000000+00:00", "microphone", "I can prepare the budget proposal.");
        AddRow(connection, "2026-08-21T21:39:00.000000+00:00", "system_audio", "This is a separate conversation.");
        AddRow(connection, "2026-08-21T22:30:00.000000+00:00", "microphone", "The latest conversation.");
    }

    private static void AddRow(
        SqliteConnection connection,
        string createdAt,
        string source,
        string text)
    {
        using var command = connection.CreateCommand();
        command.CommandText =
            "INSERT INTO transcripts(created_at, source, start_time, end_time, language, confidence, text) " +
            "VALUES($createdAt, $source, 0, 1, 'en', 0.9, $text);";
        command.Parameters.AddWithValue("$createdAt", createdAt);
        command.Parameters.AddWithValue("$source", source);
        command.Parameters.AddWithValue("$text", text);
        command.ExecuteNonQuery();

        command.CommandText =
            "INSERT INTO transcripts_fts(rowid, text) VALUES(last_insert_rowid(), $text);";
        command.ExecuteNonQuery();
    }
}
