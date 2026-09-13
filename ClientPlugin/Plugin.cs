using ClientPlugin.Settings;
using HarmonyLib;
using Sandbox.Graphics.GUI;
using VRage.Plugins;

// Pulsar compiles the plugin from source without MSBuild, so the version is defined here for it.
// MSBuild builds define LOCAL_BUILD and take the version from Version.Build.props instead.
#if !LOCAL_BUILD
[assembly: System.Reflection.AssemblyVersion("1.0.0.0")]
[assembly: System.Reflection.AssemblyFileVersion("1.0.0.0")]
#endif

namespace ClientPlugin;

// ReSharper disable once UnusedType.Global
public class Plugin : IPlugin
{
    public const string Name = "StcMenu";
    private SettingsGenerator settingsGenerator;

    public void Init(object gameInstance)
    {
        try
        {
            settingsGenerator = new SettingsGenerator();
            new Harmony(Name).PatchAll(typeof(Plugin).Assembly);
            Log.Info("Initialized");
        }
        catch (System.Exception ex)
        {
            Log.Error($"Error during initialization: {ex}");
        }
    }

    public void Dispose()
    {
        // Do NOT call harmony.UnpatchAll() here! It may break other plugins.
    }

    public void Update()
    {
    }

    // ReSharper disable once UnusedMember.Global
    public void OpenConfigDialog()
    {
        MyGuiSandbox.AddScreen(settingsGenerator.Dialog);
    }

    // ReSharper disable once UnusedMember.Global
    // Called by Pulsar before Init with the folder of the "AssetFolder" asset from StcMenu.xml
    public void LoadAssets(string folder)
    {
        Assets.AssetLoader.LoadAssets(folder);
    }
}
