using StardewModdingAPI.Utilities;

namespace ProductionStats;

internal class InGameTimeProvider : IDateProvider
{
    public SDate Now => SDate.Now();
}