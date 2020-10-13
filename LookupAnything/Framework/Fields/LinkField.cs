using System;
using Microsoft.Xna.Framework;
using Pathoschild.Stardew.LookupAnything.Framework.Lookups;

namespace Pathoschild.Stardew.LookupAnything.Framework.Fields
{
    /// <summary>A metadata field containing clickable links.</summary>
    internal class LinkField : GenericField, ILinkField
    {
        /*********
        ** Fields
        *********/
        /// <summary>Gets the subject the link points to.</summary>
        private readonly Func<ISubject> Subject;


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="label">A short field label.</param>
        /// <param name="text">The link text.</param>
        /// <param name="subject">Gets the subject the link points to.</param>
        public LinkField(string label, string text, Func<ISubject> subject)
            : base(label, new FormattedText(text, Color.Blue))
        {
            this.Subject = subject;
        }

        /// <summary>Get the subject the link points to.</summary>
        public ISubject GetLinkSubject()
        {
            return this.Subject();
        }
    }
}
