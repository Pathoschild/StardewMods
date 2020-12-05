using StardewModdingAPI;

namespace ContentPatcher.Framework.Conditions
{
    /// <summary>Indicates how an image should be patched.</summary>
    public enum PatchImageMode
    {
        /// <summary>Erase the original content within the area before drawing the new content.</summary>
        Replace = PatchMode.Replace,

        /// <summary>Draw the new content over the original content, so the original content shows through any transparent pixels.</summary>
        Overlay = PatchMode.Overlay
    }
}
