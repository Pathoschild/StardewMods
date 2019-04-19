using ContentPatcher.Framework.Tokens;

namespace ContentPatcher.Framework
{
    /// <summary>An instance which can receive token context updates.</summary>
    internal interface IContextual
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
        /// <summary>Update the instance when the context changes.</summary>
        /// <param name="context">Provides access to contextual tokens.</param>
        /// <returns>Returns whether the instance changed.</returns>
        bool UpdateContext(IContext context);
    }
}
