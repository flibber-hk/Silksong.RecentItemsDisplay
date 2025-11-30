using BepInEx.Logging;
using MonoDetour.Reflection.Unspeakable;
using System.Collections.Generic;
using System.Linq;
using TeamCherry.Localization;
using UnityEngine;
using UnityEngine.UI;
using CUtil = CanvasUtil.CanvasUtil;
using Logger = BepInEx.Logging.Logger;
using UObject = UnityEngine.Object;

namespace RecentItemsDisplay;

internal static class Display
{
    private static readonly ManualLogSource Log = Logger.CreateLogSource($"{nameof(RecentItemsDisplay)}.{nameof(Display)}");

    // TODO - send items from save data
    // TODO - make these configurable
    public static int NumDisplayableItems { get; internal set; } = 5;
    private static readonly Vector2 AnchorPoint = new(0.9f, 0.9f);

    public static int MaxItems { get; internal set; } = 10;

    private static GameObject? _canvas;
    private static GameObject? _title;
    private static readonly Queue<GameObject> _items = new();
    


    public static void Create()
    {
        if (_canvas != null) return;
        // Create base canvas
        _canvas = CUtil.CreateCanvas(RenderMode.ScreenSpaceOverlay, new Vector2(1920, 1080));

        CanvasGroup canvasGroup = _canvas.GetComponent<CanvasGroup>();
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;

        UObject.DontDestroyOnLoad(_canvas);

        _title = CUtil.CreateTextPanel(
            _canvas,
            Language.Get("RECENT_ITEMS_TITLE", $"Mods.{RecentItemsDisplayPlugin.Id}"),
            24,
            TextAnchor.MiddleCenter,
            new CanvasUtil.RectData(
                new Vector2(200, 100), Vector2.zero,
                AnchorPoint + new Vector2(-0.025f, 0.05f),
                AnchorPoint + new Vector2(-0.025f, 0.05f)
                )
            );

        UpdateVisibility();
        Show();
    }

    public static void Destroy()
    {
        if (_canvas != null) UObject.Destroy(_canvas);
        _canvas = null;

        _items.Clear();
    }

    public static void Redraw()
    {
        Destroy();
        Create();
        UpdateVisibility();
    }

    internal static void AddItem(Sprite? sprite, string msg, Color? textColor = null)
    {
        if (_canvas == null)
        {
            Create();
        }

        GameObject basePanel = CUtil.CreateBasePanel(_canvas!,
            new CanvasUtil.RectData(new Vector2(200, 50), Vector2.zero,
            AnchorPoint, AnchorPoint));

        if (sprite != null)
        {
            CUtil.CreateImagePanel(basePanel, sprite,
                new CanvasUtil.RectData(new Vector2(50, 50), Vector2.zero, new Vector2(-0.1f, 0.5f),
                    new Vector2(-0.1f, 0.5f)));
        }
        GameObject textPanel = CUtil.CreateTextPanel(basePanel, msg, 24, TextAnchor.MiddleLeft,
            new CanvasUtil.RectData(new Vector2(400, 100), Vector2.zero,
            new Vector2(1.1f, 0.5f), new Vector2(1.1f, 0.5f)),
            CanvasUtil.Fonts.GetFont("Perpetua")!);

        if (textColor.HasValue)
        {
            textPanel.GetComponent<Text>().color = textColor.Value;
        }
        
        _items.Enqueue(basePanel);
        if (_items.Count > MaxItems)
        {
            UObject.Destroy(_items.Dequeue());
        }

        UpdatePositions();
        UpdateVisibility();
    }

    private static void UpdatePositions()
    {
        int i = _items.Count - 1;
        foreach (GameObject item in _items)
        {
            Vector2 newPos = AnchorPoint + new Vector2(0, -0.06f * i--);
            item.GetComponent<RectTransform>().anchorMin = newPos;
            item.GetComponent<RectTransform>().anchorMax = newPos;
            item.SetActive(i < MaxItems - 1);
        }
    }
    private static void UpdateVisibility()
    {
        foreach ((GameObject item, int index) in _items.Reverse().Select((go, idx) => (go, idx)))
        {
            item.SetActive(index < NumDisplayableItems);
        }

        if (_title != null)
        {
            _title.SetActive(_items.Count > 0);
        }
    }

    public static void Show()
    {
        if (_canvas == null) return;
        _canvas.SetActive(true);
    }

    public static void Hide()
    {
        if (_canvas == null) return;
        _canvas.SetActive(false);
    }

    internal static void Hook()
    {
        Md.QuitToMenu.Start.PostfixMoveNext(DestroyOnQuitToMenu);
        Md.HeroController.Start.Postfix(CreateOnEnterGame);

        Md.UIManager.UIGoToPauseMenu.Postfix(HideOnPause);
        Md.UIManager.UIClosePauseMenu.Postfix(ShowOnUnpause);

        // TODO: When a menu, inv pane, etc is showing, we want to ensure hidden
        // Otherwise ensure not hidden
    }

    private static void ShowOnUnpause(UIManager self)
    {
        Show();
    }

    private static void HideOnPause(UIManager self)
    {
        Hide();
    }

    private static void CreateOnEnterGame(HeroController self)
    {
        Create();
    }

    private static void DestroyOnQuitToMenu(SpeakableEnumerator<object, QuitToMenu> self, ref bool continueEnumeration)
    {
        Destroy();
    }

}
