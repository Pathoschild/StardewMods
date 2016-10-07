using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using StardewModdingAPI.Events;
using StardewValley;

namespace ChestsAnywhere.Menus.Overlays
{
    /// <summary>An interface which supports user interaction and overlays the active menu (if any).</summary>
    internal abstract class BaseOverlay : IDisposable
    {
        /*********
        ** Properties
        *********/
        /// <summary>The last mouse state.</summary>
        private MouseState LastMouseState;

        /// <summary>Indicates whether to keep the overlay active. If <c>null</c>, the overlay is kept until explicitly disposed.</summary>
        private readonly Func<bool> KeepAliveCheck;


        /*********
        ** Public methods
        *********/
        /// <summary>Release all resources.</summary>
        public void Dispose()
        {
            GraphicsEvents.OnPostRenderGuiEvent -= this.OnPostRenderGuiEvent;
            GameEvents.UpdateTick -= this.OnUpdateTicket;
            ControlEvents.KeyPressed -= this.OnKeyPressed;
            ControlEvents.ControllerButtonPressed -= this.OnControllerButtonPressed;
        }


        /*********
        ** Protected methods
        *********/
        /****
        ** Implementation
        ****/
        /// <summary>Construct an instance.</summary>
        /// <param name="keepAlive">Indicates whether to keep the overlay active. If <c>null</c>, the overlay is kept until explicitly disposed.</param>
        protected BaseOverlay(Func<bool> keepAlive = null)
        {
            this.KeepAliveCheck = keepAlive;
            GraphicsEvents.OnPostRenderGuiEvent += this.OnPostRenderGuiEvent;
            GameEvents.UpdateTick += this.OnUpdateTicket;
            ControlEvents.KeyPressed += this.OnKeyPressed;
            ControlEvents.ControllerButtonPressed += this.OnControllerButtonPressed;
        }

        /// <summary>Draw the overlay to the screen.</summary>
        /// <param name="batch">The sprite batch being drawn.</param>
        protected virtual void Draw(SpriteBatch batch) { }

        /// <summary>The method invoked when the player left-clicks.</summary>
        /// <param name="x">The X-position of the cursor.</param>
        /// <param name="y">The Y-position of the cursor.</param>
        protected virtual void ReceiveLeftClick(int x, int y) { }

        /// <summary>The method invoked when the player presses a key.</summary>
        /// <param name="input">The key that was pressed.</param>
        protected virtual void ReceiveKeyPress(Keys input) { }

        /// <summary>The method invoked when the player presses a controller button.</summary>
        /// <param name="input">The button that was pressed.</param>
        protected virtual void ReceiveButtonPress(Buttons input) { }

        /// <summary>Draw the mouse cursor.</summary>
        /// <remarks>Derived from <see cref="StardewValley.Menus.IClickableMenu.drawMouse"/>.</remarks>
        protected void DrawCursor()
        {
            if (Game1.options.hardwareCursor)
                return;
            Game1.spriteBatch.Draw(Game1.mouseCursors, new Vector2(Game1.getOldMouseX(), Game1.getOldMouseY()), Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, 0, 16, 16), Color.White, 0.0f, Vector2.Zero, Game1.pixelZoom + Game1.dialogueButtonScale / 150f, SpriteEffects.None, 0f);
        }

        /****
        ** Event listeners
        ****/
        /// <summary>The method called when the game finishes drawing components to the screen.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The event arguments.</param>
        private void OnPostRenderGuiEvent(object sender, EventArgs e)
        {
            this.Draw(Game1.spriteBatch);
        }

        /// <summary>The method called once per event tick.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The event arguments.</param>
        private void OnUpdateTicket(object sender, EventArgs e)
        {
            // detect end of life
            if (this.KeepAliveCheck != null && !this.KeepAliveCheck())
            {
                this.Dispose();
                return;
            }

            // detect left clicks
            MouseState state = Mouse.GetState();
            if (state.LeftButton == ButtonState.Pressed && this.LastMouseState.LeftButton != ButtonState.Pressed)
                this.ReceiveLeftClick(state.X, state.Y);
            this.LastMouseState = state;
        }

        /// <summary>The method invoked when the player presses a key.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The event arguments.</param>
        private void OnKeyPressed(object sender, EventArgsKeyPressed e)
        {
            this.ReceiveKeyPress(e.KeyPressed);
        }

        /// <summary>The method invoked when the player presses a key.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The event arguments.</param>
        private void OnControllerButtonPressed(object sender, EventArgsControllerButtonPressed e)
        {
            this.ReceiveButtonPress(e.ButtonPressed);
        }
    }
}
