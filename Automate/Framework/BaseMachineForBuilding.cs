using Microsoft.Xna.Framework;
using StardewValley;
using StardewValley.Buildings;

namespace Pathoschild.Stardew.Automate.Framework
{
    /// <summary>A generic machine instance for a building.</summary>
    internal abstract class BaseMachineForBuilding<TMachine> : BaseMachine<TMachine> where TMachine : Building
    {
        /*********
        ** Protected methods
        *********/
        /// <inheritdoc />
        protected BaseMachineForBuilding(TMachine machine, GameLocation location, in Rectangle tileArea, string? machineTypeId = null)
            : base(machine, location, in tileArea, machineTypeId) { }

        /// <summary>Get the player who 'owns' the machine.</summary>
        /// <remarks>See remarks on <see cref="GenericObjectMachine{TMachine}.GetOwner"/>.</remarks>
        protected Farmer GetOwner()
        {
            Farmer mainPlayer = Game1.player;

            long ownerId = this.Machine.owner.Value;
            return ownerId != 0
                ? Game1.GetPlayer(ownerId) ?? mainPlayer
                : mainPlayer;
        }
    }
}
