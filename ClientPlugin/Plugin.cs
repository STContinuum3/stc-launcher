using System.Reflection;
using ClientPlugin.Settings;
using HarmonyLib;
using Sandbox.Graphics.GUI;
using VRage.Plugins;

// Define assembly version when compiled by Pulsar
#if !LOCAL_BUILD
[assembly: AssemblyVersion("1.0.0.0")]
[assembly: AssemblyFileVersion("1.0.0.0")]
#endif

namespace ClientPlugin;

// ReSharper disable once UnusedType.Global
public class Plugin : IPlugin
{
    public const string Name = "StcMenu";
    private SettingsGenerator settingsGenerator;

    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public void Init(object gameInstance)
    {
        VRage.Utils.MyLog.Default.WriteLine($"STCLauncher: Initializing plugin v1.0");

        try
        {
            // Initialize settings
            settingsGenerator = new SettingsGenerator();
            VRage.Utils.MyLog.Default.WriteLine($"STCLauncher: Settings initialized");

            // Apply Harmony patches
            var harmony = new Harmony(Name);
            harmony.PatchAll(Assembly.GetExecutingAssembly());
            VRage.Utils.MyLog.Default.WriteLine($"STCLauncher: Harmony patches applied successfully");

            // Initialize assets early
            Assets.AssetLoader.LoadAssets();
            VRage.Utils.MyLog.Default.WriteLine($"STCLauncher: Assets loaded");

            // The menu background video is installed by Patches.MainMenuBackgroundPatch.
            // It cannot be done here: SpaceEngineersGame.SetupPerGameSettings() runs after
            // Pulsar initialises plugins and would overwrite anything set at this point.
        }
        catch (System.Exception ex)
        {
            VRage.Utils.MyLog.Default.WriteLine($"STCLauncher: Error during initialization: {ex}");
        }
    }

    public void Dispose()
    {
        // TODO: Save state and close resources here, called when the game exits (not guaranteed!)
        // IMPORTANT: Do NOT call harmony.UnpatchAll() here! It may break other plugins.
    }

    public void Update()
    {
        // The game owns the background video screen now, including its volume and cleanup.
    }

    // ReSharper disable once UnusedMember.Global
    public void OpenConfigDialog()
    {
        MyGuiSandbox.AddScreen(settingsGenerator.Dialog);
    }

    public void LoadAssets(string folder)
    {
        Assets.AssetLoader.LoadAssets(folder);
    }
}
