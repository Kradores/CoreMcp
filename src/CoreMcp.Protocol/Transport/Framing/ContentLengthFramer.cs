using System;
using System.Collections.Generic;
using System.Text;

namespace CoreMcp.Protocol.Transport.Framing;

public sealed class ContentLengthFramer : ITransportFramer
{
    public async Task<ReadOnlyMemory<byte>?> ReadMessageAsync(
        BufferedBinaryReader reader,
        CancellationToken cancellationToken)
    {
        int? contentLength = null;

        while (true)
        {
            var line = await reader.ReadLineAsync(cancellationToken);

            if (line is null)
                return null;

            if (line.Length == 0)
                break;

            if (line.StartsWith("Content-Length:",
                StringComparison.OrdinalIgnoreCase))
            {
                contentLength = int.Parse(
                    line["Content-Length:".Length..].Trim());
            }
        }

        if (contentLength is null)
            throw new InvalidOperationException(
                "Missing Content-Length header.");

        return await reader.ReadBytesAsync(
            contentLength.Value,
            cancellationToken);
    }

    public async Task WriteMessageAsync(
        Stream output,
        ReadOnlyMemory<byte> payload,
        CancellationToken cancellationToken)
    {
        var header =
            $"Content-Length: {payload.Length}\r\n\r\n";

        await output.WriteAsync(
            Encoding.ASCII.GetBytes(header),
            cancellationToken);

        await output.WriteAsync(
            payload,
            cancellationToken);

        await output.FlushAsync(cancellationToken);
    }
}
