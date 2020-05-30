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
        /// <summary>Get the current config model.</summary>
        private readonly Func<ModConfig> GetConfig;

        /// <summary>Reset the config model to the default values.</summary>
        private readonly Action Reset;

        /// <summary>Save and apply the current config model.</summary>
        private readonly Action SaveAndApply;


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="getConfig">Get the current config model.</param>
        /// <param name="reset">Reset the config model to the default values.</param>
        /// <param name="saveAndApply">Save and apply the current config model.</param>
        public GenericModConfigMenuIntegrationForTractor(Func<ModConfig> getConfig, Action reset, Action saveAndApply)
        {
            this.GetConfig = getConfig;
            this.Reset = reset;
            this.SaveAndApply = saveAndApply;
        }

        /// <summary>Register the config menu if available.</summary>
        /// <param name="modRegistry">An API for fetching metadata about loaded mods.</param>
        /// <param name="monitor">Encapsulates monitoring and logging.</param>
        /// <param name="manifest">The mod manifest.</param>
        public void Register(IModRegistry modRegistry, IMonitor monitor, IManifest manifest)
        {
            GenericModConfigMenuIntegration configMenu = new GenericModConfigMenuIntegration(modRegistry, monitor);
            if (!configMenu.IsLoaded)
                return;

            configMenu.RegisterModConfig(manifest, this.Reset, this.SaveAndApply);
            configMenu.RegisterLabel(manifest, "Basic Config Options", "Config Options");

            configMenu.RegisterLabel(manifest, "", "");
            Regex r = new Regex(@"(?<=[A-Z])(?=[A-Z][a-z])|(?<=[^A-Z])(?=[A-Z])|(?<=[A-Za-z])(?=[^A-Za-z])");

            var config = this.GetConfig;
            foreach (var property in config().GetType().GetProperties())
            {
                string label = r.Replace(property.Name, " ");
                if (property.PropertyType == typeof(bool))
                {
                    configMenu.RegisterSimpleOption(manifest, label, "", () => (bool)property.GetValue(config(), null), (bool val) => property.SetValue(config(), val, null));
                }
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
                            configMenu.RegisterClampedOption(manifest, label, "", () => (int)property.GetValue(config(), null), (int val) => property.SetValue(config(), val, null), defaultValue, Math.Abs(defaultValue * 15));
                        }
                    }

                }
            }

            configMenu.RegisterLabel(manifest, "Tool Config Options", "Config Options");
            foreach (var field in config().StandardAttachments.GetType().GetFields())
            {
                string label = r.Replace(field.Name, " ");
                configMenu.RegisterLabel(manifest, label, "Config Options");
                object subValue = field.GetValue(config().StandardAttachments);
                foreach (var property in field.FieldType.GetProperties())
                {
                    label = r.Replace(property.Name, " ");
                    configMenu.RegisterSimpleOption(manifest, label, "", () => (bool)property.GetValue(subValue), (bool val) => property.SetValue(subValue, val, null));
                }
            }
        }
    }
}
