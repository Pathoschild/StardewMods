using Microsoft.Xna.Framework;
using StardewValley;
using SObject = StardewValley.Object;

namespace Pathoschild.Stardew.Automate.Framework
{
    /// <summary>A generic machine instance.</summary>
    internal abstract class GenericObjectMachine<TMachine> : BaseMachine<TMachine> where TMachine : SObject
    {
        /*********
        ** Public methods
        *********/
        /// <summary>Get the machine's processing state.</summary>
        public override MachineState GetState()
        {
            return this.GetGenericState();
        }

        /// <summary>Get the output item.</summary>
        public override ITrackedStack GetOutput()
        {
            return new TrackedItem(this.Machine.heldObject.Value, onEmpty: this.GenericReset);
        }


        /*********
        ** Protected methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="machine">The underlying machine.</param>
        /// <param name="location">The in-game location.</param>
        /// <param name="tile">The tile covered by the machine.</param>
        /// <param name="machineTypeId">A unique ID for the machine type, or <c>null</c> to generate it from the type name.</param>
        protected GenericObjectMachine(TMachine machine, GameLocation location, Vector2 tile, string machineTypeId = null)
            : base(machine, location, BaseMachine.GetTileAreaFor(tile), machineTypeId) { }

        /// <summary>Get a generic machine state based on the machine's <see cref="SObject.heldObject"/> and <see cref="SObject.readyForHarvest"/> fields. This always returns one of three values: <see cref="MachineState.Empty"/>, <see cref="MachineState.Processing"/>, or <see cref="MachineState.Done"/>.</summary>
        /// <param name="emptyState">The machine state if it's empty.</param>
        protected MachineState GetGenericState(MachineState emptyState = MachineState.Empty)
        {
            if (this.Machine.heldObject.Value == null)
                return emptyState;

            return this.Machine.readyForHarvest.Value
                ? MachineState.Done
                : MachineState.Processing;
        }

        /// <summary>Reset the machine so it's ready to accept a new input.</summary>
        /// <param name="item">The output item that was taken.</param>
        protected void GenericReset(Item item)
        {
            this.Machine.heldObject.Value = null;
            this.Machine.readyForHarvest.Value = false;
        }

        /// <summary>Generic logic to pull items from storage based on the given recipes.</summary>
        /// <param name="storage">The available items.</param>
        /// <param name="recipes">The recipes to match.</param>
        protected bool GenericPullRecipe(IStorage storage, IRecipe[] recipes)
        {
            return this.GenericPullRecipe(storage, recipes, out _);
        }

        /// <summary>Generic logic to pull items from storage based on the given recipes.</summary>
        /// <param name="storage">The available items.</param>
        /// <param name="recipes">The recipes to match.</param>
        /// <param name="input">The consumed item.</param>
        protected bool GenericPullRecipe(IStorage storage, IRecipe[] recipes, out Item input)
        {
            if (storage.TryGetIngredient(recipes, out IConsumable consumable, out IRecipe recipe))
            {
                input = consumable.Take();
                this.Machine.heldObject.Value = recipe.Output(input);
                this.Machine.MinutesUntilReady = recipe.Minutes(input);
                return true;
            }

            input = null;
            return false;
        }
    }
}
