namespace Pathoschild.Stardew.TractorMod.Framework.Config
{
    /// <summary>Configuration for the axe attachment.</summary>
    internal class AxeConfig
    {
        /// <summary>Whether to chop down fruit trees.</summary>
        public bool CutFruitTrees { get; set; }

        /// <summary>Whether to chop down trees which have a tapper.</summary>
        public bool CutTappedTrees { get; set; }

        /// <summary>Whether to chop down non-fruit trees.</summary>
        public bool CutTrees { get; set; }

        /// <summary>Whether to clear live crops.</summary>
        public bool ClearLiveCrops { get; set; }

        /// <summary>Whether to clear dead crops.</summary>
        public bool ClearDeadCrops { get; set; } = true;

        /// <summary>Whether to clear debris.</summary>
        public bool ClearDebris { get; set; } = true;
    }
}
