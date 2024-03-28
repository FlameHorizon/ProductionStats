using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using StardewValley;
using StardewValley.Menus;
using ProductionStats.Components;

namespace ProductionStats;

internal class ModEntry : Mod
{
    /// <summary>
    ///     Finds all the chests which are placed by the player in the world.
    /// </summary>
    private ChestFinder _chestFinder = null!; // Set on Entry.

    /// <summary>
    ///     The previous menus shown before the current lookup UI was opened.
    /// </summary>
    private readonly PerScreen<Stack<IClickableMenu>> _previousMenus = new(() => new());

    /// <summary>The mod entry point, called after the mod is first loaded.</summary>
    /// <param name="helper">
    ///     Provides methods for interacting with the mod directory, 
    ///     such as read/writing a config file or custom JSON files.
    /// </param>
    public override void Entry(IModHelper helper)
    {
        _chestFinder = new ChestFinder(helper.Multiplayer);

        // hook up events
        helper.Events.Display.MenuChanged += OnMenuChanged;
        helper.Events.Input.ButtonsChanged += OnButtonsChanged;
    }

    /// <inheritdoc cref="IDisplayEvents.MenuChanged"/>
    /// <param name="sender">The event sender.</param>
    /// <param name="e">The event data.</param>
    private void OnMenuChanged(object? sender, MenuChangedEventArgs e)
    {
        Monitor.Log("Restoring the previous menu");
        // restore the previous menu if it was hidden to show the lookup UI
        if (e.NewMenu == null
            && (e.OldMenu is ProductionMenu)
            && _previousMenus.Value.Count != 0)
        {
            Game1.activeClickableMenu = _previousMenus.Value.Pop();
        }
    }

    /// <inheritdoc cref="IInputEvents.ButtonsChanged"/>
    /// <param name="sender">The event sender.</param>
    /// <param name="e">The event data.</param>
    private void OnButtonsChanged(object? sender, ButtonsChangedEventArgs e)
    {
        var toggleMenu = new KeybindList(SButton.F5);
        var scrollUp = new KeybindList(SButton.Up);
        var scrollDown = new KeybindList(SButton.Down);

        if (toggleMenu.JustPressed())
        {
            ToggleMenu();
        }
        else if (scrollUp.JustPressed())
        {
            (Game1.activeClickableMenu as IScrollableMenu)?.ScrollUp();
        }
        else if (scrollDown.JustPressed())
        {
            (Game1.activeClickableMenu as IScrollableMenu)?.ScrollDown();
        }
    }

    private void ToggleMenu()
    {
        if (Game1.activeClickableMenu is ProductionMenu)
        {
            HideMenu();
            return;
        }
        ShowMenu();
    }

    private void ShowMenu()
    {
        Monitor.Log("Recieved a open menu request");
        try
        {
            // get target
            IEnumerable<ItemStock> items = GetItemSubjects();
            if (items.Any() == false)
            {
                Monitor.Log($"Items no target found.");
                return;
            }

            // show lookup UI
            Monitor.Log($"Found {items.Count()} items to show.");
            ShowMenuFor(items);
        }
        catch (Exception ex)
        {
            Monitor.Log($"An error occurred. {ex.Message}");
            throw;
        }
    }

    private void ShowMenuFor(IEnumerable<ItemStock> items)
    {
        items.Select(x => $"Showing {x.GetType().Name}::{x.Item.Name}.")
            .ToList()
            .ForEach(x => Monitor.Log(x));

        PushMenu(
            new ProductionMenu(
                itemStocks: items,
                monitor: Monitor,
                reflectionHelper: Helper.Reflection,
                scroll: 160,
                forceFullScreen: false
            )
        );
    }

    /// <summary>
    ///     Push a new menu onto the display stack, 
    ///     saving the previous menu if needed.
    /// </summary>
    /// <param name="menu">The menu to show.</param>
    private void PushMenu(IClickableMenu menu)
    {
        if (ShouldRestoreMenu(Game1.activeClickableMenu))
        {
            _previousMenus.Value.Push(Game1.activeClickableMenu);
            Helper
                .Reflection
                .GetField<IClickableMenu>(
                    typeof(Game1), "_activeClickableMenu")
                .SetValue(menu); // bypass Game1.activeClickableMenu, which disposes the previous menu
        }
        else
        {
            Game1.activeClickableMenu = menu;
        }
    }

    /// <summary>
    ///     Get whether a given menu should be restored 
    ///     when the lookup ends.
    /// </summary>
    /// <param name="menu">The menu to check.</param>
    private static bool ShouldRestoreMenu(IClickableMenu? menu)
    {
        return menu switch
        {
            null => false, // no menu
            ProductionMenu => false,
            _ => true,
        };
    }

    private IEnumerable<ItemStock> GetItemSubjects()
    {
        var items = _chestFinder.GetChests()
                .Select(x => x.GetItemsForCurrentPlayer())
                .SelectMany(x => x) // Make list flat 
                .Where(x => x is not null)
                .Concat(Game1.player.Items.Where(x => x is not null));

        var result = new Dictionary<string, ItemStock>();
        foreach (Item? item in items)
        {
            if (result.ContainsKey(item.Name) == false)
            {
                result[item.Name] = new ItemStock(item);
            }
            result[item.Name].Count += item.Stack;
        }

        return result.Values;
    }

    /// <summary>Hide the lookup UI for the current target.</summary>
    private static void HideMenu()
    {
        if (Game1.activeClickableMenu is ProductionMenu menu)
        {
            menu.QueueExit();
        }
    }
}
