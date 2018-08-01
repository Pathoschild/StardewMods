namespace Pathoschild.Stardew.TractorMod.Framework.Config
{
    /// <summary>Configuration for the melee weapon attachment.</summary>
    internal class MeleeWeaponConfig
    {
        /// <summary>Whether to clear dead crops.</summary>
        public bool ClearDeadCrops { get; set; } = true;

        /// <summary>Whether to attack monsters.</summary>
        public bool AttackMonsters { get; set; } = false;
    }
}
