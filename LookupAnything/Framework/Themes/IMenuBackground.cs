using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Pathoschild.Stardew.LookupAnything.Framework.Themes;

/// <summary>General menu background description</summary>
public interface IMenuBackground
{
    /// <summary>Background texture, <see cref="ThemeData.BackgroundTexture"/></summary>
    public Texture2D Texture { get; }

    /// <summary>Background source rectangle, <see cref="ThemeData.BackgroundSourceRect"/></summary>
    public Rectangle SourceRect { get; }

    /// <summary>Background primary color, <see cref="ThemeData.BackgroundPrimaryColor"/></summary>
    public Color PrimaryColor { get; }

    /// <summary>Background primary color, <see cref="ThemeData.BackgroundSecondaryColor"/></summary>
    public Color SecondaryColor { get; }

    /// <summary>Ratio of height divided by width</summary>
    public float AspectRatio { get; }

    /// <summary>Background draw implementation</summary>
    /// <param name="b">Sprite batch</param>
    /// <param name="x">screen x</param>
    /// <param name="y">screen y</param>
    /// <param name="width">screen width</param>
    /// <param name="height">screen height</param>
    public void DrawBackground(SpriteBatch b, int x, int y, int width, int height);
}
