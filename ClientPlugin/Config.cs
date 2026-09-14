using ClientPlugin.Settings;
using ClientPlugin.Settings.Elements;
using Sandbox.Graphics.GUI;

namespace ClientPlugin;

public class Config
{
    public static readonly Config Current = ConfigStorage.Load();

    public readonly string Title = "STC Menu Settings";

    [Checkbox(description: "Enable custom Star Trek background video")]
    public bool EnableCustomVideo { get; set; } = true;

    [Checkbox(description: "Hide the news panel, DLC banners and Newsletter button on the main menu. Turn off to subscribe to the newsletter for its reward blocks.")]
    public bool HideMenuNews { get; set; } = true;

    [Checkbox(description: "Enable development mode to show dev server options")]
    public bool DevMode { get; set; }

    [Button(description: "Open the Star Trek Continuum server selection dialog")]
    public void OpenServerSelection()
    {
        MyGuiSandbox.AddScreen(new GUI.ServerSelectionDialog());
    }
}
