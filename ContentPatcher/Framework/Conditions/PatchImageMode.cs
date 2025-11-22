using StardewModdingAPI;

namespace ContentPatcher.Framework.Conditions;

/// <summary>Indicates how an image should be patched.</summary>
public enum PatchImageMode
{
    /// <summary>Erase the original content within the area before drawing the new content.</summary>
    Replace = PatchMode.Replace,

    /// <summary>Draw the new content over the original content, so the original content shows through any transparent or semi-transparent pixels.</summary>
    Overlay = PatchMode.Overlay,

    /// <summary>Masks the original content so that the alpha value of every pixel in the new content gets subtracted from the corresponding pixel in the original content.</summary>
    Mask = PatchMode.Mask
}
