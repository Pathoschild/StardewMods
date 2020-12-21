using System;
using System.Reflection;
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

        /// <summary>The screen ID for which the overlay was created, to support split-screen mode.</summary>
        private readonly int ScreenId;

        /// <summary>The last viewport bounds.</summary>
        private Rectangle LastViewport;

        /// <summary>Indicates whether to keep the overlay active. If <c>null</c>, the overlay is kept until explicitly disposed.</summary>
        private readonly Func<bool> KeepAliveCheck;

        /// <summary>The UI mode to use for pixel coordinates in <see cref="ReceiveLeftClick"/> and <see cref="ReceiveCursorHover"/>, or <c>null</c> to use the current UI mode at the time the event is raised.</summary>
        private readonly bool? AssumeUiMode;


        /*********
        ** Public methods
        *********/
        /// <summary>Release all resources.</summary>
        public virtual void Dispose()
        {
            this.Events.Display.Rendered -= this.OnRendered;
            this.Events.Display.RenderedWorld -= this.OnRenderedWorld;
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
        /// <param name="assumeUiMode">The UI mode to use for pixel coordinates in <see cref="ReceiveLeftClick"/> and <see cref="ReceiveCursorHover"/>, or <c>null</c> to use the current UI mode at the time the event is raised.</param>
        protected BaseOverlay(IModEvents events, IInputHelper inputHelper, IReflectionHelper reflection, Func<bool> keepAlive = null, bool? assumeUiMode = null)
        {
            this.Events = events;
            this.InputHelper = inputHelper;
            this.Reflection = reflection;
            this.KeepAliveCheck = keepAlive;
            this.LastViewport = new Rectangle(Game1.uiViewport.X, Game1.uiViewport.Y, Game1.uiViewport.Width, Game1.uiViewport.Height);
            this.ScreenId = Context.ScreenId;
            this.AssumeUiMode = assumeUiMode;

            events.GameLoop.UpdateTicked += this.OnUpdateTicked;

            if (this.IsMethodOverridden(nameof(this.DrawUi)))
                events.Display.Rendered += this.OnRendered;
            if (this.IsMethodOverridden(nameof(this.DrawWorld)))
                events.Display.RenderedWorld += this.OnRenderedWorld;
            if (this.IsMethodOverridden(nameof(this.ReceiveButtonPress)) || this.IsMethodOverridden(nameof(this.ReceiveLeftClick)))
                events.Input.ButtonPressed += this.OnButtonPressed;
            if (this.IsMethodOverridden(nameof(this.ReceiveCursorHover)))
                events.Input.CursorMoved += this.OnCursorMoved;
            if (this.IsMethodOverridden(nameof(this.ReceiveScrollWheelAction)))
                events.Input.MouseWheelScrolled += this.OnMouseWheelScrolled;
        }

        /// <summary>Draw the overlay to the screen over the UI.</summary>
        /// <param name="batch">The sprite batch being drawn.</param>
        protected virtual void DrawUi(SpriteBatch batch) { }

        /// <summary>Draw the overlay to the screen under the UI.</summary>
        /// <param name="batch">The sprite batch being drawn.</param>
        protected virtual void DrawWorld(SpriteBatch batch) { }

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
        protected virtual void ReceiveGameWindowResized() { }

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
            if (Context.ScreenId != this.ScreenId)
                return;

            if (Constants.TargetPlatform == GamePlatform.Android)
            {
                object originMatrix = this.Reflection.GetField<object>(Game1.spriteBatch, "_matrix").GetValue() ?? Matrix.Identity;
                float nativeZoomLevel = this.Reflection.GetProperty<float>(typeof(Game1), "NativeZoomLevel").GetValue();

                Game1.spriteBatch.End();
                Game1.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, null, null, Matrix.CreateScale(nativeZoomLevel));
                this.DrawUi(Game1.spriteBatch);
                Game1.spriteBatch.End();

                Game1.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, null, null, (Matrix)originMatrix);
            }
            else
                this.DrawUi(Game1.spriteBatch);
        }

        /// <summary>The method called when the game finishes drawing components to the screen.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The event arguments.</param>
        private void OnRenderedWorld(object sender, RenderedWorldEventArgs e)
        {
            if (Context.ScreenId != this.ScreenId)
                return;

            this.DrawWorld(e.SpriteBatch);
        }

        /// <summary>The method called once per event tick.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The event arguments.</param>
        private void OnUpdateTicked(object sender, UpdateTickedEventArgs e)
        {
            if (Context.ScreenId == this.ScreenId)
            {
                // detect end of life
                if (this.KeepAliveCheck != null && !this.KeepAliveCheck())
                {
                    this.Dispose();
                    return;
                }

                // trigger window resize event
                Rectangle newViewport = Game1.uiViewport;
                if (this.LastViewport.Width != newViewport.Width || this.LastViewport.Height != newViewport.Height)
                {
                    newViewport = new Rectangle(newViewport.X, newViewport.Y, newViewport.Width, newViewport.Height);
                    this.ReceiveGameWindowResized();
                    this.LastViewport = newViewport;
                }
            }
            else if (!Context.HasScreenId(this.ScreenId))
                this.Dispose();
        }

        /// <summary>The method invoked when the player presses a key.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The event arguments.</param>
        private void OnButtonPressed(object sender, ButtonPressedEventArgs e)
        {
            if (Context.ScreenId != this.ScreenId)
                return;

            bool uiMode = this.AssumeUiMode ?? Game1.uiMode;
            bool handled;
            if (Constants.TargetPlatform == GamePlatform.Android)
            {
                float nativeZoomLevel = (float)typeof(Game1).GetProperty("NativeZoomLevel", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static).GetValue(null);
                handled = e.Button == SButton.MouseLeft || e.Button.IsUseToolButton()
                ? this.ReceiveLeftClick((int)(Game1.getMouseX() * Game1.options.zoomLevel / nativeZoomLevel), (int)(Game1.getMouseY() * Game1.options.zoomLevel / nativeZoomLevel))
                : this.ReceiveButtonPress(e.Button);
            }
            else
            {
                handled = e.Button == SButton.MouseLeft || e.Button.IsUseToolButton()
                    ? this.ReceiveLeftClick(Game1.getMouseX(uiMode), Game1.getMouseY(uiMode))
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
            if (Context.ScreenId != this.ScreenId)
                return;

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
            if (Context.ScreenId != this.ScreenId)
                return;

            bool uiMode = this.AssumeUiMode ?? Game1.uiMode;
            bool hoverHandled = this.ReceiveCursorHover(Game1.getMouseX(uiMode), Game1.getMouseY(uiMode));
            if (hoverHandled)
                Game1.InvalidateOldMouseMovement();
        }

        /// <summary>Get whether a method has been overridden by a subclass.</summary>
        /// <param name="name">The method name.</param>
        private bool IsMethodOverridden(string name)
        {
            MethodInfo method = this.GetType().GetMethod(name, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
            if (method == null)
                throw new InvalidOperationException($"Can't find method {this.GetType().FullName}.{name}.");

            return method.DeclaringType != typeof(BaseOverlay);
        }
    }
}
