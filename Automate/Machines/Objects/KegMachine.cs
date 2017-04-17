using Microsoft.Xna.Framework;
using Pathoschild.Stardew.Automate.Framework;
using SObject = StardewValley.Object;

namespace Pathoschild.Stardew.Automate.Machines.Objects
{
    /// <summary>A keg that accepts input and provides output.</summary>
    internal class KegMachine : GenericMachine
    {
        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="machine">The underlying machine.</param>
        public KegMachine(SObject machine)
            : base(machine) { }

        /// <summary>Pull items from the connected pipes.</summary>
        /// <param name="pipes">The connected IO pipes.</param>
        /// <returns>Returns whether the machine started processing an item.</returns>
        public override bool Pull(IPipe[] pipes)
        {
            SObject keg = this.Machine;

            // honey => mead
            if (pipes.TryConsume(340, 1))
            {
                keg.heldObject = new SObject(Vector2.Zero, 459, "Mead", false, true, false, false) { name = "Mead" };
                keg.minutesUntilReady = 600;
                return true;
            }

            // coffee bean => coffee
            if (pipes.TryConsume(433, 5))
            {
                keg.heldObject = new SObject(Vector2.Zero, 395, "Coffee", false, true, false, false) { name = "Coffee" };
                keg.minutesUntilReady = 120;
                return true;
            }

            // wheat => beer
            if (pipes.TryConsume(262, 1))
            {
                keg.heldObject = new SObject(Vector2.Zero, 346, "Beer", false, true, false, false) { name = "Beer" };
                keg.minutesUntilReady = 1750;
                return true;
            }

            // hops => pale ale
            if (pipes.TryConsume(304, 1))
            {
                keg.heldObject = new SObject(Vector2.Zero, 303, "Pale Ale", false, true, false, false) { name = "Pale Ale" };
                keg.minutesUntilReady = 2250;
                return true;
            }

            // fruit => wine
            if (pipes.TryGetIngredient(item => item.Sample.category == SObject.FruitsCategory, 1, out Requirement fruit))
            {
                fruit.Reduce();
                SObject sample = (SObject)fruit.Sample;

                keg.heldObject = new SObject(Vector2.Zero, 348, sample.Name + " Wine", false, true, false, false)
                {
                    name = sample.Name + " Wine",
                    Price = sample.Price * 3,
#if SDV_1_2
                    preserve = SObject.PreserveType.Wine,
                    preservedParentSheetIndex = sample.parentSheetIndex
#endif
                };
                keg.minutesUntilReady = 10000;
                return true;
            }

            // vegetable => juice
            if (pipes.TryGetIngredient(item => item.Sample.category == SObject.VegetableCategory, 1, out Requirement vegetable))
            {
                vegetable.Reduce();
                SObject sample = (SObject)vegetable.Sample;

                keg.heldObject = new SObject(Vector2.Zero, 350, sample.Name + " Juice", false, true, false, false)
                {
                    name = sample.Name + " Juice",
                    Price = (int)(sample.Price * 2.25),
#if SDV_1_2
                    preserve = SObject.PreserveType.Juice,
                    preservedParentSheetIndex = sample.parentSheetIndex
#endif
                };
                keg.minutesUntilReady = 6000;
                return true;
            }

            return false;
        }
    }
}
