using Microsoft.Extensions.Logging;

namespace UMP.DlyStc.Plugin.Cld.Shared;

public static class GlobalConstants
{
    public static string? PluginConfigFolder { get; set; }

    public static StcPluginConfig? PluginConfig { get; set; }

    public static class HostInterfaces
    {
        public static ILogger? PluginLogger;
    }

    public static class Assets
    {
        public static readonly string AsciiLogo = "";
    }
}