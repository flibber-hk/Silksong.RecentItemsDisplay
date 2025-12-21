using System.Collections.Generic;
using UnityEngine;

namespace RecentItemsDisplay.Serialization;

internal static class MessageSerializationBridge
{
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
        Display.AddItem(sprite.GetSpriteSafe(), message, textColor);
    }

    internal static void LoadItemsFromSave()
    {
        foreach (DisplayItemData item in ItemData)
        {
            Display.AddItem(item.Sprite.GetSpriteSafe(), item.Message, item.TextColor);
        }
    }
}
