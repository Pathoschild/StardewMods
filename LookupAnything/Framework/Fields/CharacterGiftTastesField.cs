using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Pathoschild.Stardew.LookupAnything.Framework.Constants;
using StardewValley;

namespace Pathoschild.Stardew.LookupAnything.Framework.Fields
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
        private static IEnumerable<IFormattedText> GetText(IDictionary<GiftTaste, Item[]> giftTastes, GiftTaste showTaste)
        {
            if (!giftTastes.ContainsKey(showTaste))
                yield break;

            Item[] items = giftTastes[showTaste].OrderBy(p => p.Name).ToArray();
            Item[] ownedItems = GameHelper.GetAllOwnedItems().ToArray();

            for (int i = 0, last = items.Length - 1; i <= last; i++)
            {
                Item item = items[i];
                bool owned = ownedItems.Any(p => p.parentSheetIndex == item.parentSheetIndex && p.category == item.category);
                string text = i != last
                    ? item.Name + ","
                    : item.Name;
                yield return new FormattedText(text, owned ? Color.Green : Color.Black);
            }
        }
    }
}