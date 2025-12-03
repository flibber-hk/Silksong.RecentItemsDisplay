using DataDrivenConstants.Marker;
using TeamCherry.Localization;

namespace RecentItemsDisplay.Localization;

[JsonData("$.*~", "**/languages/en.json")]
internal static partial class LanguageKeys
{
    internal static string LocalizeKey(this string s) => Language.Get(s, $"Mods.{RecentItemsDisplayPlugin.Id}");
}
