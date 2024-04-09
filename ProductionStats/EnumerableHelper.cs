using StardewValley;

namespace ProductionStats;

internal static class EnumerableHelper
{
    public static IEnumerable<ItemStock> ToItemStock(this IEnumerable<(Item Item, int Count)> values) 
        => values.Select(x => x.ToItemStock());

    public static ItemStock ToItemStock(this (Item Item, int Count) item) 
        => new(item.Item) { Count = item.Count };
}
