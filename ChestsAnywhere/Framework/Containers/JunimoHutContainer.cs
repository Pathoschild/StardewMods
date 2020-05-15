using StardewModdingAPI;
using StardewValley.Buildings;

namespace Pathoschild.Stardew.ChestsAnywhere.Framework.Containers
{
    /// <summary>A storage container for an in-game Junimo huts.</summary>
    internal class JunimoHutContainer : ChestContainer
    {
        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="junimoHut">The in-game junimo hut.</param>
        /// <param name="reflection">Simplifies access to private code.</param>
        public JunimoHutContainer(JunimoHut junimoHut, IReflectionHelper reflection)
            : base(junimoHut.output.Value, context: junimoHut, showColorPicker: false, reflection) { }
    }
}
