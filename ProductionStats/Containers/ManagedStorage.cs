// derived from code by Jesse Plamondon-Willard under MIT license: https://github.com/Pathoschild/StardewMods

using StardewValley;

namespace ProductionStats.Containers;

internal record ManagedStorage(IStorageContainer container)
{
    public IEnumerable<Item> GetItemsForCurrentPlayer()
    {
        return container.GetItemsForPlayer(Game1.player.UniqueMultiplayerID);
    }
}