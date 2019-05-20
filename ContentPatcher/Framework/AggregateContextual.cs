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
        private readonly HashSet<IContextual> ValuesImpl = new HashSet<IContextual>();


        /*********
        ** Accessors
        *********/
        /// <summary>The tracked contextual values.</summary>
        public IEnumerable<IContextual> Values => this.ValuesImpl;

        /// <summary>Whether the instance may change depending on the context.</summary>
        public bool IsMutable { get; private set; } = false;

        /// <summary>Whether the instance is valid for the current context.</summary>
        public bool IsReady { get; private set; } = true;


        /*********
        ** Accessors
        *********/
        /// <summary>Add contextual values to the tracker.</summary>
        /// <param name="value">The context value to track. Null values are ignored.</param>
        public AggregateContextual Add(IContextual value)
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
        public AggregateContextual Add(IEnumerable<IContextual> values)
        {
            foreach (IContextual value in values)
                this.Add(value);

            return this;
        }

        /// <summary>Update the instance when the context changes.</summary>
        /// <param name="context">Provides access to contextual tokens.</param>
        /// <returns>Returns whether the instance changed.</returns>
        public bool UpdateContext(IContext context)
        {
            bool wasReady = this.IsReady;
            this.IsReady = true;

            bool changed = false;
            foreach (IContextual contextual in this.ValuesImpl)
            {
                if (contextual.UpdateContext(context))
                    changed = true;

                if (!contextual.IsReady)
                    this.IsReady = false;
            }

            return changed || this.IsReady != wasReady;
        }

        /// <summary>Get the token names used by this patch in its fields.</summary>
        public IEnumerable<string> GetTokensUsed()
        {
            return this.ValuesImpl.SelectMany(p => p.GetTokensUsed());
        }
    }
}
