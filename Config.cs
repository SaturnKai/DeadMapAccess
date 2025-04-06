using BepInEx.Configuration;
using UnityEngine;

namespace DeadMapAccess;

static internal class Configuration {
    // global entries
    internal static Color borderColor = new Color32(19, 19, 19, 255);

    // config file entries
    internal static ConfigEntry<float> width = null!;
    internal static ConfigEntry<float> height = null!;
    internal static ConfigEntry<int> borderSize = null!;
    internal static ConfigEntry<bool> toggle = null!;
    internal static ConfigEntry<bool> showUpgrades = null!;

    public static void Init(ConfigFile config) {
        // load config
        width = config.Bind("General", "Width", 600f, "The width of the map.");
        height = config.Bind("General", "Height", 600f, "The height of the map.");
        borderSize = config.Bind("General", "Border", 6, "The size of the map border.");
        toggle = config.Bind("General", "Toggle", false, "Set the map to toggle instead of hold.");
        showUpgrades = config.Bind("General", "ShowUpgrades", true, "Show upgrades while the map is shown.");
    }
}