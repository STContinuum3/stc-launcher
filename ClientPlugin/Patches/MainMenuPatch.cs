using HarmonyLib;
using Sandbox.Graphics.GUI;
using SpaceEngineers.Game.GUI;
using ClientPlugin.GUI;
using System;
using System.Text;
using VRageMath;

namespace ClientPlugin.Patches;

/// <summary>
/// Adds an STC Servers button to the right of the main menu's New Game button and, in dev
/// mode, an STC Dev Servers button below it.
/// </summary>
[HarmonyPatch(typeof(MyGuiScreenMainMenu), "RecreateControls")]
internal static class MainMenuPatch
{
    private const float ButtonSpacing = 0.003f;
    private const string ServersButtonName = "StcServers";
    private const string DevServersButtonName = "StcDevServers";

    // Runs on every rebuild, not just construction: resolution, language and control option
    // changes call RecreateControls(false), which clears all controls before this postfix.
    private static void Postfix(MyGuiScreenMainMenu __instance)
    {
        try
        {
            if (FindNewGameButton(__instance) is not { } newGameButton)
                return;

            __instance.Controls.Add(CreateButton(newGameButton, ServersButtonName, row: 0,
                "STC Servers", "Connect to Star Trek Continuum servers", newGameButton.ColorMask,
                () => new ServerSelectionDialog()));

            if (Config.Current.DevMode)
                AddDevButton(__instance, newGameButton);
        }
        catch (Exception ex)
        {
            Log.Error($"Error adding main menu button: {ex}");
        }
    }

    /// <summary>
    /// Brings the open main menu in line with the settings. Called when the settings dialog
    /// closes, so changes show without waiting for a menu rebuild.
    ///
    /// A Hide menu news change needs a rebuild, because only CreateRightSection can create the
    /// news controls again; the rebuild also re-adds the STC buttons. A Dev mode change alone
    /// adds or removes just that button, since RecreateControls also restarts the news download
    /// and reruns the menu's startup checks.
    /// </summary>
    internal static void ApplySettings()
    {
        try
        {
            var menu = MyScreenManager.GetFirstScreenOfType<MyGuiScreenMainMenu>();
            if (menu is not { IsLoaded: true })
                return;

            if (HideMenuNewsPatch.IsApplied(menu) != Config.Current.HideMenuNews)
            {
                menu.RecreateControls(false);
                return;
            }

            if (FindNewGameButton(menu) is not { } newGameButton)
                return;

            var devButton = menu.Controls.GetControlByName(DevServersButtonName);
            if (Config.Current.DevMode && devButton == null)
                AddDevButton(menu, newGameButton);
            else if (!Config.Current.DevMode && devButton != null)
                menu.Controls.Remove(devButton);
        }
        catch (Exception ex)
        {
            Log.Error($"Error applying settings to the main menu: {ex}");
        }
    }

    // Look the button up by the name MyGuiScreenMainMenuBase.MakeButton gives it; its label is
    // localized. The in-game pause menu has no New Game button, so it gets no STC buttons.
    private static MyGuiControlButton FindNewGameButton(MyGuiScreenMainMenu menu) =>
        menu.Controls.GetControlByName("NewGame") as MyGuiControlButton;

    private static void AddDevButton(MyGuiScreenMainMenu menu, MyGuiControlButton newGameButton)
    {
        menu.Controls.Add(CreateButton(newGameButton, DevServersButtonName, row: 1,
            "STC Dev Servers", "Connect to development servers (DEVMODE only)", Color.DarkOrange,
            () => new DevServerSelectionDialog()));
    }

    /// <summary>
    /// Creates a button styled like <paramref name="template"/>, slightly narrower, placed beside
    /// it and <paramref name="row"/> button heights below.
    /// </summary>
    private static MyGuiControlButton CreateButton(MyGuiControlButton template, string name, int row,
        string text, string toolTip, Vector4 colorMask, Func<MyGuiScreenBase> createDialog)
    {
        var position = template.Position + new Vector2(
            template.Size.X + ButtonSpacing,
            row * (template.Size.Y + ButtonSpacing));

        return new MyGuiControlButton(
            position: position,
            size: template.Size * new Vector2(0.9f, 1f),
            text: new StringBuilder(text),
            onButtonClick: _ => OpenDialog(createDialog),
            toolTip: toolTip,
            textScale: template.TextScale * 0.9f,
            visualStyle: template.VisualStyle,
            colorMask: colorMask)
        {
            Name = name,
        };
    }

    private static void OpenDialog(Func<MyGuiScreenBase> createDialog)
    {
        try
        {
            MyGuiSandbox.AddScreen(createDialog());
        }
        catch (Exception ex)
        {
            Log.Error($"Error opening server selection dialog: {ex}");
        }
    }
}
