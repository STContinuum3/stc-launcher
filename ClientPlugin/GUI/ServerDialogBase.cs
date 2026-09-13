using ClientPlugin.Networking;
using Sandbox.Graphics.GUI;
using System;
using System.Collections.Generic;
using System.Text;
using VRage.Utils;
using VRageMath;

namespace ClientPlugin.GUI;

/// <summary>
/// Shared layout and connect behaviour for the server selection dialogs.
/// Controls use a centred origin, so a position is the middle of the control.
/// </summary>
public abstract class ServerDialogBase : MyGuiScreenBase
{
    private const float ButtonHeight = 0.07f;
    private const float RowSpacing = ButtonHeight + 0.015f;
    private const float CloseButtonHeight = 0.06f;
    private const float SectionGap = 0.04f;

    protected ServerDialogBase(Vector2 size) : base(backgroundColor: MyGuiConstants.SCREEN_BACKGROUND_COLOR, size: size)
    {
        EnabledBackgroundFade = true;
        m_drawEvenWithoutFocus = true;
        CanHideOthers = false;
        CloseButtonEnabled = true;

        RecreateControls(true);
    }

    public override string GetFriendlyName() => GetType().Name;

    /// <summary>Multiplier applied to a server's faction color to tint its button.</summary>
    protected abstract float ButtonColorScale { get; }

    protected abstract IEnumerable<string> TooltipLines(ServerInfo server);

    /// <summary>How the server is named in connection error messages.</summary>
    protected virtual string DisplayName(ServerInfo server) => server.Name;

    protected void AddLabel(float y, string text, float textScale, Vector4? colorMask = null)
    {
        Controls.Add(new MyGuiControlLabel(
            position: new Vector2(0f, y),
            text: text,
            textScale: textScale,
            colorMask: colorMask,
            originAlign: MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER));
    }

    protected void AddServerButton(ServerInfo server, Vector2 position, float width)
    {
        var tooltip = new StringBuilder();
        foreach (var line in TooltipLines(server))
            tooltip.AppendLine(line);

        Controls.Add(new MyGuiControlButton(
            position: position,
            size: new Vector2(width, ButtonHeight),
            text: new StringBuilder(server.Name),
            toolTip: tooltip.ToString(),
            colorMask: server.FactionColor * ButtonColorScale,
            onButtonClick: _ => OnServerClick(server)));
    }

    /// <summary>Adds server buttons in rows of <paramref name="columns"/>.</summary>
    /// <returns>The Y coordinate of the bottom edge of the last row.</returns>
    protected float AddServerGrid(ServerInfo[] servers, float top, float width, int columns = 1, float columnSpacing = 0f)
    {
        var origin = new Vector2(-(columns - 1) * columnSpacing / 2f, top);
        for (int i = 0; i < servers.Length; i++)
        {
            AddServerButton(servers[i], origin + new Vector2(i % columns * columnSpacing, i / columns * RowSpacing), width);
        }

        var rowCount = (servers.Length + columns - 1) / columns;
        return top + (rowCount - 1) * RowSpacing + ButtonHeight / 2f;
    }

    /// <summary>
    /// Adds the Close button below the content rather than at a fixed offset, so that adding
    /// servers can never park a button on top of it.
    /// </summary>
    protected void AddCloseButton(float contentBottom)
    {
        Controls.Add(new MyGuiControlButton(
            position: new Vector2(0f, contentBottom + SectionGap + CloseButtonHeight / 2f),
            size: new Vector2(0.2f, CloseButtonHeight),
            text: new StringBuilder("Close"),
            onButtonClick: _ => CloseScreen()));
    }

    protected virtual void OnServerClick(ServerInfo server)
    {
        try
        {
            CloseScreen();
            ServerConnector.ConnectToServer(server);
        }
        catch (Exception ex)
        {
            MyLog.Default.Error($"STCLauncher: Error connecting to {DisplayName(server)}: {ex}");

            MyGuiSandbox.AddScreen(MyGuiSandbox.CreateMessageBox(
                MyMessageBoxStyleEnum.Error,
                buttonType: MyMessageBoxButtonsType.OK,
                messageText: new StringBuilder($"Failed to connect to {DisplayName(server)}"),
                messageCaption: new StringBuilder("Connection Error")));
        }
    }
}
