using System.Diagnostics.CodeAnalysis;
using ContentPatcher.Framework.ConfigModels;
using StardewModdingAPI;

namespace ContentPatcher.Framework.Migrations
{
    /// <summary>Migrates patches to format version 1.26.</summary>
    [SuppressMessage("ReSharper", "InconsistentNaming", Justification = "Named for clarity.")]
    internal class Migration_1_26 : BaseMigration
    {
        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        public Migration_1_26()
            : base(new SemanticVersion(1, 26, 0)) { }

        /// <inheritdoc />
        public override bool TryMigrate(ContentConfig content, [NotNullWhen(false)] out string? error)
        {
            if (!base.TryMigrate(content, out error))
                return false;

            // 1.26 adds config sections
            foreach (ConfigSchemaFieldConfig? config in content.ConfigSchema.Values)
            {
                if (!string.IsNullOrWhiteSpace(config?.Section))
                {
                    error = this.GetNounPhraseError($"using {nameof(config.Section)} with a {nameof(content.ConfigSchema)} entry");
                    return false;
                }
            }

            return true;
        }
    }
}
