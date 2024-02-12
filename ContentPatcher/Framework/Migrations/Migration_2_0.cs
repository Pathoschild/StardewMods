using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using ContentPatcher.Framework.Conditions;
using ContentPatcher.Framework.ConfigModels;
using ContentPatcher.Framework.Patches;
using Pathoschild.Stardew.Common.Utilities;
using StardewModdingAPI;
using StardewValley;
using StardewValley.ItemTypeDefinitions;

namespace ContentPatcher.Framework.Migrations
{
    /// <summary>Migrates patches to format version 2.0 and Stardew Valley 1.6.</summary>
    [SuppressMessage("ReSharper", "InconsistentNaming", Justification = "Named for clarity.")]
    internal partial class Migration_2_0 : BaseRuntimeMigration
    {
        /*********
        ** Fields
        *********/
        /// <summary>The backing cache for <see cref="ParseObjectId"/>.</summary>
        private readonly Dictionary<string, string?> ParseObjectIdCache = new();

        /// <summary>The migrators that convert pre-1.6 edit patches to a newer asset or format.</summary>
        /// <remarks>For each edit, the first migrator which applies or returns errors is used.</remarks>
        private readonly IEditAssetMigrator[] Migrators;


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        public Migration_2_0()
            : base(new SemanticVersion(2, 0, 0))
        {
            this.AddedTokens = new InvariantSet(
                nameof(ConditionType.ModId)
            );
            this.MigrationWarnings = [
                "Some content packs haven't been updated for Stardew Valley 1.6.0. Content Patcher will try to auto-migrate them, but compatibility isn't guaranteed."
            ];

            this.Migrators = new IEditAssetMigrator[]
            {
                new BigCraftableInformationMigrator(),
                new CropsMigrator(),
                new LocationsMigrator(this.ParseObjectId),
                new NpcDispositionsMigrator(),
                new ObjectInformationMigrator(this.ParseObjectId)
            };
        }

        /// <inheritdoc />
        public override bool TryMigrate(ref PatchConfig[] patches, [NotNullWhen(false)] out string? error)
        {
            if (!base.TryMigrate(ref patches, out error))
                return false;

            // 2.0 adds Priority
            foreach (PatchConfig patch in patches)
            {
                if (patch.Priority != null)
                {
                    error = this.GetNounPhraseError($"using {nameof(patch.Priority)}");
                    return false;
                }
            }

            return true;
        }

        /// <inheritdoc />
        public override IAssetName? RedirectTarget(IAssetName assetName, IPatch patch)
        {
            foreach (IEditAssetMigrator migrator in this.Migrators)
            {
                if (migrator.AppliesTo(assetName))
                {
                    IAssetName? newName = migrator.RedirectTarget(assetName, patch);
                    if (newName != null)
                        return newName;
                }
            }

            return base.RedirectTarget(assetName, patch);
        }

        /// <inheritdoc />
        public override bool TryApplyLoadPatch<T>(LoadPatch patch, IAssetName assetName, [NotNullWhen(true)] ref T? asset, out string? error)
            where T : default
        {
            foreach (IEditAssetMigrator migrator in this.Migrators)
            {
                if (migrator.AppliesTo(patch.TargetAssetBeforeRedirection ?? assetName))
                {
                    if (migrator.TryApplyLoadPatch(patch, assetName, ref asset, out error))
                        return true;

                    if (error != null)
                        return false;
                }
            }

            return base.TryApplyLoadPatch<T>(patch, assetName, ref asset, out error);
        }

        /// <inheritdoc />
        public override bool TryApplyEditPatch<T>(IPatch patch, IAssetData asset, out string? error)
        {
            if (patch is EditDataPatch editPatch)
            {
                foreach (IEditAssetMigrator migrator in this.Migrators)
                {
                    if (migrator.AppliesTo(patch.TargetAssetBeforeRedirection ?? asset.Name))
                    {
                        if (migrator.TryApplyEditPatch<T>(editPatch, asset, out error))
                            return true;

                        if (error != null)
                            return false;
                    }
                }
            }

            return base.TryApplyEditPatch<T>(patch, asset, out error);
        }


        /*********
        ** Private methods
        *********/
        /// <summary>Get the unqualified object ID, if it's a valid object ID.</summary>
        /// <param name="rawItemId">The raw item ID, which may be an item query or non-object ID.</param>
        /// <returns>Returns the unqualified object ID, or <c>null</c> if it's not a valid object ID.</returns>
        private string? ParseObjectId(string rawItemId)
        {
            // skip null
            if (rawItemId is null)
                return null;

            // skip cached
            {
                if (this.ParseObjectIdCache.TryGetValue(rawItemId, out string? cached))
                    return cached;
            }

            // skip non-object-ID value
            ItemMetadata metadata = ItemRegistry.GetMetadata(rawItemId);
            if (metadata?.Exists() is not true || metadata.TypeIdentifier != ItemRegistry.type_object)
            {
                this.ParseObjectIdCache[rawItemId] = null;
                return null;
            }

            // apply
            this.ParseObjectIdCache[rawItemId] = metadata.LocalItemId;
            return metadata.LocalItemId;
        }

        /// <summary>The migration logic to apply pre-1.6 edit patches to a new asset or format.</summary>
        private interface IEditAssetMigrator
        {
            /// <summary>Get whether this migration applies to a patch.</summary>
            /// <param name="assetName">The asset name to check. If the asset was redirected, this is the asset name before redirection.</param>
            bool AppliesTo(IAssetName assetName);

            /// <inheritdoc cref="IRuntimeMigration.RedirectTarget" />
            IAssetName? RedirectTarget(IAssetName assetName, IPatch patch);

            /// <inheritdoc cref="IRuntimeMigration.TryApplyLoadPatch{T}" />
            bool TryApplyLoadPatch<T>(LoadPatch patch, IAssetName assetName, [NotNullWhen(true)] ref T? asset, out string? error);

            /// <inheritdoc cref="IRuntimeMigration.TryApplyEditPatch{T}" />
            bool TryApplyEditPatch<T>(EditDataPatch patch, IAssetData asset, out string? error);
        }
    }
}
