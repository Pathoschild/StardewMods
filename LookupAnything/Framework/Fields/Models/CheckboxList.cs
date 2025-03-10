using System.Collections.Generic;
using System.Linq;

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

    /// <summary>The intro text to show before the checkboxes.</summary>
    public IFormattedText[]? Intro;

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
    public CheckboxList AddIntro(params IFormattedText[] text)
    {
        this.Intro = text;
        return this;
    }

    /// <summary>Add intro text before the checkboxes.</summary>
    /// <param name="text">The text to show before the checkboxes.</param>
    public CheckboxList AddIntro(params string[] text)
    {
        return this.AddIntro(
            text.Select(p => (IFormattedText)new FormattedText(p)).ToArray()
        );
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
}
