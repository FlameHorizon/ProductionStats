﻿// derived from code by Jesse Plamondon-Willard under MIT license: https://github.com/Pathoschild/StardewMods

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ProductionStats.Common;
using StardewValley;

namespace ProductionStats.Components;

/// <summary>Simplifies access to the game's sprite sheets.</summary>
/// <remarks>
///     Each sprite is represented by a rectangle, 
///     which specifies the coordinates and dimensions of the 
///     image in the sprite sheet.
/// </remarks>
internal static class Sprites
{
    /// <summary>Sprites used to draw a letter.</summary>
    public static class Letter
    {
        /// <summary>The sprite sheet containing the letter sprites.</summary>
        public static Texture2D Sheet => Game1.content.Load<Texture2D>("LooseSprites\\letterBG");

        /// <summary>The letter background (including edges and corners).</summary>
        public static readonly Rectangle Sprite = new(0, 0, 320, 180);
    }

    /// <summary>Sprites used to draw a textbox.</summary>
    public static class Textbox
    {
        /// <summary>The sprite sheet containing the textbox sprites.</summary>
        public static Texture2D Sheet => Game1.content.Load<Texture2D>("LooseSprites\\textBox");
    }

    /// <summary>
    ///     A blank pixel which can be colorized and stretched 
    ///     to draw geometric shapes.
    /// </summary>
    public static readonly Texture2D Pixel = CommonHelper.Pixel;
}
