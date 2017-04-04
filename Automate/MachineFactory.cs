using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Pathoschild.Stardew.Automate.Framework;
using Pathoschild.Stardew.Automate.Machines.Objects;
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

                IMachine machine = this.GetMachine(obj, location, tile, reflection);
                if (machine != null)
                    yield return new MachineMetadata(location, tile, machine);
            }
        }


        /*********
        ** Private methods
        *********/
        /// <param name="obj">The object for which to get a machine.</param>
        /// <param name="location">The location containing the machine.</param>
        /// <param name="tile">The machine's position in its location.</param>
        /// <param name="reflection">Simplifies access to private game code.</param>
        private IMachine GetMachine(SObject obj, GameLocation location, Vector2 tile, IReflectionHelper reflection)
        {
            if (obj.name == "Bee House")
                return new BeeHouseMachine(obj, location, tile);
            if (obj is Cask cask)
                return new CaskMachine(cask);
            if (obj.name == "Charcoal Kiln")
                return new CharcoalKilnMachine(obj);
            if (obj.name == "Cheese Press")
                return new CheesePressMachine(obj);
            if (obj is CrabPot pot)
                return new CrabPotMachine(pot, reflection);
            if (obj.Name == "Crystalarium")
                return new CrystalariumMachine(obj, reflection);
            if (obj.Name == "Furnace")
                return new FurnaceMachine(obj, tile);
            if (obj.Name == "Keg")
                return new KegMachine(obj);
            if (obj.name == "Lightning Rod")
                return new LightningRodMachine(obj);
            if (obj.name == "Loom")
                return new LoomMachine(obj);
            if (obj.name == "Mayonnaise Machine")
                return new MayonnaiseMachine(obj);
            if (obj.Name == "Mushroom Box")
                return new MushroomBoxMachine(obj);
            if (obj.name == "Oil Maker")
                return new OilMakerMachine(obj);
            if (obj.name == "Preserves Jar")
                return new PreservesJarMachine(obj);
            if (obj.name == "Recycling Machine")
                return new RecyclingMachine(obj);
            if (obj.name == "Seed Maker")
                return new SeedMakerMachine(obj);
            if (obj.name == "Slime Egg-Press")
                return new SlimeEggPressMachine(obj);
            if (obj.name == "Soda Machine")
                return new SodaMachine(obj);
            if (obj.name == "Statue Of Endless Fortune")
                return new StatueOfEndlessFortuneMachine(obj);
            if (obj.name == "Statue Of Perfection")
                return new StatueOfPerfectionMachine(obj);
            if (obj.name == "Tapper")
                return new TapperMachine(obj, location, tile);
            if (obj.name == "Worm Bin")
                return new WormBinMachine(obj);

            return null;
        }
    }
}
