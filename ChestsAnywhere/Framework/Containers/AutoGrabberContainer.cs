using StardewValley.Menus;
using StardewValley.Objects;
using SObject = StardewValley.Object;

namespace Pathoschild.Stardew.ChestsAnywhere.Framework.Containers
{
    /// <summary>A storage container for an in-game auto-grabber.</summary>
    internal class AutoGrabberContainer : ChestContainer
    {
        /*********
        ** Fields
        *********/
        /// <summary>The underlying auto-grabber.</summary>
        private readonly SObject AutoGrabber;


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="autoGrabber">The underlying auto-grabber.</param>
        /// <param name="chest">The underlying auto-grabber chest.</param>
        /// <param name="context">The <see cref="ItemGrabMenu.context"/> value which indicates what opened the menu.</param>
        public AutoGrabberContainer(SObject autoGrabber, Chest chest, object context)
            : base(chest, context, showColorPicker: false)
        {
            this.AutoGrabber = autoGrabber;
        }


        /*********
        ** Private methods
        *********/
        /// <inheritdoc />
        protected override void OnChanged()
        {
            base.OnChanged();

            this.Chest.clearNulls();
            this.AutoGrabber.showNextIndex.Value = !this.Chest.isEmpty();
        }
    }
}
