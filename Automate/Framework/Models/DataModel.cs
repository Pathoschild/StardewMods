using System.Collections.Generic;
using System.Runtime.Serialization;
using Pathoschild.Stardew.Common;

namespace Pathoschild.Stardew.Automate.Framework.Models
{
    /// <summary>The model for Automate's internal file containing data that can't be derived automatically.</summary>
    internal class DataModel
    {
        /*********
        ** Accessors
        *********/
        /// <summary>The name to use for each floor ID.</summary>
        public Dictionary<int, DataModelFloor> FloorNames { get; set; } = new();

        /// <summary>Mods which add custom machine recipes and require a separate automation component.</summary>
        public DataModelIntegration[] SuggestedIntegrations { get; set; } = new DataModelIntegration[0];

        /// <summary>The configuration for specific machines by ID.</summary>
        public Dictionary<string, ModConfigMachine> DefaultMachineOverrides { get; set; } = new();


        /*********
        ** Public methods
        *********/
        /// <summary>Normalize the model after it's deserialized.</summary>
        /// <param name="context">The deserialization context.</param>
        [OnDeserialized]
        public void OnDeserialized(StreamingContext context)
        {
            this.FloorNames ??= new();
            this.SuggestedIntegrations ??= new DataModelIntegration[0];
            this.DefaultMachineOverrides = this.DefaultMachineOverrides.ToNonNullCaseInsensitive();
        }
    }
}
