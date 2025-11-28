
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Pathoschild.Stardew.ChestsAnywhere.Framework;
using StardewValley;
using StardewValley.Menus;

namespace Pathoschild.Stardew.ChestsAnywhere.Menus.Components;

/// <summary>A single cell in the chest search menu</summary>
internal class ChestSearchMenuCell : ClickableComponent
{
    /*********
    ** Accessors
    *********/
    /// <summary>Const padding value</summary>
    private const int Padding = 8;
    /// <summary>A source rect in <see cref="Game1.menuTexture"/> to draw as border of this cell</summary>
    private static readonly Rectangle MenuRectInset = new(0, 320, 60, 60);

    /// <summary>Base X relative to the xPositionOnScreen of parent menu</summary>
    private readonly int BaseX;
    /// <summary>Base Y relative to the yPositionOnScreen of parent menu</summary>
    private readonly int BaseY;

    /*********
    ** Public methods
    *********/
    /// <summary>Construct an instance.</summary>
    /// <param name="bounds">Clickable bounds</param>
    /// <param name="name">Clickable component name</param>
    /// <param name="baseX">Base X position relative to xPositionOnScreen</param>
    /// <param name="baseY">Base Y position relative to yPositionOnScreen</param>
    public ChestSearchMenuCell(Rectangle bounds, string name, int baseX, int baseY) : base(bounds, name)
    {
        this.BaseX = baseX;
        this.BaseY = baseY;
    }

    /// <summary>Reposition this component according to new xPositionOnScreen/yPositionOnScreen</summary>
    /// <param name="xPositionOnScreen">New xPositionOnScreen</param>
    /// <param name="yPositionOnScreen">New yPositionOnScreen</param>
    public void Reposition(int xPositionOnScreen, int yPositionOnScreen)
    {
        this.bounds.X = this.BaseX + xPositionOnScreen;
        this.bounds.Y = this.BaseY + yPositionOnScreen;
    }

    /// <summary>Draw a managed chest that should occupy this component at this tick</summary>
    /// <param name="b">Sprite batch</param>
    /// <param name="chest">Managed chest instance</param>
    public virtual void Draw(SpriteBatch b, ManagedChest chest)
    {
        if (!this.visible)
        {
            return;
        }

        IClickableMenu.drawTextureBox(
            b,
            Game1.menuTexture,
            MenuRectInset,
            this.bounds.X + Padding / 2,
            this.bounds.Y + Padding / 2,
            this.bounds.Width - Padding,
            this.bounds.Height - Padding,
            Color.White,
            drawShadow: false
        );

        int offsetX = this.bounds.X + Padding * 2;
        if (chest.Container.TryGetIcon(out Texture2D? texture, out Rectangle sourceRect, out float scale))
        {
            b.Draw(texture, new Vector2(offsetX, this.bounds.Y + Padding), sourceRect, Color.White, 0, Vector2.Zero, scale / 2, SpriteEffects.None, layerDepth: 0.9f);
            offsetX += (int)(sourceRect.Width * scale / 2 + Padding);
        }
        else
        {
            offsetX += 16 + Padding;
        }

        Utility.drawTextWithShadow(
            b,
            chest.DisplayCategory,
            Game1.smallFont,
            new(
                offsetX,
                this.bounds.Y + Padding + 4
            ),
            Game1.textColor
        );
        Utility.drawTextWithShadow(
            b,
            chest.DisplayName,
            Game1.smallFont,
            new(
                offsetX,
                this.bounds.Y + Padding + Game1.smallFont.LineSpacing
            ),
            Game1.textColor
        );
        this.ScreenReaderText = $"{chest.DisplayCategory} {chest.DisplayName}";
    }
}
