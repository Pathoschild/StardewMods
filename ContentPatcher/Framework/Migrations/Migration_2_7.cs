using System;
using System.Diagnostics.CodeAnalysis;
using ContentPatcher.Framework.ConfigModels;
using Newtonsoft.Json.Linq;
using StardewModdingAPI;
using StardewValley.GameData.Characters;

namespace ContentPatcher.Framework.Migrations;

/// <summary>Migrates patches to format version 2.6.</summary>
[SuppressMessage("ReSharper", "InconsistentNaming", Justification = "Named for clarity.")]
internal class Migration_2_7 : BaseMigration
{
    /*********
    ** Public methods
    *********/
    /// <summary>Construct an instance.</summary>
    public Migration_2_7()
        : base(new SemanticVersion(2, 7, 0)) { }

    /// <inheritdoc />
    public override bool TryMigrate(ref PatchConfig[] patches, [NotNullWhen(false)] out string? error)
    {
        if (!base.TryMigrate(ref patches, out error))
            return false;

        // 2.7 fixes a bug where you could set an enum field to a boolean value. This would previously be converted
        // numerically (like true -> 1 -> "MainGroup"), so match that logic for affected fields.
        //
        // This is based on the known affected content packs; it's not intended to cover every possible scenario (e.g.
        // patches which target "Data\Characters" or use 'Fields' instead).
        foreach (PatchConfig patch in patches)
        {
            if (patch.Entries.Count > 0 && string.Equals(patch.Target, "Data/Characters", StringComparison.OrdinalIgnoreCase))
            {
                foreach (JToken? rawEntry in patch.Entries.Values)
                {
                    if (rawEntry is not JObject entry)
                        continue;

                    JProperty? property = entry.Property(nameof(CharacterData.EndSlideShow), StringComparison.OrdinalIgnoreCase);
                    if (property is null)
                        continue;

                    string? rawValue = property.Value.Value<string>();
                    if (string.Equals(rawValue, bool.TrueString, StringComparison.OrdinalIgnoreCase))
                        property.Value = nameof(EndSlideShowBehavior.MainGroup);
                    else if (string.Equals(rawValue, bool.FalseString, StringComparison.OrdinalIgnoreCase))
                        property.Value = nameof(EndSlideShowBehavior.Hidden);
                }
            }
        }

        return true;
    }
}
