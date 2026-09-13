# StcMenu

Space Engineers client plugin for Star Trek Continuum, loaded by [Pulsar](https://github.com/SpaceGT/Pulsar).

- Adds **STC Servers** next to the main menu's New Game button, opening a server list that
  connects through the game's Direct Connect screen. With **Dev mode** on, an
  **STC Dev Servers** button opens the development servers.
- Replaces the main menu background with the Star Trek video and plays its soundtrack
  instead of the stock menu track, following the music volume slider.
- Hides the main menu news panel and DLC banners.

Settings (Pulsar plugin list → StcMenu → config): **Enable custom video**, **Dev mode**.
Servers are defined in `ClientPlugin/Networking/StcServers.cs`.

## Prerequisites

- [Space Engineers](https://store.steampowered.com/app/244850/Space_Engineers/) and [Pulsar](https://github.com/SpaceGT/Pulsar)
- [.NET Framework 4.8.1 Developer Pack](https://dotnet.microsoft.com/en-us/download/dotnet-framework/net481) and
  [.NET 10 SDK](https://dotnet.microsoft.com/en-us/download/dotnet/10.0)

## Build

```
MSBuild StcMenu.sln -p:Configuration=Release
```

The build compiles `net48` and `net10.0` against the game DLLs in `Bin64`, which is detected
from the Steam registry key or the usual Steam folders. If detection fails, create
`Directory.Build.props.user` (not committed) next to `Directory.Build.props`:

```xml
<Project>
  <PropertyGroup>
    <Bin64>D:\Steam\steamapps\common\SpaceEngineers\Bin64</Bin64>
  </PropertyGroup>
</Project>
```

The MSBuild build is a compile check only; it does not deploy anything.

## Test in game

Players get the plugin from PluginHub, where Pulsar compiles it from source and copies the
background video from `ClientPlugin/Resources` as a plugin asset (declared in `StcMenu.xml`).
Test the same way with a Pulsar development folder:

1. Start Pulsar with the `-sources` command line option and open **Sources**.
2. **Add Development Folder** → select this repository → **Select a plugin data file? Yes** → `StcMenu.xml`.
3. Enable the development folder plugin in your profile and start the game.

Do not also load a copy of `StcMenu.dll` from Pulsar's `Local` folder, or every patch is
applied twice.

## Release

1. Bump `<Version>` in `Version.Build.props` and the Pulsar build version in the
   `#if !LOCAL_BUILD` block of `ClientPlugin/Plugin.cs`.
2. Commit and push, then put that commit hash into `<Commit>` of the plugin's XML in the
   PluginHub registry (`StcMenu.xml` here is the template for it).
