// derived from code by Jesse Plamondon-Willard under MIT license: https://github.com/Pathoschild/StardewMods

using StardewValley;

namespace ProductionStats.Containers
{
    internal class ShippingBinContainer(GameLocation location) : IStorageContainer
    {
        private readonly GameLocation _location = location;

        public IEnumerable<Item> GetItemsForPlayer(long uniqueMultiplayerID)
        {
            Farm farm = _location as Farm ?? Game1.getFarm();
            StardewValley.Inventories.IInventory shippingBin = farm.getShippingBin(Game1.player);
            return shippingBin;
        }
    }
}