using HarmonyLib;
using Sandbox.Game.Screens.Helpers;
using Sandbox.Graphics.GUI;
using SpaceEngineers.Game.GUI;

namespace ClientPlugin.Patches;

/// <summary>
/// Removes the news panel, the DLC banners and the Newsletter button from the main menu while
/// the Hide menu news setting is on.
///
/// MyGuiScreenMainMenu.CreateRightSection adds the MyGuiControlNews panel, the
/// MyGuiControlDLCBanners strip and, on Steam (MyPlatformGameSettings.SHOW_NEWSLETTER_MENU_BUTTON),
/// the Newsletter button. The method is left to run and the controls are taken out afterwards.
/// The Newsletter button is the player's route to Keen's newsletter reward blocks, which is why
/// hiding it is a setting that players can turn off.
///
/// The panels are removed and their fields nulled rather than hidden. The banner sets itself
/// visible again when its promotions request completes, and a hidden news panel still drives
/// the gamepad help text and opens its link on the gamepad X button. With the fields null,
/// LoadContent skips the banner request and OpenUserRelatedScreens skips the news download;
/// every other reference to them in MyGuiScreenMainMenu is null guarded. Re-verify that if the
/// game updates. The news constructor starts one download of its own, which a postfix cannot
/// prevent.
///
/// The Newsletter button is not kept in a field. It is found by the name MakeButton gives it and
/// also removed from m_elementGroup, the list the menu adds its buttons to.
///
/// Approach adapted from the No News Plugin by WesternGamer
/// (https://github.com/WesternGamer/No-News-Plugin), originally written by austinvaness.
/// </summary>
[HarmonyPatch(typeof(MyGuiScreenMainMenu), "CreateRightSection")]
internal static class HideMenuNewsPatch
{
    private const string NewsletterButtonName = "ShowNewsletter";

    private static void Postfix(MyGuiScreenMainMenu __instance,
        ref MyGuiControlNews ___m_newsControl, ref MyGuiControlDLCBanners ___m_dlcBannersControl,
        MyGuiControlElementGroup ___m_elementGroup)
    {
        if (!Config.Current.HideMenuNews)
            return;

        Remove(__instance, ref ___m_newsControl);
        Remove(__instance, ref ___m_dlcBannersControl);

        if (__instance.Controls.GetControlByName(NewsletterButtonName) is { } newsletterButton)
        {
            __instance.Controls.Remove(newsletterButton);
            ___m_elementGroup?.Remove(newsletterButton);
        }
    }

    /// <summary>Whether <paramref name="menu"/> was built with the news removed.</summary>
    internal static bool IsApplied(MyGuiScreenMainMenu menu) =>
        AccessTools.Field(typeof(MyGuiScreenMainMenu), "m_newsControl")?.GetValue(menu) == null;

    private static void Remove<T>(MyGuiScreenMainMenu menu, ref T control) where T : MyGuiControlBase
    {
        if (control == null)
            return;

        menu.Controls.Remove(control);
        control = null;
    }
}
