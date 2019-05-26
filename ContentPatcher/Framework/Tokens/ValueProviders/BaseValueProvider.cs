using System;
using System.Collections.Generic;
using System.Linq;
using ContentPatcher.Framework.Conditions;
using Pathoschild.Stardew.Common.Utilities;

namespace ContentPatcher.Framework.Tokens.ValueProviders
{
    /// <summary>The base class for a value provider.</summary>
    internal abstract class BaseValueProvider : IValueProvider
    {
        /*********
        ** Fields
        *********/
        /// <summary>Whether multiple values may exist when no input is provided.</summary>
        protected bool CanHaveMultipleValuesForRoot { get; set; }

        /// <summary>Whether multiple values may exist when an input argument is provided.</summary>
        protected bool CanHaveMultipleValuesForInput { get; set; }

        /// <summary>Diagnostic info about the contextual instance.</summary>
        private readonly ContextualState State = new ContextualState();


        /*********
        ** Accessors
        *********/
        /// <summary>The value provider name.</summary>
        public string Name { get; }

        /// <summary>Whether the provided values can change after the provider is initialised.</summary>
        public bool IsMutable { get; protected set; } = true;

        /// <summary>Whether the instance is valid for the current context.</summary>
        public bool IsReady => this.State.IsReady;

        /// <summary>Whether the value provider allows an input argument (e.g. an NPC name for a relationship token).</summary>
        public bool AllowsInput { get; private set; }

        /// <summary>Whether the value provider requires an input argument to work, and does not provide values without it (see <see cref="IValueProvider.AllowsInput"/>).</summary>
        public bool RequiresInput { get; private set; }


        /*********
        ** Public methods
        *********/
        /// <summary>Update the instance when the context changes.</summary>
        /// <param name="context">Provides access to contextual tokens.</param>
        /// <returns>Returns whether the instance changed.</returns>
        public virtual bool UpdateContext(IContext context)
        {
            return false;
        }

        /// <summary>Get the token names used by this patch in its fields.</summary>
        public IEnumerable<string> GetTokensUsed()
        {
            return Enumerable.Empty<string>();
        }

        /// <summary>Get diagnostic info about the contextual instance.</summary>
        public IContextualState GetDiagnosticState()
        {
            return this.State.Clone();
        }

        /// <summary>Whether the value provider may return multiple values for the given input.</summary>
        /// <param name="input">The input argument, if applicable.</param>
        public bool CanHaveMultipleValues(ITokenString input = null)
        {
            return input.IsMeaningful()
                ? this.CanHaveMultipleValuesForInput
                : this.CanHaveMultipleValuesForRoot;
        }

        /// <summary>Validate that the provided input argument is valid.</summary>
        /// <param name="input">The input argument, if applicable.</param>
        /// <param name="error">The validation error, if any.</param>
        /// <returns>Returns whether validation succeeded.</returns>
        public bool TryValidateInput(ITokenString input, out string error)
        {
            // validate input
            if (input.IsMeaningful())
            {
                // check if input allowed
                if (!this.AllowsInput)
                {
                    error = $"invalid input argument ({input}), token {this.Name} doesn't allow input.";
                    return false;
                }

                // check value
                InvariantHashSet validInputs = this.GetValidInputs();
                if (validInputs?.Any() == true)
                {
                    if (!validInputs.Contains(input.Value))
                    {
                        error = $"invalid input argument ({(input.Raw != input.Value ? $"{input.Raw} => {input.Value}" : input.Value)}) for {this.Name} token, expected any of {string.Join(", ", validInputs)}";
                        return false;
                    }
                }
            }

            // no issues found
            error = null;
            return true;
        }

        /// <summary>Validate that the provided values are valid for the input argument (regardless of whether they match).</summary>
        /// <param name="input">The input argument, if applicable.</param>
        /// <param name="values">The values to validate.</param>
        /// <param name="error">The validation error, if any.</param>
        /// <returns>Returns whether validation succeeded.</returns>
        public bool TryValidateValues(ITokenString input, InvariantHashSet values, out string error)
        {
            if (!this.TryValidateInput(input, out error))
                return false;

            // default validation
            {
                InvariantHashSet validValues = this.GetAllowedValues(input);
                if (validValues?.Any() == true)
                {
                    string[] invalidValues = values
                        .Where(p => !validValues.Contains(p))
                        .Distinct()
                        .ToArray();
                    if (invalidValues.Any())
                    {
                        error = $"invalid values ({string.Join(", ", invalidValues)}); expected one of {string.Join(", ", validValues)}";
                        return false;
                    }
                }
            }

            // custom validation
            foreach (string value in values)
            {
                if (!this.TryValidate(input, value, out error))
                    return false;
            }

            // no issues found
            error = null;
            return true;
        }

        /// <summary>Get the set of valid input arguments if restricted, or an empty collection if unrestricted.</summary>
        public virtual InvariantHashSet GetValidInputs()
        {
            return new InvariantHashSet();
        }

        /// <summary>Get the allowed values for an input argument (or <c>null</c> if any value is allowed).</summary>
        /// <param name="input">The input argument, if applicable.</param>
        /// <exception cref="InvalidOperationException">The input argument doesn't match this value provider, or does not respect <see cref="IValueProvider.AllowsInput"/> or <see cref="IValueProvider.RequiresInput"/>.</exception>
        public virtual InvariantHashSet GetAllowedValues(ITokenString input)
        {
            return null;
        }

        /// <summary>Get the current values.</summary>
        /// <param name="input">The input argument, if applicable.</param>
        /// <exception cref="InvalidOperationException">The input argument doesn't match this value provider, or does not respect <see cref="IValueProvider.AllowsInput"/> or <see cref="IValueProvider.RequiresInput"/>.</exception>
        public virtual IEnumerable<string> GetValues(ITokenString input)
        {
            this.AssertInputArgument(input);
            yield break;
        }


        /*********
        ** Protected methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="name">The value provider name.</param>
        /// <param name="canHaveMultipleValuesForRoot">Whether the root value provider may contain multiple values.</param>
        protected BaseValueProvider(string name, bool canHaveMultipleValuesForRoot)
        {
            this.Name = name;
            this.CanHaveMultipleValuesForRoot = canHaveMultipleValuesForRoot;
        }

        /// <summary>Construct an instance.</summary>
        /// <param name="type">The value provider name.</param>
        /// <param name="canHaveMultipleValuesForRoot">Whether the root value provider may contain multiple values.</param>
        protected BaseValueProvider(ConditionType type, bool canHaveMultipleValuesForRoot)
            : this(type.ToString(), canHaveMultipleValuesForRoot) { }

        /// <summary>Validate that the provided value is valid for an input argument (regardless of whether they match).</summary>
        /// <param name="input">The input argument, if applicable.</param>
        /// <param name="value">The value to validate.</param>
        /// <param name="error">The validation error, if any.</param>
        /// <returns>Returns whether validation succeeded.</returns>
        protected virtual bool TryValidate(ITokenString input, string value, out string error)
        {
            error = null;
            return true;
        }

        /// <summary>Enable input arguments for this value provider.</summary>
        /// <param name="required">Whether an input argument is required when using this value provider.</param>
        /// <param name="canHaveMultipleValues">Whether the value provider may return multiple values for an input argument.</param>
        protected void EnableInputArguments(bool required, bool canHaveMultipleValues)
        {
            this.AllowsInput = true;
            this.RequiresInput = required;
            this.CanHaveMultipleValuesForInput = canHaveMultipleValues;
        }

        /// <summary>Assert that an input argument is valid for the value provider.</summary>
        /// <param name="input">The input argument to check, if applicable.</param>
        /// <exception cref="InvalidOperationException">The input argument doesn't match this value provider, or does not respect <see cref="AllowsInput"/> or <see cref="RequiresInput"/>.</exception>
        protected void AssertInputArgument(ITokenString input)
        {
            if (this.RequiresInput && !input.IsMeaningful())
                throw new InvalidOperationException($"The '{this.Name}' token requires an input argument.");
            if (!this.AllowsInput && input.IsMeaningful())
                throw new InvalidOperationException($"The '{this.Name}' token does not allow input arguments.");
        }

        /// <summary>Try to parse a raw case-insensitive string into an enum value.</summary>
        /// <typeparam name="TEnum">The enum type.</typeparam>
        /// <param name="raw">The raw string to parse.</param>
        /// <param name="result">The resulting enum value.</param>
        /// <param name="mustBeNamed">When parsing a numeric value, whether it must match one of the named enum values.</param>
        protected bool TryParseEnum<TEnum>(string raw, out TEnum result, bool mustBeNamed = true) where TEnum : struct
        {
            if (!Enum.TryParse(raw, true, out result))
                return false;

            if (mustBeNamed && !Enum.IsDefined(typeof(TEnum), result))
                return false;

            return true;
        }

        /// <summary>Set the value provider's ready flag.</summary>
        /// <param name="ready">Whether the provider is ready.</param>
        /// <returns>Returns the ready flag value.</returns>
        protected bool MarkReady(bool ready)
        {
            if (ready)
                this.State.Reset();
            else
                this.State.AddUnavailableTokens(this.Name);

            return ready;
        }

        /// <summary>Get whether the value provider's <see cref="IsReady"/> or values change when an action is invoked.</summary>
        /// <typeparam name="T">The value type.</typeparam>
        /// <param name="values">The underlying values to check.</param>
        /// <param name="action">The action to perform.</param>
        protected bool IsChanged<T>(HashSet<T> values, Action action)
        {
            return this.IsChanged(() =>
            {
                HashSet<T> oldValues = new HashSet<T>(values);
                action();
                return values.Count != oldValues.Count || values.Any(p => !oldValues.Contains(p));
            });
        }

        /// <summary>Get whether the value provider's <see cref="IsReady"/> or values change when an action is invoked.</summary>
        /// <param name="values">The underlying values to check.</param>
        /// <param name="action">The action to perform.</param>
        protected bool IsChanged(IDictionary<string, string> values, Action action)
        {
            return this.IsChanged(() =>
            {
                Dictionary<string, string> oldValues = new Dictionary<string, string>(values);
                action();
                return
                    values.Count != oldValues.Count
                    || oldValues.Any(entry => !values.TryGetValue(entry.Key, out string newValue) || entry.Value?.Equals(newValue, StringComparison.InvariantCultureIgnoreCase) != true);
            });
        }

        /// <summary>Get whether the value provider's <see cref="IsReady"/> or values change when an action is invoked.</summary>
        /// <param name="action">The action to perform, which returns true if the valus changed.</param>
        protected bool IsChanged(Func<bool> action)
        {
            bool wasReady = this.IsReady;
            return action() || this.IsReady != wasReady;
        }
    }
}
