using Microsoft.Xna.Framework;
using Pathoschild.Stardew.Automate.Framework;
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

        /// <summary>Pull items from the connected pipes.</summary>
        /// <param name="pipes">The connected IO pipes.</param>
        /// <returns>Returns whether the machine started processing an item.</returns>
        public override bool Pull(IPipe[] pipes)
        {
            SObject jar = this.Machine;

            // fruit => jelly
            if (pipes.TryGetIngredient(item => item.Sample.category == SObject.FruitsCategory, 1, out Requirement fruit))
            {
                fruit.Reduce();
                SObject sample = (SObject)fruit.Sample;

                jar.heldObject = new SObject(Vector2.Zero, 344, sample.Name + " Jelly", false, true, false, false)
                {
                    Price = 50 + sample.Price * 2,
                    name = sample.Name + " Jelly",
#if SDV_1_2
                    preserve = SObject.PreserveType.Jelly,
                    preservedParentSheetIndex = item.parentSheetIndex
#endif
                };
                jar.minutesUntilReady = 4000;
                return true;
            }

            // vegetable => pickled vegetable
            if (pipes.TryGetIngredient(item => item.Sample.category == SObject.VegetableCategory, 1, out Requirement vegetable))
            {
                vegetable.Reduce();
                SObject sample = (SObject)vegetable.Sample;

                jar.heldObject = new SObject(Vector2.Zero, 342, "Pickled " + sample.Name, false, true, false, false)
                {
                    Price = 50 + sample.Price * 2,
                    name = "Pickled " + sample.Name,
#if SDV_1_2
                    preserve = SObject.PreserveType.Pickle,
                    preservedParentSheetIndex = item.parentSheetIndex
#endif
                };
                jar.minutesUntilReady = 4000;
                return true;
            }

            return false;
        }
    }
}
