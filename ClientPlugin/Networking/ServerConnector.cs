using Sandbox;
using Sandbox.Game.Screens;
using Sandbox.Graphics.GUI;
using System;
using System.Text;
using VRageMath;

namespace ClientPlugin.Networking;

/// <summary>
/// Connects by driving the game's own Direct Connect screen, so the game still shows its
/// progress screen, reports unresponsive servers and records the last session.
/// </summary>
public static class ServerConnector
{
    public static void ConnectToServer(ServerInfo server)
    {
        Log.Info($"Connecting to {server.Name} at {server.GetFullAddress()}");

        // AddScreen only queues the screen, so the address is filled in and Connect clicked on
        // a later frame, once the screen is up and ahead of the game's connection progress screen.
        MySandboxGame.Static.Invoke(() =>
        {
            try
            {
                var screen = new MyGuiScreenServerConnect();
                MyGuiSandbox.AddScreen(screen);
                MySandboxGame.Static.Invoke(() => FillAndConnect(screen, server), "StcMenu.ConnectToServer.FillAndClick");
            }
            catch (Exception ex)
            {
                Log.Error($"Failed to open direct connect screen: {ex}");
                ShowConnectionError(server.Name, ex.Message);
            }
        }, "StcMenu.ConnectToServer");
    }

    private static void FillAndConnect(MyGuiScreenServerConnect screen, ServerInfo server)
    {
        try
        {
            if (screen.Controls.GetControlByName("Textbox") is not MyGuiControlTextbox addressBox)
            {
                Log.Warning("Could not find IP textbox in MyGuiScreenServerConnect");
                return;
            }

            addressBox.SetText(new StringBuilder(server.GetFullAddress()));

            if (screen.Controls.GetControlByName("Button") is not MyGuiControlButton connectButton)
            {
                Log.Warning("Could not find connect button in MyGuiScreenServerConnect");
                return;
            }

            connectButton.RaiseButtonClicked();
        }
        catch (Exception ex)
        {
            Log.Error($"Error interacting with direct connect screen: {ex}");
        }
    }

    private static void ShowConnectionError(string serverName, string errorMessage)
    {
        MyGuiSandbox.AddScreen(MyGuiSandbox.CreateMessageBox(
            MyMessageBoxStyleEnum.Error,
            buttonType: MyMessageBoxButtonsType.OK,
            messageText: new StringBuilder($"Failed to connect to {serverName}\n\nError: {errorMessage}\n\nPlease check that:\n- The server is online\n- Your internet connection is stable\n- No firewall is blocking the connection"),
            messageCaption: new StringBuilder("Connection Failed"),
            size: new Vector2(0.7f, 0.5f)));
    }
}
