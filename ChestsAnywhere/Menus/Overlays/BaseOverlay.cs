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
        /// <summary>The last viewport bounds.</summary>
        private Rectangle LastViewport;

        /// <summary>Indicates whether to keep the overlay active. If <c>null</c>, the overlay is kept until explicitly disposed.</summary>
        private readonly Func<bool> KeepAliveCheck;


        /*********
        ** Public methods
        *********/
        /// <summary>Release all resources.</summary>
        public virtual void Dispose()
        {
            GraphicsEvents.OnPostRenderGuiEvent -= this.OnPostRenderGuiEvent;
            GameEvents.UpdateTick -= this.OnUpdateTick;
            ControlEvents.KeyPressed -= this.OnKeyPressed;
            ControlEvents.ControllerButtonPressed -= this.OnControllerButtonPressed;
            ControlEvents.MouseChanged -= this.OnMouseChanged;
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
            ControlEvents.MouseChanged += this.OnMouseChanged;
        }

        /// <summary>Draw the overlay to the screen.</summary>
        /// <param name="batch">The sprite batch being drawn.</param>
        protected virtual void Draw(SpriteBatch batch) { }

        /// <summary>The method invoked when the player left-clicks.</summary>
        /// <param name="x">The X-position of the cursor.</param>
        /// <param name="y">The Y-position of the cursor.</param>
        /// <returns>Whether the event has been handled and shouldn't be propagated further.</returns>
        protected virtual bool ReceiveLeftClick(int x, int y)
        {
            return false;
        }

        /// <summary>The method invoked when the player presses a key.</summary>
        /// <param name="input">The key that was pressed.</param>
        /// <returns>Whether the event has been handled and shouldn't be propagated further.</returns>
        protected virtual bool ReceiveKeyPress(Keys input)
        {
            return false;
        }

        /// <summary>The method invoked when the player presses a controller button.</summary>
        /// <param name="input">The button that was pressed.</param>
        /// <returns>Whether the event has been handled and shouldn't be propagated further.</returns>
        protected virtual bool ReceiveButtonPress(Buttons input)
        {
            return false;
        }

        /// <summary>The method invoked when the player uses the mouse scroll wheel.</summary>
        /// <param name="amount">The scroll amount.</param>
        /// <returns>Whether the event has been handled and shouldn't be propagated further.</returns>
        protected virtual bool ReceiveScrollWheelAction(int amount)
        {
            return false;
        }

        /// <summary>The method invoked when the cursor is hovered.</summary>
        /// <param name="x">The cursor's X position.</param>
        /// <param name="y">The cursor's Y position.</param>
        /// <returns>Whether the event has been handled and shouldn't be propagated further.</returns>
        protected virtual bool ReceiveCursorHover(int x, int y)
        {
            return false;
        }

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
        }

        /// <summary>The method invoked when the player presses a key.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The event arguments.</param>
        private void OnKeyPressed(object sender, EventArgsKeyPressed e)
        {
            bool handled = this.ReceiveKeyPress(e.KeyPressed);
            if (handled)
                Game1.oldKBState = Keyboard.GetState();

        }

        /// <summary>The method invoked when the player presses a key.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The event arguments.</param>
        private void OnControllerButtonPressed(object sender, EventArgsControllerButtonPressed e)
        {
            bool handled = this.ReceiveButtonPress(e.ButtonPressed);
            if (handled)
                Game1.oldPadState = GamePad.GetState(PlayerIndex.One);
        }

        /// <summary>The method invoked when the mouse state changes.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The event arguments.</param>
        private void OnMouseChanged(object sender, EventArgsMouseStateChanged e)
        {
            // get data
            MouseState oldState = e.PriorState;
            MouseState newState = e.NewState;
            int newX = Game1.getMouseX();
            int newY = Game1.getMouseY();

            // raise events
            bool hoverHandled = this.ReceiveCursorHover(newX, newY);
            bool leftClickHandled = oldState.LeftButton != ButtonState.Pressed && newState.LeftButton == ButtonState.Pressed && this.ReceiveLeftClick(newX, newY);
            bool scrollHandled = oldState.ScrollWheelValue != newState.ScrollWheelValue && this.ReceiveScrollWheelAction(newState.ScrollWheelValue - oldState.ScrollWheelValue);

            // suppress handled input
            if (hoverHandled || leftClickHandled || scrollHandled)
            {
                MouseState cur = Game1.oldMouseState;
                Game1.oldMouseState = new MouseState(
                    x: cur.X,
                    y: cur.Y,
                    scrollWheel: scrollHandled ? newState.ScrollWheelValue : cur.ScrollWheelValue,
                    leftButton: leftClickHandled ? newState.LeftButton : cur.LeftButton,
                    middleButton: cur.MiddleButton,
                    rightButton: cur.RightButton,
                    xButton1: cur.XButton1,
                    xButton2: cur.XButton2
                );
            }
        }
    }
}
