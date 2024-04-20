using StardewModdingAPI.Utilities;
using StardewValley;

namespace ProductionStats;

internal record TrackedItem(Item Item, int Count, SDate Date)
{
    /// <summary>
    /// Creates instance using tuple.
    /// </summary>
    /// <param name="info">Tuple storing data.</param>
    public TrackedItem((string QualifiedItemId, int Count, SDate Date) info)
        : this(info.QualifiedItemId, info.Count, info.Date)
    {
    }

    /// <summary>
    /// Creates instance using specified data.
    /// </summary>
    /// <param name="qualifiedItemId">Item id using in <see cref="ItemRegistry"/> to spawn an item.</param>
    /// <param name="count">Number of items.</param>
    /// <param name="date">When item was acquired.</param>
    public TrackedItem(string qualifiedItemId, int count, SDate date) : this(
        ItemRegistry.Create(qualifiedItemId),
        count, 
        date)
    {
    }

    /// <summary>
    /// Converts <see cref="TrackedItem"/> to a form which can be serialized.
    /// </summary>
    /// <returns>Tuple representing tracked item.</returns>
    internal (string, int, SDate) ToSerializeable() 
        => (Item.QualifiedItemId, Count, Date);
}