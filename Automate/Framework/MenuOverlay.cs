using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Pathoschild.Stardew.Automate.Framework.Models;
using Pathoschild.Stardew.Common;
using StardewValley;
using StardewValley.Menus;
using StardewValley.Objects;

namespace Pathoschild.Stardew.Automate.Framework
{
    /// <summary>The overlay which highlights automatable machines.</summary>
    internal class MenuOverlay : IClickableMenu
    {
        /*********
        ** Properties
        *********/
        /// <summary>The tiles containing a machine and whether it's connected to anything.</summary>
        private readonly IDictionary<Vector2, bool> MachineTileConnections;

        /// <summary>The tiles containing a chest and whether it's connected to anything.</summary>
        private readonly IDictionary<Vector2, bool> ChestTileConnections;

        /// <summary>The amount by which to pan the screen slowly.</summary>
        private readonly int SlowPanAmount = 4;

        /// <summary>The amount by which to pan the screen quickly.</summary>
        private readonly int FastPanAmount = 8;

        /// <summary>The machine/storage groups that operate as single units.</summary>
        private readonly ISet<GroupData> Groups;

        /// <summary>The save-group button to render.</summary>
        private readonly ClickableTextureComponent SaveButton;

        /// <summary>The delete-group button to render.</summary>
        private readonly ClickableTextureComponent DeleteButton;

        /// <summary>The save button to render.</summary>
        private readonly ClickableTextureComponent ConnectButton;

        /// <summary>The group being edited.</summary>
        private GroupData EditingGroup;

        /// <summary>The text to show in a tooltip.</summary>
        private string HoverText;


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="machines">The machines to manage in the overlay.</param>
        /// <param name="groups">The machine/storage groups that operate as single units.</param>
        public MenuOverlay(IEnumerable<MachineMetadata> machines, ISet<GroupData> groups)
        {
            // init
            machines = machines.ToArray();
            this.MachineTileConnections = this.GetMachineTileConnections(machines);
            this.ChestTileConnections = this.GetChestTileConnections(machines);
            this.Groups = groups;

            // init buttons
            Rectangle okSprite = Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, 46);
            this.SaveButton = new ClickableTextureComponent("OK", new Rectangle(Game1.viewport.Width - Game1.tileSize * 2, Game1.viewport.Height - Game1.tileSize * 2, Game1.tileSize, Game1.tileSize), null, "Save Group", Game1.mouseCursors, okSprite, 1f);

            Rectangle cancelSprite = Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, 47);
            this.DeleteButton = new ClickableTextureComponent("OK", new Rectangle(Game1.viewport.Width - Game1.tileSize * 3, Game1.viewport.Height - Game1.tileSize * 2, Game1.tileSize, Game1.tileSize), null, "Delete Group", Game1.mouseCursors, cancelSprite, 1f);

            Rectangle buildSprite = new Rectangle(366, 373, 16, 16);
            this.ConnectButton = new ClickableTextureComponent("OK", new Rectangle(Game1.viewport.Width - Game1.tileSize * 2, Game1.tileSize, Game1.tileSize, Game1.tileSize), null, "Save Group", Game1.mouseCursors, buildSprite, Game1.pixelZoom);

            // set up viewport
            this.exitFunction = this.ReleaseViewport;
            this.TakeViewport();
        }

        /// <summary>Handle the cursor hovering over the given coordinates.</summary>
        /// <param name="x">The X coordinate of the cursor on the screen.</param>
        /// <param name="y">The Y coordinate of the cursor on the screen.</param>
        public override void performHoverAction(int x, int y)
        {
            base.performHoverAction(x, y);

            this.SaveButton.tryHover(x, y);
            this.DeleteButton.tryHover(x, y);
            this.ConnectButton.tryHover(x, y);
            if (this.SaveButton.containsPoint(x, y))
                this.HoverText = "Save Group";
            else if (this.DeleteButton.containsPoint(x, y))
                this.HoverText = "Delete Group";
            else if (this.ConnectButton.containsPoint(x, y))
                this.HoverText = "Connect Group";
            else
                this.HoverText = null;
        }

        /// <summary>Draw the overlay to the screen.</summary>
        /// <param name="spriteBatch">The sprite batch being drawn.</param>
        [SuppressMessage("ReSharper", "PossibleLossOfFraction", Justification = "Deliberate discarded for conversion to tile coordinates.")]
        public override void draw(SpriteBatch spriteBatch)
        {
            int minX = Game1.viewport.X / Game1.tileSize;
            int minY = Game1.viewport.Y / Game1.tileSize;
            int maxX = (int)Math.Ceiling((Game1.viewport.X + Game1.viewport.Width) / (decimal)Game1.tileSize);
            int maxY = (int)Math.Ceiling((Game1.viewport.Y + Game1.viewport.Height) / (decimal)Game1.tileSize);

            // draw tile overlays
            for (int x = minX; x < maxX; x++)
            {
                for (int y = minY; y < maxY; y++)
                {
                    Vector2 tile = new Vector2(x, y);
                    Rectangle screenArea = new Rectangle(x * Game1.tileSize - Game1.viewport.X, y * Game1.tileSize - Game1.viewport.Y, Game1.tileSize, Game1.tileSize);

                    // get tile color
                    Color color = Color.Black * 0.5f;
                    bool selected = this.EditingGroup?.Tiles.Contains(tile) == true;
                    if (this.MachineTileConnections.TryGetValue(tile, out bool hasPipe))
                        color = this.GetTileColor(hasPipe, selected);
                    else if (this.ChestTileConnections.TryGetValue(tile, out bool hasConnection))
                        color = this.GetTileColor(hasConnection, selected);

                    // draw background
                    spriteBatch.DrawLine(screenArea.X, screenArea.Y, new Vector2(screenArea.Width, screenArea.Height), color);

                    // draw tile border
                    int borderSize = 5;
                    Color borderColor = color * 0.75f;
                    spriteBatch.DrawLine(screenArea.X, screenArea.Y, new Vector2(screenArea.Width, borderSize), borderColor); // top
                    spriteBatch.DrawLine(screenArea.X, screenArea.Y, new Vector2(borderSize, screenArea.Height), borderColor); // left
                    spriteBatch.DrawLine(screenArea.X + screenArea.Width, screenArea.Y, new Vector2(borderSize, screenArea.Height), borderColor); // right
                    spriteBatch.DrawLine(screenArea.X, screenArea.Y + screenArea.Height, new Vector2(screenArea.Width, borderSize), borderColor); // bottom
                }
            }

            // draw group borders
            foreach (GroupData group in this.Groups)
                this.DrawGroupBorders(spriteBatch, group);
            if (this.EditingGroup != null)
                this.DrawGroupBorders(spriteBatch, this.EditingGroup, Color.Green);


            // draw edit buttons
            if (this.EditingGroup != null && this.EditingGroup.Tiles.Any())
            {
                this.SaveButton.bounds.X = ((int)this.EditingGroup.Tiles.Max(c => c.X) + 1) * Game1.tileSize - Game1.viewport.X;
                this.SaveButton.bounds.Y = ((int)this.EditingGroup.Tiles.Max(c => c.Y) + 1) * Game1.tileSize - Game1.viewport.Y;
                this.DeleteButton.bounds.X = ((int)this.EditingGroup.Tiles.Max(c => c.X) + 2) * Game1.tileSize - Game1.viewport.X;
                this.DeleteButton.bounds.Y = ((int)this.EditingGroup.Tiles.Max(c => c.Y) + 1) * Game1.tileSize - Game1.viewport.Y;

                this.SaveButton.draw(spriteBatch);
                this.DeleteButton.draw(spriteBatch);
            }
            this.ConnectButton.draw(spriteBatch);

            // draw cursor
            this.drawMouse(spriteBatch);

            // draw tooltip
            if (this.HoverText != null)
                IClickableMenu.drawHoverText(spriteBatch, this.HoverText, Game1.dialogueFont);
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

        /// <summary>The method invoked when the player left-clicks on the menu.</summary>
        /// <param name="x">The X-position of the cursor.</param>
        /// <param name="y">The Y-position of the cursor.</param>
        /// <param name="playSound">Whether to enable sound.</param>
        [SuppressMessage("ReSharper", "PossibleLossOfFraction", Justification = "Deliberate conversion to tile coordinates")]
        public override void receiveLeftClick(int x, int y, bool playSound = true)
        {
            bool isEditing = this.EditingGroup != null;

            // save click
            if (isEditing && this.SaveButton.containsPoint(x, y))
            {
                if (!this.Groups.Contains(this.EditingGroup))
                    this.Groups.Add(this.EditingGroup);
                this.EditingGroup = null;
            }

            // delete click
            else if (isEditing && this.DeleteButton.containsPoint(x, y))
            {
                this.Groups.Remove(this.EditingGroup);
                this.EditingGroup = null;
            }

            // tile click
            else
            {
                Vector2 tile = new Vector2((x + Game1.viewport.X) / Game1.tileSize, (y + Game1.viewport.Y) / Game1.tileSize);

                // start editing group
                if (this.EditingGroup == null)
                {
                    this.EditingGroup = this.Groups.FirstOrDefault(p => p.Tiles.Contains(tile)) ?? new GroupData();
                    this.EditingGroup.Add(tile);
                }

                // toggle tile in group
                else
                {
                    bool isTileReserved = this.Groups.Any(p => p != this.EditingGroup && p.Tiles.Contains(tile));
                    if (!isTileReserved)
                        this.EditingGroup?.AddOrRemove(tile);
                }
            }

        }

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

        /// <summary>Get the color to apply to a tile.</summary>
        /// <param name="hasConnection">Whether the tile is connected to anything.</param>
        /// <param name="isSelected">Whether the tile is selected.</param>
        private Color GetTileColor(bool hasConnection, bool isSelected)
        {
            if (isSelected)
                return Color.Orange * 0.2f;

            return hasConnection
                ? Color.Green * 0.2f
                : Color.Red * 0.2f;
        }

        /// <summary>Get all tile positions containing a machine and whether it's connected to anything.</summary>
        /// <param name="machines">The machines to search.</param>
        private IDictionary<Vector2, bool> GetMachineTileConnections(IEnumerable<MachineMetadata> machines)
        {
            IDictionary<Vector2, bool> found = new Dictionary<Vector2, bool>();
            foreach (MachineMetadata machine in machines)
            {
                if (!machine.Connected.Any())
                    continue;

                Rectangle bounds = machine.TileBounds;
                for (int tileX = 0; tileX < bounds.Width; tileX++)
                {
                    for (int tileY = 0; tileY < bounds.Height; tileY++)
                    {
                        Vector2 tile = new Vector2(bounds.X + tileX, bounds.Y + tileY);
                        found[tile] = machine.Connected.Any();
                    }
                }
            }
            return found;
        }

        /// <summary>Get all chest tile positions connected to a machine.</summary>
        /// <param name="machines">The machines to search.</param>
        private IDictionary<Vector2, bool> GetChestTileConnections(IEnumerable<MachineMetadata> machines)
        {
            // get connected chests
            IDictionary<Vector2, bool> found =
                (
                    from machine in machines
                    where machine.Connected.Any()
                    from pipe in machine.Connected
                    select pipe.Endpoint.GetSourceTile()
                )
                .Distinct()
                .ToDictionary(tile => tile, tile => true);

            // get unconnected chests
            foreach (var pair in Game1.currentLocation.objects)
            {
                if (pair.Value is Chest && !found.ContainsKey(pair.Key))
                    found[pair.Key] = false;
            }

            return found;
        }

        /// <summary>Draw the borders around a group's tiles.</summary>
        /// <param name="spriteBatch">The sprite batch being drawn.</param>
        /// <param name="group">The group to border.</param>
        /// <param name="color">The border color (or <c>null</c> for default).</param>
        private void DrawGroupBorders(SpriteBatch spriteBatch, GroupData group, Color? color = null)
        {
            foreach (Vector2 tile in group.Tiles)
            {
                int borderSize = 1;
                Color borderColor = color ?? (Color.White * 0.75f);
                Rectangle screenArea = new Rectangle((int)tile.X * Game1.tileSize - Game1.viewport.X, (int)tile.Y * Game1.tileSize - Game1.viewport.Y, Game1.tileSize, Game1.tileSize);

                // top
                if (!group.Tiles.Contains(new Vector2(tile.X, tile.Y - 1)))
                    spriteBatch.DrawLine(screenArea.X, screenArea.Y, new Vector2(screenArea.Width, borderSize), borderColor); // top

                // bottom
                if (!group.Tiles.Contains(new Vector2(tile.X, tile.Y + 1)))
                    spriteBatch.DrawLine(screenArea.X, screenArea.Y + screenArea.Height, new Vector2(screenArea.Width, borderSize), borderColor); // bottom

                // left
                if (!group.Tiles.Contains(new Vector2(tile.X - 1, tile.Y)))
                    spriteBatch.DrawLine(screenArea.X, screenArea.Y, new Vector2(borderSize, screenArea.Height), borderColor); // left

                // right
                if (!group.Tiles.Contains(new Vector2(tile.X + 1, tile.Y)))
                    spriteBatch.DrawLine(screenArea.X + screenArea.Width, screenArea.Y, new Vector2(borderSize, screenArea.Height), borderColor); // right
            }
        }
    }
}
