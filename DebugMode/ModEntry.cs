using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Pathoschild.Stardew.Common;
using Pathoschild.Stardew.DebugMode.Framework;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Menus;
using StardewValley.Minigames;

namespace Pathoschild.Stardew.DebugMode
{
    /// <summary>The mod entry point.</summary>
    internal class ModEntry : Mod
    {
        /*********
        ** Fields
        *********/
        /// <summary>The mod configuration settings.</summary>
        private ModConfig Config;

        /// <summary>The configured key bindings.</summary>
        private ModConfigKeys Keys;

        /// <summary>Whether to show the debug info overlay.</summary>
        private bool ShowOverlay;

        /// <summary>Whether the built-in debug mode is enabled.</summary>
        private bool GameDebugMode
        {
            get
            {
                return Game1.debugMode;
            }
            set
            {
                Game1.debugMode = value;
                Program.releaseBuild = !value;
            }
        }

        /// <summary>A pixel texture that can be stretched and colorized for display.</summary>
        private readonly Lazy<Texture2D> Pixel = new Lazy<Texture2D>(ModEntry.CreatePixel);

        /// <summary>Keyboard keys which are mapped to a destructive action in debug mode. See <see cref="ModConfig.AllowDangerousCommands"/>.</summary>
        private readonly SButton[] DestructiveKeys =
        {
            SButton.P, // ends current day
            SButton.M, // ends current season
            SButton.H, // randomizes player's hat
            SButton.I, // randomizes player's hair
            SButton.J, // randomizes player's shirt and pants
            SButton.L, // randomizes player
            SButton.U, // randomizes farmhouse wallpaper and floors
            SButton.F10 // tries to launch a multiplayer server and crashes
        };


        /*********
        ** Public methods
        *********/
        /// <summary>The mod entry point, called after the mod is first loaded.</summary>
        /// <param name="helper">Provides methods for interacting with the mod directory, such as read/writing a config file or custom JSON files.</param>
        public override void Entry(IModHelper helper)
        {
            // init
            I18n.Init(helper.Translation);
            this.Config = helper.ReadConfig<ModConfig>();
            this.Config.AllowDangerousCommands = this.Config.AllowGameDebug && this.Config.AllowDangerousCommands; // normalize for convenience
            this.Keys = this.Config.Controls.ParseControls(helper.Input, this.Monitor);

            // hook events
            helper.Events.Input.ButtonPressed += this.OnButtonPressed;
            helper.Events.Display.Rendered += this.OnRendered;
            if (this.Config.AllowGameDebug)
                helper.Events.Player.Warped += this.OnWarped;

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
        /// <summary>The method invoked when the player presses a keyboard button.</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event data.</param>
        private void OnButtonPressed(object sender, ButtonPressedEventArgs e)
        {
            // toggle debug menu
            if (this.Keys.ToggleDebug.JustPressedUnique())
            {
                this.ShowOverlay = !this.ShowOverlay;
                if (this.Config.AllowGameDebug)
                    this.GameDebugMode = !this.GameDebugMode;
            }

            // suppress dangerous actions
            if (this.GameDebugMode && !this.Config.AllowDangerousCommands && this.DestructiveKeys.Contains(e.Button))
                this.Helper.Input.Suppress(e.Button);
        }

        /// <summary>The method invoked when the player warps into a new location.</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event data.</param>
        private void OnWarped(object sender, WarpedEventArgs e)
        {
            if (this.GameDebugMode && e.IsLocalPlayer)
                this.CorrectEntryPosition(e.NewLocation, Game1.player);
        }

        /// <summary>The event called by SMAPI when rendering to the screen.</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        public void OnRendered(object sender, RenderedEventArgs e)
        {
            if (this.ShowOverlay)
                this.DrawOverlay(Game1.spriteBatch, Game1.smallFont, this.Pixel.Value);
        }

        /****
        ** Methods
        ****/
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
                player.FacingDirection = (int)facingDirection;
                player.setMovingInFacingDirection();
            }
        }

        /// <summary>Create a pixel texture that can be stretched and colorized for display.</summary>
        private static Texture2D CreatePixel()
        {
            Texture2D texture = new Texture2D(Game1.graphics.GraphicsDevice, 1, 1);
            texture.SetData(new[] { Color.White });
            return texture;
        }

        /// <summary>Draw the debug overlay to the screen.</summary>
        /// <param name="batch">The sprite batch being drawn.</param>
        /// <param name="font">The font with which to render text.</param>
        /// <param name="pixel">A pixel texture that can be stretched and colorized for display.</param>
        private void DrawOverlay(SpriteBatch batch, SpriteFont font, Texture2D pixel)
        {
            var viewport = Game1.uiMode ? Game1.uiViewport : Game1.viewport;
            int mouseX = Game1.getMouseX();
            int mouseY = Game1.getMouseY();

            // draw debug info at cursor position
            {
                // generate debug text
                string[] lines = this.GetDebugInfo().ToArray();

                // get text
                string text = string.Join(Environment.NewLine, lines.Where(p => p != null));
                Vector2 textSize = font.MeasureString(text);
                const int scrollPadding = 5;

                // calculate scroll position
                int width = (int)(textSize.X + (scrollPadding * 2) + (CommonHelper.ScrollEdgeSize.X * 2));
                int height = (int)(textSize.Y + (scrollPadding * 2) + (CommonHelper.ScrollEdgeSize.Y * 2));
                int x = (int)MathHelper.Clamp(mouseX - width, 0, viewport.Width - width);
                int y = (int)MathHelper.Clamp(mouseY, 0, viewport.Height - height);

                // draw
                CommonHelper.DrawScroll(batch, new Vector2(x, y), textSize, out Vector2 contentPos, out Rectangle bounds, padding: scrollPadding);
                batch.DrawString(font, text, new Vector2(contentPos.X, contentPos.Y), Color.Black);
            }

            // draw cursor crosshairs
            batch.Draw(pixel, new Rectangle(0, mouseY - 1, viewport.Width, 3), Color.Black * 0.5f);
            batch.Draw(pixel, new Rectangle(mouseX - 1, 0, 3, viewport.Height), Color.Black * 0.5f);
        }

        /// <summary>Get debug info for the current context.</summary>
        private IEnumerable<string> GetDebugInfo()
        {
            // location
            if (Game1.currentLocation != null)
            {
                Vector2 tile = Game1.currentCursorTile;

                yield return $"{I18n.Label_Tile()}: {tile.X}, {tile.Y}";
                yield return $"{I18n.Label_Map()}:  {Game1.currentLocation.Name}";
            }

            // menu
            if (Game1.activeClickableMenu != null)
            {
                Type menuType = Game1.activeClickableMenu.GetType();
                Type submenuType = this.GetSubmenu(Game1.activeClickableMenu)?.GetType();
                string vanillaNamespace = typeof(TitleMenu).Namespace;

                yield return $"{I18n.Label_Menu()}: {(menuType.Namespace == vanillaNamespace ? menuType.Name : menuType.FullName)}";
                if (submenuType != null)
                    yield return $"{I18n.Label_Submenu()}: {(submenuType.Namespace == vanillaNamespace ? submenuType.Name : submenuType.FullName)}";
            }

            // minigame
            if (Game1.currentMinigame != null)
            {
                Type minigameType = Game1.currentMinigame.GetType();
                string vanillaNamespace = typeof(AbigailGame).Namespace;

                yield return $"{I18n.Label_Minigame()}: {(minigameType.Namespace == vanillaNamespace ? minigameType.Name : minigameType.FullName)}";
            }

            // event
            if (Game1.CurrentEvent != null)
            {
                Event @event = Game1.CurrentEvent;
                int eventID = this.Helper.Reflection.GetField<int>(@event, "id").GetValue();
                bool isFestival = @event.isFestival;
                string festivalName = @event.FestivalName;
                double progress = @event.CurrentCommand / (double)@event.eventCommands.Length;

                if (isFestival)
                    yield return $"{I18n.Label_FestivalName()}: {festivalName}";
                else
                {
                    yield return $"{I18n.Label_EventId()}: {eventID}";
                    if (@event.CurrentCommand >= 0 && @event.CurrentCommand < @event.eventCommands.Length)
                        yield return $"{I18n.Label_EventScript()}: {@event.eventCommands[@event.CurrentCommand]} ({(int)(progress * 100)}%)";
                }
            }

            // music
            if (Game1.currentSong?.Name != null && Game1.currentSong.IsPlaying)
                yield return $"{I18n.Label_Song()}: {Game1.currentSong.Name}";
        }

        /// <summary>Get the submenu for the current menu, if any.</summary>
        /// <param name="menu">The submenu.</param>
        private IClickableMenu GetSubmenu(IClickableMenu menu)
        {
            if (menu is GameMenu gameMenu)
                return gameMenu.pages[gameMenu.currentTab];
            if (menu is TitleMenu)
                return TitleMenu.subMenu;

            return null;
        }
    }
}
