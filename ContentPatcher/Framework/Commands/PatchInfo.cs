using System;
using System.Collections.Generic;
using System.Linq;
using ContentPatcher.Framework.Conditions;
using ContentPatcher.Framework.Patches;
using StardewModdingAPI;
using StardewModdingAPI.Events;

namespace ContentPatcher.Framework.Commands
{
    /// <summary>A summary of patch info shown in the SMAPI console.</summary>
    internal class PatchInfo : PatchBaseInfo
    {
        /*********
        ** Accessors
        *********/
        /// <summary>The path to the patch from the root content file.</summary>
        public LogPathBuilder Path { get; }

        /// <summary>The <see cref="Path"/> without the content pack prefix.</summary>
        public LogPathBuilder PathWithoutContentPackPrefix { get; }

        /// <summary>The raw patch type.</summary>
        public string? RawType { get; }

        /// <summary>The parsed patch type, if valid.</summary>
        public PatchType? ParsedType { get; }

        /// <summary>The local asset name to load.</summary>
        public string? RawFromAsset { get; set; }

        /// <summary>The parsed asset name to load (if available).</summary>
        public ITokenString? ParsedFromAsset { get; set; }

        /// <summary>The asset name to intercept.</summary>
        public string? RawTargetAsset { get; }

        /// <summary>The parsed asset name (if available).</summary>
        public ITokenString? ParsedTargetAsset { get; }

        /// <summary>The priority for this patch when multiple patches apply.</summary>
        /// <remarks>This is an <see cref="AssetLoadPriority"/> or <see cref="AssetEditPriority"/> value, depending on the patch type.</remarks>
        public int? Priority { get; }

        /// <summary>The content pack which requested the patch.</summary>
        public IContentPack ContentPack { get; }

        /// <summary>Whether the patch is loaded.</summary>
        public bool IsLoaded { get; }

        /// <summary>Whether the patch is currently applied.</summary>
        public bool IsApplied { get; }

        /// <summary>The underlying patch, if any.</summary>
        public IPatch? Patch { get; }


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="patch">The patch to represent.</param>
        public PatchInfo(DisabledPatch patch)
            : this(
                path: patch.Path,
                rawType: patch.RawType,
                parsedType: patch.ParsedType,
                priority: null,
                conditions: Array.Empty<Condition>(),
                matchesContext: false,
                state: new ContextualState().AddError(patch.ReasonDisabled),
                contentPack: patch.ContentPack
            )
        {
            this.RawTargetAsset = patch.AssetName;
        }

        /// <summary>Construct an instance.</summary>
        /// <param name="patch">The patch to represent.</param>
        public PatchInfo(IPatch patch)
            : this(
                path: patch.Path,
                rawType: patch.Type.ToString(),
                parsedType: patch.Type,
                priority: patch.Priority,
                conditions: patch.Conditions,
                matchesContext: patch.IsReady,
                state: patch.GetDiagnosticState(),
                contentPack: patch.ContentPack
            )
        {
            this.ParsedType = patch.Type;
            this.RawFromAsset = patch.RawFromAsset?.Raw;
            this.ParsedFromAsset = patch.RawTargetAsset;

            this.RawTargetAsset = patch.RawTargetAsset?.Raw;
            this.ParsedTargetAsset = patch.RawTargetAsset;
            this.IsLoaded = true;
            this.IsApplied = patch.IsApplied;
            this.Patch = patch;
        }

        /// <summary>Get a human-readable list of changes applied to the asset for display when troubleshooting.</summary>
        public IEnumerable<string> GetChangeLabels()
        {
            return this.Patch?.GetChangeLabels() ?? Enumerable.Empty<string>();
        }

        /// <summary>Get a human-readable reason that the patch isn't applied.</summary>
        public override string? GetReasonNotLoaded()
        {
            return !this.IsApplied
                ? base.GetReasonNotLoaded()
                : null;
        }


        /*********
        ** Private methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="path">The path to the patch from the root content file.</param>
        /// <param name="rawType">The raw patch type.</param>
        /// <param name="parsedType">The parsed patch type, if valid.</param>
        /// <param name="priority">The priority for this patch when multiple patches apply.</param>
        /// <param name="conditions">The parsed conditions (if available).</param>
        /// <param name="contentPack">The content pack which requested the patch.</param>
        /// <param name="matchesContext">Whether the patch should be applied in the current context.</param>
        /// <param name="state">Diagnostic info about the patch.</param>
        private PatchInfo(LogPathBuilder path, string? rawType, PatchType? parsedType, int? priority, Condition[] conditions, IContentPack contentPack, bool matchesContext, IContextualState state)
            : base(conditions, matchesContext, state)
        {
            this.Path = path;
            this.RawType = rawType;
            this.ParsedType = parsedType;
            this.Priority = priority;
            this.ContentPack = contentPack;

            this.PathWithoutContentPackPrefix = new LogPathBuilder(path.Segments.Skip(1));
        }
    }
}
