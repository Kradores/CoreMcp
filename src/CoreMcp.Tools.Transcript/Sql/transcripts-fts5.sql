-- Run this script once against the transcript SQLite database before enabling
-- the MCP transcript tools. The server connects read-only and only validates
-- that these objects exist.
CREATE VIRTUAL TABLE IF NOT EXISTS transcripts_fts
USING fts5(text, content = 'transcripts', content_rowid = 'rowid');

INSERT INTO transcripts_fts(transcripts_fts) VALUES ('rebuild');

CREATE TRIGGER IF NOT EXISTS transcripts_fts_after_insert
AFTER INSERT ON transcripts BEGIN
    INSERT INTO transcripts_fts(rowid, text) VALUES (new.rowid, new.text);
END;

CREATE TRIGGER IF NOT EXISTS transcripts_fts_after_delete
AFTER DELETE ON transcripts BEGIN
    INSERT INTO transcripts_fts(transcripts_fts, rowid, text)
    VALUES ('delete', old.rowid, old.text);
END;

CREATE TRIGGER IF NOT EXISTS transcripts_fts_after_update_text
AFTER UPDATE OF text ON transcripts BEGIN
    INSERT INTO transcripts_fts(transcripts_fts, rowid, text)
    VALUES ('delete', old.rowid, old.text);
    INSERT INTO transcripts_fts(rowid, text) VALUES (new.rowid, new.text);
END;
