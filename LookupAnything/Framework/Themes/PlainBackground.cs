using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;

namespace Pathoschild.Stardew.LookupAnything.Framework.Themes;

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
