using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Pathoschild.Stardew.LookupAnything.Framework.Constants;
using Pathoschild.Stardew.LookupAnything.Framework.Models;
using StardewValley;

namespace Pathoschild.Stardew.LookupAnything.Framework.Fields;

/// <summary>A metadata field which shows which items an NPC likes receiving.</summary>
internal class CharacterGiftTastesField : GenericField
{
    /*********
    ** Accessors
    *********/
    /// <summary>The total number of items shown (including the sum of grouped entries like "11 unrevealed tastes").</summary>
    public int TotalItems { get; }

    /// <summary>The items a screen reader speaks.</summary>
    private readonly SpokenItem[] SpokenItems;

    /// <summary>The notes a screen reader speaks after the items (like the number of undiscovered tastes).</summary>
    private readonly string[] SpokenNotes;


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
        (this.SpokenItems, this.SpokenNotes) = this.GetSpokenItems(allItems, showUnknown, highlightUnrevealed, onlyOwned);
    }

    /// <inheritdoc />
    public override string? GetSpokenText()
    {
        // the sighted view colours items green if they're in the player's inventory, black if owned elsewhere, and grey if not owned
        List<string> parts = [];
        foreach (OwnershipState state in new[] { OwnershipState.Inventory, OwnershipState.Owned, OwnershipState.NotOwned })
        {
            string[] names = this.SpokenItems
                .Where(item => item.State == state)
                .Select(item => item.IsUndiscovered ? I18n.ScreenReader_GiftUndiscoveredInline(item.Name) : item.Name)
                .ToArray();
            if (names.Length == 0)
                continue;

            string list = SpokenText.AndList(names);
            parts.Add(state switch
            {
                OwnershipState.Inventory => I18n.ScreenReader_GiftGroupInventory(list),
                OwnershipState.Owned => I18n.ScreenReader_GiftGroupOwned(list),
                _ => I18n.ScreenReader_GiftGroupNotOwned(list)
            });
        }
        parts.AddRange(this.SpokenNotes);

        string text = SpokenText.Sentences(parts);
        return text.Length > 0 ? text : null;
    }

    /// <inheritdoc />
    public override IReadOnlyList<SpokenEntry> GetSpokenEntries()
    {
        List<SpokenEntry> entries = [];
        foreach (SpokenItem item in this.SpokenItems)
        {
            string text = item.State switch
            {
                OwnershipState.Inventory => I18n.ScreenReader_GiftInInventory(item.Name),
                OwnershipState.Owned => I18n.ScreenReader_GiftOwned(item.Name),
                _ => I18n.ScreenReader_GiftNotOwned(item.Name)
            };
            if (item.IsUndiscovered)
                text = I18n.ScreenReader_GiftUndiscovered(text);

            entries.Add(new SpokenEntry(text));
        }
        entries.AddRange(this.SpokenNotes.Select(note => new SpokenEntry(note)));

        return entries;
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
            return [];

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
    /// <summary>Get the items and notes a screen reader speaks, following the same filtering as <see cref="GetText"/>.</summary>
    /// <param name="items">The items to list.</param>
    /// <param name="showUnknown">Whether to show items whose gift taste the player hasn't discovered.</param>
    /// <param name="highlightUnrevealed">Whether undiscovered items are highlighted (in bold) in the sighted view.</param>
    /// <param name="onlyOwned">Whether to only show items the player owns.</param>
    private (SpokenItem[] Items, string[] Notes) GetSpokenItems(ItemRecord[] items, bool showUnknown, bool highlightUnrevealed, bool onlyOwned)
    {
        List<SpokenItem> spokenItems = [];
        List<string> notes = [];
        int unrevealed = 0;
        int unowned = 0;

        foreach (ItemRecord entry in items)
        {
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

            OwnershipState state = entry.IsInventory ? OwnershipState.Inventory : entry.IsOwned ? OwnershipState.Owned : OwnershipState.NotOwned;
            spokenItems.Add(new SpokenItem(entry.Item.DisplayName, state, highlightUnrevealed && !entry.IsRevealed));
        }

        if (unrevealed > 0)
            notes.Add(I18n.Npc_UndiscoveredGiftTaste(count: unrevealed));
        if (unowned > 0)
            notes.Add(I18n.Npc_UnownedGiftTaste(count: unowned));

        return (spokenItems.ToArray(), notes.ToArray());
    }

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

    /// <summary>Where the player has an item.</summary>
    private enum OwnershipState
    {
        /// <summary>In the player's inventory.</summary>
        Inventory,

        /// <summary>Owned, but not in the player's inventory.</summary>
        Owned,

        /// <summary>Not owned.</summary>
        NotOwned
    }

    /// <summary>An item as spoken by a screen reader.</summary>
    private record SpokenItem(string Name, OwnershipState State, bool IsUndiscovered);
}
