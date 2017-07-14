using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Pathoschild.Stardew.Automate.Framework;
using Pathoschild.Stardew.Automate.Machines.Buildings;
using Pathoschild.Stardew.Automate.Machines.Objects;
using Pathoschild.Stardew.Automate.Machines.TerrainFeatures;
using Pathoschild.Stardew.Automate.Machines.Tiles;
using Pathoschild.Stardew.Automate.Pipes;
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
    internal class MachineFactory
    {
        /*********
        ** Properties
        *********/
        /// <summary>The tile area on the farm matching the shipping bin.</summary>
        private readonly Rectangle ShippingBinArea = new Rectangle(71, 14, 2, 1);


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
                    IPipe[] pipes = this.GetConnected(location, tile).ToArray();
                    if (pipes.Any())
                        yield return new MachineMetadata(machine, location, pipes);
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
                    IPipe[] pipes = this.GetConnected(location, tile).ToArray();
                    if (pipes.Any())
                        yield return new MachineMetadata(machine, location, pipes);
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
                        IPipe[] pipes = this.GetConnected(location, area).ToArray();
                        if (pipes.Any())
                            yield return new MachineMetadata(machine, location, pipes);
                    }
                }
            }

            // map machines
            for (int x = 0; x < location.Map.Layers[0].LayerWidth; x++)
            {
                for (int y = 0; y < location.Map.Layers[0].LayerHeight; y++)
                {
                    Vector2 tile = new Vector2(x, y);
                    if (this.TryGetTileMachine(location, tile, reflection, out IMachine machine, out Vector2 size))
                    {
                        IPipe[] pipes = this.GetConnected(location, new Rectangle(x, y, (int)size.X, (int)size.Y)).ToArray();
                        if (pipes.Any())
                            yield return new MachineMetadata(machine, location, pipes);
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
            if (obj.name == "Feed Hopper")
                return new FeedHopperMachine();
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
            if (building is JunimoHut hut)
                return new JunimoHutMachine(hut);
            if (building is Mill mill)
                return new MillMachine(mill);
            if (building.buildingType == "Silo")
                return new FeedHopperMachine();

            return null;
        }

        /// <summary>Get a machine for the given tile, if applicable.</summary>
        /// <param name="location">The location containing the machine.</param>
        /// <param name="tile">The machine's position in its location.</param>
        /// <param name="reflection">Simplifies access to private game code.</param>
        /// <param name="machine">The generated machine instance.</param>
        /// <param name="size">The size of the machine on the map.</param>
        /// <remarks>Shipping bin logic from <see cref="Farm.leftClick"/>, garbage can logic from <see cref="Town.checkAction"/>.</remarks>
        private bool TryGetTileMachine(GameLocation location, Vector2 tile, IReflectionHelper reflection, out IMachine machine, out Vector2 size)
        {
            // shipping bin
            // ReSharper disable once ImpureMethodCallOnReadonlyValueField -- false positive, method is not impure
            if (location is Farm farm && this.ShippingBinArea.Contains((int)tile.X, (int)tile.Y))
            {
                machine = new ShippingBinMachine(farm);
                size = new Vector2(this.ShippingBinArea.Width, this.ShippingBinArea.Height);
                return true;
            }

            // garbage cans
            if (location is Town town)
            {
                string action = town.doesTileHaveProperty((int)tile.X, (int)tile.Y, "Action", "Buildings");
                if (!string.IsNullOrWhiteSpace(action) && action.StartsWith("Garbage ") && int.TryParse(action.Split(' ')[1], out int trashCanIndex))
                {
                    machine = new TrashCanMachine(town, tile, trashCanIndex, reflection);
                    size = Vector2.One;
                    return true;
                }
            }

            // none found
            machine = null;
            size = Vector2.Zero;
            return false;
        }

        /// <summary>Get all chests connected to the given machine.</summary>
        /// <param name="location">The location to search.</param>
        /// <param name="tile">The tile position for which to find connected tiles.</param>
        private IEnumerable<IPipe> GetConnected(GameLocation location, Vector2 tile)
        {
            foreach (Vector2 connectedTile in Utility.getSurroundingTileLocationsArray(tile))
            {
                if (this.TryGetChest(location, connectedTile, out Chest chest))
                    yield return new ChestPipe(chest);
            }
        }

        /// <summary>Get all chests connected to the given machine.</summary>
        /// <param name="location">The location to search.</param>
        /// <param name="area">The tile area for which to find connected tiles.</param>
        private IEnumerable<IPipe> GetConnected(GameLocation location, Rectangle area)
        {
            // get surrounding corner
            int left = area.X - 1;
            int top = area.Y - 1;
            int right = left + area.Width + 1;
            int bottom = top + area.Height + 1;

            // get connected chests
            for (int x = left; x <= right; x++)
            {
                if (this.TryGetChest(location, new Vector2(x, top), out Chest chest))
                    yield return new ChestPipe(chest);
            }
            for (int y = top + 1; y <= bottom - 1; y++)
            {
                if (this.TryGetChest(location, new Vector2(left, y), out Chest leftChest))
                    yield return new ChestPipe(leftChest);
                if (this.TryGetChest(location, new Vector2(right, y), out Chest rightChest))
                    yield return new ChestPipe(rightChest);
            }
            for (int x = left; x <= right; x++)
            {
                if (this.TryGetChest(location, new Vector2(x, bottom), out Chest chest))
                    yield return new ChestPipe(chest);
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
