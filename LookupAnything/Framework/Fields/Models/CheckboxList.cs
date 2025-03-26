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

    /// <summary>Whether to hide the list when drawing (e.g. when using progression mode).</summary>
    public bool IsHidden;


    /*********
    ** Public methods
    *********/
    /// <summary>A list of checkboxes with labels and an optional intro line.</summary>
    /// <param name="checkboxes">The checkbox values to display.</param>
    /// <param name="isHidden">Whether to hide the list when drawing (e.g. when using progression mode).</param>
    public CheckboxList(Checkbox[] checkboxes, bool isHidden = false)
    {
        this.Checkboxes = checkboxes;
        this.IsHidden = isHidden;
    }

    /// <summary>Construct an instance.</summary>
    /// <param name="checkboxes">The checkbox values to display.</param>
    /// <param name="isHidden">Whether to hide the list when drawing (e.g. when using progression mode).</param>
    public CheckboxList(IEnumerable<Checkbox> checkboxes, bool isHidden = false)
        : this(checkboxes.ToArray(), isHidden) { }

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
