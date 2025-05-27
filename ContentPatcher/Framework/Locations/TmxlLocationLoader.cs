using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Xml;
using System.Xml.Serialization;
using Newtonsoft.Json;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Characters;
using StardewValley.Monsters;
using StardewValley.Objects;
using StardewValley.TerrainFeatures;

namespace ContentPatcher.Framework.Locations;

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

    /// <summary>Equivalent to <see cref="SaveGame.locationSerializer"/>.</summary>
    /// <remarks>This is separate to avoid 'changes the save serializer' warnings, since it's only for compatibility with older TMXL locations.</remarks>
    private readonly Lazy<XmlSerializer> LocationSerializer = new(() => new(typeof(GameLocation), [
        typeof (Tool),
        typeof (Duggy),
        typeof (Ghost),
        typeof (GreenSlime),
        typeof (RockCrab),
        typeof (ShadowGuy),
        typeof (Child),
        typeof (Pet),
#pragma warning disable CS0618 // deliberate to support older saves
        typeof (Dog),
        typeof (Cat),
#pragma warning restore CS0618
        typeof (Horse),
        typeof (SquidKid),
        typeof (Grub),
        typeof (Fly),
        typeof (DustSpirit),
        typeof (Bug),
        typeof (BigSlime),
        typeof (BreakableContainer),
        typeof (MetalHead),
        typeof (ShadowGirl),
        typeof (Monster),
        typeof (JunimoHarvester),
        typeof (TerrainFeature)
    ]));


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
    public bool TryGetLocation(string name, [NotNullWhen(true)] out GameLocation? location)
    {
        if (this.SerializedLocations.Value.TryGetValue(name, out string? xml) && this.TryDeserialize(name, xml, out location))
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
    private bool TryDeserialize(string name, string xml, [NotNullWhen(true)] out GameLocation? location)
    {
        try
        {
            using var stringReader = new StringReader(xml);
            using var xmlReader = XmlReader.Create(stringReader, new XmlReaderSettings { ConformanceLevel = ConformanceLevel.Auto });

            location = (GameLocation?)this.LocationSerializer.Value.Deserialize(xmlReader);
            return location != null;
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
    private Dictionary<string, string> GetSerializedLocations()
    {
        try
        {
            if (SaveGame.loaded.CustomData.TryGetValue("smapi/mod-data/platonymous.tmxloader/locations", out string? json) && !string.IsNullOrWhiteSpace(json))
            {
                Dictionary<string, string> serializedLocations = [];

                var saveData = JsonConvert.DeserializeObject<SaveData>(json);
                if (saveData is not null)
                {
                    foreach (SaveLocation location in saveData.Locations)
                        serializedLocations[location.Name] = location.Objects; // if there are duplicates, TMXL overwrites the objects with the last instance
                }

                return serializedLocations;
            }
        }
        catch (Exception ex)
        {
            this.Monitor.Log("Couldn't parse location data from TMXL Map Toolkit; if a content pack is migrating TMXL locations to Content Patcher, they may not be migrated correctly.", LogLevel.Warn);
            this.Monitor.Log(ex.ToString());
        }

        return [];
    }

    /// <summary>The model for TMXL Map Toolkit's save data.</summary>
    /// <param name="Locations">The serialized location data.</param>
    private record SaveData(SaveLocation[] Locations);

    /// <summary>The data model for a serialized TMXL location.</summary>
    /// <param name="Name">The location name.</param>
    /// <param name="Objects">The serialized location instance.</param>
    public record SaveLocation(string Name, string Objects);
}
