# Transcript retrieval policy

Always call `transcripts_search` before making a claim about the user's
recorded conversation history. For a selected result, call
`transcripts_read_conversation` before quoting or summarizing it. If no result
matches, say that no matching recording was found rather than relying on model
memory.

Interpret a date without a time in `Europe/Madrid`; send explicit time ranges
to the tool in UTC. `mocrophone` is the local audio channel. `system_audio` is
mixed external audio and is not a speaker identity. Attribute speech to a
person only when the transcript explicitly supports it; otherwise describe the
attribution as an uncertain inference.
