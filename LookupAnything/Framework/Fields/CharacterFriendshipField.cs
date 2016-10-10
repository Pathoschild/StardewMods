using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Pathoschild.LookupAnything.Components;
using Pathoschild.LookupAnything.Framework.Models;
using StardewValley;

namespace Pathoschild.LookupAnything.Framework.Fields
{
    /// <summary>A metadata field which shows friendship points.</summary>
    internal class CharacterFriendshipField : GenericField
    {
        /*********
        ** Properties
        *********/
        /// <summary>The player's current friendship data with the NPC.</summary>
        private readonly FriendshipModel Friendship;


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="label">A short field label.</param>
        /// <param name="friendship">The player's current friendship data with the NPC.</param>
        public CharacterFriendshipField(string label, FriendshipModel friendship)
            : base(label, null, hasValue: true)
        {
            this.Friendship = friendship;
        }

        /// <summary>Draw the value (or return <c>null</c> to render the <see cref="GenericField.Value"/> using the default format).</summary>
        /// <param name="spriteBatch">The sprite batch being drawn.</param>
        /// <param name="font">The recommended font.</param>
        /// <param name="position">The position at which to draw.</param>
        /// <param name="wrapWidth">The maximum width before which content should be wrapped.</param>
        /// <returns>Returns the drawn dimensions, or <c>null</c> to draw the <see cref="GenericField.Value"/> using the default format.</returns>
        public override Vector2? DrawValue(SpriteBatch spriteBatch, SpriteFont font, Vector2 position, float wrapWidth)
        {
            FriendshipModel friendship = this.Friendship;

            // draw hearts
            float leftOffset = 0;
            for (int i = 0; i < friendship.TotalHearts; i++)
            {
                // get icon
                Color color;
                Rectangle icon;
                if (friendship.LockedHearts >= friendship.TotalHearts - i)
                {
                    icon = Sprites.Icons.FilledHeart;
                    color = Color.Black * 0.35f;
                }
                else if (i >= friendship.FilledHearts)
                {
                    icon = Sprites.Icons.EmptyHeart;
                    color = Color.White;
                }
                else
                {
                    icon = Sprites.Icons.FilledHeart;
                    color = Color.White;
                }

                // draw
                spriteBatch.DrawSprite(Sprites.Icons.Sheet, icon, position.X + leftOffset, position.Y, color, Game1.pixelZoom);
                leftOffset += Sprites.Icons.FilledHeart.Width * Game1.pixelZoom;
            }

            // draw stardrop (if applicable)
            if (friendship.HasStardrop)
            {
                leftOffset += 1;
                float zoom = (Sprites.Icons.EmptyHeart.Height / (Sprites.Icons.Stardrop.Height * 1f)) * Game1.pixelZoom;
                spriteBatch.DrawSprite(Sprites.Icons.Sheet, Sprites.Icons.Stardrop, position.X + leftOffset, position.Y, Color.White * 0.25f, zoom);
                leftOffset += Sprites.Icons.Stardrop.Width * zoom;
            }

            // get caption text
            string caption = null;
            if (this.Friendship.EmptyHearts == 0 && this.Friendship.LockedHearts > 0)
                caption = "(need bouquet for next)";
            else
            {
                int pointsToNext = this.Friendship.GetPointsToNext();
                if (pointsToNext > 0)
                    caption = $"(next in {pointsToNext} pts)";
            }

            // draw caption
            float spaceSize = DrawHelper.GetSpaceWidth(font);
            Vector2 textSize = Vector2.Zero;
            if (caption != null)
                textSize = spriteBatch.DrawTextBlock(font, caption, new Vector2(position.X + leftOffset + spaceSize, position.Y), wrapWidth - leftOffset);

            return new Vector2(Sprites.Icons.FilledHeart.Width * Game1.pixelZoom * this.Friendship.TotalHearts + textSize.X + spaceSize, Math.Max(Sprites.Icons.FilledHeart.Height * Game1.pixelZoom, textSize.Y));
        }
    }
}
