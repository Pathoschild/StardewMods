namespace ContentPatcher.Framework.ConfigModels
{
    /// <summary>The raw schema for a field in the <c>config.json</c> file.</summary>
    internal class ConfigSchemaFieldConfig
    {
        /*********
        ** Accessors
        *********/
        /// <summary>The comma-delimited values to allow.</summary>
        public string? AllowValues { get; }

        /// <summary>The default value if the field is missing or (if <see cref="AllowBlank"/> is <c>false</c>) blank.</summary>
        public string? Default { get; }

        /// <summary>Whether to allow blank values.</summary>
        public bool AllowBlank { get; }

        /// <summary>Whether the player can specify multiple values for this field.</summary>
        public bool AllowMultiple { get; }

        /// <summary>An optional explanation of the config field for players.</summary>
        public string? Description { get; }

        /// <summary>An optional section key to group related fields.</summary>
        public string? Section { get; }


        /*********
        ** Accessors
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="allowValues">The comma-delimited values to allow.</param>
        /// <param name="default">The default value if the field is missing or (if <paramref name="allowBlank"/> is <c>false</c>) blank.</param>
        /// <param name="allowBlank">Whether to allow blank values.</param>
        /// <param name="allowMultiple">Whether the player can specify multiple values for this field.</param>
        /// <param name="description">An optional explanation of the config field for players.</param>
        /// <param name="section">An optional section key to group related fields.</param>
        public ConfigSchemaFieldConfig(string allowValues, string @default, bool allowBlank, bool allowMultiple, string description, string section)
        {
            this.AllowValues = allowValues;
            this.Default = @default;
            this.AllowBlank = allowBlank;
            this.AllowMultiple = allowMultiple;
            this.Description = description;
            this.Section = section;
        }
    }
}
