using System;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Xna.Framework;
using Pathoschild.Stardew.LookupAnything.Framework.Lookups;

namespace Pathoschild.Stardew.LookupAnything.Framework.Fields;

/// <summary>A metadata field containing clickable links.</summary>
internal class LinkField : GenericField
{
    /*********
    ** Fields
    *********/
    /// <summary>Gets the subject the link points to, or <c>null</c> to stay on the current subject.</summary>
    private readonly Func<ISubject?> Subject;

    /*********
    ** Accessors
    *********/
    /// <inheritdoc />
    public override bool MayHaveLinks => true;


    /*********
    ** Public methods
    *********/
    /// <summary>Construct an instance.</summary>
    /// <param name="label">A short field label.</param>
    /// <param name="text">The link text.</param>
    /// <param name="subject">Gets the subject the link points to, or <c>null</c> to stay on the current subject.</param>
    public LinkField(string label, string text, Func<ISubject?> subject)
        : base(label, new FormattedText(text, Color.Blue))
    {
        this.Subject = subject;
    }

    /// <inheritdoc />
    public override bool TryGetLinkAt(int x, int y, [NotNullWhen(true)] out ISubject? subject)
    {
        if (base.TryGetLinkAt(x, y, out subject))
            return true;

        subject = this.Subject();
        return subject != null;
    }
}
