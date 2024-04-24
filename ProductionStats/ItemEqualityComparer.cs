using StardewValley;
using System.Diagnostics.CodeAnalysis;

namespace ProductionStats;

/// <summary>
/// Compares items using <see cref="Item.QualifiedItemId"/> property.
/// </summary>
internal class ItemEqualityComparer : IEqualityComparer<Item>
{
    public bool Equals(Item? x, Item? y) 
        => x.QualifiedItemId.Equals(y.QualifiedItemId) 
           && x.Quality == y.Quality;

    public int GetHashCode([DisallowNull] Item obj) 
        => obj.QualifiedItemId.GetHashCode() + obj.Quality;
}