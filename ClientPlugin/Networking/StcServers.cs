using VRageMath;

namespace ClientPlugin.Networking;

public class ServerInfo(string name, string address, int port, Color factionColor)
{
    public string Name { get; } = name;
    public string Address { get; } = address;
    public int Port { get; } = port;
    public Color FactionColor { get; } = factionColor;

    public string GetFullAddress() => $"{Address}:{Port}";
}

/// <summary>The Star Trek Continuum servers offered by the selection dialogs.</summary>
public static class StcServers
{
    private const string Host = "142.127.79.145";

    public static readonly ServerInfo Lobby = new ServerInfo("Lobby", Host, 27269, Color.Cyan);

    public static readonly ServerInfo[] Live =
    [
        new ServerInfo("Federation 1", Host, 27261, Color.Blue),
        new ServerInfo("Federation 2", Host, 27262, Color.Blue),
        new ServerInfo("Klingon 1", Host, 27263, Color.Red),
        new ServerInfo("Klingon 2", Host, 27264, Color.Red),
        new ServerInfo("Deep Space", Host, 27268, Color.Purple),
    ];

    public static readonly ServerInfo[] Dev =
    [
        new ServerInfo("Dev Lobby", Host, 27900, Color.Orange),
        new ServerInfo("Dev Fed 1", Host, 27901, Color.Orange),
        new ServerInfo("Dev Fed 2", Host, 27902, Color.Orange),
        new ServerInfo("Dev Deep Space", Host, 27903, Color.Orange),
    ];
}
