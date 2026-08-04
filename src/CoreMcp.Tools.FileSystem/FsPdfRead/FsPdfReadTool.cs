using System.Text.Json;
using CoreMcp.Framework.Tools;
using CoreMcp.Protocol.Serializer;
using CoreMcp.Protocol.Tools;
using CoreMcp.Tools.FileSystem.Services;

namespace CoreMcp.Tools.FileSystem.FsPdfRead;

public sealed class FsPdfReadTool : IMcpTool<FsPdfReadArguments>
{
    private readonly PdfReadService _service;

    public FsPdfReadTool(PdfReadService service)
    {
        _service = service;
    }

    public ToolDescriptor Definition => new(
        Name: "fs_pdf_read",
        Description:
        """
        Extracts text from a text-based PDF document.
        Pages are numbered from 1. Omit endPage to extract through the final page.
        Returns extracted text, the selected page range and whether output was truncated by the character limit.
        This tool does not perform OCR, so scanned or image-only PDFs may return little or no text.
        """);

    public async Task<CallToolResponse> ExecuteAsync(
        FsPdfReadArguments arguments,
        CancellationToken cancellationToken)
    {
        var result = await _service.ReadAsync(
            arguments.Path,
            arguments.StartPage,
            arguments.EndPage,
            arguments.MaxCharacters,
            cancellationToken);

        return new CallToolResponse(
        [
            new TextContent(
                JsonSerializer.Serialize(
                    result,
                    JsonRpcSerializer.Options))
        ]);
    }
}
