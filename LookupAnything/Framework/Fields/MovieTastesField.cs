using System.Collections.Generic;
using System.Linq;
using Pathoschild.Stardew.LookupAnything.Framework.Constants;

namespace Pathoschild.Stardew.LookupAnything.Framework.Fields;

/// <summary>A metadata field which shows how much each NPC likes watching a movie.</summary>
internal class MovieTastesField : GenericField
{
    /*********
    ** Fields
    *********/
    /// <summary>The villager names to speak.</summary>
    private readonly string[] Names;


    /*********
    ** Public methods
    *********/
    /// <summary>Construct an instance.</summary>
    /// <param name="label">A short field label.</param>
    /// <param name="giftTastes">NPCs by how much they like receiving this item.</param>
    /// <param name="showTaste">The gift taste to show.</param>
    public MovieTastesField(string label, IDictionary<GiftTaste, string[]> giftTastes, GiftTaste showTaste)
        : base(label, MovieTastesField.GetText(giftTastes, showTaste))
    {
        this.Names = giftTastes.TryGetValue(showTaste, out string[]? names)
            ? names.OrderBy(p => p).ToArray()
            : [];
    }

    /// <inheritdoc />
    public override string? GetSpokenText()
    {
        return this.Names.Length > 0
            ? SpokenText.AndList(this.Names)
            : null;
    }

    /// <inheritdoc />
    public override IReadOnlyList<SpokenEntry> GetSpokenEntries()
    {
        return this.Names.Select(name => new SpokenEntry(name)).ToArray();
    }


    /*********
    ** Private methods
    *********/
    /// <summary>Get the text to display.</summary>
    /// <param name="giftTastes">NPCs by how much they like receiving this item.</param>
    /// <param name="showTaste">The gift taste to show.</param>
    private static string? GetText(IDictionary<GiftTaste, string[]> giftTastes, GiftTaste showTaste)
    {
        if (!giftTastes.TryGetValue(showTaste, out string[]? names))
            return null;

        names = names.OrderBy(p => p).ToArray();
        return I18n.List(names);
    }
}
