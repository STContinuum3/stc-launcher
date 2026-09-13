using Sandbox.Graphics.GUI;
using VRage.Utils;

namespace ClientPlugin.Settings.Elements;

internal class Control
{
    // FIXME: This is global and not determined automatically
    public static readonly float LabelMinWidth = 0.18f;

    public readonly MyGuiControlBase GuiControl;
    public readonly float? FixedWidth;
    public readonly float MinWidth;
    public readonly MyGuiDrawAlignEnum OriginAlign;

    public Control(MyGuiControlBase guiControl, float? fixedWidth = null, float minWidth = 0f, MyGuiDrawAlignEnum originAlign = MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_CENTER)
    {
        GuiControl = guiControl;
        FixedWidth = fixedWidth;
        MinWidth = minWidth;
        OriginAlign = originAlign;
    }
}
