using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Pathoschild.Stardew.Common;
using Pathoschild.Stardew.LookupAnything.Framework;
using StardewValley;
using StardewValley.Menus;

namespace Pathoschild.Stardew.LookupAnything.Components;

/// <summary>Simplifies access to the game's sprite sheets.</summary>
/// <remarks>Each sprite is represented by a rectangle, which specifies the coordinates and dimensions of the image in the sprite sheet.</remarks>
internal static class Sprites
{
    /*********
    ** Accessors
    *********/
    /// <summary>Sprites used to draw a letter.</summary>
    public static class Letter
    {
        /// <summary>The sprite sheet containing the letter sprites.</summary>
        public static Texture2D Sheet => Game1.content.Load<Texture2D>("LooseSprites\\letterBG");

        /// <summary>Letter background source rect A (torn paper).</summary>
        private readonly static Rectangle Sprite_A = new(0, 0, 320, 180);
        /// <summary>Letter background source rect B (torn notepad).</summary>
        private readonly static Rectangle Sprite_B = new(320, 0, 320, 180);
        /// <summary>Letter background source rect C (magical).</summary>
        private readonly static Rectangle Sprite_C = new(640, 0, 320, 180);
        /// <summary>Letter background source rect D (krobus).</summary>
        private readonly static Rectangle Sprite_D = new(960, 0, 320, 180);
        /// <summary>Letter background source rect E (joja).</summary>
        private readonly static Rectangle Sprite_E = new(0, 204, 320, 180);

        /// <summary>The letter background (including edges and corners).</summary>
        public static Rectangle Sprite { get; private set; } = Sprite_A;

        /// <summary>Update letter sprite bounds according to config</summary>
        public static void UpdateSprite(MenuBackgroundOption option)
        {
            Sprite = option switch
            {
                MenuBackgroundOption.LetterBG_B => Sprite_B,
                MenuBackgroundOption.LetterBG_C => Sprite_C,
                MenuBackgroundOption.LetterBG_D => Sprite_D,
                MenuBackgroundOption.LetterBG_E => Sprite_E,
                _ => Sprite_A,
            };
        }
    }

    /// <summary>Sprites used to draw a textbox.</summary>
    public static class Textbox
    {
        /// <summary>The sprite sheet containing the textbox sprites.</summary>
        public static Texture2D Sheet => Game1.content.Load<Texture2D>("LooseSprites\\textBox");
    }

    /// <summary>A blank pixel which can be colorized and stretched to draw geometric shapes.</summary>
    public static readonly Texture2D Pixel = CommonHelper.Pixel;

    public static class MenuTiles
    {
        /// <summary>The sprite sheet containing the textbox sprites.</summary>
        public static Texture2D Sheet => Game1.content.Load<Texture2D>("Maps\\MenuTiles");
        /// <summary>Menu box source rect A (bordery).</summary>
        private readonly static Rectangle Sprite_A = new(0, 256, 60, 60);
        /// <summary>Menu box source rect B (inset).</summary>
        private readonly static Rectangle Sprite_B = new(0, 316, 60, 60);
        /// <summary>Menu box source rect B (raised).</summary>
        private readonly static Rectangle Sprite_C = new(60, 316, 60, 60);
        /// <summary>The letter background (including edges and corners).</summary>
        public static Rectangle Sprite { get; private set; } = Sprite_A;

        /// <summary>Update letter sprite bounds according to config</summary>
        public static void UpdateSprite(MenuBackgroundOption option)
        {
            Sprite = option switch
            {
                MenuBackgroundOption.MenuBox_B => Sprite_B,
                MenuBackgroundOption.MenuBox_C => Sprite_C,
                _ => Sprite_A,
            };
        }
    }

    /// <summary>Update sprite bounds according to config</summary>
    public static void UpdateSprite(MenuBackgroundOption option)
    {
        Letter.UpdateSprite(option);
        MenuTiles.UpdateSprite(option);
    }

    public static void DrawBackground(SpriteBatch backgroundBatch, int x, int y, int width, int height, MenuBackgroundOption bgOption)
    {
        if (bgOption.HasFlag(MenuBackgroundOption.LetterBG))
        {
            float scale = width >= height
                    ? width / (float)Letter.Sprite.Width
                    : height / (float)Letter.Sprite.Height;
            backgroundBatch.DrawSprite(Letter.Sheet, Letter.Sprite, x, y, Letter.Sprite.Size, scale: scale);
        }
        else if (bgOption.HasFlag(MenuBackgroundOption.MenuBox))
        {
            IClickableMenu.drawTextureBox(
                backgroundBatch, MenuTiles.Sheet, MenuTiles.Sprite,
                x, y,
                width,
                height,
                Color.White
            );
        }
        else
        {
            Utility.DrawSquare(backgroundBatch, new(x, y, width, height), 4, Color.BurlyWood, Color.Wheat);
        }
    }
}
