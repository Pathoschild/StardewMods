using Pathoschild.Stardew.TractorMod.Framework.Config;
using StardewModdingAPI;
using StardewValley;

namespace Pathoschild.Stardew.TractorMod.Framework
{
    internal abstract class ExtendedDistanceAttachment<TConfig> : BaseAttachment
        where TConfig : IExtendedDistanceConfig

    {
        /// <summary>Construct an instance.</summary>
        /// <param name="config">The mod configuration.</param>
        protected ExtendedDistanceAttachment(TConfig config, IModRegistry modRegistry, IReflectionHelper reflection, int rateLimit = 0)
            : base(modRegistry, reflection, rateLimit)
        {
            this.Config = config;
        }

        protected TConfig Config { get; }

        public override bool IsDistanceExtended => this.Config.IncreaseDistance;
    }
}
