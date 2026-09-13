using Sandbox;
using Sandbox.Graphics.GUI;
using System;
using System.Collections.Generic;
using VRageMath;

namespace ClientPlugin.Settings;

internal class SettingsScreen : MyGuiScreenBase
{
    public readonly string FriendlyName;
    public Func<List<MyGuiControlBase>> GetControls;

    public override string GetFriendlyName() => FriendlyName;

    public SettingsScreen(string friendlyName, Func<List<MyGuiControlBase>> getControls, Vector2 size) : base(
        new Vector2(0.5f, 0.5f),
        MyGuiConstants.SCREEN_BACKGROUND_COLOR,
        size,
        false,
        null,
        MySandboxGame.Config.UIBkOpacity,
        MySandboxGame.Config.UIOpacity)
    {
        FriendlyName = friendlyName;
        GetControls = getControls;

        EnabledBackgroundFade = true;
        m_drawEvenWithoutFocus = true;
        CanHideOthers = true;
        CloseButtonEnabled = true;
    }

    public override void LoadContent()
    {
        base.LoadContent();
        RecreateControls(true);
    }

    public override void OnRemoved()
    {
        ConfigStorage.Save(Config.Current);
        base.OnRemoved();
    }

    public override void RecreateControls(bool constructor)
    {
        base.RecreateControls(constructor);
        AddCaption(Name);

        foreach (var item in GetControls())
        {
            Controls.Add(item);
        }
    }
}
