using System;
using System.Data;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;
using EasyOpenVR.Data;
using static System.Text.Json.JsonSerializer;

namespace EasyOpenVR.Utils;

public class JsonUtils
{
    public record JsonDataParseResult<T>(T? Data, T Empty, string Message);

    public static JsonDataParseResult<T> ParseData<T>(string? dataStr) where T : class, new()
    {
        dataStr ??= "";
        T? data = null;
        var errorMessage = "";
        try
        {
            var typeInfo = (JsonTypeInfo<T>?)AppJsonSerializerContext.Default.GetTypeInfo(typeof(T));
            if (typeInfo != null)
            {
                data = Deserialize(dataStr, typeInfo);
            }
        }
        catch (Exception e)
        {
            errorMessage = e.Message;
        }

        return new JsonDataParseResult<T>(data, new T(), errorMessage);
    }

    public static string SerializeData<T>(T? data) where T : class
    {
        // We use relaxed encoding to avoid converting things like apostrophe to \u0027
        var options = new JsonSerializerOptions(AppJsonSerializerContext.Default.Options)
        {
            Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
            NewLine = "\n" // Force Unix linebreaks
        };
        var context = new AppJsonSerializerContext(options);
        // We use GetTypeInfo to embed the type class information at compile time, for performance, instead of reflection at runtime.
        var typeInfo = (JsonTypeInfo<T?>?)context.GetTypeInfo(typeof(T));
        return typeInfo != null ? Serialize(data, options) : "";
    }
}

public class LowercaseEnumConverter<T>() : JsonStringEnumConverter<T>(JsonNamingPolicy.SnakeCaseLower) where T : struct, Enum;

[JsonSerializable(typeof(VrManifest))]
[JsonSourceGenerationOptions(
    IncludeFields = true,
    GenerationMode = JsonSourceGenerationMode.Default,
    WriteIndented = true,
    PropertyNameCaseInsensitive = true,
    NumberHandling = JsonNumberHandling.AllowReadingFromString,
    PropertyNamingPolicy = JsonKnownNamingPolicy.SnakeCaseLower,
    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
    Converters =
    [
        typeof(LowercaseEnumConverter<LaunchTypeEnum>)
    ]
)]
public partial class AppJsonSerializerContext : JsonSerializerContext
{
}