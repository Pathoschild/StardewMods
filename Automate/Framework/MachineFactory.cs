using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Pathoschild.Stardew.Automate.Framework.Machines.Buildings;
using Pathoschild.Stardew.Automate.Framework.Machines.Objects;
using Pathoschild.Stardew.Automate.Framework.Machines.TerrainFeatures;
using Pathoschild.Stardew.Automate.Framework.Machines.Tiles;
using Pathoschild.Stardew.Automate.Framework.Models;
using Pathoschild.Stardew.Automate.Framework.Storage;
using Pathoschild.Stardew.Common;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Buildings;
using StardewValley.Locations;
using StardewValley.Objects;
using StardewValley.TerrainFeatures;
using SObject = StardewValley.Object;

namespace Pathoschild.Stardew.Automate.Framework
{
    /// <summary>Constructs machine instances.</summary>
    internal class MachineFactory
    {
        /*********
        ** Properties
        *********/
        /// <summary>The object IDs through which machines can connect, but which have no other automation properties.</summary>
        private readonly IDictionary<ObjectType, HashSet<int>> Connectors;

        /// <summary>Whether to treat the shipping bin as a machine that can be automated.</summary>
        private readonly bool AutomateShippingBin;

        /// <summary>The tile area on the farm matching the shipping bin.</summary>
        private readonly Rectangle ShippingBinArea = new Rectangle(71, 14, 2, 1);


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="connectors">The objects through which machines can connect, but which have no other automation properties.</param>
        /// <param name="automateShippingBin">Whether to treat the shipping bin as a machine that can be automated.</param>
        public MachineFactory(ModConfigObject[] connectors, bool automateShippingBin)
        {
            this.Connectors = connectors
                .GroupBy(connector => connector.Type)
                .ToDictionary(group => group.Key, group => new HashSet<int>(group.Select(p => p.ID)));
            this.AutomateShippingBin = automateShippingBin;
        }

        /// <summary>Get all machine groups in a location.</summary>
        /// <param name="location">The location to search.</param>
        /// <param name="reflection">Simplifies access to private game code.</param>
        public IEnumerable<MachineGroup> GetMachineGroups(GameLocation location, IReflectionHelper reflection)
        {
            MachineGroupBuilder builder = new MachineGroupBuilder(location);
            ISet<Vector2> visited = new HashSet<Vector2>();
            foreach (Vector2 tile in location.GetTiles())
            {
                this.FloodFillGroup(builder, location, tile, visited, reflection);
                if (builder.HasTiles())
                {
                    yield return builder.Build();
                    builder.Reset();
                }
            }
        }

        /// <summary>Get machines with connections in a location.</summary>
        /// <param name="location">The location to search.</param>
        /// <param name="reflection">Simplifies access to private game code.</param>
        public IEnumerable<MachineGroup> GetActiveMachinesGroups(GameLocation location, IReflectionHelper reflection)
        {
            return this
                .GetMachineGroups(location, reflection)
                .Where(group => group.HasInternalAutomation);
        }


        /*********
        ** Private methods
        *********/
        /// <summary>Extend the given machine group to include all machines and containers connected to the given tile, if any.</summary>
        /// <param name="group">The machine group to extend.</param>
        /// <param name="location">The location to search.</param>
        /// <param name="origin">The first tile to check.</param>
        /// <param name="visited">A lookup of visited tiles.</param>
        /// <param name="reflection">Simplifies access to private game code.</param>
        private void FloodFillGroup(MachineGroupBuilder group, GameLocation location, Vector2 origin, ISet<Vector2> visited, IReflectionHelper reflection)
        {
            // skip if already visited
            if (visited.Contains(origin))
                return;

            // flood-fill connected machines & containers
            Queue<Vector2> queue = new Queue<Vector2>();
            queue.Enqueue(origin);
            while (queue.Any())
            {
                // get tile
                Vector2 tile = queue.Dequeue();
                if (!visited.Add(tile))
                    continue;

                // check for a machine, container, or connector
                Vector2 foundOrigin = tile;
                Vector2 foundSize;
                if (this.TryGetMachine(location, tile, reflection, out IMachine machine, out foundOrigin, out Vector2 size))
                {
                    group.Add(machine);
                    foundSize = size;
                }
                else if (this.TryGetChest(location, tile, out Chest chest) && chest.Name.IndexOf("|automate:ignore|", StringComparison.InvariantCultureIgnoreCase) == -1)
                {
                    group.Add(new ChestContainer(chest));
                    foundSize = Vector2.One;
                }
                else if (this.TryGetConnector(location, tile))
                    foundSize = Vector2.One;
                else
                    continue;

                // mark tiles visited
                Rectangle tileArea = new Rectangle((int)foundOrigin.X, (int)foundOrigin.Y, (int)foundSize.X, (int)foundSize.Y);
                group.Add(tileArea);
                foreach (Vector2 cur in tileArea.GetTiles())
                    visited.Add(cur);

                // check surrounding tiles
                foreach (Vector2 next in tileArea.GetSurroundingTiles())
                {
                    if (!visited.Contains(next))
                        queue.Enqueue(next);
                }
            }
        }

        /// <summary>Get a machine from the given tile, if any.</summary>
        /// <param name="location">The location to search.</param>
        /// <param name="tile">The tile to search.</param>
        /// <param name="reflection">Simplifies access to private game code.</param>
        /// <param name="machine">The machine found on the tile.</param>
        /// <param name="origin">The machine's top-left position. This may be different from <paramref name="tile"/> for machines that take up multiple tiles.</param>
        /// <param name="size">The tile size of the machine found on the tile.</param>
        /// <returns>Returns whether a machine was found on the tile.</returns>
        private bool TryGetMachine(GameLocation location, Vector2 tile, IReflectionHelper reflection, out IMachine machine, out Vector2 origin, out Vector2 size)
        {
            origin = tile;

            // object machine
            if (location.objects.TryGetValue(tile, out SObject obj) && !(obj is Chest))
            {
                machine = this.GetMachine(obj, location, tile, reflection);
                if (machine != null)
                {
                    size = Vector2.One;
                    return true;
                }
            }

            // terrain feature machine
            if (location.terrainFeatures.TryGetValue(tile, out TerrainFeature feature))
            {
                machine = this.GetMachine(feature);
                if (machine != null)
                {
                    size = Vector2.One;
                    return true;
                }
            }

            // building machine
            if (location is BuildableGameLocation buildableLocation)
            {
                foreach (Building building in buildableLocation.buildings)
                {
                    Rectangle tileArea = new Rectangle(building.tileX.Value, building.tileY.Value, building.tilesWide.Value, building.tilesHigh.Value);
                    if (tileArea.Contains((int)tile.X, (int)tile.Y))
                    {
                        machine = this.GetMachine(building, buildableLocation);
                        if (machine != null)
                        {
                            origin = new Vector2(building.tileX.Value, building.tileY.Value);
                            size = new Vector2(building.tilesWide.Value, building.tilesHigh.Value);
                            return true;
                        }
                    }
                }
            }

            // map machines
            if (this.TryGetTileMachine(location, tile, reflection, out machine, out size))
                return true;

            // none
            size = Vector2.Zero;
            machine = null;
            return false;
        }

        /// <param name="obj">The object for which to get a machine.</param>
        /// <param name="location">The location containing the machine.</param>
        /// <param name="tile">The machine's position in its location.</param>
        /// <param name="reflection">Simplifies access to private game code.</param>
        private IMachine GetMachine(SObject obj, GameLocation location, Vector2 tile, IReflectionHelper reflection)
        {
            if (obj.ParentSheetIndex == 165)
                return new AutoGrabberMachine(obj);
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
            if (obj.name == "Incubator")
                return new CoopIncubatorMachine(obj);
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
            if (obj.name == "Slime Incubator")
                return new SlimeIncubatorMachine(obj);
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
        /// <param name="location">The location containing the machine.</param>
        private IMachine GetMachine(Building building, BuildableGameLocation location)
        {
            if (building is JunimoHut hut)
                return new JunimoHutMachine(hut);
            if (building is Mill mill)
                return new MillMachine(mill);
            if (this.AutomateShippingBin && location is Farm farm && building is ShippingBin)
                return new ShippingBinMachine(farm);
            if (building.buildingType.Value == "Silo")
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
            if (this.AutomateShippingBin && location is Farm farm && (int)tile.X == this.ShippingBinArea.X && (int)tile.Y == this.ShippingBinArea.Y)
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

        /// <summary>Get a connector from the given tile, if any.</summary>
        /// <param name="location">The location to search.</param>
        /// <param name="tile">The tile to search.</param>
        private bool TryGetConnector(GameLocation location, Vector2 tile)
        {
            // no connectors
            if (this.Connectors.Count == 0)
                return false;

            // check for possible connectors
            if (location.Objects.TryGetValue(tile, out SObject obj))
                return this.IsConnector(obj.bigCraftable.Value ? ObjectType.BigCraftable : ObjectType.Object, this.GetItemID(obj));
            if (location.terrainFeatures.TryGetValue(tile, out TerrainFeature terrainFeature) && terrainFeature is Flooring floor)
                return this.IsConnector(ObjectType.Floor, floor.whichFloor.Value);
            return false;
        }

        /// <summary>Get whether a given object should be treated as a connector.</summary>
        /// <param name="type">The object type.</param>
        /// <param name="id">The object iD.</param>
        private bool IsConnector(ObjectType type, int id)
        {
            if (this.Connectors.Count == 0)
                return false;

            return this.Connectors.TryGetValue(type, out HashSet<int> ids) && ids.Contains(id);
        }

        /// <summary>Get the object ID for a given object.</summary>
        /// <param name="obj">The object instance.</param>
        private int GetItemID(SObject obj)
        {
            // get object ID from fence ID
            if (obj is Fence fence)
            {
                switch (fence.whichType.Value)
                {
                    case Fence.wood:
                        return 322;
                    case Fence.stone:
                        return 323;
                    case Fence.steel:
                        return 324;
                    case Fence.gold:
                        return 298;
                }
            }

            // else obj ID
            return obj.ParentSheetIndex;
        }
    }
}
