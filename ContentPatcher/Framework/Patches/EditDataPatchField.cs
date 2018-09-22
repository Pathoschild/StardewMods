using ContentPatcher.Framework.Conditions;
using ContentPatcher.Framework.Tokens;

namespace ContentPatcher.Framework.Patches
{
    /// <summary>An specific field in a data file to change.</summary>
    internal class EditDataPatchField
    {
        /*********
        ** Accessors
        *********/
        /// <summary>The unique key for the entry in the data file.</summary>
        public string Key { get; }

        /// <summary>The field index to change.</summary>
        public int FieldIndex { get; }

        /// <summary>The entry value to set.</summary>
        public TokenString Value { get; }


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="key">The unique key for the entry in the data file.</param>
        /// <param name="field">The field number to change.</param>
        /// <param name="value">The entry value to set.</param>
        public EditDataPatchField(string key, int field, TokenString value)
        {
            this.Key = key;
            this.FieldIndex = field;
            this.Value = value;
        }

        /// <summary>Update the patch data when the context changes.</summary>
        /// <param name="context">Provides access to contextual tokens.</param>
        /// <returns>Returns whether the patch data changed.</returns>
        public bool UpdateContext(IContext context)
        {
            return this.Value.UpdateContext(context);
        }
    }
}
