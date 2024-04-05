// derived from code by Jesse Plamondon-Willard under MIT license: https://github.com/Pathoschild/StardewMods

using StardewValley;
using StardewValley.Objects;

namespace ProductionStats.Containers;

public class ChestContainer(Chest chest) : IStorageContainer
{
    private readonly Chest _chest = chest;

    public IEnumerable<Item> GetItemsForPlayer(long uniqueMultiplayerID)
    {
        return _chest.GetItemsForPlayer(uniqueMultiplayerID);
    }
}