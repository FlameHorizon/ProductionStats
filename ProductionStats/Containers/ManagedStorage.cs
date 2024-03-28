using StardewValley;

namespace ProductionStats.Containers;

internal record ManagedStorage(IStorageContainer container)
{
    public IEnumerable<Item> GetItemsForCurrentPlayer()
    {
        return container.GetItemsForPlayer(Game1.player.UniqueMultiplayerID);
    }
}