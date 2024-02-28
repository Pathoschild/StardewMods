namespace Pathoschild.Stardew.TractorMod.Framework.Config
{
    /// <summary>Configuration for an attachment which just has on/off and might be long-range.</summary>
    internal class ExtendedDistanceConfig : IExtendedDistanceConfig
    {
        /// <summary>Whether to enable the attachment.</summary>
        public bool Enable { get; set; } = true;

        /// <summary>If true, causes the area of effect to be increased somewhat from <see cref="ModConfig.Distance"/></summary>
        public bool IncreaseDistance { get; set; } = true;
    }
}
