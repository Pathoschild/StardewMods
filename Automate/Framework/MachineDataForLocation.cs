using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Pathoschild.Stardew.Common;

namespace Pathoschild.Stardew.Automate.Framework
{
    /// <summary>The automation data tracked for a location.</summary>
    /// <param name="LocationKey">The location key as formatted by <see cref="MachineGroupFactory.GetLocationKey"/>.</param>
    /// <param name="ActiveMachineGroups">The machines to process.</param>
    /// <param name="DisabledMachineGroups">The disabled machine groups (e.g. machines not connected to a chest).</param>
    internal record MachineDataForLocation(string LocationKey, IReadOnlyCollection<IMachineGroup> ActiveMachineGroups, IReadOnlyCollection<IMachineGroup> DisabledMachineGroups)
    {
        /*********
        ** Fields
        *********/
        /// <summary>The backing field for <see cref="OutdatedTiles"/>.</summary>
        private readonly Dictionary<Vector2, IAutomatable> OutdatedTilesImpl = new();

        /// <summary>The backing field for <see cref="ActiveTiles"/>.</summary>
        private readonly Lazy<Dictionary<Vector2, IMachineGroup>> ActiveTilesImpl = new(() => GetTileLookup(ActiveMachineGroups));

        /// <summary>The backing field for <see cref="DisabledTiles"/>.</summary>
        private readonly Lazy<Dictionary<Vector2, IMachineGroup>> DisabledTilesImpl = new(() => GetTileLookup(DisabledMachineGroups));


        /*********
        ** Accessors
        *********/
        /// <summary>The tiles which contain an active machine group.</summary>
        public IReadOnlyDictionary<Vector2, IMachineGroup> ActiveTiles => this.ActiveTilesImpl.Value;

        /// <summary>The tiles which contain an inactive machine group.</summary>
        public IReadOnlyDictionary<Vector2, IMachineGroup> DisabledTiles => this.DisabledTilesImpl.Value;

        /// <summary>The tiles containing an automatable which isn't part of a machine group because it was added after the last scan.</summary>
        public IReadOnlyDictionary<Vector2, IAutomatable> OutdatedTiles => this.OutdatedTilesImpl;


        /*********
        ** Public methods
        *********/
        /// <summary>Get whether the given area contains a tracked automateable.</summary>
        /// <param name="tileArea">The tile area to check.</param>
        /// <param name="trackedOnly">Whether to ignore outdated tiles.</param>
        public bool Contains(Rectangle tileArea, bool trackedOnly)
        {
            var activeTiles = this.ActiveTiles;
            var disabledTiles = this.DisabledTiles;
            var outdatedTiles = this.OutdatedTiles;

            foreach (Vector2 tile in tileArea.GetTiles())
            {
                if (activeTiles.ContainsKey(tile) || disabledTiles.ContainsKey(tile))
                    return true;

                if (!trackedOnly && outdatedTiles.ContainsKey(tile))
                    return true;
            }

            return false;
        }

        /// <summary>Get whether a tile area contains or is adjacent to a tracked automateable.</summary>
        /// <param name="tileArea">The tile area to check.</param>
        public bool ContainsOrAdjacent(Rectangle tileArea)
        {
            var activeTiles = this.ActiveTiles;
            var disabledTiles = this.DisabledTiles;
            var outdatedTiles = this.OutdatedTiles;

            foreach (Vector2 tile in tileArea.GetSurroundingTiles())
            {
                if (activeTiles.ContainsKey(tile) || disabledTiles.ContainsKey(tile) || outdatedTiles.ContainsKey(tile))
                    return true;
            }

            return false;
        }

        /// <summary>Get whether a tile contains or is adjacent to a chest, or to a machine group containing a chest.</summary>
        /// <param name="tileArea">The tile to check.</param>
        public bool IsConnectedToChest(Rectangle tileArea)
        {
            var activeTiles = this.ActiveTiles;
            var disabledTiles = this.DisabledTiles;
            var outdatedTiles = this.OutdatedTiles;

            foreach (Vector2 tile in tileArea.GetSurroundingTiles())
            {
                if (activeTiles.ContainsKey(tile))
                    return true;

                if (disabledTiles.TryGetValue(tile, out IMachineGroup? group) && group.Containers.Length is not 0)
                    return true;

                if (outdatedTiles.TryGetValue(tile, out IAutomatable? automateable) && automateable is IContainer)
                    return true;
            }

            return false;
        }

        /// <summary>Mark a set of tiles as containing automateable entities which aren't currently tracked in <see cref="ActiveTiles"/> or <see cref="DisabledTiles"/>.</summary>
        /// <param name="tileArea">The tile area to mark.</param>
        /// <param name="entity">The entity on the tile.</param>
        public void MarkOutdated(Rectangle tileArea, IAutomatable entity)
        {
            var outdatedTiles = this.OutdatedTilesImpl;

            foreach (Vector2 tile in tileArea.GetTiles())
                outdatedTiles[tile] = entity;
        }


        /*********
        ** Private methods
        *********/
        /// <summary>Get a lookup of machine groups by the tile positions they contain.</summary>
        /// <param name="machineGroups">The machine group to index.</param>
        private static Dictionary<Vector2, IMachineGroup> GetTileLookup(IEnumerable<IMachineGroup> machineGroups)
        {
            return
                (
                    from machineGroup in machineGroups
                    from tile in machineGroup.Tiles
                    group machineGroup by tile into tileGroup
                    select tileGroup
                )
                .ToDictionary(p => p.Key, p => p.First());
        }
    };
}
