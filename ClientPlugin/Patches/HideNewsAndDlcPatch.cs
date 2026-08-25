using HarmonyLib;
using SpaceEngineers.Game.GUI;

namespace ClientPlugin.Patches;

/// <summary>
/// Removes the news panel and the DLC banners from the main menu.
///
/// MyGuiScreenMainMenu.CreateRightSection builds the entire right hand column and nothing
/// else - the MyGuiControlNews panel, the MyGuiControlDLCBanners strip, and the newsletter
/// button behind MyPlatformGameSettings.SHOW_NEWSLETTER_MENU_BUTTON - so skipping it drops
/// exactly that block and leaves the rest of the menu untouched.
///
/// Skipping it leaves m_newsControl and m_dlcBannersControl null, which is safe: every other
/// reference to them in MyGuiScreenMainMenu is null guarded, either with an explicit check or
/// the null conditional operator. Re-verify that if the game updates.
///
/// Unlike MyGuiScreenMainMenu.AddIntroScreen (see MainMenuBackgroundPatch), this method has a
/// large body and is not at risk of being inlined out from under the patch.
///
/// Approach taken from the No News Plugin by WesternGamer
/// (https://github.com/WesternGamer/No-News-Plugin), originally written by austinvaness.
/// </summary>
[HarmonyPatch(typeof(MyGuiScreenMainMenu), "CreateRightSection")]
internal static class HideNewsAndDlcPatch
{
    private static bool Prefix()
    {
        // Returning false skips the original, so none of the right hand controls are created.
        return false;
    }
}
