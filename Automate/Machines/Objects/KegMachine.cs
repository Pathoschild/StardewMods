using Microsoft.Xna.Framework;
using Pathoschild.Stardew.Automate.Framework;
using StardewValley.Objects;
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

        /// <summary>Pull items from the connected chests.</summary>
        /// <param name="chests">The connected chests.</param>
        /// <returns>Returns whether the machine started processing an item.</returns>
        public override bool Pull(Chest[] chests)
        {
            SObject keg = this.Machine;

            // honey => mead
            if (chests.TryConsume(340, 1))
            {
                keg.heldObject = new SObject(Vector2.Zero, 459, "Mead", false, true, false, false) { name = "Mead" };
                keg.minutesUntilReady = 600;
                return true;
            }

            // coffee bean => coffee
            if (chests.TryConsume(433, 5))
            {
                keg.heldObject = new SObject(Vector2.Zero, 395, "Coffee", false, true, false, false) { name = "Coffee" };
                keg.minutesUntilReady = 120;
                return true;
            }

            // wheat => beer
            if (chests.TryConsume(262, 1))
            {
                keg.heldObject = new SObject(Vector2.Zero, 346, "Beer", false, true, false, false) { name = "Beer" };
                keg.minutesUntilReady = 1750;
                return true;
            }

            // hops => pale ale
            if (chests.TryConsume(304, 1))
            {
                keg.heldObject = new SObject(Vector2.Zero, 303, "Pale Ale", false, true, false, false) { name = "Pale Ale" };
                keg.minutesUntilReady = 2250;
                return true;
            }

            // fruit => wine
            if (chests.TryGetIngredient(item => item.category == SObject.FruitsCategory, 1, out Requirement fruit))
            {
                fruit.Consume();
                SObject item = (SObject)fruit.GetOne();

                keg.heldObject = new SObject(Vector2.Zero, 348, item.Name + " Wine", false, true, false, false)
                {
                    name = item.Name + " Wine",
                    Price = item.Price * 3,
                    preserve = SObject.PreserveType.Wine,
                    preservedParentSheetIndex = item.parentSheetIndex
                };
                keg.minutesUntilReady = 10000;
                return true;
            }

            // vegetable => juice
            if (chests.TryGetIngredient(item => item.category == SObject.VegetableCategory, 1, out Requirement vegetable))
            {
                vegetable.Consume();
                SObject item = (SObject)vegetable.GetOne();

                keg.heldObject = new SObject(Vector2.Zero, 350, item.Name + " Juice", false, true, false, false)
                {
                    name = item.Name + " Juice",
                    Price = (int)(item.Price * 2.25),
                    preserve = SObject.PreserveType.Juice,
                    preservedParentSheetIndex = item.parentSheetIndex
                };
                keg.minutesUntilReady = 6000;
                return true;
            }

            return false;
        }
    }
}
