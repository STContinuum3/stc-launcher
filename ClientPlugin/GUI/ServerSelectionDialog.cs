using ClientPlugin.Networking;
using System.Collections.Generic;
using VRageMath;

namespace ClientPlugin.GUI;

public class ServerSelectionDialog() : ServerDialogBase(new Vector2(0.6f, 0.8f))
{
    protected override float ButtonColorScale => 0.8f;

    protected override IEnumerable<string> TooltipLines(ServerInfo server) =>
    [
        $"Server: {server.Name}",
        $"Address: {server.GetFullAddress()}",
        "Click to connect",
    ];

    public override void RecreateControls(bool constructor)
    {
        base.RecreateControls(constructor);

        AddLabel(-0.32f, "Star Trek Continuum Servers", 1.2f);
        AddLabel(-0.26f, "Select a server to connect to:", 0.9f, Color.LightGray);

        // Lobby centered above a two column grid of the live servers
        var servers = StcServers.Current;
        AddServerButton(servers.Lobby, new Vector2(0f, -0.185f), 0.35f);
        var gridBottom = AddServerGrid(servers.Live, top: -0.055f, width: 0.22f, columns: 2, columnSpacing: 0.24f);

        AddCloseButton(gridBottom);
    }
}
