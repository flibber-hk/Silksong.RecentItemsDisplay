using Newtonsoft.Json;
using UnityEngine;

namespace RecentItemsDisplay.Serialization;

/// <summary>
/// Class representing a serializable sprite.
/// </summary>
public interface ISpriteProvider
{
    Sprite GetSprite();
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
public class CollectableItemSprite(string ItemName) : ISpriteProvider
{
    Sprite ISpriteProvider.GetSprite()
    {
        return CollectableItemManager.GetItemByName(ItemName).GetUIMsgSprite();
    }
}

public class ToolItemSprite(string ToolName) : ISpriteProvider
{
    Sprite ISpriteProvider.GetSprite()
    {
        return ToolItemManager.GetToolByName(ToolName).GetUIMsgSprite();
    }
}

public class ToolCrestSprite(string CrestName) : ISpriteProvider
{
    Sprite ISpriteProvider.GetSprite()
    {
        return ToolItemManager.GetCrestByName(CrestName).GetUIMsgSprite();
    }
}


public class CollectableRelicSprite(string RelicName) : ISpriteProvider
{
    Sprite ISpriteProvider.GetSprite()
    {
        return CollectableRelicManager.GetRelic(RelicName).GetUIMsgSprite();
    }
}

public class MateriumSprite(string MateriumName) : ISpriteProvider
{
    Sprite ISpriteProvider.GetSprite()
    {
        return MateriumItemManager.Instance.masterList.GetByName(MateriumName).GetPopupIcon();
    }
}