namespace CoreMcp.Tools.Transcript.TranscriptsReadConversation;

public sealed record TranscriptsReadConversationArguments(
    string ConversationRef,
    string? Cursor = null,
    int? MaxCharacters = null);
