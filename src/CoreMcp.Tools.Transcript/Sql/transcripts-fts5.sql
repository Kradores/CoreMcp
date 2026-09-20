-- Embedded and executed by CoreMcp inside a repair transaction only when needed.
CREATE VIRTUAL TABLE IF NOT EXISTS transcripts_fts
USING fts5(text, content = 'transcripts', content_rowid = 'rowid');



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

INSERT INTO transcripts_fts(transcripts_fts) VALUES ('rebuild');
