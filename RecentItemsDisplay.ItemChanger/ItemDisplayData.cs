using ItemChanger;
using ItemChanger.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.UnityConverters.Math;
using UnityEngine;

namespace RecentItemsDisplay.ItemChanger;

internal class ItemDisplayData(UIDef? origUIdef, string message, IValueProvider<Sprite>? overrideSprite, Color? color)
{
    public UIDef? _origUIdef { get; set; } = origUIdef;
    public IValueProvider<Sprite>? _overrideSprite { get; set; } = overrideSprite;

    [JsonConverter(typeof(ColorConverter))] public Color? _color = color;

    public string _message { get; set; } = message;

    public void SendToDisplay()
    {
        Sprite? sprite;

        if (_overrideSprite != null)
        {
            sprite = _overrideSprite.Value;
        }
        else if (_origUIdef != null)
        {
            sprite = _origUIdef.GetSprite();
        }
        else
        {
            sprite = null;
        }

        RecentItemsDisplayAPI.SendMessageToDisplay(
            sprite,
            _message,
            _color
        );
    }
}
