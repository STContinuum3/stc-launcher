using ClientPlugin.Networking;
using System.Collections.Generic;
using VRageMath;

namespace ClientPlugin.GUI;

public class DevServerSelectionDialog() : ServerDialogBase(new Vector2(0.5f, 0.7f))
{
    protected override float ButtonColorScale => 0.9f;

    protected override IEnumerable<string> TooltipLines(ServerInfo server) =>
    [
        $"Development Server: {server.Name}",
        $"Address: {server.GetFullAddress()}",
        "⚠ Testing environment only",
        "Click to connect",
    ];

    protected override string DisplayName(ServerInfo server) => $"development server {server.Name}";

    public override void RecreateControls(bool constructor)
    {
        base.RecreateControls(constructor);

        AddLabel(-0.29f, "Development Servers", 1.2f, Color.Orange);
        AddLabel(-0.235f, "⚠ FOR TESTING ONLY ⚠", 0.9f, Color.OrangeRed);
        AddLabel(-0.18f, "Select a development server:", 0.9f, Color.LightGray);

        var listBottom = AddServerGrid(StcServers.Current.Dev, top: -0.09f, width: 0.35f);

        AddCloseButton(listBottom);
    }

    protected override void OnServerClick(ServerInfo server)
    {
        Log.Info($"Connecting to development server {server.Name}");
        base.OnServerClick(server);
    }
}
