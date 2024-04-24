using StardewValley;
using StardewValley.ItemTypeDefinitions;

namespace ProductionStats;

internal static class VisualizationDataset
{
    public static IEnumerable<(Item Item, int Count)> Get()
    {
        Random rnd = new(1);

        var definition = (ObjectDataDefinition)ItemRegistry
            .GetTypeDefinition(ItemRegistry.type_object);

        string[] ids = definition.GetAllIds().ToArray();

        for (int i = 0; i < 30; i++)
        {
            string itemId = Convert.ToString(rnd.Next(0, ids.Length));
            int amount = rnd.Next(1, 20);
            int quality = rnd.Next(0, 5);

            Item item = ItemRegistry.Create(itemId, amount, quality);

            while (item.DisplayName.Contains("Error"))
            {
                itemId = Convert.ToString((rnd.Next(0, ids.Length)));
                item = ItemRegistry.Create(itemId, amount, quality);
            }

            yield return (item, amount);
        }
    }
}