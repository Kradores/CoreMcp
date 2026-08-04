# CoreMcp

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
