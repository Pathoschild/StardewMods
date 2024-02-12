using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using ContentPatcher.Framework.Conditions;
using ContentPatcher.Framework.ConfigModels;
using ContentPatcher.Framework.Migrations;
using ContentPatcher.Framework.Tokens;
using Microsoft.Xna.Framework;
using Pathoschild.Stardew.Common.Utilities;
using StardewModdingAPI;
using StardewModdingAPI.Utilities;
using StardewValley;

namespace ContentPatcher.Framework.Patches
{
    /// <summary>Metadata for a conditional patch.</summary>
    internal abstract class Patch : IPatch
    {
        /*********
        ** Fields
        *********/
        /// <summary>Parse an asset name.</summary>
        private readonly Func<string, IAssetName> ParseAssetNameImpl;

        /// <summary>The underlying contextual values.</summary>
        protected readonly AggregateContextual Contextuals = new();

        /// <summary>The tokens that are updated manually, rather than via <see cref="Contextuals"/>.</summary>
        private readonly HashSet<IContextual> ManuallyUpdatedTokens = new(new ObjectReferenceComparer<IContextual>());

        /// <summary>Diagnostic info about the instance.</summary>
        protected readonly ContextualState State = new();

        /// <summary>The context which provides tokens specific to this patch like <see cref="ConditionType.Target"/>.</summary>
        private readonly LocalContext PrivateContext;

        /// <summary>Whether the <see cref="FromAsset"/> file exists.</summary>
        private bool FromAssetExistsImpl;

        /// <summary>The <see cref="RawFromAsset"/> with support for managing its state.</summary>
        protected IManagedTokenString? ManagedRawFromAsset { get; }

        /// <summary>The <see cref="RawTargetAsset"/> with support for managing its state.</summary>
        protected IManagedTokenString? ManagedRawTargetAsset { get; }

        /// <summary>Whether the patch has a 'FromFile' field specified, regardless of whether it's ready.</summary>
        [MemberNotNullWhen(true, nameof(Patch.RawFromAsset), nameof(Patch.ManagedRawFromAsset))]
        protected bool HasFromAsset => this.RawFromAsset != null && this.ManagedRawFromAsset != null;

        /// <summary>Whether the patch has a 'Target' field specified, regardless of whether it's ready.</summary>
        [MemberNotNullWhen(true, nameof(Patch.RawTargetAsset), nameof(Patch.ManagedRawTargetAsset))]
        protected bool HasTargetAsset => this.RawTargetAsset != null && this.ManagedRawTargetAsset != null;

        /// <summary>The cached result for <see cref="GetTokensUsed"/>.</summary>
        protected IInvariantSet? TokensUsedCache;


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
        public IRuntimeMigration Migrator { get; }

        /// <inheritdoc />
        public IPatch? ParentPatch { get; }

        /// <inheritdoc />
        public bool IsMutable { get; } = true;

        /// <inheritdoc />
        public bool IsReady { get; protected set; }

        /// <inheritdoc />
        public string? FromAsset { get; private set; }

        /// <inheritdoc />
        public ITokenString? RawFromAsset => this.ManagedRawFromAsset;

        /// <inheritdoc />
        public IAssetName? TargetAsset { get; private set; }

        /// <inheritdoc />
        public IAssetName? TargetAssetBeforeRedirection { get; private set; }

        /// <inheritdoc />
        public ITokenString? RawTargetAsset => this.ManagedRawTargetAsset;

        /// <inheritdoc />
        public int Priority { get; }

        /// <inheritdoc />
        public UpdateRate UpdateRate { get; set; }

        /// <inheritdoc />
        public Condition[] Conditions { get; }

        /// <inheritdoc />
        public bool IsApplied { get; set; }

        /// <inheritdoc />
        public int LastChangedTick { get; protected set; }


        /*********
        ** Public methods
        *********/
        /// <inheritdoc />
        public virtual bool UpdateContext(IContext context)
        {
            // skip unneeded updates
            if (!this.IsMutable && this.Contextuals.WasEverUpdated)
                return false;

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
            if (this.ManagedRawTargetAsset?.UsesTokens(InternalConstants.FromFileTokens) == true)
                changed |= this.UpdateFromFile(this.PrivateContext) | this.UpdateTargetPath(this.PrivateContext);
            else
                changed |= this.UpdateTargetPath(this.PrivateContext) | this.UpdateFromFile(this.PrivateContext);
            isReady &= this.RawTargetAsset?.IsReady != false && this.RawFromAsset?.IsReady != false;

            // update contextuals
            if (isReady)
            {
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
            }

            // check from asset existence
            if (isReady && this.HasFromAsset && this.FromAsset != null)
            {
                this.FromAssetExistsImpl = this.ContentPack.HasFile(this.FromAsset);
                if (!this.FromAssetExistsImpl && this.Conditions.All(p => p.IsMatch))
                    this.State.AddError($"{nameof(PatchConfig.FromFile)} '{this.FromAsset}' does not exist");
            }

            // update
            this.IsReady = isReady;
            if (changed || this.IsReady != wasReady)
                return this.MarkUpdated();

            return false;
        }

        /// <inheritdoc />
        [MemberNotNullWhen(true, nameof(Patch.RawFromAsset), nameof(Patch.FromAsset), nameof(Patch.ManagedRawFromAsset))]
        public bool FromAssetExists()
        {
            return this.FromAssetExistsImpl;
        }

        /// <inheritdoc />
        public virtual T Load<T>(IAssetName assetName)
            where T : notnull
        {
            throw new NotSupportedException("This patch type doesn't support loading assets.");
        }

        /// <inheritdoc />
        public virtual void Edit<T>(IAssetData asset)
            where T : notnull
        {
            throw new NotSupportedException("This patch type doesn't support loading assets.");
        }

        /// <inheritdoc />
        public IInvariantSet GetTokensUsed()
        {
            return this.TokensUsedCache ??= this.Contextuals.GetTokensUsed();
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
        /// <param name="priority">The priority for this patch when multiple patches apply.</param>
        /// <param name="updateRate">When the patch should be updated.</param>
        /// <param name="conditions">The conditions which determine whether this patch should be applied.</param>
        /// <param name="parseAssetName">Parse an asset name.</param>
        /// <param name="contentPack">The content pack which requested the patch.</param>
        /// <param name="migrator">The aggregate migration which applies for this patch.</param>
        /// <param name="parentPatch">The parent <see cref="PatchType.Include"/> patch for which this patch was loaded, if any.</param>
        /// <param name="fromAsset">The normalized asset key from which to load the local asset (if applicable), including tokens.</param>
        protected Patch(int[] indexPath, LogPathBuilder path, PatchType type, IManagedTokenString? assetName, int priority, UpdateRate updateRate, IEnumerable<Condition> conditions, IContentPack contentPack, IRuntimeMigration migrator, IPatch? parentPatch, Func<string, IAssetName> parseAssetName, IManagedTokenString? fromAsset = null)
        {
            this.IndexPath = indexPath;
            this.Path = path;
            this.Type = type;
            this.ManagedRawTargetAsset = assetName;
            this.Priority = priority;
            this.UpdateRate = updateRate;
            this.Conditions = conditions.ToArray();
            this.ParseAssetNameImpl = parseAssetName;
            this.PrivateContext = new LocalContext(scope: contentPack.Manifest.UniqueID);
            this.ManagedRawFromAsset = fromAsset;
            this.ContentPack = contentPack;
            this.Migrator = migrator;
            this.ParentPatch = parentPatch;

            this.Contextuals
                .Add(this.Conditions)
                .Add(assetName)
                .Add(fromAsset);
            if (assetName != null)
                this.ManuallyUpdatedTokens.Add(assetName);
            if (fromAsset != null)
                this.ManuallyUpdatedTokens.Add(fromAsset);

            this.LastChangedTick = Game1.ticks;
        }

        /// <summary>Track that the patch values were updated.</summary>
        /// <returns>Returns <c>true</c> for convenience in <see cref="UpdateContext"/>.</returns>
        protected bool MarkUpdated()
        {
            this.LastChangedTick = Game1.ticks;

            return true;
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
        protected bool TryReadArea(TokenRectangle? tokenArea, int defaultX, int defaultY, int defaultWidth, int defaultHeight, out Rectangle area, [NotNullWhen(false)] out string? error)
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
            if (!this.HasTargetAsset)
                return false;

            bool changed = this.ManagedRawTargetAsset.UpdateContext(context);

            if (this.RawTargetAsset.IsReady)
            {
                IAssetName assetName = this.ParseAssetNameImpl(this.RawTargetAsset.Value!);
                IAssetName? redirectedTo = this.Migrator.RedirectTarget(assetName, this);

                this.TargetAsset = redirectedTo ?? assetName;
                this.TargetAssetBeforeRedirection = redirectedTo != null ? assetName : null;

                context.SetLocalValue(nameof(ConditionType.Target), this.TargetAsset.Name);
                context.SetLocalValue(nameof(ConditionType.TargetPathOnly), System.IO.Path.GetDirectoryName(this.TargetAsset.Name));
                context.SetLocalValue(nameof(ConditionType.TargetWithoutPath), System.IO.Path.GetFileName(this.TargetAsset.Name));
            }
            else
            {
                this.TargetAsset = null;
                context.SetLocalValue(nameof(ConditionType.Target), "", ready: false);
                context.SetLocalValue(nameof(ConditionType.TargetPathOnly), "", ready: false);
                context.SetLocalValue(nameof(ConditionType.TargetWithoutPath), "", ready: false);
            }

            return changed;
        }

        /// <summary>Update the 'FromFile' value, and add the relevant tokens to the patch context.</summary>
        /// <param name="context">The local patch context (already updated from the parent context).</param>
        /// <returns>Returns whether the field changed.</returns>
        private bool UpdateFromFile(LocalContext context)
        {
            // no value
            if (!this.HasFromAsset)
            {
                this.FromAsset = null;
                context.SetLocalValue(nameof(ConditionType.FromFile), "");
                return false;
            }

            // update
            bool changed = this.ManagedRawFromAsset.UpdateContext(context);
            if (this.RawFromAsset.IsReady)
            {
                this.FromAsset = this.NormalizeLocalAssetPath(this.RawFromAsset.Value!, logName: $"{nameof(PatchConfig.FromFile)} field");
                context.SetLocalValue(nameof(ConditionType.FromFile), this.FromAsset);
            }
            else
            {
                this.FromAsset = null;
                context.SetLocalValue(nameof(ConditionType.FromFile), "", ready: false);
            }

            return changed;
        }

        /// <summary>Get a normalized file path relative to the content pack folder.</summary>
        /// <param name="path">The relative asset path.</param>
        /// <param name="logName">A descriptive name for the field being normalized shown in error messages.</param>
        private string? NormalizeLocalAssetPath(string? path, string logName)
        {
            try
            {
                // ignore empty paths
                if (string.IsNullOrWhiteSpace(path))
                    return null;

                // normalize format
                string newPath = PathUtilities.NormalizePath(path);

                // add .xnb extension if needed (it's stripped from asset names)
                string fullPath = this.ContentPack.GetFullPath(newPath);
                if (!File.Exists(fullPath))
                {
                    if (File.Exists($"{fullPath}.xnb"))
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
