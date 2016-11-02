using System;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Pathoschild.Stardew.DebugMode.Framework;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;

namespace Pathoschild.Stardew.DebugMode
{
    /// <summary>The mod entry point.</summary>
    public class ModEntry : Mod
    {
        /*********
        ** Properties
        *********/
        /// <summary>The mod configuration settings.</summary>
        private ModConfig Config;

        /// <summary>Whether the built-in debug mode is enabled.</summary>
        private bool DebugMode
        {
            get { return Game1.debugMode; }
            set { Game1.debugMode = value; }
        }

        /// <summary>Keyboard keys which are mapped to a destructive action in debug mode. See <see cref="ModConfig.AllowDangerousCommands"/>.</summary>
        private readonly Keys[] DestructiveKeys =
        {
            Keys.P, // ends current day
            Keys.M, // ends current season
            Keys.H, // randomises player's hat
            Keys.I, // randomises player's hair
            Keys.J, // randomises player's shirt and pants
            Keys.L, // randomises player
            Keys.U, // randomises farmhouse wallpaper and floors
            Keys.F10 // tries to launch a multiplayer server and crashes
        };


        /*********
        ** Public methods
        *********/
        /// <summary>Initialise the mod.</summary>
        public override void Entry(params object[] objects)
        {
            // initialise
            this.Config = new RawModConfig().InitializeConfig(this.BaseConfigPath).GetParsed();

            // hook input events
            ControlEvents.KeyPressed += this.ReceiveKeyPress;
            if (this.Config.Controller.HasAny())
            {
                ControlEvents.ControllerButtonPressed += this.ReceiveButtonPress;
                ControlEvents.ControllerTriggerPressed += this.ReceiveTriggerPress;
            }

            // hook overlay
            Lazy<Texture2D> pixel = new Lazy<Texture2D>(() =>
            {
                Texture2D texture = new Texture2D(Game1.graphics.GraphicsDevice, 1, 1);
                texture.SetData(new[] { Color.White });
                return texture;
            });
            GraphicsEvents.OnPostRenderEvent += (sender, e) => this.OnPostRenderEvent(sender, e, pixel.Value);
        }


        /*********
        ** Private methods
        *********/
        /// <summary>The event called by SMAPI when rendering to the screen.</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        /// <param name="pixel">The cached pixel used to draw overlays.</param>
        public void OnPostRenderEvent(object sender, EventArgs e, Texture2D pixel)
        {
            if (!this.DebugMode)
                return;
            SpriteBatch batch = Game1.spriteBatch;
            SpriteFont font = Game1.smallFont;

            // draw cursor tile position
            {
                Vector2 tile = Game1.currentCursorTile;
                Vector2 position = new Vector2(Game1.getOldMouseX(), Game1.getOldMouseY());

                string text = $"{tile.X}, {tile.Y}";
                Vector2 textSize = font.MeasureString(text);
                batch.DrawString(font, text, new Vector2(position.X - textSize.X, position.Y), Color.Red);
            }

            // draw cursor crosshairs
            batch.Draw(pixel, new Rectangle(0, Game1.getOldMouseY() - 1, Game1.viewport.Width, 3), Color.Black * 0.5f);
            batch.Draw(pixel, new Rectangle(Game1.getOldMouseX() - 1, 0, 3, Game1.viewport.Height), Color.Black * 0.5f);
        }

        /// <summary>The method invoked when the player presses a keyboard button.</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event data.</param>
        private void ReceiveKeyPress(object sender, EventArgsKeyPressed e)
        {
            // handle hotkey
            this.HandleInput(e.KeyPressed, this.Config.Keyboard);

            // suppress dangerous actions
            if (!this.Config.AllowDangerousCommands && this.DebugMode && this.DestructiveKeys.Contains(e.KeyPressed))
            {
                Keys[] pressedKeys = Game1.oldKBState.GetPressedKeys().Union(new[] { e.KeyPressed }).ToArray();
                Game1.oldKBState = new KeyboardState(pressedKeys);
            }
        }

        /// <summary>The method invoked when the player presses a controller button.</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event data.</param>
        private void ReceiveButtonPress(object sender, EventArgsControllerButtonPressed e)
        {
            this.HandleInput(e.ButtonPressed, this.Config.Controller);
        }

        /// <summary>The method invoked when the player presses a controller trigger button.</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event data.</param>
        private void ReceiveTriggerPress(object sender, EventArgsControllerTriggerPressed e)
        {
            this.HandleInput(e.ButtonPressed, this.Config.Controller);
        }

        /// <summary>The method invoked when the player presses an input button.</summary>
        /// <typeparam name="TKey">The input type.</typeparam>
        /// <param name="key">The pressed input.</param>
        /// <param name="map">The configured input mapping.</param>
        private void HandleInput<TKey>(TKey key, InputMapConfiguration<TKey> map)
        {
            if (!map.IsValidKey(key))
                return;

            // perform bound action
            if (key.Equals(map.ToggleDebug))
                this.DebugMode = !this.DebugMode;
        }
    }
}
