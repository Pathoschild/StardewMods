using System.Collections.Generic;
using Newtonsoft.Json.Linq;

namespace ContentPatcher.Framework.Tokens.Json
{
    /// <summary>A JSON structure containing tokenizable values.</summary>
    internal class TokenizableJToken : IContextual
    {
        /*********
        ** Fields
        *********/
        /// <summary>The underlying contextual values.</summary>
        private readonly AggregateContextual Contextuals = new AggregateContextual();


        /*********
        ** Accessors
        *********/
        /// <summary>The underlying JSON structure.</summary>
        public JToken Value { get; }

        /// <summary>Whether the instance may change depending on the context.</summary>
        public bool IsMutable => this.Contextuals.IsMutable;

        /// <summary>Whether the instance is valid for the current context.</summary>
        public bool IsReady => this.Contextuals.IsReady;

        /// <summary>Whether the tokenizable token represents a string value, instead of an object or array.</summary>
        public bool IsString => this.Value is JValue;


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="value">The JSON object to modify.</param>
        /// <param name="proxyFields">The tokenizable fields which can be updated to change tokens in the JSON structure.</param>
        public TokenizableJToken(JToken value, IEnumerable<TokenizableProxy> proxyFields)
        {
            this.Value = value;
            this.Contextuals.Add(proxyFields);
        }

        /// <summary>Update the instance when the context changes.</summary>
        /// <param name="context">Provides access to contextual tokens.</param>
        /// <returns>Returns whether the instance changed.</returns>
        public bool UpdateContext(IContext context)
        {
            return this.Contextuals.UpdateContext(context);
        }

        /// <summary>Get the token names used by this patch in its fields.</summary>
        public IEnumerable<string> GetTokensUsed()
        {
            return this.Contextuals.GetTokensUsed();
        }

        /// <summary>Get diagnostic info about the contextual instance.</summary>
        public IContextualState GetDiagnosticState()
        {
            return this.Contextuals.GetDiagnosticState();
        }

        /// <summary>Get the token strings contained in the JSON structure.</summary>
        public IEnumerable<IManagedTokenString> GetTokenStrings()
        {
            foreach (IContextual contextual in this.Contextuals.Values)
            {
                if (contextual is IManagedTokenString tokenStr)
                    yield return tokenStr;
                if (contextual is TokenizableProxy proxy)
                    yield return proxy.TokenString;
            }
        }
    }
}
