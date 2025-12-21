using BepInEx.Logging;
using Newtonsoft.Json;
using System;
using UnityEngine;
using Logger = BepInEx.Logging.Logger;

namespace RecentItemsDisplay.Serialization;

/// <summary>
/// Class representing a serializable sprite.
/// </summary>
public interface ISpriteProvider
{
    Sprite GetSprite();
}

public static class SpriteProviderExtensions
{
    private static ManualLogSource Log = Logger.CreateLogSource(nameof(SpriteProviderExtensions));

    public static Sprite GetSpriteSafe(this ISpriteProvider provider)
    {
        try
        {
            return provider.GetSprite();
        }
        catch (Exception)
        {
            Log.LogWarning($"Failed to get sprite for provider of type {provider?.GetType().Name ?? string.Empty}");
            return NonSerializableSprite.NullSprite();
        }
    }
}

/// <summary>
/// Class representing a sprite that is not serializable.
/// </summary>
public class NonSerializableSprite : ISpriteProvider
{
    [JsonIgnore] public Sprite? RuntimeSprite { get; init; }

    public static Sprite NullSprite()
    {
        return Sprite.Create(new Rect(0, 0, 1, 1), Vector2.zero, 1);
    }

    Sprite ISpriteProvider.GetSprite()
    {
        if (RuntimeSprite != null)
        {
            return RuntimeSprite;
        }

        return NullSprite();
    }
}

/// <summary>
/// Class representing a sprite that is associated with a collectable item.
/// </summary>
public class CollectableItemSprite(string itemName) : ISpriteProvider
{
    public string ItemName { get; set; } = itemName;

    Sprite ISpriteProvider.GetSprite()
    {
        return CollectableItemManager.GetItemByName(ItemName).GetUIMsgSprite();
    }
}

public class ToolItemSprite(string toolName) : ISpriteProvider
{
    public string ToolName { get; set; } = toolName;

    Sprite ISpriteProvider.GetSprite()
    {
        return ToolItemManager.GetToolByName(ToolName).GetUIMsgSprite();
    }
}

public class ToolCrestSprite(string crestName) : ISpriteProvider
{
    public string CrestName { get; set; } = crestName;

    Sprite ISpriteProvider.GetSprite()
    {
        return ToolItemManager.GetCrestByName(CrestName).GetUIMsgSprite();
    }
}


public class CollectableRelicSprite(string relicName) : ISpriteProvider
{
    public string RelicName { get; set; } = relicName;

    Sprite ISpriteProvider.GetSprite()
    {
        return CollectableRelicManager.GetRelic(RelicName).GetUIMsgSprite();
    }
}

public class MateriumSprite(string materiumName) : ISpriteProvider
{
    public string MateriumName { get; set; } = materiumName;

    Sprite ISpriteProvider.GetSprite()
    {
        return MateriumItemManager.Instance.masterList.GetByName(MateriumName).GetPopupIcon();
    }
}