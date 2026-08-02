using CoreMcp.Protocol.Serializer;
using System.Reflection;
using System.Text.Json;

namespace CoreMcp.Framework.Reflection;

internal static class JsonSchemaGenerator
{
    private static readonly NullabilityInfoContext NullabilityContext = new();

    public static JsonElement Generate(Type type)
    {
        var schema = GenerateSchema(type);

        return JsonSerializer.SerializeToElement(
            schema,
            JsonRpcSerializer.Options);
    }

    private static object GenerateSchema(Type type)
    {
        type = Nullable.GetUnderlyingType(type) ?? type;

        if (type.IsEnum)
        {
            return GenerateEnumSchema(type);
        }

        if (TryGetPrimitiveType(type, out var jsonType))
        {
            return new Dictionary<string, object?>
            {
                ["type"] = jsonType
            };
        }

        if (TryGetEnumerableType(type, out var elementType))
        {
            return new Dictionary<string, object?>
            {
                ["type"] = "array",
                ["items"] = GenerateSchema(elementType)
            };
        }

        return GenerateObjectSchema(type);
    }

    private static object GenerateObjectSchema(Type type)
    {
        var properties = new Dictionary<string, object?>();
        var required = new List<string>();

        foreach (var property in type.GetProperties(BindingFlags.Public | BindingFlags.Instance))
        {
            var propertyName =
                JsonRpcSerializer.Options.PropertyNamingPolicy?.ConvertName(property.Name)
                ?? property.Name;

            properties[propertyName] =
                GenerateSchema(property.PropertyType);

            var nullability =
                NullabilityContext.Create(property);

            if (nullability.WriteState == NullabilityState.NotNull)
            {
                required.Add(propertyName);
            }
        }

        return new Dictionary<string, object?>
        {
            ["type"] = "object",
            ["properties"] = properties,
            ["required"] = required
        };
    }

    private static object GenerateEnumSchema(Type type)
    {
        return new Dictionary<string, object?>
        {
            ["type"] = "string",
            ["enum"] = Enum.GetNames(type)
        };
    }

    private static bool TryGetPrimitiveType(
        Type type,
        out string jsonType)
    {
        jsonType = type switch
        {
            _ when type == typeof(string) => "string",
            _ when type == typeof(bool) => "boolean",

            _ when type == typeof(byte) => "integer",
            _ when type == typeof(short) => "integer",
            _ when type == typeof(int) => "integer",
            _ when type == typeof(long) => "integer",

            _ when type == typeof(float) => "number",
            _ when type == typeof(double) => "number",
            _ when type == typeof(decimal) => "number",

            _ => string.Empty
        };

        return jsonType.Length > 0;
    }

    private static bool TryGetEnumerableType(
        Type type,
        out Type elementType)
    {
        if (type.IsArray)
        {
            elementType = type.GetElementType()!;
            return true;
        }

        if (type.IsGenericType &&
            type.GetGenericTypeDefinition() == typeof(List<>))
        {
            elementType = type.GetGenericArguments()[0];
            return true;
        }

        elementType = null!;
        return false;
    }
}