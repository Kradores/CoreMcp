namespace CoreMcp.Protocol.Transport.Framing;

public sealed class TransportFramerFactory
{
    private readonly ContentLengthFramer _contentLength;
    private readonly NdjsonFramer _ndjson;

    public TransportFramerFactory(ContentLengthFramer contentLength, NdjsonFramer ndjson)
    {
        _contentLength = contentLength;
        _ndjson = ndjson;
    }

    public async Task<ITransportFramer?> DetectAsync(
        BufferedBinaryReader reader,
        CancellationToken cancellationToken)
    {
        var first = await reader.PeekByteAsync(cancellationToken);

        if (first is null)
            return null;

        if (first == (byte)'{')
            return _ndjson;

        return _contentLength;
    }
}
