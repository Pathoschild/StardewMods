using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
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
        /// <param name="onlyRevealed">Whether to only show gift tastes the player has discovered for themselves.</param>
        /// <param name="highlightUnrevealed">Whether to highlight items which haven't been revealed in the NPC profile yet.</param>
        /// <param name="onlyOwned">Whether to only show gift tastes for items which the player owns somewhere in the world.</param>
        /// <param name="ownedItemsCache">A lookup cache for owned items, as created by <see cref="GetOwnedItemsCache"/>.</param>
        public CharacterGiftTastesField(string label, IDictionary<GiftTaste, GiftTasteModel[]> giftTastes, GiftTaste showTaste, bool onlyRevealed, bool highlightUnrevealed, bool onlyOwned, IDictionary<string, bool> ownedItemsCache)
            : base(label, CharacterGiftTastesField.GetText(giftTastes, showTaste, onlyRevealed, highlightUnrevealed, onlyOwned, ownedItemsCache)) { }

        /// <summary>Get a lookup cache for owned items indexed by <see cref="Item.QualifiedItemId"/>.</summary>
        /// <param name="gameHelper">Provides utility methods for interacting with the game code.</param>
        public static IDictionary<string, bool> GetOwnedItemsCache(GameHelper gameHelper)
        {
            return gameHelper
                .GetAllOwnedItems()
                .GroupBy(entry => entry.Item.QualifiedItemId)
                .ToDictionary(group => group.Key, group => group.Any(p => p.IsInInventory));
        }


        /*********
        ** Private methods
        *********/
        /// <summary>Get the text to display.</summary>
        /// <param name="giftTastes">The items by how much this NPC likes receiving them.</param>
        /// <param name="showTaste">The gift taste to show.</param>
        /// <param name="onlyRevealed">Whether to only show gift tastes the player has discovered for themselves.</param>
        /// <param name="highlightUnrevealed">Whether to highlight items which haven't been revealed in the NPC profile yet.</param>
        /// <param name="onlyOwned">Whether to only show gift tastes for items which the player owns somewhere in the world.</param>
        /// <param name="ownedItemsCache">A lookup cache for owned items, as created by <see cref="GetOwnedItemsCache"/>.</param>
        private static IEnumerable<IFormattedText> GetText(IDictionary<GiftTaste, GiftTasteModel[]> giftTastes, GiftTaste showTaste, bool onlyRevealed, bool highlightUnrevealed, bool onlyOwned, IDictionary<string, bool> ownedItemsCache)
        {
            if (!giftTastes.ContainsKey(showTaste))
                yield break;

            // get data

            var items =
                (
                    from entry in giftTastes[showTaste]
                    let item = entry.Item

                    let ownership = ownedItemsCache.TryGetValue(item.QualifiedItemId, out bool rawVal) ? rawVal : null as bool? // true = in inventory, false = owned elsewhere, null = none found
                    let isOwned = ownership is not null
                    let inInventory = ownership is true

                    orderby inInventory descending, isOwned descending, item.DisplayName
                    select new { Item = item, IsInventory = inInventory, IsOwned = isOwned, entry.IsRevealed }
                )
                .ToArray();

            // generate text
            if (items.Any())
            {
                int unrevealed = 0;
                int unowned = 0;

                for (int i = 0, last = items.Length - 1; i <= last; i++)
                {
                    var entry = items[i];

                    if (onlyRevealed && !entry.IsRevealed)
                    {
                        unrevealed++;
                        continue;
                    }

                    if (onlyOwned && !entry.IsOwned)
                    {
                        unowned++;
                        continue;
                    }

                    string text = i != last
                        ? entry.Item.DisplayName + ", "
                        : entry.Item.DisplayName;
                    bool bold = highlightUnrevealed && !entry.IsRevealed;

                    if (entry.IsInventory)
                        yield return new FormattedText(text, Color.Green, bold);
                    else if (entry.IsOwned)
                        yield return new FormattedText(text, Color.Black, bold);
                    else
                        yield return new FormattedText(text, Color.Gray, bold);
                }

                if (unrevealed > 0)
                    yield return new FormattedText(I18n.Npc_UndiscoveredGiftTaste(count: unrevealed), Color.Gray);

                if (unowned > 0)
                    yield return new FormattedText(I18n.Npc_UnownedGiftTaste(count: unowned), Color.Gray);
            }
        }
    }
}
