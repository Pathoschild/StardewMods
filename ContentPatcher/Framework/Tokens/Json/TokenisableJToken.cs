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
        /// <summary>The JSON fields whose values may change based on the context.</summary>
        private readonly TokenisableProxy[] TokenisableFields;


        /*********
        ** Accessors
        *********/
        /// <summary>The underlying JSON structure.</summary>
        public JToken Value { get; }

        /// <summary>Whether the instance may change depending on the context.</summary>
        public bool IsMutable { get; }

        /// <summary>Whether the instance is valid for the current context.</summary>
        public bool IsReady { get; private set; }


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="value">The JSON object to modify.</param>
        /// <param name="context">Provides access to contextual tokens.</param>
        public TokenisableJToken(JToken value, IContext context)
        {
            this.Value = value;
            this.TokenisableFields = this.ResolveTokenisableFields(value, context).ToArray();
            this.IsMutable = this.TokenisableFields.Any(p => p.IsMutable);
            this.IsReady = !this.TokenisableFields.Any() || this.TokenisableFields.All(p => p.IsReady);
        }

        /// <summary>Update the instance when the context changes.</summary>
        /// <param name="context">Provides access to contextual tokens.</param>
        /// <returns>Returns whether the instance changed.</returns>
        public bool UpdateContext(IContext context)
        {
            bool changed = false;

            foreach (IContextual field in this.TokenisableFields)
            {
                if (field.UpdateContext(context))
                    changed = true;
            }
            this.IsReady = this.TokenisableFields.All(p => p.IsReady);

            return changed;
        }

        /// <summary>Get the token names used by this patch in its fields.</summary>
        public IEnumerable<string> GetTokensUsed()
        {
            return this.TokenisableFields.SelectMany(p => p.GetTokensUsed());
        }

        /// <summary>Get the token strings contained in the JSON structure.</summary>
        public IEnumerable<ITokenString> GetTokenStrings()
        {
            return this.TokenisableFields.Select(p => p.TokenString);
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
