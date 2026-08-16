using System.Text.Json;
using System.Text.Json.Serialization;
using ModelaFlow.PatternCore.Geometry;
using ModelaFlow.PatternCore.Pattern;

namespace ModelaFlow.PatternCore.Serialization;

/// <summary>
/// Round-trip JSON for PatternDocument (storage / export input). Does not recalculate geometry.
/// </summary>
public static class PatternDocumentJson
{
    public static JsonSerializerOptions Options { get; } = CreateOptions();

    public static string Serialize(PatternDocument document) =>
        JsonSerializer.Serialize(document, Options);

    public static PatternDocument Deserialize(string json) =>
        JsonSerializer.Deserialize<PatternDocument>(json, Options)
        ?? throw new InvalidOperationException("Geometry JSON deserialized to null.");

    private static JsonSerializerOptions CreateOptions()
    {
        var options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = false,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };
        options.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.CamelCase));
        options.Converters.Add(new Contour2DJsonConverter());
        return options;
    }
}

internal sealed class Contour2DJsonConverter : JsonConverter<Contour2D>
{
    public override Contour2D Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        using var doc = JsonDocument.ParseValue(ref reader);
        var root = doc.RootElement;
        var isClosed = root.TryGetProperty("isClosed", out var closedProp) && closedProp.GetBoolean();
        if (!root.TryGetProperty("edges", out var edgesProp))
            throw new JsonException("Contour2D requires 'edges'.");

        var edges = edgesProp.Deserialize<List<PathEdge>>(options)
                    ?? throw new JsonException("Contour2D edges deserialize failed.");
        return new Contour2D(edges, isClosed);
    }

    public override void Write(Utf8JsonWriter writer, Contour2D value, JsonSerializerOptions options)
    {
        writer.WriteStartObject();
        writer.WriteBoolean("isClosed", value.IsClosed);
        writer.WritePropertyName("edges");
        JsonSerializer.Serialize(writer, value.Edges, options);
        writer.WriteEndObject();
    }
}
