using ClientPlugin.Assets;
using HarmonyLib;
using Sandbox.Game;
using Sandbox.Game.Gui;
using Sandbox.Game.Screens;
using Sandbox.Graphics.GUI;
using System;
using VRage.Audio;
using VRage.Utils;
using VRageRender;

namespace ClientPlugin.Patches;

/// <summary>
/// Replaces the main menu background with our own video, playing its soundtrack in place of
/// the stock menu track.
///
/// MyGuiScreenIntroVideo.LoadContent picks m_currentVideo out of m_videos, and TryPlayVideo
/// then hands it to the renderer with m_volume. Rewriting those in a prefix leaves every
/// other part of the job - looping, the menu overlay, transitions and cleanup - to the game.
///
/// Why here rather than assigning MyPerGameSettings.GUI.MainMenuBackgroundVideos during
/// Plugin.Init: under Pulsar the plugin is initialised BEFORE
/// SpaceEngineersGame.SetupPerGameSettings() runs, so that field is overwritten with the 13
/// stock videos immediately afterwards. Patching LoadContent has no ordering dependency.
///
/// Why not patch MyGuiScreenMainMenu.AddIntroScreen: it is a two-line private method called
/// straight from the MyGuiScreenMainMenu constructor, so the JIT inlines it and a prefix on
/// it is silently never invoked - Harmony still reports the patch as applied.
/// </summary>
[HarmonyPatch(typeof(MyGuiScreenIntroVideo))]
internal static class MainMenuBackgroundPatch
{
    /// <summary>The background screen running our video, or null when the stock one is up.</summary>
    internal static MyGuiScreenIntroVideo ActiveScreen;

    /// <summary>
    /// How much louder than the music slider to play the video's soundtrack.
    ///
    /// On Windows the video runs through DirectShow, whose IBasicAudio volume is expressed
    /// in hundredths of a decibel from -10000 (-100 dB, silent) to 0 (0 dB). VRage maps that
    /// onto 0..1 as put_Volume((value - 1) * 10000), so 1.0 IS the file's native level and
    /// the API cannot amplify past it. Boosting therefore scales the level up that curve and
    /// clamps at native, which means the slider reaches full volume early: at this multiplier
    /// anything from roughly 1/boost upwards already plays at 0 dB.
    ///
    /// Note the scale is linear in decibels, so a given multiplier is a larger change than it
    /// looks - at a 0.5 music slider, 1.5x moves -50 dB to -25 dB.
    /// </summary>
    private const float SoundtrackBoost = 1.50f;

    private static float SoundtrackVolume =>
        Math.Min(1f, MyAudio.Static.VolumeMusic * SoundtrackBoost);

    [HarmonyPrefix]
    [HarmonyPatch("LoadContent")]
    private static void LoadContentPrefix(MyGuiScreenIntroVideo __instance, ref string[] ___m_videos, ref float ___m_volume)
    {
        // Only the menu background screen is built from MyPerGameSettings' array
        // (MyGuiScreenIntroVideo.CreateBackgroundScreen passes it straight through), so this
        // reference check keeps us clear of the startup intro and credits videos, which use
        // their own lists and must not be touched.
        if (!ReferenceEquals(___m_videos, MyPerGameSettings.GUI.MainMenuBackgroundVideos))
            return;

        if (!Config.Current.EnableCustomVideo)
            return;

        var videoPath = AssetLoader.VideoPath;
        if (videoPath == null)
        {
            MyLog.Default.Warning("STCLauncher: No custom video available, keeping stock menu backgrounds");
            return;
        }

        // TryPlayVideo does Path.Combine(ContentPath, entry), which returns the entry
        // unchanged when it is already rooted - so our absolute path works.
        ___m_videos = new[] { videoPath };

        // While ShowPictures is set the screen draws static loading images and never calls
        // TryPlayVideo. The game turns it on for Steam Deck; force it off so the video plays
        // regardless of platform.
        __instance.ShowPictures = false;

        // CreateBackgroundScreen hardcodes volume 0 because stock menu videos are silent
        // wallpaper. Ours has a soundtrack, so play it at the player's music volume and keep
        // the stock menu track out of its way (see MenuMusicPatch).
        ___m_volume = SoundtrackVolume;
        ActiveScreen = __instance;
        MyAudio.Static.StopMusic();

        MyLog.Default.Info($"STCLauncher: Main menu background video replaced with {videoPath} (volume {___m_volume:0.00})");
    }

    [HarmonyPostfix]
    [HarmonyPatch("Update")]
    private static void UpdatePostfix(MyGuiScreenIntroVideo __instance, uint ___m_videoID, ref float ___m_volume)
    {
        if (!ReferenceEquals(__instance, ActiveScreen))
            return;

        // The game fades the video out through m_transitionAlpha while the screen closes.
        // Leave that alone.
        if (__instance.State == MyGuiScreenState.CLOSING)
            return;

        var volume = SoundtrackVolume;
        if (Math.Abs(volume - ___m_volume) < 0.001f)
            return;

        // Follow the music slider live, and keep m_volume current so that when the video
        // loops, Loop() -> TryPlayVideo() restarts it at the right level.
        ___m_volume = volume;

        if (MyRenderProxy.IsVideoValid(___m_videoID))
            MyRenderProxy.SetVideoVolume(___m_videoID, volume);
    }

    [HarmonyPostfix]
    [HarmonyPatch("UnloadContent")]
    private static void UnloadContentPostfix(MyGuiScreenIntroVideo __instance)
    {
        if (ReferenceEquals(__instance, ActiveScreen))
            ActiveScreen = null;
    }
}

/// <summary>
/// Stops the stock main menu track from starting on top of our video's soundtrack.
///
/// MyGuiScreenMainMenuBase.Update calls MyAudio.Static.PlayMusic(MyPerGameSettings.MainMenuTrack)
/// once, guarded by m_musicPlayed. Setting that flag up front makes it skip the call. Only
/// active while our background video is on screen, so the in-game pause menu is unaffected.
/// </summary>
[HarmonyPatch(typeof(MyGuiScreenMainMenuBase), "Update")]
internal static class MenuMusicPatch
{
    private static void Prefix(ref bool ___m_musicPlayed)
    {
        if (MainMenuBackgroundPatch.ActiveScreen != null)
            ___m_musicPlayed = true;
    }
}
