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

    // Runs on every rebuild, not just construction: resolution, language and control option
    // changes call RecreateControls(false), which clears all controls before this postfix.
    private static void Postfix(MyGuiScreenMainMenu __instance)
    {
        try
        {
            // Look the button up by the name MyGuiScreenMainMenuBase.MakeButton gives it; its
            // label is localized. The in-game pause menu has no New Game button, so it gets no
            // STC buttons.
            if (__instance.Controls.GetControlByName("NewGame") is not MyGuiControlButton newGameButton)
                return;

            var position = newGameButton.Position + new Vector2(newGameButton.Size.X + ButtonSpacing, 0f);
            __instance.Controls.Add(CreateButton(newGameButton, position,
                "STC Servers", "Connect to Star Trek Continuum servers", newGameButton.ColorMask,
                () => new ServerSelectionDialog()));

            if (Config.Current.DevMode)
            {
                position.Y += newGameButton.Size.Y + ButtonSpacing;
                __instance.Controls.Add(CreateButton(newGameButton, position,
                    "STC Dev Servers", "Connect to development servers (DEVMODE only)", Color.DarkOrange,
                    () => new DevServerSelectionDialog()));
            }
        }
        catch (Exception ex)
        {
            Log.Error($"Error adding main menu button: {ex}");
        }
    }

    /// <summary>Creates a button styled like <paramref name="template"/>, slightly narrower to fit beside it.</summary>
    private static MyGuiControlButton CreateButton(MyGuiControlButton template, Vector2 position, string text,
        string toolTip, Vector4 colorMask, Func<MyGuiScreenBase> createDialog)
    {
        return new MyGuiControlButton(
            position: position,
            size: template.Size * new Vector2(0.9f, 1f),
            text: new StringBuilder(text),
            onButtonClick: _ => OpenDialog(createDialog),
            toolTip: toolTip,
            textScale: template.TextScale * 0.9f,
            visualStyle: template.VisualStyle,
            colorMask: colorMask);
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
