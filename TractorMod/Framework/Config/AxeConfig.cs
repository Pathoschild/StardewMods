namespace Pathoschild.Stardew.TractorMod.Framework.Config
{
    /// <summary>Configuration for the axe attachment.</summary>
    internal class AxeConfig
    {
        /// <summary>Whether to clear fruit tree seeds.</summary>
        public bool ClearFruitTreeSeeds { get; set; }

        /// <summary>Whether to clear fruit trees that aren't fully grown.</summary>
        public bool ClearFruitTreeSaplings { get; set; }

        /// <summary>Whether to clear fully-grown fruit trees.</summary>
        public bool CutGrownFruitTrees { get; set; }

        /// <summary>Whether to clear non-fruit tree seeds.</summary>
        public bool ClearTreeSeeds { get; set; }

        /// <summary>Whether to clear non-fruit trees that aren't fully grown.</summary>
        public bool ClearTreeSaplings { get; set; }

        /// <summary>Whether to clear full-grown non-fruit trees.</summary>
        public bool CutGrownTrees { get; set; }

        /// <summary>Whether to cut non-fruit trees that have a tapper.</summary>
        public bool CutTappedTrees { get; set; }

        /// <summary>Whether to cut choppable bushes.</summary>
        public bool CutBushes { get; set; }

        /// <summary>Whether to cut giant crops.</summary>
        public bool CutGiantCrops { get; set; } = true;

        /// <summary>Whether to clear live crops.</summary>
        public bool ClearLiveCrops { get; set; }

        /// <summary>Whether to clear dead crops.</summary>
        public bool ClearDeadCrops { get; set; } = true;

        /// <summary>Whether to clear debris like weeds, twigs, giant stumps, and fallen logs.</summary>
        public bool ClearDebris { get; set; } = true;
    }
}
