using ItemChanger;
using ItemChanger.Enums;
using ItemChanger.Events.Args;
using ItemChanger.Items;
using ItemChanger.Modules;
using ItemChanger.Serialization;
using ItemChanger.Tags;
using Newtonsoft.Json;
using System.Collections.Generic;
using UnityEngine;

namespace RecentItemsDisplay.ItemChanger;

public class RecentItemsModule : Module
{
    public RecentItemsModule() => ModuleHandlingProperties = ModuleHandlingFlags.AllowDeserializationFailure | ModuleHandlingFlags.RemoveOnNewProfile;
    
    [JsonProperty] private Queue<ItemDisplayData> ItemDatas = new(20);

    protected override void DoLoad()
    {
        ItemChangerHost.Singleton.LifecycleEvents.OnSafeToGiveItems += RecentItemsDisplayAPI.RefreshDisplay;
        Item.AfterGiveGlobal += OnGiveItem;
        RecentItemsDisplayAPI.OnCreateDisplay += SendAll;
    }

    protected override void DoUnload()
    {
        ItemChangerHost.Singleton.LifecycleEvents.OnSafeToGiveItems -= RecentItemsDisplayAPI.RefreshDisplay;
        Item.AfterGiveGlobal -= OnGiveItem;
        RecentItemsDisplayAPI.OnCreateDisplay -= SendAll;
    }

    private void OnGiveItem(ReadOnlyGiveEventArgs args)
    {
        UIDef? uidef = args.Item.UIDef;

        IValueProvider<Sprite>? overrideSprite = null;
        string text = uidef?.GetPostviewName() ?? string.Empty;
        Color? color = null;

        ReadTags(args.Item, ref overrideSprite, ref text, ref color);

        ItemDisplayData data = new(uidef, text, overrideSprite, color);
        data.SendToDisplay();
        ItemDatas.Enqueue(data);
    }

    private void ReadTags(Item item, ref IValueProvider<Sprite>? overrideSprite, ref string text, ref Color? color)
    {
        foreach (IInteropTag tag in item.GetTags<IInteropTag>())
        {
            if (tag.Message != "RecentItemsDisplay")
            {
                continue;
            }

            if (tag.TryGetProperty("OverrideSprite", out IValueProvider<Sprite>? oSprite))
            {
                overrideSprite = oSprite;
            }
            if (tag.TryGetProperty("OverrideText", out string? oText))
            {
                text = oText ?? text;
            }
            if (tag.TryGetProperty("OverrideColor", out Color? oColor))
            {
                color = oColor ?? color;
            }
        }
    }

    private void SendAll()
    {
        foreach (ItemDisplayData item in ItemDatas)
        {
            item.SendToDisplay();
        }
    }
}
