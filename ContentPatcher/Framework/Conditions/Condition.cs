using System.Collections.Generic;
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
        private readonly ContextualState State = new ContextualState();


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
        public InvariantHashSet CurrentValues { get; private set; }

        /// <summary>Whether the instance may change depending on the context.</summary>
        public bool IsMutable => this.Contextuals.IsMutable;

        /// <summary>Whether the instance is valid for the current context.</summary>
        public bool IsReady => this.Contextuals.IsReady && this.State.IsReady;

        /// <summary>Whether the condition matches the current context.</summary>
        public bool IsMatch { get; set; }


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="name">The token name in the context.</param>
        /// <param name="input">The token input arguments.</param>
        /// <param name="values">The token values for which this condition is valid.</param>
        public Condition(string name, IManagedTokenString input, IManagedTokenString values)
        {
            // save values
            this.Name = name;
            this.Input = new InputArguments(input);
            this.Values = values;
            this.Contextuals = new AggregateContextual()
                .Add(input)
                .Add(values);

            // init values
            if (this.IsReady)
                this.CurrentValues = this.Values.SplitValuesUnique();
        }

        /// <summary>Get whether the condition is for a given condition type.</summary>
        /// <param name="type">The condition type.</param>
        public bool Is(ConditionType type)
        {
            return this.Name.EqualsIgnoreCase(type.ToString());
        }

        /// <summary>Update the instance when the context changes.</summary>
        /// <param name="context">Provides access to contextual tokens.</param>
        /// <returns>Returns whether the instance changed.</returns>
        public bool UpdateContext(IContext context)
        {
            // reset
            bool wasReady = this.IsReady;
            bool wasMatch = this.IsMatch;
            this.State.Reset();

            // update contextuals
            bool changed = this.Contextuals.UpdateContext(context);

            // check token name
            if (!context.Contains(this.Name, enforceContext: true))
                this.State.AddUnreadyTokens(this.Name);

            // update values
            this.CurrentValues = this.IsReady
                ? this.Values.SplitValuesUnique()
                : new InvariantHashSet();

            // update match
            this.IsMatch =
                this.IsReady
                && context
                    .GetValues(this.Name, this.Input, enforceContext: true)
                    .Any(value => this.CurrentValues.Contains(value));

            return
                changed
                || wasReady != this.IsReady
                || wasMatch != this.IsMatch;
        }

        /// <summary>Get the token names used by this patch in its fields.</summary>
        public IEnumerable<string> GetTokensUsed()
        {
            yield return this.Name;
            foreach (string token in this.Contextuals.GetTokensUsed())
                yield return token;
        }

        /// <summary>Get diagnostic info about the contextual instance.</summary>
        public IContextualState GetDiagnosticState()
        {
            return this.State.Clone()
                .MergeFrom(this.Contextuals.GetDiagnosticState());
        }
    }
}
