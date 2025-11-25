using BepInEx.Logging;
using System;
using Logger = BepInEx.Logging.Logger;


namespace RecentItemsDisplay;

internal static class VanillaItems
{
    private static readonly ManualLogSource Log = Logger.CreateLogSource($"{nameof(RecentItemsDisplay)}.{nameof(VanillaItems)}");

    public static void Hook()
    {
        // Most relevant SavedItem subclasses implement their own collect method
        // and delegate SavedItem.Get to that, rather than the other way round,
        // and many places throughout the game call the submethod.
        // So we have to manually hook several subclasses that define the relevant methods.

        // TODO - triage the remaining subclasses of SavedItem
        // TODO - implement settings for omitting e.g. journal entries
        
        // MateriumItem - an isolated direct subclass of SavedItem
        Md.MateriumItem.Get.Prefix(OnCollectMateriumItem);
        
        // QuestGroupBase - TODO: investigate instances of QGB

        // QuestTargetCounter subclasses
        Md.CollectableItem.Collect.Prefix(OnCollectCollectableItem);
        Md.CollectableRelic.Get.Prefix(OnCollectCollectableRelic);
        // EnemyJournalRecord.Get.Prefix(...); - TODO
        // FakeCollectible - all subclasses call base.Get
        Md.FakeCollectable.Get.Prefix(GetFakeCollectable);
        // JournalQuestTarget - ???
        // QuestTarget* - ???
        // ToolItemBase has two direct subclasses
        Md.ToolItem.Unlock.Prefix(OnCollectToolItem);
        Md.ToolCrest.Unlock.Prefix(OnCollectToolCrest);

        // Special cases
        // Shop items which don't have a saved item should be handled separately
        Md.ShopItem.SetPurchased.Postfix(OnBuyShopItem);
        // Mask Shards, Spool Pieces
        // Silk skills (see GlobalEnums.WeaverSpireAbility and the associated FSMs)
    }

    private static void GetFakeCollectable(FakeCollectable self, ref bool showPopup)
    {
        Display.AddItem(self.GetUIMsgSprite(), self.GetUIMsgName());
    }

    private static void OnBuyShopItem(ShopItem self, ref Action onComplete, ref int subItemIndex)
    {
        if (self.savedItem != null) return;

        Display.AddItem(self.ItemSprite, self.DisplayName);
    }


    private static void OnCollectCollectableRelic(CollectableRelic self, ref bool showPopup)
    {
        Display.AddItem(self.GetUIMsgSprite(), self.GetUIMsgName());
    }

    private static void OnCollectMateriumItem(MateriumItem self, ref bool showPopup)
    {
        Display.AddItem(self.GetPopupIcon(), self.GetPopupName());
    }

    private static void OnCollectToolCrest(ToolCrest self)
    {
        // bool showPopup = ???, probably false
        Display.AddItem(self.GetUIMsgSprite(), self.displayName.ToString());
    }

    private static void OnCollectToolItem(ToolItem self, ref Action afterTutorialMsg, ref ToolItem.PopupFlags popupFlags)
    {
        // bool showPopup = popupFlags != ToolItem.PopupFlags.None;
        Display.AddItem(self.GetUIMsgSprite(), self.GetUIMsgName());
    }

    private static void OnCollectCollectableItem(CollectableItem self, ref int amount, ref bool showPopup)
    {
        Display.AddItem(self.GetUIMsgSprite(), self.GetUIMsgName());
    }
}
