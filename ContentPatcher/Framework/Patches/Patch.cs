using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ContentPatcher.Framework.Conditions;
using ContentPatcher.Framework.ConfigModels;
using ContentPatcher.Framework.Tokens;
using Microsoft.Xna.Framework;
using Pathoschild.Stardew.Common.Utilities;
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

        /// <summary>The tokens that are updated manually, rather than via <see cref="Contextuals"/>.</summary>
        private readonly HashSet<IContextual> ManuallyUpdatedTokens = new HashSet<IContextual>(new ObjectReferenceComparer<IContextual>());

        /// <summary>Diagnostic info about the instance.</summary>
        protected readonly ContextualState State = new ContextualState();

        /// <summary>The context which provides tokens specific to this patch like <see cref="ConditionType.Target"/>.</summary>
        private readonly LocalContext PrivateContext;

        /// <summary>Whether the <see cref="FromAsset"/> file exists.</summary>
        private bool FromAssetExistsImpl;

        /// <summary>The <see cref="RawFromAsset"/> with support for managing its state.</summary>
        private IManagedTokenString ManagedRawFromAsset { get; }

        /// <summary>The <see cref="RawTargetAsset"/> with support for managing its state.</summary>
        protected IManagedTokenString ManagedRawTargetAsset { get; }


        /*********
        ** Accessors
        *********/
        /// <inheritdoc />
        public int[] IndexPath { get; }

        /// <inheritdoc />
        public LogPathBuilder Path { get; }

        /// <inheritdoc />
        public PatchType Type { get; }

        /// <inheritdoc />
        public IContentPack ContentPack { get; }

        /// <inheritdoc />
        public IPatch ParentPatch { get; }

        /// <inheritdoc />
        public bool IsMutable { get; } = true;

        /// <inheritdoc />
        public bool IsReady { get; protected set; }

        /// <inheritdoc />
        public string FromAsset { get; private set; }

        /// <inheritdoc />
        public ITokenString RawFromAsset => this.ManagedRawFromAsset;

        /// <inheritdoc />
        public string TargetAsset { get; private set; }

        /// <inheritdoc />
        public ITokenString RawTargetAsset => this.ManagedRawTargetAsset;

        /// <inheritdoc />
        public UpdateRate UpdateRate { get; set; }

        /// <inheritdoc />
        public Condition[] Conditions { get; }

        /// <inheritdoc />
        public bool IsApplied { get; set; }


        /*********
        ** Public methods
        *********/
        /// <inheritdoc />
        public virtual bool UpdateContext(IContext context)
        {
            // reset
            bool wasReady = this.IsReady;
            this.State.Reset();
            bool isReady = true;

            // update local tokens
            // (FromFile and Target may reference each other, so they need to be updated in a
            // specific order. A circular reference isn't possible since that's checked when the
            // patch is loaded.)
            this.PrivateContext.Update(context);
            bool changed = false;
            if (this.ManagedRawTargetAsset?.UsesTokens(ConditionType.FromFile) == true)
                changed |= this.UpdateFromFile(this.PrivateContext) | this.UpdateTargetPath(this.PrivateContext);
            else
                changed |= this.UpdateTargetPath(this.PrivateContext) | this.UpdateFromFile(this.PrivateContext);
            isReady &= this.RawTargetAsset?.IsReady != false && this.RawFromAsset?.IsReady != false;

            // update contextuals
            changed |= this.Contextuals.UpdateContext(
                this.PrivateContext,
                update: p => !this.ManuallyUpdatedTokens.Contains(p),

                // This avoids propagating irrelevant changes. For example, consider this condition:
                //    "{{Time}}": "0800"
                // 
                // Since the condition key will be different on each time change, the condition would be marked as
                // changed which would trigger a patch update. But the patch should only update if the *result*
                // changes, which we check below via isReady.
                countChange: p => p is not Condition
            );
            isReady &= this.Contextuals.IsReady && (!this.Conditions.Any() || this.Conditions.All(p => p.IsMatch));
            this.FromAssetExistsImpl = false;

            // check from asset existence
            if (isReady && this.FromAsset != null)
            {
                this.FromAssetExistsImpl = this.ContentPack.HasFile(this.FromAsset);
                if (!this.FromAssetExistsImpl && this.Conditions.All(p => p.IsMatch))
                    this.State.AddErrors($"{nameof(PatchConfig.FromFile)} '{this.FromAsset}' does not exist");
            }

            // update
            this.IsReady = isReady;
            return changed || this.IsReady != wasReady;
        }

        /// <inheritdoc />
        public bool FromAssetExists()
        {
            return this.FromAssetExistsImpl;
        }

        /// <inheritdoc />
        public virtual T Load<T>(IAssetInfo asset)
        {
            throw new NotSupportedException("This patch type doesn't support loading assets.");
        }

        /// <inheritdoc />
        public virtual void Edit<T>(IAssetData asset)
        {
            throw new NotSupportedException("This patch type doesn't support loading assets.");
        }

        /// <inheritdoc />
        public virtual IEnumerable<string> GetTokensUsed()
        {
            return this.Contextuals.GetTokensUsed();
        }

        /// <inheritdoc />
        public IContextualState GetDiagnosticState()
        {
            return this.State.Clone()
                .MergeFrom(this.Contextuals.GetDiagnosticState());
        }

        /// <inheritdoc />
        public abstract IEnumerable<string> GetChangeLabels();


        /*********
        ** Protected methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="indexPath">The path of indexes from the root <c>content.json</c> to this patch; see <see cref="IPatch.IndexPath"/>.</param>
        /// <param name="path">The path to the patch from the root content file.</param>
        /// <param name="type">The patch type.</param>
        /// <param name="assetName">The normalized asset name to intercept.</param>
        /// <param name="conditions">The conditions which determine whether this patch should be applied.</param>
        /// <param name="updateRate">When the patch should be updated.</param>
        /// <param name="normalizeAssetName">Normalize an asset name.</param>
        /// <param name="contentPack">The content pack which requested the patch.</param>
        /// <param name="parentPatch">The parent <see cref="PatchType.Include"/> patch for which this patch was loaded, if any.</param>
        /// <param name="fromAsset">The normalized asset key from which to load the local asset (if applicable), including tokens.</param>
        protected Patch(int[] indexPath, LogPathBuilder path, PatchType type, IManagedTokenString assetName, IEnumerable<Condition> conditions, UpdateRate updateRate, IContentPack contentPack, IPatch parentPatch, Func<string, string> normalizeAssetName, IManagedTokenString fromAsset = null)
        {
            this.IndexPath = indexPath;
            this.Path = path;
            this.Type = type;
            this.ManagedRawTargetAsset = assetName;
            this.Conditions = conditions.ToArray();
            this.UpdateRate = updateRate;
            this.NormalizeAssetNameImpl = normalizeAssetName;
            this.PrivateContext = new LocalContext(scope: contentPack.Manifest.UniqueID);
            this.ManagedRawFromAsset = fromAsset;
            this.ContentPack = contentPack;
            this.ParentPatch = parentPatch;

            this.Contextuals
                .Add(this.Conditions)
                .Add(assetName)
                .Add(fromAsset);
            this.ManuallyUpdatedTokens.Add(assetName);
            this.ManuallyUpdatedTokens.Add(fromAsset);
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

        /// <summary>A utility method for returning false with an out error.</summary>
        /// <param name="inError">The error message.</param>
        /// <param name="outError">The input error.</param>
        /// <returns>Return false.</returns>
        protected bool Fail(string inError, out string outError)
        {
            outError = inError;
            return false;
        }

        /// <summary>Update the target path, and add the relevant tokens to the patch context.</summary>
        /// <param name="context">The local patch context (already updated from the parent context).</param>
        /// <returns>Returns whether the field changed.</returns>
        private bool UpdateTargetPath(LocalContext context)
        {
            if (this.RawTargetAsset == null)
                return false;

            bool changed = this.ManagedRawTargetAsset.UpdateContext(context);

            if (this.RawTargetAsset.IsReady)
            {
                this.TargetAsset = this.NormalizeAssetNameImpl(this.RawTargetAsset.Value);
                context.SetLocalValue(ConditionType.Target.ToString(), this.TargetAsset);
                context.SetLocalValue(ConditionType.TargetPathOnly.ToString(), System.IO.Path.GetDirectoryName(this.TargetAsset));
                context.SetLocalValue(ConditionType.TargetWithoutPath.ToString(), System.IO.Path.GetFileName(this.TargetAsset));
            }
            else
            {
                this.TargetAsset = "";
                context.SetLocalValue(ConditionType.Target.ToString(), "", ready: false);
                context.SetLocalValue(ConditionType.TargetPathOnly.ToString(), "", ready: false);
                context.SetLocalValue(ConditionType.TargetWithoutPath.ToString(), "", ready: false);
            }

            return changed;
        }

        /// <summary>Update the 'FromFile' value, and add the relevant tokens to the patch context.</summary>
        /// <param name="context">The local patch context (already updated from the parent context).</param>
        /// <returns>Returns whether the field changed.</returns>
        private bool UpdateFromFile(LocalContext context)
        {
            // no value
            if (this.ManagedRawFromAsset == null)
            {
                this.FromAsset = null;
                context.SetLocalValue(ConditionType.FromFile.ToString(), "");
                return false;
            }

            // update
            bool changed = this.ManagedRawFromAsset.UpdateContext(context);
            if (this.RawFromAsset.IsReady)
            {
                this.FromAsset = this.NormalizeLocalAssetPath(this.RawFromAsset.Value, logName: $"{nameof(PatchConfig.FromFile)} field");
                context.SetLocalValue(ConditionType.FromFile.ToString(), this.FromAsset);
            }
            else
            {
                this.FromAsset = null;
                context.SetLocalValue(ConditionType.FromFile.ToString(), "", ready: false);
            }

            return changed;
        }

        /// <summary>Get a normalized file path relative to the content pack folder.</summary>
        /// <param name="path">The relative asset path.</param>
        /// <param name="logName">A descriptive name for the field being normalized shown in error messages.</param>
        private string NormalizeLocalAssetPath(string path, string logName)
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
                    if (File.Exists($"{fullPath}.xnb") || System.IO.Path.GetExtension(path) == ".xnb")
                        newPath += ".xnb";
                }

                return newPath;
            }
            catch (Exception ex)
            {
                throw new FormatException($"The {logName} for patch '{this.Path}' isn't a valid asset path (current value: '{path}').", ex);
            }
        }
    }
}
