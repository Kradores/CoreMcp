using System.Text;

namespace CoreMcp.Protocol.Transport.Framing;

public sealed class NdjsonFramer : ITransportFramer
{
    public async Task<ReadOnlyMemory<byte>?> ReadMessageAsync(
    BufferedBinaryReader reader,
    CancellationToken cancellationToken)
    {
        while (true)
        {
            Console.Error.WriteLine("[NDJSON] Waiting for line...");

            var line = await reader.ReadLineAsync(cancellationToken);

            if (line is null)
                return null;

            // Ignore blank lines
            if (string.IsNullOrWhiteSpace(line))
                continue;

            Console.Error.WriteLine($"[NDJSON] Read line: {line}");

            return Encoding.UTF8.GetBytes(line);
        }
    }

    public async Task WriteMessageAsync(
        Stream output,
        ReadOnlyMemory<byte> payload,
        CancellationToken cancellationToken)
    {
        Console.Error.WriteLine("Sending:");
        Console.Error.WriteLine(Encoding.UTF8.GetString(payload.Span));
        Console.Error.WriteLine(BitConverter.ToString(payload.ToArray()));

        await output.WriteAsync(payload, cancellationToken);
        await output.WriteAsync("\n"u8.ToArray(), cancellationToken);
        await output.FlushAsync(cancellationToken);
    }
}