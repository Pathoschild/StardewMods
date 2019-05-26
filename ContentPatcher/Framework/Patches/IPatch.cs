using ContentPatcher.Framework.Conditions;
using ContentPatcher.Framework.Tokens;
using StardewModdingAPI;

namespace ContentPatcher.Framework.Patches
{
    /// <summary>A patch which can be applied to an asset.</summary>
    internal interface IPatch : IContextual
    {
        /*********
        ** Accessors
        *********/
        /// <summary>A unique name for this patch shown in log messages.</summary>
        string LogName { get; }

        /// <summary>The patch type.</summary>
        PatchType Type { get; }

        /// <summary>The content pack which requested the patch.</summary>
        ManagedContentPack ContentPack { get; }

        /// <summary>The asset key to load from the content pack instead.</summary>
        ITokenString FromLocalAsset { get; }

        /// <summary>The normalised asset name to intercept.</summary>
        string TargetAsset { get; }

        /// <summary>The raw asset name to intercept, including tokens.</summary>
        ITokenString RawTargetAsset { get; }

        /// <summary>The conditions which determine whether this patch should be applied.</summary>
        Condition[] Conditions { get; }

        /// <summary>Whether the patch is currently applied to the target asset.</summary>
        bool IsApplied { get; set; }


        /*********
        ** Public methods
        *********/
        /// <summary>Get whether the <see cref="FromLocalAsset"/> file exists.</summary>
        bool FromLocalAssetExists();

        /// <summary>Load the initial version of the asset.</summary>
        /// <typeparam name="T">The asset type.</typeparam>
        /// <param name="asset">The asset to load.</param>
        /// <exception cref="System.NotSupportedException">The current patch type doesn't support loading assets.</exception>
        T Load<T>(IAssetInfo asset);

        /// <summary>Apply the patch to a loaded asset.</summary>
        /// <typeparam name="T">The asset type.</typeparam>
        /// <param name="asset">The asset to edit.</param>
        /// <exception cref="System.NotSupportedException">The current patch type doesn't support editing assets.</exception>
        void Edit<T>(IAssetData asset);

        /// <summary>Get the context which provides tokens for this patch, including patch-specific tokens like <see cref="ConditionType.Target"/>.</summary>
        IContext GetPatchContext();
    }
}
