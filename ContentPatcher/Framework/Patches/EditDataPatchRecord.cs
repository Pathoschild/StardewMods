using ContentPatcher.Framework.Conditions;
using ContentPatcher.Framework.Tokens;

namespace ContentPatcher.Framework.Patches
{
    /// <summary>An entry in a data file to change.</summary>
    internal class EditDataPatchRecord
    {
        /*********
        ** Accessors
        *********/
        /// <summary>The unique key for the entry in the data file.</summary>
        public string Key { get; }

        /// <summary>The entry value to set.</summary>
        public TokenString Value { get; }


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="key">The unique key for the entry in the data file.</param>
        /// <param name="value">The entry value to set.</param>
        public EditDataPatchRecord(string key, TokenString value)
        {
            this.Key = key;
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
