using ContentPatcher.Framework.Tokens.ValueProviders;
using StardewModdingAPI;

namespace ContentPatcher.Framework.Tokens
{
    /// <summary>A token provided by a mod.</summary>
    internal class ModProvidedToken : GenericToken
    {
        /*********
        ** Accessors
        *********/
        /// <summary>The mod which registered the token.</summary>
        public IManifest Mod { get; }


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="name">The token name.</param>
        /// <param name="mod">The mod which registered the token.</param>
        /// <param name="provider">The underlying value provider.</param>
        public ModProvidedToken(string name, IManifest mod, IValueProvider provider)
            : base(name, provider)
        {
            this.Mod = mod;
        }
    }
}
