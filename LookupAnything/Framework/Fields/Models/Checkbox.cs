namespace Pathoschild.Stardew.LookupAnything.Framework.Fields.Models;

/// <summary>A checkbox with a label.</summary>
/// <param name="IsChecked">Whether the checkbox is checked.</param>
/// <param name="Text">The text to display next to the checkbox.</param>
internal record Checkbox(bool IsChecked, params IFormattedText[] Text)
{
    /// <summary>The text a screen reader speaks for the checkbox, if it differs from the displayed text (e.g. because the text uses colours or layout to show meaning).</summary>
    public string? SpokenText { get; init; }

    /// <summary>Plain wording for an unmet condition in a fish summary.</summary>
    public string? UnmetText { get; init; }

    /// <summary>Construct an instance.</summary>
    /// <param name="isChecked">Whether the checkbox is checked.</param>
    /// <param name="text">The text to display next to the checkbox.</param>
    public Checkbox(bool isChecked, string text)
        : this(isChecked, new FormattedText(text)) { }
}
