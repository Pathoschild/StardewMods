using System.Diagnostics.CodeAnalysis;
using System.Runtime.Serialization;
using Pathoschild.Stardew.Common;

namespace Pathoschild.Stardew.DataLayers.Framework
{
    /// <summary>Configures the settings for each data layer.</summary>
    internal class ModConfigLayers
    {
        /*********
        ** Accessors
        *********/
        /// <summary>Settings for the accessible layer.</summary>
        public LayerConfig Accessible { get; set; } = new(updatesPerSecond: 2);

        /// <summary>Settings for the buildable layer.</summary>
        public LayerConfig Buildable { get; set; } = new(updatesPerSecond: 2);

        /// <summary>Settings for the bee house layer.</summary>
        public LayerConfig CoverageForBeeHouses { get; set; } = new(updatesPerSecond: 60);

        /// <summary>Settings for the Junimo hut layer.</summary>
        public LayerConfig CoverageForJunimoHuts { get; set; } = new(updatesPerSecond: 60);

        /// <summary>Settings for the scarecrow layer.</summary>
        public LayerConfig CoverageForScarecrows { get; set; } = new(updatesPerSecond: 60);

        /// <summary>Settings for the sprinkler layer.</summary>
        public LayerConfig CoverageForSprinklers { get; set; } = new(updatesPerSecond: 60);

        /// <summary>Settings for the fertilizer layer.</summary>
        public LayerConfig CropFertilizer { get; set; } = new(updatesPerSecond: 30);

        /// <summary>Settings for the crop harvest layer.</summary>
        public LayerConfig CropHarvest { get; set; } = new(updatesPerSecond: 2);

        /// <summary>Settings for the crop water layer.</summary>
        public LayerConfig CropWater { get; set; } = new(updatesPerSecond: 30);

        /// <summary>Settings for the crop paddy water layer.</summary>
        public LayerConfig CropPaddyWater { get; set; } = new(updatesPerSecond: 30);

        /// <summary>Settings for the machine processing layer.</summary>
        public LayerConfig Machines { get; set; } = new(updatesPerSecond: 2);

        /// <summary>Settings for the tile grid layer.</summary>
        public LayerConfig TileGrid { get; set; } = new(updatesPerSecond: 1);

        /// <summary>Settings for the tillable layer.</summary>
        public LayerConfig Tillable { get; set; } = new(updatesPerSecond: 2);


        /*********
        ** Public methods
        *********/
        /// <summary>Normalize the model after it's deserialized.</summary>
        /// <param name="context">The deserialization context.</param>
        [OnDeserialized]
        [SuppressMessage("ReSharper", "NullCoalescingConditionIsAlwaysNotNullAccordingToAPIContract", Justification = SuppressReasons.MethodValidatesNullability)]
        [SuppressMessage("ReSharper", "UnusedMember.Global", Justification = SuppressReasons.UsedViaOnDeserialized)]
        public void OnDeserialized(StreamingContext context)
        {
            this.Accessible ??= new LayerConfig(updatesPerSecond: 2);
            this.Buildable ??= new LayerConfig(updatesPerSecond: 2);
            this.CoverageForBeeHouses ??= new LayerConfig(updatesPerSecond: 60);
            this.CoverageForJunimoHuts ??= new LayerConfig(updatesPerSecond: 60);
            this.CoverageForScarecrows ??= new LayerConfig(updatesPerSecond: 60);
            this.CoverageForSprinklers ??= new LayerConfig(updatesPerSecond: 60);
            this.CropFertilizer ??= new LayerConfig(updatesPerSecond: 30);
            this.CropHarvest ??= new LayerConfig(updatesPerSecond: 2);
            this.CropWater ??= new LayerConfig(updatesPerSecond: 30);
            this.CropPaddyWater ??= new LayerConfig(updatesPerSecond: 30);
            this.Machines ??= new LayerConfig(updatesPerSecond: 2);
            this.TileGrid ??= new LayerConfig(updatesPerSecond: 1);
            this.Tillable ??= new LayerConfig(updatesPerSecond: 2);
        }
    }
}
