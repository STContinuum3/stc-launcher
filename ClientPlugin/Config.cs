using ClientPlugin.Settings;
using ClientPlugin.Settings.Elements;
using ClientPlugin.Settings.Tools;
using Sandbox.Graphics.GUI;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;
using VRage.Input;
using VRageMath;

namespace ClientPlugin;

public enum ServerDisplayMode
{
    Grid,
    List,
    Compact
}

public class Config : INotifyPropertyChanged
{
    #region Options

    // Star Trek Continuum Launcher configuration options
    private bool enableCustomVideo = true;
    private bool showServerPing = true;
    private string preferredServer = "Federation 1";
    private ServerDisplayMode serverDisplayMode = ServerDisplayMode.Grid;
    private Color factionThemeColor = Color.Blue;
    private bool autoConnectToLastServer = false;
    private Binding quickConnectKeybind = new Binding(MyKeys.F12);
    private bool devMode = false;

    #endregion

    #region User interface

    public readonly string Title = "STC Launcher Settings";

    // Star Trek Continuum Launcher settings controls

    [Checkbox(description: "Enable custom Star Trek background video")]
    public bool EnableCustomVideo
    {
        get => enableCustomVideo;
        set => SetField(ref enableCustomVideo, value);
    }

    [Checkbox(description: "Show server ping information in server selection dialog")]
    public bool ShowServerPing
    {
        get => showServerPing;
        set => SetField(ref showServerPing, value);
    }

    [Textbox(description: "Preferred server name for quick connect")]
    public string PreferredServer
    {
        get => preferredServer;
        set => SetField(ref preferredServer, value);
    }

    [Dropdown(description: "How servers are displayed in the selection dialog")]
    public ServerDisplayMode ServerDisplayMode
    {
        get => serverDisplayMode;
        set => SetField(ref serverDisplayMode, value);
    }

    [Color(description: "Theme color for faction buttons")]
    public Color FactionThemeColor
    {
        get => factionThemeColor;
        set => SetField(ref factionThemeColor, value);
    }

    [Checkbox(description: "Automatically connect to the last used server")]
    public bool AutoConnectToLastServer
    {
        get => autoConnectToLastServer;
        set => SetField(ref autoConnectToLastServer, value);
    }

    [Keybind(description: "Hotkey to quickly open server selection dialog")]
    public Binding QuickConnectKeybind
    {
        get => quickConnectKeybind;
        set => SetField(ref quickConnectKeybind, value);
    }

    [Checkbox(description: "Enable development mode to show dev server options")]
    public bool DevMode
    {
        get => devMode;
        set => SetField(ref devMode, value);
    }

    [Button(description: "Open the Star Trek Continuum server selection dialog")]
    public void OpenServerSelection()
    {
        MyGuiSandbox.AddScreen(new GUI.ServerSelectionDialog());
    }

    [Button(description: "Open the custom video assets folder")]
    public void OpenAssetsFolder()
    {
        try
        {
            var assetsPath = Assets.AssetLoader.VideosFolderPath;
            System.Diagnostics.Process.Start("explorer.exe", assetsPath);
        }
        catch (Exception ex)
        {
            MyGuiSandbox.AddScreen(MyGuiSandbox.CreateMessageBox(
                MyMessageBoxStyleEnum.Error,
                buttonType: MyMessageBoxButtonsType.OK,
                messageText: new StringBuilder($"Failed to open assets folder:\n{ex.Message}"),
                messageCaption: new StringBuilder("Error"),
                size: new Vector2(0.6f, 0.4f)
            ));
        }
    }

    #endregion

    #region Property change notification boilerplate

    public static readonly Config Default = new Config();
    public static readonly Config Current = ConfigStorage.Load();

    public event PropertyChangedEventHandler PropertyChanged;

    protected virtual void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    private bool SetField<T>(ref T field, T value, [CallerMemberName] string propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value)) return false;
        field = value;
        OnPropertyChanged(propertyName);
        return true;
    }

    #endregion
}
