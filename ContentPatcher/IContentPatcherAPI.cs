using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StardewModdingAPI;

namespace ContentPatcher
{
    public interface IContentPatcherAPI
    {
        /// <summary>Register a token. Tokens should be registerd in the GameLoop.GameLaunched event.</summary>
        /// <param name="mod">The manifest of the mod this token comes from.</param>
        /// <param name="name">The name of the token.</param>
        /// <param name="valueFunc">The token value, or null if it is currently unavailable.</param>
        void RegisterToken(IManifest mod, string name, Func<ITokenString, string[]> valueFunc);

        /// <summary>Update a token.</summary>
        /// <param name="mod">The manifest of the mod this token comes from.</param>
        /// <param name="name">The name of the token.</param>
        void UpdateToken(IManifest mod, string name);
    }
}
