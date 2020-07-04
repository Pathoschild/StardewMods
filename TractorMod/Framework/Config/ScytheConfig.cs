namespace Pathoschild.Stardew.TractorMod.Framework.Config
{
    /// <summary>Configuration for the scythe attachment.</summary>
    internal class ScytheConfig
    {
        /// <summary>Whether to harvest crops.</summary>
        public bool HarvestCrops { get; set; } = true;

        /// <summary>Whether to harvest flowers.</summary>
        public bool HarvestFlowers { get; set; } = true;

        /// <summary>Whether to harvest forage.</summary>
        public bool HarvestForage { get; set; } = true;

        /// <summary>Whether to harvest fruit trees.</summary>
        public bool HarvestFruitTrees { get; set; } = true;

        /// <summary>Whether to collect machine output.</summary>
        public bool HarvestMachines { get; set; } = false;

        /// <summary>Whether to cut tall grass.</summary>
        public bool HarvestGrass { get; set; } = true;

        /// <summary>Whether to clear dead crops.</summary>
        public bool ClearDeadCrops { get; set; } = true;

        /// <summary>Whether to clear weeds.</summary>
        public bool ClearWeeds { get; set; } = true;
    }
}
