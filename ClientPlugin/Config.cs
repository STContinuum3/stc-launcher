using ClientPlugin.Settings;
using ClientPlugin.Settings.Elements;
using Sandbox.Graphics.GUI;
using System;
using System.Text;
using VRageMath;

namespace ClientPlugin;

public class Config
{
    public static readonly Config Current = ConfigStorage.Load();

    public readonly string Title = "STC Launcher Settings";

    [Checkbox(description: "Enable custom Star Trek background video")]
    public bool EnableCustomVideo { get; set; } = true;

    [Checkbox(description: "Enable development mode to show dev server options")]
    public bool DevMode { get; set; }

    [Button(description: "Open the Star Trek Continuum server selection dialog")]
    public void OpenServerSelection()
    {
        MyGuiSandbox.AddScreen(new GUI.ServerSelectionDialog());
    }

    [Button(description: "Open the custom video assets folder")]
    public void OpenAssetsFolder()
    {
        try
        {
            var assetsPath = Assets.AssetLoader.VideosFolderPath;
            System.Diagnostics.Process.Start("explorer.exe", assetsPath);
        }
        catch (Exception ex)
        {
            MyGuiSandbox.AddScreen(MyGuiSandbox.CreateMessageBox(
                MyMessageBoxStyleEnum.Error,
                buttonType: MyMessageBoxButtonsType.OK,
                messageText: new StringBuilder($"Failed to open assets folder:\n{ex.Message}"),
                messageCaption: new StringBuilder("Error"),
                size: new Vector2(0.6f, 0.4f)
            ));
        }
    }
}
