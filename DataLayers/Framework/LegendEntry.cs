using Microsoft.Xna.Framework;
using StardewModdingAPI;

namespace Pathoschild.Stardew.DataLayers.Framework
{
    /// <summary>An entry to display in the overlay legend.</summary>
    internal class LegendEntry
    {
        /*********
        ** Accessors
        *********/
        /// <summary>A unique identifier for the legend entry.</summary>
        public string Id { get; }

        /// <summary>The entry name.</summary>
        public string Name { get; }

        /// <summary>The tile color.</summary>
        public Color Color { get; }


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="id">A unique identifier for the legend entry.</param>
        /// <param name="name">The entry name.</param>
        /// <param name="color">The tile color.</param>
        public LegendEntry(string id, string name, Color color)
        {
            this.Id = id;
            this.Name = name;
            this.Color = color;
        }

        /// <summary>Construct an instance.</summary>
        /// <param name="translations">The translation helper from which to get the display text based on the <paramref name="id"/>.</param>
        /// <param name="id">A unique identifier for the legend entry.</param>
        /// <param name="color">The tile color.</param>
        public LegendEntry(ITranslationHelper translations, string id, Color color)
            : this(id, translations.Get(id), color) { }
    }
}
