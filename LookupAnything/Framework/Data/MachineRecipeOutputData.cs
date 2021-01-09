namespace Pathoschild.Stardew.LookupAnything.Framework.Data
{
    /// <summary>Metadata for an output item in a machine recipe.</summary>
    internal class MachineRecipeOutputData
    {
        /*********
        ** Accessors
        *********/
        /// <summary>The item IDs created by the recipe.</summary>
        public int[] Ids { get; set; }

        /// <summary>The minimum number of items produced by the recipe (or <c>null</c> for the default).</summary>
        public int? MinOutput { get; set; }

        /// <summary>The maximum number of items produced by the recipe (or <c>null</c> for the default).</summary>
        public int? MaxOutput { get; set; }

        /// <summary>The percentage chance of this recipe being produced (or <c>null</c> if the recipe is always used).</summary>
        public decimal? OutputChance { get; set; }
    }
}
