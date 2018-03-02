namespace Pathoschild.Stardew.TractorMod.Framework.Config
{
    /// <summary>Configuration for the pickaxe attachment.</summary>
    internal class PickAxeConfig
    {
        /// <summary>Whether to clear debris.</summary>
        public bool ClearDebris { get; set; } = true;

        /// <summary>Whether to clear dead crops.</summary>
        public bool ClearDeadCrops { get; set; } = true;

        /// <summary>Whether to clear tilled dirt.</summary>
        public bool ClearDirt { get; set; } = true;

        /// <summary>Whether to clear flooring.</summary>
        public bool ClearFlooring { get; set; } = false;

        /// <summary>Whether to clear boulders and meteorites.</summary>
        public bool ClearBouldersAndMeteorites { get; set; } = true;
    }
}
