using System.Buffers;
using System.Text;

namespace CoreMcp.Protocol.Transport;

public sealed class BufferedBinaryReader
{
    private readonly Stream _stream;
    private readonly byte[] _buffer;

    private int _position;
    private int _length;

    public BufferedBinaryReader(
        Stream stream,
        int bufferSize = 4096)
    {
        _stream = stream;
        _buffer = new byte[bufferSize];
    }

    public async Task<byte?> PeekByteAsync(
        CancellationToken cancellationToken = default)
    {
        if (!await EnsureDataAsync(cancellationToken))
            return null;

        return _buffer[_position];
    }

    public async Task<byte?> ReadByteAsync(
        CancellationToken cancellationToken = default)
    {
        var value = await PeekByteAsync(cancellationToken);

        if (value is null)
            return null;

        _position++;

        return value;
    }

    public async Task<int> ReadAsync(
        Memory<byte> destination,
        CancellationToken cancellationToken = default)
    {
        if (destination.Length == 0)
            return 0;

        var totalRead = 0;

        while (destination.Length > 0)
        {
            if (!await EnsureDataAsync(cancellationToken))
                break;

            var available = _length - _position;
            var toCopy = Math.Min(available, destination.Length);

            _buffer.AsMemory(_position, toCopy)
                .CopyTo(destination);

            _position += toCopy;
            totalRead += toCopy;
            destination = destination[toCopy..];

            // Match Stream.ReadAsync behavior:
            // return as soon as we've read something.
            if (totalRead > 0)
                break;
        }

        return totalRead;
    }

    public async Task<ReadOnlyMemory<byte>> ReadBytesAsync(
        int count,
        CancellationToken cancellationToken = default)
    {
        var buffer = new byte[count];

        var offset = 0;

        while (offset < count)
        {
            var read = await ReadAsync(
                buffer.AsMemory(offset),
                cancellationToken);

            if (read == 0)
                throw new EndOfStreamException(
                    "Unexpected end of stream while reading payload.");

            offset += read;
        }

        return buffer;
    }

    public async Task<string?> ReadLineAsync(
    CancellationToken cancellationToken = default)
    {
        var buffer = new ArrayBufferWriter<byte>();

        while (true)
        {
            var value = await ReadByteAsync(cancellationToken);

            if (value is null)
            {
                if (buffer.WrittenCount == 0)
                    return null;

                throw new EndOfStreamException(
                    "Unexpected end of stream while reading line.");
            }

            if (value == (byte)'\n')
            {
                var span = buffer.WrittenSpan;

                if (span.Length > 0 && span[^1] == (byte)'\r')
                    span = span[..^1];

                return Encoding.UTF8.GetString(span);
            }

            buffer.Write(new[] { value.Value });
        }
    }

    private async Task<bool> EnsureDataAsync(
        CancellationToken cancellationToken)
    {
        if (_position < _length)
            return true;

        _length = await _stream.ReadAsync(
            _buffer,
            cancellationToken);

        _position = 0;

        return _length > 0;
    }
}