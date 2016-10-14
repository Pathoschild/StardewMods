using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using StardewModdingAPI.Events;
using StardewValley;
using Rectangle = xTile.Dimensions.Rectangle;

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

        /// <summary>The last viewport bounds.</summary>
        private Rectangle LastViewport;

        /// <summary>Indicates whether to keep the overlay active. If <c>null</c>, the overlay is kept until explicitly disposed.</summary>
        private readonly Func<bool> KeepAliveCheck;


        /*********
        ** Public methods
        *********/
        /// <summary>Release all resources.</summary>
        public void Dispose()
        {
            GraphicsEvents.OnPostRenderGuiEvent -= this.OnPostRenderGuiEvent;
            GameEvents.UpdateTick -= this.OnUpdateTick;
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
            this.LastViewport = new Rectangle(Game1.viewport.X, Game1.viewport.Y, Game1.viewport.Width, Game1.viewport.Height);
            GraphicsEvents.OnPostRenderGuiEvent += this.OnPostRenderGuiEvent;
            GameEvents.UpdateTick += this.OnUpdateTick;
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

        /// <summary>The method invoked when the player uses the mouse scroll wheel.</summary>
        /// <param name="amount">The scroll amount.</param>
        protected virtual void ReceiveScrollWheelAction(int amount) { }

        /// <summary>The method invoked when the cursor is hovered.</summary>
        /// <param name="x">The cursor's X position.</param>
        /// <param name="y">The cursor's Y position.</param>
        protected virtual void ReceiveCursorHover(int x, int y) { }

        /// <summary>The method invoked when the player resizes the game windoww.</summary>
        /// <param name="oldBounds">The previous game window bounds.</param>
        /// <param name="newBounds">The new game window bounds.</param>
        protected virtual void ReceiveGameWindowResized(Rectangle oldBounds, Rectangle newBounds) { }

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
        private void OnUpdateTick(object sender, EventArgs e)
        {
            // detect end of life
            if (this.KeepAliveCheck != null && !this.KeepAliveCheck())
            {
                this.Dispose();
                return;
            }

            // trigger window resize event
            Rectangle newViewport = Game1.viewport;
            if (this.LastViewport.Width != newViewport.Width || this.LastViewport.Height != newViewport.Height)
            {
                newViewport = new Rectangle(newViewport.X, newViewport.Y, newViewport.Width, newViewport.Height);
                this.ReceiveGameWindowResized(this.LastViewport, newViewport);
                this.LastViewport = newViewport;
            }

            // trigger mouse events
            MouseState mouseState = Mouse.GetState();
            int mouseX = Game1.getOldMouseX();
            int mouseY = Game1.getOldMouseY();
            this.ReceiveCursorHover(mouseX, mouseY);
            if (mouseState.LeftButton == ButtonState.Pressed && this.LastMouseState.LeftButton != ButtonState.Pressed)
                this.ReceiveLeftClick(mouseX, mouseY);
            if (mouseState.ScrollWheelValue != this.LastMouseState.ScrollWheelValue)
                this.ReceiveScrollWheelAction(mouseState.ScrollWheelValue - this.LastMouseState.ScrollWheelValue);
            this.LastMouseState = mouseState;
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
