using System.Collections.Generic;
using Microsoft.Xna.Framework;
using StardewValley;
using StardewValley.Objects;

namespace Pathoschild.Stardew.ChestsAnywhere.Framework.Containers
{
    /// <summary>A storage container for the shipping bin.</summary>
    internal class ShippingBinContainer : ChestContainer
    {
        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="contents">The shipping bin inventory.</param>
        public ShippingBinContainer(List<Item> contents)
            : base(new Chest(0, contents, Vector2.Zero), isEditable: false) { }

        /// <summary>Get whether the in-game container is open.</summary>
        public override bool IsOpen()
        {
            return false; // can't be opened directly
        }

        /// <summary>Get whether the container has its default name.</summary>
        public override bool HasDefaultName()
        {
            return true; // name isn't editable
        }
    }
}
