using LitJson;
using System;
using System.Linq;
using System.Net;
using VRage;
using VRage.Http;
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

/// <summary>One complete set of the servers offered by the selection dialogs.</summary>
public class ServerList(ServerInfo lobby, ServerInfo[] live, ServerInfo[] dev)
{
    public ServerInfo Lobby { get; } = lobby;
    public ServerInfo[] Live { get; } = live;
    public ServerInfo[] Dev { get; } = dev;

    public int Count => 1 + Live.Length + Dev.Length;
}

/// <summary>
/// The Star Trek Continuum servers offered by the selection dialogs.
///
/// The list is read from servers.json on the repository's main branch once per game launch, so
/// a server move only needs that file edited, not a plugin release. Until the download finishes,
/// or if it fails or the file is invalid, the built-in list below is used.
/// </summary>
public static class StcServers
{
    private const string ServerListUrl = "https://raw.githubusercontent.com/STContinuum3/stc-launcher/main/servers.json";

    private const string Host = "142.127.79.145";

    // Fallback copy of servers.json; keep the two in step
    private static readonly ServerList BuiltIn = new(
        new ServerInfo("Lobby", Host, 27269, Color.Cyan),
        [
            new ServerInfo("Federation 1", Host, 27261, Color.Blue),
            new ServerInfo("Federation 2", Host, 27262, Color.Blue),
            new ServerInfo("Klingon 1", Host, 27263, Color.Red),
            new ServerInfo("Klingon 2", Host, 27264, Color.Red),
            new ServerInfo("Deep Space", Host, 27268, Color.Purple),
        ],
        [
            new ServerInfo("Dev Lobby", Host, 27900, Color.Orange),
            new ServerInfo("Dev Fed 1", Host, 27901, Color.Orange),
            new ServerInfo("Dev Fed 2", Host, 27902, Color.Orange),
            new ServerInfo("Dev Deep Space", Host, 27903, Color.Orange),
        ]);

    /// <summary>
    /// The servers to offer. Replaced as a whole when servers.json loads, so a dialog reading it
    /// never sees a mix of old and new entries.
    /// </summary>
    public static ServerList Current { get; private set; } = BuiltIn;

    /// <summary>Starts downloading servers.json in the background.</summary>
    public static void Refresh()
    {
        // The game's own HTTP client, as used for the DLC banners. It calls back on a worker thread.
        MyVRage.Platform.Http.SendRequestAsync(ServerListUrl, null, HttpMethod.GET, OnResponse, predetermined: true);
    }

    private static void OnResponse(HttpStatusCode status, string content)
    {
        try
        {
            if (status != HttpStatusCode.OK)
            {
                Log.Warning($"Could not download servers.json (HTTP {(int)status}), using the built-in server list");
                return;
            }

            var servers = Parse(content);
            Current = servers;
            Log.Info($"Loaded {servers.Count} servers from servers.json");
        }
        catch (Exception ex)
        {
            Log.Warning($"Invalid servers.json, using the built-in server list: {ex.Message}");
        }
    }

    private static ServerList Parse(string json)
    {
        var file = JsonMapper.ToObject<ServerListJson>(json) ?? throw new FormatException("The file is empty");

        return new ServerList(
            ToServerInfo(file.Lobby ?? throw new FormatException("Lobby is missing")),
            (file.Live ?? []).Select(ToServerInfo).ToArray(),
            (file.Dev ?? []).Select(ToServerInfo).ToArray());
    }

    private static ServerInfo ToServerInfo(ServerJson server)
    {
        if (string.IsNullOrWhiteSpace(server?.Name) || string.IsNullOrWhiteSpace(server.Address))
            throw new FormatException("A server is missing its Name or Address");

        if (server.Port is < 1 or > 65535)
            throw new FormatException($"{server.Name} has an invalid Port {server.Port}");

        var color = ParseColor(server.Color)
            ?? throw new FormatException($"{server.Name} has an invalid Color '{server.Color}', expected #RRGGBB");

        return new ServerInfo(server.Name, server.Address, server.Port, color);
    }

    // FromHtml returns null for a malformed value but throws on bad hex digits
    private static Color? ParseColor(string html)
    {
        try
        {
            return ColorExtensions.FromHtml(html);
        }
        catch (FormatException)
        {
            return null;
        }
    }

    // Property names match the keys in servers.json. Unknown keys are ignored.
    private class ServerListJson
    {
        public ServerJson Lobby { get; set; }
        public ServerJson[] Live { get; set; }
        public ServerJson[] Dev { get; set; }
    }

    private class ServerJson
    {
        public string Name { get; set; }
        public string Address { get; set; }
        public int Port { get; set; }
        public string Color { get; set; }
    }
}
