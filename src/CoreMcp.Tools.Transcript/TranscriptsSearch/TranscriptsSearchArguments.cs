namespace CoreMcp.Tools.Transcript.TranscriptsSearch;

public sealed record TranscriptsSearchArguments(
    string? Query = null,
    string? Date = null,
    string? FromUtc = null,
    string? ToUtc = null,
    string? Source = null,
    int? Limit = null);
