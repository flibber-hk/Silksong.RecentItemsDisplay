using BepInEx;
using ItemChanger;
using ItemChanger.Items;

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
            Item.AfterGiveGlobal += args =>
            {
                if (args.Item.UIDef != null)
                {
                    RecentItemsDisplayAPI.SendMessageToDisplay(
                        args.Item.UIDef.GetSprite(),
                        args.Item.UIDef.GetPostviewName()
                    );
                }
            };
            Logger.LogInfo($"Plugin {Name} ({Id}) has loaded!");
        }
    }
}
