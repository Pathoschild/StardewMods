using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Pathoschild.Stardew.LookupAnything.Framework.Constants;
using Pathoschild.Stardew.LookupAnything.Framework.Models;

namespace Pathoschild.Stardew.LookupAnything.Framework.Fields
{
    /// <summary>A metadata field which shows how much each NPC likes receiving this item.</summary>
    internal class ItemGiftTastesField : GenericField
    {
        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="gameHelper">Provides utility methods for interacting with the game code.</param>
        /// <param name="label">A short field label.</param>
        /// <param name="itemID">The item ID for which to show gift tastes.</param>
        /// <param name="giftTastes">NPCs by how much they like receiving this item.</param>
        /// <param name="showTaste">The gift taste to show.</param>
        /// <param name="onlyRevealed">Only show gift tastes the player has discovered for themselves.</param>
        public ItemGiftTastesField(GameHelper gameHelper, string label, int itemID, IDictionary<GiftTaste, GiftTasteModel[]> giftTastes, GiftTaste showTaste, bool onlyRevealed)
            : base(gameHelper, label, ItemGiftTastesField.GetText(giftTastes, showTaste, onlyRevealed)) { }


        /*********
        ** Private methods
        *********/
        /// <summary>Get the text to display.</summary>
        /// <param name="giftTastes">NPCs by how much they like receiving this item.</param>
        /// <param name="showTaste">The gift taste to show.</param>
        /// <param name="onlyRevealed">Only show gift tastes the player has discovered for themselves.</param>
        private static IEnumerable<IFormattedText> GetText(IDictionary<GiftTaste, GiftTasteModel[]> giftTastes, GiftTaste showTaste, bool onlyRevealed)
        {
            if (!giftTastes.ContainsKey(showTaste))
                yield break;

            // get data
            string[] names =
                (
                    from entry in giftTastes[showTaste]
                    orderby entry.Villager.Name ascending
                    where !onlyRevealed || entry.IsRevealed
                    select entry.Villager.Name
                )
                .ToArray();
            int unrevealed = onlyRevealed
                ? giftTastes[showTaste].Count(p => !p.IsRevealed)
                : 0;

            // build result
            if (names.Any())
            {
                yield return new FormattedText(string.Join(", ", names));
                if (unrevealed > 0)
                    yield return new FormattedText(L10n.Item.UndiscoveredVillagersAppend(count: unrevealed), Color.Gray);
            }
            else
                yield return new FormattedText(L10n.Item.UndiscoveredVillagers(count: unrevealed), Color.Gray);
        }
    }
}
