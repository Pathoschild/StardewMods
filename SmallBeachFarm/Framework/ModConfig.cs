namespace Pathoschild.Stardew.SmallBeachFarm.Framework
{
    /// <summary>The mod configuration.</summary>
    internal class ModConfig
    {
        /*********
        ** Accessors
        *********/
        /// <summary>Whether to regenerate the <c>{season}_beach_flipped.png</c> files when the game is launched, which enables automatic compatibility with many map recolors.</summary>
        public bool RegenerateFlippedBeach { get; set; } = true;
    }
}
