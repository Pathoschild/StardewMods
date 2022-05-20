using System.Collections.Generic;

namespace Pathoschild.Stardew.Common.Utilities
{
    /// <summary>A hash set optimized for immutable, case-insensitive, ordered string lookups.</summary>
    internal interface IInvariantSet : IReadOnlySet<string>
    {
        /*********
        ** Methods
        *********/
        /// <summary>Get an invariant set with the given value added to this set's values.</summary>
        /// <param name="other">The value to add.</param>
        IInvariantSet GetWith(string other);

        /// <summary>Get an invariant set with the given values added to this set's values.</summary>
        /// <param name="other">The values to add.</param>
        IInvariantSet GetWith(ICollection<string> other);

        /// <summary>Get an invariant set with the given value removed from this set's values.</summary>
        /// <param name="other">The value to remove.</param>
        IInvariantSet GetWithout(string other);

        /// <summary>Get an invariant set with the given values removed from this set's values.</summary>
        /// <param name="other">The values to remove.</param>
        IInvariantSet GetWithout(IEnumerable<string> other);
    }
}
