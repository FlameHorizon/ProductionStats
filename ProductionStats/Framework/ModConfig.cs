// derived from code by Jesse Plamondon-Willard under MIT license: https://github.com/Pathoschild/StardewMods

using System.Runtime.Serialization;

namespace ProductionStats.Framework;

internal class ModConfig
{
    public ModConfigKeys Controls { get; set; } = new();
    
    /// <param name="context">The deserialization context.</param>
    [OnDeserialized]
    public void OnDeserialized(StreamingContext context)
    {
        Controls ??= new ModConfigKeys();
    }
}