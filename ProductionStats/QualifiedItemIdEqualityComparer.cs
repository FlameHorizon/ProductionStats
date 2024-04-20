using StardewValley;
using System.Diagnostics.CodeAnalysis;

namespace ProductionStats;

/// <summary>
/// Compares items using <see cref="Item.QualifiedItemId"/> property.
/// </summary>
internal class QualifiedItemIdEqualityComparer : IEqualityComparer<Item>
{
    public bool Equals(Item? x, Item? y) 
        => x.QualifiedItemId.Equals(y.QualifiedItemId);

    public int GetHashCode([DisallowNull] Item obj) 
        => obj.QualifiedItemId.GetHashCode();
}