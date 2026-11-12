using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Pathoschild.Stardew.LookupAnything.Framework.Constants;
using Pathoschild.Stardew.LookupAnything.Framework.Models;

namespace Pathoschild.Stardew.LookupAnything.Framework.Fields;

/// <summary>A metadata field which shows how much each NPC likes receiving this item.</summary>
internal class ItemGiftTastesField : GenericField
{
    /*********
    ** Fields
    *********/
    /// <summary>The villagers a screen reader speaks, with whether their taste is undiscovered (shown in bold in the sighted view).</summary>
    private readonly SpokenEntry[] SpokenVillagers;

    /// <summary>The note a screen reader speaks after the villagers (like the number of undiscovered tastes).</summary>
    private readonly string? SpokenNote;


    /*********
    ** Public methods
    *********/
    /// <summary>Construct an instance.</summary>
    /// <param name="label">A short field label.</param>
    /// <param name="giftTastes">NPCs by how much they like receiving this item.</param>
    /// <param name="showTaste">The gift taste to show.</param>
    /// <param name="showUnknown">Whether to show gift tastes which the player hasn't learned about in-game yet</param>
    /// <param name="highlightUnrevealed">Whether to highlight items which haven't been revealed in the NPC profile yet.</param>
    public ItemGiftTastesField(string label, IDictionary<GiftTaste, GiftTasteModel[]> giftTastes, GiftTaste showTaste, bool showUnknown, bool highlightUnrevealed)
        : base(label, ItemGiftTastesField.GetText(giftTastes, showTaste, showUnknown, highlightUnrevealed))
    {
        if (giftTastes.TryGetValue(showTaste, out GiftTasteModel[]? allEntries))
        {
            GiftTasteModel[] visibleEntries = allEntries
                .Where(entry => showUnknown || entry.IsRevealed)
                .OrderBy(entry => entry.Villager.displayName)
                .ToArray();
            int unrevealed = showUnknown ? 0 : allEntries.Count(entry => !entry.IsRevealed);

            this.SpokenVillagers = visibleEntries
                .Select(entry => new SpokenEntry(highlightUnrevealed && !entry.IsRevealed ? I18n.ScreenReader_GiftUndiscovered(entry.Villager.displayName) : entry.Villager.displayName))
                .ToArray();
            this.SpokenNote = visibleEntries.Any()
                ? (unrevealed > 0 ? I18n.Item_UndiscoveredGiftTasteAppended(count: unrevealed) : null)
                : I18n.Item_UndiscoveredGiftTaste(count: unrevealed);
        }
        else
            this.SpokenVillagers = [];
    }

    /// <inheritdoc />
    public override string? GetSpokenText()
    {
        string text = SpokenText.Sentences([SpokenText.AndList(this.SpokenVillagers.Select(entry => entry.Text)), this.SpokenNote]);
        return text.Length > 0 ? text : null;
    }

    /// <inheritdoc />
    public override IReadOnlyList<SpokenEntry> GetSpokenEntries()
    {
        return this.SpokenNote != null
            ? [.. this.SpokenVillagers, new SpokenEntry(this.SpokenNote)]
            : this.SpokenVillagers;
    }


    /*********
    ** Private methods
    *********/
    /// <summary>Get the text to display.</summary>
    /// <param name="giftTastes">NPCs by how much they like receiving this item.</param>
    /// <param name="showTaste">The gift taste to show.</param>
    /// <param name="showUnknown">Whether to show gift tastes which the player hasn't learned about in-game yet</param>
    /// <param name="highlightUnrevealed">Whether to highlight items which haven't been revealed in the NPC profile yet.</param>
    private static IEnumerable<IFormattedText> GetText(IDictionary<GiftTaste, GiftTasteModel[]> giftTastes, GiftTaste showTaste, bool showUnknown, bool highlightUnrevealed)
    {
        if (!giftTastes.TryGetValue(showTaste, out GiftTasteModel[]? allEntries))
            yield break;

        // get data
        GiftTasteModel[] visibleEntries =
            (
                from entry in allEntries
                orderby entry.Villager.displayName ascending
                where showUnknown || entry.IsRevealed
                select entry
            )
            .ToArray();
        int unrevealed = !showUnknown
            ? giftTastes[showTaste].Count(p => !p.IsRevealed)
            : 0;

        // build result
        if (visibleEntries.Any())
        {
            for (int i = 0, last = visibleEntries.Length - 1; i <= last; i++)
            {
                GiftTasteModel entry = visibleEntries[i];

                yield return new FormattedText(
                    text: entry.Villager.displayName + (i != last ? I18n.Generic_ListSeparator() : ""),
                    bold: highlightUnrevealed && !entry.IsRevealed
                );
            }

            if (unrevealed > 0)
                yield return new FormattedText(I18n.Item_UndiscoveredGiftTasteAppended(count: unrevealed), Color.Gray);
        }
        else
            yield return new FormattedText(I18n.Item_UndiscoveredGiftTaste(count: unrevealed), Color.Gray);
    }
}
