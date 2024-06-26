﻿using StardewValley;

namespace ProductionStats;

internal static class EnumerableHelper
{
    /// <summary>
    /// Converts enumerable to set of <see cref="ItemStock"/>.
    /// </summary>
    /// <param name="values">Values to convert</param>
    /// <returns>Set of <see cref="ItemStock"/> objects for given values.</returns>
    public static IEnumerable<ItemStock> ToItemStock(this IEnumerable<(Item Item, int Count)> values)
        => values.Select(x => x.ToItemStock());

    /// <summary>
    /// Converts single tuple to <see cref="ItemStock"/>.
    /// </summary>
    /// <param name="item">Item to convert.</param>
    /// <returns><see cref="ItemStock"/> representing a given tuple.</returns>
    public static ItemStock ToItemStock(this (Item Item, int Count) item)
        => new(item.Item) { Count = item.Count };
}
