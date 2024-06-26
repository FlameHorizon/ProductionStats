﻿// derived from code by Jesse Plamondon-Willard under MIT license: https://github.com/Pathoschild/StardewMods

namespace ProductionStats.Components;

/// <summary>
///     A Lookup Anything menu which supports scrolling.
/// </summary>
internal interface IScrollableMenu
{
    /// <summary>
    ///     Scroll up the menu content by the specified amount (if possible).
    /// </summary>
    /// <param name="amount">
    ///     The amount to scroll, or <c>null</c> for a default amount.
    /// </param>
    void ScrollUp(int? amount = null);

    /// <summary>
    ///     Scroll down the menu content by the specified amount (if possible).
    /// </summary>
    /// <param name="amount">
    ///     The amount to scroll, or <c>null</c> for a default amount.
    /// </param>
    void ScrollDown(int? amount = null);
}