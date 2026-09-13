using Sandbox.Graphics.GUI;
using ClientPlugin.Networking;
using System;
using System.Text;
using VRage.Game;
using VRage.Utils;
using VRageMath;

namespace ClientPlugin.GUI;

public class DevServerSelectionDialog : MyGuiScreenBase
{
    // Development servers list
    private readonly ServerInfo[] devServers = new ServerInfo[]
    {
        new ServerInfo("Dev Lobby", "142.127.79.145", 27900, Color.Orange),
        new ServerInfo("Dev Fed 1", "142.127.79.145", 27901, Color.Orange),
        new ServerInfo("Dev Fed 2", "142.127.79.145", 27902, Color.Orange),
        new ServerInfo("Dev Deep Space", "142.127.79.145", 27903, Color.Orange)
    };

    public DevServerSelectionDialog() : base(
        position: new Vector2(0.5f, 0.5f),
        backgroundColor: MyGuiConstants.SCREEN_BACKGROUND_COLOR,
        size: new Vector2(0.5f, 0.7f))
    {
        EnabledBackgroundFade = true;
        m_closeOnEsc = true;
        m_drawEvenWithoutFocus = true;
        CanHideOthers = false;
        CanBeHidden = true;
        CloseButtonEnabled = true;

        RecreateControls(true);
    }

    // Layout. Controls use a centred origin, so a position is the middle of the control.
    private const float ButtonWidth = 0.35f;
    private const float ButtonHeight = 0.07f;
    private const float RowSpacing = ButtonHeight + 0.015f;
    private const float ListTop = -0.09f;
    private const float CloseButtonHeight = 0.06f;
    private const float SectionGap = 0.04f;

    public override void RecreateControls(bool constructor)
    {
        base.RecreateControls(constructor);

        // Title
        var title = new MyGuiControlLabel(
            position: new Vector2(0f, -0.29f),
            text: "Development Servers",
            textScale: 1.2f,
            colorMask: Color.Orange,
            originAlign: MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER
        );
        Controls.Add(title);

        // Warning subtitle
        var warning = new MyGuiControlLabel(
            position: new Vector2(0f, -0.235f),
            text: "⚠ FOR TESTING ONLY ⚠",
            textScale: 0.9f,
            colorMask: Color.OrangeRed,
            originAlign: MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER
        );
        Controls.Add(warning);

        // Subtitle
        var subtitle = new MyGuiControlLabel(
            position: new Vector2(0f, -0.18f),
            text: "Select a development server:",
            textScale: 0.9f,
            colorMask: Color.LightGray,
            originAlign: MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER
        );
        Controls.Add(subtitle);

        // Server buttons in a single column
        var buttonSize = new Vector2(ButtonWidth, ButtonHeight);

        for (int i = 0; i < devServers.Length; i++)
        {
            var buttonPosition = new Vector2(0f, ListTop + i * RowSpacing);
            Controls.Add(CreateServerButton(devServers[i], buttonPosition, buttonSize));
        }

        // Close button, placed below the last server rather than at a fixed offset, so that
        // adding servers can never park one on top of it.
        var listBottom = ListTop + (devServers.Length - 1) * RowSpacing + ButtonHeight / 2f;

        var closeButton = new MyGuiControlButton(
            position: new Vector2(0f, listBottom + SectionGap + CloseButtonHeight / 2f),
            size: new Vector2(0.2f, CloseButtonHeight),
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
            colorMask: server.FactionColor * 0.9f
        );

        // Add tooltip with server details
        var tooltip = new StringBuilder();
        tooltip.AppendLine($"Development Server: {server.Name}");
        tooltip.AppendLine($"Address: {server.Address}:{server.Port}");
        tooltip.AppendLine("⚠ Testing environment only");
        tooltip.AppendLine("Click to connect");

        button.SetToolTip(tooltip.ToString());

        return button;
    }

    private void OnServerButtonClick(ServerInfo server)
    {
        try
        {
            MyLog.Default.Info($"STCLauncher: Connecting to development server {server.Name}");

            // Close the dialog first
            CloseScreen();

            // Connect to server using native direct connect dialog
            ServerConnector.ConnectToServer(server);
        }
        catch (Exception ex)
        {
            MyLog.Default.Error($"STCLauncher: Error connecting to dev server {server.Name}: {ex}");

            MyGuiSandbox.AddScreen(MyGuiSandbox.CreateMessageBox(
                MyMessageBoxStyleEnum.Error,
                buttonType: MyMessageBoxButtonsType.OK,
                messageText: new StringBuilder($"Failed to connect to development server {server.Name}"),
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
        return "DevServerSelectionDialog";
    }
}
