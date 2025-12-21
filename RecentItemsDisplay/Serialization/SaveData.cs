using System.Collections.Generic;

namespace RecentItemsDisplay.Serialization;

internal class SaveData
{
    public List<DisplayItemData> Datas { get; set; } = [];

    public static void SetData(SaveData? data)
    {
        data ??= new();
        MessageSerializationBridge.SetItemData(data.Datas);
    }

    public static SaveData GetData()
    {
        SaveData data = new();
        data.Datas = MessageSerializationBridge.GetItemData();
        return data;
    }
}
