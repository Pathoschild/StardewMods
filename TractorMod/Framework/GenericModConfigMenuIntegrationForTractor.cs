using System;
using System.Text.RegularExpressions;
using Common.Integrations.GenericModConfigMenu;
using StardewModdingAPI;

namespace Pathoschild.Stardew.TractorMod.Framework
{
    /// <summary>Registers the mod configuration with Generic Mod Config Menu.</summary>
    internal class GenericModConfigMenuIntegrationForTractor
    {
        /*********
        ** Fields
        *********/
        /// <summary>The Generic Mod Config Menu integration.</summary>
        private readonly GenericModConfigMenuIntegration<ModConfig> ConfigMenu;

        /// <summary>Get the current config model.</summary>
        private readonly Func<ModConfig> GetConfig;


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="modRegistry">An API for fetching metadata about loaded mods.</param>
        /// <param name="monitor">Encapsulates monitoring and logging.</param>
        /// <param name="manifest">The mod manifest.</param>
        /// <param name="getConfig">Get the current config model.</param>
        /// <param name="reset">Reset the config model to the default values.</param>
        /// <param name="saveAndApply">Save and apply the current config model.</param>
        public GenericModConfigMenuIntegrationForTractor(IModRegistry modRegistry, IMonitor monitor, IManifest manifest, Func<ModConfig> getConfig, Action reset, Action saveAndApply)
        {
            this.ConfigMenu = new GenericModConfigMenuIntegration<ModConfig>(modRegistry, monitor, manifest, getConfig, reset, saveAndApply);
            this.GetConfig = getConfig;
        }

        /// <summary>Register the config menu if available.</summary>
        public void Register()
        {
            var menu = this.ConfigMenu;
            if (!menu.IsLoaded)
                return;

            menu
                .RegisterConfig()
                .AddLabel("Basic Config Options", "Config Options")
                .AddLabel("", "");

            Regex r = new Regex(@"(?<=[A-Z])(?=[A-Z][a-z])|(?<=[^A-Z])(?=[A-Z])|(?<=[A-Za-z])(?=[^A-Za-z])");

            foreach (var property in this.GetConfig().GetType().GetProperties())
            {
                string label = r.Replace(property.Name, " ");
                if (property.PropertyType == typeof(bool))
                    menu.AddCheckbox(label, "", config => (bool)property.GetValue(config, null), (config, val) => property.SetValue(config, val, null));

                else if (property.PropertyType == typeof(int))
                {
                    //Don't do anything with the price
                    if (property.Name.Contains("Price"))
                        continue;

                    var defaultValues = new ModConfig();
                    foreach (var defaultProperty in defaultValues.GetType().GetProperties())
                    {
                        if (property.Name == defaultProperty.Name)
                        {
                            int defaultValue = (int)defaultProperty.GetValue(defaultValues);
                            menu.AddNumberField(label, "", config => (int)property.GetValue(config, null), (config, val) => property.SetValue(config, val, null), defaultValue, Math.Abs(defaultValue * 15));
                        }
                    }

                }
            }

            menu.AddLabel("Tool Config Options", "Config Options");
            foreach (var field in this.GetConfig().StandardAttachments.GetType().GetFields())
            {
                string label = r.Replace(field.Name, " ");
                menu.AddLabel(label, "Config Options");
                object subValue = field.GetValue(this.GetConfig().StandardAttachments);
                foreach (var property in field.FieldType.GetProperties())
                {
                    label = r.Replace(property.Name, " ");
                    menu.AddCheckbox(label, "", config => (bool)property.GetValue(subValue), (config, val) => property.SetValue(subValue, val, null));
                }
            }
        }
    }
}
