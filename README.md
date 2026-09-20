# CoreMcp

## Portable Windows executable

Build on a machine with the .NET 10 SDK:

```powershell
dotnet publish src/CoreMcp.Server/CoreMcp.Server.csproj -c Release -p:PublishProfile=Portable
```

Distribute `src/CoreMcp.Server/bin/Release/net10.0/win-x64/publish/CoreMcp.Server.exe`.
The publish directory contains one executable, including .NET and SQLite; users
do not need to install .NET or copy the SQL script. Native libraries extract to
the user's temporary directory at runtime, which must be writable. This build
targets Windows x64 and is not an installer or a Windows service.

Place the executable in a stable location, for example
`C:\Users\YourName\Apps\CoreMcp\CoreMcp.Server.exe`. Configure your MCP client to
launch it using standard input/output. For clients using `mcpServers`, the entry
looks like this (replace both paths with your actual locations):

```json
{
  "mcpServers": {
    "coremcp": {
      "command": "C:\\Users\\YourName\\Apps\\CoreMcp\\CoreMcp.Server.exe",
      "args": [],
      "env": {
        "COREMCP_TRANSCRIPTS_CONNECTION_STRING": "Data Source=C:\\Users\\YourName\\Transcripts\\transcripts.db"
      }
    }
  }
}
```

Restart the MCP client after changing its configuration. Launching the executable
by double-clicking does not open a setup window: the MCP client launches it.
To update, stop the client, replace the executable in the same location, and
restart the client. Republish when updating the bundled runtime or dependencies.

## Automatic transcript search setup

The audio transcription service creates `transcripts.db` and its `transcripts`
table. On each transcript request, CoreMcp checks the search index and its three
synchronization triggers. Missing objects are created transactionally from an
embedded SQL script, and existing transcripts are indexed. Healthy indexes are
not rebuilt. Normal queries use read-only connections; repair requires write
access to the database and its containing directory.

If the database is deleted, restart the audio transcription service to recreate
it. The next transcript request rebuilds its search index without restarting
CoreMcp. A missing database or a failed repair returns a transcript-tool error;
other tools remain available. CoreMcp never creates an empty database itself.
There is no background polling: initialization and retry happen on requests.

If the database is busy, retry the request. SQLite waits up to 30 seconds by
default; a positive `Default Timeout` in the connection string overrides this.
If an existing search object has an incompatible definition, CoreMcp reports it
without deleting it. Check the database configuration and resolve the schema
conflict before retrying. Do not run the SQL script on every startup: its rebuild
operation is intended only for repair.

Developers can smoke-test the published executable using Python 3:

```powershell
python tests/portable_smoke.py src/CoreMcp.Server/bin/Release/net10.0/win-x64/publish/CoreMcp.Server.exe
```

This test copies only the executable to an unrelated temporary directory and uses
temporary databases. A clean Windows machine without .NET remains the release
acceptance environment for confirming the complete end-user experience.

## Filesystem safeguards

Mutating filesystem tools reject paths that overlap protected system directories,
including Windows, Program Files, Program Files (x86), and ProgramData. This
also blocks a parent target such as a drive root when it contains a protected
directory.

To protect additional locations, set `CORE_MCP_RESTRICTED_DIRECTORIES` before
starting the server. Supply absolute paths separated by `;`, for example:

```text
CORE_MCP_RESTRICTED_DIRECTORIES=D:\ProductionData;D:\Backups
```

This is an accidental-destruction safeguard, not a sandbox: normal filesystem
access outside the restricted list remains available to the server process.
