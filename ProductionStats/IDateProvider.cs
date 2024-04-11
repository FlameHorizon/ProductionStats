using StardewModdingAPI.Utilities;

namespace ProductionStats;

internal interface IDateProvider
{
    SDate Now { get; }
}
