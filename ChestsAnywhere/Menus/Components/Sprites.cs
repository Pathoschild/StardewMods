﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;

namespace Pathoschild.Stardew.ChestsAnywhere.Menus.Components
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
            public static readonly Rectangle TopLeft = new Rectangle(0, 384, 5, 5);

            /// <summary>The top-right corner.</summary>
            public static readonly Rectangle TopRight = new Rectangle(11, 384, 5, 5);

            /// <summary>The bottom-left corner.</summary>
            public static readonly Rectangle BottomLeft = new Rectangle(0, 395, 5, 5);

            /// <summary>The bottom-right corner.</summary>
            public static readonly Rectangle BottomRight = new Rectangle(11, 395, 5, 5);

            /// <summary>The top edge.</summary>
            public static readonly Rectangle Top = new Rectangle(4, 384, 1, 3);

            /// <summary>The left edge.</summary>
            public static readonly Rectangle Left = new Rectangle(0, 388, 3, 1);

            /// <summary>The right edge.</summary>
            public static readonly Rectangle Right = new Rectangle(13, 388, 3, 1);

            /// <summary>The bottom edge.</summary>
            public static readonly Rectangle Bottom = new Rectangle(4, 397, 1, 3);

            /// <summary>The tab background.</summary>
            public static readonly Rectangle Background = new Rectangle(5, 387, 1, 1);
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
            public static readonly Rectangle TopLeft = new Rectangle(12, 12, 24, 24);

            /// <summary>The top-right corner.</summary>
            public static readonly Rectangle TopRight = new Rectangle(220, 12, 24, 24);

            /// <summary>The bottom-left corner.</summary>
            public static readonly Rectangle BottomLeft = new Rectangle(12, 220, 24, 24);

            /// <summary>The bottom-right corner.</summary>
            public static readonly Rectangle BottomRight = new Rectangle(220, 220, 24, 24);

            /// <summary>The middle-left corner.</summary>
            public static readonly Rectangle MiddleLeft = new Rectangle(12, 84, 24, 24);

            /// <summary>The middle-right corner.</summary>
            public static readonly Rectangle MiddleRight = new Rectangle(220, 84, 24, 24);

            /// <summary>The top border.</summary>
            public static readonly Rectangle Top = new Rectangle(40, 12, 1, 24);

            /// <summary>The left border.</summary>
            public static readonly Rectangle Left = new Rectangle(12, 36, 24, 1);

            /// <summary>The right border.</summary>
            public static readonly Rectangle Right = new Rectangle(220, 40, 24, 1);

            /// <summary>The bottom border.</summary>
            public static readonly Rectangle Bottom = new Rectangle(36, 220, 1, 24);

            /// <summary>The middle border.</summary>
            public static readonly Rectangle Middle = new Rectangle(132, 84, 1, 24);

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

            public static readonly Rectangle Stack = new Rectangle(108, 491, 16, 16);
        }

        /// <summary>Sprites used to draw a textbox.</summary>
        public static class Textbox
        {
            /// <summary>The sprite sheet containing the textbox sprites.</summary>
            public static Texture2D Sheet => Game1.content.Load<Texture2D>("LooseSprites\\textBox");
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

            /// <summary>A speech bubble icon.</summary>
            public static readonly Rectangle SpeechBubble = new Rectangle(66, 4, 14, 12);

            /// <summary>An empty checkbox icon.</summary>
            public static readonly Rectangle EmptyCheckbox = new Rectangle(227, 425, 9, 9);

            /// <summary>A filled checkbox icon.</summary>
            public static readonly Rectangle FilledCheckbox = new Rectangle(236, 425, 9, 9);

            /// <summary>An exit button.</summary>
            public static readonly Rectangle ExitButton = new Rectangle(337, 494, 12, 12);
        }


        /*********
        ** Extensions
        *********/
        /// <summary>Draw a sprite to the screen.</summary>
        /// <param name="batch">The sprite batch.</param>
        /// <param name="sheet">The sprite sheet containing the sprite.</param>
        /// <param name="sprite">The sprite coordinates and dimensions in the sprite sheet.</param>
        /// <param name="x">The X-position at which to draw the sprite.</param>
        /// <param name="y">The X-position at which to draw the sprite.</param>
        /// <param name="width">The width to draw.</param>
        /// <param name="height">The height to draw.</param>
        /// <param name="color">The color to tint the sprite.</param>
        public static void Draw(this SpriteBatch batch, Texture2D sheet, Rectangle sprite, int x, int y, int width, int height, Color? color = null)
        {
            batch.Draw(sheet, new Rectangle(x, y, width, height), sprite, color ?? Color.White);
        }

        /// <summary>Draw a menu background within the specified bounds.</summary>
        /// <param name="batch">The sprite batch.</param>
        /// <param name="bounds">The menu bounds.</param>
        /// <param name="bisect">Whether to bisect into two stacked areas.</param>
        public static void DrawMenuBackground(this SpriteBatch batch, Rectangle bounds, bool bisect = false)
        {
            // alias sprites to simplify code
            var sheet = Sprites.Menu.Sheet;
            var top = Sprites.Menu.Top;
            var topRight = Sprites.Menu.TopRight;
            var right = Sprites.Menu.Right;
            var bottomRight = Sprites.Menu.BottomRight;
            var bottom = Sprites.Menu.Bottom;
            var bottomLeft = Sprites.Menu.BottomLeft;
            var left = Sprites.Menu.Left;
            var topLeft = Sprites.Menu.TopLeft;
            var middleLeft = Sprites.Menu.MiddleLeft;
            var middle = Sprites.Menu.Middle;
            var middleRight = Sprites.Menu.MiddleRight;
            
            // draw background
            batch.Draw(sheet, Sprites.Menu.Background, bounds.X + left.Width, bounds.Y + top.Height, bounds.Width - left.Width - right.Width, bounds.Height - top.Height - bottom.Height);

            // draw borders
            batch.Draw(sheet, top, bounds.X + topLeft.Width, bounds.Y, bounds.Width - topLeft.Width - topRight.Width, top.Height);
            batch.Draw(sheet, left, bounds.X, bounds.Y + topLeft.Height, left.Width, bounds.Height - topLeft.Height - bottomLeft.Height);
            batch.Draw(sheet, right, bounds.X + bounds.Width - right.Width, bounds.Y + topRight.Height, right.Width, bounds.Height - topRight.Height - bottomRight.Height);
            if (bisect)
                batch.Draw(sheet, middle, bounds.X + middleLeft.Width, bounds.Y + bounds.Height / 2 - middle.Height / 2, bounds.Width - middleLeft.Width - middleRight.Width, middleRight.Height);
            batch.Draw(sheet, bottom, bounds.X + bottomLeft.Width, bounds.Y + bounds.Height - bottom.Height, bounds.Width - bottomLeft.Width - bottomRight.Width, bottom.Height);

            // draw border joints
            batch.Draw(sheet, topLeft, bounds.X, bounds.Y, topLeft.Width, topLeft.Height);
            batch.Draw(sheet, topRight, bounds.X + bounds.Width - topRight.Width, bounds.Y, topRight.Width, topRight.Height);
            batch.Draw(sheet, bottomLeft, bounds.X, bounds.Y + bounds.Height - bottomLeft.Height, bottomLeft.Width, bottomLeft.Height);
            batch.Draw(sheet, bottomRight, bounds.X + bounds.Width - bottomRight.Width, bounds.Y + bounds.Height - bottomRight.Height, bottomRight.Width, bottomRight.Height);
            if (bisect)
            {
                batch.Draw(sheet, middleLeft, bounds.X, bounds.Y + bounds.Height / 2 - middleLeft.Height / 2, middleLeft.Width, middleLeft.Height);
                batch.Draw(sheet, middleRight, bounds.X + bounds.Width - middleRight.Width, bounds.Y + bounds.Height / 2 - middleRight.Height / 2, middleRight.Width, middleRight.Height);
            }
        }
    }
}
