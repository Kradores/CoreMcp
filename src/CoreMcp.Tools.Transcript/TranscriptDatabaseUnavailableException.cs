using Microsoft.Data.Sqlite;

namespace CoreMcp.Tools.Transcript;

public sealed class TranscriptDatabaseUnavailableException(string message) : Exception(message)
{
    internal static TranscriptDatabaseUnavailableException FromSqlite(SqliteException error) =>
        new(error.SqliteErrorCode switch
        {
            5 or 6 => "The transcript database is busy. Retry the request after the audio service finishes writing.",
            3 or 8 => "CoreMcp needs write access to the transcript database and its directory to repair the search index.",
            14 => "The transcript database cannot be opened. Check its configured location and access permissions, and start the audio transcription service to recreate it if missing.",
            _ => "The transcript database could not be read or its search index repaired. Check that the audio service has created a valid database and retry."
        });
}
