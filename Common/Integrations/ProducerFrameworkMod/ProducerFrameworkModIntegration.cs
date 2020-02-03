using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
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

        /// <summary>Whether the integration has already logged a warning caused by an invalid recipe.</summary>
        private bool LoggedInvalidRecipeError = false;


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

            return this.ReadRecipes(this.ModApi.GetRecipes());
        }

        /// <summary>Get the list of recipes for a machine.</summary>
        /// <param name="machine">The machine object.</param>
        /// <remarks>The recipe format follow the MachineRecipeData class properties from Lookup Anything mod. There are some additional properties that are not presented on that class, these ones has the name of the content pack properties of this mod.</remarks>
        public IEnumerable<ProducerFrameworkRecipe> GetRecipes(SObject machine)
        {
            this.AssertLoaded();

            return this.ReadRecipes(this.ModApi.GetRecipes(machine));
        }


        /*********
        ** Private methods
        *********/
        /// <summary>Read the metadata for recipes provided by <see cref="GetRecipes()"/> or <see cref="GetRecipes(SObject)"/>.</summary>
        /// <param name="raw">The raw recipe data.</param>
        private IEnumerable<ProducerFrameworkRecipe> ReadRecipes(IEnumerable<IDictionary<string, object>> raw)
        {
            return raw
                .Select(this.ReadRecipe)
                .Where(p => p != null);
        }


        /// <summary>Read the metadata for a recipe provided by <see cref="GetRecipes()"/> or <see cref="GetRecipes(SObject)"/>.</summary>
        /// <param name="raw">The raw recipe data.</param>
        [SuppressMessage("ReSharper", "UseObjectOrCollectionInitializer", Justification = "Avoid object initializer so it's clear which line failed in stack traces reported by players, since we don't control the format being parsed.")]
        private ProducerFrameworkRecipe ReadRecipe(IDictionary<string, object> raw)
        {
            try
            {
                ProducerFrameworkRecipe recipe = new ProducerFrameworkRecipe();
                recipe.InputId = raw["InputKey"] as int?;
                recipe.MachineId = (int)raw["MachineID"];
                recipe.Ingredients = ((List<Dictionary<string, object>>)raw["Ingredients"]).Select(this.ReadIngredient).ToArray();
                recipe.ExceptIngredients = ((List<Dictionary<string, object>>)raw["ExceptIngredients"]).Select(this.ReadIngredient).Select(p => p.InputId).ToArray();
                recipe.OutputId = (int)raw["Output"];
                recipe.MinOutput = (int)raw["MinOutput"];
                recipe.MaxOutput = (int)raw["MaxOutput"];
                recipe.PreserveType = (SObject.PreserveType?)raw["PreserveType"];
                recipe.OutputChance = (double)raw["OutputChance"];
                return recipe;
            }
            catch (Exception ex)
            {
                if (!this.LoggedInvalidRecipeError)
                {
                    this.LoggedInvalidRecipeError = true;
                    this.Monitor.Log("Failed to load some recipes from Producer Framework Mod. Some custom machines may not appear in lookups.", LogLevel.Warn);
                    this.Monitor.Log(ex.ToString());
                }
                return null;
            }
        }

        /// <summary>Read an ingredient if it's supported.</summary>
        /// <param name="raw">The raw ingredient model.</param>
        private ProducerFrameworkIngredient ReadIngredient(IDictionary<string, object> raw)
        {
            int? id = int.TryParse(raw["ID"]?.ToString(), out int parsedId)
                ? parsedId
                : null as int?;
            int count = raw.TryGetValue("Count", out object rawCount)
                ? (int)rawCount
                : 1;
            return new ProducerFrameworkIngredient { InputId = id, Count = count };
        }
    }
}
