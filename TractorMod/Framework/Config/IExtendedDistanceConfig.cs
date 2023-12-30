namespace Pathoschild.Stardew.TractorMod.Framework.Config
{
    /// <summary>
    ///   Attachments that can affect a wider radius implement this config
    /// </summary>
    internal interface IExtendedDistanceConfig
    {
        /// <summary>If true, causes the area of effect to be increased somewhat from <see cref="ModConfig.Distance"/></summary>
        public bool IncreaseDistance { get; set; }
    }
}
