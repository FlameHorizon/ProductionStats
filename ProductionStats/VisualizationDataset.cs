using StardewValley;
using StardewValley.ItemTypeDefinitions;

namespace ProductionStats;

internal static class VisualizationDataset
{
    public static IEnumerable<(Item Item, int Count)> Get()
    {
        Random rnd = new(1);

        string itemType = ItemRegistry.type_object;
        ObjectDataDefinition definition = (ObjectDataDefinition)ItemRegistry.GetTypeDefinition(itemType);
        var ids = definition.GetAllIds().ToArray();

        for (int i = 0; i < 30; i++)
        {
            string itemId = Convert.ToString((rnd.Next(0, ids.Length)));
            int amount = rnd.Next(1, 20);
            Item item = ItemRegistry.Create(itemId, amount);

            while (item.DisplayName.Contains("Error"))
            {
                itemId = Convert.ToString((rnd.Next(0, ids.Length)));
                item = ItemRegistry.Create(itemId, amount);
            }

            yield return (item, amount);
        }
    }
}