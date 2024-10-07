using System;
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
        ** Accessors
        *********/
        /// <summary>The total number of items shown (including the sum of grouped entries like "11 unrevealed tastes").</summary>
        public int TotalItems { get; }


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="label">A short field label.</param>
        /// <param name="giftTastes">The items by how much this NPC likes receiving them.</param>
        /// <param name="showTaste">The gift taste to show.</param>
        /// <param name="showUnknown">Whether to show gift tastes the player hasn't discovered yet.</param>
        /// <param name="highlightUnrevealed">Whether to highlight items which haven't been revealed in the NPC profile yet.</param>
        /// <param name="onlyOwned">Whether to only show gift tastes for items which the player owns somewhere in the world.</param>
        /// <param name="ownedItemsCache">A lookup cache for owned items, as created by <see cref="GetOwnedItemsCache"/>.</param>
        public CharacterGiftTastesField(string label, IDictionary<GiftTaste, GiftTasteModel[]> giftTastes, GiftTaste showTaste, bool showUnknown, bool highlightUnrevealed, bool onlyOwned, IDictionary<string, bool> ownedItemsCache)
            : base(label)
        {
            ItemRecord[] allItems = this.GetGiftTasteRecords(giftTastes, showTaste, ownedItemsCache);

            this.TotalItems = allItems.Length;
            this.Value = this.GetText(allItems, showUnknown, highlightUnrevealed, onlyOwned).ToArray();
            this.HasValue = this.Value.Length > 0;
        }

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
        /// <summary>Get the items that can be listed for the current gift taste, ignoring filter options.</summary>
        /// <param name="giftTastes">The items by how much this NPC likes receiving them.</param>
        /// <param name="showTaste">The gift taste to show.</param>
        /// <param name="ownedItemsCache">A lookup cache for owned items, as created by <see cref="GetOwnedItemsCache"/>.</param>
        private ItemRecord[] GetGiftTasteRecords(IDictionary<GiftTaste, GiftTasteModel[]> giftTastes, GiftTaste showTaste, IDictionary<string, bool> ownedItemsCache)
        {
            if (!giftTastes.TryGetValue(showTaste, out GiftTasteModel[]? entries))
                return Array.Empty<ItemRecord>();

            // get data
            return
                (
                    from entry in entries
                    let item = entry.Item

                    let ownership = ownedItemsCache.TryGetValue(item.QualifiedItemId, out bool rawVal) ? rawVal : null as bool? // true = in inventory, false = owned elsewhere, null = none found
                    let isOwned = ownership is not null
                    let inInventory = ownership is true

                    orderby inInventory descending, isOwned descending, item.DisplayName
                    select new ItemRecord(item, inInventory, isOwned, entry.IsRevealed)
                )
                .ToArray();
        }

        /// <summary>Get the text to display.</summary>
        /// <param name="items">The items that can be listed for the current gift taste, ignoring filter options.</param>
        /// <param name="showUnknown">Whether to show gift tastes the player hasn't discovered yet.</param>
        /// <param name="highlightUnrevealed">Whether to highlight items which haven't been revealed in the NPC profile yet.</param>
        /// <param name="onlyOwned">Whether to only show gift tastes for items which the player owns somewhere in the world.</param>
        private IEnumerable<IFormattedText> GetText(ItemRecord[] items, bool showUnknown, bool highlightUnrevealed, bool onlyOwned)
        {
            // generate text
            if (items.Any())
            {
                int unrevealed = 0;
                int unowned = 0;

                for (int i = 0, last = items.Length - 1; i <= last; i++)
                {
                    var entry = items[i];

                    if (!showUnknown && !entry.IsRevealed)
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
                        ? entry.Item.DisplayName + I18n.Generic_ListSeparator()
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

        /// <summary>An item that can be shown in the list.</summary>
        /// <param name="Item">The item instance.</param>
        /// <param name="IsInventory">Whether this item is in the player's inventory.</param>
        /// <param name="IsOwned">Whether the player owns at least one of this item somewhere in the world.</param>
        /// <param name="IsRevealed">Whether the player has discovered this gift taste in-game.</param>
        private record ItemRecord(Item Item, bool IsInventory, bool IsOwned, bool IsRevealed);
    }
}
