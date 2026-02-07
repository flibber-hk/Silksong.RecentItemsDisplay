using RecentItemsDisplay.Serialization;
using System;
using UnityEngine;

namespace RecentItemsDisplay;

public static class RecentItemsDisplayAPI
{
    public static void AddVanillaItemsSuppression(Func<bool> shouldSuppress)
    {
        VanillaItemSerializationPath.ShouldSuppressVanillaItems += shouldSuppress;
    }

    public static void SendMessageToDisplay(Sprite sprite, string message)
    {
        SendMessageToDisplay(sprite, message, null);
    }

    public static void SendMessageToDisplay(Sprite sprite, string message, Color? color)
    {
        Display.AddItem(sprite, message, color);
    }
}
