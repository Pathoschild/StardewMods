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
        private void FloodFillGroup(MachineGroupBuilder group, GameLocation location, in Vector2 origin, ISet<Vector2> visited, IReflectionHelper reflection)
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

                // add machine, container, or connector which covers this tile
                if (this.TryAddEntity(group, location, tile, reflection, out Rectangle tileArea))
                {
                    group.Add(tileArea);
                    foreach (Vector2 cur in tileArea.GetTiles())
                        visited.Add(cur);
                }
                else
                    continue;

                // attach to entities in surrounding tiles
                foreach (Vector2 next in tileArea.GetSurroundingTiles())
                {
                    if (!visited.Contains(next))
                        queue.Enqueue(next);
                }
            }
        }

        /// <summary>Add any machine, container, or connector on the given tile to the machine group.</summary>
        /// <param name="group">The machine group to extend.</param>
        /// <param name="location">The location to search.</param>
        /// <param name="tile">The tile to search.</param>
        /// <param name="reflection">Simplifies access to private game code.</param>
        /// <param name="tileArea">The tile area covered by the matching machine, container, or connector.</param>
        private bool TryAddEntity(MachineGroupBuilder group, GameLocation location, in Vector2 tile, IReflectionHelper reflection, out Rectangle tileArea)
        {
            // machine
            IMachine machine = this.TryGetMachine(location, tile, reflection);
            if (machine != null)
            {
                group.Add(machine);
                tileArea = machine.TileArea;
                return true;
            }

            // container
            IContainer container = this.TryGetContainer(location, tile);
            if (container != null && container.Name?.IndexOf("|automate:ignore|", StringComparison.InvariantCultureIgnoreCase) == -1)
            {
                group.Add(container);
                tileArea = container.TileArea;
                return true;
            }

            // connector
            if (this.TryGetConnector(location, tile))
            {
                tileArea = new Rectangle((int)tile.X, (int)tile.Y, 1, 1);
                return true;
            }

            // none found
            tileArea = Rectangle.Empty;
            return false;
        }

        /// <summary>Get a machine from the given tile, if any.</summary>
        /// <param name="location">The location to search.</param>
        /// <param name="tile">The tile to search.</param>
        /// <param name="reflection">Simplifies access to private game code.</param>
        private IMachine TryGetMachine(GameLocation location, Vector2 tile, IReflectionHelper reflection)
        {
            // object machine
            if (location.objects.TryGetValue(tile, out SObject obj) && !(obj is Chest))
            {
                IMachine machine = this.GetMachine(obj, location, tile, reflection);
                if (machine != null)
                    return machine;
            }

            // terrain feature machine
            if (location.terrainFeatures.TryGetValue(tile, out TerrainFeature feature))
            {
                IMachine machine = this.GetMachine(feature, location, tile);
                if (machine != null)
                    return machine;
            }

            // building machine
            if (location is BuildableGameLocation buildableLocation)
            {
                foreach (Building building in buildableLocation.buildings)
                {
                    Rectangle tileArea = new Rectangle(building.tileX.Value, building.tileY.Value, building.tilesWide.Value, building.tilesHigh.Value);
                    if (tileArea.Contains((int)tile.X, (int)tile.Y))
                    {
                        IMachine machine = this.GetMachine(building, buildableLocation);
                        if (machine != null)
                            return machine;
                    }
                }
            }

            // map machines
            {
                IMachine machine = this.TryGetTileMachine(location, tile, reflection);
                if (machine != null)
                    return machine;
            }

            // none
            return null;
        }

        /// <param name="obj">The object for which to get a machine.</param>
        /// <param name="location">The location containing the machine.</param>
        /// <param name="tile">The machine's position in its location.</param>
        /// <param name="reflection">Simplifies access to private game code.</param>
        private IMachine GetMachine(SObject obj, GameLocation location, in Vector2 tile, IReflectionHelper reflection)
        {
            if (obj.ParentSheetIndex == 165)
                return new AutoGrabberMachine(obj, location);
            if (obj.name == "Bee House")
                return new BeeHouseMachine(obj, location);
            if (obj is Cask cask)
                return new CaskMachine(cask, location);
            if (obj.name == "Charcoal Kiln")
                return new CharcoalKilnMachine(obj, location);
            if (obj.name == "Cheese Press")
                return new CheesePressMachine(obj, location);
            if (obj is CrabPot pot)
                return new CrabPotMachine(pot, location, reflection);
            if (obj.Name == "Crystalarium")
                return new CrystalariumMachine(obj, location, reflection);
            if (obj.name == "Feed Hopper")
                return new FeedHopperMachine(obj, location);
            if (obj.Name == "Furnace")
                return new FurnaceMachine(obj, location);
            if (obj.name == "Incubator")
                return new CoopIncubatorMachine(obj, location);
            if (obj.Name == "Keg")
                return new KegMachine(obj, location);
            if (obj.name == "Lightning Rod")
                return new LightningRodMachine(obj, location);
            if (obj.name == "Loom")
                return new LoomMachine(obj, location);
            if (obj.name == "Mayonnaise Machine")
                return new MayonnaiseMachine(obj, location);
            if (obj.Name == "Mushroom Box")
                return new MushroomBoxMachine(obj, location);
            if (obj.name == "Oil Maker")
                return new OilMakerMachine(obj, location);
            if (obj.name == "Preserves Jar")
                return new PreservesJarMachine(obj, location);
            if (obj.name == "Recycling Machine")
                return new RecyclingMachine(obj, location);
            if (obj.name == "Seed Maker")
                return new SeedMakerMachine(obj, location);
            if (obj.name == "Slime Egg-Press")
                return new SlimeEggPressMachine(obj, location);
            if (obj.name == "Slime Incubator")
                return new SlimeIncubatorMachine(obj, location);
            if (obj.name == "Soda Machine")
                return new SodaMachine(obj, location);
            if (obj.name == "Statue Of Endless Fortune")
                return new StatueOfEndlessFortuneMachine(obj, location);
            if (obj.name == "Statue Of Perfection")
                return new StatueOfPerfectionMachine(obj, location);
            if (obj.name == "Tapper")
            {
                if (location.terrainFeatures.TryGetValue(tile, out TerrainFeature terrainFeature) && terrainFeature is Tree tree)
                    return new TapperMachine(obj, location, tree.treeType.Value);
            }
            if (obj.name == "Worm Bin")
                return new WormBinMachine(obj, location);
            return null;
        }

        /// <summary>Get a machine for the given terrain feature, if applicable.</summary>
        /// <param name="feature">The terrain feature for which to get a machine.</param>
        /// <param name="location">The location containing the machine.</param>
        /// <param name="tile">The machine's position in its location.</param>
        private IMachine GetMachine(TerrainFeature feature, GameLocation location, in Vector2 tile)
        {
            if (feature is FruitTree fruitTree)
                return new FruitTreeMachine(fruitTree, location, tile);
            return null;
        }

        /// <summary>Get a machine for the given building, if applicable.</summary>
        /// <param name="building">The building for which to get a machine.</param>
        /// <param name="location">The location containing the machine.</param>
        private IMachine GetMachine(Building building, BuildableGameLocation location)
        {
            if (building is JunimoHut hut)
                return new JunimoHutMachine(hut, location);
            if (building is Mill mill)
                return new MillMachine(mill, location);
            if (this.AutomateShippingBin && building is ShippingBin bin)
                return new ShippingBinMachine(bin, location, Game1.getFarm());
            if (building.buildingType.Value == "Silo")
                return new FeedHopperMachine(building, location);
            return null;
        }

        /// <summary>Get a machine for the given tile, if applicable.</summary>
        /// <param name="location">The location containing the machine.</param>
        /// <param name="tile">The machine's position in its location.</param>
        /// <param name="reflection">Simplifies access to private game code.</param>
        /// <remarks>Shipping bin logic from <see cref="Farm.leftClick"/>, garbage can logic from <see cref="Town.checkAction"/>.</remarks>
        private IMachine TryGetTileMachine(GameLocation location, in Vector2 tile, IReflectionHelper reflection)
        {
            // shipping bin
            if (this.AutomateShippingBin && location is Farm farm && (int)tile.X == this.ShippingBinArea.X && (int)tile.Y == this.ShippingBinArea.Y)
            {
                return new ShippingBinMachine(farm, this.ShippingBinArea);
            }

            // garbage cans
            if (location is Town town)
            {
                string action = town.doesTileHaveProperty((int)tile.X, (int)tile.Y, "Action", "Buildings");
                if (!string.IsNullOrWhiteSpace(action) && action.StartsWith("Garbage ") && int.TryParse(action.Split(' ')[1], out int trashCanIndex))
                    return new TrashCanMachine(town, tile, trashCanIndex, reflection);
            }

            // none found
            return null;
        }

        /// <summary>Get the container on a given tile, if any.</summary>
        /// <param name="location">The location to search.</param>
        /// <param name="tile">The tile to search.</param>
        private IContainer TryGetContainer(GameLocation location, in Vector2 tile)
        {
            if (location.objects.TryGetValue(tile, out SObject obj) && obj is Chest chest)
                return new ChestContainer(chest, location, tile);

            return null;
        }

        /// <summary>Get a connector from the given tile, if any.</summary>
        /// <param name="location">The location to search.</param>
        /// <param name="tile">The tile to search.</param>
        private bool TryGetConnector(GameLocation location, in Vector2 tile)
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
