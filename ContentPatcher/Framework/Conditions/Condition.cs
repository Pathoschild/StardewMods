using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using ContentPatcher.Framework.Tokens;
using Pathoschild.Stardew.Common.Utilities;

namespace ContentPatcher.Framework.Conditions
{
    /// <summary>A condition that can be checked against the token context.</summary>
    internal class Condition : IContextual
    {
        /*********
        ** Fields
        *********/
        /// <summary>The underlying contextual values.</summary>
        private readonly AggregateContextual Contextuals;

        /// <summary>Diagnostic info about the instance.</summary>
        private readonly ContextualState State = new();

        /// <summary>Whether the token represented by <see cref="Name"/> is mutable.</summary>
        private readonly bool IsTokenMutable;


        /*********
        ** Accessors
        *********/
        /// <summary>The token name in the context.</summary>
        public string Name { get; }

        /// <summary>The token input arguments.</summary>
        public IInputArguments Input { get; }

        /// <summary>The token values for which this condition is valid.</summary>
        public ITokenString Values { get; }

        /// <summary>The current values from <see cref="Values"/>.</summary>
        public InvariantHashSet? CurrentValues { get; private set; }

        /// <inheritdoc />
        public bool IsMutable => this.IsTokenMutable || this.Contextuals.IsMutable;

        /// <inheritdoc />
        [MemberNotNullWhen(true, nameof(Condition.CurrentValues))]
        public bool IsReady => this.Contextuals.IsReady && this.State.IsReady && this.CurrentValues != null;

        /// <summary>Whether the condition matches the current context.</summary>
        [MemberNotNullWhen(true, nameof(Condition.CurrentValues))]
        public bool IsMatch { get; private set; }


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="name">The token name in the context.</param>
        /// <param name="input">The token input arguments.</param>
        /// <param name="values">The token values for which this condition is valid.</param>
        /// <param name="isTokenMutable">Whether the token represented by <paramref name="name"/> is mutable.</param>
        public Condition(string name, IManagedTokenString input, IManagedTokenString values, bool isTokenMutable)
        {
            // save values
            this.Name = name;
            this.Input = new InputArguments(input);
            this.Values = values;
            this.IsTokenMutable = isTokenMutable;
            this.Contextuals = new AggregateContextual()
                .Add(input)
                .Add(values);

            // init immutable values
            if (this.Values.IsReady)
                this.CurrentValues = this.Values.SplitValuesUnique();
        }

        /// <summary>Get whether the condition is for a given condition type.</summary>
        /// <param name="type">The condition type.</param>
        public bool Is(ConditionType type)
        {
            return this.Name.EqualsIgnoreCase(type.ToString());
        }

        /// <inheritdoc />
        public bool UpdateContext(IContext context)
        {
            // skip unneeded updates
            if (!this.IsMutable && this.Contextuals.WasEverUpdated)
                return false;

            // reset
            bool wasReady = this.IsReady;
            bool wasMatch = this.IsMatch;
            this.State.Reset();

            // update contextuals
            bool changed = this.Contextuals.UpdateContext(context);

            // get token
            IToken? token = context.GetToken(this.Name, enforceContext: true);
            if (token == null)
                this.State.AddUnreadyTokens(this.Name);

            // update values
            if (this.IsReady && token != null)
            {
                this.CurrentValues = this.Values.SplitValuesUnique(token.NormalizeValue);
                this.IsMatch = token
                    .GetValues(this.Input)
                    .Any(value => this.CurrentValues.Contains(value));
            }
            else
            {
                this.CurrentValues = new InvariantHashSet();
                this.IsMatch = false;
            }

            return
                changed
                || wasReady != this.IsReady
                || wasMatch != this.IsMatch;
        }

        /// <inheritdoc />
        public IEnumerable<string> GetTokensUsed()
        {
            yield return this.Name;
            foreach (string token in this.Contextuals.GetTokensUsed())
                yield return token;
        }

        /// <inheritdoc />
        public IContextualState GetDiagnosticState()
        {
            return this.State.Clone()
                .MergeFrom(this.Contextuals.GetDiagnosticState());
        }
    }
}
