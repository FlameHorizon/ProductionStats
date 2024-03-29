// derived from code by Jesse Plamondon-Willard under MIT license: https://github.com/Pathoschild/StardewMods

using StardewValley;

namespace ProductionStats.Framework
{
    internal static class Constant
    {
        public static bool AllowBold => Game1.content.GetCurrentLanguage() != LocalizedContentManager.LanguageCode.zh;
    }
}