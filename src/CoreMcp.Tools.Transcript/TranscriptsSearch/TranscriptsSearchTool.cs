using System.Text.Json;
using CoreMcp.Framework.Tools;
using CoreMcp.Protocol.Serializer;
using CoreMcp.Protocol.Tools;

namespace CoreMcp.Tools.Transcript.TranscriptsSearch;

public sealed class TranscriptsSearchTool : IMcpTool<TranscriptsSearchArguments>
{
    private readonly TranscriptRepository _repository;

    public TranscriptsSearchTool(TranscriptRepository repository)
    {
        _repository = repository;
    }

    public ToolDescriptor Definition => new(
        Name: "transcripts_search",
        Description:
            """
            Finds recorded conversations in the transcript database.

            Always use this tool before answering a question about a user's recorded conversation history, including a recent conversation, a conversation on a date, or a conversation about a topic. Use the returned conversationRef with transcripts_read_conversation before quoting or summarizing a selected conversation.

            query is an optional keyword or phrase hint. date is an optional local calendar date in yyyy-MM-dd and is interpreted in Europe/Madrid. fromUtc and toUtc are optional canonical UTC timestamps in yyyy-MM-ddTHH:mm:ss.ffffff+00:00. source is optional and can be system_audio or microphone. With no filters this returns the latest conversation.

            The result groups both audio channels into one conversation after five minutes without any audio. Source identifies an audio channel, not a person. Do not treat system_audio as reliable speaker attribution.
            """);

    public async Task<CallToolResponse> ExecuteAsync(
        TranscriptsSearchArguments arguments,
        CancellationToken cancellationToken)
    {
        var result = await _repository.SearchAsync(arguments, cancellationToken);

        return new CallToolResponse(
        [
            new TextContent(JsonSerializer.Serialize(
                result,
                JsonRpcSerializer.Options))
        ]);
    }
}
