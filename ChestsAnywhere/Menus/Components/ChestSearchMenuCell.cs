
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Pathoschild.Stardew.ChestsAnywhere.Framework;
using StardewValley;
using StardewValley.Menus;

namespace Pathoschild.Stardew.ChestsAnywhere.Menus.Components;

/// <summary>A chest result in the chest search menu.</summary>
internal class ChestSearchMenuCell : ClickableComponent
{
    /*********
    ** Fields
    *********/
    /// <summary>The pixel padding around each cell in the menu.</summary>
    private const int Padding = 8;

    /// <summary>The pixel area in the <see cref="Game1.menuTexture"/> to draw as borders for this cell.</summary>
    private static readonly Rectangle MenuRectInset = new(0, 320, 60, 60);

    /// <summary>The X pixel position at which to draw this cell, relative to the search menu's <see cref="IClickableMenu.xPositionOnScreen"/> value.</summary>
    private readonly int BaseX;

    /// <summary>The Y pixel position at which to draw this cell, relative to the search menu's <see cref="IClickableMenu.yPositionOnScreen"/> value.</summary>
    private readonly int BaseY;


    /*********
    ** Public methods
    *********/
    /// <summary>Construct an instance.</summary>
    /// <param name="bounds">The clickable bounds.</param>
    /// <param name="name">The component name.</param>
    /// <param name="baseX"><inheritdoc cref="BaseX" path="/summary"/></param>
    /// <param name="baseY"><inheritdoc cref="BaseY" path="/summary"/></param>
    public ChestSearchMenuCell(Rectangle bounds, string name, int baseX, int baseY)
        : base(bounds, name)
    {
        this.BaseX = baseX;
        this.BaseY = baseY;
    }

    /// <summary>Reposition this component to match the parent menu.</summary>
    /// <param name="xPositionOnScreen">The parent menu's <see cref="IClickableMenu.xPositionOnScreen"/> value.</param>
    /// <param name="yPositionOnScreen">The parent menu's <see cref="IClickableMenu.yPositionOnScreen"/> value.</param>
    public void Reposition(int xPositionOnScreen, int yPositionOnScreen)
    {
        this.bounds.X = this.BaseX + xPositionOnScreen;
        this.bounds.Y = this.BaseY + yPositionOnScreen;
    }

    /// <summary>Draw the cell to the screen.</summary>
    /// <param name="spriteBatch">The sprite batch being drawn.</param>
    /// <param name="chest">The chest to draw in the cell.</param>
    public virtual void Draw(SpriteBatch spriteBatch, ManagedChest chest)
    {
        if (!this.visible)
            return;

        IClickableMenu.drawTextureBox(
            b: spriteBatch,
            texture: Game1.menuTexture,
            sourceRect: MenuRectInset,
            x: this.bounds.X + Padding / 2,
            y: this.bounds.Y + Padding / 2,
            width: this.bounds.Width - Padding,
            height: this.bounds.Height - Padding,
            color: Color.White,
            drawShadow: false
        );

        int offsetX = this.bounds.X + Padding * 2;
        if (chest.Container.TryGetIcon(out Texture2D? texture, out Rectangle sourceRect, out float iconScale))
        {
            spriteBatch.Draw(texture, new Vector2(offsetX, this.bounds.Y + Padding), sourceRect, Color.White, 0, Vector2.Zero, iconScale / 2, SpriteEffects.None, layerDepth: 0.9f);
            offsetX += (int)(sourceRect.Width * iconScale / 2 + Padding);
        }
        else
            offsetX += 16 + Padding;

        Utility.drawTextWithShadow(
            b: spriteBatch,
            text: chest.DisplayCategory,
            font: Game1.smallFont,
            position: new(offsetX, this.bounds.Y + Padding + 4),
            color: Game1.textColor
        );
        Utility.drawTextWithShadow(
            b: spriteBatch,
            text: chest.DisplayName,
            font: Game1.smallFont,
            position: new(offsetX, this.bounds.Y + Padding + Game1.smallFont.LineSpacing),
            color: Game1.textColor
        );

        this.ScreenReaderText = $"{chest.DisplayCategory} {chest.DisplayName}";
    }
}
