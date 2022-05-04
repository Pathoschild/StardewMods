using ContentPatcher.Framework.ConfigModels;
using StardewValley;

namespace ContentPatcher.Framework.Locations
{
    /// <summary>Metadata for a location that was renamed based on the <see cref="ContentConfig.CustomLocations"/> field.</summary>
    internal class MigratedLocation
    {
        /*********
        ** Accessors
        *********/
        /// <summary>The previous location name.</summary>
        public string OldName { get; }

        /// <summary>The location in the save file which was renamed.</summary>
        public GameLocation SaveLocation { get; }

        /// <summary>The custom location data for which the location was renamed.</summary>
        public CustomLocationData CustomLocation { get; }


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="oldName">The previous location name.</param>
        /// <param name="saveLocation">The location in the save file which was renamed.</param>
        /// <param name="customLocation">The custom location data for which the location was renamed.</param>
        public MigratedLocation(string oldName, GameLocation saveLocation, CustomLocationData customLocation)
        {
            this.OldName = oldName;
            this.SaveLocation = saveLocation;
            this.CustomLocation = customLocation;
        }
    }
}
