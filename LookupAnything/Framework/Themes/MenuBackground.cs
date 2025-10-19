using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Pathoschild.Stardew.Common;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Menus;

namespace Pathoschild.Stardew.LookupAnything.Framework.Themes;

/// <summary>A normalized background to apply based on the raw theme data.</summary>
public class MenuBackground
{
    /*********
    ** Fields
    *********/
    /// <summary>The underlying theme data.</summary>
    private readonly ThemeData Data;


    /*********
    ** Accessors
    *********/
    /// <summary>The loaded background texture to draw.</summary>
    public Texture2D Texture { get; }

    /// <summary>The pixel area within the <see cref="Texture"/> to draw.</summary>
    public Rectangle SourceRect { get; }

    /// <summary>The background color tint.</summary>
    public Color BackgroundColor { get; }

    /// <summary>The border color tint.</summary>
    public Color BorderColor { get; }

    /// <summary>The aspect ratio of the <see cref="SourceRect"/>.</summary>
    public float AspectRatio { get; }


    /*********
    ** Public methods
    *********/
    /// <summary>Construct an instance.</summary>
    /// <param name="content">The game content API from which to load the background texture.</param>
    /// <param name="data">The underlying theme data.</param>
    public MenuBackground(IGameContentHelper content, ThemeData data)
    {
        this.Data = data;
        this.Texture = content.Load<Texture2D>(data.BackgroundTexture);
        this.SourceRect = data.BackgroundSourceRect.IsEmpty ? this.Texture.Bounds : data.BackgroundSourceRect;
        this.BackgroundColor = Utility.StringToColor(data.BackgroundColor) ?? Color.White;
        this.BorderColor = Utility.StringToColor(data.BorderColor) ?? Color.Black;
        this.AspectRatio = this.Data.BackgroundType == MenuBackgroundType.FixedSprite
            ? (float)this.SourceRect.Height / this.SourceRect.Width
            : this.AspectRatio = 180f / 320;
    }

    /// <summary>Draw the menu background to the screen.</summary>
    /// <param name="spriteBatch">The sprite batch being drawn.</param>
    /// <param name="x">The left X pixel position at which to start drawing the background.</param>
    /// <param name="y">The top Y pixel position at which to start drawing the background.</param>
    /// <param name="width">The pixel width within which to draw the background.</param>
    /// <param name="height">The pixel height within which to draw the background.</param>
    public void Draw(SpriteBatch spriteBatch, int x, int y, int width, int height)
    {
        int padding = this.Data.BackgroundPadding;
        x -= padding;
        width += padding * 2;

        switch (this.Data.BackgroundType)
        {
            case MenuBackgroundType.PlainColor:
                y -= padding;
                height += padding;
                Utility.DrawSquare(spriteBatch, new Rectangle(x, y, width, height), padding, this.BorderColor, this.BackgroundColor);
                break;

            case MenuBackgroundType.FixedSprite:
                {
                    y -= (int)(padding * this.AspectRatio);
                    height += (int)(padding * 2 * this.AspectRatio);
                    float scale = width >= height ? width / (float)this.SourceRect.Width : height / (float)this.SourceRect.Height;
                    spriteBatch.DrawSprite(this.Texture, this.SourceRect, x, y, this.SourceRect.Size, color: this.BackgroundColor, scale: scale);
                }
                break;

            case MenuBackgroundType.MenuBox:
                y -= padding;
                height += padding;
                IClickableMenu.drawTextureBox(spriteBatch, this.Texture, this.SourceRect, x, y, width, height, this.BackgroundColor);
                break;
        }
    }
}
