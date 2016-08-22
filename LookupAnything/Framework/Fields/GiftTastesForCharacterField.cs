using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Pathoschild.LookupAnything.Components;
using Pathoschild.LookupAnything.Framework.Constants;
using StardewValley;

namespace Pathoschild.LookupAnything.Framework.Fields
{
    /// <summary>A metadata field which shows which items an NPC likes receiving.</summary>
    public class GiftTastesForCharacterField : GenericField
    {
        /*********
        ** Properties
        *********/
        /// <summary>The items by how much this NPC likes receiving them.</summary>
        private readonly IDictionary<GiftTaste, Item[]> GiftTastes;

        /// <summary>The tastes to display.</summary>
        private readonly GiftTaste[] ShowTastes;


        /*********
        ** Accessors
        *********/
        /// <summary>Whether the field should be displayed.</summary>
        public override bool HasValue => this.GiftTastes.ContainsKey(GiftTaste.Love) || this.GiftTastes.ContainsKey(GiftTaste.Like);


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="label">A short field label.</param>
        /// <param name="giftTastes">The items by how much this NPC likes receiving them.</param>
        /// <param name="showTastes">The tastes to display.</param>
        public GiftTastesForCharacterField(string label, IDictionary<GiftTaste, Item[]> giftTastes, params GiftTaste[] showTastes)
            : base(label, null)
        {
            this.GiftTastes = giftTastes;
            this.ShowTastes = showTastes;
        }

        /// <summary>Draw the value (or return <c>null</c> to render the <see cref="GenericField.Value"/> using the default format).</summary>
        /// <param name="sprites">The sprite batch in which to draw.</param>
        /// <param name="font">The recommended font.</param>
        /// <param name="position">The position at which to draw.</param>
        /// <param name="wrapWidth">The maximum width before which content should be wrapped..</param>
        /// <returns>Returns the drawn dimensions, or <c>null</c> to draw the <see cref="GenericField.Value"/> using the default format.</returns>
        public override Vector2? DrawValue(SpriteBatch sprites, SpriteFont font, Vector2 position, float wrapWidth)
        {
            float topOffset = 0;
            foreach (GiftTaste taste in this.ShowTastes)
            {
                if (!this.GiftTastes.ContainsKey(taste))
                    continue;

                string[] names = this.GiftTastes[taste].Select(p => p.Name).OrderBy(p => p).ToArray();
                const int gutterSize = 3; // space between label and list
                Vector2 labelSize = sprites.DrawStringBlock(Game1.smoothFont, $"{taste}s:", new Vector2(position.X, position.Y + topOffset), wrapWidth);
                Vector2 listSize = sprites.DrawStringBlock(Game1.smoothFont, $"{string.Join(", ", names)}.", new Vector2(position.X + labelSize.X + gutterSize, position.Y + topOffset), wrapWidth - labelSize.X - gutterSize);
                topOffset += Math.Max(labelSize.Y, listSize.Y);
            }
            return new Vector2(wrapWidth, topOffset);
        }
    }
}