using System.Collections.Generic;

namespace EasyOpenVR.Data;

public class VrManifest
{
    public string Source = "";
    public List<Application> Applications = [];
}

public class Application
{
    public string AppKey = "";
    public LaunchTypeEnum LaunchType = LaunchTypeEnum.Binary;
    public string? Url = null;
    public string? BinaryPathWindows = null;
    public string? BinaryPathLinux = null;
    public string? BinaryPathOsx = null;
    public string? ActionManifestPath = null;
    public string? ImagePath = null;
    public bool? IsDashboardOverlay = false;
    public Dictionary<string, Strings> Strings = new();
}

public record struct Strings(
    string? Name,
    string? Description
);

public enum LaunchTypeEnum
{
    Binary,
    Url
}