using System;
using System.Collections.Generic;
using System.Linq;
using ContentPatcher.Framework.Conditions;
using ContentPatcher.Framework.ConfigModels;
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
        protected readonly AggregateContextual Contextuals = new AggregateContextual();

        /// <summary>Diagnostic info about the instance.</summary>
        protected readonly ContextualState State = new ContextualState();

        /// <summary>The context which provides tokens for this patch, including patch-specific tokens like <see cref="ConditionType.Target"/>.</summary>
        protected SinglePatchContext PrivateContext { get; }

        /// <summary>Whether the <see cref="FromLocalAsset"/> file exists.</summary>
        private bool FromLocalAssetExistsImpl;


        /*********
        ** Accessors
        *********/
        /// <summary>A unique name for this patch shown in log messages.</summary>
        public string LogName { get; }

        /// <summary>The patch type.</summary>
        public PatchType Type { get; }

        /// <summary>The content pack which requested the patch.</summary>
        public ManagedContentPack ContentPack { get; }

        /// <summary>Whether the instance may change depending on the context.</summary>
        public bool IsMutable { get; } = true;

        /// <summary>Whether the instance is valid for the current context.</summary>
        public bool IsReady { get; private set; }

        /// <summary>The raw asset key to intercept (if applicable), including tokens.</summary>
        public ITokenString FromLocalAsset { get; }

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
            // reset
            bool wasReady = this.IsReady;
            this.State.Reset();
            bool changed;

            // update patch context
            changed = this.RawTargetAsset.UpdateContext(context);
            this.TargetAsset = this.RawTargetAsset.IsReady ? this.NormaliseAssetName(this.RawTargetAsset.Value) : "";
            this.PrivateContext.Update(context, this.RawTargetAsset);

            // update contextual values
            changed = this.Contextuals.UpdateContext(this.PrivateContext) || changed;
            this.FromLocalAssetExistsImpl = false;
            if (this.FromLocalAsset?.IsReady == true)
            {
                this.FromLocalAssetExistsImpl = this.ContentPack.HasFile(this.FromLocalAsset.Value);
                if (!this.FromLocalAssetExistsImpl)
                    this.State.AddErrors($"{nameof(PatchConfig.FromFile)} file '{this.FromLocalAsset.Value}' does not exist");
            }

            // update ready flag
            // note: from file asset existence deliberately isn't checked here, so we can show warnings at runtime instead.
            this.IsReady =
                this.Contextuals.IsReady
                && (!this.Conditions.Any() || this.Conditions.All(p => p.IsMatch(this.PrivateContext)));

            return changed || this.IsReady != wasReady;
        }

        /// <summary>Get whether the <see cref="FromLocalAsset"/> file exists.</summary>
        public bool FromLocalAssetExists()
        {
            return this.FromLocalAssetExistsImpl;
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
            return this.Contextuals.GetTokensUsed();
        }

        /// <summary>Get diagnostic info about the contextual instance.</summary>
        public IContextualState GetDiagnosticState()
        {
            return this.State.Clone()
                .MergeFrom(this.Contextuals.GetDiagnosticState());
        }

        /// <summary>Get the context which provides tokens for this patch, including patch-specific tokens like <see cref="ConditionType.Target"/>.</summary>
        public IContext GetPatchContext()
        {
            return this.PrivateContext;
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
        /// <param name="fromLocalAsset">The raw asset key to intercept (if applicable), including tokens.</param>
        protected Patch(string logName, PatchType type, ManagedContentPack contentPack, ITokenString assetName, IEnumerable<Condition> conditions, Func<string, string> normaliseAssetName, ITokenString fromLocalAsset = null)
        {
            this.LogName = logName;
            this.Type = type;
            this.ContentPack = contentPack;
            this.RawTargetAsset = assetName;
            this.Conditions = conditions.ToArray();
            this.NormaliseAssetName = normaliseAssetName;
            this.PrivateContext = new SinglePatchContext(scope: this.ContentPack.Manifest.UniqueID);
            this.FromLocalAsset = fromLocalAsset;

            this.Contextuals
                .Add(this.Conditions)
                .Add(this.RawTargetAsset)
                .Add(this.FromLocalAsset);
        }
    }
}
