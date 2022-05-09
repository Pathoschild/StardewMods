using System.Collections.Generic;
using System.Reflection;

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
        public IDictionary<string, string?> GetCustomSettings()
        {
            Dictionary<string, string?> customSettings = new();

            var defaults = new ModConfigMachine();
            foreach (PropertyInfo property in this.GetType().GetProperties())
            {
                object? curValue = property.GetValue(this);
                object? defaultValue = property.GetValue(defaults);

                bool equal = defaultValue?.Equals(curValue) ?? curValue is null;
                if (!equal)
                    customSettings[property.Name] = curValue?.ToString();
            }

            return customSettings;
        }
    }
}
