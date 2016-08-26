using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Pathoschild.LookupAnything.Components;
using StardewValley;

namespace Pathoschild.LookupAnything.Framework.Fields
{
    /// <summary>A metadata field which shows friendship points.</summary>
    public class CharacterFriendshipField : GenericField
    {
        /*********
        ** Properties
        *********/
        /// <summary>The player's current friendship points with the NPC.</summary>
        private readonly int FriendshipPoints;

        /// <summary>The number of friendship points per heart level.</summary>
        private readonly int PointsPerLevel;

        /// <summary>The maximum number of friendship points.</summary>
        private readonly int MaxPoints;


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="label">A short field label.</param>
        /// <param name="friendshipPoints">The player's current friendship points with the NPC.</param>
        /// <param name="pointsPerLevel">The number of points per heart level.</param>
        /// <param name="maxPoints">The maximum number of points.</param>
        public CharacterFriendshipField(string label, int friendshipPoints, int pointsPerLevel, int maxPoints)
            : base(label, null, hasValue: true)
        {
            this.FriendshipPoints = friendshipPoints;
            this.PointsPerLevel = pointsPerLevel;
            this.MaxPoints = maxPoints;
        }

        /// <summary>Draw the value (or return <c>null</c> to render the <see cref="GenericField.Value"/> using the default format).</summary>
        /// <param name="sprites">The sprite batch in which to draw.</param>
        /// <param name="font">The recommended font.</param>
        /// <param name="position">The position at which to draw.</param>
        /// <param name="wrapWidth">The maximum width before which content should be wrapped..</param>
        /// <returns>Returns the drawn dimensions, or <c>null</c> to draw the <see cref="GenericField.Value"/> using the default format.</returns>
        public override Vector2? DrawValue(SpriteBatch sprites, SpriteFont font, Vector2 position, float wrapWidth)
        {
            // get points
            int filledHearts = this.FriendshipPoints / this.PointsPerLevel;
            int emptyHearts = (this.MaxPoints / this.PointsPerLevel) - filledHearts;

            // draw hearts
            float leftOffset = 0;
            for (int i = 0; i < filledHearts; i++)
            {
                sprites.DrawBlock(Sprites.Icons.Sheet, Sprites.Icons.FilledHeart, position.X + leftOffset, position.Y, scale: Game1.pixelZoom);
                leftOffset += Sprites.Icons.FilledHeart.Width * Game1.pixelZoom;
            }
            for (int i = 0; i < emptyHearts; i++)
            {
                sprites.DrawBlock(Sprites.Icons.Sheet, Sprites.Icons.EmptyHeart, position.X + leftOffset, position.Y, scale: Game1.pixelZoom);
                leftOffset += Sprites.Icons.FilledHeart.Width * Game1.pixelZoom;
            }

            // draw caption
            float spaceSize = Sprites.GetSpaceWidth(font);
            Vector2 textSize = this.FriendshipPoints < this.MaxPoints
                ? sprites.DrawStringBlock(font, $"(next in {this.PointsPerLevel - (this.FriendshipPoints % this.PointsPerLevel)} pts)", new Vector2(position.X + leftOffset + spaceSize, position.Y), wrapWidth - leftOffset)
                : Vector2.Zero;

            return new Vector2(Math.Max(Sprites.Icons.FilledHeart.Height, textSize.X + spaceSize), Math.Max(Sprites.Icons.FilledHeart.Height * Game1.pixelZoom, textSize.Y));
        }
    }
}