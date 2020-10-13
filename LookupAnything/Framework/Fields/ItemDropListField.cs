using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Pathoschild.Stardew.Common;
using Pathoschild.Stardew.LookupAnything.Framework.Data;
using SObject = StardewValley.Object;

namespace Pathoschild.Stardew.LookupAnything.Framework.Fields
{
    /// <summary>A metadata field which shows a list of item drops.</summary>
    internal class ItemDropListField : GenericField
    {
        /*********
        ** Fields
        *********/
        /// <summary>Provides utility methods for interacting with the game code.</summary>
        protected GameHelper GameHelper;

        /// <summary>The possible drops.</summary>
        private readonly Tuple<ItemDropData, SObject, SpriteInfo>[] Drops;

        /// <summary>The text to display before the list, if any.</summary>
        private readonly string Preface;

        /// <summary>The text to display if there are no items.</summary>
        private readonly string DefaultText;

        /// <summary>Whether to fade out non-guaranteed drops.</summary>
        private readonly bool FadeNonGuaranteed;

        /// <summary>Whether to cross out non-guaranteed drops.</summary>
        private readonly bool CrossOutNonGuaranteed;


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="gameHelper">Provides utility methods for interacting with the game code.</param>
        /// <param name="label">A short field label.</param>
        /// <param name="drops">The possible drops.</param>
        /// <param name="sort">Whether to sort the resulting list by probability and name.</param>
        /// <param name="fadeNonGuaranteed">Whether to fade out non-guaranteed drops.</param>
        /// <param name="crossOutNonGuaranteed">Whether to cross out non-guaranteed drops.</param>
        /// <param name="defaultText">The text to display if there are no items (or <c>null</c> to hide the field).</param>
        /// <param name="preface">The text to display before the list, if any.</param>
        public ItemDropListField(GameHelper gameHelper, string label, IEnumerable<ItemDropData> drops, bool sort = true, bool fadeNonGuaranteed = false, bool crossOutNonGuaranteed = false, string defaultText = null, string preface = null)
            : base(label)
        {
            this.GameHelper = gameHelper;
            this.Drops = this.GetEntries(drops, gameHelper).ToArray();
            if (sort)
                this.Drops = this.Drops.OrderByDescending(p => p.Item1.Probability).ThenBy(p => p.Item2.DisplayName).ToArray();

            this.HasValue = defaultText != null || this.Drops.Any();
            this.FadeNonGuaranteed = fadeNonGuaranteed;
            this.CrossOutNonGuaranteed = crossOutNonGuaranteed;
            this.Preface = preface;
            this.DefaultText = defaultText;
        }

        /// <summary>Draw the value (or return <c>null</c> to render the <see cref="GenericField.Value"/> using the default format).</summary>
        /// <param name="spriteBatch">The sprite batch being drawn.</param>
        /// <param name="font">The recommended font.</param>
        /// <param name="position">The position at which to draw.</param>
        /// <param name="wrapWidth">The maximum width before which content should be wrapped.</param>
        /// <returns>Returns the drawn dimensions, or <c>null</c> to draw the <see cref="GenericField.Value"/> using the default format.</returns>
        public override Vector2? DrawValue(SpriteBatch spriteBatch, SpriteFont font, Vector2 position, float wrapWidth)
        {
            if (!this.Drops.Any())
                return spriteBatch.DrawTextBlock(font, this.DefaultText, position, wrapWidth);

            float height = 0;

            // draw preface
            if (!string.IsNullOrWhiteSpace(this.Preface))
            {
                Vector2 prefaceSize = spriteBatch.DrawTextBlock(font, this.Preface, position, wrapWidth);
                height += (int)prefaceSize.Y;
            }

            // list drops
            Vector2 iconSize = new Vector2(font.MeasureString("ABC").Y);
            foreach (var entry in this.Drops)
            {
                // get data
                ItemDropData drop = entry.Item1;
                SObject item = entry.Item2;
                SpriteInfo sprite = entry.Item3;
                bool isGuaranteed = drop.Probability > .99f;
                bool shouldFade = this.FadeNonGuaranteed && !isGuaranteed;
                bool shouldCrossOut = this.CrossOutNonGuaranteed && !isGuaranteed;

                // draw icon
                spriteBatch.DrawSpriteWithin(sprite, position.X, position.Y + height, iconSize, shouldFade ? Color.White * 0.5f : Color.White);

                // draw text
                string text = isGuaranteed ? item.DisplayName : I18n.Generic_PercentChanceOf(percent: (int)(Math.Round(drop.Probability, 4) * 100), label: item.DisplayName);
                if (drop.MinDrop != drop.MaxDrop)
                    text += $" ({I18n.Generic_Range(min: drop.MinDrop, max: drop.MaxDrop)})";
                else if (drop.MinDrop > 1)
                    text += $" ({drop.MinDrop})";
                Vector2 textSize = spriteBatch.DrawTextBlock(font, text, position + new Vector2(iconSize.X + 5, height + 5), wrapWidth, shouldFade ? Color.Gray : Color.Black);

                // cross out item if it definitely won't drop
                if (shouldCrossOut)
                    spriteBatch.DrawLine(position.X + iconSize.X + 5, position.Y + height + iconSize.Y / 2, new Vector2(textSize.X, 1), this.FadeNonGuaranteed ? Color.Gray : Color.Black);

                height += textSize.Y + 5;
            }

            // return size
            return new Vector2(wrapWidth, height);
        }


        /*********
        ** Private methods
        *********/
        /// <summary>Get the internal drop list entries.</summary>
        /// <param name="drops">The possible drops.</param>
        /// <param name="gameHelper">Provides utility methods for interacting with the game code.</param>
        private IEnumerable<Tuple<ItemDropData, SObject, SpriteInfo>> GetEntries(IEnumerable<ItemDropData> drops, GameHelper gameHelper)
        {
            foreach (ItemDropData drop in drops)
            {
                SObject item = this.GameHelper.GetObjectBySpriteIndex(drop.ItemID);
                SpriteInfo sprite = gameHelper.GetSprite(item);
                yield return Tuple.Create(drop, item, sprite);
            }
        }
    }
}
