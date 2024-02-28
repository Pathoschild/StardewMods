using System.Diagnostics.CodeAnalysis;
using ContentPatcher.Framework.Patches;
using StardewModdingAPI;

namespace ContentPatcher.Framework.Migrations
{
    /// <summary>Overrides patches at runtime to migrate them to a given format version.</summary>
    internal interface IRuntimeMigration : IMigration
    {
        /// <summary>Get the actual asset name to edit, if different from the one resolved from the patch data.</summary>
        /// <param name="assetName">The resolved asset name being loaded or edited. If earlier migrations already redirected the target, this is the new asset name.</param>
        /// <param name="patch">The load or edit patch being applied.</param>
        /// <returns>Returns the new asset name to load or edit instead, or <c>null</c> to keep the current one as-is.</returns>
        IAssetName? RedirectTarget(IAssetName assetName, IPatch patch);

        /// <summary>Apply a load patch to the asset at runtime, overriding the normal apply log.</summary>
        /// <typeparam name="T">The asset type being loaded.</typeparam>
        /// <param name="patch">The load patch to apply, with any contextual values (e.g. token strings) already updated.</param>
        /// <param name="assetName">The resolved asset name being loaded.</param>
        /// <param name="asset">The asset data to load. This is normally <c>null</c> before this method is called, but if multiple migrations apply to the same patch, then this will be the result of the previous migrations.</param>
        /// <param name="error">An error message which indicates why migration failed.</param>
        /// <returns>Returns whether the load was overridden, so that the patch isn't applied normally after calling this method.</returns>
        bool TryApplyLoadPatch<T>(LoadPatch patch, IAssetName assetName, [NotNullWhen(true)] ref T? asset, out string? error)
            where T : notnull;

        /// <summary>Apply an edit patch to the asset at runtime, overriding the normal apply log.</summary>
        /// <typeparam name="T">The asset type being loaded.</typeparam>
        /// <param name="patch">The edit patch to apply, with any contextual values (e.g. token strings) already updated.</param>
        /// <param name="asset">The loaded asset data, with any previous patches in the list already applied. If multiple migrations apply to the same patch, then the earlier migrations are already applied to this parameter at this point.</param>
        /// <param name="error">An error message which indicates why migration failed.</param>
        /// <returns>Returns whether the edit was overridden, so that the patch isn't applied normally after calling this method.</returns>
        bool TryApplyEditPatch<T>(IPatch patch, IAssetData asset, out string? error)
            where T : notnull;
    }
}
