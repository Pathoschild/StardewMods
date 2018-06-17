using System.Collections.Generic;
using ContentPatcher.Framework.Conditions;
using StardewModdingAPI;

namespace ContentPatcher.Framework.Patches
{
    /// <summary>A patch which can be applied to an asset.</summary>
    internal interface IPatch
    {
        /*********
        ** Properties
        *********/
        /// <summary>A unique name for this patch shown in log messages.</summary>
        string LogName { get; }

        /// <summary>The patch type.</summary>
        PatchType Type { get; }

        /// <summary>The content pack which requested the patch.</summary>
        ManagedContentPack ContentPack { get; }

        /// <summary>The normalised asset name to intercept.</summary>
        string AssetName { get; }

        /// <summary>The raw asset name to intercept, including tokens.</summary>
        TokenString TokenableAssetName { get; }

        /// <summary>The conditions which determine whether this patch should be applied.</summary>
        ConditionDictionary Conditions { get; }

        /// <summary>Whether this patch should be applied in the latest context.</summary>
        bool MatchesContext { get; }

        /// <summary>Whether the patch is currently applied to the target asset.</summary>
        bool IsApplied { get; set; }


        /*********
        ** Public methods
        *********/
        /// <summary>Update the patch data when the context changes.</summary>
        /// <param name="context">The condition context.</param>
        /// <param name="tokenisableConditions">The conditions which can be used in tokens.</param>
        /// <returns>Returns whether the patch data changed.</returns>
        bool UpdateContext(ConditionContext context, IDictionary<ConditionKey, string> tokenisableConditions);

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

        /// <summary>Get the condition tokens used by this patch in its fields.</summary>
        IEnumerable<ConditionKey> GetTokensUsed();
    }
}
