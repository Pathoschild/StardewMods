using System.Collections.Generic;
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
        TokenString FromLocalAsset { get; }

        /// <summary>The normalised asset name to intercept.</summary>
        string TargetAsset { get; }

        /// <summary>The raw asset name to intercept, including tokens.</summary>
        TokenString RawTargetAsset { get; }

        /// <summary>The conditions which determine whether this patch should be applied.</summary>
        ConditionDictionary Conditions { get; }

        /// <summary>Whether the patch is currently applied to the target asset.</summary>
        bool IsApplied { get; set; }


        /*********
        ** Public methods
        *********/
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

        /// <summary>Get the tokens used by this patch in its fields.</summary>
        IEnumerable<TokenName> GetTokensUsed();
    }
}
