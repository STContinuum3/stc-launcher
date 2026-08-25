using HarmonyLib;
using Sandbox.Graphics.GUI;
using SpaceEngineers.Game.GUI;
using ClientPlugin;
using ClientPlugin.GUI;
using System;
using System.Text;
using VRage.Game;
using VRage.Utils;
using VRageMath;

namespace ClientPlugin.Patches;

[HarmonyPatch(typeof(MyGuiScreenMainMenu), "RecreateControls")]
internal class MainMenuPatch
{
    private static void Postfix(MyGuiScreenMainMenu __instance, bool constructor)
    {
        try
        {
            MyLog.Default.Info("STCLauncher: MainMenuPatch.Postfix called");

            if (!constructor)
            {
                MyLog.Default.Info("STCLauncher: Not constructor, returning");
                return;
            }

            // Find the "New Game" button to position our button next to it
            MyGuiControlButton newGameButton = null;
            MyLog.Default.Info($"STCLauncher: Searching for New Game button among {__instance.Controls.Count} controls");

            foreach (var control in __instance.Controls.GetVisibleControls())
            {
                if (control is MyGuiControlButton button)
                {
                    MyLog.Default.Info($"STCLauncher: Found button with text: {button.Text?.ToString() ?? "null"}");
                    if (button.Text != null &&
                        (button.Text.ToString().Contains("New Game") ||
                         button.Text.ToString().Contains("NEW GAME") ||
                         button.Text.ToString().Contains("New World")))
                    {
                        newGameButton = button;
                        MyLog.Default.Info($"STCLauncher: Found New Game button at position {button.Position}");
                        break;
                    }
                }
            }

            if (newGameButton != null)
            {
                // Calculate position to the right of the New Game button
                // Adjust X position by button width plus spacing
                float buttonSpacing = 0.003f; // Small spacing between buttons
                Vector2 buttonPosition = newGameButton.Position + new Vector2(newGameButton.Size.X + buttonSpacing, 0f);

                MyLog.Default.Info($"STCLauncher: Creating STC button at position {buttonPosition}");

                // Create the Star Trek Continuum button with slightly smaller size to fit
                var stcButton = new MyGuiControlButton(
                    position: buttonPosition,
                    size: newGameButton.Size * new Vector2(0.9f, 1f), // Slightly narrower to fit
                    text: new StringBuilder("STC Servers"),
                    onButtonClick: OnStarTrekContinuumClick,
                    toolTip: "Connect to Star Trek Continuum servers",
                    textScale: newGameButton.TextScale * 0.9f, // Slightly smaller text
                    visualStyle: newGameButton.VisualStyle,
                    colorMask: newGameButton.ColorMask
                );

                // Add the button to the screen
                __instance.Controls.Add(stcButton);
                MyLog.Default.Info("STCLauncher: STC button added to main menu");

                // Add Dev Servers button if DevMode is enabled
                if (Config.Current.DevMode)
                {
                    MyLog.Default.Info("STCLauncher: DevMode is enabled, adding Dev Servers button");

                    // Calculate position below the STC Servers button
                    float devButtonSpacing = 0.003f; // Small vertical spacing between buttons
                    Vector2 devButtonPosition = buttonPosition + new Vector2(0f, newGameButton.Size.Y + devButtonSpacing);

                    // Create the Dev Servers button with orange tint
                    var devButton = new MyGuiControlButton(
                        position: devButtonPosition,
                        size: newGameButton.Size * new Vector2(0.9f, 1f), // Same size as STC button
                        text: new StringBuilder("STC Dev Servers"),
                        onButtonClick: OnDevServersClick,
                        toolTip: "Connect to development servers (DEVMODE only)",
                        textScale: newGameButton.TextScale * 0.9f,
                        visualStyle: newGameButton.VisualStyle,
                        colorMask: new Color(255, 140, 0) // Orange color for dev environment
                    );

                    __instance.Controls.Add(devButton);
                    MyLog.Default.Info("STCLauncher: Dev Servers button added to main menu");
                }
            }
            else
            {
                MyLog.Default.Warning("STCLauncher: Could not find New Game button on main menu");
            }
        }
        catch (Exception ex)
        {
            MyLog.Default.Error($"STCLauncher: Error adding main menu button: {ex}");
        }
    }

    private static void OnStarTrekContinuumClick(MyGuiControlButton sender)
    {
        try
        {
            MyLog.Default.Info("STCLauncher: STC button clicked, opening server selection dialog");
            // Open the server selection dialog
            var dialog = new ServerSelectionDialog();
            MyGuiSandbox.AddScreen(dialog);
            MyLog.Default.Info("STCLauncher: Server selection dialog added to screen");
        }
        catch (Exception ex)
        {
            MyLog.Default.Error($"STCLauncher: Error opening server selection dialog: {ex}");
        }
    }

    private static void OnDevServersClick(MyGuiControlButton sender)
    {
        try
        {
            MyLog.Default.Info("STCLauncher: Dev Servers button clicked, opening dev server selection dialog");
            // Open the development server selection dialog
            var dialog = new DevServerSelectionDialog();
            MyGuiSandbox.AddScreen(dialog);
            MyLog.Default.Info("STCLauncher: Dev server selection dialog added to screen");
        }
        catch (Exception ex)
        {
            MyLog.Default.Error($"STCLauncher: Error opening dev server selection dialog: {ex}");
        }
    }
}
