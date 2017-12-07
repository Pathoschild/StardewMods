using Microsoft.Xna.Framework;

namespace Pathoschild.Stardew.DataMaps.Framework
{
    /// <summary>An entry to display in the overlay legend.</summary>
    internal struct LegendEntry
    {
        /*********
        ** Accessors
        *********/
        /// <summary>The entry name.</summary>
        public string Name { get; }

        /// <summary>The tile color.</summary>
        public Color Color { get; }


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="name">The entry name.</param>
        /// <param name="color">The tile color.</param>
        public LegendEntry(string name, Color color)
        {
            this.Name = name;
            this.Color = color;
        }
    }
}
