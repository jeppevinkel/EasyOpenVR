using System;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using Software.Boll.EasyUtils;

namespace EasyOpenVR.Data;

public class VrManifestBuilder
{
    private readonly VrManifest _vrManifest = new();
    private readonly JsonUtils _jsonUtils = new(new VrManifestJsonSerializerContext());

    public VrManifestBuilder(string source = "builtin")
    {
        _vrManifest.Source = source;
    }

    public VrManifestBuilder AddApplication(Application application)
    {
        _vrManifest.Applications.Add(application);
        return this;
    }

    public JsonResult<VrManifest> BuildAndSerialize()
    {
        var options = new JsonSerializerOptions
        {
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping, // To prevent ' to become \u0027
            IncludeFields = true, // Else the objects become empty due to the use of fields
            WriteIndented = true, // To make the manifest human-readable, and it matches what is common.
            PropertyNameCaseInsensitive = true, // Just convenient
            NumberHandling = JsonNumberHandling.AllowReadingFromString, // For safety
            PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower, // To ensure compatibility with SteamVR
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull, // To simplify the output
            Converters =
            {
                new LowercaseEnumConverter<LaunchTypeEnum>() // To ensure compatible values from enums
            },
        };
        var ctx = new VrManifestJsonSerializerContext(options);
        var json = new JsonUtils(ctx);
        return json.Serialize(_vrManifest);
    }
}

public class LowercaseEnumConverter<T>() : JsonStringEnumConverter<T>(JsonNamingPolicy.SnakeCaseLower) where T : struct, Enum;

public class ApplicationBuilder
{
    private readonly Application _application = new();

    public ApplicationBuilder(string appKey)
    {
        _application.AppKey = appKey;
    }

    public ApplicationBuilder SetBinaryPathWindows(string path)
    {
        _application.BinaryPathWindows = path;
        return this;
    }

    public ApplicationBuilder SetBinaryPathLinux(string path)
    {
        _application.BinaryPathLinux = path;
        return this;
    }

    public ApplicationBuilder SetBinaryPathOsx(string path)
    {
        _application.BinaryPathOsx = path;
        return this;
    }

    public ApplicationBuilder IsDashboardOverlay()
    {
        _application.IsDashboardOverlay = true;
        return this;
    }

    public ApplicationBuilder SetLaunchType(LaunchTypeEnum launchType)
    {
        _application.LaunchType = launchType;
        return this;
    }

    public ApplicationBuilder AddStrings(string posixLocale, Strings strings)
    {
        _application.Strings.Add(posixLocale, strings);
        return this;
    }

    public ApplicationBuilder SetImagePath(string imagePath)
    {
        _application.ImagePath = imagePath;
        return this;
    }

    public ApplicationBuilder SetActionManifestPath(string actionManifestPath)
    {
        _application.ActionManifestPath = actionManifestPath;
        return this;
    }

    public Application Build()
    {
        return _application;
    }
}