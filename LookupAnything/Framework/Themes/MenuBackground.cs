using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Pathoschild.Stardew.Common;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Menus;

namespace Pathoschild.Stardew.LookupAnything.Framework.Themes;


/// <summary>Standard menu background implementation</summary>
public class MenuBackground : IMenuBackground
{
    public Texture2D Texture { get; private set; }
    public Rectangle SourceRect { get; private set; }
    public Color PrimaryColor { get; private set; }
    public Color SecondaryColor { get; private set; }
    public float AspectRatio { get; private set; }

    /// <summary>Data for this background</summary>
    private readonly ThemeData Data;

    public MenuBackground(IGameContentHelper content, ThemeData data)
    {
        this.Data = data;
        this.Texture = content.Load<Texture2D>(data.BackgroundTexture);
        if (data.BackgroundSourceRect.IsEmpty)
        {
            this.SourceRect = this.Texture.Bounds;
        }
        else
        {
            this.SourceRect = data.BackgroundSourceRect;
        }
        this.PrimaryColor = Utility.StringToColor(data.BackgroundPrimaryColor) ?? Color.White;
        this.SecondaryColor = Utility.StringToColor(data.BackgroundSecondaryColor) ?? Color.Black;

        if (this.Data.BackgroundCategory == MenuBackgroundCategory.FixedSprite)
        {
            this.AspectRatio = (float)this.SourceRect.Height / this.SourceRect.Width;
        }
        else
        {
            this.AspectRatio = 180f / 320;
        }

    }

    public void DrawBackground(SpriteBatch b, int x, int y, int width, int height)
    {
        int bgPad = this.Data.BackgroundPadding;
        x -= bgPad;
        width += bgPad * 2;

        switch (this.Data.BackgroundCategory)
        {
            case MenuBackgroundCategory.PlainColor:
                y -= bgPad;
                height += bgPad;
                Utility.DrawSquare(b, new(x, y, width, height), bgPad, this.SecondaryColor, this.PrimaryColor);
                break;
            case MenuBackgroundCategory.FixedSprite:
                {
                    y -= (int)(bgPad * this.AspectRatio);
                    height += (int)(bgPad * 2 * this.AspectRatio);
                    float scale = width >= height ? width / (float)this.SourceRect.Width : height / (float)this.SourceRect.Height;
                    b.DrawSprite(
                        this.Texture, this.SourceRect,
                        x, y,
                        this.SourceRect.Size,
                        color: this.PrimaryColor,
                        scale: scale
                    );
                }
                break;
            case MenuBackgroundCategory.MenuBox:
                y -= bgPad;
                height += bgPad;
                IClickableMenu.drawTextureBox(
                    b,
                    this.Texture,
                    this.SourceRect,
                    x, y,
                    width,
                    height,
                    this.PrimaryColor
                );
                break;
        }
    }
}
