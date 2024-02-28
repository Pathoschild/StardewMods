using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using StardewModdingAPI;

namespace Pathoschild.Stardew.DataLayers.Framework
{
    /// <summary>Simplifies accessing display colors loaded from <c>assets/colors.json</c>.</summary>
    internal class ColorScheme
    {
        /*********
        ** Fields
        *********/
        /// <summary>The applied color scheme ID.</summary>
        private readonly string Id;

        /// <summary>The available colors.</summary>
        private readonly Dictionary<string, Color> Colors;

        /// <summary>The monitor with which to log error messages.</summary>
        private readonly IMonitor Monitor;

        /// <summary>The filename within the mod folder in which colors are stored.</summary>
        public const string AssetName = "assets/colors.json";


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="id">The applied color scheme ID.</param>
        /// <param name="colors">The available colors.</param>
        /// <param name="monitor">The monitor with which to log error messages.</param>
        public ColorScheme(string id, Dictionary<string, Color> colors, IMonitor monitor)
        {
            this.Id = id;
            this.Colors = colors;
            this.Monitor = monitor;
        }

        /// <summary>Get a display color.</summary>
        /// <param name="layerId">The unique ID for the layer getting colors.</param>
        /// <param name="colorName">The color name (without the layer prefix), like <c>Selected</c>.</param>
        /// <param name="defaultColor">The color to use if it's not in the color scheme.</param>
        public Color Get(string layerId, string colorName, Color defaultColor)
        {
            string key = layerId + "_" + colorName;

            if (!this.Colors.TryGetValue(key, out Color color))
            {
                bool isDefaultScheme = ColorScheme.IsDefaultColorScheme(this.Id);

                this.Monitor.LogOnce($"Layer '{layerId}' expected color '{key}'{(!isDefaultScheme ? $" in color scheme '{this.Id}'" : "")} in {ColorScheme.AssetName}, but it wasn't found. The default color will be used instead.", LogLevel.Warn);
                color = defaultColor;
            }

            return color;
        }

        /// <summary>Get whether a color scheme ID is the default one.</summary>
        /// <param name="id">The color scheme ID to check.</param>
        public static bool IsDefaultColorScheme(string id)
        {
            return string.Equals(id, "Default", StringComparison.OrdinalIgnoreCase);
        }
    }
}
