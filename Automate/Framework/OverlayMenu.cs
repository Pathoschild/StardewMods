using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Pathoschild.Stardew.Common;
using StardewValley;
using StardewValley.Menus;

namespace Pathoschild.Stardew.Automate.Framework
{
    /// <summary>The overlay which highlights automatable machines.</summary>
    internal class OverlayMenu : IClickableMenu
    {
        /*********
        ** Properties
        *********/
        /// <summary>The amount by which to pan the screen slowly.</summary>
        private readonly int SlowPanAmount = 4;

        /// <summary>The amount by which to pan the screen quickly.</summary>
        private readonly int FastPanAmount = 8;

        /// <summary>The padding to apply to tile backgrounds to make the grid visible.</summary>
        private readonly int TileGap = 1;

        /// <summary>A machine group lookup by tile coordinate.</summary>
        private readonly IDictionary<Vector2, MachineGroup> GroupTiles;


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="machineGroups">The machine groups to display.</param>
        public OverlayMenu(IEnumerable<MachineGroup> machineGroups)
        {
            // init machine groups
            machineGroups = machineGroups.ToArray();
            this.GroupTiles =
                (
                    from machineGroup in machineGroups
                    from tile in machineGroup.Tiles
                    select new { tile, machineGroup }
                )
                .ToDictionary(p => p.tile, p => p.machineGroup);

            // set up viewport
            this.exitFunction = this.ReleaseViewport;
            this.TakeViewport();
        }

        /// <summary>Draw the overlay to the screen.</summary>
        /// <param name="spriteBatch">The sprite batch being drawn.</param>
        [SuppressMessage("ReSharper", "PossibleLossOfFraction", Justification = "Deliberate discarded for conversion to tile coordinates.")]
        public override void draw(SpriteBatch spriteBatch)
        {
            // get on-screen tiles
            IEnumerable<Vector2> visibleTiles = TileHelper.GetTiles(
                x: Game1.viewport.X / Game1.tileSize,
                y: Game1.viewport.Y / Game1.tileSize,
                width: (int)(Game1.viewport.Width / (decimal)Game1.tileSize) + 2, // extend off-screen slightly to avoid overlay edges being visible
                height: (int)(Game1.viewport.Height / (decimal)Game1.tileSize) + 2
            );

            // draw each tile
            foreach (Vector2 tile in visibleTiles)
            {
                // get tile's screen coordinates
                float screenX = tile.X * Game1.tileSize - Game1.viewport.X;
                float screenY = tile.Y * Game1.tileSize - Game1.viewport.Y;
                int tileSize = Game1.tileSize;

                // get machine group
                this.GroupTiles.TryGetValue(tile, out MachineGroup group);
                bool isGrouped = group != null;
                bool isActive = isGrouped && group.HasInternalAutomation;

                // draw background
                {
                    Color color = Color.Black * 0.5f;
                    if (isActive)
                        color = Color.Green * 0.2f;
                    else if (isGrouped)
                        color = Color.Red * 0.2f;

                    spriteBatch.DrawLine(screenX + this.TileGap, screenY + this.TileGap, new Vector2(tileSize - this.TileGap * 2, tileSize - this.TileGap * 2), color);
                }

                // draw group edge borders
                if (group != null)
                    this.DrawEdgeBorders(spriteBatch, group, tile, group.HasInternalAutomation ? Color.Green : Color.Red);
            }

            // draw cursor
            this.drawMouse(spriteBatch);
        }

        /// <summary>Handle keyboard input from the player.</summary>
        /// <param name="key">The key that was pressed.</param>
        public override void receiveKeyPress(Keys key)
        {
            base.receiveKeyPress(key);

            // move camera using directional keys
            if (Game1.options.SnappyMenus)
                return;

            if (Game1.options.doesInputListContain(Game1.options.moveDownButton, key))
                this.PanScreen(0, this.SlowPanAmount);
            else if (Game1.options.doesInputListContain(Game1.options.moveRightButton, key))
                this.PanScreen(this.SlowPanAmount, 0);
            else if (Game1.options.doesInputListContain(Game1.options.moveUpButton, key))
                this.PanScreen(0, -this.SlowPanAmount);
            else if (Game1.options.doesInputListContain(Game1.options.moveLeftButton, key))
                this.PanScreen(-this.SlowPanAmount, 0);
        }

        /// <summary>Update the menu when the game state changes.</summary>
        /// <param name="time">The current game time.</param>
        public override void update(GameTime time)
        {
            base.update(time);

            // pan camera using mouse edge scrolling
            int mouseX = Game1.getOldMouseX() + Game1.viewport.X;
            int mouseY = Game1.getOldMouseY() + Game1.viewport.Y;

            if (mouseX - Game1.viewport.X < Game1.tileSize)
                this.PanScreen(-this.FastPanAmount, 0);
            else if (mouseX - (Game1.viewport.X + Game1.viewport.Width) >= -Game1.tileSize * 2)
                this.PanScreen(this.FastPanAmount, 0);
            if (mouseY - Game1.viewport.Y < Game1.tileSize)
                this.PanScreen(0, -this.FastPanAmount);
            else if (mouseY - (Game1.viewport.Y + Game1.viewport.Height) >= -Game1.tileSize)
                this.PanScreen(0, this.FastPanAmount);

            foreach (Keys pressedKey in Game1.oldKBState.GetPressedKeys())
                this.receiveKeyPress(pressedKey);
        }

        /// <summary>The method invoked when the player right-clicks on the menu.</summary>
        /// <param name="x">The X-position of the cursor.</param>
        /// <param name="y">The Y-position of the cursor.</param>
        /// <param name="playSound">Whether to enable sound.</param>
        public override void receiveRightClick(int x, int y, bool playSound = true) { }


        /*********
        ** Private methods
        *********/
        /// <summary>The method called to update the camera's position.</summary>
        /// <param name="x">Pans the camera by adding to the X coordinate.</param>
        /// <param name="y">Pans the camera by adding to the Y coordinate.</param>
        private void PanScreen(int x, int y)
        {
            Game1.previousViewportPosition.X = Game1.viewport.Location.X;
            Game1.previousViewportPosition.Y = Game1.viewport.Location.Y;
            Game1.viewport.X += x;
            Game1.viewport.Y += y;

            bool shouldClamp =
                Game1.currentLocation.IsOutdoors
                || Game1.currentLocation.Name.Contains("Sewer")
                || Game1.currentLocation.Name.Contains("BugLand")
                || Game1.currentLocation.Name.Contains("WitchSwamp")
                || Game1.currentLocation.Name.Contains("UndergroundMine"); // Jumps the camera, might exclude in the future. Included here because the overlay has a gap at the top and left side
            if (shouldClamp)
                Game1.clampViewportToGameMap();

            Game1.updateRaindropPosition();
        }

        /// <summary>Take over the viewport for the overlay.</summary>
        private void TakeViewport()
        {
            Game1.viewportFreeze = true;
            Game1.displayHUD = false;
        }

        /// <summary>Release the viewport and reset it to normal.</summary>
        private void ReleaseViewport()
        {
            Game1.viewportFreeze = false;
            Game1.displayHUD = true;
        }

        /// <summary>Draw borders for each unconnected edge of a tile.</summary>
        /// <param name="spriteBatch">The sprite batch being drawn.</param>
        /// <param name="group">The machine group.</param>
        /// <param name="tile">The group tile.</param>
        /// <param name="color">The border color.</param>
        private void DrawEdgeBorders(SpriteBatch spriteBatch, MachineGroup group, Vector2 tile, Color color)
        {
            int borderSize = 3;
            float screenX = tile.X * Game1.tileSize - Game1.viewport.X;
            float screenY = tile.Y * Game1.tileSize - Game1.viewport.Y;
            float tileSize = Game1.tileSize;

            // top
            if (!group.Tiles.Contains(new Vector2(tile.X, tile.Y - 1)))
                spriteBatch.DrawLine(screenX, screenY, new Vector2(tileSize, borderSize), color); // top

            // bottom
            if (!group.Tiles.Contains(new Vector2(tile.X, tile.Y + 1)))
                spriteBatch.DrawLine(screenX, screenY + tileSize, new Vector2(tileSize, borderSize), color); // bottom

            // left
            if (!group.Tiles.Contains(new Vector2(tile.X - 1, tile.Y)))
                spriteBatch.DrawLine(screenX, screenY, new Vector2(borderSize, tileSize), color); // left

            // right
            if (!group.Tiles.Contains(new Vector2(tile.X + 1, tile.Y)))
                spriteBatch.DrawLine(screenX + tileSize, screenY, new Vector2(borderSize, tileSize), color); // right
        }
    }
}
