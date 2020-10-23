using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Pathoschild.Stardew.Common.UI;
using Pathoschild.Stardew.LookupAnything.Framework.Models;
using StardewValley;

namespace Pathoschild.Stardew.LookupAnything.Framework.Fields
{
    /// <summary>A metadata field which shows friendship points.</summary>
    internal class CharacterFriendshipField : GenericField
    {
        /*********
        ** Fields
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
            : base(label, hasValue: true)
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

            // draw status
            float leftOffset = 0;
            {
                string statusText = I18n.For(friendship.Status, friendship.CanHousemate);
                Vector2 textSize = spriteBatch.DrawTextBlock(font, statusText, new Vector2(position.X + leftOffset, position.Y), wrapWidth - leftOffset);
                leftOffset += textSize.X + DrawHelper.GetSpaceWidth(font);
            }

            // draw hearts
            for (int i = 0; i < friendship.TotalHearts; i++)
            {
                // get icon
                Color color;
                Rectangle icon;
                if (friendship.LockedHearts >= friendship.TotalHearts - i)
                {
                    icon = CommonSprites.Icons.FilledHeart;
                    color = Color.Black * 0.35f;
                }
                else if (i >= friendship.FilledHearts)
                {
                    icon = CommonSprites.Icons.EmptyHeart;
                    color = Color.White;
                }
                else
                {
                    icon = CommonSprites.Icons.FilledHeart;
                    color = Color.White;
                }

                // draw
                spriteBatch.DrawSprite(CommonSprites.Icons.Sheet, icon, position.X + leftOffset, position.Y, color, Game1.pixelZoom);
                leftOffset += CommonSprites.Icons.FilledHeart.Width * Game1.pixelZoom;
            }

            // draw stardrop (if applicable)
            if (friendship.HasStardrop)
            {
                leftOffset += 1;
                float zoom = (CommonSprites.Icons.EmptyHeart.Height / (CommonSprites.Icons.Stardrop.Height * 1f)) * Game1.pixelZoom;
                spriteBatch.DrawSprite(CommonSprites.Icons.Sheet, CommonSprites.Icons.Stardrop, position.X + leftOffset, position.Y, Color.White * 0.25f, zoom);
                leftOffset += CommonSprites.Icons.Stardrop.Width * zoom;
            }

            // get caption text
            string caption = null;
            if (this.Friendship.EmptyHearts == 0 && this.Friendship.LockedHearts > 0)
                caption = $"({I18n.Npc_Friendship_NeedBouquet()})";
            else
            {
                int pointsToNext = this.Friendship.GetPointsToNext();
                if (pointsToNext > 0)
                    caption = $"({I18n.Npc_Friendship_NeedPoints(pointsToNext)})";
            }

            // draw caption
            {
                float spaceSize = DrawHelper.GetSpaceWidth(font);
                Vector2 textSize = Vector2.Zero;
                if (caption != null)
                    textSize = spriteBatch.DrawTextBlock(font, caption, new Vector2(position.X + leftOffset + spaceSize, position.Y), wrapWidth - leftOffset);

                return new Vector2(CommonSprites.Icons.FilledHeart.Width * Game1.pixelZoom * this.Friendship.TotalHearts + textSize.X + spaceSize, Math.Max(CommonSprites.Icons.FilledHeart.Height * Game1.pixelZoom, textSize.Y));
            }
        }
    }
}
