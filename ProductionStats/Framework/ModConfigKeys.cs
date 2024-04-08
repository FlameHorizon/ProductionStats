// derived from code by Jesse Plamondon-Willard under MIT license: https://github.com/Pathoschild/StardewMods

using StardewModdingAPI;
using StardewModdingAPI.Utilities;
using System.Runtime.Serialization;

namespace ProductionStats.Framework;

internal class ModConfigKeys
{
    public KeybindList ToggleMenu { get; set; } = new(SButton.F5);
    public KeybindList Sort { get; set; } = new(SButton.S);
    public KeybindList ScrollUp { get; set; } = new(SButton.Up);
    public KeybindList ScrollDown { get; set; } = new(SButton.Down);
    public KeybindList PageUp { get; set; } = new(SButton.PageUp);
    public KeybindList PageDown { get; set; } = new(SButton.PageDown);
    public KeybindList FocusSearch { get; set; } = new(SButton.F);
    public KeybindList DisplayDebugInfo { get; internal set; } = new(SButton.F12);
    public KeybindList ToggleProductionMenu { get; internal set; } = new(SButton.F11);

    /// <summary>Normalize the model after it's deserialized.</summary>
    /// <param name="context">The deserialization context.</param>
    [OnDeserialized]
    public void OnDeserialized(StreamingContext context)
    {
        ToggleMenu ??= new KeybindList();
        Sort ??= new KeybindList();
        ScrollUp ??= new KeybindList();
        ScrollDown ??= new KeybindList();
        PageUp ??= new KeybindList();
        PageDown ??= new KeybindList();
        FocusSearch ??= new KeybindList();
        DisplayDebugInfo ??= new KeybindList();
        ToggleMenu ??= new KeybindList();
    }
}