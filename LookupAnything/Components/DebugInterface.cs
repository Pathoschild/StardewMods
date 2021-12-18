using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Pathoschild.Stardew.Common;
using Pathoschild.Stardew.LookupAnything.Framework;
using Pathoschild.Stardew.LookupAnything.Framework.Lookups;
using StardewModdingAPI;
using StardewValley;

namespace Pathoschild.Stardew.LookupAnything.Components
{
    /// <summary>Draws debug information to the screen.</summary>
    internal class DebugInterface
    {
        /*********
        ** Fields
        *********/
        /// <summary>Provides utility methods for interacting with the game code.</summary>
        private readonly GameHelper GameHelper;

        /// <summary>Finds and analyzes lookup targets in the world.</summary>
        private readonly TargetFactory TargetFactory;

        /// <summary>Encapsulates monitoring and logging.</summary>
        private readonly IMonitor Monitor;

        /// <summary>The mod configuration.</summary>
        private readonly Func<ModConfig> Config;


        /*********
        ** Accessors
        *********/
        /// <summary>Whether the debug interface is enabled.</summary>
        public bool Enabled { get; set; }


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="gameHelper">Provides utility methods for interacting with the game code.</param>
        /// <param name="targetFactory">Finds and analyzes lookup targets in the world.</param>
        /// <param name="config">The mod configuration.</param>
        /// <param name="monitor">Encapsulates monitoring and logging.</param>
        public DebugInterface(GameHelper gameHelper, TargetFactory targetFactory, Func<ModConfig> config, IMonitor monitor)
        {
            this.GameHelper = gameHelper;
            this.TargetFactory = targetFactory;
            this.Config = config;
            this.Monitor = monitor;
        }

        /// <summary>Draw debug metadata to the screen.</summary>
        /// <param name="spriteBatch">The sprite batch being drawn.</param>
        public void Draw(SpriteBatch spriteBatch)
        {
            if (!this.Enabled)
                return;

            this.Monitor.InterceptErrors("drawing debug info", () =>
            {
                var config = this.Config();

                // get location info
                GameLocation currentLocation = Game1.currentLocation;
                Vector2 cursorTile = Game1.currentCursorTile;
                Vector2 cursorPosition = this.GameHelper.GetScreenCoordinatesFromCursor();

                // show 'debug enabled' warning + cursor position
                {
                    string metadata = $"Debug info enabled; press {string.Join(" or ", config.Controls.ToggleDebug)} to disable. Cursor tile ({cursorTile.X}, {cursorTile.Y}), position ({cursorPosition.X}, {cursorPosition.Y}).";
                    this.GameHelper.DrawHoverBox(spriteBatch, metadata, Vector2.Zero, Game1.uiViewport.Width);
                }

                // show cursor pixel
                spriteBatch.DrawLine(cursorPosition.X - 1, cursorPosition.Y - 1, new Vector2(Game1.pixelZoom, Game1.pixelZoom), Color.DarkRed);

                // show targets within detection radius
                Rectangle tileArea = this.GameHelper.GetScreenCoordinatesFromTile(Game1.currentCursorTile);
                IEnumerable<ITarget> targets = this.TargetFactory.GetNearbyTargets(currentLocation, cursorTile).Where(p => p.Type != SubjectType.Tile);
                // if targets overlap, prioritize info on known targets
                foreach (ITarget target in targets)
                {
                    // get metadata
                    bool spriteAreaIntersects = target.GetWorldArea().Intersects(tileArea);
                    ISubject subject = target.GetSubject();

                    // draw tile
                    {
                        Rectangle tile = this.GameHelper.GetScreenCoordinatesFromTile(target.Tile);
                        Color color = (subject != null ? Color.Green : Color.Red) * .5f;
                        spriteBatch.DrawLine(tile.X, tile.Y, new Vector2(tile.Width, tile.Height), color);
                    }

                    // draw sprite box
                    if (subject != null)
                    {
                        int borderSize = 3;
                        Color borderColor = Color.Green;
                        if (!spriteAreaIntersects)
                        {
                            borderSize = 1;
                            borderColor *= 0.5f;
                        }

                        Rectangle spriteBox = target.GetWorldArea();
                        spriteBatch.DrawLine(spriteBox.X, spriteBox.Y, new Vector2(spriteBox.Width, borderSize), borderColor); // top
                        spriteBatch.DrawLine(spriteBox.X, spriteBox.Y, new Vector2(borderSize, spriteBox.Height), borderColor); // left
                        spriteBatch.DrawLine(spriteBox.X + spriteBox.Width, spriteBox.Y, new Vector2(borderSize, spriteBox.Height), borderColor); // right
                        spriteBatch.DrawLine(spriteBox.X, spriteBox.Y + spriteBox.Height, new Vector2(spriteBox.Width, borderSize), borderColor); // bottom
                    }
                }

                // show current target name (if any)
                {
                    ISubject subject = this.TargetFactory.GetSubjectFrom(Game1.player, currentLocation, hasCursor: Game1.wasMouseVisibleThisFrame);
                    if (subject != null)
                        this.GameHelper.DrawHoverBox(spriteBatch, subject.Name, new Vector2(Game1.getMouseX(), Game1.getMouseY()) + new Vector2(Game1.tileSize / 2f), Game1.uiViewport.Width / 4f);
                }
            }, this.OnDrawError);
        }


        /*********
        ** Public methods
        *********/
        /// <summary>The method invoked when an unhandled exception is intercepted.</summary>
        /// <param name="ex">The intercepted exception.</param>
        private void OnDrawError(Exception ex)
        {
            this.Monitor.InterceptErrors("handling an error in the debug code", () =>
            {
                this.Enabled = false;
            });
        }
    }
}
