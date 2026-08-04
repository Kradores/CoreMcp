using System.Text;
using CoreMcp.Tools.FileSystem.FsPdfRead;
using CoreMcp.Tools.FileSystem.Internal;
using UglyToad.PdfPig;
using UglyToad.PdfPig.DocumentLayoutAnalysis.TextExtractor;

namespace CoreMcp.Tools.FileSystem.Services;

public sealed class PdfReadService
{
    public Task<FsPdfReadResult> ReadAsync(
        string path,
        int startPage,
        int? endPage,
        int? maxCharacters,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);

        var fullPath = Path.GetFullPath(path);

        if (!File.Exists(fullPath))
            throw new FileNotFoundException("The specified PDF does not exist.", fullPath);

        if (!string.Equals(Path.GetExtension(fullPath), ".pdf", StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException(
                "fs_pdf_read only supports files with a .pdf extension.");
        }

        var limit = maxCharacters ?? FileSystemLimits.MaxCharacters;

        if (limit <= 0)
            throw new ArgumentOutOfRangeException(nameof(maxCharacters));

        using var document = PdfDocument.Open(fullPath);

        if (startPage < 1 || startPage > document.NumberOfPages)
        {
            throw new ArgumentOutOfRangeException(
                nameof(startPage),
                $"startPage must be between 1 and {document.NumberOfPages}.");
        }

        var finalPage = endPage ?? document.NumberOfPages;

        if (finalPage < startPage || finalPage > document.NumberOfPages)
        {
            throw new ArgumentOutOfRangeException(
                nameof(endPage),
                $"endPage must be between {startPage} and {document.NumberOfPages}.");
        }

        var content = new StringBuilder();
        var truncated = false;

        for (var pageNumber = startPage; pageNumber <= finalPage; pageNumber++)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var pageText = ContentOrderTextExtractor.GetText(
                document.GetPage(pageNumber));

            if (content.Length > 0 && pageText.Length > 0)
            {
                if (content.Length + Environment.NewLine.Length > limit)
                {
                    truncated = true;
                    break;
                }

                content.AppendLine();
            }

            var remaining = limit - content.Length;

            if (pageText.Length > remaining)
            {
                content.Append(pageText.AsSpan(0, remaining));
                truncated = true;
                break;
            }

            content.Append(pageText);
        }

        var file = new FileInfo(fullPath);

        return Task.FromResult(
            new FsPdfReadResult(
                Path: fullPath,
                Size: file.Length,
                PageCount: document.NumberOfPages,
                StartPage: startPage,
                EndPage: finalPage,
                Truncated: truncated,
                Content: content.ToString()));
    }
}
