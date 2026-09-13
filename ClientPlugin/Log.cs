using VRage.Utils;

namespace ClientPlugin;

/// <summary>Writes to the game log with the plugin name as prefix.</summary>
internal static class Log
{
    private const string Prefix = Plugin.Name + ": ";

    public static void Info(string message) => MyLog.Default.Info(Prefix + message);

    public static void Warning(string message) => MyLog.Default.Warning(Prefix + message);

    public static void Error(string message) => MyLog.Default.Error(Prefix + message);
}
