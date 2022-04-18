using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;

namespace Pathoschild.Stardew.Common.UI
{
    /// <summary>Simplifies access to the game's sprite sheets.</summary>
    /// <remarks>Each sprite is represented by a rectangle, which specifies the coordinates and dimensions of the image in the sprite sheet.</remarks>
    internal static class CommonSprites
    {
        /// <summary>Sprites used to draw a button.</summary>
        public static class Button
        {
            /// <summary>The sprite sheet containing the icon sprites.</summary>
            public static Texture2D Sheet => Game1.mouseCursors;

            /// <summary>The legend background.</summary>
            public static readonly Rectangle Background = new(297, 364, 1, 1);

            /// <summary>The top border.</summary>
            public static readonly Rectangle Top = new(279, 284, 1, 4);

            /// <summary>The bottom border.</summary>
            public static readonly Rectangle Bottom = new(279, 296, 1, 4);

            /// <summary>The left border.</summary>
            public static readonly Rectangle Left = new(274, 289, 4, 1);

            /// <summary>The right border.</summary>
            public static readonly Rectangle Right = new(286, 289, 4, 1);

            /// <summary>The top-left corner.</summary>
            public static readonly Rectangle TopLeft = new(274, 284, 4, 4);

            /// <summary>The top-right corner.</summary>
            public static readonly Rectangle TopRight = new(286, 284, 4, 4);

            /// <summary>The bottom-left corner.</summary>
            public static readonly Rectangle BottomLeft = new(274, 296, 4, 4);

            /// <summary>The bottom-right corner.</summary>
            public static readonly Rectangle BottomRight = new(286, 296, 4, 4);
        }

        /// <summary>Sprites used to draw a dropdown list.</summary>
        public static class DropDown
        {
            /// <summary>The sprite sheet containing the menu sprites.</summary>
            public static readonly Texture2D Sheet = Game1.mouseCursors;

            /// <summary>The background for the selected item.</summary>
            public static readonly Rectangle ActiveBackground = new(258, 258, 4, 4);

            /// <summary>The background for a non-selected, non-hovered item.</summary>
            public static readonly Rectangle InactiveBackground = new(269, 258, 4, 4);

            /// <summary>The background for an item under the cursor.</summary>
            public static readonly Rectangle HoverBackground = new(161, 340, 4, 4);
        }

        /// <summary>Sprites used to draw icons.</summary>
        public static class Icons
        {
            /// <summary>The sprite sheet containing the icon sprites.</summary>
            public static Texture2D Sheet => Game1.mouseCursors;

            /// <summary>A down arrow.</summary>
            public static readonly Rectangle DownArrow = new(12, 76, 40, 44);

            /// <summary>An up arrow.</summary>
            public static readonly Rectangle UpArrow = new(76, 72, 40, 44);

            /// <summary>A left arrow.</summary>
            public static readonly Rectangle LeftArrow = new(8, 268, 44, 40);

            /// <summary>A right arrow.</summary>
            public static readonly Rectangle RightArrow = new(12, 204, 44, 40);

            /// <summary>A speech bubble icon.</summary>
            public static readonly Rectangle SpeechBubble = new(66, 4, 14, 12);

            /// <summary>An empty checkbox icon.</summary>
            public static readonly Rectangle EmptyCheckbox = new(227, 425, 9, 9);

            /// <summary>A filled heart indicating a friendship level.</summary>
            public static readonly Rectangle FilledHeart = new(211, 428, 7, 6);

            /// <summary>An empty heart indicating a missing friendship level.</summary>
            public static readonly Rectangle EmptyHeart = new(218, 428, 7, 6);

            /// <summary>A filled checkbox icon.</summary>
            public static readonly Rectangle FilledCheckbox = new(236, 425, 9, 9);

            /// <summary>An exit button.</summary>
            public static readonly Rectangle ExitButton = new(337, 494, 12, 12);

            /// <summary>A stardrop icon.</summary>
            public static readonly Rectangle Stardrop = new(346, 392, 8, 8);
        }

        /// <summary>Sprites used to draw a scroll.</summary>
        public static class Scroll
        {
            /// <summary>The sprite sheet containing the icon sprites.</summary>
            public static Texture2D Sheet => Game1.mouseCursors;

            /// <summary>The legend background.</summary>
            public static readonly Rectangle Background = new(334, 321, 1, 1);

            /// <summary>The top border.</summary>
            public static readonly Rectangle Top = new(331, 318, 1, 2);

            /// <summary>The bottom border.</summary>
            public static readonly Rectangle Bottom = new(327, 334, 1, 2);

            /// <summary>The left border.</summary>
            public static readonly Rectangle Left = new(325, 320, 6, 1);

            /// <summary>The right border.</summary>
            public static readonly Rectangle Right = new(344, 320, 6, 1);

            /// <summary>The top-left corner.</summary>
            public static readonly Rectangle TopLeft = new(325, 318, 6, 2);

            /// <summary>The top-right corner.</summary>
            public static readonly Rectangle TopRight = new(344, 318, 6, 2);

            /// <summary>The bottom-left corner.</summary>
            public static readonly Rectangle BottomLeft = new(325, 334, 6, 2);

            /// <summary>The bottom-right corner.</summary>
            public static readonly Rectangle BottomRight = new(344, 334, 6, 2);
        }

        /// <summary>Sprites used to draw a tab.</summary>
        public static class Tab
        {
            /// <summary>The sprite sheet containing the tab sprites.</summary>
            public static readonly Texture2D Sheet = Game1.mouseCursors;

            /// <summary>The top-left corner.</summary>
            public static readonly Rectangle TopLeft = new(0, 384, 5, 5);

            /// <summary>The top-right corner.</summary>
            public static readonly Rectangle TopRight = new(11, 384, 5, 5);

            /// <summary>The bottom-left corner.</summary>
            public static readonly Rectangle BottomLeft = new(0, 395, 5, 5);

            /// <summary>The bottom-right corner.</summary>
            public static readonly Rectangle BottomRight = new(11, 395, 5, 5);

            /// <summary>The top edge.</summary>
            public static readonly Rectangle Top = new(4, 384, 1, 3);

            /// <summary>The left edge.</summary>
            public static readonly Rectangle Left = new(0, 388, 3, 1);

            /// <summary>The right edge.</summary>
            public static readonly Rectangle Right = new(13, 388, 3, 1);

            /// <summary>The bottom edge.</summary>
            public static readonly Rectangle Bottom = new(4, 397, 1, 3);

            /// <summary>The tab background.</summary>
            public static readonly Rectangle Background = new(5, 387, 1, 1);
        }
    }
}
