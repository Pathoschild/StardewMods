using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Pathoschild.Stardew.Automate.Framework.Models
{
    /// <summary>Configuration settings for a specific machine.</summary>
    internal class ModConfigMachine
    {
        /*********
        ** Accessors
        *********/
        /// <summary>The priority order in which this machine should be processed relative to other machines. Machines have a default priority of 0, with higher values processed first.</summary>
        public int Priority { get; set; } = 0;

        /// <summary>Whether this machine type should be enabled.</summary>
        public bool Enabled { get; set; } = true;


        /*********
        ** Public methods
        *********/
        /// <summary>Get the machine settings which don't match the default.</summary>
        public IDictionary<string, string> GetCustomSettings()
        {
            IDictionary<string, string> customSettings = new Dictionary<string, string>();

            var defaults = new ModConfigMachine();
            foreach (var property in this.GetType().GetProperties())
            {
                object curValue = property.GetValue(this);
                object defaultValue = property.GetValue(defaults);

                if (!curValue.Equals(defaultValue))
                    customSettings[property.Name] = curValue?.ToString();
            }

            return customSettings;
        }
    }
}
