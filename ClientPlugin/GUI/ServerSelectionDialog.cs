using Sandbox.Graphics.GUI;
using ClientPlugin.Networking;
using System;
using System.Text;
using VRage.Game;
using VRage.Utils;
using VRageMath;

namespace ClientPlugin.GUI;

public class ServerSelectionDialog : MyGuiScreenBase
{
    private readonly ServerInfo lobbyServer = new ServerInfo("Lobby", "142.127.79.145", 27269, Color.Cyan);

    private readonly ServerInfo[] servers = new ServerInfo[]
    {
        new ServerInfo("Federation 1", "142.127.79.145", 27261, Color.Blue),
        new ServerInfo("Federation 2", "142.127.79.145", 27262, Color.Blue),
        new ServerInfo("Klingon 1", "142.127.79.145", 27263, Color.Red),
        new ServerInfo("Klingon 2", "142.127.79.145", 27264, Color.Red),
        new ServerInfo("Romulan 1", "142.127.79.145", 27265, Color.Green),
        new ServerInfo("Romulan 2", "142.127.79.145", 27266, Color.Green),
        new ServerInfo("Core", "142.127.79.145", 27267, Color.Yellow),
        new ServerInfo("Deep Space", "142.127.79.145", 27268, Color.Purple)
    };

    public ServerSelectionDialog() : base(
        position: new Vector2(0.5f, 0.5f),
        backgroundColor: MyGuiConstants.SCREEN_BACKGROUND_COLOR,
        size: new Vector2(0.6f, 0.8f))
    {
        EnabledBackgroundFade = true;
        m_closeOnEsc = true;
        m_drawEvenWithoutFocus = true;
        CanHideOthers = false;
        CanBeHidden = true;
        CloseButtonEnabled = true;

        RecreateControls(true);
    }

    public override void RecreateControls(bool constructor)
    {
        base.RecreateControls(constructor);

        // Title
        var title = new MyGuiControlLabel(
            position: new Vector2(0f, -0.28f),
            text: "Star Trek Continuum Servers",
            textScale: 1.2f,
            originAlign: MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER
        );
        Controls.Add(title);

        // Subtitle
        var subtitle = new MyGuiControlLabel(
            position: new Vector2(0f, -0.22f),
            text: "Select a server to connect to:",
            textScale: 0.9f,
            colorMask: Color.LightGray,
            originAlign: MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER
        );
        Controls.Add(subtitle);

        // Lobby button - centered above the grid
        var lobbyButton = CreateServerButton(lobbyServer, new Vector2(0f, -0.12f), new Vector2(0.35f, 0.08f));
        Controls.Add(lobbyButton);

        // Server buttons in a 2x4 grid
        const float buttonWidth = 0.22f;
        const float buttonHeight = 0.08f;
        const float horizontalSpacing = 0.25f;
        const float verticalSpacing = 0.1f;

        Vector2 startPosition = new Vector2(-horizontalSpacing / 2f, -0.01f);

        for (int i = 0; i < servers.Length; i++)
        {
            int row = i / 2;
            int col = i % 2;

            Vector2 buttonPosition = startPosition + new Vector2(
                col * horizontalSpacing,
                row * verticalSpacing
            );

            var serverButton = CreateServerButton(servers[i], buttonPosition, new Vector2(buttonWidth, buttonHeight));
            Controls.Add(serverButton);
        }

        // Close button
        var closeButton = new MyGuiControlButton(
            position: new Vector2(0f, 0.33f),
            size: new Vector2(0.2f, 0.06f),
            text: new StringBuilder("Close"),
            onButtonClick: OnCloseClick,
            visualStyle: MyGuiControlButtonStyleEnum.Default
        );
        Controls.Add(closeButton);
    }

    private MyGuiControlButton CreateServerButton(ServerInfo server, Vector2 position, Vector2 size)
    {
        var button = new MyGuiControlButton(
            position: position,
            size: size,
            text: new StringBuilder(server.Name),
            onButtonClick: (btn) => OnServerButtonClick(server),
            visualStyle: MyGuiControlButtonStyleEnum.Default,
            textScale: 0.8f,
            colorMask: server.FactionColor * 0.8f
        );

        // Add tooltip with server details
        var tooltip = new StringBuilder();
        tooltip.AppendLine($"Server: {server.Name}");
        tooltip.AppendLine($"Address: {server.Address}:{server.Port}");
        tooltip.AppendLine("Click to connect");

        button.SetToolTip(tooltip.ToString());

        return button;
    }

    private void OnServerButtonClick(ServerInfo server)
    {
        try
        {
            // Close the dialog first
            CloseScreen();

            // Connect to server using native direct connect dialog
            ServerConnector.ConnectToServer(server);
        }
        catch (Exception ex)
        {
            MyLog.Default.Error($"STCLauncher: Error connecting to server {server.Name}: {ex}");

            MyGuiSandbox.AddScreen(MyGuiSandbox.CreateMessageBox(
                MyMessageBoxStyleEnum.Error,
                buttonType: MyMessageBoxButtonsType.OK,
                messageText: new StringBuilder($"Failed to connect to {server.Name}"),
                messageCaption: new StringBuilder("Connection Error")
            ));
        }
    }

    private void OnCloseClick(MyGuiControlButton sender)
    {
        CloseScreen();
    }

    public override string GetFriendlyName()
    {
        return "ServerSelectionDialog";
    }
}

public class ServerInfo
{
    public string Name { get; }
    public string Address { get; }
    public int Port { get; }
    public Color FactionColor { get; }

    public ServerInfo(string name, string address, int port, Color factionColor)
    {
        Name = name;
        Address = address;
        Port = port;
        FactionColor = factionColor;
    }

    public string GetFullAddress()
    {
        return $"{Address}:{Port}";
    }
}
