using System.IO;
using VRage.Utils;

namespace ClientPlugin.Assets;

/// <summary>
/// Locates the main menu background video, which ships as a Pulsar asset.
///
/// StcMenu.xml declares ClientPlugin/Resources as the reserved "AssetFolder" asset. Pulsar
/// copies that folder into its plugin cache (PluginHub) or resolves it in place (development
/// folder), then passes the folder to Plugin.LoadAssets before Plugin.Init. Pulsar compiles
/// plugins from source without embedded resources, so this is the only way to ship the video.
/// </summary>
public static class AssetLoader
{
    private const string VideoFileName = "star_trek_background.wmv";

    /// <summary>Full path of the background video, or null when Pulsar did not provide it.</summary>
    public static string VideoPath { get; private set; }

    public static void LoadAssets(string folder)
    {
        var path = Path.Combine(folder, VideoFileName);
        if (!File.Exists(path))
        {
            MyLog.Default.Warning($"STCLauncher: Background video not found in asset folder: {path}");
            return;
        }

        VideoPath = path;
        MyLog.Default.Info($"STCLauncher: Background video: {path}");
    }
}
