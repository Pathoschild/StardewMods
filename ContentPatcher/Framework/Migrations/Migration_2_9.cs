using System.Diagnostics.CodeAnalysis;
using ContentPatcher.Framework.ConfigModels;
using StardewModdingAPI;
using StardewValley;

namespace ContentPatcher.Framework.Migrations;

/// <summary>Migrates patches to format version 2.9.</summary>
[SuppressMessage("ReSharper", "InconsistentNaming", Justification = "Named for clarity.")]
internal class Migration_2_9 : BaseMigration
{
    /*********
    ** Public methods
    *********/
    /// <summary>Construct an instance.</summary>
    public Migration_2_9()
        : base(new SemanticVersion(2, 9, 0)) { }

    /// <inheritdoc />
    public override bool TryMigrate(ref PatchConfig[] patches, [NotNullWhen(false)] out string? error)
    {
        if (!base.TryMigrate(ref patches, out error))
            return false;

        // 2.9 adds `PatchMode.Mask`
        foreach (PatchConfig patch in patches)
        {
            if (patch.PatchMode != null && Utility.TryParseEnum(patch.PatchMode, out PatchMode patchMode) && patchMode == PatchMode.Mask)
            {
                error = this.GetNounPhraseError($"using {nameof(patch.PatchMode)} {nameof(PatchMode.Mask)}");
                return false;
            }
        }

        return true;
    }
}
