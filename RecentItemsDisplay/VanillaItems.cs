using BepInEx.Logging;
using HutongGames.PlayMaker;
using HutongGames.PlayMaker.Actions;
using MonoDetour;
using Silksong.FsmUtil;
using Silksong.UnityHelper.Extensions;
using System;
using System.Linq;
using TeamCherry.Localization;
using UnityEngine;
using Logger = BepInEx.Logging.Logger;
using UObject = UnityEngine.Object;


namespace RecentItemsDisplay;

internal static class VanillaItems
{
    // Separate method for forward-compatibility
    private static void SendToDisplay(Sprite sprite, string text) => Display.AddItem(sprite, text);

    private static readonly MonoDetourManager mgr = new($"{RecentItemsDisplayPlugin.Id} :: {nameof(VanillaItems)}");
    private static readonly MonoDetourManager fsmMgr = new($"{RecentItemsDisplayPlugin.Id} :: {nameof(VanillaItems)} :: FSM");
    private static readonly ManualLogSource Log = Logger.CreateLogSource($"{nameof(RecentItemsDisplay)}.{nameof(VanillaItems)}");

    public static void Hook()
    {
        // Most relevant SavedItem subclasses implement their own collect method
        // and delegate SavedItem.Get to that, rather than the other way round,
        // and many places throughout the game call the submethod.
        // So we have to manually hook several subclasses that define the relevant methods.

        // TODO - triage the remaining subclasses of SavedItem
        // TODO - implement settings for showing (and omitting) e.g. journal entries
        
        // MateriumItem - an isolated direct subclass of SavedItem
        Md.MateriumItem.Get.Prefix(OnCollectMateriumItem, manager: mgr);
        
        // QuestGroupBase - TODO: investigate instances of QGB

        // QuestTargetCounter subclasses
        Md.CollectableItem.Collect.Prefix(OnCollectCollectableItem, manager: mgr);
        Md.CollectableRelic.Get.Prefix(OnCollectCollectableRelic, manager: mgr);
        // EnemyJournalRecord.Get.Prefix(...); - TODO
        // FakeCollectible - all subclasses call base.Get
        Md.FakeCollectable.Get.Prefix(GetFakeCollectable, manager: mgr);
        // JournalQuestTarget - ???
        // QuestTarget* - ???
        // ToolItemBase has two direct subclasses
        Md.ToolItem.Unlock.Prefix(OnCollectToolItem, manager: mgr);
        Md.ToolCrest.Unlock.Prefix(OnCollectToolCrest, manager: mgr);

        // Special cases
        // Shop items which don't have a saved item can be handled separately
        Md.ShopItem.SetPurchased.Postfix(OnBuyShopItem, manager: mgr);
        // Watch the UI message for silk skills and powerups (ancestral arts)
        Md.PowerUpGetMsg.Setup.Postfix(OnGetPowerUp, manager: mgr);
        Md.SkillGetMsg.Setup.Postfix(OnGetSkill, manager: mgr);

        // Items which directly modify PlayerData via an FSM
        Md.PlayMakerFSM.Start.Prefix(WatchFsms, manager: fsmMgr);
    }

    private static bool NameMatches(this string self, string target)
    {
        if (target == self) return true;
        if (!self.StartsWith(target)) return false;

        string remainder = self.Substring(target.Length);
        if (string.IsNullOrWhiteSpace(remainder)) return true;

        return remainder.TrimStart().StartsWith("(");
    }

    private static void OnGetSkill(SkillGetMsg self, ref ToolItemSkill skill)
    {
        SendToDisplay(skill.GetUIMsgSprite(), skill.GetUIMsgName());
    }

    private static void OnGetPowerUp(PowerUpGetMsg self, ref PowerUpGetMsg.PowerUps skill)
    {
        SendToDisplay(self.solidSprite.sprite, self.nameText.text);
    }

    private static void WatchFsms(PlayMakerFSM self)
    {
        WatchSpoolsAndMaskShards(self);
        WatchSilkHearts(self);

        // Vesticrest
        WatchUpgradeFsm(self);

        // Citadel melodies
        WatchMelodyFsm(self);

        // Other abilities
        WatchGeneralGetFsm(self);

        // TODO - Deduplicate courier stuff
    }

    private static void WatchGeneralGetFsm(PlayMakerFSM self)
    {
        // Materium, Farsight represent building the item, I think I'll keep them
        // Silk skills, Ancestral Arts seem to be ignored
        // Silk Heart and Hunter Upgrades should be excluded

        if (!self.gameObject.name.NameMatches("UI Msg Get Item") || self.FsmName != "Msg Control") return;

        if (self.GetState("Stop Up") is not FsmState giveState) return;

        giveState.InsertMethod(0, static a =>
        {
            string check = a.Fsm.GetFsmString("Item").Value;

            if (check == "SilkHeart" || check.StartsWith("Hunter Combo"))
            {
                return;
            }
            
            GameObject go = a.Fsm.FsmComponent.gameObject;

            GameObject? icon = go.FindChild("Icon");
            if (icon == null) return;

            GameObject? itemName = go.FindChild("Item Name");
            if (itemName == null) return;

            Sprite sprite = icon.GetComponent<SpriteRenderer>().sprite;

            string text;
            if (itemName.TryGetComponent<TMProOld.TextMeshPro>(out TMProOld.TextMeshPro oldTmpro))
            {
                text = oldTmpro.text;
            }
            else if (itemName.TryGetComponent<TMPro.TextMeshPro>(out TMPro.TextMeshPro newTmpro))
            {
                text = newTmpro.text;
            }
            else
            {
                return;
            }

            SendToDisplay(sprite, text);
        });
    }

    private static void WatchMelodyFsm(PlayMakerFSM self)
    {
        if (!self.gameObject.name.NameMatches("UI Msg Get Item Melody") || self.FsmName != "Msg Control") return;

        if (self.GetState("Stop Up") is not FsmState giveState) return;

        giveState.InsertMethod(0, static a =>
        {
            GameObject go = a.Fsm.FsmComponent.gameObject;

            GameObject? icon = go.FindChild("Contents/Icon");
            if (icon == null) return;

            GameObject? itemName = go.FindChild("Contents/Item Name");
            if (itemName == null) return;

            Sprite sprite = icon.GetComponent<SpriteRenderer>().sprite;

            string text;
            if (itemName.TryGetComponent<TMProOld.TextMeshPro>(out TMProOld.TextMeshPro oldTmpro))
            {
                text = oldTmpro.text;
            }
            else if (itemName.TryGetComponent<TMPro.TextMeshPro>(out TMPro.TextMeshPro newTmpro))
            {
                text = newTmpro.text;
            }
            else
            {
                return;
            }

            SendToDisplay(sprite, text);
        });
    }

    private static void WatchUpgradeFsm(PlayMakerFSM self)
    {
        if (!self.gameObject.name.NameMatches("UI Msg Crest Evolve") || self.FsmName != "Msg Control") return;

        string[] stateNames = ["Set Socket 1", "Set Socket 2"];
        foreach (string stateName in stateNames)
        {
            if (self.GetState(stateName) is not FsmState unlockState)
            {
                continue;
            }

            unlockState.AddMethod(static a =>
            {
                FsmState state = a.State;
                SetGameObject? sgo = state.GetFirstActionOfType<SetGameObject>();
                if (sgo == null) return;

                GameObject? crest = sgo.gameObject.Value.FindChild("Pivot/Crest");
                if (crest == null) return;

                Sprite sprite = crest.GetComponent<SpriteRenderer>().sprite;
                SendToDisplay(sprite, Language.Get("UI_MSG_TITLE_EXTRASLOT_NAME", "Tools"));
            });
        }
    }

    private static void WatchSilkHearts(PlayMakerFSM self)
    {
        // There is technically a SavedItemTrackerMarker but that is on the outer scene, not the memory scene
        // which is where the item is given from

        if (!self.gameObject.name.NameMatches("Memory Control") || self.FsmName != "Memory Control")
        {
            return;
        }

        FsmState? state = self.GetState("End Scene");
        if (state == null) { return; }

        foreach (RunFSM runFsm in state.Actions.OfType<RunFSM>())
        {            
            if (runFsm.fsmTemplateControl.fsmTemplate.name == "memory_scene_control_pre_end_silkheart")
            {
                // Get the sprite
                GameObject? spriteOwner = self.gameObject.scene.FindGameObject("GameObject/silk_cocoon_core/heart/heart_core_beat");
                if (spriteOwner == null)
                {
                    Log.LogError($"Unable to locate heart core in scene {self.gameObject.scene.name}");
                    return;
                }

                Sprite sprite = spriteOwner.GetComponent<SpriteRenderer>().sprite;

                // Technically this should be done on the template FSM, but I think this is fine
                state.InsertMethod(1, a => { SendToDisplay(sprite, Language.Get("MEMORY_MSG_TITLE_SILKHEART", "Prompts")); });
            }

        }
    }

    private static void CheckTrackerMarker(PlayMakerFSM self, string fsmName, string goPrefix, string stateName, string langKey, string sheet = "UI")
    {
        if (self.FsmName != fsmName || !self.gameObject.name.NameMatches(goPrefix)) return;

        FsmState? state = self.GetState(stateName);
        if (state == null)
        {
            Log.LogInfo($"No state {stateName} on fsm {self.FsmName} on {self.gameObject.name}");
            return;
        }

        state.InsertMethod(0, a =>
        {
            PlayMakerFSM fsm = a.Fsm.FsmComponent;
            SavedItemTrackerMarker sitm = fsm.gameObject.GetComponent<SavedItemTrackerMarker>();
            // The name on the saved item is not set so we look it up in Language
            string name = Language.Get(langKey, sheet);
            SendToDisplay(sitm.items[0].GetPopupIcon(), name);
        });
    }

    private static void WatchSpoolsAndMaskShards(PlayMakerFSM self)
    {
        CheckTrackerMarker(self, "Heart Container Control", "Heart Piece", "Save Collected", "INV_NAME_HEART_PIECE_1");
        CheckTrackerMarker(self, "Control", "Silk Spool", "Save", "INV_NAME_SPOOL_PIECE_HALF");
    }

    private static void GetFakeCollectable(FakeCollectable self, ref bool showPopup)
    {
        SendToDisplay(self.GetUIMsgSprite(), self.GetUIMsgName());
    }

    private static void OnBuyShopItem(ShopItem self, ref Action onComplete, ref int subItemIndex)
    {
        if (self.savedItem != null) return;

        SendToDisplay(self.ItemSprite, self.DisplayName);
    }


    private static void OnCollectCollectableRelic(CollectableRelic self, ref bool showPopup)
    {
        SendToDisplay(self.GetUIMsgSprite(), self.GetUIMsgName());
    }

    private static void OnCollectMateriumItem(MateriumItem self, ref bool showPopup)
    {
        SendToDisplay(self.GetPopupIcon(), self.GetPopupName());
    }

    private static void OnCollectToolCrest(ToolCrest self)
    {
        // bool showPopup = ???, probably false
        SendToDisplay(self.GetUIMsgSprite(), self.displayName.ToString());
    }

    private static void OnCollectToolItem(ToolItem self, ref Action afterTutorialMsg, ref ToolItem.PopupFlags popupFlags)
    {
        // bool showPopup = popupFlags != ToolItem.PopupFlags.None;
        SendToDisplay(self.GetUIMsgSprite(), self.GetUIMsgName());
    }

    private static void OnCollectCollectableItem(CollectableItem self, ref int amount, ref bool showPopup)
    {
        if (self is DeliveryQuestItem dqi)
        {
            // When getting a delivery quest item, you get multiple at once
            // for each break that's allowed.
            // It makes sense to only show one in RecentItems
            if (dqi.CollectedAmount > 0) return;
        }

        SendToDisplay(self.GetUIMsgSprite(), self.GetUIMsgName());
    }
}
