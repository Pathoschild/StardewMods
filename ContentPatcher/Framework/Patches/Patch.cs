using System;
using System.Collections.Generic;
using System.Linq;
using ContentPatcher.Framework.Conditions;
using ContentPatcher.Framework.Lexing.LexTokens;
using ContentPatcher.Framework.Tokens;
using StardewModdingAPI;

namespace ContentPatcher.Framework.Patches
{
    /// <summary>Metadata for a conditional patch.</summary>
    internal abstract class Patch : IPatch
    {
        /*********
        ** Fields
        *********/
        /// <summary>Normalise an asset name.</summary>
        private readonly Func<string, string> NormaliseAssetName;

        /// <summary>The underlying contextual values.</summary>
        protected readonly List<IContextual> ContextualValues = new List<IContextual>();


        /*********
        ** Accessors
        *********/
        /// <summary>Whether the instance may change depending on the context.</summary>
        public bool IsMutable { get; } = true;

        /// <summary>Whether the instance is valid for the current context.</summary>
        public bool IsReady { get; protected set; }

        /// <summary>The last context used to update this patch.</summary>
        protected IContext LastContext { get; private set; }

        /// <summary>A unique name for this patch shown in log messages.</summary>
        public string LogName { get; }

        /// <summary>The patch type.</summary>
        public PatchType Type { get; }

        /// <summary>The content pack which requested the patch.</summary>
        public ManagedContentPack ContentPack { get; }

        /// <summary>The raw asset key to intercept (if applicable), including tokens.</summary>
        public ITokenString FromLocalAsset { get; protected set; }

        /// <summary>The normalised asset name to intercept.</summary>
        public string TargetAsset { get; private set; }

        /// <summary>The raw asset name to intercept, including tokens.</summary>
        public ITokenString RawTargetAsset { get; }

        /// <summary>The conditions which determine whether this patch should be applied.</summary>
        public Condition[] Conditions { get; }

        /// <summary>Whether the patch is currently applied to the target asset.</summary>
        public bool IsApplied { get; set; }


        /*********
        ** Public methods
        *********/
        /// <summary>Update the patch data when the context changes.</summary>
        /// <param name="context">Provides access to contextual tokens.</param>
        /// <returns>Returns whether the patch data changed.</returns>
        public virtual bool UpdateContext(IContext context)
        {
            this.LastContext = context;
            bool changed = false;

            // update contextual values
            foreach (IContextual contextual in this.ContextualValues)
            {
                bool wasReady = contextual.IsReady;
                if (contextual.UpdateContext(context) || contextual.IsReady != wasReady)
                    changed = true;
            }

            // update source asset
            if (this.FromLocalAsset != null)
            {
                bool sourceChanged = this.FromLocalAsset.UpdateContext(context);
                this.IsReady = this.IsReady && this.FromLocalAsset.IsReady && this.ContentPack.HasFile(this.FromLocalAsset.Value);
                changed = changed || sourceChanged;
            }

            // update target asset
            this.TargetAsset = this.NormaliseAssetName(this.RawTargetAsset.Value);

            // update ready flag
            {
                bool wasReady = this.IsReady;
                this.IsReady =
                    (!this.Conditions.Any() || this.Conditions.All(p => p.IsMatch(context)))
                    && this.GetTokensUsed().All(name => context.Contains(name, enforceContext: true));
                changed = changed || this.IsReady != wasReady;
            }

            return changed;
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

        /// <summary>Get the token names used by this patch in its fields.</summary>
        public virtual IEnumerable<string> GetTokensUsed()
        {
            if (this.FromLocalAsset != null)
            {
                foreach (LexTokenToken lexToken in this.FromLocalAsset.GetTokenPlaceholders(recursive: true))
                    yield return lexToken.Name;
            }
            foreach (LexTokenToken lexToken in this.RawTargetAsset.GetTokenPlaceholders(recursive: true))
                yield return lexToken.Name;
        }


        /*********
        ** Protected methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="logName">A unique name for this patch shown in log messages.</param>
        /// <param name="type">The patch type.</param>
        /// <param name="contentPack">The content pack which requested the patch.</param>
        /// <param name="assetName">The normalised asset name to intercept.</param>
        /// <param name="conditions">The conditions which determine whether this patch should be applied.</param>
        /// <param name="normaliseAssetName">Normalise an asset name.</param>
        protected Patch(string logName, PatchType type, ManagedContentPack contentPack, ITokenString assetName, IEnumerable<Condition> conditions, Func<string, string> normaliseAssetName)
        {
            // set values
            this.LogName = logName;
            this.Type = type;
            this.ContentPack = contentPack;
            this.RawTargetAsset = assetName;
            this.Conditions = conditions.ToArray();
            this.NormaliseAssetName = normaliseAssetName;

            // track contextuals
            this.ContextualValues.AddRange(this.Conditions);
            this.ContextualValues.Add(this.RawTargetAsset);
        }
    }
}
