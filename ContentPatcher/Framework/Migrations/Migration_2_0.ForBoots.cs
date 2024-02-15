using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using ContentPatcher.Framework.Migrations.Internal;
using ContentPatcher.Framework.Patches;
using StardewModdingAPI;

namespace ContentPatcher.Framework.Migrations
{
    internal partial class Migration_2_0 : BaseRuntimeMigration
    {
        /// <summary>The migration logic to apply pre-1.6 <c>Data/Boots</c> patches to the new format.</summary>
        private class BootsMigrator : IEditAssetMigrator
        {
            /*********
            ** Fields
            *********/
            /// <summary>The asset name.</summary>
            private const string AssetName = "Data/Boots";


            /*********
            ** Public methods
            *********/
            /// <inheritdoc />
            public bool AppliesTo(IAssetName assetName)
            {
                return assetName?.IsEquivalentTo(BootsMigrator.AssetName, useBaseName: true) is true;
            }

            /// <inheritdoc />
            public IAssetName? RedirectTarget(IAssetName assetName, IPatch patch)
            {
                return null; // same asset name
            }

            /// <inheritdoc />
            public bool TryApplyLoadPatch<T>(LoadPatch patch, IAssetName assetName, [NotNullWhen(true)] ref T? asset, out string? error)
            {
                var data = patch.Load<Dictionary<string, string>>(assetName)!;
                this.MigrateData(data);
                asset = (T)(object)data;

                error = null;
                return true;
            }

            /// <inheritdoc />
            public bool TryApplyEditPatch<T>(EditDataPatch patch, IAssetData asset, Action<string, IMonitor> onWarning, out string? error)
            {
                var data = (Dictionary<string, string>)asset.Data;
                patch.Edit<Dictionary<string, string>>(asset, onWarning);
                this.MigrateData(data);

                error = null;
                return true;
            }


            /*********
            ** Private methods
            *********/
            /// <summary>Migrate pre-1.6 data to the new format.</summary>
            /// <param name="asset">The asset data to update.</param>
            private void MigrateData(IDictionary<string, string> asset)
            {
                foreach ((string key, string fromEntry) in asset)
                {
                    int fieldCount = RuntimeMigrationHelper.CountFields(fromEntry, '/');

                    if (fieldCount == 6)
                    {
                        string name = fromEntry[..fromEntry.IndexOf('/')];
                        asset[key] = fromEntry + '/' + name;
                    }
                }
            }
        }
    }
}
