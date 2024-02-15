using System;
using System.Diagnostics.CodeAnalysis;
using ContentPatcher.Framework.Conditions;
using ContentPatcher.Framework.Patches;
using StardewModdingAPI;

namespace ContentPatcher.Framework.Migrations.Internal
{
    /// <summary>A migrator which applies older <see cref="PatchType.EditData"/> patches to a newer asset or format.</summary>
    internal interface IEditAssetMigrator
    {
        /// <summary>Get whether this migration applies to a patch.</summary>
        /// <param name="assetName">The asset name to check. If the asset was redirected, this is the asset name before redirection.</param>
        bool AppliesTo(IAssetName assetName);

        /// <inheritdoc cref="IRuntimeMigration.RedirectTarget" />
        IAssetName? RedirectTarget(IAssetName assetName, IPatch patch);

        /// <inheritdoc cref="IRuntimeMigration.TryApplyLoadPatch{T}" />
        bool TryApplyLoadPatch<T>(LoadPatch patch, IAssetName assetName, [NotNullWhen(true)] ref T? asset, out string? error);

        /// <inheritdoc cref="IRuntimeMigration.TryApplyEditPatch{T}" />
        /// <param name="patch" />
        /// <param name="asset" />
        /// <param name="onWarning">A callback to invoke before logging a warning message. The warning message is still logged after calling it.</param>
        /// <param name="error" />
        bool TryApplyEditPatch<T>(EditDataPatch patch, IAssetData asset, Action<string, IMonitor> onWarning, out string? error);
    }
}
