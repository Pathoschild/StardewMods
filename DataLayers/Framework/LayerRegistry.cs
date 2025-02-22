using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Pathoschild.Stardew.DataLayers.Layers;
using Pathoschild.Stardew.DataLayers.Layers.Coverage;
using Pathoschild.Stardew.DataLayers.Layers.Crops;
using StardewModdingAPI;
using StardewModdingAPI.Utilities;

namespace Pathoschild.Stardew.DataLayers.Framework
{
    /// <summary>Manages the data layers that should be available in-game.</summary>
    internal class LayerRegistry
    {
        /*********
        ** Fields
        *********/
        /// <summary>Get the current color scheme.</summary>
        private readonly Func<ColorScheme> ColorScheme;

        /// <summary>Get the mod configuration.</summary>
        private readonly Func<ModConfig> Config;

        /// <summary>Handles access to the supported mod integrations.</summary>
        private readonly Func<ModIntegrations?> Mods;

        /// <summary>Encapsulates monitoring and logging.</summary>
        private readonly IMonitor Monitor;

        /// <summary>The cached data layers.</summary>
        private ILayer[]? Layers;

        /// <summary>The data layers registered through the API.</summary>
        private readonly Dictionary<string, LayerRegistration> CustomLayers = [];

        /// <summary>Maps key bindings to the layers they should activate.</summary>
        private readonly IDictionary<KeybindList, ILayer> ShortcutMap = new Dictionary<KeybindList, ILayer>();


        /*********
        ** Accessors
        *********/
        /// <summary>Get whether the layer registry is ready to use.</summary>
        public bool IsReady => this.GetLayers().Count > 0;


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="colorScheme">The color scheme.</param>
        /// <param name="config">The mod configuration.</param>
        /// <param name="mods">Handles access to the supported mod integrations.</param>
        /// <param name="monitor">Encapsulates monitoring and logging.</param>
        internal LayerRegistry(Func<ColorScheme> colorScheme, Func<ModConfig> config, Func<ModIntegrations?> mods, IMonitor monitor)
        {
            this.ColorScheme = colorScheme;
            this.Config = config;
            this.Mods = mods;
            this.Monitor = monitor;
        }

        /// <summary>Get the layers which should be available in-game.</summary>
        public IReadOnlyList<ILayer> GetLayers()
        {
            this.InitializeIfNeeded();

            return this.Layers;
        }

        /// <summary>Get the data for custom layers registered through the API.</summary>
        /// <remarks>To get the resulting data layers, call <see cref="GetLayers"/> instead.</remarks>
        public IEnumerable<LayerRegistration> GetCustomLayerData()
        {
            return this.CustomLayers.Values;
        }

        /// <summary>Try to get the layer whose shortcut keybind was just activated.</summary>
        /// <param name="layer">The layer that was activated, if found.</param>
        /// <param name="keybind">The keybind which activated a layer, if found.</param>
        /// <returns>Returns whether an activated layer and keybind were found.</returns>
        public bool TryGetLayerByKeybind([NotNullWhen(true)] out ILayer? layer, [NotNullWhen(true)] out KeybindList? keybind)
        {
            this.InitializeIfNeeded();

            foreach ((KeybindList key, ILayer curLayer) in this.ShortcutMap)
            {
                if (key.JustPressed())
                {
                    layer = curLayer;
                    keybind = key;
                    return true;
                }
            }

            layer = null;
            keybind = null;
            return false;
        }

        /// <summary>Register or overwrite a layer registered through the mod API.</summary>
        /// <param name="layer">The layer data to register.</param>
        public void RegisterCustomLayer(LayerRegistration layer)
        {
            this.CustomLayers[layer.UniqueId] = layer;
            this.ResetCache();
        }

        /// <summary>Reset the cached layer data.</summary>
        public void ResetCache()
        {
            this.Layers = null;
            this.ShortcutMap.Clear();
        }


        /*********
        ** Private methods
        *********/
        /// <summary>Initialize the data layers if they're not already loaded.</summary>
        [MemberNotNull(nameof(LayerRegistry.Layers))]
        private void InitializeIfNeeded()
        {
            if (this.Layers is not null)
                return;

            this.Layers = this.BuildLayersCache().ToArray();

            this.ShortcutMap.Clear();
            foreach (ILayer layer in this.Layers)
            {
                if (layer.ShortcutKey.IsBound)
                    this.ShortcutMap[layer.ShortcutKey] = layer;
            }
        }

        /// <summary>Get the enabled data layers.</summary>
        private IEnumerable<ILayer> BuildLayersCache()
        {
            ModConfig config = this.Config();
            ColorScheme colors = this.ColorScheme();
            ModIntegrations? mods = this.Mods();
            if (mods is null)
                yield break; // not initialized yet

            ModConfigLayers layers = config.Layers;

            if (layers.Accessible.IsEnabled())
                yield return new AccessibleLayer(layers.Accessible, colors);
            if (layers.Buildable.IsEnabled())
                yield return new BuildableLayer(layers.Buildable, colors);
            if (layers.CoverageForBeeHouses.IsEnabled())
                yield return new BeeHouseLayer(layers.CoverageForBeeHouses, colors);
            if (layers.CoverageForScarecrows.IsEnabled())
                yield return new ScarecrowLayer(layers.CoverageForScarecrows, colors);
            if (layers.CoverageForSprinklers.IsEnabled())
                yield return new SprinklerLayer(layers.CoverageForSprinklers, colors, mods);
            if (layers.CoverageForJunimoHuts.IsEnabled())
                yield return new JunimoHutLayer(layers.CoverageForJunimoHuts, colors, mods);
            if (layers.CropWater.IsEnabled())
                yield return new CropWaterLayer(layers.CropWater, colors);
            if (layers.CropPaddyWater.IsEnabled())
                yield return new CropPaddyWaterLayer(layers.CropPaddyWater, colors);
            if (layers.CropFertilizer.IsEnabled())
                yield return new CropFertilizerLayer(layers.CropFertilizer, colors, mods);
            if (layers.CropHarvest.IsEnabled())
                yield return new CropHarvestLayer(layers.CropHarvest, colors);
            if (layers.Machines.IsEnabled() && mods.Automate.IsLoaded)
                yield return new MachineLayer(layers.Machines, colors, mods);
            if (layers.Tillable.IsEnabled())
                yield return new TillableLayer(layers.Tillable, colors);

            foreach (LayerRegistration layer in this.CustomLayers.Values)
                yield return new ModLayer(layer, config.GetModLayerConfig(layer.UniqueId), colors, this.Monitor);

            // add separate grid layer if grid isn't enabled for all layers
            if (!config.ShowGrid && layers.TileGrid.IsEnabled())
                yield return new GridLayer(layers.TileGrid);
        }
    }
}
