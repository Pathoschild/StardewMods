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
    public class CharacterGiftTastesField : GenericField
    {
        /*********
        ** Properties
        *********/
        /// <summary>The items by how much this NPC likes receiving them.</summary>
        private readonly IDictionary<GiftTaste, Item[]> GiftTastes;


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="label">A short field label.</param>
        /// <param name="giftTastes">The items by how much this NPC likes receiving them.</param>
        public CharacterGiftTastesField(string label, IDictionary<GiftTaste, Item[]> giftTastes)
            : base(label, null, hasValue: giftTastes.ContainsKey(GiftTaste.Love) || giftTastes.ContainsKey(GiftTaste.Like))
        {
            this.GiftTastes = giftTastes;
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

            // loved gifts
            if (this.GiftTastes.ContainsKey(GiftTaste.Love))
            {
                string[] names = this.GiftTastes[GiftTaste.Love].Select(p => p.Name).OrderBy(p => p).ToArray();
                Vector2 labelSize = sprites.DrawStringBlock(font, string.Join(", ", names), new Vector2(position.X, position.Y + topOffset), wrapWidth);
                topOffset += labelSize.Y;
            }

            return new Vector2(wrapWidth, topOffset);
        }
    }
}