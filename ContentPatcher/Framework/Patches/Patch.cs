using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ContentPatcher.Framework.Conditions;
using ContentPatcher.Framework.ConfigModels;
using ContentPatcher.Framework.Tokens;
using Microsoft.Xna.Framework;
using Pathoschild.Stardew.Common;
using StardewModdingAPI;

namespace ContentPatcher.Framework.Patches
{
    /// <summary>Metadata for a conditional patch.</summary>
    internal abstract class Patch : IPatch
    {
        /*********
        ** Fields
        *********/
        /// <summary>Normalize an asset name.</summary>
        private readonly Func<string, string> NormalizeAssetNameImpl;

        /// <summary>The underlying contextual values.</summary>
        protected readonly AggregateContextual Contextuals = new AggregateContextual();

        /// <summary>Diagnostic info about the instance.</summary>
        protected readonly ContextualState State = new ContextualState();

        /// <summary>The context which provides tokens specific to this patch like <see cref="ConditionType.Target"/>.</summary>
        protected LocalContext PrivateContext { get; }

        /// <summary>Whether the <see cref="FromAsset"/> file exists.</summary>
        private bool FromAssetExistsImpl;


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
        public bool IsReady { get; protected set; }

        /// <summary>The normalized asset key from which to load the local asset (if applicable).</summary>
        public string FromAsset { get; private set; }

        /// <summary>The raw asset key from which to load the local asset (if applicable), including tokens.</summary>
        public ITokenString RawFromAsset { get; }

        /// <summary>The normalized asset name to intercept.</summary>
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

            // update target asset
            changed = this.RawTargetAsset.UpdateContext(context);
            this.TargetAsset = this.RawTargetAsset.IsReady ? this.NormalizeAssetNameImpl(this.RawTargetAsset.Value) : "";

            // update local tokens
            this.PrivateContext.Update(context);
            if (this.RawTargetAsset.IsReady)
            {
                string path = PathUtilities.NormalizePathSeparators(this.RawTargetAsset.Value);

                this.PrivateContext.SetLocalValue(ConditionType.Target.ToString(), path);
                this.PrivateContext.SetLocalValue(ConditionType.TargetWithoutPath.ToString(), Path.GetFileName(path));
            }

            // update contextuals
            changed = this.Contextuals.UpdateContext(this.PrivateContext) || changed;
            this.FromAssetExistsImpl = false;

            // update from asset
            this.FromAsset = this.RawFromAsset?.IsReady == true
                ? this.NormalizeLocalAssetPath(this.RawFromAsset.Value, logName: $"{nameof(PatchConfig.FromFile)} field")
                : null;
            if (this.Contextuals.IsReady && this.FromAsset != null)
            {
                this.FromAssetExistsImpl = this.ContentPack.HasFile(this.FromAsset);
                if (!this.FromAssetExistsImpl && this.Conditions.All(p => p.IsMatch(context)))
                    this.State.AddErrors($"{nameof(PatchConfig.FromFile)} '{this.FromAsset}' does not exist");
            }

            // update ready flag
            // note: from file asset existence deliberately isn't checked here, so we can show warnings at runtime instead.
            this.IsReady =
                this.Contextuals.IsReady
                && (!this.Conditions.Any() || this.Conditions.All(p => p.IsMatch(this.PrivateContext)));

            return changed || this.IsReady != wasReady;
        }

        /// <summary>Get whether the <see cref="FromAsset"/> file exists.</summary>
        public bool FromAssetExists()
        {
            return this.FromAssetExistsImpl;
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

        /// <summary>Get a human-readable list of changes applied to the asset for display when troubleshooting.</summary>
        public abstract IEnumerable<string> GetChangeLabels();


        /*********
        ** Protected methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="logName">A unique name for this patch shown in log messages.</param>
        /// <param name="type">The patch type.</param>
        /// <param name="contentPack">The content pack which requested the patch.</param>
        /// <param name="assetName">The normalized asset name to intercept.</param>
        /// <param name="conditions">The conditions which determine whether this patch should be applied.</param>
        /// <param name="normalizeAssetName">Normalize an asset name.</param>
        /// <param name="fromAsset">The normalized asset key from which to load the local asset (if applicable), including tokens.</param>
        protected Patch(string logName, PatchType type, ManagedContentPack contentPack, ITokenString assetName, IEnumerable<Condition> conditions, Func<string, string> normalizeAssetName, ITokenString fromAsset = null)
        {
            this.LogName = logName;
            this.Type = type;
            this.ContentPack = contentPack;
            this.RawTargetAsset = assetName;
            this.Conditions = conditions.ToArray();
            this.NormalizeAssetNameImpl = normalizeAssetName;
            this.PrivateContext = new LocalContext(scope: this.ContentPack.Manifest.UniqueID);
            this.RawFromAsset = fromAsset;

            this.Contextuals
                .Add(this.Conditions)
                .Add(this.RawTargetAsset)
                .Add(this.RawFromAsset);
        }

        /// <summary>Get a normalized file path relative to the content pack folder.</summary>
        /// <param name="path">The relative asset path.</param>
        /// <param name="logName">A descriptive name for the field being normalized shown in error messages.</param>
        protected string NormalizeLocalAssetPath(string path, string logName)
        {
            try
            {
                // normalize asset name
                if (string.IsNullOrWhiteSpace(path))
                    return null;
                string newPath = this.NormalizeAssetNameImpl(path);

                // add .xnb extension if needed (it's stripped from asset names)
                string fullPath = this.ContentPack.GetFullPath(newPath);
                if (!File.Exists(fullPath))
                {
                    if (File.Exists($"{fullPath}.xnb") || Path.GetExtension(path) == ".xnb")
                        newPath += ".xnb";
                }

                return newPath;
            }
            catch (Exception ex)
            {
                throw new FormatException($"The {logName} for patch '{this.LogName}' isn't a valid asset path (current value: '{path}').", ex);
            }
        }

        /// <summary>Try to read a tokenized rectangle.</summary>
        /// <param name="tokenArea">The tokenized rectangle to parse.</param>
        /// <param name="defaultX">The X value if the input area is null.</param>
        /// <param name="defaultY">The Y value if the input area is null.</param>
        /// <param name="defaultWidth">The width if the input area is null.</param>
        /// <param name="defaultHeight">The height if the input area is null.</param>
        /// <param name="area">The parsed rectangle.</param>
        /// <param name="error">The error phrase indicating why parsing failed, if applicable.</param>
        /// <returns>Returns whether the rectangle was successfully parsed.</returns>
        protected bool TryReadArea(TokenRectangle tokenArea, int defaultX, int defaultY, int defaultWidth, int defaultHeight, out Rectangle area, out string error)
        {
            if (tokenArea != null)
                return tokenArea.TryGetRectangle(out area, out error);

            area = new Rectangle(defaultX, defaultY, defaultWidth, defaultHeight);
            error = null;
            return true;
        }
    }
}
