using Sandbox.Graphics.GUI;
using System;
using System.Collections.Generic;
using System.Linq;
using ClientPlugin.Settings.Elements;
using VRage.Utils;
using VRageMath;

namespace ClientPlugin.Settings.Layouts;

internal class Simple
{
    public static readonly Vector2 SettingsPanelSize = new Vector2(0.5f, 0.7f);
    private const float ElementPadding = 0.01f;

    private readonly Func<List<List<Control>>> getControls;
    private List<List<Control>> controls;
    private MyGuiControlParent parent;
    private MyGuiControlScrollablePanel scrollPanel;

    public Simple(Func<List<List<Control>>> getControls)
    {
        this.getControls = getControls;
    }

    /// <summary>
    /// Builds a fresh set of controls inside a scroll panel and lays them out.
    /// </summary>
    /// <returns>Controls to be parented to the screen.</returns>
    public List<MyGuiControlBase> RecreateControls()
    {
        controls = getControls();

        parent = new MyGuiControlParent()
        {
            OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_TOP,
            Position = Vector2.Zero, 
            Size = new Vector2(SettingsPanelSize.X-0.01f, SettingsPanelSize.Y-0.09f),
        };

        scrollPanel = new MyGuiControlScrollablePanel(parent)
        {
            BackgroundTexture = null,
            BorderHighlightEnabled = false,
            BorderEnabled = false,
            OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER,
            Position = new Vector2(0f, 0.03f), // Do not overlap the dialog's title
            Size = parent.Size,
            ScrollbarVEnabled = true,
            CanFocusChildren = true,
            ScrolledAreaPadding = new MyGuiBorderThickness(0.005f),
            DrawScrollBarSeparator = true,
        };

        foreach (var row in controls)
        {
            foreach (var control in row)
            {
                parent.Controls.Add(control.GuiControl);
            }
        }

        LayoutControls();
        return new List<MyGuiControlBase> { scrollPanel };
    }

    private void LayoutControls()
    {
        var totalHeight = ElementPadding + controls.Select(row => row.Max(c => c.GuiControl.Size.Y) + ElementPadding).Sum();
        parent.Size = new Vector2(parent.Size.X, totalHeight);
            
        var rowY = -0.5f * totalHeight + ElementPadding;
        foreach (var row in controls)
        {
            // Vertical
                
            var rowHeight = row.Max(c => c.GuiControl.Size.Y);
            var controlY = rowY + 0.5f * rowHeight;
                
            rowY += rowHeight + ElementPadding;
                
            // Horizontal

            var controlX = -0.5f * parent.Size.X + ElementPadding;
            foreach (var control in row)
            {
                var guiControl = control.GuiControl;
                guiControl.Position = new Vector2(controlX, controlY);
                guiControl.OriginAlign = control.OriginAlign;

                var sizeY = guiControl.Size.Y;
                if (control.FixedWidth.HasValue)
                {
                    guiControl.Size = new Vector2(control.FixedWidth.Value, sizeY);
                    guiControl.SetMaxWidth(control.FixedWidth.Value);
                }
                else
                {
                    guiControl.Size = new Vector2(Math.Max(guiControl.Size.X, control.MinWidth), sizeY);
                }

                controlX += guiControl.Size.X;
            }
        }
            
        scrollPanel.RefreshInternals();
    }
}