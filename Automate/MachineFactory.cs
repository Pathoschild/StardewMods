using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Pathoschild.Stardew.Automate.Framework;
using Pathoschild.Stardew.Automate.Machines.Buildings;
using Pathoschild.Stardew.Automate.Machines.Objects;
using Pathoschild.Stardew.Automate.Machines.TerrainFeatures;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Buildings;
using StardewValley.Locations;
using StardewValley.Objects;
using StardewValley.TerrainFeatures;
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
            // object machines
            foreach (KeyValuePair<Vector2, SObject> pair in location.objects)
            {
                Vector2 tile = pair.Key;
                SObject obj = pair.Value;

                IMachine machine = this.GetMachine(obj, location, tile, reflection);
                if (machine != null)
                {
                    Chest[] chests = this.GetConnected(location, tile).ToArray();
                    if (chests.Any())
                        yield return new MachineMetadata(location, chests, machine);
                }
            }

            // terrain feature machines
            foreach (KeyValuePair<Vector2, TerrainFeature> pair in location.terrainFeatures)
            {
                Vector2 tile = pair.Key;
                TerrainFeature feature = pair.Value;

                IMachine machine = this.GetMachine(feature);
                if (machine != null)
                {
                    Chest[] chests = this.GetConnected(location, tile).ToArray();
                    if (chests.Any())
                        yield return new MachineMetadata(location, chests, machine);
                }
            }

            // building machines
            if (location is BuildableGameLocation buildableLocation)
            {
                foreach (Building building in buildableLocation.buildings)
                {
                    IMachine machine = this.GetMachine(building);
                    if (machine != null)
                    {
                        Rectangle area = new Rectangle(building.tileX, building.tileY, building.tilesWide, building.tilesHigh);
                        Chest[] chests = this.GetConnected(location, area).ToArray();
                        if (chests.Any())
                            yield return new MachineMetadata(location, chests, machine);
                    }
                }
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

        /// <summary>Get a machine for the given terrain feature, if applicable.</summary>
        /// <param name="feature">The terrain feature for which to get a machine.</param>
        private IMachine GetMachine(TerrainFeature feature)
        {
            if (feature is FruitTree fruitTree)
                return new FruitTreeMachine(fruitTree);

            return null;
        }

        /// <summary>Get a machine for the given building, if applicable.</summary>
        /// <param name="building">The building for which to get a machine.</param>
        private IMachine GetMachine(Building building)
        {
            if (building is Mill mill)
                return new MillMachine(mill);

            return null;
        }

        /// <summary>Get all chests connected to the given machine.</summary>
        /// <param name="location">The location to search.</param>
        /// <param name="tile">The tile position for which to find connected tiles.</param>
        private IEnumerable<Chest> GetConnected(GameLocation location, Vector2 tile)
        {
            foreach (Vector2 connectedTile in Utility.getSurroundingTileLocationsArray(tile))
            {
                if (this.TryGetChest(location, connectedTile, out Chest chest))
                    yield return chest;
            }
        }

        /// <summary>Get all chests connected to the given machine.</summary>
        /// <param name="location">The location to search.</param>
        /// <param name="area">The tile area for which to find connected tiles.</param>
        private IEnumerable<Chest> GetConnected(GameLocation location, Rectangle area)
        {
            // get surrounding corner
            int left = area.X - 1;
            int top = area.Y - 1;
            int right = left + area.Width + 1;
            int bottom = top + area.Height + 1;

            // get connected chests
            int i = 0;
            for (int x = left; x <= right; x++)
            {
                if (this.TryGetChest(location, new Vector2(x, top), out Chest chest))
                    yield return chest;
            }
            for (int y = top + 1; y <= bottom - 1; y++)
            {
                if (this.TryGetChest(location, new Vector2(left, y), out Chest leftChest))
                    yield return leftChest;
                if (this.TryGetChest(location, new Vector2(right, y), out Chest rightChest))
                    yield return rightChest;
            }
            for (int x = left; x <= right; x++)
            {
                if (this.TryGetChest(location, new Vector2(x, bottom), out Chest chest))
                    yield return chest;
            }
        }

        /// <summary>Get the chest on a given tile, if any.</summary>
        /// <param name="location">The location to search.</param>
        /// <param name="tile">The tile to search.</param>
        /// <param name="chest">The chest found on the tile.</param>
        private bool TryGetChest(GameLocation location, Vector2 tile, out Chest chest)
        {
            if (location.objects.TryGetValue(tile, out SObject obj))
            {
                chest = obj as Chest;
                return chest != null;
            }

            chest = null;
            return false;
        }
    }
}
