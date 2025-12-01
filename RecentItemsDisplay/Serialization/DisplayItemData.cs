using UnityEngine;

namespace RecentItemsDisplay.Serialization;

internal class DisplayItemData
{
    public required ISpriteProvider Sprite { get; set; }
    public required string Message { get; set; }
    public Color? TextColor { get; set; } = null;
}
