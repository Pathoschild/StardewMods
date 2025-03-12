using System.Collections.Generic;
using System.Linq;
using Pathoschild.Stardew.Common;

namespace Pathoschild.Stardew.LookupAnything.Framework.Fields.Models;

/// <summary>A list of checkboxes with labels. The list may optionally contain intro text.</summary>
/// <param name="checkboxes">The checkbox values to display.</param>
internal class CheckboxList(CheckboxList.Checkbox[] checkboxes)
{
    /*********
    ** Fields
    *********/
    /// <summary>The checkbox values to display.</summary>
    public Checkbox[] Checkboxes = checkboxes;

    /// <summary>The intro text and icon to show before the checkboxes.</summary>
    public IntroData? Intro;

    /*********
    ** Public methods
    *********/
    /// <summary>Construct an instance.</summary>
    public CheckboxList() : this([])
    {
    }

    /// <summary>Construct an instance.</summary>
    /// <param name="checkboxes">The checkbox values to display.</param>
    public CheckboxList(IEnumerable<Checkbox> checkboxes) : this(checkboxes.ToArray())
    {
    }

    /// <summary>Add intro text before the checkboxes.</summary>
    /// <param name="text">The text to show before the checkboxes.</param>
    public CheckboxList AddIntro(string text, SpriteInfo? icon = null)
    {
        this.Intro = new(text, icon);
        return this;
    }

    /// <summary>A checkbox with a label.</summary>
    /// <param name="IsChecked">Whether the checkbox is checked.</param>
    /// <param name="Text">The text to display next to the checkbox.</param>
    internal record Checkbox(bool IsChecked, params IFormattedText[] Text)
    {
        /// <summary>Construct an instance.</summary>
        /// <param name="isChecked">Whether the checkbox is checked.</param>
        /// <param name="text">The text to display next to the checkbox.</param>
        public Checkbox(bool isChecked, string text) : this(isChecked, new FormattedText(text))
        {
        }
    }

    /// <summary>The text and icon to display above a checkbox list.</summary>
    /// <param name="Text">The text to display above the checkbox list.</param>
    /// <param name="Icon">The icon to display above the checkbox list.</param>
    internal record IntroData(string Text, SpriteInfo? Icon);
}
