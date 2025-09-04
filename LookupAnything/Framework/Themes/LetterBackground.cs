using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Pathoschild.Stardew.Common;
using StardewValley;

namespace Pathoschild.Stardew.LookupAnything.Framework.Themes;

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
