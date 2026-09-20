"""Exercise only the published executable; all databases and logs are temporary."""
import json
from contextlib import closing
import os
from pathlib import Path
import queue
import shutil
import sqlite3
import subprocess
import sys
import tempfile
import threading


def main():
    executable = Path(sys.argv[1]).resolve(strict=True)
    assert [p.name for p in executable.parent.iterdir()] == [executable.name], "Publish directory must contain only the executable"
    with tempfile.TemporaryDirectory(prefix="CoreMcp.Portable.") as directory:
        root = Path(directory)
        binary = root / executable.name
        shutil.copy2(executable, binary)
        database = root / "transcripts.db"
        environment = os.environ.copy()
        environment.update({
            "COREMCP_TRANSCRIPTS_CONNECTION_STRING": f"Data Source={database};Default Timeout=1",
            "DOTNET_ROOT": str(root / "no-installed-runtime"),
            "DOTNET_ROOT_X64": str(root / "no-installed-runtime"),
            "DOTNET_MULTILEVEL_LOOKUP": "0",
            "DOTNET_BUNDLE_EXTRACT_BASE_DIR": str(root / "bundle"),
            "DOTNET_HOST_TRACE": "1",
            "DOTNET_HOST_TRACEFILE": str(root / "host.log"),
            "PATH": os.path.join(os.environ["SystemRoot"], "System32"),
        })
        with (root / "stderr.log").open("w") as log:
            process = subprocess.Popen([str(binary)], cwd=root, env=environment,
                                       stdin=subprocess.PIPE, stdout=subprocess.PIPE,
                                       stderr=log, text=True, encoding="utf-8")
            responses = queue.Queue()

            def read_output():
                for line in process.stdout:
                    responses.put(line)

            threading.Thread(target=read_output, daemon=True).start()
            request_id = 0

            def call(method, parameters):
                nonlocal request_id
                request_id += 1
                process.stdin.write(json.dumps({"jsonrpc": "2.0", "id": request_id,
                                               "method": method, "params": parameters}) + "\n")
                process.stdin.flush()
                response = json.loads(responses.get(timeout=30))
                assert response["id"] == request_id, response
                assert "error" not in response, response
                return response["result"]

            def tool(name, arguments):
                return call("tools/call", {"name": name, "arguments": arguments})

            def create_database():
                with closing(sqlite3.connect(database)) as connection:
                    connection.executescript("""
                        CREATE TABLE transcripts(created_at TEXT, source TEXT, start_time REAL,
                          end_time REAL, language TEXT, confidence REAL, text TEXT);
                        INSERT INTO transcripts VALUES('2026-08-21T21:30:00.000000+00:00',
                          'microphone',0,1,'en',0.9,'portable budget');
                    """)

            try:
                call("initialize", {"protocolVersion": "2024-11-05", "capabilities": {},
                                    "clientInfo": {"name": "portable-smoke", "version": "1"}})
                process.stdin.write(json.dumps({"jsonrpc": "2.0", "method": "notifications/initialized"}) + "\n")
                process.stdin.flush()
                assert any(t["name"] == "transcripts_search" for t in call("tools/list", {})["tools"])
                assert tool("transcripts_search", {})["isError"]
                assert not database.exists()
                assert not tool("echo", {"message": "still running"}).get("isError", False)
                create_database()
                for attempt in range(2):
                    result = tool("transcripts_search", {"query": "budget"})
                    assert not result.get("isError", False), result
                    found = json.loads(result["content"][0]["text"])
                    conversation = found["conversations"][0]["conversationRef"]
                    result = tool("transcripts_read_conversation", {"conversationRef": conversation})
                    assert not result.get("isError", False), result
                    assert "portable budget" in result["content"][0]["text"]
                    if attempt == 0:
                        database.unlink()
                        assert tool("transcripts_search", {})["isError"]
                        create_database()
                host_trace = (root / "host.log").read_text(encoding="utf-8")
                assert "self-contained" in host_trace.lower(), "Host did not identify a self-contained application"
                assert list((root / "bundle").rglob("e_sqlite3.dll")), "Bundled SQLite was not extracted"
                process.stdin.close()
                assert process.wait(timeout=10) == 0
            finally:
                if process.poll() is None:
                    process.kill()
                    process.wait(timeout=10)
                process.stdout.close()
        print("PASS: standalone bundle, MCP startup/tools, unavailable DB, provisioning, search/read, and live database recreation")


if __name__ == "__main__":
    main()
