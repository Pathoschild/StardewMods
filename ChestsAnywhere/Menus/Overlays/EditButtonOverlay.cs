using System;
using ChestsAnywhere.Framework;
using ChestsAnywhere.Menus.Components;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.Menus;

namespace ChestsAnywhere.Menus.Overlays
{
    /// <summary>An edit button that can be overlaid over the active menu, and switches to the edit chest UI when clicked.</summary>
    internal class EditButtonOverlay : BaseOverlay
    {
        /*********
        ** Properties
        *********/
        /// <summary>Fetches the current chest (if any).</summary>
        private readonly Func<ManagedChest> FetchChest;

        /// <summary>The mod configuration.</summary>
        private readonly ModConfig Config;

        /// <summary>The underlying button.</summary>
        private readonly ClickableTextureComponent Button;


        /*********
        ** Accessors
        *********/
        /// <summary>Whether the component should be temporarily hidden and ignore input. (Call <see cref="BaseOverlay.Dispose"/> to remove it permanently.)</summary>
        public bool IsDisabled { get; set; }


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="bounds">The bounds within which to draw the button.</param>
        /// <param name="fetchChest">Fetches the current chest (if any).</param>
        /// <param name="config">The mod configuration.</param>
        /// <param name="keepAlive">Indicates whether to keep the overlay active.</param>
        public EditButtonOverlay(Rectangle bounds, Func<ManagedChest> fetchChest, ModConfig config, Func<bool> keepAlive)
            : base(keepAlive)
        {
            this.FetchChest = fetchChest;
            this.Config = config;

            float zoom = bounds.Width / (Sprites.Icons.SpeechBubble.Width * 1f);
            this.Button = new ClickableTextureComponent("edit-chest", bounds, null, "edit chest", Sprites.Icons.Sheet, Sprites.Icons.SpeechBubble, zoom);
        }

        /// <summary>The method invoked when the player left-clicks.</summary>
        /// <param name="x">The X-position of the cursor.</param>
        /// <param name="y">The Y-position of the cursor.</param>
        protected override void ReceiveLeftClick(int x, int y)
        {
            if (this.IsDisabled)
                return;

            if (this.Button.containsPoint(x, y))
                Game1.activeClickableMenu = new EditChestForm(this.FetchChest(), this.Config);
        }

        /// <summary>The method invoked when the cursor is hovered.</summary>
        /// <param name="x">The cursor's X position.</param>
        /// <param name="y">The cursor's Y position.</param>
        protected override void ReceiveCursorHover(int x, int y)
        {
            if (this.IsDisabled)
                return;

            this.Button.tryHover(x, y);
        }

        /// <summary>Draw the overlay to the screen.</summary>
        /// <param name="batch">The sprite batch being drawn.</param>
        protected override void Draw(SpriteBatch batch)
        {
            if (this.IsDisabled)
                return;

            this.Button.draw(batch);
            this.DrawCursor();
        }
    }
}
