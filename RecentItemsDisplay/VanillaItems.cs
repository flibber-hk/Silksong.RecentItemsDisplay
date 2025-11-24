using BepInEx.Logging;
using System;

namespace RecentItemsDisplay;

internal static class VanillaItems
{
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
        // FakeCollectible - should check subclasses, maybe
        // JournalQuestTarget - ???
        // QuestTarget* - ???
        // ToolItemBase has two subclasses
        Md.ToolItem.Unlock.Prefix(OnCollectToolItem);
        Md.ToolCrest.Unlock.Prefix(OnCollectToolCrest);
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
