using System.Collections.Generic;
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
        private readonly AggregateContextual Contextuals;


        /*********
        ** Accessors
        *********/
        /// <summary>The unique key for the entry in the data file.</summary>
        public ITokenString Key { get; }

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
        public EditDataPatchRecord(ITokenString key, TokenisableJToken value)
        {
            this.Key = key;
            this.Value = value;

            this.Contextuals = new AggregateContextual()
                .Add(key)
                .Add(value);
        }

        /// <summary>Get all token strings used in the record.</summary>
        public IEnumerable<ITokenString> GetTokenStrings()
        {
            yield return this.Key;
            if (this.Value != null)
            {
                foreach (ITokenString str in this.Value.GetTokenStrings())
                    yield return str;
            }
        }

        /// <summary>Update the instance when the context changes.</summary>
        /// <param name="context">Provides access to contextual tokens.</param>
        /// <returns>Returns whether the instance changed.</returns>
        public bool UpdateContext(IContext context)
        {
            return this.Contextuals.UpdateContext(context);
        }

        /// <summary>Get the token names used by this patch in its fields.</summary>
        public IEnumerable<string> GetTokensUsed()
        {
            return this.Contextuals.GetTokensUsed();
        }
    }
}
