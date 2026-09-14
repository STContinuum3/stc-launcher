# StcMenu

Space Engineers client plugin for Star Trek Continuum, loaded by [Pulsar](https://github.com/SpaceGT/Pulsar).

- Adds **STC Servers** next to the main menu's New Game button, opening a server list that
  connects through the game's Direct Connect screen. With **Dev mode** on, an
  **STC Dev Servers** button opens the development servers.
- Replaces the main menu background with the Star Trek Continuum video and plays its soundtrack
  instead of the stock menu track, following the music volume slider.
- Hides the main menu news panel and DLC banners. The Newsletter button stays.

Settings (Pulsar plugin list → StcMenu → config): **Enable custom video**, **Dev mode**.

## Network access

The plugin contacts two hosts, both over HTTPS:

- **github.com** (via Pulsar): the background video, downloaded as a plugin asset (see [Media](#media)).
- **raw.githubusercontent.com** (by the plugin): [`servers.json`](servers.json) from the `main`
  branch, once per game launch.

## Servers

The server list lives in [`servers.json`](servers.json). Edit it on `main` to move or add
servers; players pick up the change the next time they start the game, with no plugin release.
If the download fails or the file is invalid, the plugin logs a warning and uses the built-in
copy in `ClientPlugin/Networking/StcServers.cs`, so keep that copy in step when servers change.

Each server has a `Name`, an `Address`, a `Port` and a `Color` (`#RRGGBB`, used to tint its
button). `Lobby` is required; `Live` and `Dev` are lists.

## Media

The main menu background video (`star_trek_background.wmv`) is not in this repository. It is
published as `stc-menu-video-v1.zip` on the
[`media-v1` release](https://github.com/STContinuum3/stc-launcher/releases/tag/media-v1), and
Pulsar downloads it as a plugin asset, checking the SHA-256 in `StcMenu.xml`.

- **Footage:** recorded in game by Star Trek Continuum members.
- **Soundtrack:** created by siege3581.

The video and its soundtrack are **not** covered by the MIT licence in [`LICENSE`](LICENSE), which
applies to the source code only. All rights remain with their creators; they are distributed
with StcMenu with permission and may not be reused outside it.

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

Players get the plugin from PluginHub, where Pulsar compiles it from source and downloads the
background video as a plugin asset (declared in `StcMenu.xml`). Test the same way with a Pulsar
development folder:

1. Start Pulsar with the `-sources` command line option and open **Sources**.
2. **Add Development Folder** → select this repository → **Select a plugin data file? Yes** → `StcMenu.xml`.
3. Enable the development folder plugin in your profile and start the game.

Do not also load a copy of `StcMenu.dll` from Pulsar's `Local` folder, or every patch is
applied twice.

To test a pushed branch exactly as PluginHub builds it, add it in **Sources** as a remote
plugin with `StcMenu.xml`. Pulsar reads the XML from the branch but compiles the commit in its
`<Commit>`, so set that to a pushed commit containing the code under test. Pulsar also caches
the XML for two hours; delete `%AppData%\Pulsar\Legacy\Sources\Plugins\STContinuum3-stc-launcher.bin`
to make it fetch a new one on the next start.

## Updating the video

1. Encode the new video as WMV (the game plays menu videos through DirectShow). Keep it small:
   Pulsar allows 30 seconds for the whole download, so aim for a zip of 15 MB or less.
2. Zip it with `star_trek_background.wmv` at the root of the archive, as `stc-menu-video-vN.zip`.
3. Test it before publishing: copy `StcMenu.xml` to `StcMenu.local.xml` (ignored by git), change
   its asset to `<Asset Name="AssetFolder" Path="stc-menu-video-vN.zip" Extract="true" />` with
   the zip in the repository root, and select `StcMenu.local.xml` as the development folder's
   plugin data file.
4. Publish the zip on a new `media-vN` release, then update `Url` and `Sha256`
   (`Get-FileHash -Algorithm SHA256`) in `StcMenu.xml` and in the PluginHub registry copy.
   Changing the asset needs a PluginHub pull request, like a code release.
5. Update the credits in [Media](#media) if they change.

## Release

1. Bump `<Version>` in `Version.Build.props` and the Pulsar build version in the
   `#if !LOCAL_BUILD` block of `ClientPlugin/Plugin.cs`.
2. Commit and push, then put that commit hash into `<Commit>` of the plugin's XML in the
   PluginHub registry (`StcMenu.xml` here is the template for it).
