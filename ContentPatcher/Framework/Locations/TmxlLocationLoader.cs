using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Xml;
using Newtonsoft.Json;
using StardewModdingAPI;
using StardewValley;

namespace ContentPatcher.Framework.Locations
{
    /// <summary>Handles loading locations from TMXL Map Toolkit's serialized data.</summary>
    [SuppressMessage("ReSharper", "CommentTypo", Justification = "'TMXL' is not a typo.")]
    [SuppressMessage("ReSharper", "IdentifierTypo", Justification = "'TMXL' is not a typo.")]
    [SuppressMessage("ReSharper", "StringLiteralTypo", Justification = "'TMXL' is not a typo.")]
    internal class TmxlLocationLoader
    {
        /*********
        ** Fields
        *********/
        /// <summary>Encapsulates monitoring and logging.</summary>
        private readonly IMonitor Monitor;

        /// <summary>The serialized TMXL location data by name.</summary>
        private readonly Lazy<IDictionary<string, string>> SerializedLocations;


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="monitor">Encapsulates monitoring and logging.</param>
        public TmxlLocationLoader(IMonitor monitor)
        {
            this.Monitor = monitor;
            this.SerializedLocations = new(this.GetSerializedLocations);
        }

        /// <summary>Try to load a location from the TMXL Map Toolkit's serialized data.</summary>
        /// <param name="name">The location name to load.</param>
        /// <param name="location">The loaded location data, if applicable.</param>
        /// <returns>Returns whether the location was successfully loaded.</returns>
        public bool TryGetLocation(string name, out GameLocation location)
        {
            if (this.SerializedLocations.Value.TryGetValue(name, out string xml) && this.TryDeserialize(name, xml, out location))
                return true;

            location = null;
            return false;
        }


        /*********
        ** Private methods
        *********/
        /// <summary>Try to deserialize a location from the TMXL Map Toolkit data.</summary>
        /// <param name="name">The location name for logged exceptions.</param>
        /// <param name="xml">The raw serialized XML to parse.</param>
        /// <param name="location">The parsed location, if applicable.</param>
        /// <returns>Returns whether the location was successfully deserialized.</returns>
        private bool TryDeserialize(string name, string xml, out GameLocation location)
        {
            try
            {
                using var stringReader = new StringReader(xml);
                using var xmlReader = XmlReader.Create(stringReader, new XmlReaderSettings { ConformanceLevel = ConformanceLevel.Auto });

                location = (GameLocation)SaveGame.locationSerializer.Deserialize(xmlReader);
                return true;
            }
            catch (Exception ex)
            {
                this.Monitor.Log($"Couldn't parse the '{name}' location data from TMXL Map Toolkit. The location may not be migrated correctly.", LogLevel.Warn);
                this.Monitor.Log(ex.ToString());
                location = null;
                return false;
            }
        }

        /// <summary>Get the raw serialized locations from the TMXL Map Toolkit data.</summary>
        private IDictionary<string, string> GetSerializedLocations()
        {
            try
            {
                if (SaveGame.loaded.CustomData.TryGetValue("smapi/mod-data/platonymous.tmxloader/locations", out string json) && !string.IsNullOrWhiteSpace(json))
                {
                    var saveData = JsonConvert.DeserializeObject<SaveData>(json);
                    return saveData.Locations.ToDictionary(p => p.Name, p => p.Objects);
                }
            }
            catch (Exception ex)
            {
                this.Monitor.Log("Couldn't parse location data from TMXL Map Toolkit; if a content pack is migrating TMXL locations to Content Patcher, they may not be migrated correctly.", LogLevel.Warn);
                this.Monitor.Log(ex.ToString());
            }

            return new Dictionary<string, string>();
        }

        /// <summary>The model for TMXL Map Toolkit's save data.</summary>
        [SuppressMessage("ReSharper", "ClassNeverInstantiated.Local", Justification = "Used via deserialization.")]
        [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Local", Justification = "Used via deserialization.")]
        private class SaveData
        {
            /// <summary>The serialized location data.</summary>
            public SaveLocation[] Locations { get; set; }

            /// <summary>The data model for a serialized TMXL location.</summary>
            public class SaveLocation
            {
                /// <summary>The location name.</summary>
                public string Name { get; set; }

                /// <summary>The serialized location instance.</summary>
                public string Objects { get; set; }
            }
        }
    }
}
