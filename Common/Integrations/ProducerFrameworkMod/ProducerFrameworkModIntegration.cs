using System.Collections.Generic;
using System.Linq;
using StardewModdingAPI;
using SObject = StardewValley.Object;

namespace Pathoschild.Stardew.Common.Integrations.ProducerFrameworkMod
{
    /// <summary>Handles the logic for integrating with the Producer Framework Mod.</summary>
    internal class ProducerFrameworkModIntegration : BaseIntegration
    {
        /*********
        ** Fields
        *********/
        /// <summary>The mod's public API.</summary>
        private readonly IProducerFrameworkModApi ModApi;


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="modRegistry">An API for fetching metadata about loaded mods.</param>
        /// <param name="monitor">Encapsulates monitoring and logging.</param>
        public ProducerFrameworkModIntegration(IModRegistry modRegistry, IMonitor monitor)
            : base("Producer Framework Mod", "Digus.ProducerFrameworkMod", "1.3.0", modRegistry, monitor)
        {
            if (!this.IsLoaded)
                return;

            // get mod API
            this.ModApi = this.GetValidatedApi<IProducerFrameworkModApi>();
            this.IsLoaded = this.ModApi != null;
        }

        /// <summary>Get the list of recipes.</summary>
        /// <remarks>The recipe format follow the MachineRecipeData class properties from Lookup Anything mod. There are some additional properties that are not presented on that class, these ones has the name of the content pack properties of this mod.</remarks>
        public IEnumerable<ProducerFrameworkRecipe> GetRecipes()
        {
            this.AssertLoaded();

            return this.ModApi.GetRecipes().Select(this.ReadRecipe);
        }

        /// <summary>Get the list of recipes for a machine.</summary>
        /// <param name="machine">The machine object.</param>
        /// <remarks>The recipe format follow the MachineRecipeData class properties from Lookup Anything mod. There are some additional properties that are not presented on that class, these ones has the name of the content pack properties of this mod.</remarks>
        public IEnumerable<ProducerFrameworkRecipe> GetRecipes(SObject machine)
        {
            this.AssertLoaded();

            return this.ModApi.GetRecipes(machine).Select(this.ReadRecipe);
        }


        /*********
        ** Private methods
        *********/
        /// <summary>Read the metadata for a recipe provided by <see cref="GetRecipes()"/> or <see cref="GetRecipes(SObject)"/>.</summary>
        /// <param name="raw">The raw recipe data.</param>
        private ProducerFrameworkRecipe ReadRecipe(IDictionary<string, object> raw)
        {
            ProducerFrameworkRecipe recipe = new ProducerFrameworkRecipe();
            recipe.InputId = raw["InputKey"] as int?;
            recipe.MachineId = (int)raw["MachineID"];
            recipe.Ingredients = ((List<Dictionary<string, object>>)raw["Ingredients"]).ToDictionary(p => (int)p["ID"], p => (int)p["Count"]);
            recipe.ExceptIngredients = ((List<Dictionary<string, object>>)raw["ExceptIngredients"]).Select(p => (int)p["ID"]).ToArray();
            recipe.OutputId = (int)raw["Output"];
            recipe.MinOutput = (int)raw["MinOutput"];
            recipe.MaxOutput = (int)raw["MaxOutput"];
            recipe.PreserveType = (SObject.PreserveType?)raw["PreserveType"];
            recipe.OutputChance = (double)raw["OutputChance"];
            return recipe;
        }
    }
}
