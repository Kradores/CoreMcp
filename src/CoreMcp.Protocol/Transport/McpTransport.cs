using System.Text;

namespace CoreMcp.Protocol.Transport;

public sealed class McpTransport
{
    private readonly Stream _input;
    private readonly Stream _output;
    private readonly byte[] _singleByteBuffer = new byte[1];

    public McpTransport(Stream input, Stream output)
    {
        _input = input;
        _output = output;
    }

    public async Task WriteMessageAsync(
        ReadOnlyMemory<byte> message,
        CancellationToken cancellationToken = default)
    {
        var header =
            $"Content-Length: {message.Length}\r\n\r\n";

        var headerBytes = Encoding.ASCII.GetBytes(header);

        await _output.WriteAsync(headerBytes, cancellationToken);
        await _output.WriteAsync(message, cancellationToken);
        await _output.FlushAsync(cancellationToken);
    }

    public async Task<ReadOnlyMemory<byte>?> ReadMessageAsync(
        CancellationToken cancellationToken = default)
    {
        int? contentLength = null;

        while (true)
        {
            var line = await ReadLineAsync(cancellationToken);

            if (line is null)
                return null;

            if (line.Length == 0)
                break;

            if (line.StartsWith("Content-Length:", StringComparison.OrdinalIgnoreCase))
            {
                contentLength = int.Parse(
                    line["Content-Length:".Length..].Trim());
            }
        }

        if (contentLength is null)
            throw new InvalidOperationException(
                "Missing Content-Length header.");

        var payload = new byte[contentLength.Value];

        var totalRead = 0;

        while (totalRead < payload.Length)
        {
            var read = await _input.ReadAsync(
                payload.AsMemory(totalRead),
                cancellationToken);

            if (read == 0)
                throw new EndOfStreamException();

            totalRead += read;
        }

        return payload;
    }

    private async Task<string?> ReadLineAsync(
    CancellationToken cancellationToken)
    {
        var bytes = new List<byte>();

        while (true)
        {
            var buffer = _singleByteBuffer;

            var read = await _input.ReadAsync(buffer, cancellationToken);

            if (read == 0)
            {
                if (bytes.Count == 0)
                    return null;

                throw new EndOfStreamException();
            }

            bytes.Add(buffer[0]);

            var count = bytes.Count;

            if (count >= 2 &&
                bytes[count - 2] == '\r' &&
                bytes[count - 1] == '\n')
            {
                bytes.RemoveRange(count - 2, 2);

                return Encoding.ASCII.GetString(bytes.ToArray());
            }
        }
    }
}