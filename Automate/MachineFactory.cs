using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Pathoschild.Stardew.Automate.Framework;
using Pathoschild.Stardew.Automate.Machines;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Locations;
using StardewValley.Objects;
using SObject = StardewValley.Object;

namespace Pathoschild.Stardew.Automate
{
    /// <summary>Constructs machine instances.</summary>
    public class MachineFactory
    {
        /*********
        ** Public methods
        *********/
        /// <summary>Get all locations containing a player chest.</summary>
        public IEnumerable<GameLocation> GetLocationsWithChests()
        {
            IEnumerable<GameLocation> locations =
                Game1.locations
                    .Concat(
                        from location in Game1.locations.OfType<BuildableGameLocation>()
                        from building in location.buildings
                        where building.indoors != null
                        select building.indoors
                    );

            foreach (GameLocation location in locations)
            {
                if (location.objects.Values.OfType<Chest>().Any(p => p.playerChest))
                    yield return location;
            }
        }

        /// <summary>Get all machines in a given location.</summary>
        /// <param name="location">The location to search.</param>
        /// <param name="reflection">Simplifies access to private game code.</param>
        public IEnumerable<MachineMetadata> GetMachinesIn(GameLocation location, IReflectionHelper reflection)
        {
            foreach (KeyValuePair<Vector2, SObject> pair in location.objects)
            {
                Vector2 tile = pair.Key;
                SObject obj = pair.Value;
                if (pair.Value == null)
                    continue;

                IMachine machine = this.GetMachine(obj);
                if (machine != null)
                    yield return new MachineMetadata(location, tile, machine);
            }
        }


        /*********
        ** Private methods
        *********/
        /// <param name="obj">The object for which to get a machine.</param>
        private IMachine GetMachine(SObject obj)
        {
            if (obj.name == "Keg")
                return new KegMachine(obj);

            return null;
        }
    }
}
