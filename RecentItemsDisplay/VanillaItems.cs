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
        // TODO - implement settings for omitting e.g. journal entries
        
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
        // Items which directly modify PlayerData via an FSM
        Md.PlayMakerFSM.Start.Prefix(WatchFsms, manager: fsmMgr);
    }

    private static void WatchFsms(PlayMakerFSM self)
    {
        WatchSpoolsAndMaskShards(self);
        WatchSilkHearts(self);
        // Weaver shrine FSM skills (seemingly covers silkspear, dash, silksphere, walljump, silkdash, harpoondash, superjump)
        WatchWeaverShrines(self);
        // Eva - Hunter2, Hunter3, VestiYellow, VestiBlue, Sylphsong
        // Hunter2 and Hunter3 are handled by the ToolCrest
        WatchEva(self);
        // Needolin
        WatchWidow(self);
        // Rune Rage
        WatchRuneRage(self);
        // Cross Stitch
        WatchPhantom(self);
        // Pale Nails
        WatchPaleNails(self);
        // TODO - Double Jump, Glide, Needle Strike
        // TODO - Beastling Call, Elegy of the Deep
        // TODO - Journal
        // TODO - Probably Everbloom
        // TODO - Deduplicate courier stuff
    }

    private static void AddSsgmWatcher(FsmState getState, int index)
    {
        getState.InsertMethod(index, static a =>
        {
            FsmState state = a.State;

            SpawnSkillGetMsg? ssgm = state.GetFirstActionOfType<SpawnSkillGetMsg>();
            if (ssgm == null) return;

            GameObject msgPrefab = ssgm.MsgPrefab.Value;
            ToolItemSkill? skill = ssgm.Skill.Value as ToolItemSkill;
            if (skill == null) return;

            Display.AddItem(skill.GetUIMsgSprite(), skill.GetUIMsgName());
        });

    }

    private static void WatchPaleNails(PlayMakerFSM self)
    {
        if (!self.gameObject.name.StartsWith("Silk Needle Spell Get") || self.FsmName != "Control")
        {
            return;
        }

        FsmState? state = self.GetState("Msg");
        if (state == null) { return; }

        AddSsgmWatcher(state, 0);
    }

    private static void WatchPhantom(PlayMakerFSM self)
    {
        if (!self.gameObject.name.StartsWith("Phantom") || self.FsmName != "Control")
        {
            return;
        }

        FsmState? state = self.GetState("UI Msg");
        if (state == null) { return; }

        AddSsgmWatcher(state, 1);
    }

    private static void WatchRuneRage(PlayMakerFSM self)
    {
        if (!self.gameObject.name.StartsWith("Memory Control") || self.FsmName != "Memory Control")
        {
            return;
        }

        FsmState? state = self.GetState("Get Rune Bomb");
        if (state == null) { return; }

        AddSsgmWatcher(state, 0);
    }

    private static void WatchWidow(PlayMakerFSM self)
    {
        if (!self.gameObject.name.StartsWith("Spinner Boss") || self.FsmName != "Control")
        {
            return;
        }

        // This isn't the state where they get needolin, that's a few states previously
        if (self.GetState("Get Needolin") is not FsmState needolinState)
        {
            return;
        }

        needolinState.InsertMethod(0, static a =>
        {
            // Make Spugm state not dependent on give state
            FsmState state = a.Fsm.GetState("Get Needolin");

            SpawnPowerUpGetMsg? spugm = state.GetFirstActionOfType<SpawnPowerUpGetMsg>();
            if (spugm == null) return;

            GameObject msgPrefab = spugm.MsgPrefab.Value;
            PowerUpGetMsg.PowerUps powerup = (PowerUpGetMsg.PowerUps)spugm.PowerUp.Value;

            PowerUpGetMsg.PowerUpInfo powerupInfo = msgPrefab.GetComponent<PowerUpGetMsg>().powerUpInfos[(int)powerup];
            Display.AddItem(powerupInfo.SolidSprite, powerupInfo.Name.ToString());
        });
    }

    private static void WatchEva(PlayMakerFSM self)
    {
        if (!self.gameObject.name.StartsWith("Crest Upgrade Shrine") || self.FsmName != "Dialogue")
        {
            return;
        }

        // Sylphsong
        if (self.GetState("Set Bound") is FsmState sylphState)
        {
            sylphState.InsertMethod(1, static a =>
            {
                FsmState state = a.State;

                SpawnPowerUpGetMsg? spugm = state.GetFirstActionOfType<SpawnPowerUpGetMsg>();
                if (spugm == null) return;

                GameObject msgPrefab = spugm.MsgPrefab.Value;
                PowerUpGetMsg.PowerUps powerup = (PowerUpGetMsg.PowerUps)spugm.PowerUp.Value;
                
                PowerUpGetMsg.PowerUpInfo powerupInfo = msgPrefab.GetComponent<PowerUpGetMsg>().powerUpInfos[(int)powerup];
                Display.AddItem(powerupInfo.SolidSprite, powerupInfo.Name.ToString());
            });
        }

        // Vesticrest
        string[] stateNames = ["Unlock First Slot", "Unlock Other Slot"];
        foreach (string stateName in stateNames)
        {
            if (self.GetState(stateName) is not FsmState unlockState)
            {
                continue;
            }

            // Just before/after the SetPlayerDataBool, but the position doesn't really matter
            unlockState.InsertMethod(4, static a =>
            {
                FsmState state = a.State;

                CreateObject? co = state.GetFirstActionOfType<CreateObject>();
                if (co == null) return;

                // The same sprite seems to be used for both
                GameObject prefab = co.gameObject.Value;
                GameObject? crest = prefab.FindChild("Tool_Socket_Evolve_lvl1/Pivot/Crest");
                if (crest == null) return;

                Sprite sprite = crest.GetComponent<SpriteRenderer>().sprite;
                Display.AddItem(sprite, Language.Get("UI_MSG_TITLE_EXTRASLOT_NAME", "Tools"));
            });
        }
        
    }

    private static void WatchWeaverShrines(PlayMakerFSM self)
    {
        if (self.FsmName != "Inspection")
        {
            return;
        }
        if (!self.gameObject.name.StartsWith("Shrine Weaver Ability")
            // I don't believe this one is used but it makes sense to include
            && !self.gameObject.name.StartsWith("Shrine First Weaver NPC"))
        {
            return;
        }

        FsmState? end = self.GetState("End");
        if (end == null) return;

        end.InsertMethod(0, static a =>
        {
            Fsm fsm = a.fsm;
            // First, check if it's a tool
            FsmObject? obj = fsm.GetFsmObject("Skill Item");
            if (obj != null && obj.Value is SavedItem item)
            {
                Display.AddItem(item.GetPopupIcon(), item.GetPopupName());
                return;
            }

            // Otherwise, check if it's a powerup
            PowerUpGetMsg.PowerUps powerup = (PowerUpGetMsg.PowerUps)fsm.GetFsmEnum("Powerup").Value;

            FsmState powerupState = fsm.GetState("Powerup Msg");
            GameObject? msgPrefab = powerupState.GetFirstActionOfType<SpawnPowerUpGetMsg>()?.MsgPrefab.Value;
            if (msgPrefab != null)
            {
                PowerUpGetMsg.PowerUpInfo powerupInfo = msgPrefab.GetComponent<PowerUpGetMsg>().powerUpInfos[(int)powerup];
                Display.AddItem(powerupInfo.SolidSprite, powerupInfo.Name.ToString());
                return;
            }
        });
    }

    private static void WatchSilkHearts(PlayMakerFSM self)
    {
        // There is technically a SavedItemTrackerMarker but that is on the outer scene, not the memory scene
        // which is where the item is given from

        if (!self.gameObject.name.StartsWith("Memory Control") || self.FsmName != "Memory Control")
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
                state.InsertMethod(1, a => { Display.AddItem(sprite, Language.Get("MEMORY_MSG_TITLE_SILKHEART", "Prompts")); });
            }

        }
    }

    private static void CheckTrackerMarker(PlayMakerFSM self, string fsmName, string goPrefix, string stateName, string langKey, string sheet = "UI")
    {
        if (self.FsmName != fsmName || !self.gameObject.name.StartsWith(goPrefix)) return;

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
            Display.AddItem(sitm.items[0].GetPopupIcon(), name);
        });
    }

    private static void WatchSpoolsAndMaskShards(PlayMakerFSM self)
    {
        CheckTrackerMarker(self, "Heart Container Control", "Heart Piece", "Save Collected", "INV_NAME_HEART_PIECE_1");
        CheckTrackerMarker(self, "Control", "Silk Spool", "Save", "INV_NAME_SPOOL_PIECE_HALF");
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
