# StcMenu Cleanup Plan

**Status:** IMPLEMENTED on 2026-09-13 in 12 commits `e8297ed..cb87d58` (not pushed). C# went from 3,237 to 1,065 lines, and tracked files from 57 to 34. Both MSBuild targets and a Pulsar-style compile (no `LOCAL_BUILD`) build clean after every commit. **Still pending:** the in-game checks in §8 (run through a Pulsar dev folder), and the StcMenu.xml values in D8.
**Branch audited:** `stcmenu-rebrand-and-menu-background` @ `1a8dc08`. Game code is v1.210.014.
**How it was produced:**
- 8 agents each audited one area: Settings framework, Config + entry point, Tools/preloader, Harmony patches, GUI dialogs, Networking + Assets, build/repo infra, and a whole-program reachability pass.
- 2 more agents then tried to disprove the high-impact claims against the decompiled game code, the decompiled Pulsar loader, the .NET SDK targets and git history.
- Every item below survived that check. Items that failed it are listed in §9.

## Headline numbers

| | Now | After (all phases, recommended options) |
|---|---|---|
| C# lines (`ClientPlugin/**/*.cs`) | 3,237 | ~1,100–1,250 |
| C# files | 33 | ~21 (15 deleted, 3 added: `StcServers`, `ServerDialogBase`, `Log`) |
| Other tracked non-asset lines removed | — | ~650 (App.config, setup.py, scripts, template comments) |

The bulk of the reduction comes from three places:
1. **Unused template scaffolding** (~1,100 lines): `Tools/`, the preloader stub, unused Settings element types, and `App.config`.
2. **Code that only supports six no-op settings** (~540 lines).
3. **Duplication** (~400 lines): the two server dialogs, MainMenuPatch, and AssetLoader.

The audit also turned up **4 real bugs** (§6). All are confirmed against game or SDK code. They are kept separate so you can decide whether they ride along.

---

## 1. Decisions

| # | Question | Decision / recommendation | Affects |
|---|---|---|---|
| D1 | Remove the **6 settings that nothing reads** (`ShowServerPing`, `PreferredServer`, `ServerDisplayMode`, `FactionThemeColor`, `AutoConnectToLastServer`, `QuickConnectKeybind`)? The settings dialog would lose 6 rows. Existing `StcMenu.cfg` files still load, because XmlSerializer ignores unknown elements. | ✅ **DECIDED: remove.** | Phase 3 |
| D2 | **How is the plugin distributed?** | ✅ **DECIDED: PluginHub (Pulsar source build), and it must include the video.** Pulsar's source compiler embeds no resources, so the video has to ship as a Pulsar *asset*. Design in §1a. | §1a, A3/A4/C4, C5/A8, B12 |
| D3 | Include the **bug fixes** (§6) in this cleanup? | ✅ **DECIDED: include**, as their own commits after the zero-behavior phases. | Phase 5 |
| D4 | Keep **Linux build support** (csproj Linux branches, `Deploy.sh`, `verify_props.sh`, `clean.sh`)? | Your call. Keeping costs little. Dropping saves ~90 lines. Linux *players* are unaffected either way, because Pulsar compiles from source. | Phase 6 |
| D5 | Prune the **22 game DLL references** the code provably doesn't touch? | **No.** It doesn't help Pulsar builds, carries CS0012 risk, and the se-dev plugin guidance says not to touch game references. | §9 |
| D6 | Rename the log prefix **`STCLauncher:`** (64 sites, pre-rebrand name) to `StcMenu:` via one constant? | **Yes**, unless you have log-scraping that greps for `STCLauncher`. | Phase 4 |
| D7 | Delete the **commented-out Romulan 1/2 and Core servers**? They are recoverable from `e286939`. | **Yes**, unless they're coming back soon. | Phase 2 |
| D8 | `StcMenu.xml` placeholders (`RepoId`, `FriendlyName`, `Author`, `Tooltip`, `Description`, `Commit`) and `README.md` (still the template README) need **your values**. With D2 these are now **required**: the PluginHub validator rejects an empty `FriendlyName`/`Author` or a non-hex `Commit`. | I fill `RepoId` = `STContinuum3/stc-launcher` (public repo, confirmed). You supply the rest, or we leave TODOs until you submit. | Phase 6 |

D4–D7 were not answered, so the recommendation in each row applies (D4 defaults to **keep Linux**, i.e. no change). Say so if you want otherwise.

---

## 1a. Shipping the video through PluginHub (D2)

### What Pulsar actually does
Verified in the decompiled Pulsar loader: `Pulsar.Shared.Assets`, `Pulsar.Shared.Data`, `Pulsar.Compiler`, `Pulsar.Legacy.Loader`.

| Fact | Evidence |
|---|---|
| Source builds contain **no embedded resources**. Only `*.cs` from `SourceDirectories` are compiled, and `Emit()` gets no manifest resources. So `EmbeddedResource` in the csproj never reaches PluginHub users. | `RoslynCompiler.cs:57-62`, `GitHubPlugin.cs:235` |
| Pulsar ships non-code files via `<Asset Name=… Path=…/>` (repo-relative) or `<Asset Name=… Url=… Sha256=…/>`. For GitHub/PluginHub plugins the path is read from the downloaded commit zip and copied to `%AppData%\Pulsar\GitHub\<plugin>\Assets\<Name>\…`. Folder assets keep file names. | `AssetResolver.ResolveGithubAsset/ResolveGithubFolder`, `PluginCache.AssetsDirectory` |
| **The legacy `<AssetFolder>…</AssetFolder>` element is no longer parsed.** Only an `<Asset>` whose `Name` is the reserved `"AssetFolder"` triggers `Plugin.LoadAssets(string folder)`. The commented hint at `StcMenu.xml:32-33` is stale. | `GitHubPlugin.Assets` is `[XmlElement("Asset")]`; `PluginAsset.ReservedAssetFolder`; `PluginInstance.cs:121-131` |
| `LoadAssets(string)` is called in `Instantiate()`, **before** `Init()`. | `PluginInstance.cs:60-72` |
| Pulsar dev-folder plugins (the "Test" profile) resolve the same `Path` in place inside the repo, so they exercise exactly the PluginHub code path. | `LocalFolderPlugin.GetAssembly`, `AssetResolver.ResolveDevfolderAsset` |
| A plain local DLL (`Pulsar\Legacy\Local\StcMenu.dll`, what `Deploy.bat` produces) gets assets only from a sidecar `StcMenu.xml` next to it. That sidecar's `<Id>` would replace the DLL's Id and collide with the PluginHub plugin's GUID. | `LocalPlugin.TryLoadDataFile`, `LocalPlugin.GetAssembly` |
| Pulsar compiles with **C# 14**, which matches `LangVersion 14`. Primary constructors, `??=` and collection expressions are all fine. | `RoslynCompiler.cs:77` |
| The repo `STContinuum3/stc-launcher` is **public**. The video (78.5 MB) is under GitHub's 100 MB file limit. Every PluginHub install or plugin update downloads the commit zip, video included (~75 MB). | `gh repo view` |
| The `<Id>` GUID + `<RepoId>` form is accepted on PluginHub (e.g. `Aurora.xml`), so keep the existing GUID. | PluginHub registry |

### ✅ Decided design: asset only, and dev testing uses the same path as PluginHub
The owner chose "asset only, no embedding", so the dev version is tested exactly the way the PluginHub version works.
1. **`StcMenu.xml`:** add `<Asset Name="AssetFolder" Path="ClientPlugin/Resources" />`.
   - Point it at `Resources/`, not `ClientPlugin/Assets/`. A folder asset copies *every* file, and `Assets/` holds `AssetLoader.cs`.
   - Delete the stale `<AssetFolder>` comment.
2. **`Plugin.LoadAssets(string folder)` becomes the only video entry point.** It hands `folder` to `AssetLoader`, which resolves `<folder>\star_trek_background.wmv`. If the folder or file is missing, the stock backgrounds show. This **reverses C5/A8**.
3. **Remove all extraction code and the `<EmbeddedResource>`.** The DLL shrinks by 78 MB. **BUG-3, A5, A6, A7 and the Init-time extraction become moot**, because that code is deleted.
4. **Delete the user-override folder machinery:** the `Storage\StcMenu\Assets\Videos` lookup, `CreateReadmeFile`, `VideosFolderPath`/`AssetsFolderPath`, and the **"Open the custom video assets folder"** settings button (A3, A4, C4).
5. **Dev testing goes through a Pulsar development folder.** In Pulsar: Sources → *Add Development Folder* → the repo root → *Select a plugin data file? Yes* → `StcMenu.xml`. Pulsar then compiles from source and resolves the asset in place, exactly like PluginHub.
6. **Remove the MSBuild auto-deploy** (`DeployPlugin` target, `Deploy.bat`, `Deploy.sh`). A deployed `Legacy\Local\StcMenu.dll` would have no video, and loading it next to the dev folder would apply every patch twice. The MSBuild build stays as the IDE / compile check.
   - **Manual step for you:** remove or disable `%AppData%\Pulsar\Legacy\Local\StcMenu.dll` (and the stale `StcLauncher.dll` entry) in your Pulsar profile.
   - Optionally delete the old extracted `%AppData%\SpaceEngineers\Storage\StcMenu\Assets\embedded_background.wmv` (78 MB).

### Not in scope (you do this)
Submitting or updating the PluginHub registry PR (`StarCpt/PluginHub`, `Plugins/StcMenu.xml` with the release `<Commit>`). That's an outward-facing action on your account.

---

## 2. Phase 1: Remove unused template scaffolding (zero behavior change)

One commit. Everything here is recoverable from template commit `696afb0`.

| ID | Remove | Evidence | ~Lines |
|---|---|---|---|
| T1–T4 | **The whole `ClientPlugin/Tools/` folder:** `TranspilerHelpers.cs`, `PreloaderHelpers.cs`, `Hashing.cs`, `GameAssembliesToPublicize.cs`, `IgnoresAccessChecksToAttribute.cs` | No call sites outside `Tools/`. No `using ClientPlugin.Tools` anywhere. The only transpiler user (`ExampleTranspilerPatch`) was deleted in `1046f3e`. The two publicizer files are comment-only. Verified: `Tools.Tools.GetLabelOrDefault` in Settings resolves to `ClientPlugin.Settings.Tools`, so it is unaffected. | 440 |
| T2 | `ClientPlugin/Preloader.cs` | Entirely inside `#if HAS_PRELOADER_PATCHES`, which is never defined, so it compiles to nothing in both MSBuild and Pulsar. | 40 |
| T5 | `Mono.Cecil` PackageReference (`ClientPlugin.csproj:270`) | Used only by the files above. Harmony 2.4.2 bundles its own private Cecil, has no package dependency on it, and exposes no assembly reference to it. | 1 |
| T7 | Commented-out Krafs.Publicizer blocks (`ClientPlugin.csproj:271-293`) | XML comments. Publicizer not in use (private access is via Harmony `___field` injection). | 22 |
| C11 | `ClientPlugin/App.config` | Byte-identical to the template. A DLL's config is never read at runtime, and `StcMenu.dll.config` isn't deployed anyway. For net48 it is passed to RAR, but only affects unification warnings. **Check:** net48 build warning list identical before/after. | 182 |
| S1, S2 | `Settings/Elements/Slider.cs`, `Settings/Elements/Separator.cs` | No `[Slider]` / `[Separator]` usage anywhere. Elements are only discovered via reflection over `Config`. | 160 |
| S3 | `Control.RightMargin` (+ its uses in `Layouts/Simple.cs`) | Only Slider and Separator set it, so it is always 0 after S1/S2. | 5 |
| — | `README.md:89-95` (publicizer section), `.gitignore:367-368` (`*.il`, only for TranspilerHelpers dumps) | Stale after T1–T7. | 9 |

---

## 3. Phase 2: Remove dead code in the custom STC code (zero behavior change)

| ID | Change | Where | Evidence | ~Lines |
|---|---|---|---|---|
| A1, A2 | Delete `GetRandomVideoFile()` and `ValidateVideoFile()` | `Assets/AssetLoader.cs:129-147`, `:199-214` | Zero callers (repo-wide `git grep`, no reflection/string refs). They duplicate the game's own random pick and `File.Exists` check. | 35 |
| N1 | Delete `IsValidServerAddress()`, `TestServerConnection()` (a stub that always reports "reachable"), and `using System.Net` | `Networking/ServerConnector.cs:104-136`, `:6` | Zero live callers. No ping feature exists. | 35 |
| ~~C5, A8~~ | ~~Delete `Plugin.LoadAssets(string folder)`~~. **Withdrawn by D2:** this method becomes the PluginHub video entry point (§1a). It is reworked in Phase 4 (5.4, A-HUB), not deleted. | — | — | 0 |
| C6 | Delete `Plugin.Instance` and use the `settingsGenerator` field directly | `Plugin.cs:20,26,33,60,71-72` | Only read inside `Plugin.cs`. Pulsar never reflects on it, and calls `OpenConfigDialog` on the same instance it created. | 3 |
| P5 | Drop always-true or always-false guards: `videos == null`, `___m_videos == null`, `___m_videoID != uint.MaxValue &&`, `MyAudio.Static?.` → `MyAudio.Static.` | `Patches/MainMenuBackgroundPatch.cs:53,63,70,90,115` | AssetLoader never returns null. `MainMenuBackgroundVideos` is set before any screen exists. `IsVideoValid` is a safe dictionary lookup that the game calls unguarded. `MyAudio.Static` is a null-object (`MyNullAudio`/`MyXAudio2` with `m_canPlay=false`) even without a sound device, and the game dereferences it unguarded in the same update. | ~0 (shorter expressions) |
| N2 | `MyGuiSandbox.CreateScreen(typeof(MyGuiScreenServerConnect))` → `new MyGuiScreenServerConnect()`, and drop the null check | `ServerConnector.cs:28-35` | The non-generic `CreateScreen(Type)` is plain `Activator.CreateInstance` and can't return null. Only the generic overload checks for type substitution. The game itself uses `new`. | 7 |
| N3 | Remove the **outer** try/catch in `ConnectToServer` and keep the two inner catches inside the `Invoke` callbacks | `ServerConnector.cs:17-18,83-88` | It only covers one log line plus queueing an action. The dialog-level catch is a strict superset (verified, see §9). | 6 |
| A7 | Remove dead branches and debug logging in extraction: the `stream != null` else-branch, the duplicate `File.Exists`, and the resource-name dump / size / extension log lines | `AssetLoader.cs:77,80-84,232-235,258,265-268` | A manifest stream opened by a name from `GetManifestResourceNames()` can't be null. | 20 |
| S6 | Delete `Binding.IsPressed/HasPressed/AreModifiersMatch/ToString` | `Settings/Tools/Binding.cs:20-39` | No callers. Nothing polls the keybind. *Moot if D1 = yes (file deleted).* | 20 |
| S7 | Delete `DropdownAttribute.VisibleRows` | `Settings/Elements/Dropdown.cs:10,27,29` | Assigned, never read. *Moot if D1 = yes.* | 3 |
| S8 | Delete the Color alpha path (`HasAlpha`, the Rgba regex and helpers), the redundant `color != PropertyGetter()` check, and the wrapper locals | `Settings/Elements/Color.cs`, `Settings/Tools/Tools.cs:32,39-42,61-76` | No `hasAlpha: true` anywhere. *Moot if D1 = yes.* | 30 |
| S9 | Delete the mouse branch in Keybind | `Settings/Elements/Keybind.cs:97-107,112-114` | `ControlButtonData` is only ever built with `Keyboard`. *Moot if D1 = yes.* | 15 |
| G6 | Delete the commented-out servers (per D7) | `GUI/ServerSelectionDialog.cs:21-23` | — | 3 |
| X11 | Remove unused usings: `MainMenuPatch.cs:4` (`ClientPlugin`), `:8` (`VRage.Game`), and `ServerConnector.cs:8` (`VRage.Game`) | — | Checked type-by-type against the decompiled namespaces. **Do NOT remove `using System.Reflection;` from `Plugin.cs`**: the Pulsar-only `#if !LOCAL_BUILD` version attributes need it. | 3 |

---

## 4. Phase 3: Remove the six no-op settings (D1 decided: yes)

Visible change: the settings dialog shows only **Enable custom video**, **Dev mode**, and **Open server selection**. The "Open custom video assets folder" button goes in Phase 4 (A3/A4/C4).

| ID | Change | ~Lines |
|---|---|---|
| C1 | Delete from `Config.cs`: the 6 properties + backing fields, the `ServerDisplayMode` enum, and the now-unused usings (`Settings.Tools`, `VRage.Input`, `VRageMath` if unused). | 57 |
| C2 | Convert the remaining options to auto-properties (`[Checkbox(...)] public bool DevMode { get; set; }`). Delete `INotifyPropertyChanged`, `PropertyChanged`, `OnPropertyChanged`, `SetField`. No subscribers exist. XML element names and reflection order are unchanged. | 30 |
| C3, S11 | Delete `Config.Default`. `ConfigStorage.Load` returns `new Config()` in its three fallbacks. This also removes a latent trap: `Default` must currently be declared above `Current`, or `Current` becomes null. | 4 |
| S10 | Delete the element types that lose their last user: `Settings/Elements/Textbox.cs`, `Dropdown.cs`, `Color.cs`, `Keybind.cs`, `Settings/Tools/Binding.cs`, plus the color helpers and regex fields in `Settings/Tools/Tools.cs:31-76` (only `GetLabelOrDefault` remains). Then remove `Control.Offset` / `FillFactor` and the fill-factor math in `Simple` if nothing else uses them (re-grep after deleting). | ~450 |

---

## 5. Phase 4: Simplify (behavior-preserving refactors)

Each row is its own commit, with an in-game check.

### 5.1 Settings framework

| ID | Change | Evidence it's equivalent | ~Lines |
|---|---|---|---|
| S4 | **Collapse the layout abstraction.** Delete `Layouts/Layout.cs`, `Layouts/None.cs`, `SetLayout<T>`, `RefreshLayout`, `SettingsScreen.UpdateSize`, and the unused `position` constructor parameter. Make `Simple` a plain class or static builder. Construct the screen at `(0.5, 0.7)` directly. `OpenConfigDialog` becomes `MyGuiSandbox.AddScreen(settingsGenerator.Dialog)`, and `using ClientPlugin.Settings.Layouts` goes away. | The dialog is only ever shown after `SetLayout<Simple>()`, so `None`'s methods never run. In `MyGuiScreenBase`, the `Size` setter only stores the value, and the close-button position is recomputed by the `CloseButtonEnabled` setter, which `SettingsScreen`'s constructor already calls. Building at the final size gives the same close-button position, so the `CloseButtonEnabled = CloseButtonEnabled` hack can go too. | ~90 |
| S5 | **Simplify `SettingsGenerator.ExtractAttributes`.** Iterate `GetProperties()` then `GetMethods()` (same order as today). Create an `Action` delegate only for methods that carry `[Button]`, instead of for every public method including `ToString`, `GetType` and accessors. Keep the type-validation throw. Drop `AttributeInfo`, the `controls` field indirection, and `using System.Linq.Expressions`. | Row order today is all properties, then all methods (not interleaved). `Delegate.CreateDelegate(typeof(Action), null, m)` produces the same `System.Action`. The verifier ran it on both net48 and net10. Also removes a trap: adding a public generic method to Config would currently crash Init. | ~45 |

### 5.2 Server dialogs (`GUI/`)

| ID | Change | Evidence | ~Lines |
|---|---|---|---|
| G2 | **Single source of truth for servers.** New `Networking/StcServers.cs` holds `ServerInfo` (moved out of `ServerSelectionDialog.cs`; use a class with a primary constructor, not a `record`, since net48 has no `IsExternalInit`) and `static class StcServers { const string Host = "142.127.79.145"; Lobby; Live[]; Dev[]; }`. `ServerConnector` no longer needs `using ClientPlugin.GUI`, which removes the GUI↔Networking cycle. | The IP literal appears 10 times across the two dialogs. | ~10 net |
| G1, G4, G5 | **Shared abstract base `ServerDialogBase`** for both dialogs. It holds the constructor flags, layout constants, `AddLabel`, `AddServerButton`, `AddServerGrid(servers, top, width, columns, columnSpacing)`, `AddCloseButton`, and `OnServerClick`. `GetFriendlyName() => GetType().Name`. Subclasses keep only size, colour scale (0.8 vs 0.9), tooltip lines, labels, and the dev log line. Drop arguments equal to game defaults: `position (0.5,0.5)`, `m_closeOnEsc = true`, `CanBeHidden = true`, button `visualStyle: Default`, `textScale: 0.8f`. Pass the tooltip via the constructor and use `GetFullAddress()` instead of rebuilding `$"{Address}:{Port}"`. **Keep the dialog-level try/catch** (§9). | The verifier recomputed every button position from the current code and from the proposed grid formula, and they are identical. Live: (±0.12, −0.055/0.03), (−0.12, 0.115), Close at 0.22. Dev: x=0, y=−0.09+i·0.085, Close at 0.27. Defaults checked in `MyGuiScreenBase.cs:85,234,397` and `MyGuiControlButton.cs:810`. Keep `EnabledBackgroundFade`, `m_drawEvenWithoutFocus`, `CanHideOthers=false`, `CloseButtonEnabled`: those are *not* defaults. | ~190 |

### 5.3 Patches

| ID | Change | Evidence | ~Lines |
|---|---|---|---|
| P2 | Delete the per-call debug logging in `MainMenuPatch`: lines 21, 25, 31, 37 (once per button), 44, 57, 73, 78, 97, 102, 115, 119, 131, 135. Keep the catch's error log. | Runs on every main-menu *and* pause-menu build. Line 102 logs a false "New Game not found" warning every time the pause menu opens. | 14 |
| P3 | Merge the two copy-pasted `MyGuiControlButton` constructions into a local `Add(text, tip, color, onClick)` helper. Merge the two identical click handlers. Use `Color.DarkOrange` for `new Color(255,140,0)`. Make the class `static`. | Size ×(0.9,1), textScale ×0.9, visualStyle and 0.003 spacing are identical for both buttons. `Color.DarkOrange` packs to the same 0xFF008CFF. | ~35 |
| P6 | `HideNewsAndDlcPatch`: body → `private static bool Prefix() => false;`. Keep it as a separate class (§9). | — | 4 |

### 5.4 Networking and assets

| ID | Change | Evidence | ~Lines |
|---|---|---|---|
| N4 | Pull the fill-and-click code into `FillAndConnect(screen, server)`. Use `GetFullAddress()`. Cut the 5 Info logs per click to 1 (keep the 2 warnings). Fix the misleading comments ("ensure main thread", "wait a frame for the screen to initialize"). **Keep both `Invoke` hops** and add a comment explaining they keep the connect screen loaded before the fill-and-click. | Removing a hop changes frame timing, so it isn't cleanup. | ~15 |
| A-HUB (D2) | **Ship the video as a Pulsar asset.** <br>• `StcMenu.xml`: add `<Asset Name="AssetFolder" Path="ClientPlugin/Resources" />`. <br>• `Plugin.LoadAssets(string folder)` → `AssetLoader.SetAssetFolder(folder)`. <br>• `AssetLoader.GetVideoPath()` resolves in this order: (1) `<assetFolder>\star_trek_background.wmv` if it exists; (2) otherwise the extracted embedded resource; (3) otherwise null. <br>• `GetCustomVideoFiles()` returns `[path]` or `[]`. <br>• Keep the eager `AssetLoader.LoadAssets()` call in `Init` (§9), which now just resolves. Pulsar calls `LoadAssets(string)` before `Init`. | This behavior change is required by D2. MSBuild DLL behavior is unchanged: there is no asset folder, so it takes the embedded path. | ~+10 |
| A5, A6 | AssetLoader: build the extraction path inside the extraction try block. Remove the three-way `InitializePaths` setup and the broken `STCLauncher_Assets` fallback, which never creates its directory, so `File.Create` throws anyway. Cache the resolved path with `videoPath ??= Resolve()` (null isn't cached, so failures still retry). This stops ~7 log lines plus a resource scan on every menu load. | Same end state on failure (stock backgrounds). | ~60 |
| A3, A4, C4 | **Applies (D2 decided):** delete the user-override `Storage\StcMenu\Assets\Videos` fallback, `CreateReadmeFile` (its text is wrong anyway), `VideosFolderPath` / `AssetsFolderPath`, and the **`Config.OpenAssetsFolder`** settings button. | Neither shipping path uses the folder. PluginHub and dev-folder builds use the Pulsar asset, and the MSBuild DLL uses the embedded copy. The override never worked, because the embedded video always won. **Visible change:** one fewer settings button. | ~100 |

### 5.5 Cross-cutting

| ID | Change | ~Lines |
|---|---|---|
| X9 | `internal static class Log { Info / Warning / Error }` with one prefix constant (per D6). Replaces 64 hand-typed `"STCLauncher: "` prefixes and the 5 fully-qualified `VRage.Utils.MyLog.Default.WriteLine` calls in `Plugin.cs`. | ~20 |
| C8 | Drop `[MethodImpl(NoInlining)]` on `Init` **and** switch `Assembly.GetExecutingAssembly()` → `typeof(Plugin).Assembly`. They must go together: inlined, `GetExecutingAssembly` would return the caller's assembly. Add a comment that `using System.Reflection` is needed for the Pulsar build. | 1 |
| C9, X12 | Stale comments and noise, all in one commit: <br>• `Plugin.cs` template TODO (57); obsolete `Update()` comment (65); `$"..."` strings with nothing interpolated; hard-coded `"v1.0"`. <br>• `AssetLoader.cs`: "temp location" (239), "without conversion" (86), "Validate video file" (79). <br>• `MainMenuBackgroundPatch.cs:86`: says "player's music volume" but the volume is boosted 1.5×. <br>• **Factually wrong ordering comment** at `MainMenuBackgroundPatch.cs:22-25` / `Plugin.cs:45-47`. `SetupPerGameSettings` runs *before* plugin Init. The real reason is that the menu's `CreateBackgroundScreen` captures the stock array during `Initialize()`. <br>• Settings title `"STC Launcher Settings"` → match the new name (per D6). | ~15 |

---

## 6. Phase 5: Bug fixes (behavior changes, flagged)

| ID | Bug | Fix | Evidence |
|---|---|---|---|
| BUG-1 | **The STC buttons disappear** after changing resolution, language, controller or mouse/keyboard options, or when the user/cloud user changes. | Remove `if (!constructor) return;` and the `bool constructor` parameter (`MainMenuPatch.cs:23-27`). | Those paths call `RecreateControls(false)` (`MySandboxGame.cs:655,667,2232`; `MyGuiScreenOptionsGame.cs:774`; `...Controller.cs:1080`; `...MouseKeyboard.cs:948`). `MyGuiScreenMainMenu.RecreateControls` calls base first, which runs `Controls.Clear()` (`MyGuiScreenBase.cs:1419-1421`). So the guard never prevented duplicates, it only skips re-adding. |
| BUG-2 | **No STC buttons on non-English clients.** The patch matches the label text "New Game". German shows "Neues Spiel", French "Nouvelle partie", and so on. `"NEW GAME"` and `"New World"` never match in any language. | `if (__instance.Controls.GetControlByName("NewGame") is not MyGuiControlButton newGame) return;` replaces the loop (−17 lines). | `MakeButton` sets `Name = name` (`MyGuiScreenMainMenuBase.cs:309`). `"NewGame"` is unique in the game (`MyGuiScreenMainMenu.cs:306`) and present with or without Continue/QuickStart. The in-game menu has none, so the pause menu is unchanged. Pulsar's Plugins button has no name. |
| BUG-3 | **A crash or full disk mid-extraction leaves a truncated video forever.** The partial file is newer than the DLL, so it is never re-extracted. | Extract to `embedded_background.wmv.tmp`, then move over the final file. Optionally also re-extract when the file length ≠ resource length, which also avoids a 78 MB rewrite on every rebuild. | `AssetLoader.cs:247-248, 259`. |
| BUG-4 | **Version bumps in `Version.Build.props` never reach the DLL.** `ClientPlugin.csproj:15-17` sets `AssemblyVersion`/`FileVersion` *after* `Directory.Build.props` is imported, so the csproj wins. The comment there claims the opposite. | Delete csproj lines 15-17. Add a note in `README`/`Plugin.cs` that the Pulsar-build version in `Plugin.cs:9-12` is a second copy to bump. | SDK import order (`Sdk.props:49` → `Microsoft.Common.props:34`). Generated `obj/**/ClientPlugin.AssemblyInfo.cs` contains `"1.0.0.0"`; if `Version.Build.props` had won it would be `"1.0.0"`. |
| BUG-5 *(optional)* | **Bin64 auto-detect fails under VS 18's 32-bit MSBuild.** The Steam uninstall key exists only in the 64-bit registry view. Builds work today only because `Directory.Build.props.user` exists. | `csproj:24`: `$([MSBuild]::GetRegistryValueFromView('HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\Steam App 244850', 'InstallLocation', null, RegistryView.Registry64, RegistryView.Registry32))`. | Verified the registry views on this machine. |

---

## 7. Phase 6: Build and repo infrastructure

| ID | Change | Caveat | ~Lines |
|---|---|---|---|
| B2 | Remove csproj properties that equal SDK defaults: `OutputType`, `RootNamespace` (= project name `ClientPlugin`), `GenerateAssemblyInfo`, `Configuration`/`Platform` defaults, `ErrorReport`, `DebugSymbols`, `Optimize`, `PULSAR` define (no `#if PULSAR`). Merge the two per-config groups into unconditional `PlatformTarget x64` + `<DefineConstants>$(DefineConstants);LOCAL_BUILD</DefineConstants>`. Keep `DebugType` per config. | Must use `$(DefineConstants);…`, because `TRACE` is added *before* the project body. **Keep** `WarningLevel 4` (net10 default is 10, which would add warnings), `OutputPath` (the explicit `/` matters for `Deploy.sh` if D4 keeps Linux), and `RunPostBuildEvent`. | ~20 |
| B4 | Replace `verify_props.bat` + `verify_props.sh` with `<Error Condition="!Exists('$(Bin64)')" Text="Invalid Bin64 path '$(Bin64)' in Directory.Build.props[.user]" />` in the `VerifyProps` target. | Same failure condition, no shell per target framework. Update `StcMenu.sln:9-10` Solution Items. | ~38 |
| B7 | Delete `setup.py`. Remove its mentions in `README.md` and `Directory.Build.props:14-15`. | The template-rename half is spent. **But** the `.user` file it writes is load-bearing on this machine until BUG-5 is fixed. Keep your existing `Directory.Build.props.user`, and document creating it by hand. | ~335 |
| B8 | Delete `Clean.bat` / `clean.sh` and their `StcMenu.sln:11,15` entries. | They wipe all of bin/obj, which differs slightly from `dotnet clean`. Fine to drop. Also `Clean.bat` has no `%~dp0`, so it is CWD-dependent. | 10 |
| B9 | Delete `.run/Vanilla.run.xml` (path resolves to a non-existent `H:\Program Files (x86)\…`, launches SE without Pulsar) and `ClientPluginTemplate.sln.DotSettings` (name doesn't match `StcMenu.sln`, so it never loads). | — | 18 |
| B5 | Deploy scripts: minimal change only. Pass `$(TargetPath)` instead of hand-building the path from `$(PathSeparator)` + `$(OutputPath)`. Leave the scripts otherwise. | A full inline-MSBuild rewrite is possible, but it's churn with medium risk. Not recommended in this pass. | ~5 |
| B10 | AI-tool pointer files (`.clinerules`, `.github/copilot-instructions.md`, `.vscode/AGENTS.md`): keep only the ones for tools you use. Add one line of STC context to `AGENTS.md`. | Needs your input. | ≤3 |
| B11 | Replace the template `README.md` with a ~30-line project README: purpose, build (`MSBuild … -p:Configuration=Release`), deploy path, `-p:RunPostBuildEvent=Never` for dry builds, Bin64 `.user` override, the two places to bump the version, and the D2 video caveat. Delete `Docs/ConfigDialogExample.png` if it's only used there. | Needs D8 input. | ~−95 |
| B12 | `StcMenu.xml`: fill the TODOs (D8, now required for PluginHub). Delete the ~40 lines of template comment blocks, including the stale `<AssetFolder>` hint, which current Pulsar ignores. Keep the `<Asset Name="AssetFolder" …/>` line added by A-HUB. | `<Commit>` must be the release commit you submit to PluginHub. | ~40 |
| B13 | `.gitignore`: drop the stale `/Bin64/`, `/Torch/`, `*.il` lines. Leave the rest. | Cosmetic. | 3 |
| B14 | **Note only:** the 78 MB `.wmv` is a plain git blob, not LFS. No history rewrite. Decide on LFS before the next video re-encode, but check first that Pulsar's source download handles LFS pointers. | — | 0 |

---

## 8. Verification per phase

**Build** (every phase), with `"C:\Program Files\Microsoft Visual Studio\18\Community\MSBuild\Current\Bin\MSBuild.exe" StcMenu.sln -p:Configuration=Release`:
- both `net48` and `net10.0` compile;
- the warning list does not grow (capture a baseline before Phase 1);
- for Phases 1–2, optionally diff the public API surface of `StcMenu.dll`.

**In-game smoke test** (Phases 2–5), with the DLL deployed via Pulsar Legacy:
1. The main menu shows **STC Servers**. With Dev mode on (and a menu rebuild), **STC Dev Servers** also shows.
2. The STC Servers dialog: title, lobby button, 2-column grid (Deep Space alone bottom-left, as today), tooltips, Close and X buttons, Esc closes it.
3. The Dev dialog: 1-column list, warning label.
4. Clicking a server opens Direct Connect with the address filled in and connects.
5. Background video plays with the boosted soundtrack. Stock menu music is muted. Turning Enable custom video off restores the stock background and music.
6. News and DLC panels are hidden in both the main menu and the pause menu.
7. Pulsar → plugin config opens the settings dialog, with the same size and close-button position. Toggling Dev mode persists across restart via `StcMenu.cfg`.
8. The pause menu shows no STC buttons, and **no "New Game not found" warning in the log**.
9. After BUG-1/BUG-2: change resolution → the STC buttons are still there. Switch the game language to German → the STC buttons appear.
10. After BUG-3: delete `Storage\StcMenu\Assets\embedded_background.wmv` → it is re-extracted on the next launch.
11. **PluginHub path (A-HUB), via a Pulsar dev-folder profile pointing at the repo:**
    - Pulsar compiles the plugin from source. The log shows `Loading assets for StcMenu`.
    - The background video plays from `<repo>\ClientPlugin\Resources\star_trek_background.wmv`.
    - Nothing is extracted to `Storage\StcMenu\Assets`.
    - The settings dialog shows only Enable custom video, Dev mode and Open server selection.
12. **Local DLL path:** deploy with `Deploy.bat` → the video still plays via the embedded fallback.

---

## 9. Considered and rejected (do NOT do these)

| Proposal | Why rejected |
|---|---|
| Delete `Plugin.LoadAssets(string)` as dead (original C5/A8) | Withdrawn after D2. It is Pulsar's entry point for the PluginHub video asset (§1a). |
| Ship the video on PluginHub via a `Url=` asset (GitHub Release) instead of a repo `Path=` asset | The video is already in the repo, so the commit zip carries it regardless, and a `Url` adds a release artifact plus a Sha256 to maintain. Revisit only if the video is ever removed from the repo. |
| Remove the `AssetLoader.LoadAssets()` call from `Plugin.Init` and extract lazily on the first menu load (C7) | `Init` runs before `RunLoop`, and the menu's `LoadContent` runs on the first update frame with no loading screen covering it. Lazy extraction would move a one-time 78 MB write into a visible frame. Keep the eager call, and add caching (A6). |
| Remove the dialog-level try/catch around `ConnectToServer` (G3) | Its catch covers a strict superset of the connector's outer catch, and `MyScreenManager.HandleInput` has no catch of its own. Remove the connector's outer catch instead (N3). |
| Replace the Direct Connect screen automation with `MyJoinGameHelper.JoinServer(address)` | Not equivalent: it unloads to the menu (restarting the video), shows no progress or cancel screen and no "server not responding" message, and skips `SaveLastSessionInfo`. |
| Collapse the two nested `MySandboxGame.Static.Invoke` hops | The only ordering that guarantees the connect screen is loaded before the fill-and-click. Changing frame timing isn't cleanup. |
| Remove the `#if !LOCAL_BUILD [assembly: AssemblyVersion]` block or `using System.Reflection` in `Plugin.cs` | Required for Pulsar source builds. Pulsar compiles without MSBuild or `LOCAL_BUILD` and reads `assembly.GetName().Version`. Removing it silently breaks Pulsar builds while local builds still pass. |
| Remove `WarningLevel 4` from the csproj | The net10 SDK default is 10, which would add warnings. |
| Prune the 22 unused game DLL references (D5) | No benefit for Pulsar (it ignores the csproj). CS0012 risk from transitive type exposure. Contradicts se-dev plugin guidance. |
| Merge `HideNewsAndDlcPatch` into `MainMenuPatch` | Harmony allows it, but `PatchWithAttributes` resolves all targets in a class before patching. If a game update renames the private `CreateRightSection`, the merged class would also lose the STC-buttons postfix. Separate classes isolate that. |
| Remove the `Config.OpenServerSelection` settings button as a duplicate of the main-menu button | It's a fallback entry point if `MainMenuPatch` fails after a game update. Removal is a visible change for no line savings. |
| Put the lobby button into the server grid loop | Its gap to the grid (0.13) differs from the row pitch (0.085). Looping would move it. |
| Rewrite git history to drop or LFS the video | Out of scope, and destructive for an already-pushed branch. |

---

## 10. Latent issues noted but not in scope

- `[Button]` settings methods are bound to a delegate with `this == null`. That's fine today because neither uses instance state. It would NRE if a future button did. Worth a one-line comment in `SettingsGenerator` after S5.
- If `new SettingsGenerator()` throws in `Init`, the catch swallows it and `OpenConfigDialog` later throws an NRE.
- Turning on Dev mode doesn't show the Dev button until the main menu is rebuilt. BUG-1's fix makes a resolution or language change rebuild it, but a settings toggle alone still won't.
- The live server dialog's 5th server (Deep Space) sits alone in the left column since Romulan and Core were commented out. The refactor preserves this. Center it only if you want a visual change.
- 78 MB video extraction happens at `Init` even when **Enable custom video** is off. Guarding it on the setting would be a small behavior change, so it's not included.
