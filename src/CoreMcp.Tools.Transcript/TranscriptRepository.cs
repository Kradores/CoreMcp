using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using CoreMcp.Protocol.Exceptions;
using CoreMcp.Tools.Transcript.Models;
using CoreMcp.Tools.Transcript.TranscriptsReadConversation;
using CoreMcp.Tools.Transcript.TranscriptsSearch;
using Microsoft.Data.Sqlite;

namespace CoreMcp.Tools.Transcript;

public sealed class TranscriptRepository
{
    private const int DefaultSearchLimit = 5;
    private const int MaxSearchLimit = 10;
    private const int DefaultMaxCharacters = 20_000;
    private const int MaxMaxCharacters = 50_000;
    private static readonly TimeSpan ConversationGap = TimeSpan.FromMinutes(5);
    private readonly TranscriptDatabaseInitializer _initializer;

    public TranscriptRepository(TranscriptDatabaseOptions options)
    {
        _initializer = new TranscriptDatabaseInitializer(options);
    }

    public async Task ValidateAsync(CancellationToken cancellationToken = default)
    {
        await using var connection = await OpenValidatedConnectionAsync(
            cancellationToken);
    }

    public async Task<TranscriptSearchResult> SearchAsync(
        TranscriptsSearchArguments arguments,
        CancellationToken cancellationToken = default)
    {
        var criteria = SearchCriteria.Create(arguments);

        await using var connection = await OpenValidatedConnectionAsync(
            cancellationToken);

        var anchors = await FindAnchorsAsync(
            connection,
            criteria,
            cancellationToken);

        var candidates = new List<TranscriptConversationCandidate>();
        var seenConversations = new HashSet<long>();

        foreach (var anchor in anchors)
        {
            var conversation = await ReadConversationAsync(
                connection,
                anchor,
                cancellationToken);

            var firstRowId = conversation[0].RowId;

            if (!seenConversations.Add(firstRowId))
                continue;

            candidates.Add(CreateCandidate(conversation, anchor));

            if (candidates.Count == criteria.Limit)
                break;
        }

        return new TranscriptSearchResult(candidates);
    }

    public async Task<TranscriptConversationPage> ReadConversationAsync(
        TranscriptsReadConversationArguments arguments,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(arguments.ConversationRef))
            throw new InvalidParamsException("conversationRef is required.");

        var firstRowId = DecodeReference(arguments.ConversationRef, "conversationRef");
        var maxCharacters = ValidateMaxCharacters(arguments.MaxCharacters);

        await using var connection = await OpenValidatedConnectionAsync(
            cancellationToken);

        var anchor = await FindRowByIdAsync(
            connection,
            firstRowId,
            cancellationToken);

        if (anchor is null)
        {
            throw new InvalidParamsException(
                "conversationRef does not identify an available transcript row.");
        }

        var conversation = await ReadConversationAsync(
            connection,
            anchor,
            cancellationToken);

        var actualFirstRowId = conversation[0].RowId;
        var conversationRef = EncodeReference(actualFirstRowId);
        var startIndex = GetPageStartIndex(conversation, arguments.Cursor);
        var segments = new List<TranscriptSegment>();
        var characterCount = 0;
        var nextIndex = startIndex;

        while (nextIndex < conversation.Count)
        {
            var row = conversation[nextIndex];

            if (segments.Count > 0 &&
                characterCount + row.Text.Length > maxCharacters)
            {
                break;
            }

            segments.Add(ToSegment(row));
            characterCount += row.Text.Length;
            nextIndex++;
        }

        var nextCursor = nextIndex < conversation.Count
            ? EncodeReference(conversation[nextIndex - 1].RowId)
            : null;

        return new TranscriptConversationPage(
            ConversationRef: conversationRef,
            StartedAt: FormatUtc(conversation[0].CreatedAt),
            EndedAt: FormatUtc(conversation[^1].CreatedAt),
            Segments: segments,
            NextCursor: nextCursor);
    }

    private Task<SqliteConnection> OpenValidatedConnectionAsync(CancellationToken cancellationToken) =>
        _initializer.OpenAsync(cancellationToken);

    private static async Task<IReadOnlyList<StoredTranscript>> FindAnchorsAsync(
        SqliteConnection connection,
        SearchCriteria criteria,
        CancellationToken cancellationToken)
    {
        var sql = new StringBuilder();

        sql.Append("SELECT t.rowid, t.created_at, t.source, t.start_time, t.end_time, t.language, t.confidence, t.text ");

        if (criteria.FtsQuery is not null)
            sql.Append("FROM transcripts_fts JOIN transcripts t ON t.rowid = transcripts_fts.rowid ");
        else
            sql.Append("FROM transcripts t ");

        var filters = new List<string>();
        if (criteria.FtsQuery is not null)
            filters.Add("transcripts_fts MATCH $match");
        if (criteria.FromUtc is not null)
            filters.Add("t.created_at >= $fromUtc");
        if (criteria.ToUtc is not null)
            filters.Add("t.created_at < $toUtc");
        if (criteria.Source is not null)
            filters.Add("t.source = $source");

        if (filters.Count > 0)
            sql.Append("WHERE ").Append(string.Join(" AND ", filters)).Append(' ');

        sql.Append("ORDER BY t.created_at DESC, t.rowid DESC LIMIT $limit;");

        await using var command = connection.CreateCommand();
        command.CommandText = sql.ToString();
        command.Parameters.AddWithValue("$limit", criteria.AnchorLimit);

        if (criteria.FtsQuery is not null)
            command.Parameters.AddWithValue("$match", criteria.FtsQuery);
        if (criteria.FromUtc is not null)
            command.Parameters.AddWithValue("$fromUtc", FormatUtc(criteria.FromUtc.Value));
        if (criteria.ToUtc is not null)
            command.Parameters.AddWithValue("$toUtc", FormatUtc(criteria.ToUtc.Value));
        if (criteria.Source is not null)
            command.Parameters.AddWithValue("$source", criteria.Source);

        return await ReadRowsAsync(command, cancellationToken);
    }

    private static async Task<List<StoredTranscript>> ReadConversationAsync(
        SqliteConnection connection,
        StoredTranscript anchor,
        CancellationToken cancellationToken)
    {
        var before = new List<StoredTranscript>();
        var current = anchor;

        while (true)
        {
            var previous = await FindAdjacentRowAsync(
                connection,
                current,
                previous: true,
                cancellationToken);

            if (previous is null || current.CreatedAt - previous.CreatedAt >= ConversationGap)
                break;

            before.Add(previous);
            current = previous;
        }

        before.Reverse();

        var conversation = new List<StoredTranscript>(before.Count + 1);
        conversation.AddRange(before);
        conversation.Add(anchor);

        current = anchor;
        while (true)
        {
            var next = await FindAdjacentRowAsync(
                connection,
                current,
                previous: false,
                cancellationToken);

            if (next is null || next.CreatedAt - current.CreatedAt >= ConversationGap)
                break;

            conversation.Add(next);
            current = next;
        }

        return conversation;
    }

    private static async Task<StoredTranscript?> FindAdjacentRowAsync(
        SqliteConnection connection,
        StoredTranscript row,
        bool previous,
        CancellationToken cancellationToken)
    {
        await using var command = connection.CreateCommand();
        command.CommandText = previous
            ? """
              SELECT rowid, created_at, source, start_time, end_time, language, confidence, text
              FROM transcripts
              WHERE created_at < $createdAt
                 OR (created_at = $createdAt AND rowid < $rowId)
              ORDER BY created_at DESC, rowid DESC
              LIMIT 1;
              """
            : """
              SELECT rowid, created_at, source, start_time, end_time, language, confidence, text
              FROM transcripts
              WHERE created_at > $createdAt
                 OR (created_at = $createdAt AND rowid > $rowId)
              ORDER BY created_at ASC, rowid ASC
              LIMIT 1;
              """;
        command.Parameters.AddWithValue("$createdAt", FormatUtc(row.CreatedAt));
        command.Parameters.AddWithValue("$rowId", row.RowId);

        var rows = await ReadRowsAsync(command, cancellationToken);
        return rows.Count == 0 ? null : rows[0];
    }

    private static async Task<StoredTranscript?> FindRowByIdAsync(
        SqliteConnection connection,
        long rowId,
        CancellationToken cancellationToken)
    {
        await using var command = connection.CreateCommand();
        command.CommandText =
            "SELECT rowid, created_at, source, start_time, end_time, language, confidence, text " +
            "FROM transcripts WHERE rowid = $rowId;";
        command.Parameters.AddWithValue("$rowId", rowId);

        var rows = await ReadRowsAsync(command, cancellationToken);
        return rows.Count == 0 ? null : rows[0];
    }

    private static async Task<IReadOnlyList<StoredTranscript>> ReadRowsAsync(
        SqliteCommand command,
        CancellationToken cancellationToken)
    {
        var rows = new List<StoredTranscript>();
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);

        while (await reader.ReadAsync(cancellationToken))
        {
            rows.Add(new StoredTranscript(
                RowId: reader.GetInt64(0),
                CreatedAt: ParseStoredUtc(reader.GetString(1)),
                Source: reader.GetString(2),
                StartTime: ReadValue(reader, 3),
                EndTime: ReadValue(reader, 4),
                Language: reader.IsDBNull(5) ? null : reader.GetString(5),
                Confidence: reader.IsDBNull(6) ? null : Convert.ToDouble(reader.GetValue(6), CultureInfo.InvariantCulture),
                Text: reader.IsDBNull(7) ? string.Empty : reader.GetString(7)));
        }

        return rows;
    }

    private static TranscriptConversationCandidate CreateCandidate(
        IReadOnlyList<StoredTranscript> conversation,
        StoredTranscript anchor)
    {
        var confidences = conversation
            .Where(row => row.Confidence is not null)
            .Select(row => row.Confidence!.Value)
            .ToArray();

        return new TranscriptConversationCandidate(
            ConversationRef: EncodeReference(conversation[0].RowId),
            StartedAt: FormatUtc(conversation[0].CreatedAt),
            EndedAt: FormatUtc(conversation[^1].CreatedAt),
            Sources: conversation
                .Select(row => row.Source)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToArray(),
            Excerpt: CreateExcerpt(anchor.Text),
            Language: anchor.Language,
            AverageConfidence: confidences.Length == 0 ? null : confidences.Average());
    }

    private static TranscriptSegment ToSegment(StoredTranscript row) => new(
        CreatedAt: FormatUtc(row.CreatedAt),
        Source: row.Source,
        StartTime: row.StartTime,
        EndTime: row.EndTime,
        Language: row.Language,
        Confidence: row.Confidence,
        Text: row.Text);

    private static int GetPageStartIndex(
        IReadOnlyList<StoredTranscript> conversation,
        string? cursor)
    {
        if (string.IsNullOrWhiteSpace(cursor))
            return 0;

        var rowId = DecodeReference(cursor, "cursor");
        var index = conversation
            .Select((row, index) => (row, index))
            .FirstOrDefault(item => item.row.RowId == rowId)
            .index;

        if (index == 0 && conversation[0].RowId != rowId)
            throw new InvalidParamsException("cursor does not belong to this conversation.");

        return index + 1;
    }

    private static int ValidateMaxCharacters(int? requested)
    {
        var value = requested ?? DefaultMaxCharacters;

        if (value is < 1 or > MaxMaxCharacters)
        {
            throw new InvalidParamsException(
                $"maxCharacters must be between 1 and {MaxMaxCharacters}.");
        }

        return value;
    }

    private static object? ReadValue(SqliteDataReader reader, int ordinal) =>
        reader.IsDBNull(ordinal) ? null : reader.GetValue(ordinal);

    private static string CreateExcerpt(string text)
    {
        var normalized = Regex.Replace(text, "\\s+", " ").Trim();
        return normalized.Length <= 500 ? normalized : normalized[..500] + "…";
    }

    private static DateTimeOffset ParseStoredUtc(string value)
    {
        if (DateTimeOffset.TryParseExact(
                value,
                "yyyy-MM-dd'T'HH:mm:ss.ffffffzzz",
                CultureInfo.InvariantCulture,
                DateTimeStyles.None,
                out var parsed) &&
            parsed.Offset == TimeSpan.Zero)
        {
            return parsed;
        }

        throw new InvalidOperationException(
            "transcripts.created_at must use UTC format yyyy-MM-ddTHH:mm:ss.ffffff+00:00.");
    }

    private static string FormatUtc(DateTimeOffset value) =>
        value.ToUniversalTime().ToString(
            "yyyy-MM-dd'T'HH:mm:ss.ffffff'+00:00'",
            CultureInfo.InvariantCulture);

    private static string EncodeReference(long rowId)
    {
        var bytes = Encoding.UTF8.GetBytes($"v1:{rowId}");
        return Convert.ToBase64String(bytes)
            .TrimEnd('=')
            .Replace('+', '-')
            .Replace('/', '_');
    }

    private static long DecodeReference(string value, string parameterName)
    {
        try
        {
            var base64 = value.Replace('-', '+').Replace('_', '/');
            base64 = base64.PadRight(base64.Length + (4 - base64.Length % 4) % 4, '=');
            var decoded = Encoding.UTF8.GetString(Convert.FromBase64String(base64));

            if (!decoded.StartsWith("v1:", StringComparison.Ordinal) ||
                !long.TryParse(decoded[3..], NumberStyles.None, CultureInfo.InvariantCulture, out var rowId) ||
                rowId <= 0)
            {
                throw new FormatException();
            }

            return rowId;
        }
        catch (FormatException)
        {
            throw new InvalidParamsException($"{parameterName} is invalid.");
        }
    }

    private sealed record StoredTranscript(
        long RowId,
        DateTimeOffset CreatedAt,
        string Source,
        object? StartTime,
        object? EndTime,
        string? Language,
        double? Confidence,
        string Text);

    private sealed record SearchCriteria(
        string? FtsQuery,
        DateTimeOffset? FromUtc,
        DateTimeOffset? ToUtc,
        string? Source,
        int Limit)
    {
        public int AnchorLimit => checked(Limit * 20);

        public static SearchCriteria Create(TranscriptsSearchArguments arguments)
        {
            if (!string.IsNullOrWhiteSpace(arguments.Date) &&
                (!string.IsNullOrWhiteSpace(arguments.FromUtc) ||
                 !string.IsNullOrWhiteSpace(arguments.ToUtc)))
            {
                throw new InvalidParamsException(
                    "date cannot be combined with fromUtc or toUtc.");
            }

            var (fromUtc, toUtc) = !string.IsNullOrWhiteSpace(arguments.Date)
                ? GetLocalDateRange(arguments.Date)
                : (ParseInputUtc(arguments.FromUtc, "fromUtc"), ParseInputUtc(arguments.ToUtc, "toUtc"));

            if (fromUtc is not null && toUtc is not null && fromUtc >= toUtc)
                throw new InvalidParamsException("fromUtc must be before toUtc.");

            var source = string.IsNullOrWhiteSpace(arguments.Source)
                ? null
                : arguments.Source.Trim();

            if (source is not null &&
                !string.Equals(source, "system_audio", StringComparison.OrdinalIgnoreCase) &&
                !string.Equals(source, "microphone", StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidParamsException(
                    "source must be system_audio or microphone.");
            }

            var limit = arguments.Limit ??
                (string.IsNullOrWhiteSpace(arguments.Query) &&
                 string.IsNullOrWhiteSpace(arguments.Date) &&
                 string.IsNullOrWhiteSpace(arguments.FromUtc) &&
                 string.IsNullOrWhiteSpace(arguments.ToUtc) &&
                 source is null
                    ? 1
                    : DefaultSearchLimit);
            if (limit is < 1 or > MaxSearchLimit)
            {
                throw new InvalidParamsException(
                    $"limit must be between 1 and {MaxSearchLimit}.");
            }

            return new SearchCriteria(
                FtsQuery: BuildFtsQuery(arguments.Query),
                FromUtc: fromUtc,
                ToUtc: toUtc,
                Source: source,
                Limit: limit);
        }

        private static string? BuildFtsQuery(string? query)
        {
            if (string.IsNullOrWhiteSpace(query))
                return null;

            var terms = Regex.Matches(query, "[\\p{L}\\p{N}_-]+")
                .Select(match => match.Value)
                .ToArray();

            if (terms.Length == 0)
                throw new InvalidParamsException("query must contain letters or numbers.");

            return string.Join(" AND ", terms.Select(term => $"\"{term}\""));
        }

        private static DateTimeOffset? ParseInputUtc(
            string? value,
            string parameterName)
        {
            if (string.IsNullOrWhiteSpace(value))
                return null;

            if (!DateTimeOffset.TryParseExact(
                    value,
                    "yyyy-MM-dd'T'HH:mm:ss.ffffffzzz",
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.None,
                    out var parsed) ||
                parsed.Offset != TimeSpan.Zero)
            {
                throw new InvalidParamsException(
                    $"{parameterName} must use UTC format yyyy-MM-ddTHH:mm:ss.ffffff+00:00.");
            }

            return parsed;
        }

        private static (DateTimeOffset FromUtc, DateTimeOffset ToUtc) GetLocalDateRange(
            string date)
        {
            if (!DateOnly.TryParseExact(
                    date,
                    "yyyy-MM-dd",
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.None,
                    out var parsed))
            {
                throw new InvalidParamsException("date must use format yyyy-MM-dd.");
            }

            var zone = GetMadridTimeZone();
            var start = parsed.ToDateTime(TimeOnly.MinValue, DateTimeKind.Unspecified);
            var end = parsed.AddDays(1).ToDateTime(TimeOnly.MinValue, DateTimeKind.Unspecified);

            return (
                new DateTimeOffset(TimeZoneInfo.ConvertTimeToUtc(start, zone)),
                new DateTimeOffset(TimeZoneInfo.ConvertTimeToUtc(end, zone)));
        }

        private static TimeZoneInfo GetMadridTimeZone()
        {
            try
            {
                return TimeZoneInfo.FindSystemTimeZoneById("Europe/Madrid");
            }
            catch (TimeZoneNotFoundException)
            {
                return TimeZoneInfo.FindSystemTimeZoneById("Romance Standard Time");
            }
        }
    }
}
