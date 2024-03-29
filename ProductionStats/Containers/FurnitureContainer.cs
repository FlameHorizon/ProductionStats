// derived from code by Jesse Plamondon-Willard under MIT license: https://github.com/Pathoschild/StardewMods

using StardewValley.Objects;
using StardewValley;

namespace ProductionStats.Containers;

internal class FurnitureContainer(StorageFurniture furniture) : IStorageContainer
{
    private readonly StorageFurniture _furniture = furniture;

    public IEnumerable<Item> GetItemsForPlayer(long uniqueMultiplayerID)
    {
        return _furniture.heldItems;
    }
}