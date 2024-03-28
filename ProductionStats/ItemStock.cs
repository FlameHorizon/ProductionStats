using StardewValley;

namespace ProductionStats;
public class ItemStock(Item item)
{
    public Item Item { get; set; } = item;
    public int Count { get; set; } = 0;
}
