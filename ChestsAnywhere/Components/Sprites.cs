using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;

namespace ChestsAnywhere.Components
{
    /// <summary>Simplifies access to the game's sprite sheets.</summary>
    /// <remarks>Each sprite is represented by a rectangle, which specifies the coordinates and dimensions of the image in the sprite sheet.</remarks>
    internal static class Sprites
    {
        /*********
        ** Accessors
        *********/
        /// <summary>Sprites used to draw a tab.</summary>
        public static class Tab
        {
            /// <summary>The sprite sheet containing the tab sprites.</summary>
            public static readonly Texture2D Sheet = Game1.mouseCursors;

            /// <summary>The top-left corner.</summary>
            public static readonly Rectangle TopLeft = new Rectangle(16, 384, 4, 4);

            /// <summary>The top-right corner.</summary>
            public static readonly Rectangle TopRight = new Rectangle(28, 384, 4, 4);

            /// <summary>The bottom-left corner.</summary>
            public static readonly Rectangle BottomLeft = new Rectangle(16, 396, 4, 4);

            /// <summary>The bottom-right corner.</summary>
            public static readonly Rectangle BottomRight = new Rectangle(28, 396, 4, 4);

            /// <summary>The top edge.</summary>
            public static readonly Rectangle Top = new Rectangle(21, 384, 4, 4);

            /// <summary>The left edge.</summary>
            public static readonly Rectangle Left = new Rectangle(16, 389, 4, 4);

            /// <summary>The right edge.</summary>
            public static readonly Rectangle Right = new Rectangle(28, 389, 4, 4);

            /// <summary>The bottom edge.</summary>
            public static readonly Rectangle Bottom = new Rectangle(21, 396, 4, 4);

            /// <summary>The tab background.</summary>
            public static readonly Rectangle Background = new Rectangle(21, 373, 4, 4);
        }

        /// <summary>Sprites used to draw a dropdown list.</summary>
        public static class DropDown
        {
            /// <summary>The sprite sheet containing the menu sprites.</summary>
            public static readonly Texture2D Sheet = Game1.mouseCursors;

            /// <summary>The background for the selected item.</summary>
            public static readonly Rectangle ActiveBackground = new Rectangle(258, 258, 4, 4);

            /// <summary>The background for a non-selected, non-hovered item.</summary>
            public static readonly Rectangle InactiveBackground = new Rectangle(269, 258, 4, 4);

            /// <summary>The background for an item under the cursor.</summary>
            public static readonly Rectangle HoverBackground = new Rectangle(161, 340, 4, 4);
        }

        /// <summary>Sprites used to draw menus.</summary>
            public static class Menu
        {
            /// <summary>The sprite sheet containing the menu sprites.</summary>
            public static readonly Texture2D Sheet = Game1.menuTexture;

            /// <summary>The top-left corner.</summary>
            public static readonly Rectangle TopLeft = new Rectangle(12, 16, 32, 32);

            /// <summary>The top-right corner.</summary>
            public static readonly Rectangle TopRight = new Rectangle(212, 16, 32, 32);

            /// <summary>The bottom-left corner.</summary>
            public static readonly Rectangle BottomLeft = new Rectangle(12, 208, 32, 32);

            /// <summary>The bottom-right corner.</summary>
            public static readonly Rectangle BottomRight = new Rectangle(212, 208, 32, 32);

            /// <summary>The middle-left corner.</summary>
            public static readonly Rectangle MiddleLeft = new Rectangle(12, 80, 32, 32);

            /// <summary>The middle-middle corner.</summary>
            public static readonly Rectangle MiddleMiddle = new Rectangle(132, 80, 32, 32);

            /// <summary>The middle-right corner.</summary>
            public static readonly Rectangle MiddleRight = new Rectangle(212, 80, 32, 32);

            /// <summary>The top edge.</summary>
            public static readonly Rectangle Top = new Rectangle(40, 16, 32, 32);

            /// <summary>The left edge.</summary>
            public static readonly Rectangle Left = new Rectangle(12, 36, 32, 32);

            /// <summary>The right edge.</summary>
            public static readonly Rectangle Right = new Rectangle(212, 40, 32, 32);

            /// <summary>The bottom edge.</summary>
            public static readonly Rectangle Bottom = new Rectangle(36, 208, 32, 32);

            /// <summary>The menu background.</summary>
            public static readonly Rectangle Background = new Rectangle(64, 128, 64, 64);

            /// <summary>The square borders representing an inventory slot.</summary>
            public static readonly Rectangle Slot = new Rectangle(128, 128, 64, 64);

            /// <summary>A disabled variant of <see cref="Slot"/>.</summary>
            public static readonly Rectangle SlotDisabled = new Rectangle(64, 896, 64, 64);
        }

        /// <summary>Sprites used to draw buttons.</summary>
        public static class Buttons
        {
            /// <summary>The sprite sheet containing the button sprites.</summary>
            public static readonly Texture2D Sheet = Game1.mouseCursors;

            /// <summary>The inventory 'organize' button.</summary>
            public static readonly Rectangle Organize = new Rectangle(162, 440, 16, 16);
        }

        /// <summary>Sprites used to draw icons.</summary>
        public static class Icons
        {
            /// <summary>The sprite sheet containing the icon sprites.</summary>
            public static Texture2D Sheet => Game1.mouseCursors;

            /// <summary>A down arrow for scrolling content.</summary>
            public static readonly Rectangle DownArrow = new Rectangle(12, 76, 40, 44);

            /// <summary>An up arrow for scrolling content.</summary>
            public static readonly Rectangle UpArrow = new Rectangle(76, 72, 40, 44);
        }


        /*********
        ** Extensions
        *********/
        /// <summary>Draw a sprite to the screen.</summary>
        /// <param name="batch">The sprite batch.</param>
        /// <param name="sheet">The sprite sheet containing the sprite.</param>
        /// <param name="sprite">The sprite coordinates and dimensions in the sprite sheet.</param>
        /// <param name="x">The X-position at which to draw the sprite.</param>
        /// <param name="Y">The X-position at which to draw the sprite.</param>
        /// <param name="width">The width to draw.</param>
        /// <param name="height">The height to draw.</param>
        /// <param name="color">The color to tint the sprite.</param>
        public static void Draw(this SpriteBatch batch, Texture2D sheet, Rectangle sprite, int x, int y, int width, int height, Color? color = null)
        {
            batch.Draw(sheet, new Rectangle(x, y, width, height), sprite, color ?? Color.White);
        }
    }
}
