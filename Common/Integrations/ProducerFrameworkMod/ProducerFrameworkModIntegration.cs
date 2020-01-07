using System;
using System.Collections.Generic;
using System.Text;
using StardewModdingAPI;

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

        public ProducerFrameworkModIntegration(IModRegistry modRegistry, IMonitor monitor)
            : base("Producer Framework Mod", "Digus.ProducerFrameworkMod", "1.3.0", modRegistry, monitor)
        {
            if (!this.IsLoaded)
                return;

            // get mod API
            this.ModApi = this.GetValidatedApi<IProducerFrameworkModApi>();
            this.IsLoaded = this.ModApi != null;
        }

        public IEnumerable<Dictionary<string,object>> GetRecipes(StardewValley.Object producer)
        {
            this.AssertLoaded();
            return this.ModApi.GetRecipes(producer);
        }

        public IEnumerable<Dictionary<string, object>> GetRecipes()
        {
            this.AssertLoaded();
            return this.ModApi.GetRecipes();
        }
    }
}
