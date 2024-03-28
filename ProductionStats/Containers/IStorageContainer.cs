using StardewValley;

namespace ProductionStats.Containers;

internal interface IStorageContainer
{
    IEnumerable<Item> GetItemsForPlayer(long uniqueMultiplayerID);
}