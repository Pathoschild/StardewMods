using ContentPatcher.Framework.Conditions;

namespace ContentPatcher.Framework.Patches
{
    /// <summary>An invalid patch that couldn't be loaded.</summary>
    internal class DisabledPatch
    {
        /*********
        ** Accessors
        *********/
        /// <summary>The path to the patch from the root content file.</summary>
        public LogPathBuilder Path { get; }

        /// <summary>The raw patch type.</summary>
        public string RawType { get; }

        /// <summary>The parsed patch type, if valid.</summary>
        public PatchType? ParsedType { get; }

        /// <summary>The raw asset name to intercept.</summary>
        public string AssetName { get; }

        /// <summary>The content pack which requested the patch.</summary>
        public ManagedContentPack ContentPack { get; }

        /// <summary>The reason this patch is disabled.</summary>
        public string ReasonDisabled { get; }

        /// <summary>The parent patch for which this patch was loaded, if any.</summary>
        public Patch ParentPatch { get; }


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="path">The path to the patch from the root content file.</param>
        /// <param name="rawType">The raw patch type.</param>
        /// <param name="parsedType">The parsed patch type, if valid.</param>
        /// <param name="assetName">The raw asset name to intercept.</param>
        /// <param name="contentPack">The content pack which requested the patch.</param>
        /// <param name="parentPatch">The parent patch for which this patch was loaded, if any.</param>
        /// <param name="reasonDisabled">The reason this patch is disabled.</param>
        public DisabledPatch(LogPathBuilder path, string rawType, PatchType? parsedType, string assetName, ManagedContentPack contentPack, Patch parentPatch, string reasonDisabled)
        {
            this.Path = path;
            this.RawType = rawType;
            this.ParsedType = parsedType;
            this.ContentPack = contentPack;
            this.AssetName = assetName;
            this.ParentPatch = parentPatch;
            this.ReasonDisabled = reasonDisabled;
        }
    }
}
