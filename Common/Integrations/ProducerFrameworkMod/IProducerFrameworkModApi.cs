using System.Collections.Generic;
using SObject = StardewValley.Object;

namespace Pathoschild.Stardew.Common.Integrations.ProducerFrameworkMod
{
    /// <summary>The API provided by the Producer Framework Mod.</summary>
    public interface IProducerFrameworkModApi
    {
        /// <summary>Get the list of recipes.</summary>
        /// <remarks>The recipe format follow the MachineRecipeData class properties from Lookup Anything mod. There are some additional properties that are not presented on that class, these ones has the name of the content pack properties of this mod.</remarks>
        List<Dictionary<string, object>> GetRecipes();

        /// <summary>Get the list of recipes for a machine.</summary>
        /// <param name="machine">The machine object.</param>
        /// <remarks>The recipe format follow the MachineRecipeData class properties from Lookup Anything mod. There are some additional properties that are not presented on that class, these ones has the name of the content pack properties of this mod.</remarks>
        List<Dictionary<string, object>> GetRecipes(SObject machine);
    }
}
