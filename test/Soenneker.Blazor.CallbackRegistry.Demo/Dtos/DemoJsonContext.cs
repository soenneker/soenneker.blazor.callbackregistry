using System.Text.Json.Serialization;

namespace Soenneker.Blazor.CallbackRegistry.Demo.Dtos;

[JsonSerializable(typeof(TestDto))]
internal partial class DemoJsonContext : JsonSerializerContext
{
}
