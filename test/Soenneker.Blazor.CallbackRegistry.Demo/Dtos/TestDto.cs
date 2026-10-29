using System.Text.Json.Serialization;

namespace Soenneker.Blazor.CallbackRegistry.Demo.Dtos;

public class TestDto
{
    [JsonPropertyName("id")]
    public required string Id { get; set; }

    [JsonPropertyName("int")]
    public int Int { get; set; }
}
