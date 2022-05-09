using System.Linq;

namespace Pathoschild.Stardew.LookupAnything.Framework.Fields.Models
{
    /// <summary>The recipes in a type group with column alignment sizes.</summary>
    /// <param name="Type">The recipe type.</param>
    /// <param name="Recipes">The recipes in the group.</param>
    /// <param name="ColumnWidths">The width of each column in the group.</param>
    internal record RecipeByTypeGroup(string Type, RecipeEntry[] Recipes, float[] ColumnWidths)
    {
        /*********
        ** Accessors
        *********/
        /// <summary>The combined column width, excluding spacing between columns.</summary>
        public float TotalColumnWidth { get; } = ColumnWidths.Sum(p => p);
    }
}
