using System.Collections.Generic;
using System.Linq;
using Pathoschild.LookupAnything.Framework.Constants;
using StardewValley;

namespace Pathoschild.LookupAnything.Framework.Fields
{
    /// <summary>A metadata field which shows which items an NPC likes receiving.</summary>
    internal class CharacterGiftTastesField : GenericField
    {
        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="label">A short field label.</param>
        /// <param name="giftTastes">The items by how much this NPC likes receiving them.</param>
        /// <param name="showTaste">The gift taste to show.</param>
        public CharacterGiftTastesField(string label, IDictionary<GiftTaste, Item[]> giftTastes, GiftTaste showTaste)
            : base(label, CharacterGiftTastesField.GetText(giftTastes, showTaste)) { }


        /*********
        ** Private methods
        *********/
        /// <summary>Get the text to display.</summary>
        /// <param name="giftTastes">The items by how much this NPC likes receiving them.</param>
        /// <param name="showTaste">The gift taste to show.</param>
        private static string GetText(IDictionary<GiftTaste, Item[]> giftTastes, GiftTaste showTaste)
        {
            if (!giftTastes.ContainsKey(showTaste))
                return null;

            string[] names = giftTastes[showTaste].Select(p => p.Name).OrderBy(p => p).ToArray();
            return string.Join(", ", names);
        }
    }
}