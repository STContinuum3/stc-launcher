using System.IO;

namespace ClientPlugin.Assets;

/// <summary>
/// Locates the main menu background video, which ships as a Pulsar asset.
///
/// StcMenu.xml declares the reserved "AssetFolder" asset as a zip on the repository's GitHub
/// releases. Pulsar downloads it into its plugin cache, verifies its SHA-256, extracts it and
/// passes the extracted folder to Plugin.LoadAssets before Plugin.Init. Pulsar compiles plugins
/// from source without embedded resources, and the video is kept out of the repository so the
/// source download stays small.
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
            Log.Warning($"Background video not found in asset folder: {path}");
            return;
        }

        VideoPath = path;
        Log.Info($"Background video: {path}");
    }
}
