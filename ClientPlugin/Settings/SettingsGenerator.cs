using ClientPlugin.Settings.Elements;
using ClientPlugin.Settings.Layouts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace ClientPlugin.Settings;

internal class SettingsGenerator
{
    public readonly SettingsScreen Dialog;

    public SettingsGenerator()
    {
        // Each row factory creates fresh controls, so the dialog can be opened repeatedly
        var rows = ExtractRows();
        var layout = new Simple(() => rows.Select(row => row()).ToList());
        Dialog = new SettingsScreen(Config.Current.Title, layout.RecreateControls, Simple.SettingsPanelSize);
    }

    private static List<Func<List<Control>>> ExtractRows()
    {
        var rows = new List<Func<List<Control>>>();

        foreach (var propertyInfo in typeof(Config).GetProperties())
        {
            foreach (var element in propertyInfo.GetCustomAttributes().OfType<IElement>())
            {
                Validate(element, propertyInfo.PropertyType, propertyInfo.Name);
                rows.Add(() => element.GetControls(
                    propertyInfo.Name,
                    () => propertyInfo.GetValue(Config.Current),
                    value => propertyInfo.SetValue(Config.Current, value)));
            }
        }

        foreach (var methodInfo in typeof(Config).GetMethods())
        {
            foreach (var element in methodInfo.GetCustomAttributes().OfType<IElement>())
            {
                Validate(element, typeof(Delegate), methodInfo.Name);

                // Bound with a null target, so [Button] methods must not use instance state
                var action = Delegate.CreateDelegate(typeof(Action), null, methodInfo);
                rows.Add(() => element.GetControls(methodInfo.Name, () => action, null));
            }
        }

        return rows;
    }

    private static void Validate(IElement element, Type type, string name)
    {
        if (!element.SupportedTypes.Any(t => t.IsAssignableFrom(type)))
        {
            throw new Exception(
                $"Element {element.GetType().Name} for {name} expects "
                + $"{string.Join("/", element.SupportedTypes)} but "
                + $"recieved {type.FullName}");
        }
    }
}
