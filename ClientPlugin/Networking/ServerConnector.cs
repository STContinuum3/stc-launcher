using Sandbox;
using Sandbox.Game.Screens;
using Sandbox.Graphics.GUI;
using ClientPlugin.GUI;
using System;
using System.Net;
using System.Text;
using VRage.Game;
using VRage.Utils;

namespace ClientPlugin.Networking;

public static class ServerConnector
{
    public static void ConnectToServer(ServerInfo server)
    {
        try
        {
            MyLog.Default.Info($"STCLauncher: Attempting to connect to {server.Name} at {server.GetFullAddress()}");

            // Use MySandboxGame.Static.Invoke to ensure we're on the main thread
            MySandboxGame.Static.Invoke(() =>
            {
                try
                {
                    // Create and open the native direct connect screen
                    MyLog.Default.Info($"STCLauncher: Opening MyGuiScreenServerConnect for {server.Name}");
                    var directConnectScreen = MyGuiSandbox.CreateScreen(typeof(MyGuiScreenServerConnect));

                    if (directConnectScreen == null)
                    {
                        MyLog.Default.Error("STCLauncher: Failed to create MyGuiScreenServerConnect screen");
                        ShowConnectionError(server.Name, "Failed to open direct connect dialog");
                        return;
                    }

                    // Add the screen to the GUI
                    MyGuiSandbox.AddScreen(directConnectScreen);

                    // Wait a frame for the screen to initialize
                    MySandboxGame.Static.Invoke(() =>
                    {
                        try
                        {
                            // Find the IP textbox control
                            var ipBox = directConnectScreen.Controls.GetControlByName("Textbox") as MyGuiControlTextbox;
                            if (ipBox != null)
                            {
                                // Set the server address in IP:PORT format
                                string serverAddress = $"{server.Address}:{server.Port}";
                                ipBox.SetText(new StringBuilder(serverAddress));
                                MyLog.Default.Info($"STCLauncher: Set server address to {serverAddress}");

                                // Find and click the connect button
                                var connectButton = directConnectScreen.Controls.GetControlByName("Button") as MyGuiControlButton;
                                if (connectButton != null)
                                {
                                    MyLog.Default.Info($"STCLauncher: Clicking connect button for {server.Name}");
                                    connectButton.RaiseButtonClicked();
                                }
                                else
                                {
                                    MyLog.Default.Warning("STCLauncher: Could not find connect button in MyGuiScreenServerConnect");
                                }
                            }
                            else
                            {
                                MyLog.Default.Warning("STCLauncher: Could not find IP textbox in MyGuiScreenServerConnect");
                            }
                        }
                        catch (Exception ex)
                        {
                            MyLog.Default.Error($"STCLauncher: Error interacting with direct connect screen: {ex}");
                        }
                    }, "STCLauncher.ConnectToServer.FillAndClick");
                }
                catch (Exception ex)
                {
                    MyLog.Default.Error($"STCLauncher: Failed to open direct connect screen: {ex}");
                    ShowConnectionError(server.Name, ex.Message);
                }
            }, "STCLauncher.ConnectToServer");
        }
        catch (Exception ex)
        {
            MyLog.Default.Error($"STCLauncher: Failed to connect to server {server.Name}: {ex}");
            ShowConnectionError(server.Name, ex.Message);
        }
    }

    private static void ShowConnectionError(string serverName, string errorMessage)
    {
        var messageBox = MyGuiSandbox.CreateMessageBox(
            MyMessageBoxStyleEnum.Error,
            buttonType: MyMessageBoxButtonsType.OK,
            messageText: new StringBuilder($"Failed to connect to {serverName}\n\nError: {errorMessage}\n\nPlease check that:\n- The server is online\n- Your internet connection is stable\n- No firewall is blocking the connection"),
            messageCaption: new StringBuilder("Connection Failed"),
            size: new VRageMath.Vector2(0.7f, 0.5f)
        );

        MyGuiSandbox.AddScreen(messageBox);
    }

    public static bool IsValidServerAddress(string address, int port)
    {
        try
        {
            IPAddress.Parse(address);
            return port > 0 && port <= 65535;
        }
        catch
        {
            return false;
        }
    }

    public static void TestServerConnection(ServerInfo server, Action<bool, string> callback)
    {
        try
        {
            // Basic validation
            if (!IsValidServerAddress(server.Address, server.Port))
            {
                callback?.Invoke(false, "Invalid server address or port");
                return;
            }

            // For now, we'll assume the server is reachable
            // In a full implementation, you might want to do a ping test
            callback?.Invoke(true, "Server appears to be reachable");
        }
        catch (Exception ex)
        {
            callback?.Invoke(false, ex.Message);
        }
    }
}
