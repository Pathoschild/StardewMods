using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;

namespace Pathoschild.Stardew.DataMaps.Framework
{
    /// <summary>Simplifies access to the game's sprite sheets.</summary>
    /// <remarks>Each sprite is represented by a rectangle, which specifies the coordinates and dimensions of the image in the sprite sheet.</remarks>
    internal static class Sprites
    {
        /// <summary>Sprites used to draw a legend box.</summary>
        public static class Legend
        {
            /// <summary>The sprite sheet containing the icon sprites.</summary>
            public static Texture2D Sheet => Game1.mouseCursors;

            /// <summary>The legend background.</summary>
            public static readonly Rectangle Background = new Rectangle(334, 321, 1, 1);

            /// <summary>The top border.</summary>
            public static readonly Rectangle Top = new Rectangle(331, 318, 1, 2);

            /// <summary>The bottom border.</summary>
            public static readonly Rectangle Bottom = new Rectangle(327, 334, 1, 2);

            /// <summary>The left border.</summary>
            public static readonly Rectangle Left = new Rectangle(325, 320, 6, 1);

            /// <summary>The right border.</summary>
            public static readonly Rectangle Right = new Rectangle(344, 320, 6, 1);

            /// <summary>The top-left corner.</summary>
            public static readonly Rectangle TopLeft = new Rectangle(325, 318, 6, 2);

            /// <summary>The top-right corner.</summary>
            public static readonly Rectangle TopRight = new Rectangle(344, 318, 6, 2);

            /// <summary>The bottom-left corner.</summary>
            public static readonly Rectangle BottomLeft = new Rectangle(325, 334, 6, 2);

            /// <summary>The bottom-right corner.</summary>
            public static readonly Rectangle BottomRight = new Rectangle(344, 334, 6, 2);
        }
    }
}
