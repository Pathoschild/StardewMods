using Pathoschild.Stardew.Common.Utilities;

namespace ContentPatcher.Framework
{
    /// <summary>Diagnostic info about a contextual object.</summary>
    internal interface IContextualState
    {
        /*********
        ** Accessors
        *********/
        /// <summary>Whether the instance is valid in general (ignoring the context).</summary>
        bool IsValid { get; }

        /// <summary>Whether <see cref="IsValid"/> and the instance is applicable in the current context.</summary>
        bool IsInScope { get; }

        /// <summary>Whether <see cref="IsInScope"/> and there are no issues preventing the contextual from being used.</summary>
        bool IsReady { get; }

        /// <summary>The unknown tokens required by the instance, if any.</summary>
        InvariantHashSet InvalidTokens { get; }

        /// <summary>The valid tokens required by the instance which aren't available in the current context, if any.</summary>
        InvariantHashSet UnavailableTokens { get; }

        /// <summary>Error phrases indicating why the instance is not ready to use, if any.</summary>
        InvariantHashSet Errors { get; }
    }
}
