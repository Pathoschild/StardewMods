using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Pathoschild.Stardew.LookupAnything.Framework.Constants;
using Pathoschild.Stardew.LookupAnything.Framework.ItemScanning;
using Pathoschild.Stardew.LookupAnything.Framework.Models;
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
        /// <param name="gameHelper">Provides utility methods for interacting with the game code.</param>
        /// <param name="label">A short field label.</param>
        /// <param name="giftTastes">The items by how much this NPC likes receiving them.</param>
        /// <param name="showTaste">The gift taste to show.</param>
        /// <param name="onlyRevealed">Only show gift tastes the player has discovered for themselves.</param>
        /// <param name="highlightUnrevealed">Whether to highlight items which haven't been revealed in the NPC profile yet.</param>
        public CharacterGiftTastesField(GameHelper gameHelper, string label, IDictionary<GiftTaste, GiftTasteModel[]> giftTastes, GiftTaste showTaste, bool onlyRevealed, bool highlightUnrevealed)
            : base(gameHelper, label, CharacterGiftTastesField.GetText(gameHelper, giftTastes, showTaste, onlyRevealed, highlightUnrevealed)) { }


        /*********
        ** Private methods
        *********/
        /// <summary>Get the text to display.</summary>
        /// <param name="gameHelper">Provides utility methods for interacting with the game code.</param>
        /// <param name="giftTastes">The items by how much this NPC likes receiving them.</param>
        /// <param name="showTaste">The gift taste to show.</param>
        /// <param name="onlyRevealed">Only show gift tastes the player has discovered for themselves.</param>
        /// <param name="highlightUnrevealed">Whether to highlight items which haven't been revealed in the NPC profile yet.</param>
        private static IEnumerable<IFormattedText> GetText(GameHelper gameHelper, IDictionary<GiftTaste, GiftTasteModel[]> giftTastes, GiftTaste showTaste, bool onlyRevealed, bool highlightUnrevealed)
        {
            if (!giftTastes.ContainsKey(showTaste))
                yield break;

            // get data
            FoundItem[] ownedItems = gameHelper.GetAllOwnedItems().ToArray();
            Item[] inventory = ownedItems.Where(p => p.IsInInventory).Select(p => p.Item).ToArray();
            var items =
                (
                    from entry in giftTastes[showTaste]
                    let item = entry.Item
                    let isInventory = inventory.Any(p => p.ParentSheetIndex == item.ParentSheetIndex && p.Category == item.Category)
                    let isOwned = ownedItems.Any(p => p.Item.ParentSheetIndex == item.ParentSheetIndex && p.Item.Category == item.Category)
                    where !onlyRevealed || entry.IsRevealed
                    orderby isInventory descending, isOwned descending, item.DisplayName
                    select new { Item = item, IsInventory = isInventory, IsOwned = isOwned, isRevealed = entry.IsRevealed }
                )
                .ToArray();
            int unrevealed = onlyRevealed
                ? giftTastes[showTaste].Count(p => !p.IsRevealed)
                : 0;

            // generate text
            if (items.Any())
            {
                for (int i = 0, last = items.Length - 1; i <= last; i++)
                {
                    var entry = items[i];
                    string text = i != last
                        ? entry.Item.DisplayName + ", "
                        : entry.Item.DisplayName;
                    bool bold = highlightUnrevealed && !entry.isRevealed;

                    if (entry.IsInventory)
                        yield return new FormattedText(text, Color.Green, bold);
                    else if (entry.IsOwned)
                        yield return new FormattedText(text, Color.Black, bold);
                    else
                        yield return new FormattedText(text, Color.Gray, bold);
                }

                if (unrevealed > 0)
                    yield return new FormattedText(L10n.Npc.UndiscoveredVillagersAppend(count: unrevealed), Color.Gray);
            }
            else
                yield return new FormattedText(L10n.Npc.UndiscoveredVillagers(count: unrevealed), Color.Gray);
        }
    }
}
