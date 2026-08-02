namespace CoreMcp.Protocol.Transport.Framing;

public interface ITransportFramer
{
    Task<ReadOnlyMemory<byte>?> ReadMessageAsync(
        BufferedBinaryReader reader,
        CancellationToken cancellationToken);

    Task WriteMessageAsync(
        Stream output,
        ReadOnlyMemory<byte> payload,
        CancellationToken cancellationToken);
}
