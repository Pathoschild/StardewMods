namespace Pathoschild.Stardew.LookupAnything.Framework.Data
{
    /// <summary>Metadata for a machine's recipes.</summary>
    internal class MachineRecipesData
    {
        /*********
        ** Accessors
        *********/
        /// <summary>The machine item ID.</summary>
        public int MachineID { get; set; }

        /// <summary>The machine recipes.</summary>
        public MachineRecipeData[] Recipes { get; set; }
    }
}
