
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Pathoschild.Stardew.ChestsAnywhere.Framework;
using StardewValley;
using StardewValley.Menus;

namespace Pathoschild.Stardew.ChestsAnywhere.Menus.Search;

internal class ChestSearchMenuCell(Rectangle bounds, string name) : ClickableComponent(bounds, name)
{
    internal const int Padding = 8;
    protected static readonly Rectangle MenuRectInset = new(0, 320, 60, 60);

    internal int BaseX { get; set; } = 0;
    internal int BaseY { get; set; } = 0;

    public void Reposition(int x, int y)
    {
        this.bounds.X = this.BaseX + x;
        this.bounds.Y = this.BaseY + y;
    }

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
    }
}
