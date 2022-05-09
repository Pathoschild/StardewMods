using System;
using Microsoft.Xna.Framework;
using StardewValley;
using StardewValley.Buildings;

namespace Pathoschild.Stardew.Automate.Framework
{
    /// <summary>The base implementation for a machine.</summary>
    internal abstract class BaseMachine : IMachine
    {
        /*********
        ** Accessors
        *********/
        /// <summary>A unique ID for the machine type.</summary>
        /// <remarks>This value should be identical for two machines if they have the exact same behavior and input logic. For example, if one machine in a group can't process input due to missing items, Automate will skip any other empty machines of that type in the same group since it assumes they need the same inputs.</remarks>
        public string MachineTypeID { get; protected set; }

        /// <summary>The location which contains the machine.</summary>
        public GameLocation Location { get; }

        /// <summary>The tile area covered by the machine.</summary>
        public Rectangle TileArea { get; }


        /*********
        ** Public methods
        *********/
        /// <summary>Get the machine's processing state.</summary>
        public abstract MachineState GetState();

        /// <summary>Get the output item.</summary>
        public abstract ITrackedStack? GetOutput();

        /// <summary>Provide input to the machine.</summary>
        /// <param name="input">The available items.</param>
        /// <returns>Returns whether the machine started processing an item.</returns>
        public abstract bool SetInput(IStorage input);

        /// <summary>Get the default ID for an Automate machine type.</summary>
        internal static string GetDefaultMachineId(Type machineType)
        {
            string id = machineType.Name;
            if (id.EndsWith("Machine"))
                id = id.Substring(0, id.Length - "Machine".Length);

            return id;
        }

        /*********
        ** Protected methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="location">The machine's in-game location.</param>
        /// <param name="tileArea">The tile area covered by the machine.</param>
        /// <param name="machineTypeId">A unique ID for the machine type, or <c>null</c> to generate it from the type name.</param>
        protected BaseMachine(GameLocation location, in Rectangle tileArea, string? machineTypeId = null)
        {
            this.MachineTypeID = machineTypeId ?? this.GetDefaultMachineId();
            this.Location = location;
            this.TileArea = tileArea;
        }

        /// <summary>Get the tile area for a building.</summary>
        /// <param name="building">The building.</param>
        protected static Rectangle GetTileAreaFor(Building building)
        {
            return new Rectangle(building.tileX.Value, building.tileY.Value, building.tilesWide.Value, building.tilesHigh.Value);
        }

        /// <summary>Get the tile area for a placed object.</summary>
        /// <param name="tile">The tile position.</param>
        protected static Rectangle GetTileAreaFor(in Vector2 tile)
        {
            return new Rectangle((int)tile.X, (int)tile.Y, 1, 1);
        }

        /// <summary>Get a tracked stack for an item, if it's not null.</summary>
        /// <param name="item">The item to track.</param>
        /// <param name="onEmpty">The callback invoked when the stack is empty.</param>
        protected ITrackedStack? GetTracked(Item? item, Action<Item>? onEmpty = null)
        {
            return item != null
                ? new TrackedItem(item, onEmpty: onEmpty)
                : null;
        }

        /// <summary>Get the default ID for the machine type.</summary>
        private string GetDefaultMachineId()
        {
            return BaseMachine.GetDefaultMachineId(this.GetType());
        }
    }

    /// <summary>The base implementation for a machine.</summary>
    internal abstract class BaseMachine<TMachine> : BaseMachine
    {
        /*********
        ** Fields
        *********/
        /// <summary>The underlying entity automated by this machine. This is only stored for the machine instance, and can be null if not applicable.</summary>
        protected TMachine Machine { get; }


        /*********
        ** Protected methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="machine">The underlying entity automated by this machine. This is only stored for the machine instance, and can be null if not applicable.</param>
        /// <param name="location">The machine's in-game location.</param>
        /// <param name="tileArea">The tile area covered by the machine.</param>
        /// <param name="machineTypeId">A unique ID for the machine type, or <c>null</c> to generate it from the type name.</param>
        protected BaseMachine(TMachine machine, GameLocation location, in Rectangle tileArea, string? machineTypeId = null)
            : base(location, tileArea, machineTypeId)
        {
            this.Machine = machine;
        }
    }
}
