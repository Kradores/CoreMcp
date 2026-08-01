using System.Text.Json.Serialization;

namespace CoreMcp.Protocol.Tools;

[JsonPolymorphic(TypeDiscriminatorPropertyName = "type")]
[JsonDerivedType(typeof(TextContent), "text")]
public abstract record ToolContent;