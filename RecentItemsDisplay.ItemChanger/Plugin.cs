using BepInEx;
using ItemChanger;

namespace RecentItemsDisplay.ItemChanger
{
    // TODO - adjust the plugin guid as needed
    [BepInAutoPlugin(id: "io.github.flibber-hk.recentitemsdisplay_itemchanger")]
    [BepInDependency("io.github.silksong.itemchanger")]
    public partial class Plugin : BaseUnityPlugin
    {
        private void Awake()
        {
            RecentItemsDisplayAPI.AddVanillaItemsSuppression(() => ItemChangerHost.Singleton.ActiveProfile != null);
            ItemChangerHost.Singleton.LifecycleEvents.OnEnterGame += () => ItemChangerHost.Singleton.ActiveProfile!.Modules.GetOrAdd<RecentItemsModule>();
            Logger.LogInfo($"Plugin {Name} ({Id}) has loaded!");
        }
    }
}
