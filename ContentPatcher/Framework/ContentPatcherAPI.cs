using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ContentPatcher.Framework.Tokens;
using ContentPatcher.Framework.Tokens.ValueProviders;
using StardewModdingAPI;

namespace ContentPatcher.Framework
{
    public class ContentPatcherAPI : IContentPatcherAPI
    {
        /*********
        ** Fields
        *********/
        /// <summary>Encapsulates monitoring and logging.</summary>
        private readonly IMonitor Monitor;

        /// <summary>The action to add a mod token.</summary>
        private readonly Action<IToken> AddModToken;

        /// <summary>The action to add a token for pending partial context update.</summary>
        private readonly Action<string> AddPendingTokenUpdate;

        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="monitor">Encapsulates monitoring and logging.</param>
        internal ContentPatcherAPI(IMonitor monitor, Action<IToken> addModToken, Action<string> addPendingTokenUpdate)
        {
            this.Monitor = monitor;
            this.AddModToken = addModToken;
            this.AddPendingTokenUpdate = addPendingTokenUpdate;
        }

        /// <summary>Register a token.</summary>
        /// <param name="mod">The mod this token comes from.</param>
        /// <param name="name">The name of the token.</param>
        /// <param name="valueFunc">The function providing values.</param>
        public void RegisterToken(IManifest mod, string name, Func<ITokenString, string[]> valueFunc)
        {
            string tokenScope = mod.UniqueID;
            this.Monitor.Log($"Registering token through API: {mod.UniqueID}/{name}");

            IToken token = new GenericToken(new ModValueProvider(name, valueFunc), tokenScope);
            this.AddModToken(token);
        }

        public void UpdateToken(IManifest mod, string name)
        {
            string tokenScope = mod.UniqueID;
            this.AddPendingTokenUpdate(tokenScope + InternalConstants.TokenScopeSeparator + name);
        }
    }
}
