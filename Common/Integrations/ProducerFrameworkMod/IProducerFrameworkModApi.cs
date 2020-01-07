using System;
using System.Collections.Generic;
using System.Text;
using SObject = StardewValley.Object;

namespace Pathoschild.Stardew.Common.Integrations.ProducerFrameworkMod
{
    /// <summary>The API provided by the Producer Framework Mod.</summary>
    public interface IProducerFrameworkModApi
    {
        /// <summary>
        /// Get the list of recipes
        /// The recipe format follow the MachineRecipeData class properties from Lookup Anything mod.
        /// There are some additional properties that are not presented on that class, these ones has the name of the content pack properties of this mod.
        /// </summary>
        /// <returns>The list of recipes</returns>
        List<Dictionary<string, object>> GetRecipes();

        /// <summary>
        /// Get the list of recipes for the producer with the giving name.
        /// The recipe format follow the MachineRecipeData class properties from Lookup Anything mod.
        /// There are some additional properties that are not presented on that class, these ones has the name of the content pack properties of this mod.
        /// </summary>
        /// <param name="producerName">The name of the producer.</param>
        /// <returns>The list of recipes</returns>
        List<Dictionary<string, object>> GetRecipes(string producerName);

        /// <summary>
        /// Get the list of recipes for the producer.
        /// The recipe format follow the MachineRecipeData class properties from Lookup Anything mod.
        /// There are some additional properties that are not presented on that class, these ones has the name of the content pack properties of this mod.
        /// </summary>
        /// <param name="producerObject">The Stardew Valley Object for the producer.</param>
        /// <returns>The list of recipes</returns>
        List<Dictionary<string, object>> GetRecipes(SObject producerObject);
    }
}
