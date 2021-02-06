namespace ContentPatcher.Framework.Patches
{
    /// <summary>Metadata about a patch update for verbose logging.</summary>
    internal class PatchAuditChange
    {
        /*********
        ** Accessors
        *********/
        /// <summary>The patch whose state changed.</summary>
        public IPatch Patch { get; }

        /// <summary>The value of <see cref="IPatch.IsReady"/> before the update.</summary>
        public bool WasReady { get; }

        /// <summary>The value of <see cref="IPatch.FromAsset"/> before the update.</summary>
        public string WasFromAsset { get; }

        /// <summary>The value of <see cref="IPatch.TargetAsset"/> before the update.</summary>
        public string WasTargetAsset { get; }

        /// <summary>Whether the asset will be invalidated from the cache as a result of the change.</summary>
        public bool WillInvalidate { get; }


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="patch">The patch whose state changed.</param>
        /// <param name="wasReady">The value of <see cref="IPatch.IsReady"/> before the update.</param>
        /// <param name="wasFromAsset">The value of <see cref="IPatch.FromAsset"/> before the update.</param>
        /// <param name="wasTargetAsset">The value of <see cref="IPatch.TargetAsset"/> before the update.</param>
        /// <param name="willInvalidate">Whether the asset will be invalidated from the cache as a result of the change.</param>
        public PatchAuditChange(IPatch patch, bool wasReady, string wasFromAsset, string wasTargetAsset, bool willInvalidate)
        {
            this.Patch = patch;
            this.WasReady = wasReady;
            this.WasFromAsset = wasFromAsset;
            this.WasTargetAsset = wasTargetAsset;
            this.WillInvalidate = willInvalidate;
        }
    }
}
