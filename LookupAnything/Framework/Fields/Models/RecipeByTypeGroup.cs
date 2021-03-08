using System.Collections.Generic;
using System.Linq;

namespace Pathoschild.Stardew.LookupAnything.Framework.Fields.Models
{
    /// <summary>The recipes in a type group with column alignment sizes.</summary>
    internal class RecipeByTypeGroup
    {
        /*********
        ** Accessors
        *********/
        /// <summary>The recipe type.</summary>
        public string Type { get; }

        /// <summary>The recipes in the group.</summary>
        public RecipeEntry[] Recipes { get; }

        /// <summary>The combined column width, excluding spacing between columns.</summary>
        public float TotalColumnWidth { get; }

        /// <summary>The width of each column in the group.</summary>
        public float[] ColumnWidths { get; }


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="type">The recipe type.</param>
        /// <param name="recipes">The recipes in the group.</param>
        /// <param name="columnWidths">The width of each column in the group.</param>
        public RecipeByTypeGroup(string type, IEnumerable<RecipeEntry> recipes, IEnumerable<float> columnWidths)
        {
            this.Type = type;
            this.Recipes = recipes.ToArray();
            this.ColumnWidths = columnWidths.ToArray();
            this.TotalColumnWidth = this.ColumnWidths.Sum(p => p);
        }
    }
}
