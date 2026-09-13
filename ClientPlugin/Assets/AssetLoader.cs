using System;
using System.IO;
using System.Linq;
using System.Reflection;
using VRage.FileSystem;
using VRage.Utils;

namespace ClientPlugin.Assets;

public static class AssetLoader
{
    private static string _assetsFolderPath;
    private static string _videosFolderPath;

    public static string AssetsFolderPath
    {
        get
        {
            if (_assetsFolderPath == null)
            {
                InitializePaths();
            }
            return _assetsFolderPath;
        }
    }

    public static string VideosFolderPath
    {
        get
        {
            if (_videosFolderPath == null)
            {
                InitializePaths();
            }
            return _videosFolderPath;
        }
    }

    private static void InitializePaths()
    {
        try
        {
            // Get the plugin's data directory
            string pluginDataPath = Path.Combine(MyFileSystem.UserDataPath, "Storage", Plugin.Name);
            _assetsFolderPath = Path.Combine(pluginDataPath, "Assets");
            _videosFolderPath = Path.Combine(_assetsFolderPath, "Videos");

            // Create directories if they don't exist
            Directory.CreateDirectory(_assetsFolderPath);
            Directory.CreateDirectory(_videosFolderPath);

            MyLog.Default.Info($"STCLauncher: Assets folder: {_assetsFolderPath}");
            MyLog.Default.Info($"STCLauncher: Videos folder: {_videosFolderPath}");
        }
        catch (Exception ex)
        {
            MyLog.Default.Error($"STCLauncher: Failed to initialize asset paths: {ex}");

            // Fallback paths
            _assetsFolderPath = Path.Combine(MyFileSystem.UserDataPath, "STCLauncher_Assets");
            _videosFolderPath = Path.Combine(_assetsFolderPath, "Videos");
        }
    }

    public static string[] GetCustomVideoFiles()
    {
        try
        {
            // Initialize paths if not already done
            if (_assetsFolderPath == null || _videosFolderPath == null)
            {
                InitializePaths();
            }

            // First try to get embedded video
            var embeddedVideo = ExtractEmbeddedVideo();
            if (embeddedVideo != null && File.Exists(embeddedVideo))
            {
                // Validate video file
                var fileInfo = new FileInfo(embeddedVideo);
                MyLog.Default.Info($"STCLauncher: Using embedded video: {embeddedVideo}");
                MyLog.Default.Info($"  File size: {fileInfo.Length / (1024 * 1024)}MB");
                MyLog.Default.Info($"  Extension: {fileInfo.Extension}");
                MyLog.Default.Info($"  Full path: {fileInfo.FullName}");

                // Return path directly without conversion
                return new string[] { embeddedVideo };
            }

            // Fallback to external video files
            if (!Directory.Exists(VideosFolderPath))
            {
                MyLog.Default.Warning($"STCLauncher: Videos directory not found: {VideosFolderPath}");
                CreateReadmeFile();
                return new string[0];
            }

            // Only WMV format is supported by Space Engineers (proven by CustomLoadingBackgrounds)
            string[] videoExtensions = { "*.wmv" };

            var videoFiles = videoExtensions
                .SelectMany(ext => Directory.GetFiles(VideosFolderPath, ext, SearchOption.TopDirectoryOnly))
                .OrderBy(f => f)
                .ToArray();

            if (videoFiles.Length > 0)
            {
                MyLog.Default.Info($"STCLauncher: Found {videoFiles.Length} external video file(s):");
                foreach (var file in videoFiles)
                {
                    MyLog.Default.Info($"  - {Path.GetFileName(file)}");
                }
            }
            else
            {
                MyLog.Default.Info($"STCLauncher: No video files found in {VideosFolderPath}");
                CreateReadmeFile();
            }

            return videoFiles;
        }
        catch (Exception ex)
        {
            MyLog.Default.Error($"STCLauncher: Error getting custom video files: {ex}");
            return new string[0];
        }
    }

    private static void CreateReadmeFile()
    {
        try
        {
            string readmePath = Path.Combine(VideosFolderPath, "README.txt");

            if (!File.Exists(readmePath))
            {
                string readmeContent = @"Star Trek Continuum Launcher - Custom Video Background

EMBEDDED VIDEO:
This plugin comes with a built-in Star Trek background video (WMV format) that will be used automatically.
No additional video files are required!

CUSTOM VIDEO (OPTIONAL):
To override the embedded video with your own custom background:

1. Place your video file in this folder
2. Supported format: .wmv (Windows Media Video) ONLY
3. Recommended resolution: 1920x1080 or higher
4. File name suggestion: star_trek_background.wmv

PRIORITY ORDER:
1. Embedded video (included with plugin) - DEFAULT
2. External video files (placed in this folder) - OVERRIDE

The plugin will automatically use the embedded video unless you place
a custom video file in this folder, which will take priority.

For best results with custom videos:
- Use Windows Media Video 9 codec for WMV files
- Keep file size reasonable (under 100MB)
- Ensure the video loops seamlessly
- Test the video plays correctly in Windows Media Player

The video will replace the default Space Engineers main menu background.

Note: This folder is located at:
" + VideosFolderPath;

                File.WriteAllText(readmePath, readmeContent);
                MyLog.Default.Info($"STCLauncher: Created README file at {readmePath}");
            }
        }
        catch (Exception ex)
        {
            MyLog.Default.Error($"STCLauncher: Failed to create README file: {ex}");
        }
    }

    private static string ExtractEmbeddedVideo()
    {
        try
        {
            var assembly = Assembly.GetExecutingAssembly();
            var resourceNames = assembly.GetManifestResourceNames();

            MyLog.Default.Info($"STCLauncher: Looking for embedded video in {resourceNames.Length} resources");

            // Look for embedded WMV video resource (only format supported by SE)
            var videoResource = resourceNames.FirstOrDefault(name =>
                name.EndsWith(".wmv", StringComparison.OrdinalIgnoreCase));

            if (videoResource == null)
            {
                MyLog.Default.Warning("STCLauncher: No embedded video resource found in assembly");
                foreach (var name in resourceNames)
                {
                    MyLog.Default.Info($"  Resource: {name}");
                }
                return null;
            }

            // Extract the embedded video to temp location with correct extension
            var videoExtension = Path.GetExtension(videoResource);
            var tempVideoPath = Path.Combine(AssetsFolderPath, "embedded_background" + videoExtension);

            MyLog.Default.Info($"STCLauncher: Embedded video will be extracted to: {tempVideoPath}");

            // Only extract if file doesn't exist or is older than the assembly
            var assemblyPath = assembly.Location;
            var shouldExtract = !File.Exists(tempVideoPath) ||
                              File.GetLastWriteTime(tempVideoPath) < File.GetLastWriteTime(assemblyPath);

            if (shouldExtract)
            {
                MyLog.Default.Info($"STCLauncher: Extracting embedded video resource: {videoResource}");

                using (var stream = assembly.GetManifestResourceStream(videoResource))
                {
                    if (stream != null)
                    {
                        MyLog.Default.Info($"STCLauncher: Resource stream size: {stream.Length} bytes");
                        using (var fileStream = File.Create(tempVideoPath))
                        {
                            stream.CopyTo(fileStream);
                        }
                        MyLog.Default.Info($"STCLauncher: Successfully extracted video to: {tempVideoPath}");
                    }
                    else
                    {
                        MyLog.Default.Error($"STCLauncher: Failed to get resource stream for: {videoResource}");
                    }
                }
            }
            else
            {
                MyLog.Default.Info($"STCLauncher: Using existing extracted video: {tempVideoPath}");
            }

            return File.Exists(tempVideoPath) ? tempVideoPath : null;
        }
        catch (Exception ex)
        {
            MyLog.Default.Error($"STCLauncher: Error extracting embedded video: {ex}");
            return null;
        }
    }

    public static void LoadAssets(string folder = null)
    {
        try
        {
            MyLog.Default.Info("STCLauncher: Loading assets...");

            // Initialize paths
            InitializePaths();

            // Check for custom videos
            var videoFiles = GetCustomVideoFiles();

            if (videoFiles.Length > 0)
            {
                MyLog.Default.Info($"STCLauncher: Successfully loaded {videoFiles.Length} video asset(s)");
            }
            else
            {
                MyLog.Default.Info("STCLauncher: No custom video assets found, using defaults");
            }
        }
        catch (Exception ex)
        {
            MyLog.Default.Error($"STCLauncher: Failed to load assets: {ex}");
        }
    }
}
