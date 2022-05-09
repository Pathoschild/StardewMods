using Pathoschild.Stardew.Common.Utilities;

namespace ContentPatcher.Framework.ConfigModels
{
    /// <summary>A user-defined token whose value may depend on other tokens.</summary>
    internal class DynamicTokenConfig
    {
        /*********
        ** Accessors
        *********/
        /// <summary>The name of the token to set.</summary>
        public string? Name { get; }

        /// <summary>The value to set.</summary>
        public string? Value { get; }

        /// <summary>The criteria to apply. See the README for valid values.</summary>
        public InvariantDictionary<string?> When { get; }


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="name">The name of the token to set.</param>
        /// <param name="value">The value to set.</param>
        /// <param name="when">The criteria to apply. See the README for valid values.</param>
        public DynamicTokenConfig(string name, string value, InvariantDictionary<string?>? when)
        {
            this.Name = name;
            this.Value = value;
            this.When = when ?? new();
        }
    }
}
