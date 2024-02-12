using System.Diagnostics.CodeAnalysis;
using ContentPatcher.Framework.Patches;
using StardewModdingAPI;

namespace ContentPatcher.Framework.Migrations
{
    /// <summary>The base implementation for a format version migrator which overrides patches at runtime.</summary>
    internal abstract class BaseRuntimeMigration : BaseMigration, IRuntimeMigration
    {
        /*********
        ** Public methods
        *********/
        /// <inheritdoc />
        public virtual IAssetName? RedirectTarget(IAssetName assetName, IPatch patch)
        {
            return null;
        }

        /// <inheritdoc />
        public virtual bool TryApplyLoadPatch<T>(LoadPatch patch, IAssetName assetName, [NotNullWhen(true)] ref T? asset, out string? error)
            where T : notnull
        {
            error = null;
            return false;
        }

        /// <inheritdoc />
        public virtual bool TryApplyEditPatch<T>(IPatch patch, IAssetData asset, out string? error)
            where T : notnull
        {
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
