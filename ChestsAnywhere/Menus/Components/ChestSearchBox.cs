
using System;
using Pathoschild.Stardew.ChestsAnywhere.Framework;
using StardewValley;
using StardewValley.Extensions;
using StardewValley.Menus;

namespace Pathoschild.Stardew.ChestsAnywhere.Menus.Components;

/// <summary>A search box, which is a <see cref="ValidatedTextBox"/> with a label and a match predicate</summary>
/// <param name="Label">String label</param>
/// <param name="Box">Text box component</param>
/// <param name="MatchesFunc">Chest match predicate</param>
internal record ChestSearchBox(string Label, ValidatedTextBox Box, Func<ManagedChest, string, bool> MatchesFunc)
{
    /// <summary>Last known value of textbox</summary>
    private string LastValue = string.Empty;
    /// <summary>The last tick when LastValue was checked</summary>
    private int LastCheckedTick = -1;

    internal ClickableComponent Clickable = new(Box.GetBounds(), Label) { ScreenReaderText = Label };

    /// <summary>Whether the last value is valid</summary>
    internal bool HasValue => !string.IsNullOrEmpty(this.LastValue);

    /// <summary>Check if the text box has changed, only checks once per tick</summary>
    /// <returns>Whether textbox has changed</returns>
    internal virtual bool CheckChange()
    {
        if (Game1.ticks == this.LastCheckedTick)
        {
            return false;
        }
        this.LastCheckedTick = Game1.ticks;
        if (this.Box.Text.EqualsIgnoreCase(this.LastValue))
        {
            return false;
        }
        this.LastValue = this.Box.Text;
        return true;
    }

    /// <summary>Invokes <see cref="MatchesFunc"/> on current value</summary>
    /// <param name="chest">Managed chest to check</param>
    /// <returns>whether chest matches predicate</returns>
    internal bool Matches(ManagedChest chest)
    {
        return this.MatchesFunc(chest, this.LastValue);
    }
}
