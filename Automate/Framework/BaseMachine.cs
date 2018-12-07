using Microsoft.Xna.Framework;
using StardewValley;
using StardewValley.Buildings;
using SObject = StardewValley.Object;

namespace Pathoschild.Stardew.Automate.Framework
{
    /// <summary>The base implementation for a machine.</summary>
    internal abstract class BaseMachine : IMachine
    {
        /*********
        ** Accessors
        *********/
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
        public abstract ITrackedStack GetOutput();

        /// <summary>Provide input to the machine.</summary>
        /// <param name="input">The available items.</param>
        /// <returns>Returns whether the machine started processing an item.</returns>
        public abstract bool SetInput(IStorage input);


        /*********
        ** Protected methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="location">The machine's in-game location.</param>
        /// <param name="tileArea">The tile area covered by the machine.</param>
        protected BaseMachine(GameLocation location, in Rectangle tileArea)
        {
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
        /// <param name="obj">The placed object.</param>
        protected static Rectangle GetTileAreaFor(SObject obj)
        {
            return BaseMachine.GetTileAreaFor(obj.TileLocation);
        }

        /// <summary>Get the tile area for a placed object.</summary>
        /// <param name="tile">The tile position.</param>
        protected static Rectangle GetTileAreaFor(in Vector2 tile)
        {
            return new Rectangle((int)tile.X, (int)tile.Y, 1, 1);
        }
    }

    /// <summary>The base implementation for a machine.</summary>
    internal abstract class BaseMachine<TMachine> : BaseMachine
    {
        /*********
        ** Properties
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
        protected BaseMachine(TMachine machine, GameLocation location, in Rectangle tileArea)
            : base(location, tileArea)
        {
            this.Machine = machine;
        }
    }
}
