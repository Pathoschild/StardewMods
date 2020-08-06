using System;
using System.Collections.Generic;
using System.Linq;
using ContentPatcher.Framework.Tokens.ValueProviders;
using Pathoschild.Stardew.Common.Utilities;

namespace ContentPatcher.Framework.Tokens
{
    /// <summary>High-level logic for a Content Patcher token wrapped around an underlying value provider.</summary>
    internal class Token : IToken
    {
        /*********
        ** Fields
        *********/
        /// <summary>The underlying value provider.</summary>
        protected IValueProvider Values { get; }


        /*********
        ** Accessors
        *********/
        /// <inheritdoc />
        public string Scope { get; }

        /// <inheritdoc />
        public virtual string Name { get; }

        /// <inheritdoc />
        public bool IsMutable => this.Values.IsMutable;

        /// <inheritdoc />
        public bool IsReady => this.Values.IsReady;

        /// <inheritdoc />
        public bool RequiresInput => this.Values.RequiresPositionalInput;


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="provider">The underlying value provider.</param>
        /// <param name="scope">The mod namespace in which the token is accessible, or <c>null</c> for any namespace.</param>
        public Token(IValueProvider provider, string scope = null)
        {
            this.Values = provider;
            this.Scope = scope;
            this.Name = provider.Name;
        }

        /// <inheritdoc />
        public virtual bool UpdateContext(IContext context)
        {
            return this.Values.UpdateContext(context);
        }

        /// <inheritdoc />
        public virtual IEnumerable<string> GetTokensUsed()
        {
            return this.Values.GetTokensUsed();
        }

        /// <inheritdoc />
        public virtual IContextualState GetDiagnosticState()
        {
            return this.Values.GetDiagnosticState();
        }

        /// <inheritdoc />
        public virtual bool CanHaveMultipleValues(IInputArguments input)
        {
            // 'contains' limited to true/false
            if (input.ReservedArgs.ContainsKey(InputArguments.ContainsKey))
                return false;

            // default logic
            return this.Values.CanHaveMultipleValues(input);
        }

        /// <inheritdoc />
        public virtual bool TryValidateInput(IInputArguments input, out string error)
        {
            return this.Values.TryValidateInput(input, out error);
        }

        /// <inheritdoc />
        public virtual bool TryValidateValues(IInputArguments input, InvariantHashSet values, IContext context, out string error)
        {
            // 'contains' limited to true/false
            if (input.ReservedArgs.ContainsKey(InputArguments.ContainsKey))
            {
                string[] invalidValues = values
                    .Where(p => !bool.TryParse(p, out bool _))
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .ToArray();
                if (invalidValues.Any())
                {
                    error = $"invalid values ({string.Join(", ", invalidValues)}); expected 'true' or 'false' when used with 'contains'.";
                    return false;
                }

                error = null;
                return true;
            }

            // default logic
            if (!this.TryValidateInput(input, out error) || !this.Values.TryValidateValues(input, values, out error))
                return false;

            error = null;
            return true;
        }

        /// <inheritdoc />
        public virtual InvariantHashSet GetAllowedInputArguments()
        {
            return this.Values.GetValidPositionalArgs();
        }

        /// <inheritdoc />
        public virtual bool HasBoundedValues(IInputArguments input, out InvariantHashSet allowedValues)
        {
            // 'contains' limited to true/false
            if (input.ReservedArgs.ContainsKey(InputArguments.ContainsKey))
            {
                allowedValues = InvariantHashSet.Boolean();
                return true;
            }

            // default logic
            return this.Values.HasBoundedValues(input, out allowedValues);
        }

        /// <inheritdoc />
        public virtual bool HasBoundedRangeValues(IInputArguments input, out int min, out int max)
        {
            // 'contains' limited to true/false
            if (input.ReservedArgs.ContainsKey(InputArguments.ContainsKey))
            {
                min = -1;
                max = -1;
                return false;
            }

            // default logic
            return this.Values.HasBoundedRangeValues(input, out min, out max);
        }

        /// <inheritdoc />
        public virtual IEnumerable<string> GetValues(IInputArguments input)
        {
            // get values
            var values = this.Values.GetValues(input);

            // apply contains
            if (input.ReservedArgs.TryGetValue(InputArguments.ContainsKey, out IInputArgumentValue rawSearch))
            {
                InvariantHashSet search = new InvariantHashSet(rawSearch.Parsed);
                bool match = search.Any() && values.Any(value => search.Contains(value));
                values = new[] { match.ToString() };
            }

            return values;
        }


        /*********
        ** Protected methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="name">The token name.</param>
        /// <param name="provider">The underlying value provider.</param>
        /// <param name="scope">The mod namespace in which the token is accessible, or <c>null</c> for any namespace.</param>
        protected Token(string name, IValueProvider provider, string scope = null)
            : this(provider, scope)
        {
            this.Name = name;
        }
    }
}
