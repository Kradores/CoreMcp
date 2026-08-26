using System.Text.Json;
using CoreMcp.Framework.Tools;
using CoreMcp.Protocol.Serializer;
using CoreMcp.Protocol.Tools;

namespace CoreMcp.Tools.Transcript.TranscriptsReadConversation;

public sealed class TranscriptsReadConversationTool
    : IMcpTool<TranscriptsReadConversationArguments>
{
    private readonly TranscriptRepository _repository;

    public TranscriptsReadConversationTool(TranscriptRepository repository)
    {
        _repository = repository;
    }

    public ToolDescriptor Definition => new(
        Name: "transcripts_read_conversation",
        Description:
            """
            Reads one conversation returned by transcripts_search.

            Call this after selecting a conversationRef from transcripts_search and before quoting or summarizing it. It returns chronological raw transcript segments and preserves source as an audio channel. microphone is the local channel; system_audio can contain several people and must not be treated as a reliable speaker label.

            Use nextCursor as cursor when the result is paginated. maxCharacters controls the text budget for a page.
            """);

    public async Task<CallToolResponse> ExecuteAsync(
        TranscriptsReadConversationArguments arguments,
        CancellationToken cancellationToken)
    {
        var result = await _repository.ReadConversationAsync(
            arguments,
            cancellationToken);

        return new CallToolResponse(
        [
            new TextContent(JsonSerializer.Serialize(
                result,
                JsonRpcSerializer.Options))
        ]);
    }
}
