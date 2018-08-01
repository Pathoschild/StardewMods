using StardewValley;
using SObject = StardewValley.Object;

namespace Pathoschild.Stardew.Automate.Framework
{
    /// <summary>A generic machine instance.</summary>
    internal abstract class GenericMachine<TMachine> : IMachine where TMachine : SObject
    {
        /*********
        ** Properties
        *********/
        /// <summary>The underlying machine.</summary>
        protected TMachine Machine { get; }


        /*********
        ** Public methods
        *********/
        /// <summary>Get the machine's processing state.</summary>
        public virtual MachineState GetState()
        {
            if (this.Machine.heldObject.Value == null)
                return MachineState.Empty;

            return this.Machine.readyForHarvest.Value
                ? MachineState.Done
                : MachineState.Processing;
        }

        /// <summary>Get the output item.</summary>
        public virtual ITrackedStack GetOutput()
        {
            return new TrackedItem(this.Machine.heldObject.Value, onEmpty: this.GenericReset);
        }

        /// <summary>Provide input to the machine.</summary>
        /// <param name="input">The available items.</param>
        /// <returns>Returns whether the machine started processing an item.</returns>
        public abstract bool SetInput(IStorage input);


        /*********
        ** Protected methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="machine">The underlying machine.</param>
        protected GenericMachine(TMachine machine)
        {
            this.Machine = machine;
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
        protected bool GenericPullRecipe(IStorage storage, Recipe[] recipes)
        {
            if (storage.TryGetIngredient(recipes, out IConsumable consumable, out Recipe recipe))
            {
                this.Machine.heldObject.Value = recipe.Output(consumable.Take());
                this.Machine.MinutesUntilReady = recipe.Minutes;
                return true;
            }
            return false;
        }
    }

    /// <summary>A generic machine instance.</summary>
    internal abstract class GenericMachine : GenericMachine<SObject>
    {
        /*********
        ** Protected methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="machine">The underlying machine.</param>
        protected GenericMachine(SObject machine)
            : base(machine) { }
    }
}
