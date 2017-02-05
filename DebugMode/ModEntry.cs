using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Pathoschild.Stardew.Common;
using Pathoschild.Stardew.DebugMode.Framework;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Menus;

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

        /// <summary>A pixel texture that can be stretched and colourised for display.</summary>
        private readonly Lazy<Texture2D> Pixel = new Lazy<Texture2D>(ModEntry.CreatePixel);

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
        /// <summary>The mod entry point, called after the mod is first loaded.</summary>
        /// <param name="helper">Provides methods for interacting with the mod directory, such as read/writing a config file or custom JSON files.</param>
        public override void Entry(IModHelper helper)
        {
            // initialise
            this.Config = helper.ReadConfig<RawModConfig>().GetParsed();

            // hook events
            ControlEvents.KeyPressed += this.ReceiveKeyPress;
            if (this.Config.Controller.HasAny())
            {
                ControlEvents.ControllerButtonPressed += this.ReceiveButtonPress;
                ControlEvents.ControllerTriggerPressed += this.ReceiveTriggerPress;
            }
            GameEvents.GameLoaded += this.ReceiveGameLoaded;
            LocationEvents.CurrentLocationChanged += this.ReceiveCurrentLocationChanged;
            GraphicsEvents.OnPostRenderEvent += this.OnPostRenderEvent;
        }


        /*********
        ** Private methods
        *********/
        /****
        ** Event handlers
        ****/
        /// <summary>The method invoked when the player loads the game.</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void ReceiveGameLoaded(object sender, EventArgs e)
        {
            // check for an updated version
            if (this.Config.CheckForUpdates)
            {
                Task.Factory.StartNew(() =>
                {
                    UpdateHelper.LogVersionCheck(this.Monitor, this.ModManifest.Version, "DebugMode").Wait();
                });
            }
        }

        /// <summary>The event called by SMAPI when rendering to the screen.</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        public void OnPostRenderEvent(object sender, EventArgs e)
        {
            if (this.DebugMode)
                this.DrawOverlay(Game1.spriteBatch, Game1.smallFont, this.Pixel.Value);
        }

        /// <summary>The method invoked when the player presses a keyboard button.</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event data.</param>
        private void ReceiveKeyPress(object sender, EventArgsKeyPressed e)
        {
            // handle hotkey
            this.HandleInput(e.KeyPressed, this.Config.Keyboard);

            // suppress dangerous actions
            if (this.DebugMode && !this.Config.AllowDangerousCommands)
                this.SuppressKeyIfDangerous(e.KeyPressed);
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

        /// <summary>The method invoked when the player warps into a new location.</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event data.</param>
        private void ReceiveCurrentLocationChanged(object sender, EventArgsCurrentLocationChanged e)
        {
            if (this.DebugMode)
                this.CorrectEntryPosition(e.NewLocation, Game1.player);
        }

        /****
        ** Methods
        ****/
        /// <summary>Suppress the specified key if it's considered dangerous (see <see cref="ModConfig.AllowDangerousCommands"/>).</summary>
        /// <param name="key">The pressed key to suppress.</param>
        private void SuppressKeyIfDangerous(Keys key)
        {
            if (this.DestructiveKeys.Contains(key))
            {
                Keys[] pressedKeys = Game1.oldKBState.GetPressedKeys().Union(new[] { key }).ToArray();
                Game1.oldKBState = new KeyboardState(pressedKeys);
            }
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

        /// <summary>Correct the player's position when they warp into an area.</summary>
        /// <param name="location">The location the player entered.</param>
        /// <param name="player">The player who just warped.</param>
        private void CorrectEntryPosition(GameLocation location, Farmer player)
        {
            switch (location.Name)
            {
                // desert (move from inside wall to natural entry point)
                case "SandyHouse":
                    this.MovePlayerFrom(player, new Vector2(16, 3), new Vector2(4, 9), PlayerDirection.Up);
                    break;

                // mountain (move down a bit to natural entry point)
                case "Mountain":
                    this.MovePlayerFrom(player, new Vector2(15, 35), new Vector2(15, 40), PlayerDirection.Up);
                    break;

                // town (move from middle of field near community center to path between town and community center)
                case "Town":
                    this.MovePlayerFrom(player, new Vector2(35, 35), new Vector2(48, 43), PlayerDirection.Up);
                    break;
            }
        }

        /// <summary>Move the player from one tile to another, if they're on that tile.</summary>
        /// <param name="player">The player to move.</param>
        /// <param name="fromTile">The tile position from which to move the player.</param>
        /// <param name="toTile">The tile position to which to move the player.</param>
        /// <param name="facingDirection">The direction the player should be facing after they're moved.</param>
        private void MovePlayerFrom(Farmer player, Vector2 fromTile, Vector2 toTile, PlayerDirection facingDirection)
        {
            if (player.getTileX() == (int)fromTile.X && player.getTileY() == (int)fromTile.Y)
            {
                player.Position = new Vector2(toTile.X * Game1.tileSize, toTile.Y * Game1.tileSize);
                player.facingDirection = (int)facingDirection;
                player.setMovingInFacingDirection();
            }
        }

        /// <summary>Create a pixel texture that can be stretched and colourised for display.</summary>
        private static Texture2D CreatePixel()
        {
            Texture2D texture = new Texture2D(Game1.graphics.GraphicsDevice, 1, 1);
            texture.SetData(new[] { Color.White });
            return texture;
        }

        /// <summary>Draw the debug overlay to the screen.</summary>
        /// <param name="batch">The sprite batch being drawn.</param>
        /// <param name="font">The font with which to render text.</param>
        /// <param name="pixel">A pixel texture that can be stretched and colourised for display.</param>
        private void DrawOverlay(SpriteBatch batch, SpriteFont font, Texture2D pixel)
        {
            // draw debug info at cursor position
            {
                // get cursor position
                Vector2 tile = Game1.currentCursorTile;
                Vector2 position = new Vector2(Game1.getOldMouseX(), Game1.getOldMouseY());

                // generate debug text
                GameLocation location = Game1.currentLocation;
                Type menuType = Game1.activeClickableMenu?.GetType();
                string[] lines = {
                    location != null ? $"tile: {tile.X}, {tile.Y}" : null,
                    location != null ? $"map:  {location.Name}" : null,
                    menuType != null ? $"menu: {(menuType.Namespace == typeof(TitleMenu).Namespace ? menuType.Name : menuType.FullName)}" : null
                };

                // draw text
                string text = string.Join(Environment.NewLine, lines.Where(p => p != null));
                Vector2 textSize = font.MeasureString(text);
                batch.DrawString(font, text, new Vector2(position.X - textSize.X, position.Y), Color.Red);
            }

            // draw cursor crosshairs
            batch.Draw(pixel, new Rectangle(0, Game1.getOldMouseY() - 1, Game1.viewport.Width, 3), Color.Black * 0.5f);
            batch.Draw(pixel, new Rectangle(Game1.getOldMouseX() - 1, 0, 3, Game1.viewport.Height), Color.Black * 0.5f);
        }
    }
}
