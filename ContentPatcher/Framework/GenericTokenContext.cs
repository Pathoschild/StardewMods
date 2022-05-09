using System;
using System.Collections.Generic;
using ContentPatcher.Framework.Tokens;
using Pathoschild.Stardew.Common.Utilities;

namespace ContentPatcher.Framework
{
    /// <summary>A generic token context.</summary>
    internal class GenericTokenContext : IContext
    {
        /*********
        ** Fields
        *********/
        /// <summary>Get whether a mod is installed.</summary>
        private readonly Func<string, bool> IsModInstalledImpl;

        /// <summary>Get the value for the <see cref="UpdateTick"/> field.</summary>
        private readonly Func<int> GetUpdateTick;


        /*********
        ** Accessors
        *********/
        /// <summary>The available tokens.</summary>
        public InvariantDictionary<IToken> Tokens { get; } = new();

        /// <inheritdoc />
        public int UpdateTick => this.GetUpdateTick();


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="isModInstalled">Get whether a mod is installed.</param>
        /// <param name="getUpdateTick">Get the value for the <see cref="IContext.UpdateTick"/> field.</param>
        public GenericTokenContext(Func<string, bool> isModInstalled, Func<int> getUpdateTick)
        {
            this.IsModInstalledImpl = isModInstalled;
            this.GetUpdateTick = getUpdateTick;
        }

        /// <inheritdoc />
        public bool IsModInstalled(string id)
        {
            return this.IsModInstalledImpl(id);
        }

        /// <summary>Save the given token to the context.</summary>
        /// <param name="token">The token to save.</param>
        public void Save(IToken token)
        {
            this.Tokens[token.Name] = token;
        }

        /// <summary>Remove a token from the context.</summary>
        /// <param name="name">The token name to remove.</param>
        public void Remove(string name)
        {
            this.Tokens.Remove(name);
        }

        /// <inheritdoc />
        public bool Contains(string name, bool enforceContext)
        {
            return this.GetToken(name, enforceContext) != null;
        }

        /// <inheritdoc />
        public IToken? GetToken(string name, bool enforceContext)
        {
            return this.Tokens.TryGetValue(name, out IToken? token) && this.ShouldConsider(token, enforceContext)
                ? token
                : null;
        }

        /// <inheritdoc />
        public IEnumerable<IToken> GetTokens(bool enforceContext)
        {
            foreach (IToken token in this.Tokens.Values)
            {
                if (this.ShouldConsider(token, enforceContext))
                    yield return token;
            }
        }

        /// <inheritdoc />
        public IEnumerable<string> GetValues(string name, IInputArguments input, bool enforceContext)
        {
            IToken? token = this.GetToken(name, enforceContext);
            return token?.GetValues(input) ?? Array.Empty<string>();
        }


        /*********
        ** Private methods
        *********/
        /// <summary>Get whether a given token should be considered.</summary>
        /// <param name="token">The token to check.</param>
        /// <param name="enforceContext">Whether to only consider tokens that are available in the context.</param>
        private bool ShouldConsider(IToken token, bool enforceContext)
        {
            return !enforceContext || token.IsReady;
        }
    }
}
