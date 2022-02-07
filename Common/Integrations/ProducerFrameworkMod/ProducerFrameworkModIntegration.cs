//using System;
//using System.Collections.Generic;
//using System.Diagnostics.CodeAnalysis;
//using System.Linq;
//using StardewModdingAPI;
//using PreserveType = StardewValley.Object.PreserveType;
//using SObject = StardewValley.Object;

//namespace Pathoschild.Stardew.Common.Integrations.ProducerFrameworkMod
//{
//    /// <summary>Handles the logic for integrating with the Producer Framework Mod.</summary>
//    internal class ProducerFrameworkModIntegration : BaseIntegration<IProducerFrameworkModApi>
//    {
//        /*********
//        ** Fields
//        *********/
//        /// <summary>Whether the integration has already logged a warning caused by an invalid recipe.</summary>
//        private bool LoggedInvalidRecipeError;


//        /*********
//        ** Public methods
//        *********/
//        /// <summary>Construct an instance.</summary>
//        /// <param name="modRegistry">An API for fetching metadata about loaded mods.</param>
//        /// <param name="monitor">Encapsulates monitoring and logging.</param>
//        public ProducerFrameworkModIntegration(IModRegistry modRegistry, IMonitor monitor)
//            : base("Producer Framework Mod", "Digus.ProducerFrameworkMod", "1.3.0", modRegistry, monitor) { }

//        /// <summary>Get the list of recipes.</summary>
//        /// <remarks>The recipe format follow the MachineRecipeData class properties from Lookup Anything mod. There are some additional properties that are not presented on that class, these ones has the name of the content pack properties of this mod.</remarks>
//        public IEnumerable<ProducerFrameworkRecipe> GetRecipes()
//        {
//            this.AssertLoaded();

//            return this.ReadRecipes(this.ModApi.GetRecipes());
//        }

//        /// <summary>Get the list of recipes for a machine.</summary>
//        /// <param name="machine">The machine object.</param>
//        /// <remarks>The recipe format follow the MachineRecipeData class properties from Lookup Anything mod. There are some additional properties that are not presented on that class, these ones has the name of the content pack properties of this mod.</remarks>
//        public IEnumerable<ProducerFrameworkRecipe> GetRecipes(SObject machine)
//        {
//            this.AssertLoaded();

//            return this.ReadRecipes(this.ModApi.GetRecipes(machine));
//        }


//        /*********
//        ** Private methods
//        *********/
//        /// <summary>Read the metadata for recipes provided by <see cref="GetRecipes()"/> or <see cref="GetRecipes(SObject)"/>.</summary>
//        /// <param name="raw">The raw recipe data.</param>
//        private IEnumerable<ProducerFrameworkRecipe> ReadRecipes(IEnumerable<IDictionary<string, object?>> raw)
//        {
//            return raw
//                .Select(this.ReadRecipe)
//                .WhereNotNull();
//        }


//        /// <summary>Read the metadata for a recipe provided by <see cref="GetRecipes()"/> or <see cref="GetRecipes(SObject)"/>.</summary>
//        /// <param name="raw">The raw recipe data.</param>
//        [SuppressMessage("ReSharper", "UseObjectOrCollectionInitializer", Justification = "Avoid object initializer so it's clear which line failed in stack traces reported by players, since we don't control the format being parsed.")]
//        private ProducerFrameworkRecipe? ReadRecipe(IDictionary<string, object?> raw)
//        {
//            try
//            {
//                int? inputId = raw["InputKey"] as int?;
//                int machineId = (int)raw["MachineID"]!;
//                ProducerFrameworkIngredient[] ingredients = ((List<Dictionary<string, object?>>)raw["Ingredients"]!).Select(this.ReadIngredient).ToArray();
//                int?[] exceptIngredients = ((List<Dictionary<string, object?>>)raw["ExceptIngredients"]!).Select(this.ReadIngredient).Select(p => p.InputId).ToArray();
//                int outputId = (int)raw["Output"]!;
//                int minOutput = (int)raw["MinOutput"]!;
//                int maxOutput = (int)raw["MaxOutput"]!;
//                PreserveType? preserveType = (PreserveType?)raw["PreserveType"];
//                double outputChance = (double)raw["OutputChance"]!;

//                return new ProducerFrameworkRecipe(
//                    inputId: inputId,
//                    machineId: machineId,
//                    ingredients: ingredients,
//                    exceptIngredients: exceptIngredients,
//                    outputId: outputId,
//                    minOutput: minOutput,
//                    maxOutput: maxOutput,
//                    preserveType: preserveType,
//                    outputChance: outputChance
//                );
//            }
//            catch (Exception ex)
//            {
//                if (!this.LoggedInvalidRecipeError)
//                {
//                    this.LoggedInvalidRecipeError = true;
//                    this.Monitor.Log("Failed to load some recipes from Producer Framework Mod. Some custom machines may not appear in lookups.", LogLevel.Warn);
//                    this.Monitor.Log(ex.ToString());
//                }
//                return null;
//            }
//        }

//        /// <summary>Read an ingredient if it's supported.</summary>
//        /// <param name="raw">The raw ingredient model.</param>
//        private ProducerFrameworkIngredient ReadIngredient(IDictionary<string, object?> raw)
//        {
//            int? id = int.TryParse(raw["ID"]?.ToString(), out int parsedId)
//                ? parsedId
//                : null;
//            int count = raw.TryGetValue("Count", out object? rawCount) && rawCount != null
//                ? (int)rawCount
//                : 1;
//            return new ProducerFrameworkIngredient { InputId = id, Count = count };
//        }
//    }
//}
