﻿// derived from code by Jesse Plamondon-Willard under MIT license: https://github.com/Pathoschild/StardewMods

using System.ComponentModel;

namespace ProductionStats;

/// <summary>
/// Defines set of available sorting orders in the menu.
/// </summary>
internal enum SortOrder
{
    [Description("Name (desc)")]
    DescendingByName,

    [Description("Name (asc)")]
    AscendingByName,

    [Description("Count (desc)")]
    AscendingByCount,
    
    [Description("Count (asc)")]
    DescendingByCount,
    
    [Description("None")]
    None
}