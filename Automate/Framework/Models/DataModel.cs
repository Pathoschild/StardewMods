using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Pathoschild.Stardew.Common;

namespace Pathoschild.Stardew.Automate.Framework.Models
{
    /// <summary>The model for Automate's internal file containing data that can't be derived automatically.</summary>
    internal class DataModel
    {
        /*********
        ** Accessors
        *********/
        /// <summary>Mods which add custom machine recipes and require a separate automation component.</summary>
        public DataModelIntegration[] SuggestedIntegrations { get; }

        /// <summary>The configuration for specific machines by ID.</summary>
        [JsonProperty(ObjectCreationHandling = ObjectCreationHandling.Auto)]
        public Dictionary<string, ModConfigMachine> DefaultMachineOverrides { get; } = new(StringComparer.OrdinalIgnoreCase);


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="suggestedIntegrations">Mods which add custom machine recipes and require a separate automation component.</param>
        public DataModel(DataModelIntegration[]? suggestedIntegrations)
        {
            this.SuggestedIntegrations = suggestedIntegrations ?? Array.Empty<DataModelIntegration>();
            this.DefaultMachineOverrides = this.DefaultMachineOverrides.ToNonNullCaseInsensitive();
        }
    }
}
