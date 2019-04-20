using System.Collections.Generic;
using ContentPatcher.Framework.Conditions;
using ContentPatcher.Framework.Tokens.Json;

namespace ContentPatcher.Framework.Patches
{
    /// <summary>A specific field in a data file to change.</summary>
    internal class EditDataPatchField
    {
        /*********
        ** Accessors
        *********/
        /// <summary>The unique key for the entry in the data file.</summary>
        public TokenString EntryKey { get; }

        /// <summary>The field index to change.</summary>
        public string FieldKey { get; }

        /// <summary>The entry value to set.</summary>
        public TokenisableJToken Value { get; }


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="entryKey">The unique key for the entry in the data file.</param>
        /// <param name="fieldKey">The field number to change.</param>
        /// <param name="value">The entry value to set.</param>
        public EditDataPatchField(TokenString entryKey, string fieldKey, TokenisableJToken value)
        {
            this.EntryKey = entryKey;
            this.FieldKey = fieldKey;
            this.Value = value;
        }

        /// <summary>Get all token strings used in the record.</summary>
        public IEnumerable<TokenString> GetTokenStrings()
        {
            yield return this.EntryKey;
            foreach (TokenString str in this.Value.GetTokenStrings())
                yield return str;
        }
    }
}
