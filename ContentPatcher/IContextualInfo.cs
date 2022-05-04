using System.Collections.Generic;

namespace ContentPatcher
{
    /// <summary>Basic metadata about an instance which can receive token context updates.</summary>
    public interface IContextualInfo
    {
        /*********
        ** Accessors
        *********/
        /// <summary>Whether the instance may change depending on the context.</summary>
        bool IsMutable { get; }

        /// <summary>Whether the instance is valid for the current context.</summary>
        bool IsReady { get; }


        /*********
        ** Methods
        *********/
        /// <summary>Get the token names used by this patch in its fields.</summary>
        IEnumerable<string> GetTokensUsed();
    }
}
