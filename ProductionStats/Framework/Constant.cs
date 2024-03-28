using StardewValley;

namespace ProductionStats.Framework
{
    internal static class Constant
    {
        public static bool AllowBold => Game1.content.GetCurrentLanguage() != LocalizedContentManager.LanguageCode.zh;
    }
}