using System;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Pathoschild.Stardew.DebugMode.Framework;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Menus;
using SFarmer = StardewValley.Farmer;

namespace Pathoschild.Stardew.DebugMode
{
    /// <summary>The mod entry point.</summary>
    internal class ModEntry : Mod
    {
        /*********
        ** Properties
        *********/
        /// <summary>The mod configuration settings.</summary>
        private ModConfig Config;

        /// <summary>Whether the built-in debug mode is enabled.</summary>
        private bool DebugMode
        {
            get => Game1.debugMode;
            set => Game1.debugMode = value;
        }

        /// <summary>A pixel texture that can be stretched and colourised for display.</summary>
        private readonly Lazy<Texture2D> Pixel = new Lazy<Texture2D>(ModEntry.CreatePixel);

        /// <summary>Keyboard keys which are mapped to a destructive action in debug mode. See <see cref="ModConfig.AllowDangerousCommands"/>.</summary>
        private readonly SButton[] DestructiveKeys =
        {
            SButton.P, // ends current day
            SButton.M, // ends current season
            SButton.H, // randomises player's hat
            SButton.I, // randomises player's hair
            SButton.J, // randomises player's shirt and pants
            SButton.L, // randomises player
            SButton.U, // randomises farmhouse wallpaper and floors
            SButton.F10 // tries to launch a multiplayer server and crashes
        };


        /*********
        ** Public methods
        *********/
        /// <summary>The mod entry point, called after the mod is first loaded.</summary>
        /// <param name="helper">Provides methods for interacting with the mod directory, such as read/writing a config file or custom JSON files.</param>
        public override void Entry(IModHelper helper)
        {
            // initialise
            this.Config = helper.ReadConfig<ModConfig>();

            // hook events
            InputEvents.ButtonPressed += this.InputEvents_ButtonPressed;
            LocationEvents.CurrentLocationChanged += this.LocationEvents_CurrentLocationChanged;
            GraphicsEvents.OnPostRenderEvent += this.GraphicsEvents_OnPostRenderEvent;

            // validate translations
            if (!helper.Translation.GetTranslations().Any())
                this.Monitor.Log("The translation files in this mod's i18n folder seem to be missing. The mod will still work, but you'll see 'missing translation' messages. Try reinstalling the mod to fix this.", LogLevel.Warn);
        }


        /*********
        ** Private methods
        *********/
        /****
        ** Event handlers
        ****/
        /// <summary>The event called by SMAPI when rendering to the screen.</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        public void GraphicsEvents_OnPostRenderEvent(object sender, EventArgs e)
        {
            if (this.DebugMode)
                this.DrawOverlay(Game1.spriteBatch, Game1.smallFont, this.Pixel.Value);
        }

        /// <summary>The method invoked when the player presses a keyboard button.</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event data.</param>
        private void InputEvents_ButtonPressed(object sender, EventArgsInput e)
        {
            // toggle debug menu
            if (this.Config.Controls.ToggleDebug.Contains(e.Button))
            {
                Program.releaseBuild = !Program.releaseBuild;
                this.DebugMode = !this.DebugMode;
            }

            // suppress dangerous actions
            if (this.DebugMode && !this.Config.AllowDangerousCommands && this.DestructiveKeys.Contains(e.Button))
                e.SuppressButton();
        }

        /// <summary>The method invoked when the player warps into a new location.</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event data.</param>
        private void LocationEvents_CurrentLocationChanged(object sender, EventArgsCurrentLocationChanged e)
        {
            if (this.DebugMode)
                this.CorrectEntryPosition(e.NewLocation, Game1.player);
        }

        /****
        ** Methods
        ****/
        /// <summary>Correct the player's position when they warp into an area.</summary>
        /// <param name="location">The location the player entered.</param>
        /// <param name="player">The player who just warped.</param>
        private void CorrectEntryPosition(GameLocation location, SFarmer player)
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
        private void MovePlayerFrom(SFarmer player, Vector2 fromTile, Vector2 toTile, PlayerDirection facingDirection)
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
                    location != null ? $"{this.Helper.Translation.Get("label.tile")}: {tile.X}, {tile.Y}" : null,
                    location != null ? $"{this.Helper.Translation.Get("label.map")}:  {location.Name}" : null,
                    menuType != null ? $"{this.Helper.Translation.Get("label.menu")}: {(menuType.Namespace == typeof(TitleMenu).Namespace ? menuType.Name : menuType.FullName)}" : null
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
