using System;
using System.Linq;
using ContentPatcher.Framework.Patches;
using StardewModdingAPI;

namespace ContentPatcher.Framework.Conditions
{
    /// <summary>Metadata for a conditional patch.</summary>
    internal abstract class Patch : IPatch
    {
        /*********
        ** Properties
        *********/
        /// <summary>Handles loading assets from content packs.</summary>
        protected AssetLoader AssetLoader { get; }


        /*********
        ** Accessors
        *********/
        /// <summary>The patch type.</summary>
        public PatchType Type { get; }

        /// <summary>The content pack which requested the patch.</summary>
        public IContentPack ContentPack { get; }

        /// <summary>The normalised asset name to intercept.</summary>
        public string AssetName { get; }

        /// <summary>The conditions which determine whether this patch should be applied.</summary>
        public ConditionDictionary Conditions { get; }


        /*********
        ** Public methods
        *********/
        /// <summary>Get whether this patch should be applied.</summary>
        /// <param name="context">The condition context.</param>
        public bool IsMatch(ConditionContext context)
        {
            return
                this.Conditions.Count == 0
                || this.Conditions.Values.All(p => p.IsMatch(context));
        }

        /// <summary>Load the initial version of the asset.</summary>
        /// <typeparam name="T">The asset type.</typeparam>
        /// <param name="asset">The asset to load.</param>
        /// <exception cref="NotSupportedException">The current patch type doesn't support loading assets.</exception>
        public virtual T Load<T>(IAssetInfo asset)
        {
            throw new NotSupportedException("This patch type doesn't support loading assets.");
        }

        /// <summary>Apply the patch to a loaded asset.</summary>
        /// <typeparam name="T">The asset type.</typeparam>
        /// <param name="asset">The asset to edit.</param>
        /// <exception cref="NotSupportedException">The current patch type doesn't support editing assets.</exception>
        public virtual void Edit<T>(IAssetData asset)
        {
            throw new NotSupportedException("This patch type doesn't support loading assets.");
        }


        /*********
        ** Protected methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="type">The patch type.</param>
        /// <param name="assetLoader">Handles loading assets from content packs.</param>
        /// <param name="contentPack">The content pack which requested the patch.</param>
        /// <param name="assetName">The normalised asset name to intercept.</param>
        /// <param name="conditions">The conditions which determine whether this patch should be applied.</param>
        protected Patch(PatchType type, AssetLoader assetLoader, IContentPack contentPack, string assetName, ConditionDictionary conditions)
        {
            this.Type = type;
            this.AssetLoader = assetLoader;
            this.ContentPack = contentPack;
            this.AssetName = assetName;
            this.Conditions = conditions;
        }
    }
}
