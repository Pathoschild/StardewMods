using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

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
