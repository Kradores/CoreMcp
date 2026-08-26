namespace CoreMcp.Tools.Transcript.Models;

public sealed record TranscriptSearchResult(
    IReadOnlyList<TranscriptConversationCandidate> Conversations);
