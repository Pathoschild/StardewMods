using Microsoft.Xna.Framework;
using StardewValley;
using SObject = StardewValley.Object;

namespace Pathoschild.Stardew.Automate.Framework.Machines.Objects
{
    /// <summary>A deconstructor that accepts input and provides output.</summary>
    /// <remarks>Derived from <see cref="SObject.performObjectDropInAction"/> (search for 'Deconstructor').</remarks>
    internal class DeconstructorMachine : GenericObjectMachine<SObject>
    {
        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="machine">The underlying machine.</param>
        /// <param name="location">The location containing the machine.</param>
        /// <param name="tile">The tile covered by the machine.</param>
        public DeconstructorMachine(SObject machine, GameLocation location, Vector2 tile)
            : base(machine, location, tile) { }

        /// <summary>Provide input to the machine.</summary>
        /// <param name="input">The available items.</param>
        /// <returns>Returns whether the machine started processing an item.</returns>
        public override bool SetInput(IStorage input)
        {
            foreach (var trackedStack in input.GetItems())
            {
                SObject output = this.Machine.GetDeconstructorOutput(trackedStack.Sample.getOne());
                if (output != null)
                {
                    trackedStack.Reduce(1);
                    this.Machine.heldObject.Value = output;
                    this.Machine.MinutesUntilReady = 60;
                    return true;
                }
            }

            return false;
        }
    }
}
