﻿// derived from code by Jesse Plamondon-Willard under MIT license: https://github.com/Pathoschild/StardewMods

using Microsoft.Xna.Framework;

namespace ProductionStats.Framework;

/// <summary>A snippet of formatted text.</summary>
internal struct FormattedText : IFormattedText
{
    /********
    ** Accessors
    *********/
    /// <summary>The text to format.</summary>
    public string? Text { get; }

    /// <summary>The font color (or <c>null</c> for the default color).</summary>
    public Color? Color { get; }

    /// <summary>Whether to draw bold text.</summary>
    public bool Bold { get; }


    /********
    ** Public methods
    *********/
    /// <summary>Construct an instance.</summary>
    /// <param name="text">The text to format.</param>
    /// <param name="color">The font color (or <c>null</c> for the default color).</param>
    /// <param name="bold">Whether to draw bold text.</param>
    public FormattedText(string? text, Color? color = null, bool bold = false)
    {
        Text = text;
        Color = color;
        Bold = bold;
    }
}