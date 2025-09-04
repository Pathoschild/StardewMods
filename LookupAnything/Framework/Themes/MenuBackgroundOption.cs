using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Pathoschild.Stardew.Common;
using StardewValley;
using StardewValley.Menus;

namespace Pathoschild.Stardew.LookupAnything.Framework.Themes;

/// <summary>General menu background description</summary>
public interface IMenuBackground
{
    /// <summary>Background texture</summary>
    public Texture2D Texture { get; }

    /// <summary>Background source rectangle</summary>
    public Rectangle SourceRect { get; }

    /// <summary>Background draw implementation</summary>
    /// <param name="b">Sprite batch</param>
    /// <param name="x">screen x</param>
    /// <param name="y">screen y</param>
    /// <param name="width">screen width</param>
    /// <param name="height">screen height</param>
    public void DrawBackground(SpriteBatch b, int x, int y, int width, int height);
}

/// <summary>Plain 2 color rectangle</summary>
/// <param name="background">the fill color</param>
/// <param name="border">the border color</param>
public sealed class PlainBackground(Color background, Color border) : IMenuBackground
{
    /// <summary>Background texture (not used)</summary>
    public Texture2D Texture => Game1.staminaRect;

    /// <summary>Background source rectangle (not used)</summary>
    public Rectangle SourceRect { get; } = Rectangle.Empty;

    /// <summary>Draw a bordered rectangle using <see cref="Utility.DrawSquare"/></summary>
    /// <param name="b">Sprite batch</param>
    /// <param name="x">screen x</param>
    /// <param name="y">screen y</param>
    /// <param name="width">screen width</param>
    /// <param name="height">screen height</param>
    public void DrawBackground(SpriteBatch b, int x, int y, int width, int height)
    {
        Utility.DrawSquare(b, new(x, y, width, height), 4, border, background);
    }
}

/// <summary>LetterBG style background</summary>
/// <param name="x">source rect x</param>
/// <param name="y">source rect y</param>
/// <param name="texturePath">asset path to letter texture</param>
internal sealed class LetterBackground(int x, int y, string texturePath = "LooseSprites\\letterBG") : IMenuBackground
{
    /// <summary>Standard width of letter texture</summary>
    public const int WIDTH = 320;
    /// <summary>Standard height of letter texture</summary>
    public const int HEIGHT = 180;

    /// <summary>Background texture, default LooseSprites/letterBG</summary>
    public Texture2D Texture => Game1.content.Load<Texture2D>(texturePath);

    /// <summary>Background source rectangle</summary>
    public Rectangle SourceRect { get; set; } = new(x, y, WIDTH, HEIGHT);

    /// <summary>Draw a letter background using <see cref="DrawHelper.DrawSprite"/></summary>
    /// <param name="b">Sprite batch</param>
    /// <param name="x">screen x</param>
    /// <param name="y">screen y</param>
    /// <param name="width">screen width</param>
    /// <param name="height">screen height</param>
    public void DrawBackground(SpriteBatch b, int x, int y, int width, int height)
    {
        float scale = width >= height
                ? width / (float)this.SourceRect.Width
                : height / (float)this.SourceRect.Height;
        b.DrawSprite(this.Texture, this.SourceRect, x, y, this.SourceRect.Size, scale: scale);
    }
}

/// <summary>MenuBox style background</summary>
/// <param name="x">source rect x</param>
/// <param name="y">source rect y</param>
/// <param name="texturePath">asset path to MenuTiles texture</param>
internal sealed class MenuBoxBackground(int x, int y, string texturePath = "Maps\\MenuTiles") : IMenuBackground
{
    /// <summary>Standard width of one menu box border texture</summary>
    public const int WIDTH = 60;
    /// <summary>Standard height of one menu box border texture</summary>
    public const int HEIGHT = 60;

    /// <summary>Background texture, default Maps/MenuTiles</summary>
    public Texture2D Texture => Game1.content.Load<Texture2D>(texturePath);

    /// <summary>Background source rectangle</summary>
    public Rectangle SourceRect { get; set; } = new(x, y, WIDTH, HEIGHT);

    /// <summary>Draw a letter background using <see cref="IClickableMenu.drawTextureBox"/></summary>
    /// <param name="b">Sprite batch</param>
    /// <param name="x">screen x</param>
    /// <param name="y">screen y</param>
    /// <param name="width">screen width</param>
    /// <param name="height">screen height</param>
    public void DrawBackground(SpriteBatch b, int x, int y, int width, int height)
    {
        IClickableMenu.drawTextureBox(
            b,
            this.Texture, this.SourceRect,
            x, y,
            width,
            height,
            Color.White
        );
    }
}
