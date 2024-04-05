// derived from code by Jesse Plamondon-Willard under MIT license: https://github.com/Pathoschild/StardewMods

using ProductionStats.Components;
using ProductionStats.Framework;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using StardewValley;
using StardewValley.Menus;

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

    /// <summary>
    ///     Sort options which can be applied to the menu and change order in 
    ///     which items are shown.
    /// </summary>
    private readonly Queue<SortOrder> _sortOrders = new(
    [
        SortOrder.None,
        SortOrder.AscendingByName,
        SortOrder.DescendingByName,
        SortOrder.AscendingByCount,
        SortOrder.DescendingByCount,
    ]);

    private ModConfig _config = null!; // Set in Entry;

    /// <summary>The configure key bindings.</summary>
    private ModConfigKeys _keys => _config.Controls;

    /// <summary>The mod entry point, called after the mod is first loaded.</summary>
    /// <param name="helper">
    ///     Provides methods for interacting with the mod directory, 
    ///     such as read/writing a config file or custom JSON files.
    /// </param>
    public override void Entry(IModHelper helper)
    {
        _config = helper.ReadConfig<ModConfig>();
        _chestFinder = new ChestFinder(helper.Multiplayer);

        // hook up events
        helper.Events.GameLoop.GameLaunched += OnGameLaunched;
        helper.Events.Display.MenuChanged += OnMenuChanged;
        helper.Events.Input.ButtonsChanged += OnButtonsChanged;
    }

    private void OnGameLaunched(object? sender, GameLaunchedEventArgs e)
    {
        // get Generic Mod Config Menu's API (if it's installed)
        var configMenu = Helper
            .ModRegistry
            .GetApi<IGenericModConfigMenuApi>("spacechase0.GenericModConfigMenu");

        if (configMenu is null)
            return;

        // register mod
        configMenu.Register(
            mod: ModManifest,
            reset: () => _config = new ModConfig(),
            save: () => Helper.WriteConfig(_config)
        );

        configMenu.AddSectionTitle(
            mod: ModManifest,
            text: () => "Controls",
            tooltip: () => "Section dedicated to interactions with this mod"
        );

        configMenu.AddKeybindList(
            mod: ModManifest,
            name: () => "Toggle menu",
            tooltip: () => "Toggles menu which display number of items in player's possesion",
            getValue: () => _config.Controls.ToggleMenu,
            setValue: value => _config.Controls.ToggleMenu = value
        );

        configMenu.AddKeybindList(
            mod: ModManifest,
            name: () => "Change sort order",
            tooltip: () => "Changes sorting order which is used to display player's item in menu",
            getValue: () => _config.Controls.Sort,
            setValue: value => _config.Controls.Sort = value
        );
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
        if (Context.IsWorldReady == false)
        {
            return;
        }

        if (_keys.ToggleMenu.JustPressed())
        {
            ToggleMenu();
        }
        else if (_keys.Sort.JustPressed())
        {
            Sort();
        }
        else if (_keys.FocusSearch.JustPressed())
        {
            FocusSearch();
        }
        else if (_keys.ScrollUp.JustPressed())
        {
            (Game1.activeClickableMenu as IScrollableMenu)?.ScrollUp();
        }
        else if (_keys.ScrollDown.JustPressed())
        {
            (Game1.activeClickableMenu as IScrollableMenu)?.ScrollDown();
        }
        else if (_keys.PageUp.JustPressed())
        {
            (Game1.activeClickableMenu as IScrollableMenu)?.ScrollUp(Game1.activeClickableMenu.height);
        }
        else if (_keys.PageDown.JustPressed())
        {
            (Game1.activeClickableMenu as IScrollableMenu)?.ScrollDown(Game1.activeClickableMenu.height);
        }
    }

    private void FocusSearch()
    {
        if (Game1.activeClickableMenu is not ProductionMenu menu)
        {
            Monitor.Log("Focus search can't be applied on this menu.");
            return;
        }

        menu.FocusSearch();
    }

    private void Sort()
    {
        if (Game1.activeClickableMenu is not ProductionMenu menu)
        {
            Monitor.Log("Sort can't be applied on this menu.");
            return;
        }

        if (menu.IsSearchTextBoxFocused)
        {
            return;
        }

        // sort items
        SortOrder sortOrder = _sortOrders.Dequeue();

        menu.ApplySort(sortOrder);
        HUDMessage message = new($"View sorted by {sortOrder.GetDescription()}", 500f)
        {
            noIcon = true,
        };

        Game1.addHUDMessage(message);
        _sortOrders.Enqueue(sortOrder);
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

        ProductionMenu menu = new(
            itemStocks: items,
            monitor: Monitor,
            reflectionHelper: Helper.Reflection,
            scroll: 160,
            forceFullScreen: false);

        PushMenu(menu);
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
            Helper.Reflection
                .GetField<IClickableMenu>(typeof(Game1), "_activeClickableMenu")
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
                .Concat(Game1.player.Items)
                .Where(x => x is not null);

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
            Game1.displayHUD = true;
        }
    }
}
