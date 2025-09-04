using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.Menus;

namespace Pathoschild.Stardew.LookupAnything.Framework.Themes;

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
