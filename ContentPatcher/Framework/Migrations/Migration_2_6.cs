using System.Diagnostics.CodeAnalysis;
using System.Linq;
using ContentPatcher.Framework.ConfigModels;
using StardewModdingAPI;

namespace ContentPatcher.Framework.Migrations;

/// <summary>Migrates patches to format version 2.6.</summary>
[SuppressMessage("ReSharper", "InconsistentNaming", Justification = "Named for clarity.")]
internal class Migration_2_6 : BaseMigration
{
    /*********
    ** Public methods
    *********/
    /// <summary>Construct an instance.</summary>
    public Migration_2_6()
        : base(new SemanticVersion(2, 6, 0)) { }

    /// <inheritdoc />
    public override bool TryMigrate(ref PatchConfig[] patches, [NotNullWhen(false)] out string? error)
    {
        if (!base.TryMigrate(ref patches, out error))
            return false;

        foreach (PatchConfig patch in patches)
        {
            // 2.6 adds AddNpcWarps
            if (patch.AddNpcWarps.Any())
            {
                error = this.GetNounPhraseError($"using the {nameof(patch.AddNpcWarps)} field");
                return false;
            }
        }

        return true;
    }
}
