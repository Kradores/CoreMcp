namespace CoreMcp.Tools.Transcript;

public sealed record TranscriptDatabaseOptions(string? ConnectionString)
{
    public const string ConnectionStringEnvironmentVariable =
        "COREMCP_TRANSCRIPTS_CONNECTION_STRING";

    public static TranscriptDatabaseOptions FromEnvironment() =>
        new(Environment.GetEnvironmentVariable(
            ConnectionStringEnvironmentVariable));
}
