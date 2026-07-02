using System.Text.Json;
using System.Text.Json.Serialization;
namespace TripleTriad.Util;

public sealed record class JsonContentLoaderEx : IContentLoader
{
    private static readonly JsonSerializerOptions s_jsonSerializerOptions = new()
    {
        WriteIndented = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        Converters =
        {
            new JsonStringEnumConverter(),
            new RangeJsonConverter<int>(),
            new RangeJsonConverter<float>(),
            new RangeJsonConverter<HslColor>(),
            new ThicknessJsonConverter(),
            new RectangleFJsonConverter(),
            new Size2JsonConverter(),
            new SizeJsonConverter(),
        }
    };

    public T Load<T>(ContentManager contentManager, string path)
    {
        using var utf8Json = contentManager.OpenStream(path);
        return JsonSerializer.Deserialize<T>(utf8Json, s_jsonSerializerOptions)
            ?? throw new ArgumentException(null, nameof(path));
    }
}
