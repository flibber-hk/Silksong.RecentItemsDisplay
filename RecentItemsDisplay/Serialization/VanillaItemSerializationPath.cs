using System;
using System.Collections.Generic;
using UnityEngine;

namespace RecentItemsDisplay.Serialization;

internal static class VanillaItemSerializationPath
{
    internal static event Func<bool>? ShouldSuppressVanillaItems;

    private static bool ShouldSuppress()
    {
        if (ShouldSuppressVanillaItems == null) return false;
        foreach (Func<bool> toInvoke in ShouldSuppressVanillaItems.GetInvocationList())
        {
            if (toInvoke == null) continue;
            bool b;
            try
            {
                b = toInvoke.Invoke();
            }
            catch (Exception ex)
            {
                RecentItemsDisplayPlugin.InstanceLogger.LogError($"Error invoking subscriber to VanillaItem...ShouldSuppress\n" + ex);

                b = false;
            }
            if (b)
            {
                return true;
            }
        }
        return false;
    }

    private static void AddVanillaItem(Sprite sprite, string message, Color? textColor)
    {
        if (ShouldSuppress()) return;
        Display.AddItem(sprite, message, textColor);
    }

    private static Queue<DisplayItemData> ItemData = new();

    internal static void SetItemData(IEnumerable<DisplayItemData> datas) => ItemData = new(datas);
    internal static List<DisplayItemData> GetItemData() => [.. ItemData];

    internal static void Hook()
    {
        // TODO - we can enable this when ItemData is properly associated with the save file
        Display.OnCreateDisplay += LoadItemsFromSave;
    }

    internal static void SendItemToDisplay(ISpriteProvider sprite, string message, Color? textColor = null)
    {
        if (ItemData.Count > Display.MaxItems)
        {
            ItemData.Dequeue();
        }
        ItemData.Enqueue(new DisplayItemData() { Sprite = sprite, Message = message, TextColor = textColor });
        AddVanillaItem(sprite.GetSpriteSafe(), message, textColor);
    }

    internal static void LoadItemsFromSave()
    {
        foreach (DisplayItemData item in ItemData)
        {
            AddVanillaItem(item.Sprite.GetSpriteSafe(), item.Message, item.TextColor);
        }
    }
}
