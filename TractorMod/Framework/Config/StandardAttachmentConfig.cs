using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pathoschild.Stardew.TractorMod.Framework.Config
{
    /// <summary>A set of control bindings.</summary>
    internal class StandardAttachmentsConfig
    {
        /// <summary>The Axe attachment.</summary>
        public AxeConfig Axe = new AxeConfig();

        /// <summary>The Fertilizer attachment.</summary>
        public FertilizerConfig Fertilizer = new FertilizerConfig();

        /// <summary>The Grass Starter attachment.</summary>
        public GrassStarterConfig GrassStarter = new GrassStarterConfig();

        /// <summary>The Hoe attachment.</summary>
        public HoeConfig Hoe = new HoeConfig();

        /// <summary>The PickAxe attachment.</summary>
        public PickAxeConfig PickAxe = new PickAxeConfig();

        /// <summary>The Scythe attachment.</summary>
        public ScytheConfig Scythe = new ScytheConfig();

        /// <summary>The Seeds attachment.</summary>
        public SeedsConfig Seeds = new SeedsConfig();

        /// <summary>The Watering Can attachment.</summary>
        public WateringCanConfig WateringCan = new WateringCanConfig();

        /// <summary>The SeedBag attachment.</summary>
        public SeedBagModConfig SeedBagMod = new SeedBagModConfig();
    }
}
