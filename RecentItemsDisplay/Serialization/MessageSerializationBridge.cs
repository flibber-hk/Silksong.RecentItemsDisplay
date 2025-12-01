using System.Collections.Generic;
using UnityEngine;

namespace RecentItemsDisplay.Serialization;

internal static class MessageSerializationBridge
{
    // TODO - serialize this
    private static Queue<DisplayItemData> ItemData = new();

    internal static void Hook()
    {
        Display.OnCreateDisplay += LoadItemsFromSave;
    }

    internal static void SendItemToDisplay(ISpriteProvider sprite, string message, Color? textColor = null)
    {
        if (ItemData.Count > Display.MaxItems)
        {
            ItemData.Dequeue();
        }
        ItemData.Enqueue(new DisplayItemData() { Sprite = sprite, Message = message, TextColor = textColor });
        Display.AddItem(sprite.GetSprite(), message, textColor);
    }

    internal static void LoadItemsFromSave()
    {
        foreach (DisplayItemData item in ItemData)
        {
            Display.AddItem(item.Sprite.GetSprite(), item.Message, item.TextColor);
        }
    }
}
