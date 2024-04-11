﻿// derived from code by Jesse Plamondon-Willard under MIT license: https://github.com/Pathoschild/StardewMods

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using ProductionStats.Common;
using ProductionStats.Common.UI;
using ProductionStats.Framework;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Menus;

namespace ProductionStats.Components;

internal class ProductionMenu : BaseMenu, IScrollableMenu, IDisposable
{
    private readonly IEnumerable<ItemStock> _production;
    private readonly string _title = string.Empty;
    private readonly IMonitor _monitor;
    private readonly IReflectionHelper _reflection;
    private readonly int _scrollAmount;
    private readonly bool _forceFullScreen;

    /// <summary>
    /// The clickable 'scroll up' icon.
    /// </summary>
    private readonly ClickableTextureComponent _scrollUpButton;

    /// <summary>
    /// The clickable 'scroll down' icon.
    /// </summary>
    private readonly ClickableTextureComponent _scrollDownButton;

    /// <summary>
    /// The clickable 'go back' icon.
    /// </summary>
    private readonly ClickableTextureComponent _previousPageButton;

    /// <summary>
    /// The clickable 'go next' icon.
    /// </summary>
    private readonly ClickableTextureComponent _nextPageButton;

    /// <summary>The aspect ratio of the page background.</summary>
    private readonly Vector2 _aspectRatio = new(
        Sprites.Letter.Sprite.Width,
        Sprites.Letter.Sprite.Height);

    /// <summary>The spacing around the scroll buttons.</summary>
    private readonly int _scrollButtonGutter = 15;

    /// <summary>The search input box.</summary>
    private readonly SearchTextBox _searchTextBox;

    /// <summary>
    /// The current search results.
    /// </summary>
    private IEnumerable<ItemStock> _searchResults = [];

    /// <summary>
    ///     Whether to exit the menu on the next update tick.
    /// </summary>
    private bool _exitOnNextTick;

    /// <summary>The number of pixels to scroll.</summary>
    private int _currentScroll;

    /// <summary>
    ///     Whether the game's draw mode has been validated for compatibility.
    /// </summary>
    private bool _validatedDrawMode;

    /// <summary>The maximum pixels to scroll.</summary>
    private int _maxScroll;

    /// <summary>
    ///     The blend state to use when rendering the content sprite batch.
    /// </summary>
    private readonly BlendState _contentBlendState = new()
    {
        AlphaBlendFunction = BlendFunction.Add,
        AlphaSourceBlend = Blend.Zero,
        AlphaDestinationBlend = Blend.One,

        ColorBlendFunction = BlendFunction.Add,
        ColorSourceBlend = Blend.SourceAlpha,
        ColorDestinationBlend = Blend.InverseSourceAlpha
    };

    /// <summary>
    /// Raises an event when metric page has 
    /// been changed to previous (left arrow).
    /// </summary>
    public EventHandler<ChangedPageArgs>? ChangedToPreviousPage;

    /// <summary>
    /// Raises an event when metric page has 
    /// been changed to next (right arrow).
    /// </summary>
    public EventHandler<ChangedPageArgs>? ChangedToNextPage;

    /// <summary>
    /// Raises an event when metric page is closing.
    /// </summary>
    public EventHandler? Closing;

    public ProductionMenu(
        IEnumerable<ItemStock> production,
        string title,
        IMonitor monitor,
        IReflectionHelper reflectionHelper,
        int scroll,
        bool forceFullScreen)
    {
        _production = production;
        _title = title;
        _monitor = monitor;
        _reflection = reflectionHelper;
        _scrollAmount = scroll;
        _forceFullScreen = forceFullScreen;

        // create search textbox
        _searchTextBox = new SearchTextBox(Game1.smallFont, Color.Black);

        // add scroll buttons
        _scrollUpButton = new ClickableTextureComponent(
            bounds: Rectangle.Empty,
            texture: CommonSprites.Icons.Sheet,
            sourceRect: CommonSprites.Icons.UpArrow,
            scale: 1);

        _scrollDownButton = new ClickableTextureComponent(
            Rectangle.Empty,
            CommonSprites.Icons.Sheet,
            CommonSprites.Icons.DownArrow,
            1);

        _previousPageButton = new ClickableTextureComponent(
            bounds: Rectangle.Empty,
            texture: CommonSprites.Icons.Sheet,
            sourceRect: CommonSprites.Icons.LeftArrow,
            scale: 1);

        _nextPageButton = new ClickableTextureComponent(
            Rectangle.Empty,
            CommonSprites.Icons.Sheet,
            CommonSprites.Icons.RightArrow,
            scale: 1);

        UpdateLayout();
        _searchTextBox.OnChanged += (_, text) => ReceiveSearchTextboxChanged(text);
    }

    private void ReceiveSearchTextboxChanged(string search)
    {
        // get search words
        string[] words = (search ?? "").Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
        if (!words.Any())
        {
            _searchResults = _production;
            return;
        }

        // get results
        _searchResults = _production
            .Where(entry => words.All(word => entry.Item.DisplayName.IndexOf(word, StringComparison.OrdinalIgnoreCase) >= 0))
            .Select(entry => (entry,
                words.Select(word => entry.Item.DisplayName.IndexOf(word, StringComparison.OrdinalIgnoreCase))
                .Min()))
            .OrderBy(x => x.Item2)
            .Select(x => x.entry)
            .ToList();
    }

    private void UpdateLayout()
    {
        Point viewport = GetViewportSize();

        // update size & position
        if (_forceFullScreen)
        {
            xPositionOnScreen = 0;
            yPositionOnScreen = 0;
            width = viewport.X;
            height = viewport.Y;
        }
        else
        {
            width = Math.Min(Game1.tileSize * 20, viewport.X);
            height = Math.Min((int)(_aspectRatio.Y / _aspectRatio.X * width), viewport.Y);

            // derived from Utility.getTopLeftPositionForCenteringOnScreen,
            // adjusted to account for possibly different GPU viewport size.
            Vector2 origin = new(viewport.X / 2 - width / 2,
                viewport.Y / 2 - height / 2);
            xPositionOnScreen = (int)origin.X;
            yPositionOnScreen = (int)origin.Y;
        }

        // update up/down buttons
        int x = xPositionOnScreen;
        int y = yPositionOnScreen;
        int gutter = _scrollButtonGutter;
        float contentHeight = height - gutter * 2;

        _scrollUpButton.bounds = new Rectangle(
            x: x + gutter,
            y: (int)(y + contentHeight - CommonSprites.Icons.UpArrow.Height - gutter - CommonSprites.Icons.DownArrow.Height),
            width: CommonSprites.Icons.UpArrow.Height,
            height: CommonSprites.Icons.UpArrow.Width);

        _scrollDownButton.bounds = new Rectangle(
            x: x + gutter,
            y: (int)(y + contentHeight - CommonSprites.Icons.DownArrow.Height),
            width: CommonSprites.Icons.DownArrow.Height,
            height: CommonSprites.Icons.DownArrow.Width);
    }

    /// <summary>
    /// Indicates if we are filtering or not. If we are, they disable sorting.
    /// </summary>
    public bool IsFiltering => string.IsNullOrEmpty(_searchTextBox.Text) == false;

    public bool IsSearchTextBoxFocused => _searchTextBox.Selected;

    public override void receiveKeyPress(Keys key)
    {
        if (key.Equals(Keys.Escape))
        {
            if (IsSearchTextBoxFocused)
            {
                _searchTextBox.Deselect();
            }
            else
            {
                exitThisMenu();
                Closing?.Invoke(this, EventArgs.Empty);
            }
        }
    }

    /// <summary>The method invoked when the player left-clicks on the lookup UI.</summary>
    /// <param name="x">The X-position of the cursor.</param>
    /// <param name="y">The Y-position of the cursor.</param>
    /// <param name="playSound">Whether to enable sound.</param>
    public override void receiveLeftClick(int x, int y, bool playSound = true)
    {
        HandleLeftClick(x, y);
    }

    /// <summary>Handle a left-click from the player's mouse or controller.</summary>
    /// <param name="x">The x-position of the cursor.</param>
    /// <param name="y">The y-position of the cursor.</param>
    public void HandleLeftClick(int x, int y)
    {
        if (_searchTextBox.Bounds.Contains(x, y))
        {
            _searchTextBox.Select();
        }
        else
        {
            _searchTextBox.Deselect();
        }

        if (_previousPageButton.bounds.Contains(x, y))
        {
            PreviousMetric();
        }
        else if (_nextPageButton.bounds.Contains(x, y))
        {
            NextMetric();
        }

        // close menu when clicked outside
        if (isWithinBounds(x, y) == false)
        {
            exitThisMenu();
        }
        // scroll up or down
        else if (_scrollUpButton.containsPoint(x, y))
        {
            ScrollUp();
        }
        else if (_scrollDownButton.containsPoint(x, y))
        {
            ScrollDown();
        }
    }

    private void NextMetric()
    {
        ChangedToNextPage?.Invoke(this, new ChangedPageArgs(_title));
    }

    private void PreviousMetric()
    {
        ChangedToPreviousPage?.Invoke(this, new ChangedPageArgs(_title));
    }

    /// <summary>The method invoked when the player right-clicks on the lookup UI.</summary>
    /// <param name="x">The X-position of the cursor.</param>
    /// <param name="y">The Y-position of the cursor.</param>
    /// <param name="playSound">Whether to enable sound.</param>
    public override void receiveRightClick(int x, int y, bool playSound = true) { }

    /// <summary>The method invoked when the player scrolls the mouse wheel on the lookup UI.</summary>
    /// <param name="direction">The scroll direction.</param>
    public override void receiveScrollWheelAction(int direction)
    {
        if (direction > 0)    // positive number scrolls content up
        {
            ScrollUp();
        }
        else
        {
            ScrollDown();
        }
    }

    /// <summary>The method called when the game window changes size.</summary>
    /// <param name="oldBounds">The former viewport.</param>
    /// <param name="newBounds">The new viewport.</param>
    public override void gameWindowSizeChanged(Rectangle oldBounds, Rectangle newBounds)
    {
        UpdateLayout();
    }

    /// <summary>The method called when the player presses a controller button.</summary>
    /// <param name="button">The controller button pressed.</param>
    public override void receiveGamePadButton(Buttons button)
    {
        switch (button)
        {
            // left click
            case Buttons.A:
                Point p = Game1.getMousePosition();
                HandleLeftClick(p.X, p.Y);
                break;

            // exit
            case Buttons.B:
                exitThisMenu();
                break;

            // scroll up
            case Buttons.RightThumbstickUp:
                ScrollUp();
                break;

            // scroll down
            case Buttons.RightThumbstickDown:
                ScrollDown();
                break;
        }
    }

    /// <summary>Render the UI.</summary>
    /// <param name="spriteBatch">The sprite batch being drawn.</param>
    public override void draw(SpriteBatch spriteBatch)
    {
        // disable when game is using immediate sprite sorting
        // (This prevents Lookup Anything from creating new sprite batches, which breaks its core rendering logic.
        // Fortunately this very rarely happens; the only known case is the Stardew Valley Fair, when the only thing
        // you can look up anyway is the farmer.)
        if (_validatedDrawMode == false)
        {
            IReflectedField<SpriteSortMode> sortModeField = _reflection
                .GetField<SpriteSortMode>(Game1.spriteBatch, "_sortMode");

            if (sortModeField.GetValue() == SpriteSortMode.Immediate)
            {
                _monitor.Log("Aborted the lookup because the game's " +
                    "current rendering mode isn't compatible with the mod's UI. " +
                    "This only happens in rare cases (e.g. the Stardew Valley Fair).",
                    LogLevel.Warn);

                exitThisMenu(playSound: false);
                return;
            }
            _validatedDrawMode = true;
        }

        // calculate dimensions
        int x = xPositionOnScreen;
        int y = yPositionOnScreen;
        const int gutter = 15;
        float leftOffset = gutter;
        float topOffset = gutter;
        float contentWidth = width - gutter * 2;
        float contentHeight = height - gutter * 2;

        // I'm going to leave this as in the future we might
        // actually want to draw borders again.
        //int tableBorderWidth = 1;

        // get font
        SpriteFont font = Game1.smallFont;
        float lineHeight = font.MeasureString("ABC").Y;
        float spaceWidth = DrawHelper.GetSpaceWidth(font);

        // draw background
        // (This uses a separate sprite batch because it needs to be drawn before the
        // foreground batch, and we can't use the foreground batch because the background is
        // outside the clipping area.)
        using (SpriteBatch backgroundBatch = new(Game1.graphics.GraphicsDevice))
        {
            float scale = width >= height
                ? width / (float)Sprites.Letter.Sprite.Width
                : height / (float)Sprites.Letter.Sprite.Height;

            backgroundBatch.Begin(
                SpriteSortMode.Deferred,
                BlendState.NonPremultiplied,
                SamplerState.PointClamp);

            backgroundBatch.DrawSprite(
                Sprites.Letter.Sheet,
                Sprites.Letter.Sprite,
                x,
                y,
                scale: scale);

            backgroundBatch.End();
        }

        // draw foreground
        // (This uses a separate sprite batch to set a clipping area for scrolling.)
        using (SpriteBatch contentBatch = new(Game1.graphics.GraphicsDevice))
        {
            GraphicsDevice device = Game1.graphics.GraphicsDevice;
            Rectangle prevScissorRectangle = device.ScissorRectangle;
            try
            {
                // begin draw
                device.ScissorRectangle = new Rectangle(
                    x: x + gutter,
                    y: y + gutter,
                    width: (int)contentWidth,
                    height: (int)contentHeight);

                contentBatch.Begin(
                    sortMode: SpriteSortMode.Deferred,
                    blendState: _contentBlendState,
                    samplerState: SamplerState.PointClamp,
                    depthStencilState: null,
                    rasterizerState: new RasterizerState { ScissorTestEnable = true });

                // scroll view
                // don't scroll past top
                _currentScroll = Math.Max(0, _currentScroll);

                // don't scroll past bottom
                _currentScroll = Math.Min(_maxScroll, _currentScroll);

                // scrolled down == move text up
                topOffset -= _currentScroll;
                float wrapWidth = width - leftOffset - gutter;
                _searchTextBox.Bounds = new Rectangle(
                            x: x + (int)leftOffset,
                            y: y + (int)topOffset,
                            width: (int)wrapWidth,
                            height: _searchTextBox.Bounds.Height);

                // draw search box
                _searchTextBox.Draw(contentBatch);
                topOffset += _searchTextBox.Bounds.Height + 15;

                var stringMeasure = font.MeasureString(_title);
                var centerX = (Game1.viewport.Width / 2) - (stringMeasure.X / 2);

                var titleBounds = contentBatch.DrawTextBlock(
                    font,
                    _title,
                    new(centerX, y + topOffset),
                    wrapWidth);

                // draw left / right arrows
                _previousPageButton.bounds = new Rectangle(
                    x: (Game1.viewport.Width / 2) - CommonSprites.Icons.LeftArrow.Width - 160,
                    y: y + (int)topOffset,
                    width: CommonSprites.Icons.LeftArrow.Width,
                    height: CommonSprites.Icons.LeftArrow.Height);

                _nextPageButton.bounds = new Rectangle(
                    x: (Game1.viewport.Width / 2) + 160,
                    y: y + (int)topOffset,
                    width: CommonSprites.Icons.LeftArrow.Width,
                    height: CommonSprites.Icons.LeftArrow.Height);

                _previousPageButton.draw(contentBatch);
                _nextPageButton.draw(contentBatch);

                topOffset += titleBounds.Y + 15;

                IEnumerable<ItemStock> items = IsFiltering
                    ? _searchResults ?? _production
                    : _production;

                bool leftSide = true;
                foreach (ItemStock stock in items)
                {
                    leftOffset = leftSide ? gutter : leftOffset + 500;

                    stock.Item.drawInMenu(
                        spriteBatch: contentBatch,
                        location: new Vector2(x + leftOffset, y + topOffset),
                        scaleSize: 1,
                        transparency: 1f,
                        layerDepth: 1f,
                        drawStackNumber: StackDrawType.Hide,
                        color: Color.White,
                        drawShadow: false);

                    leftOffset += 80;

                    // drawing item count
                    string item = $"{stock.Count}x ";
                    Vector2 itemCountPosition = new(x + leftOffset, y + topOffset + 15);
                    Vector2 itemCountSize = contentBatch.DrawTextBlock(
                            font: font,
                            text: $"{item}",
                            position: itemCountPosition,
                            wrapWidth: wrapWidth);

                    // drawing item name
                    Vector2 namePosition = new(
                        x + leftOffset + itemCountSize.X + spaceWidth,
                        y + topOffset + 15);

                    Vector2 nameSize = contentBatch.DrawTextBlock(
                        font: font,
                        text: $"{stock.Item.DisplayName}",
                        position: namePosition,
                        wrapWidth: wrapWidth,
                        bold: Constant.AllowBold);

                    // when last item was drawn on the right side,
                    // go to next row below and set column left as current.
                    if (leftSide == false)
                    {
                        topOffset += Math.Max(nameSize.Y, itemCountSize.Y);

                        // draw spacer
                        topOffset += lineHeight;
                    }

                    leftSide = !leftSide;
                }

                // update max scroll
                _maxScroll = Math.Max(0, (int)(topOffset - contentHeight + _currentScroll));

                // draw scroll icons
                if (_maxScroll > 0 && _currentScroll > 0)
                {
                    _scrollUpButton.draw(spriteBatch);
                }

                if (_maxScroll > 0 && _currentScroll < _maxScroll)
                {
                    _scrollDownButton.draw(spriteBatch);
                }

                // end draw
                contentBatch.End();
            }

            catch (ArgumentException ex) when (
                !UseSafeDimensions
                && ex.ParamName == "value"
                && ex.StackTrace?.Contains("Microsoft.Xna.Framework.Graphics.GraphicsDevice.set_ScissorRectangle") == true)
            {
                _monitor.Log("The viewport size seems to be inaccurate. " +
                    "Enabling compatibility mode; lookup menu may be misaligned.",
                    LogLevel.Warn);

                _monitor.Log(ex.ToString());
                UseSafeDimensions = true;
                UpdateLayout();
            }
            finally
            {
                device.ScissorRectangle = prevScissorRectangle;
            }
        }

        // draw cursor
        drawMouse(Game1.spriteBatch);
    }

    /// <summary>Exit the menu at the next safe opportunity.</summary>
    /// <remarks>
    ///     This circumvents an issue where the game may freeze in 
    ///     some cases like the load selection screen when the menu is 
    ///     exited at an arbitrary time.
    /// </remarks>
    public void QueueExit()
    {
        _exitOnNextTick = true;
    }

    /// <inheritdoc />
    public void ScrollUp(int? amount = null)
    {
        _currentScroll -= amount ?? _scrollAmount;
    }

    /// <inheritdoc />
    public void ScrollDown(int? amount = null)
    {
        _currentScroll += amount ?? _scrollAmount;
    }

    /// <summary>Clean up after the menu when it's disposed.</summary>
    public void Dispose()
    {
        _searchTextBox.Dispose();
        _contentBlendState.Dispose();
    }

    internal void FocusSearch() => _searchTextBox.Select();

    /// <summary>Update the menu state if needed.</summary>
    /// <param name="time">The elapsed game time.</param>
    public override void update(GameTime time)
    {
        if (_exitOnNextTick && readyToClose())
        {
            exitThisMenu();
        }
        else
        {
            base.update(time);
        }
    }
}
