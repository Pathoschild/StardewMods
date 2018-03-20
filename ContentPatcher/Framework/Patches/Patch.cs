using System;
using System.Collections.Generic;
using System.Linq;
using ContentPatcher.Framework.Conditions;
using StardewModdingAPI;

namespace ContentPatcher.Framework.Patches
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
        /// <summary>A unique name for this patch shown in log messages.</summary>
        public string LogName { get; }

        /// <summary>The patch type.</summary>
        public PatchType Type { get; }

        /// <summary>The content pack which requested the patch.</summary>
        public IContentPack ContentPack { get; }

        /// <summary>The normalised asset name to intercept.</summary>
        public string AssetName { get; }

        /// <summary>The conditions which determine whether this patch should be applied.</summary>
        public ConditionDictionary Conditions { get; }

        /// <summary>Whether this patch should be applied in the latest context.</summary>
        public bool MatchesContext { get; private set; }


        /*********
        ** Public methods
        *********/
        /// <summary>Update the patch data when the context changes.</summary>
        /// <param name="context">The condition context.</param>
        /// <returns>Returns whether the patch data changed.</returns>
        public virtual bool UpdateContext(ConditionContext context)
        {
            bool wasMatch = this.MatchesContext;
            this.MatchesContext = this.Conditions.Count == 0 || this.Conditions.Values.All(p => p.IsMatch(context));
            return wasMatch != this.MatchesContext;
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

        /// <summary>Get the condition tokens used by this patch in its fields.</summary>
        public virtual IEnumerable<ConditionKey> GetTokensUsed()
        {
            yield break;
        }


        /*********
        ** Protected methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="logName">A unique name for this patch shown in log messages.</param>
        /// <param name="type">The patch type.</param>
        /// <param name="assetLoader">Handles loading assets from content packs.</param>
        /// <param name="contentPack">The content pack which requested the patch.</param>
        /// <param name="assetName">The normalised asset name to intercept.</param>
        /// <param name="conditions">The conditions which determine whether this patch should be applied.</param>
        protected Patch(string logName, PatchType type, AssetLoader assetLoader, IContentPack contentPack, string assetName, ConditionDictionary conditions)
        {
            this.LogName = logName;
            this.Type = type;
            this.AssetLoader = assetLoader;
            this.ContentPack = contentPack;
            this.AssetName = assetName;
            this.Conditions = conditions;
        }
    }
}
