using System.Collections.Generic;
using System.Linq;
using Pathoschild.Stardew.Common;

namespace Pathoschild.Stardew.LookupAnything.Framework.Fields.Models;

/// <summary>A list of checkboxes with labels and an optional intro line.</summary>
internal class CheckboxList
{
    /*********
    ** Accessors
    *********/
    /// <summary>The checkbox values to display.</summary>
    public Checkbox[] Checkboxes;

    /// <summary>The intro text and icon to show before the checkboxes.</summary>
    public IntroData? Intro;


    /*********
    ** Public methods
    *********/
    /// <summary>A list of checkboxes with labels and an optional intro line.</summary>
    /// <param name="checkboxes">The checkbox values to display.</param>
    public CheckboxList(params Checkbox[] checkboxes)
    {
        this.Checkboxes = checkboxes;
    }

    /// <summary>Construct an instance.</summary>
    /// <param name="checkboxes">The checkbox values to display.</param>
    public CheckboxList(IEnumerable<Checkbox> checkboxes)
        : this(checkboxes.ToArray()) { }

    /// <summary>Add intro text before the checkboxes.</summary>
    /// <param name="text">The text to render before the checkbox list.</param>
    /// <param name="icon">The icon to render before the <paramref name="text"/>, if any.</param>
    public CheckboxList AddIntro(string text, SpriteInfo? icon = null)
    {
        this.Intro = new IntroData(text, icon);
        return this;
    }

    /// <summary>The text and icon to render above a checkbox list.</summary>
    /// <param name="Text">The text to render above the checkbox list.</param>
    /// <param name="Icon">The icon to render before the <see cref="Icon"/>, if any.</param>
    internal record IntroData(string Text, SpriteInfo? Icon);
}
