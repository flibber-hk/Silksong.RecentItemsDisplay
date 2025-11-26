using BepInEx;

namespace RecentItemsDisplay;

[BepInDependency("org.silksong-modding.i18n")]
[BepInDependency("org.silksong-modding.fsmutil")]
[BepInDependency("org.silksong-modding.unityhelper")]
[BepInDependency("io.github.flibber-hk.canvasutil")]
[BepInAutoPlugin(id: "io.github.flibber-hk.recentitemsdisplay")]
public partial class RecentItemsDisplayPlugin : BaseUnityPlugin
{
    public static RecentItemsDisplayPlugin Instance { get; private set; }

    private void Awake()
    {
        Instance = this;

        Display.Hook();
        VanillaItems.Hook();

        Logger.LogInfo($"Plugin {Name} ({Id}) has loaded!");
    }
}
