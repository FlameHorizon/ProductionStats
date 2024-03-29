// derived from code by Jesse Plamondon-Willard under MIT license: https://github.com/Pathoschild/StardewMods

using StardewValley;

namespace ProductionStats.Containers;

internal interface IStorageContainer
{
    IEnumerable<Item> GetItemsForPlayer(long uniqueMultiplayerID);
}