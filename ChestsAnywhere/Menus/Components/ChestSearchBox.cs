using System;
using Pathoschild.Stardew.ChestsAnywhere.Framework;
using StardewValley;
using StardewValley.Extensions;
using StardewValley.Menus;

namespace Pathoschild.Stardew.ChestsAnywhere.Menus.Components;

/// <summary>A chest search box.</summary>
/// <remarks>This is a validated textbox with a label and a match predicate.</remarks>
internal class ChestSearchBox
{
    /*********
    ** Fields
    *********/
    /// <summary>The last search text for which we updated results.</summary>
    private string LastValue = string.Empty;

    /// <summary>The tick where we last updated <see cref="LastValue"/>.</summary>
    private int LastCheckedTick = -1;


    /*********
    ** Accessors
    *********/
    /// <summary>Whether a search is specified and ready to apply.</summary>
    public bool HasValue => !string.IsNullOrEmpty(this.LastValue);

    /// <summary>The translated label text for the search field.</summary>
    public string Label { get; init; }

    /// <summary>The underlying text box.</summary>
    public ValidatedTextBox TextBox { get; init; }

    /// <summary>A predicate which gets whether a chest matches the given search text.</summary>
    public Func<ManagedChest, string, bool> MatchesSearchText { get; init; }

    /// <summary>The underlying clickable component on screen.</summary>
    public ClickableComponent Clickable;


    /*********
    ** Public methods
    *********/
    /// <summary>Construct an instance.</summary>
    /// <param name="label"><inheritdoc cref="Label" path="/summary"/></param>
    /// <param name="textBox"><inheritdoc cref="TextBox" path="/summary"/></param>
    /// <param name="matchesSearchText"><inheritdoc cref="MatchesSearchText" path="/summary"/></param>
    public ChestSearchBox(string label, ValidatedTextBox textBox, Func<ManagedChest, string, bool> matchesSearchText)
    {
        this.Label = label;
        this.TextBox = textBox;
        this.MatchesSearchText = matchesSearchText;
        this.Clickable = new ClickableComponent(textBox.GetBounds(), label) { ScreenReaderText = label };
    }

    /// <summary>Get whether the text box has changed. This only checks once per tick.</summary>
    public virtual bool CheckChange()
    {
        if (Game1.ticks == this.LastCheckedTick)
            return false;

        this.LastCheckedTick = Game1.ticks;
        if (this.TextBox.Text.EqualsIgnoreCase(this.LastValue))
            return false;

        this.LastValue = this.TextBox.Text;
        return true;
    }

    /// <summary>Get whether the current search text matches a chest.</summary>
    /// <param name="chest">The chest to check.</param>
    public bool Matches(ManagedChest chest)
    {
        return this.MatchesSearchText(chest, this.LastValue);
    }
}
