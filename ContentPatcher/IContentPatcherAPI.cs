using ContentPatcher.Framework.Tokens.ValueProviders;
using StardewModdingAPI;

namespace ContentPatcher
{
    /// <summary>The Content Patcher API which other mods can access.</summary>
    internal interface IContentPatcherAPI
    {
        /*********
        ** Methods
        *********/
        /// <summary>Register a token.</summary>
        /// <param name="mod">The mod this token comes from.</param>
        /// <param name="valueProvider">The token value provider.</param>
        void RegisterToken(IManifest mod, IValueProvider valueProvider);
    }
}
