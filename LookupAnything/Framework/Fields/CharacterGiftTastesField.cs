using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Pathoschild.Stardew.Common;
using Pathoschild.Stardew.LookupAnything.Framework.Constants;
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
        /// <param name="label">A short field label.</param>
        /// <param name="giftTastes">The items by how much this NPC likes receiving them.</param>
        /// <param name="showTaste">The gift taste to show.</param>
        /// <param name="onlyRevealed">Only show gift tastes the player has discovered for themselves.</param>
        /// <param name="highlightUnrevealed">Whether to highlight items which haven't been revealed in the NPC profile yet.</param>
        /// <param name="ownedItemsCache">A lookup cache for owned items, as created by <see cref="GetOwnedItemsCache"/>.</param>
        public CharacterGiftTastesField(string label, IDictionary<GiftTaste, GiftTasteModel[]> giftTastes, GiftTaste showTaste, bool onlyRevealed, bool highlightUnrevealed, IDictionary<string, bool> ownedItemsCache)
            : base(label, CharacterGiftTastesField.GetText(giftTastes, showTaste, onlyRevealed, highlightUnrevealed, ownedItemsCache)) { }

        /// <summary>Get a lookup cache for owned items.</summary>
        /// <param name="gameHelper">Provides utility methods for interacting with the game code.</param>
        public static IDictionary<string, bool> GetOwnedItemsCache(GameHelper gameHelper)
        {
            return gameHelper
                .GetAllOwnedItems()
                .GroupBy(entry => CharacterGiftTastesField.GetOwnedItemKey(entry.Item))
                .ToDictionary(group => group.Key, group => group.Any(p => p.IsInInventory));
        }


        /*********
        ** Private methods
        *********/
        /// <summary>Get an item's lookup key in the <see cref="GetOwnedItemsCache"/> lookup.</summary>
        /// <param name="item">The item instance.</param>
        private static string GetOwnedItemKey(Item item)
        {
            return $"{item.GetItemType()}:{item.ParentSheetIndex}";
        }

        /// <summary>Get the text to display.</summary>
        /// <param name="giftTastes">The items by how much this NPC likes receiving them.</param>
        /// <param name="showTaste">The gift taste to show.</param>
        /// <param name="onlyRevealed">Only show gift tastes the player has discovered for themselves.</param>
        /// <param name="highlightUnrevealed">Whether to highlight items which haven't been revealed in the NPC profile yet.</param>
        /// <param name="ownedItemsCache">A lookup cache for owned items, as created by <see cref="GetOwnedItemsCache"/>.</param>
        private static IEnumerable<IFormattedText> GetText(IDictionary<GiftTaste, GiftTasteModel[]> giftTastes, GiftTaste showTaste, bool onlyRevealed, bool highlightUnrevealed, IDictionary<string, bool> ownedItemsCache)
        {
            if (!giftTastes.ContainsKey(showTaste))
                yield break;

            // get data

            var items =
                (
                    from entry in giftTastes[showTaste]
                    let item = entry.Item

                    let inInventory = ownedItemsCache.TryGetValue(CharacterGiftTastesField.GetOwnedItemKey(item), out bool rawVal)
                        ? rawVal
                        : null as bool?
                    let isOwned = inInventory != null

                    where !onlyRevealed || entry.IsRevealed
                    orderby inInventory ?? false descending, isOwned descending, item.DisplayName
                    select new { Item = item, IsInventory = inInventory ?? false, IsOwned = isOwned, isRevealed = entry.IsRevealed }
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
                    yield return new FormattedText(I18n.Npc_UndiscoveredGiftTasteAppended(count: unrevealed), Color.Gray);
            }
            else
                yield return new FormattedText(I18n.Npc_UndiscoveredGiftTaste(count: unrevealed), Color.Gray);
        }
    }
}
