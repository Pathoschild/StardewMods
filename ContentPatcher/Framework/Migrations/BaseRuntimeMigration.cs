using System;
using System.Diagnostics.CodeAnalysis;
using ContentPatcher.Framework.Conditions;
using ContentPatcher.Framework.Migrations.Internal;
using ContentPatcher.Framework.Patches;
using StardewModdingAPI;

namespace ContentPatcher.Framework.Migrations
{
    /// <summary>The base implementation for a format version migrator which overrides patches at runtime.</summary>
    internal abstract class BaseRuntimeMigration : BaseMigration, IRuntimeMigration
    {
        /*********
        ** Fields
        *********/
        /// <summary>The migrators that convert older <see cref="PatchType.EditData"/> patches to a newer asset or format.</summary>
        /// <remarks>For each edit, the first migrator which applies or returns errors is used.</remarks>
        protected IEditAssetMigrator[] RuntimeEditDataMigrators = Array.Empty<IEditAssetMigrator>();


        /*********
        ** Public methods
        *********/
        /// <inheritdoc />
        public virtual IAssetName? RedirectTarget(IAssetName assetName, IPatch patch)
        {
            foreach (IEditAssetMigrator migrator in this.RuntimeEditDataMigrators)
            {
                if (migrator.AppliesTo(assetName))
                {
                    IAssetName? newName = migrator.RedirectTarget(assetName, patch);
                    if (newName != null)
                        return newName;
                }
            }

            return null;
        }

        /// <inheritdoc />
        public virtual bool TryApplyLoadPatch<T>(LoadPatch patch, IAssetName assetName, [NotNullWhen(true)] ref T? asset, out string? error)
            where T : notnull
        {
            foreach (IEditAssetMigrator migrator in this.RuntimeEditDataMigrators)
            {
                if (migrator.AppliesTo(patch.TargetAssetBeforeRedirection ?? assetName))
                {
                    if (migrator.TryApplyLoadPatch(patch, assetName, ref asset, out error))
                        return true;

                    if (error != null)
                        return false;
                }
            }

            error = null;
            return false;
        }

        /// <inheritdoc />
        public virtual bool TryApplyEditPatch<T>(IPatch patch, IAssetData asset, out string? error)
            where T : notnull
        {
            if (patch is EditDataPatch editPatch)
            {
                foreach (IEditAssetMigrator migrator in this.RuntimeEditDataMigrators)
                {
                    if (migrator.AppliesTo(patch.TargetAssetBeforeRedirection ?? asset.Name))
                    {
                        // log warning if runtime migration has issues
                        bool hasLoggedWarning = false;
                        void OnWarning(string warning, IMonitor monitor)
                        {
                            if (!hasLoggedWarning)
                                monitor.Log($"Data patch \"{patch.Path}\" reported warnings when applying runtime migration {this.Version}. (For the mod author: see https://smapi.io/cp-migrate to avoid runtime migrations.)", LogLevel.Warn);
                            hasLoggedWarning = true;
                        }

                        // apply
                        if (migrator.TryApplyEditPatch<T>(editPatch, asset, OnWarning, out error))
                            return true;
                        if (error != null)
                            return false;
                    }
                }
            }

            error = null;
            return false;
        }


        /*********
        ** Protected methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="version">The version to which this migration applies.</param>
        protected BaseRuntimeMigration(ISemanticVersion version)
            : base(version) { }
    }
}
