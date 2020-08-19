using System;
using System.Collections.Generic;
using System.Linq;
using ContentPatcher.Framework.Conditions;
using Pathoschild.Stardew.Common;
using Pathoschild.Stardew.Common.Utilities;
using StardewValley;

namespace ContentPatcher.Framework.Tokens.ValueProviders
{
    /// <summary>The base class for a value provider.</summary>
    internal abstract class BaseValueProvider : IValueProvider
    {
        /*********
        ** Fields
        *********/
        /// <summary>Whether multiple values may exist when no input is provided.</summary>
        protected bool MayReturnMultipleValuesForRoot { get; set; }

        /// <summary>Whether multiple values may exist when input arguments are provided.</summary>
        protected bool MayReturnMultipleValuesForInput { get; set; }

        /// <summary>The named input arguments recognised by this value provider.</summary>
        protected readonly ISet<string> ValidNamedArguments = new InvariantHashSet();

        /// <summary>Diagnostic info about the contextual instance.</summary>
        private readonly ContextualState State = new ContextualState();

        /// <summary>The maximum number of positional arguments allowed, if limited.</summary>
        private int? MaxPositionalArgs;


        /*********
        ** Accessors
        *********/
        /// <inheritdoc />
        public string Name { get; }

        /// <inheritdoc />
        public bool IsMutable { get; protected set; } = true;

        /// <inheritdoc />
        public bool IsReady => this.State.IsReady;

        /// <inheritdoc />
        public bool AllowsPositionalInput { get; private set; }

        /// <inheritdoc />
        public bool RequiresPositionalInput { get; private set; }


        /*********
        ** Public methods
        *********/
        /// <inheritdoc />
        public virtual bool UpdateContext(IContext context)
        {
            return false;
        }

        /// <inheritdoc />
        public virtual IEnumerable<string> GetTokensUsed()
        {
            return Enumerable.Empty<string>();
        }

        /// <inheritdoc />
        public IContextualState GetDiagnosticState()
        {
            return this.State.Clone();
        }

        /// <inheritdoc />
        public bool CanHaveMultipleValues(IInputArguments input)
        {
            return input.HasPositionalArgs
                ? this.MayReturnMultipleValuesForInput
                : this.MayReturnMultipleValuesForRoot;
        }

        /// <inheritdoc />
        public virtual bool TryValidateInput(IInputArguments input, out string error)
        {
            if (input.IsReady)
            {
                // validate positional arguments
                if (input.HasPositionalArgs)
                {
                    // check if input allowed
                    if (!this.AllowsPositionalInput)
                    {
                        error = $"invalid input arguments ({input.TokenString.Value}), token {this.Name} doesn't allow input.";
                        return false;
                    }

                    // check argument count
                    if (input.PositionalArgs.Length > this.MaxPositionalArgs)
                    {
                        error = $"invalid input arguments ({input.TokenString.Value}), token {this.Name} doesn't allow more than {this.MaxPositionalArgs} argument{(this.MaxPositionalArgs == 1 ? "" : "s")}.";
                        return false;
                    }

                    // check values
                    InvariantHashSet validInputs = this.GetValidPositionalArgs();
                    if (validInputs?.Any() == true)
                    {
                        if (input.PositionalArgs.Any(arg => !validInputs.Contains(arg)))
                        {
                            string raw = input.TokenString.Raw;
                            string parsed = input.TokenString.Value;
                            error = $"invalid input arguments ({(raw != parsed ? $"{raw} => {parsed}" : parsed)}) for {this.Name} token, expected any of '{string.Join("', '", validInputs.OrderByIgnoreCase(p => p))}'";
                            return false;
                        }
                    }
                }

                // validate named arguments
                if (input.HasNamedArgs)
                {
                    if (this.ValidNamedArguments != null)
                    {
                        if (!this.ValidNamedArguments.Any())
                        {
                            error = $"invalid named argument '{input.NamedArgs.First().Key}' for {this.Name} token, which does not accept any named arguments.";
                            return false;
                        }

                        string invalidKey = (from arg in input.NamedArgs where !this.ValidNamedArguments.Contains(arg.Key) select arg.Key).FirstOrDefault();
                        if (invalidKey != null)
                        {
                            error = $"invalid named argument '{invalidKey}' for {this.Name} token, expected any of '{string.Join("', '", this.ValidNamedArguments.OrderByIgnoreCase(p => p))}'";
                            return false;
                        }
                    }
                }
            }

            // no issues found
            error = null;
            return true;
        }

        /// <inheritdoc />
        public virtual bool TryValidateValues(IInputArguments input, InvariantHashSet values, out string error)
        {
            if (!this.TryValidateInput(input, out error))
                return false;

            // validate bounded values
            if (this.HasBoundedRangeValues(input, out int min, out int max))
            {
                string[] invalidValues = values
                    .Where(p => !int.TryParse(p, out int val) || val < min || val > max)
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .ToArray();

                if (invalidValues.Any())
                {
                    error = $"invalid values ({string.Join(", ", invalidValues)}); expected an integer between {min} and {max}.";
                    return false;
                }
            }
            else if (this.HasBoundedValues(input, out InvariantHashSet validValues))
            {
                string[] invalidValues = values
                    .Where(p => !validValues.Contains(p))
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .ToArray();

                if (invalidValues.Any())
                {
                    error = $"invalid values ({string.Join(", ", invalidValues)}); expected one of {string.Join(", ", validValues)}";
                    return false;
                }
            }

            // no issues found
            error = null;
            return true;
        }

        /// <inheritdoc />
        public virtual InvariantHashSet GetValidPositionalArgs()
        {
            return new InvariantHashSet();
        }

        /// <inheritdoc />
        public virtual bool HasBoundedValues(IInputArguments input, out InvariantHashSet allowedValues)
        {
            allowedValues = null;
            return false;
        }

        /// <inheritdoc />
        public virtual bool HasBoundedRangeValues(IInputArguments input, out int min, out int max)
        {
            min = 0;
            max = 0;
            return false;
        }

        /// <inheritdoc />
        public virtual IEnumerable<string> GetValues(IInputArguments input)
        {
            this.AssertInput(input);
            yield break;
        }


        /*********
        ** Protected methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="name">The value provider name.</param>
        /// <param name="mayReturnMultipleValuesForRoot">Whether the root value provider may contain multiple values.</param>
        protected BaseValueProvider(string name, bool mayReturnMultipleValuesForRoot)
        {
            this.Name = name;
            this.MayReturnMultipleValuesForRoot = mayReturnMultipleValuesForRoot;
        }

        /// <summary>Construct an instance.</summary>
        /// <param name="type">The value provider name.</param>
        /// <param name="mayReturnMultipleValuesForRoot">Whether the root value provider may contain multiple values.</param>
        protected BaseValueProvider(ConditionType type, bool mayReturnMultipleValuesForRoot)
            : this(type.ToString(), mayReturnMultipleValuesForRoot) { }

        /// <summary>Enable input arguments for this value provider.</summary>
        /// <param name="required">Whether input arguments are required when using this value provider.</param>
        /// <param name="mayReturnMultipleValues">Whether the value provider may return multiple values for input arguments.</param>
        /// <param name="maxPositionalArgs">The maximum number of positional arguments allowed, if limited.</param>
        protected void EnableInputArguments(bool required, bool mayReturnMultipleValues, int? maxPositionalArgs)
        {
            this.AllowsPositionalInput = true;
            this.RequiresPositionalInput = required;
            this.MayReturnMultipleValuesForInput = mayReturnMultipleValues;
            this.MaxPositionalArgs = maxPositionalArgs;
        }

        /// <summary>Assert that the given input arguments are valid for the value provider.</summary>
        /// <param name="input">The input arguments.</param>
        /// <exception cref="InvalidOperationException">The input arguments don't match this value provider.</exception>
        protected void AssertInput(IInputArguments input)
        {
            if (this.RequiresPositionalInput && !input.HasPositionalArgs)
                throw new InvalidOperationException($"The '{this.Name}' token requires input arguments.");
            if (!this.AllowsPositionalInput && input.HasPositionalArgs)
                throw new InvalidOperationException($"The '{this.Name}' token does not allow input arguments.");
            if (input.PositionalArgs.Length > this.MaxPositionalArgs)
                throw new InvalidOperationException($"The '{this.Name}' token doesn't allow more than {this.MaxPositionalArgs} input argument{(this.MaxPositionalArgs == 1 ? "" : "s")}.");
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
                this.State.AddUnreadyTokens(this.Name);

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
                return this.IsChanged(oldValues, values);
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
                    || oldValues.Any(entry => !values.TryGetValue(entry.Key, out string newValue) || entry.Value?.EqualsIgnoreCase(newValue) != true);
            });
        }

        /// <summary>Get whether the value provider's <see cref="IsReady"/> or values change when an action is invoked.</summary>
        /// <param name="action">The action to perform, which returns true if the values changed.</param>
        protected bool IsChanged(Func<bool> action)
        {
            bool wasReady = this.IsReady;
            return action() || this.IsReady != wasReady;
        }

        /// <summary>Get all social NPCs.</summary>
        protected IEnumerable<NPC> GetSocialVillagers()
        {
            foreach (NPC npc in this.GetAllCharacters())
            {
                bool isSocial =
                    npc.CanSocialize
                    || (npc.Name == "Krobus" && !Game1.player.friendshipData.ContainsKey(npc.Name)); // Krobus is marked non-social before he's met

                if (isSocial)
                    yield return npc;
            }
        }

        /// <summary>Get all characters in reachable locations.</summary>
        /// <remarks>This is similar to <see cref="Utility.getAllCharacters()"/>, but doesn't sometimes crash when a farmhand warps and <see cref="Game1.currentLocation"/> isn't set yet.</remarks>
        protected IEnumerable<NPC> GetAllCharacters()
        {
            return CommonHelper
                .GetLocations()
                .SelectMany(p => p.characters)
                .Distinct(new ObjectReferenceComparer<NPC>());
        }

        /// <summary>Get whether the values in a collection changed.</summary>
        /// <typeparam name="T">The value type.</typeparam>
        /// <param name="oldValues">The old values to check.</param>
        /// <param name="newValues">The new values to check.</param>
        protected bool IsChanged<T>(HashSet<T> oldValues, HashSet<T> newValues)
        {
            return newValues.Count != oldValues.Count || newValues.Any(p => !oldValues.Contains(p));
        }
    }
}
