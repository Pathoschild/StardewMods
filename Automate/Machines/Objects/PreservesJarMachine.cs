using Microsoft.Xna.Framework;
using Pathoschild.Stardew.Automate.Framework;
using StardewValley.Objects;
using SObject = StardewValley.Object;

namespace Pathoschild.Stardew.Automate.Machines.Objects
{
    /// <summary>A preserves jar that accepts input and provides output.</summary>
    internal class PreservesJarMachine : GenericMachine
    {
        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="machine">The underlying machine.</param>
        public PreservesJarMachine(SObject machine)
            : base(machine) { }

        /// <summary>Pull items from the connected chests.</summary>
        /// <param name="chests">The connected chests.</param>
        /// <returns>Returns whether the machine started processing an item.</returns>
        public override bool Pull(Chest[] chests)
        {
            SObject jar = this.Machine;

            // fruit => jelly
            if (chests.TryGetIngredient(item => item.category == SObject.FruitsCategory, 1, out Requirement fruit))
            {
                fruit.Consume();
                SObject item = (SObject)fruit.GetOne();

                jar.heldObject = new SObject(Vector2.Zero, 344, item.Name + " Jelly", false, true, false, false)
                {
                    Price = 50 + item.Price * 2,
                    name = item.Name + " Jelly",
                    preserve = SObject.PreserveType.Jelly,
                    preservedParentSheetIndex = item.parentSheetIndex
                };
                jar.minutesUntilReady = 4000;
                return true;
            }

            // vegetable => pickled vegetable
            if (chests.TryGetIngredient(item => item.category == SObject.VegetableCategory, 1, out Requirement vegetable))
            {
                vegetable.Consume();
                SObject item = (SObject)vegetable.GetOne();

                jar.heldObject = new SObject(Vector2.Zero, 342, "Pickled " + item.Name, false, true, false, false)
                {
                    Price = 50 + item.Price * 2,
                    name = "Pickled " + item.Name,
                    preserve = SObject.PreserveType.Pickle,
                    preservedParentSheetIndex = item.parentSheetIndex
                };
                jar.minutesUntilReady = 4000;
                return true;
            }

            return false;
        }
    }
}
