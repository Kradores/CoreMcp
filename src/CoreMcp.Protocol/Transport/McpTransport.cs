using CoreMcp.Protocol.Transport.Framing;
using System.Text;

namespace CoreMcp.Protocol.Transport;

public sealed class McpTransport
{
    private readonly Stream _output;
    private readonly BufferedBinaryReader _reader;
    private readonly TransportFramerFactory _factory;

    private ITransportFramer? _framer;

    public McpTransport(
    Stream output,
    TransportFramerFactory factory,
    BufferedBinaryReader reader)
    {
        _output = output;
        _factory = factory;
        _reader = reader;
    }

    public async Task<ReadOnlyMemory<byte>?> ReadMessageAsync(
        CancellationToken cancellationToken = default)
    {
        _framer ??= await _factory.DetectAsync(_reader, cancellationToken);

        Console.Error.WriteLine($"[TRANSPORT] Using {_framer?.GetType().Name}");

        if (_framer is null)
            return null;

        Console.Error.WriteLine("[TRANSPORT] ReadMessageAsync");

        var payload = await _framer.ReadMessageAsync(_reader, cancellationToken);

        Console.Error.WriteLine(
            $"[TRANSPORT] Payload length = {payload?.Length ?? -1}");

        return payload;
    }

    public Task WriteMessageAsync(
        ReadOnlyMemory<byte> payload,
        CancellationToken cancellationToken = default)
    {
        if (_framer is null)
            throw new InvalidOperationException(
                "Cannot write before transport framing has been detected.");

        return _framer.WriteMessageAsync(
            _output,
            payload,
            cancellationToken);
    }
}