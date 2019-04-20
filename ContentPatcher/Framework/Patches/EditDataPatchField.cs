using System.Collections.Generic;
using System.Linq;
using ContentPatcher.Framework.Conditions;
using ContentPatcher.Framework.Tokens;
using ContentPatcher.Framework.Tokens.Json;

namespace ContentPatcher.Framework.Patches
{
    /// <summary>A specific field in a data file to change.</summary>
    internal class EditDataPatchField : IContextual
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
        public TokenString EntryKey { get; }

        /// <summary>The field index to change.</summary>
        public TokenString FieldKey { get; }

        /// <summary>The entry value to set.</summary>
        public TokenisableJToken Value { get; }

        /// <summary>Whether the instance may change depending on the context.</summary>
        public bool IsMutable => this.EntryKey.IsMutable || (this.Value?.IsMutable == true);

        /// <summary>Whether the instance is valid for the current context.</summary>
        public bool IsReady => this.EntryKey.IsReady && this.Value?.IsReady != false;


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="entryKey">The unique key for the entry in the data file.</param>
        /// <param name="fieldKey">The field number to change.</param>
        /// <param name="value">The entry value to set.</param>
        public EditDataPatchField(TokenString entryKey, TokenString fieldKey, TokenisableJToken value)
        {
            this.EntryKey = entryKey;
            this.FieldKey = fieldKey;
            this.Value = value;

            this.ContextualValues = new IContextual[] { entryKey, fieldKey, value }.Where(p => p != null).ToArray();
        }

        /// <summary>Get all token strings used in the record.</summary>
        public IEnumerable<TokenString> GetTokenStrings()
        {
            yield return this.EntryKey;
            yield return this.FieldKey;
            foreach (TokenString str in this.Value.GetTokenStrings())
                yield return str;
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
