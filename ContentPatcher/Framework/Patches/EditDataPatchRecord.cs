using System.Collections.Generic;
using System.Linq;
using ContentPatcher.Framework.Conditions;
using ContentPatcher.Framework.Tokens;
using ContentPatcher.Framework.Tokens.Json;

namespace ContentPatcher.Framework.Patches
{
    /// <summary>An entry in a data file to change.</summary>
    internal class EditDataPatchRecord : IContextual
    {
        /*********
        ** Fields
        *********/
        /// <summary>The underlying contextual values.</summary>
        private readonly IContextual[] ContextualValues;


        /*********
        ** Accessors
        *********/
        /// <summary>The unique key for the entry in the data file.</summary>
        public TokenString Key { get; }

        /// <summary>The entry value to set.</summary>
        public TokenisableJToken Value { get; }

        /// <summary>Whether the instance may change depending on the context.</summary>
        public bool IsMutable => this.Key.IsMutable || (this.Value?.IsMutable == true);

        /// <summary>Whether the instance is valid for the current context.</summary>
        public bool IsReady => this.Key.IsReady && this.Value?.IsReady != false;


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="key">The unique key for the entry in the data file.</param>
        /// <param name="value">The entry value to set.</param>
        public EditDataPatchRecord(TokenString key, TokenisableJToken value)
        {
            this.Key = key;
            this.Value = value;

            this.ContextualValues = new IContextual[] { key, value }.Where(p => p != null).ToArray();
        }

        /// <summary>Get all token strings used in the record.</summary>
        public IEnumerable<TokenString> GetTokenStrings()
        {
            yield return this.Key;
            if (this.Value != null)
            {
                foreach (TokenString str in this.Value.GetTokenStrings())
                    yield return str;
            }
        }

        /// <summary>Update the instance when the context changes.</summary>
        /// <param name="context">Provides access to contextual tokens.</param>
        /// <returns>Returns whether the instance changed.</returns>
        public bool UpdateContext(IContext context)
        {
            bool changed = false;

            foreach (IContextual value in this.ContextualValues)
            {
                if (value.UpdateContext(context))
                    changed = true;
            }

            return changed;
        }
    }
}
