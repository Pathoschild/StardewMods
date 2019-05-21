using System;
using System.Collections.Generic;
using System.Linq;
using ContentPatcher.Framework.Conditions;
using Newtonsoft.Json.Linq;

namespace ContentPatcher.Framework.Tokens.Json
{
    /// <summary>A JSON structure containing tokenisable values.</summary>
    internal class TokenisableJToken : IContextual
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
        public TokenisableJToken(JToken value, IContext context)
        {
            this.Value = value;
            this.Contextuals.Add(this.ResolveTokenisableFields(value, context));
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
        public IEnumerable<ITokenString> GetTokenStrings()
        {
            return this.Contextuals.Values.OfType<ITokenString>();
        }


        /*********
        ** Private methods
        *********/
        /// <summary>Find all tokenisable fields in a JSON structure, replace immutable tokens with their values, and get a list of mutable tokens.</summary>
        /// <param name="token">The JSON structure to scan.</param>
        /// <param name="context">Provides access to contextual tokens.</param>
        private IEnumerable<TokenisableProxy> ResolveTokenisableFields(JToken token, IContext context)
        {
            switch (token)
            {
                case JValue valueToken:
                    {
                        string value = valueToken.Value<string>();
                        TokenisableProxy proxy = this.TryResolveTokenisableFields(value, context, val => valueToken.Value = val);
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
                            TokenisableProxy proxy = this.TryResolveTokenisableFields(property.Name, context, val =>
                            {
                                var newProperty = new JProperty(val, property.Value);
                                property.Replace(newProperty);
                                property = newProperty;
                            });
                            if (proxy != null)
                                yield return proxy;
                        }

                        // resolve property values
                        foreach (TokenisableProxy contextual in this.ResolveTokenisableFields(property.Value, context))
                            yield return contextual;
                    }
                    break;

                case JArray arrToken:
                    foreach (JToken valueToken in arrToken)
                    {
                        foreach (TokenisableProxy contextual in this.ResolveTokenisableFields(valueToken, context))
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
        private TokenisableProxy TryResolveTokenisableFields(string str, IContext context, Action<string> setValue)
        {
            ITokenString tokenStr = new TokenString(str, context);

            // handle mutable token
            if (tokenStr.IsMutable)
                return new TokenisableProxy(tokenStr, setValue);

            // substitute immutable value
            if (tokenStr.Value != str)
                setValue(tokenStr.Value);
            return null;
        }
    }
}
