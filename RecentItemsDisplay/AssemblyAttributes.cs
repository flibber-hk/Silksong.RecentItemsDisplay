using MonoDetour.HookGen;

[assembly: System.Resources.NeutralResourcesLanguage("EN")]

[assembly: MonoDetourTargets(typeof(GameManager))]
[assembly: MonoDetourTargets(typeof(QuitToMenu))]
[assembly: MonoDetourTargets(typeof(InvAnimateUpAndDown))]
[assembly: MonoDetourTargets(typeof(UIManager))]
[assembly: MonoDetourTargets(typeof(GameMap))]
[assembly: MonoDetourTargets(typeof(HeroController))]

[assembly: MonoDetourTargets(typeof(CollectableItem))]
[assembly: MonoDetourTargets(typeof(CollectableRelic))]
[assembly: MonoDetourTargets(typeof(ToolItem))]
[assembly: MonoDetourTargets(typeof(ToolCrest))]
[assembly: MonoDetourTargets(typeof(MateriumItem))]
[assembly: MonoDetourTargets(typeof(FakeCollectable))]
[assembly: MonoDetourTargets(typeof(ShopItem))]
[assembly: MonoDetourTargets(typeof(PlayMakerFSM))]
[assembly: MonoDetourTargets(typeof(PowerUpGetMsg))]
[assembly: MonoDetourTargets(typeof(SkillGetMsg))]
