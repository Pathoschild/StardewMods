using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Pathoschild.Stardew.Automate.Framework.Models;
using Pathoschild.Stardew.Automate.Framework.Storage;
using Pathoschild.Stardew.Common;
using StardewValley;
using StardewValley.Buildings;
using StardewValley.Locations;
using StardewValley.Objects;
using StardewValley.TerrainFeatures;
using SObject = StardewValley.Object;

namespace Pathoschild.Stardew.Automate.Framework
{
    /// <summary>Constructs machine groups.</summary>
    internal class MachineGroupFactory
    {
        /*********
        ** Fields
        *********/
        /// <summary>The automation factories which construct machines, containers, and connectors.</summary>
        private readonly IList<IAutomationFactory> AutomationFactories = new List<IAutomationFactory>();

        /// <summary>Get the configuration for specific machines by ID, if any.</summary>
        private readonly Func<string, ModConfigMachine?> GetMachineOverride;

        /// <summary>Build a storage manager for the given containers.</summary>
        private readonly Func<IContainer[], StorageManager> BuildStorage;


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="getMachineOverride">Get the configuration for specific machines by ID, if any.</param>
        /// <param name="buildStorage">Build a storage manager for the given containers.</param>
        public MachineGroupFactory(Func<string, ModConfigMachine?> getMachineOverride, Func<IContainer[], StorageManager> buildStorage)
        {
            this.GetMachineOverride = getMachineOverride;
            this.BuildStorage = buildStorage;
        }

        /// <summary>Add an automation factory.</summary>
        /// <param name="factory">An automation factory which construct machines, containers, and connectors.</param>
        public void Add(IAutomationFactory factory)
        {
            this.AutomationFactories.Add(factory);
        }

        /// <summary>Get the unique key which identifies a location.</summary>
        /// <param name="location">The location instance.</param>
        public string GetLocationKey(GameLocation location)
        {
            return location.uniqueName.Value != null && location.uniqueName.Value != location.Name
                ? $"{location.Name} ({location.uniqueName.Value})"
                : location.Name;
        }

        /// <summary>Sort machines by priority.</summary>
        /// <param name="machines">The machines to sort.</param>
        public IEnumerable<IMachine> SortMachines(IEnumerable<IMachine> machines)
        {
            return
                (
                    from machine in machines
                    let config = this.GetMachineOverride(machine.MachineTypeID)
                    orderby config?.Priority ?? 0 descending
                    select machine
                );
        }

        /// <summary>Get all machine groups in a location.</summary>
        /// <param name="location">The location to search.</param>
        public IEnumerable<IMachineGroup> GetMachineGroups(GameLocation location)
        {
            MachineGroupBuilder builder = new(this.GetLocationKey(location), this.SortMachines, this.BuildStorage);
            LocationFloodFillIndex locationIndex = new(location);
            ISet<Vector2> visited = new HashSet<Vector2>();
            foreach (Vector2 tile in location.GetTiles())
            {
                this.FloodFillGroup(builder, location, tile, locationIndex, visited);
                if (builder.HasTiles())
                {
                    yield return builder.Build();
                    builder.Reset();
                }
            }
        }

        /// <summary>Get whether an object is automatable.</summary>
        /// <param name="location">The location to check.</param>
        /// <param name="tile">The tile to check.</param>
        /// <param name="obj">The object to check.</param>
        public bool IsAutomatable(GameLocation location, Vector2 tile, SObject obj)
        {
            return this.GetEntityFor(location, tile, obj) != null;
        }

        /// <summary>Get whether a terrain feature is automatable.</summary>
        /// <param name="location">The location to check.</param>
        /// <param name="tile">The tile to check.</param>
        /// <param name="terrainFeature">The terrain feature to check.</param>
        public bool IsAutomatable(GameLocation location, Vector2 tile, TerrainFeature terrainFeature)
        {
            return this.GetEntityFor(location, tile, terrainFeature) != null;
        }

        /// <summary>Get whether a building is automatable.</summary>
        /// <param name="location">The location to check.</param>
        /// <param name="tile">The tile to check.</param>
        /// <param name="building">The building to check.</param>
        public bool IsAutomatable(BuildableGameLocation location, Vector2 tile, Building building)
        {
            return this.GetEntityFor(location, tile, building) != null;
        }

        /// <summary>Get the registered automation factories.</summary>
        public IEnumerable<IAutomationFactory> GetFactories()
        {
            return this.AutomationFactories.Select(p => p);
        }


        /*********
        ** Private methods
        *********/
        /// <summary>Extend the given machine group to include all machines and containers connected to the given tile, if any.</summary>
        /// <param name="machineGroup">The machine group to extend.</param>
        /// <param name="location">The location to search.</param>
        /// <param name="origin">The first tile to check.</param>
        /// <param name="locationIndex">An indexed view of the location.</param>
        /// <param name="visited">A lookup of visited tiles.</param>
        private void FloodFillGroup(MachineGroupBuilder machineGroup, GameLocation location, in Vector2 origin, LocationFloodFillIndex locationIndex, ISet<Vector2> visited)
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

                // add machines, containers, or connectors which covers this tile
                if (this.TryAddEntity(machineGroup, location, locationIndex, tile))
                {
                    foreach (Rectangle tileArea in machineGroup.NewTileAreas)
                    {
                        // mark visited
                        foreach (Vector2 cur in tileArea.GetTiles())
                            visited.Add(cur);

                        // connect entities on surrounding tiles
                        foreach (Vector2 next in tileArea.GetSurroundingTiles())
                        {
                            if (!visited.Contains(next))
                                queue.Enqueue(next);
                        }
                    }
                    machineGroup.NewTileAreas.Clear();
                }
            }
        }

        /// <summary>Add any machine, container, or connector on the given tile to the machine group.</summary>
        /// <param name="group">The machine group to extend.</param>
        /// <param name="location">The location to search.</param>
        /// <param name="locationIndex">An indexed view of the location.</param>
        /// <param name="tile">The tile to search.</param>
        private bool TryAddEntity(MachineGroupBuilder group, GameLocation location, LocationFloodFillIndex locationIndex, in Vector2 tile)
        {
            IAutomatable? entity = this.GetEntity(location, locationIndex, tile);
            switch (entity)
            {
                case null:
                    return false;

                case IMachine machine:
                    if (this.GetMachineOverride(machine.MachineTypeID)?.Enabled != false)
                        group.Add(machine);
                    return true;

                case IContainer container:
                    if (container.StorageAllowed() || container.TakingItemsAllowed())
                    {
                        group.Add(container);
                        return true;
                    }
                    return false;

                default:
                    group.Add(entity.TileArea); // connector
                    return true;
            }
        }

        /// <summary>Get a machine, container, or connector from the given tile, if any.</summary>
        /// <param name="location">The location to search.</param>
        /// <param name="locationIndex">An indexed view of the location.</param>
        /// <param name="tile">The tile to search.</param>
        private IAutomatable? GetEntity(GameLocation location, LocationFloodFillIndex locationIndex, Vector2 tile)
        {
            // from entity
            foreach (object target in locationIndex.GetEntities(tile))
            {
                switch (target)
                {
                    case SObject obj:
                        {
                            IAutomatable? entity = this.GetEntityFor(location, tile, obj);
                            if (entity != null)
                                return entity;

                            if (obj is IndoorPot pot && pot.bush.Value != null)
                            {
                                entity = this.GetEntityFor(location, tile, pot.bush.Value);
                                if (entity != null)
                                    return entity;
                            }
                        }
                        break;

                    case TerrainFeature feature:
                        {
                            IAutomatable? entity = this.GetEntityFor(location, tile, feature);
                            if (entity != null)
                                return entity;
                        }
                        break;

                    case Building building:
                        {
                            IAutomatable? entity = this.GetEntityFor((BuildableGameLocation)location, tile, building);
                            if (entity != null)
                                return entity;
                        }
                        break;
                }
            }

            // from tile position
            foreach (IAutomationFactory factory in this.AutomationFactories)
            {
                IAutomatable? entity = factory.GetForTile(location, tile);
                if (entity != null)
                    return entity;
            }

            // none found
            return null;
        }

        /// <summary>Get a machine, container, or connector from the given object, if any.</summary>
        /// <param name="location">The location to search.</param>
        /// <param name="tile">The tile to search.</param>
        /// <param name="obj">The object to check.</param>
        private IAutomatable? GetEntityFor(GameLocation location, Vector2 tile, SObject obj)
        {
            foreach (IAutomationFactory factory in this.AutomationFactories)
            {
                IAutomatable? entity = factory.GetFor(obj, location, tile);
                if (entity != null)
                    return entity;
            }

            return null;
        }

        /// <summary>Get a machine, container, or connector from the given terrain feature, if any.</summary>
        /// <param name="location">The location to search.</param>
        /// <param name="tile">The tile to search.</param>
        /// <param name="feature">The terrain feature to check.</param>
        private IAutomatable? GetEntityFor(GameLocation location, Vector2 tile, TerrainFeature feature)
        {
            foreach (IAutomationFactory factory in this.AutomationFactories)
            {
                IAutomatable? entity = factory.GetFor(feature, location, tile);
                if (entity != null)
                    return entity;
            }

            return null;
        }

        /// <summary>Get a machine, container, or connector from the given building, if any.</summary>
        /// <param name="location">The location to search.</param>
        /// <param name="tile">The tile to search.</param>
        /// <param name="building">The building to check.</param>
        private IAutomatable? GetEntityFor(BuildableGameLocation location, Vector2 tile, Building building)
        {
            foreach (IAutomationFactory factory in this.AutomationFactories)
            {
                IAutomatable? entity = factory.GetFor(building, location, tile);
                if (entity != null)
                    return entity;
            }

            return null;
        }
    }
}
