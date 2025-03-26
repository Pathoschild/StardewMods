namespace Pathoschild.Stardew.LookupAnything.Framework.Fields.Models;

/// <summary>A checkbox with a label.</summary>
/// <param name="IsChecked">Whether the checkbox is checked.</param>
/// <param name="Text">The text to display next to the checkbox.</param>
internal record Checkbox(bool IsChecked, params IFormattedText[] Text)
{
    /// <summary>Construct an instance.</summary>
    /// <param name="isChecked">Whether the checkbox is checked.</param>
    /// <param name="text">The text to display next to the checkbox.</param>
    public Checkbox(bool isChecked, string text)
        : this(isChecked, new FormattedText(text)) { }
}
