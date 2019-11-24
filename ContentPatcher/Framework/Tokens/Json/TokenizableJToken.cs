using System;
using System.Collections.Generic;
using ContentPatcher.Framework.Conditions;
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


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="value">The JSON object to modify.</param>
        /// <param name="context">Provides access to contextual tokens.</param>
        public TokenizableJToken(JToken value, IContext context)
        {
            this.Value = value;
            this.Contextuals.Add(this.ResolveTokenizableFields(value, context));
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
        public IEnumerable<IParsedTokenString> GetTokenStrings()
        {
            foreach (IContextual contextual in this.Contextuals.Values)
            {
                if (contextual is IParsedTokenString tokenStr)
                    yield return tokenStr;
                if (contextual is TokenizableProxy proxy)
                    yield return proxy.TokenString;
            }
        }


        /*********
        ** Private methods
        *********/
        /// <summary>Find all tokenizable fields in a JSON structure, replace immutable tokens with their values, and get a list of mutable tokens.</summary>
        /// <param name="token">The JSON structure to scan.</param>
        /// <param name="context">Provides access to contextual tokens.</param>
        private IEnumerable<TokenizableProxy> ResolveTokenizableFields(JToken token, IContext context)
        {
            switch (token)
            {
                case JValue valueToken:
                    {
                        string value = valueToken.Value<string>();
                        TokenizableProxy proxy = this.TryResolveTokenizableFields(value, context, val => valueToken.Value = val);
                        if (proxy != null)
                            yield return proxy;
                        break;
                    }

                case JObject objToken:
                    foreach (JProperty p in objToken.Properties())
                    {
                        JProperty property = p;

                        // resolve property name
                        {
                            TokenizableProxy proxy = this.TryResolveTokenizableFields(property.Name, context, val =>
                            {
                                var newProperty = new JProperty(val, property.Value);
                                property.Replace(newProperty);
                                property = newProperty;
                            });
                            if (proxy != null)
                                yield return proxy;
                        }

                        // resolve property values
                        foreach (TokenizableProxy contextual in this.ResolveTokenizableFields(property.Value, context))
                            yield return contextual;
                    }
                    break;

                case JArray arrToken:
                    foreach (JToken valueToken in arrToken)
                    {
                        foreach (TokenizableProxy contextual in this.ResolveTokenizableFields(valueToken, context))
                            yield return contextual;
                    }
                    break;

                default:
                    throw new InvalidOperationException($"Unknown JSON token: {token.GetType().FullName} ({token.Type})");
            }
        }

        /// <summary>Resolve tokens in a string field, replace immutable tokens with their values, and get mutable tokens.</summary>
        /// <param name="str">The string to scan.</param>
        /// <param name="context">Provides access to contextual tokens.</param>
        /// <param name="setValue">Update the source with a new value.</param>
        private TokenizableProxy TryResolveTokenizableFields(string str, IContext context, Action<string> setValue)
        {
            IParsedTokenString tokenStr = new TokenString(str, context);

            // handle mutable token
            if (tokenStr.IsMutable)
                return new TokenizableProxy(tokenStr, setValue);

            // substitute immutable value
            if (tokenStr.Value != str)
                setValue(tokenStr.Value);
            return null;
        }
    }
}
