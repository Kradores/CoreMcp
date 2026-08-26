namespace CoreMcp.Tools.Transcript.Models;

public sealed record TranscriptConversationCandidate(
    string ConversationRef,
    string StartedAt,
    string EndedAt,
    IReadOnlyList<string> Sources,
    string Excerpt,
    string? Language,
    double? AverageConfidence);
