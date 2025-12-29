using StardewModdingAPI;

namespace ContentPatcher.Framework.Conditions;

/// <summary>Indicates how an image should be patched.</summary>
public enum PatchImageMode
{
    /// <inheritdoc cref="PatchMode.Replace" />
    Replace = PatchMode.Replace,

    /// <inheritdoc cref="PatchMode.Overlay" />
    Overlay = PatchMode.Overlay,

    /// <inheritdoc cref="PatchMode.Mask" />
    Mask = PatchMode.Mask
}
