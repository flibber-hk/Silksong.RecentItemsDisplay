using BepInEx;
using RecentItemsDisplay.Serialization;
using Silksong.DataManager;

namespace RecentItemsDisplay;

[BepInDependency("org.silksong-modding.i18n")]
[BepInDependency("org.silksong-modding.fsmutil")]
[BepInDependency("org.silksong-modding.unityhelper")]
[BepInDependency("org.silksong-modding.datamanager")]
[BepInDependency("org.silksong-modding.modmenu")]
[BepInDependency("io.github.flibber-hk.canvasutil")]
[BepInAutoPlugin(id: "io.github.flibber-hk.recentitemsdisplay")]
public partial class RecentItemsDisplayPlugin : BaseUnityPlugin, ISaveDataMod<SaveData>
{
    public static RecentItemsDisplayPlugin Instance { get; private set; }
    SaveData? ISaveDataMod<SaveData>.SaveData
    {
        get => SaveData.GetData();
        set => SaveData.SetData(value);
    }

    // TODO - item source (area etc)
    // TODO - ItemChanger integration

    private void Awake()
    {
        Instance = this;

        ConfigSettings.Init(Config);
        Display.Hook();
        VanillaItems.Hook();
        MessageSerializationBridge.Hook();

        Logger.LogInfo($"Plugin {Name} ({Id}) has loaded!");
    }
}
