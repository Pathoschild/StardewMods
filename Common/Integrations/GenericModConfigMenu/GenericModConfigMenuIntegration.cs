using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;

namespace Pathoschild.Stardew.Common.Integrations.GenericModConfigMenu
{
    /// <summary>Handles the logic for integrating with the Generic .</summary>
    internal class GenericModConfigMenuIntegration : BaseIntegration
    {
        /*********
        ** Fields
        *********/
        /// <summary>The mod's public API.</summary>
        private readonly IGenericModConfigMenuApi ModApi;

        /// <summary>The manifest for the parent mod.</summary>
        private readonly IManifest ModManifest;


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// 
        /// <param name="modRegistry">An API for fetching metadata about loaded mods.</param>
        /// <param name="monitor">Encapsulates monitoring and logging.</param>
        public GenericModConfigMenuIntegration(IManifest modManifest, IModRegistry modRegistry, IMonitor monitor)
            : base("Generic Mod Config Menu", "spacechase0.GenericModConfigMenu", "1.0", modRegistry, monitor)
        {
            if (!this.IsLoaded)
                return;

            this.ModManifest = modManifest;

            // get mod API
            this.ModApi = this.GetValidatedApi<IGenericModConfigMenuApi>();
            this.IsLoaded = this.ModApi != null;
        }

        public void RegisterConfig(Action revertToDefault, Action saveToFile)
        {
            this.AssertLoaded();
            this.ModApi.RegisterModConfig(this.ModManifest, revertToDefault, saveToFile);
        }

        public void RegisterSimpleOption(string optionName, string optionDesc, Func<bool> optionGet, Action<bool> optionSet)
        {
            this.AssertLoaded();
            this.ModApi.RegisterSimpleOption(this.ModManifest, optionName, optionDesc, optionGet, optionSet);
        }

        public void RegisterSimpleOption(string optionName, string optionDesc, Func<int> optionGet, Action<int> optionSet)
        {
            this.AssertLoaded();
            this.ModApi.RegisterSimpleOption(this.ModManifest, optionName, optionDesc, optionGet, optionSet);
        }

        public void RegisterSimpleOption(string optionName, string optionDesc, Func<float> optionGet, Action<float> optionSet)
        {
            this.AssertLoaded();
            this.ModApi.RegisterSimpleOption(this.ModManifest, optionName, optionDesc, optionGet, optionSet);
        }

        public void RegisterSimpleOption(string optionName, string optionDesc, Func<string> optionGet, Action<string> optionSet)
        {
            this.AssertLoaded();
            this.ModApi.RegisterSimpleOption(this.ModManifest, optionName, optionDesc, optionGet, optionSet);
        }

        public void RegisterSimpleOption(string optionName, string optionDesc, Func<SButton> optionGet, Action<SButton> optionSet)
        {
            this.AssertLoaded();
            this.ModApi.RegisterSimpleOption(this.ModManifest, optionName, optionDesc, optionGet, optionSet);
        }

        public void RegisterClampedOption(string optionName, string optionDesc, Func<int> optionGet, Action<int> optionSet, int min, int max)
        {
            this.AssertLoaded();
            this.ModApi.RegisterClampedOption(this.ModManifest, optionName, optionDesc, optionGet, optionSet, min, max);
        }

        public void RegisterClampedOption(string optionName, string optionDesc, Func<float> optionGet, Action<float> optionSet, float min, float max)
        {
            this.AssertLoaded();
            this.ModApi.RegisterClampedOption(this.ModManifest, optionName, optionDesc, optionGet, optionSet, min, max);
        }

        public void RegisterChoiceOption(string optionName, string optionDesc, Func<string> optionGet, Action<string> optionSet, string[] choices)
        {
            this.AssertLoaded();
            this.ModApi.RegisterChoiceOption(this.ModManifest, optionName, optionDesc, optionGet, optionSet, choices);
        }

        public void RegisterComplexOption(string optionName, string optionDesc, Func<Vector2, object, object> widgetUpdate, Func<SpriteBatch, Vector2, object, object> widgetDraw, Action<object> onSave)
        {
            this.AssertLoaded();
            this.ModApi.RegisterComplexOption(this.ModManifest, optionName, optionDesc, widgetUpdate, widgetDraw, onSave);
        }
    }
}
