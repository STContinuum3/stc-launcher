using HarmonyLib;
using Sandbox.Game.Screens.Helpers;
using Sandbox.Graphics.GUI;
using SpaceEngineers.Game.GUI;

namespace ClientPlugin.Patches;

/// <summary>
/// Removes the news panel and the DLC banners from the main menu, keeping the Newsletter button.
///
/// MyGuiScreenMainMenu.CreateRightSection adds the MyGuiControlNews panel, the
/// MyGuiControlDLCBanners strip and, on Steam (MyPlatformGameSettings.SHOW_NEWSLETTER_MENU_BUTTON),
/// the Newsletter button. That button is the player's route to the newsletter reward blocks, so
/// the method is left to run and only the two panels are taken out afterwards. The button's
/// position is computed from the banner before this postfix runs, so it does not move.
///
/// The panels are removed and their fields nulled rather than hidden. The banner sets itself
/// visible again when its promotions request completes, and a hidden news panel still drives
/// the gamepad help text and opens its link on the gamepad X button. With the fields null,
/// LoadContent skips the banner request and OpenUserRelatedScreens skips the news download;
/// every other reference to them in MyGuiScreenMainMenu is null guarded. Re-verify that if the
/// game updates. The news constructor starts one download of its own, which a postfix cannot
/// prevent.
///
/// Approach adapted from the No News Plugin by WesternGamer
/// (https://github.com/WesternGamer/No-News-Plugin), originally written by austinvaness.
/// </summary>
[HarmonyPatch(typeof(MyGuiScreenMainMenu), "CreateRightSection")]
internal static class HideNewsAndDlcPatch
{
    private static void Postfix(MyGuiScreenMainMenu __instance,
        ref MyGuiControlNews ___m_newsControl, ref MyGuiControlDLCBanners ___m_dlcBannersControl)
    {
        Remove(__instance, ref ___m_newsControl);
        Remove(__instance, ref ___m_dlcBannersControl);
    }

    private static void Remove<T>(MyGuiScreenMainMenu menu, ref T control) where T : MyGuiControlBase
    {
        if (control == null)
            return;

        menu.Controls.Remove(control);
        control = null;
    }
}
