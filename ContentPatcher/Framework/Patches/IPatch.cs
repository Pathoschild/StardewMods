using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using ContentPatcher.Framework.Conditions;
using StardewModdingAPI;
using StardewValley;

namespace ContentPatcher.Framework.Patches
{
    /// <summary>A patch which can be applied to an asset.</summary>
    internal interface IPatch : IContextual
    {
        /*********
        ** Accessors
        *********/
        /// <summary>The path of indexes from the root <c>content.json</c> to this patch, used to sort patches by global load order.</summary>
        /// <remarks>For example, the first patch in <c>content.json</c> is <c>[0]</c>. If that patch is an <see cref="PatchType.Include"/> patch, the third patch it loads would be <c>[0, 2]</c> (i.e. patch index 2 within patch index 0).</remarks>
        int[] IndexPath { get; }

        /// <summary>The path to the patch from the root content file.</summary>
        LogPathBuilder Path { get; }

        /// <summary>The patch type.</summary>
        PatchType Type { get; }

        /// <summary>The parent patch for which this patch was loaded, if any.</summary>
        IPatch? ParentPatch { get; }

        /// <summary>The content pack which requested the patch.</summary>
        IContentPack ContentPack { get; }

        /// <summary>The normalized asset key from which to load the local asset (if applicable).</summary>
        string? FromAsset { get; }

        /// <summary>The raw asset key from which to load the local asset (if applicable), including tokens.</summary>
        ITokenString? RawFromAsset { get; }

        /// <summary>The normalized asset name to intercept.</summary>
        IAssetName? TargetAsset { get; }

        /// <summary>The raw asset name to intercept, including tokens.</summary>
        ITokenString? RawTargetAsset { get; }

        /// <summary>When the patch should be updated.</summary>
        UpdateRate UpdateRate { get; }

        /// <summary>The conditions which determine whether this patch should be applied.</summary>
        Condition[] Conditions { get; }

        /// <summary>Whether the patch is currently applied to the target asset.</summary>
        bool IsApplied { get; set; }

        /// <summary>The <see cref="Game1.ticks"/> value when this patch last changed due to a context update.</summary>
        int LastChangedTick { get; }


        /*********
        ** Public methods
        *********/
        /// <summary>Get whether the <see cref="FromAsset"/> file exists.</summary>
        bool FromAssetExists();

        /// <summary>Load the initial version of the asset.</summary>
        /// <typeparam name="T">The asset type.</typeparam>
        /// <param name="assetName">The asset name to load.</param>
        /// <exception cref="System.NotSupportedException">The current patch type doesn't support loading assets.</exception>
        T Load<T>(IAssetName assetName)
            where T : notnull;

        /// <summary>Apply the patch to a loaded asset.</summary>
        /// <typeparam name="T">The asset type.</typeparam>
        /// <param name="asset">The asset to edit.</param>
        /// <exception cref="System.NotSupportedException">The current patch type doesn't support editing assets.</exception>
        void Edit<T>(IAssetData asset)
            where T : notnull;

        /// <summary>Get a human-readable list of changes applied to the asset for display when troubleshooting.</summary>
        IEnumerable<string> GetChangeLabels();
    }
}
