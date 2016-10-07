using System;
using ChestsAnywhere.Menus.Overlays;
using StardewValley.Menus;

namespace ChestsAnywhere.Framework
{
    /// <summary>Context for a chest currently being viewed or edited.</summary>
    internal class EditChestContext
    {
        /*********
        ** Accessors
        *********/
        /// <summary>The chest being viewed or edited.</summary>
        public Func<ManagedChest> Chest { get; set; }

        /// <summary>The initial menu used to view the chest.</summary>
        public IClickableMenu ViewMenu { get; set; }

        /// <summary>The edit button overlaid onto the view menu (if any).</summary>
        public EditButtonOverlay EditButton { get; set; }

        /// <summary>The overlaid menu used to edit the chest (if open).</summary>
        public IClickableMenu EditMenu { get; set; }


        /*********
        ** Public methods
        *********/
        /// <summary>Release owned resources and destroy owned components.</summary>
        public void Dispose()
        {
            this.EditButton?.Dispose();
        }
    }
}