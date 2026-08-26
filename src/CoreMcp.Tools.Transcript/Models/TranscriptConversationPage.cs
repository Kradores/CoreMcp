namespace CoreMcp.Tools.Transcript.Models;

public sealed record TranscriptConversationPage(
    string ConversationRef,
    string StartedAt,
    string EndedAt,
    IReadOnlyList<TranscriptSegment> Segments,
    string? NextCursor);
