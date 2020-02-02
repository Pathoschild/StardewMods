using ContentPatcher.Framework.Tokens;

namespace ContentPatcher.Framework
{
    /// <summary>An instance which can receive token context updates.</summary>
    internal interface IContextual : IContextualInfo
    {
        /*********
        ** Methods
        *********/
        /// <summary>Update the instance when the context changes.</summary>
        /// <param name="context">Provides access to contextual tokens.</param>
        /// <returns>Returns whether the instance changed.</returns>
        bool UpdateContext(IContext context);

        /// <summary>Get diagnostic info about the contextual instance.</summary>
        IContextualState GetDiagnosticState();
    }
}
