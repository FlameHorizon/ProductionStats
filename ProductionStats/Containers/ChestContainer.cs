using StardewValley.Objects;
using StardewValley;

namespace ProductionStats.Containers;

public class ChestContainer(Chest chest) : IStorageContainer
{
    private readonly Chest _chest = chest;

    public IEnumerable<Item> GetItemsForPlayer(long uniqueMultiplayerID)
    {
        return _chest.GetItemsForPlayer(uniqueMultiplayerID);
    }
}