using BepInEx.Logging;
using MonoDetour.Reflection.Unspeakable;
using System.Collections.Generic;
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

    // TODO - make this configurable
    public static int MaxItems { get; internal set; } = 5;

    private static readonly Queue<GameObject> items = new();

    private static GameObject? canvas;

    // TODO - make this configurable
    private static readonly Vector2 AnchorPoint = new(0.9f, 0.9f);

    public static void Create()
    {
        if (canvas != null) return;
        // Create base canvas
        canvas = CUtil.CreateCanvas(RenderMode.ScreenSpaceOverlay, new Vector2(1920, 1080));

        CanvasGroup canvasGroup = canvas.GetComponent<CanvasGroup>();
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;

        UObject.DontDestroyOnLoad(canvas);

        CUtil.CreateTextPanel(
            canvas,
            Language.Get("RECENT_ITEMS_TITLE", $"Mods.{RecentItemsDisplayPlugin.Id}"),
            24,
            TextAnchor.MiddleCenter,
            new CanvasUtil.RectData(
                new Vector2(200, 100), Vector2.zero,
                AnchorPoint + new Vector2(-0.025f, 0.05f),
                AnchorPoint + new Vector2(-0.025f, 0.05f)
                )
            );

        Show();
    }

    public static void Destroy()
    {
        if (canvas != null) UObject.Destroy(canvas);
        canvas = null;

        items.Clear();
    }

    public static void Redraw()
    {
        Destroy();
        Create();
        // TODO - send items from save data
    }

    internal static void AddItem(Sprite? sprite, string msg, Color? textColor = null)
    {
        if (canvas == null)
        {
            Create();
        }

        GameObject basePanel = CUtil.CreateBasePanel(canvas!,
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
        
        items.Enqueue(basePanel);
        if (items.Count > MaxItems)
        {
            UObject.Destroy(items.Dequeue());
        }

        UpdatePositions();
    }

    private static void UpdatePositions()
    {
        int i = items.Count - 1;
        foreach (GameObject item in items)
        {
            Vector2 newPos = AnchorPoint + new Vector2(0, -0.06f * i--);
            item.GetComponent<RectTransform>().anchorMin = newPos;
            item.GetComponent<RectTransform>().anchorMax = newPos;
            item.SetActive(i < MaxItems - 1);
        }
    }

    public static void Show()
    {
        if (canvas == null) return;
        canvas.SetActive(true);
    }

    public static void Hide()
    {
        if (canvas == null) return;
        canvas.SetActive(false);
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
