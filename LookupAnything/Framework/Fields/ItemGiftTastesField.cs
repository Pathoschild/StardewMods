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
    ** Public methods
    *********/
    /// <summary>Construct an instance.</summary>
    /// <param name="label">A short field label.</param>
    /// <param name="giftTastes">NPCs by how much they like receiving this item.</param>
    /// <param name="showTaste">The gift taste to show.</param>
    /// <param name="showUnknown">Whether to show gift tastes which the player hasn't learned about in-game yet</param>
    /// <param name="highlightUnrevealed">Whether to highlight items which haven't been revealed in the NPC profile yet.</param>
    public ItemGiftTastesField(string label, IDictionary<GiftTaste, GiftTasteModel[]> giftTastes, GiftTaste showTaste, bool showUnknown, bool highlightUnrevealed)
        : base(label, ItemGiftTastesField.GetText(giftTastes, showTaste, showUnknown, highlightUnrevealed)) { }


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
