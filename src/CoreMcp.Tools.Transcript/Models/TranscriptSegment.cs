namespace CoreMcp.Tools.Transcript.Models;

public sealed record TranscriptSegment(
    string CreatedAt,
    string Source,
    object? StartTime,
    object? EndTime,
    string? Language,
    double? Confidence,
    string Text);
