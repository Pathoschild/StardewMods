using Pathoschild.Stardew.TractorMod.Framework.Config;
using StardewModdingAPI;

namespace Pathoschild.Stardew.TractorMod.Framework
{
    /// <summary>The base class for attachments that can operate over a larger area.
    /// </summary>
    /// <typeparam name="TConfig">The type of the config file.</typeparam>
    internal abstract class ExtendedDistanceAttachment<TConfig> : BaseAttachment
        where TConfig : IExtendedDistanceConfig

    {
        /// <summary>Construct an instance.</summary>
        /// <param name="config">The configuration for the attachment.</param>
        protected ExtendedDistanceAttachment(TConfig config, IModRegistry modRegistry, int rateLimit = 0)
            : base(modRegistry, rateLimit)
        {
            this.Config = config;
        }

        /// <summary>The configuration for the attachment.</summary>
        protected TConfig Config { get; }

        /// <summary>If true, the attachment's area of affect should be expanded by a tile.</summary>
        public override bool IsDistanceExtended => this.Config.IncreaseDistance;
    }
}
