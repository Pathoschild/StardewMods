using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using Rectangle = xTile.Dimensions.Rectangle;

namespace Pathoschild.Stardew.Common.UI
{
    /// <summary>An interface which supports user interaction and overlays the active menu (if any).</summary>
    internal abstract class BaseOverlay : IDisposable
    {
        /*********
        ** Fields
        *********/
        /// <summary>The SMAPI events available for mods.</summary>
        private readonly IModEvents Events;

        /// <summary>An API for checking and changing input state.</summary>
        protected readonly IInputHelper InputHelper;

        /// <summary>Simplifies access to private code.</summary>
        protected readonly IReflectionHelper Reflection;

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
            this.Events.Display.Rendered -= this.OnRendered;
            this.Events.GameLoop.UpdateTicked -= this.OnUpdateTicked;
            this.Events.Input.ButtonPressed -= this.OnButtonPressed;
            this.Events.Input.CursorMoved -= this.OnCursorMoved;
            this.Events.Input.MouseWheelScrolled -= this.OnMouseWheelScrolled;
        }


        /*********
        ** Protected methods
        *********/
        /****
        ** Implementation
        ****/
        /// <summary>Construct an instance.</summary>
        /// <param name="events">The SMAPI events available for mods.</param>
        /// <param name="inputHelper">An API for checking and changing input state.</param>
        /// <param name="reflection">Simplifies access to private code.</param>
        /// <param name="keepAlive">Indicates whether to keep the overlay active. If <c>null</c>, the overlay is kept until explicitly disposed.</param>
        protected BaseOverlay(IModEvents events, IInputHelper inputHelper, IReflectionHelper reflection, Func<bool> keepAlive = null)
        {
            this.Events = events;
            this.InputHelper = inputHelper;
            this.Reflection = reflection;
            this.KeepAliveCheck = keepAlive;
            this.LastViewport = new Rectangle(Game1.viewport.X, Game1.viewport.Y, Game1.viewport.Width, Game1.viewport.Height);

            events.Display.Rendered += this.OnRendered;
            events.GameLoop.UpdateTicked += this.OnUpdateTicked;
            events.Input.ButtonPressed += this.OnButtonPressed;
            events.Input.CursorMoved += this.OnCursorMoved;
            events.Input.MouseWheelScrolled += this.OnMouseWheelScrolled;
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

        /// <summary>The method invoked when the player presses a button.</summary>
        /// <param name="input">The button that was pressed.</param>
        /// <returns>Whether the event has been handled and shouldn't be propagated further.</returns>
        protected virtual bool ReceiveButtonPress(SButton input)
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

        /// <summary>The method invoked when the player resizes the game window.</summary>
        /// <param name="oldBounds">The previous game window bounds.</param>
        /// <param name="newBounds">The new game window bounds.</param>
        protected virtual void ReceiveGameWindowResized(Rectangle oldBounds, Rectangle newBounds) { }

        /// <summary>Draw the mouse cursor.</summary>
        /// <remarks>Derived from <see cref="StardewValley.Menus.IClickableMenu.drawMouse"/>.</remarks>
        protected void DrawCursor()
        {
            if (Game1.options.hardwareCursor)
                return;

            Vector2 cursorPos = new Vector2(Game1.getMouseX(), Game1.getMouseY());
            if (Constants.TargetPlatform == GamePlatform.Android)
                cursorPos *= Game1.options.zoomLevel / this.Reflection.GetProperty<float>(typeof(Game1), "NativeZoomLevel").GetValue();

            Game1.spriteBatch.Draw(Game1.mouseCursors, cursorPos, Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, Game1.options.SnappyMenus ? 44 : 0, 16, 16), Color.White * Game1.mouseCursorTransparency, 0.0f, Vector2.Zero, Game1.pixelZoom + Game1.dialogueButtonScale / 150f, SpriteEffects.None, 1f);
        }

        /****
        ** Event listeners
        ****/
        /// <summary>The method called when the game finishes drawing components to the screen.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The event arguments.</param>
        private void OnRendered(object sender, RenderedEventArgs e)
        {
            if (Constants.TargetPlatform == GamePlatform.Android)
            {
                object originMatrix = this.Reflection.GetField<object>(Game1.spriteBatch, "_matrix").GetValue() ?? Matrix.Identity;
                float nativeZoomLevel = this.Reflection.GetProperty<float>(typeof(Game1), "NativeZoomLevel").GetValue();

                Game1.spriteBatch.End();
                Game1.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, null, null, Matrix.CreateScale(nativeZoomLevel));
                this.Draw(Game1.spriteBatch);
                Game1.spriteBatch.End();

                Game1.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, null, null, (Matrix)originMatrix);
            }
            else
                this.Draw(Game1.spriteBatch);
        }

        /// <summary>The method called once per event tick.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The event arguments.</param>
        private void OnUpdateTicked(object sender, UpdateTickedEventArgs e)
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
        private void OnButtonPressed(object sender, ButtonPressedEventArgs e)
        {
            bool handled;
            if (Constants.TargetPlatform == GamePlatform.Android)
            {
                float NativeZoomLevel = (float)typeof(Game1).GetProperty("NativeZoomLevel", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static).GetValue(null);
                handled = e.Button == SButton.MouseLeft || e.Button.IsUseToolButton()
                ? this.ReceiveLeftClick((int)(Game1.getMouseX() * Game1.options.zoomLevel / NativeZoomLevel), (int)(Game1.getMouseY() * Game1.options.zoomLevel / NativeZoomLevel))
                : this.ReceiveButtonPress(e.Button);
            }
            else
            {
                handled = e.Button == SButton.MouseLeft || e.Button.IsUseToolButton()
                    ? this.ReceiveLeftClick(Game1.getMouseX(), Game1.getMouseY())
                    : this.ReceiveButtonPress(e.Button);
            }

            if (handled)
                this.InputHelper.Suppress(e.Button);
        }

        /// <summary>The method invoked when the mouse wheel is scrolled.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The event arguments.</param>
        private void OnMouseWheelScrolled(object sender, MouseWheelScrolledEventArgs e)
        {
            bool scrollHandled = this.ReceiveScrollWheelAction(e.Delta);
            if (scrollHandled)
            {
                MouseState cur = Game1.oldMouseState;
                Game1.oldMouseState = new MouseState(
                    x: cur.X,
                    y: cur.Y,
                    scrollWheel: e.NewValue,
                    leftButton: cur.LeftButton,
                    middleButton: cur.MiddleButton,
                    rightButton: cur.RightButton,
                    xButton1: cur.XButton1,
                    xButton2: cur.XButton2
                );
            }
        }

        /// <summary>The method invoked when the in-game cursor is moved.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The event arguments.</param>
        private void OnCursorMoved(object sender, CursorMovedEventArgs e)
        {
            int x = (int)e.NewPosition.ScreenPixels.X;
            int y = (int)e.NewPosition.ScreenPixels.Y;

            bool hoverHandled = this.ReceiveCursorHover(x, y);
            if (hoverHandled)
            {
                MouseState cur = Game1.oldMouseState;
                Game1.oldMouseState = new MouseState(
                    x: x,
                    y: y,
                    scrollWheel: cur.ScrollWheelValue,
                    leftButton: cur.LeftButton,
                    middleButton: cur.MiddleButton,
                    rightButton: cur.RightButton,
                    xButton1: cur.XButton1,
                    xButton2: cur.XButton2
                );
            }
        }
    }
}
