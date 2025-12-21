using BepInEx.Configuration;
using Silksong.ModMenu.Plugin;

namespace RecentItemsDisplay.Serialization;

internal static class ConfigSettings
{
    public static ConfigEntry<bool> DisplayEnabled = null!;
    public static ConfigEntry<int> NumDisplayableItems = null!;

    public static void Init(ConfigFile config)
    {
        DisplayEnabled = config.Bind(
            "General", "DisplayEnabled", true,
            new ConfigDescription("Whether or not to show the display"));

        NumDisplayableItems = config.Bind(
            "General", "NumDisplayableItems", 5,
            new ConfigDescription(
                "The max number of recent items to display",
                new AcceptableValueRange<int>(1, Display.MaxItems),
                MenuElementGenerators.CreateIntSliderGenerator()
            )
        );
    }
}
