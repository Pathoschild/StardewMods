using System;
using System.Collections.Generic;
using System.Linq;
using ContentPatcher.Framework.Tokens;

namespace ContentPatcher.Framework
{
    /// <summary>Provides an aggregate wrapper for a collection of contextual values.</summary>
    internal class AggregateContextual : IContextual
    {
        /*********
        ** Fields
        *********/
        /// <summary>The underlying values.</summary>
        private readonly HashSet<IContextual> ValuesImpl = new();


        /*********
        ** Accessors
        *********/
        /// <summary>The tracked contextual values.</summary>
        public IEnumerable<IContextual> Values => this.ValuesImpl;

        /// <inheritdoc />
        public bool IsMutable { get; private set; }

        /// <inheritdoc />
        public bool IsReady { get; private set; } = true;

        /// <summary>Get whether the contextuals have ever been updated.</summary>
        public bool WasEverUpdated { get; private set; }


        /*********
        ** Accessors
        *********/
        /// <summary>Add contextual values to the tracker.</summary>
        /// <param name="value">The context value to track. Null values are ignored.</param>
        public AggregateContextual Add(IContextual? value)
        {
            if (value != null)
            {
                this.ValuesImpl.Add(value);
                this.IsMutable = this.IsMutable || value.IsMutable;
                this.IsReady = this.IsReady && value.IsReady;
            }

            return this;
        }

        /// <summary>Add contextual values to the tracker.</summary>
        /// <param name="values">The context values to track. Null values are ignored.</param>
        public AggregateContextual Add(IEnumerable<IContextual?>? values)
        {
            if (values != null)
            {
                foreach (IContextual? value in values)
                    this.Add(value);
            }

            return this;
        }

        /// <summary>Remove contextual values from the tracker.</summary>
        /// <param name="value">The context value to stop tracking. Null values are ignored.</param>
        public AggregateContextual Remove(IContextual? value)
        {
            if (value != null && this.ValuesImpl.Remove(value))
            {
                this.IsMutable = this.ValuesImpl.Any(p => p.IsMutable);
                this.IsReady = this.ValuesImpl.All(p => p.IsReady);
            }

            return this;
        }

        /// <summary>Remove contextual values from the tracker.</summary>
        /// <param name="values">The context values to stop tracking. Null values are ignored.</param>
        public AggregateContextual Remove(IEnumerable<IContextual?>? values)
        {
            if (values != null)
            {
                foreach (IContextual? value in values)
                    this.Remove(value);
            }

            return this;
        }

        /// <inheritdoc />
        bool IContextual.UpdateContext(IContext context)
        {
            return this.UpdateContext(context, update: null, countChange: null);
        }

        /// <summary>Update the instance when the context changes.</summary>
        /// <param name="context">Provides access to contextual tokens.</param>
        /// <param name="update">Matches contextuals to update, or <c>null</c> for all contextuals.</param>
        /// <param name="countChange">Matches contextuals which should be counted as a change for the return value if their state changed, or <c>null</c> for all contextuals.</param>
        /// <returns>Returns whether the instance changed.</returns>
        public bool UpdateContext(IContext context, Func<IContextual, bool>? update = null, Func<IContextual, bool>? countChange = null)
        {
            bool wasReady = this.IsReady;
            this.IsReady = true;

            bool changed = false;
            foreach (IContextual contextual in this.ValuesImpl)
            {
                if (update?.Invoke(contextual) == false)
                    continue;

                if (contextual.UpdateContext(context) && countChange?.Invoke(contextual) != false)
                    changed = true;

                if (!contextual.IsReady)
                    this.IsReady = false;
            }

            this.WasEverUpdated = true;
            return changed || this.IsReady != wasReady;
        }

        /// <inheritdoc />
        public IEnumerable<string> GetTokensUsed()
        {
            return this.ValuesImpl.SelectMany(p => p.GetTokensUsed());
        }

        /// <inheritdoc />
        public IContextualState GetDiagnosticState()
        {
            ContextualState state = new ContextualState();
            foreach (IContextual contextual in this.ValuesImpl)
                state.MergeFrom(contextual.GetDiagnosticState());
            return state;
        }
    }
}
