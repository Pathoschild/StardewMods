using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Pathoschild.Stardew.Common.Integrations;
using StardewModdingAPI;

namespace Common.Integrations.GenericModConfigMenu
{
    internal class GenericModConfigMenuIntegration : BaseIntegration
    {
        private readonly IGenericModConfigMenuApi ModApi;
        public GenericModConfigMenuIntegration(IModRegistry modRegistry, IMonitor monitor)
            : base("Generic Mod Config Menu", "spacechase0.GenericModConfigMenu", "1.1.0", modRegistry, monitor)
        {
            if (!this.IsLoaded)
                return;
            // get mod API
            this.ModApi = this.GetValidatedApi<IGenericModConfigMenuApi>();
            this.IsLoaded = this.ModApi != null;
        }

        public void RegisterModConfig(IManifest mod, Action revertToDefault, Action saveToFile)
        {
            this.AssertLoaded();
            this.ModApi.RegisterModConfig(mod, revertToDefault, saveToFile);
        }

        public void RegisterLabel(IManifest mod, string labelName, string labelDesc)
        {
            this.AssertLoaded();
            this.ModApi.RegisterLabel(mod, labelName, labelDesc);
        }
        public void RegisterSimpleOption(IManifest mod, string optionName, string optionDesc, Func<bool> optionGet, Action<bool> optionSet)
        {
            this.AssertLoaded();
            this.ModApi.RegisterSimpleOption(mod, optionName, optionDesc, optionGet, optionSet);
        }
        public void RegisterSimpleOption(IManifest mod, string optionName, string optionDesc, Func<int> optionGet, Action<int> optionSet)
        {
            this.AssertLoaded();
            this.ModApi.RegisterSimpleOption(mod, optionName, optionDesc, optionGet, optionSet);
        }
        public void RegisterSimpleOption(IManifest mod, string optionName, string optionDesc, Func<float> optionGet, Action<float> optionSet)
        {
            this.AssertLoaded();
            this.ModApi.RegisterSimpleOption(mod, optionName, optionDesc, optionGet, optionSet);
        }
        public void RegisterSimpleOption(IManifest mod, string optionName, string optionDesc, Func<string> optionGet, Action<string> optionSet)
        {
            this.AssertLoaded();
            this.ModApi.RegisterSimpleOption(mod, optionName, optionDesc, optionGet, optionSet);
        }
        public void RegisterSimpleOption(IManifest mod, string optionName, string optionDesc, Func<SButton> optionGet, Action<SButton> optionSet)
        {
            this.AssertLoaded();
            this.ModApi.RegisterSimpleOption(mod, optionName, optionDesc, optionGet, optionSet);
        }

        public void RegisterClampedOption(IManifest mod, string optionName, string optionDesc, Func<int> optionGet, Action<int> optionSet, int min, int max)
        {
            this.AssertLoaded();
            this.ModApi.RegisterClampedOption(mod, optionName, optionDesc, optionGet, optionSet, min, max);
        }
        public void RegisterClampedOption(IManifest mod, string optionName, string optionDesc, Func<float> optionGet, Action<float> optionSet, float min, float max)
        {
            this.AssertLoaded();
            this.ModApi.RegisterClampedOption(mod, optionName, optionDesc, optionGet, optionSet, min, max);
        }
        public void RegisterChoiceOption(IManifest mod, string optionName, string optionDesc, Func<string> optionGet, Action<string> optionSet, string[] choices)
        {
            this.AssertLoaded();
            this.ModApi.RegisterChoiceOption(mod, optionName, optionDesc, optionGet, optionSet, choices);
        }

        public void RegisterComplexOption(IManifest mod, string optionName, string optionDesc, Func<Vector2, object, object> widgetUpdate,
                                                        Func<SpriteBatch, Vector2, object, object> widgetDraw, Action<object> onSave)
        {
            this.AssertLoaded();
            this.ModApi.RegisterComplexOption(mod, optionName, optionDesc, widgetUpdate, widgetDraw, onSave);
        }
    }
}
