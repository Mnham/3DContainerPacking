using System.Text.Json.Serialization;

namespace CromulentBisgetti.DemoApp.Visualization;

internal sealed record PackingVisualizationContainer(
    [property: JsonPropertyName("length")] decimal Length,
    [property: JsonPropertyName("width")] decimal Width,
    [property: JsonPropertyName("height")] decimal Height,
    [property: JsonPropertyName("packingResult")] PackingVisualizationResult PackingResult);

internal sealed record PackingVisualizationResult(
    [property: JsonPropertyName("packedItems")] IReadOnlyList<PackingVisualizationItem> PackedItems);

internal sealed record PackingVisualizationItem(
    [property: JsonPropertyName("id")] int ID,
    [property: JsonPropertyName("coordX")] decimal CoordX,
    [property: JsonPropertyName("coordY")] decimal CoordY,
    [property: JsonPropertyName("coordZ")] decimal CoordZ,
    [property: JsonPropertyName("packDimX")] decimal PackDimX,
    [property: JsonPropertyName("packDimY")] decimal PackDimY,
    [property: JsonPropertyName("packDimZ")] decimal PackDimZ);
