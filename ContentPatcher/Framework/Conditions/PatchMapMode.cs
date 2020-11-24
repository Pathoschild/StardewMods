namespace ContentPatcher.Framework.Conditions
{
    /// <summary>Indicates how a map should be patched.</summary>
    public enum PatchMapMode
    {
        /// <summary>Overlay the source map onto the target map. Layers and tiles in the target area which don't have an equivalent in the source area are kept as-is.</summary>
        Overlay,

        /// <summary>Fully replace the target area with the source area. All target tiles are erased and replaced by the new content.</summary>
        Replace,

        /// <summary>Remove target tiles if their layer doesn't exist in the source map.</summary>
        ClearMissingLayers,

        /// <summary>Remove target tiles if there's no matching tile on the source layer.</summary>
        ClearMissingTiles
    }
}
