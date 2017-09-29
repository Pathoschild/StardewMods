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

        private List<HashSet<Vector2>> GroupsOfTiles;

        private HashSet<Vector2> ClickedTiles;

        private ClickableTextureComponent saveButton;

        private ClickableTextureComponent deleteButton;

        private string hoverText = "";


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="machines">The machines to manage in the overlay.</param>
        public MenuOverlay(IEnumerable<MachineMetadata> machines)
        {
            machines = machines.ToArray();
            this.MachineTileConnections = this.GetMachineTileConnections(machines);
            this.ChestTileConnections = this.GetChestTileConnections(machines);
            this.GroupsOfTiles = new List<HashSet<Vector2>>();
            this.ClickedTiles = new HashSet<Vector2>();
            SetUpButtons();

            this.exitFunction = this.ReleaseViewport;

            this.TakeViewport();
        }

        public void SetUpButtons()
        {
            Rectangle okXNBTilesheet = Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, 46, -1, -1);
            ClickableTextureComponent okTextureComponent = new ClickableTextureComponent("OK", new Microsoft.Xna.Framework.Rectangle(Game1.viewport.Width - Game1.tileSize * 2, Game1.viewport.Height - Game1.tileSize * 2, Game1.tileSize, Game1.tileSize), (string)null, "Save Group", Game1.mouseCursors, okXNBTilesheet, 1f, false);
            this.saveButton = okTextureComponent;

            Rectangle cancelXNBTileSheet = Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, 47, -1, -1);
            ClickableTextureComponent cancelTextureComponent = new ClickableTextureComponent("OK", new Microsoft.Xna.Framework.Rectangle(Game1.viewport.Width - Game1.tileSize * 3, Game1.viewport.Height - Game1.tileSize * 2, Game1.tileSize, Game1.tileSize), (string)null, "Delete Group", Game1.mouseCursors, cancelXNBTileSheet, 1f, false);
            this.deleteButton = cancelTextureComponent;
        }

        public override void performHoverAction(int x, int y)
        {
            this.saveButton.tryHover(x, y, 0.1f);
            this.deleteButton.tryHover(x, y, 0.1f);
            if (this.saveButton.containsPoint(x, y))
                this.hoverText = "Save Group";
            else if (this.deleteButton.containsPoint(x, y))
                this.hoverText = "Delete Group";
            else
                this.hoverText = "";
        }

        private Color ColorTiles(Color color, Vector2 tile, bool hasConnection, out bool isSelected)
        {
            if (this.ClickedTiles.Contains(tile))
            {
                isSelected = true;
                color = Color.Orange * 0.2f;
            }
            else
            {
                isSelected = false;
                if (hasConnection)
                    color = Color.Green * 0.2f;
                else
                    color = Color.Red * 0.2f;
            }

            return color;
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


            int borderSize = 5;

            for (int x = minX; x < maxX; x++)
            {
                for (int y = minY; y < maxY; y++)
                {
                    Vector2 tile = new Vector2(x, y);
                    Rectangle screenArea = new Rectangle(x * Game1.tileSize - Game1.viewport.X, y * Game1.tileSize - Game1.viewport.Y, Game1.tileSize, Game1.tileSize);

                    // get color coding
                    Color color = Color.Black * 0.5f;
                    bool selected = true;
                    if (this.MachineTileConnections.TryGetValue(tile, out bool hasPipe))
                    {
                        color = ColorTiles(color, tile, hasPipe, out bool isSelected);

                        selected = isSelected; // should this be is Not selected or something
                    }
                    else if (this.ChestTileConnections.TryGetValue(tile, out bool hasConnection))
                    {
                        color = ColorTiles(color, tile, hasConnection, out bool isSelected);

                        selected = isSelected;
                    }

                    // draw background
                    spriteBatch.DrawLine(screenArea.X, screenArea.Y, new Vector2(screenArea.Width, screenArea.Height), color);

                    // draw tile border if tile is not selected
                    if (!selected)
                    {
                        Color borderColor = color * 0.75f;
                        spriteBatch.DrawLine(screenArea.X, screenArea.Y, new Vector2(screenArea.Width, borderSize), borderColor); // top
                        spriteBatch.DrawLine(screenArea.X, screenArea.Y, new Vector2(borderSize, screenArea.Height), borderColor); // left
                        spriteBatch.DrawLine(screenArea.X + screenArea.Width, screenArea.Y, new Vector2(borderSize, screenArea.Height), borderColor); // right
                        spriteBatch.DrawLine(screenArea.X, screenArea.Y + screenArea.Height, new Vector2(screenArea.Width, borderSize), borderColor); // bottom
                    }
                }
            }

            if(this.GroupsOfTiles.Any())
            {
                foreach (HashSet<Vector2> groupedTiles in this.GroupsOfTiles)
                {
                    foreach (Vector2 tile in groupedTiles)
                    {
                        Color borderColor = Color.White * 0.75f;
                        Rectangle screenArea = new Rectangle((int)tile.X * Game1.tileSize - Game1.viewport.X, (int)tile.Y * Game1.tileSize - Game1.viewport.Y, Game1.tileSize, Game1.tileSize);

                        //get surrounding corner
                        float left = tile.X - 1;
                        float top = tile.Y - 1;
                        float right = tile.X + 1;
                        float bottom = tile.Y + 1;

                        if (!groupedTiles.Contains(new Vector2(tile.X, top)))
                        {
                            spriteBatch.DrawLine(screenArea.X, screenArea.Y, new Vector2(screenArea.Width, borderSize), borderColor); // top
                        }
                        if (!groupedTiles.Contains(new Vector2(tile.X, bottom)))
                        {
                            spriteBatch.DrawLine(screenArea.X, screenArea.Y + screenArea.Height, new Vector2(screenArea.Width, borderSize), borderColor); // bottom
                        }
                        if (!groupedTiles.Contains(new Vector2(left, tile.Y)))
                        {
                            spriteBatch.DrawLine(screenArea.X, screenArea.Y, new Vector2(borderSize, screenArea.Height), borderColor); // left
                        }
                        if (!groupedTiles.Contains(new Vector2(right, tile.Y)))
                        {
                            spriteBatch.DrawLine(screenArea.X + screenArea.Width, screenArea.Y, new Vector2(borderSize, screenArea.Height), borderColor); // right
                        }
                    }
                }

                //this.deleteButton.draw(spriteBatch);
            }

            if(this.ClickedTiles.Count > 1)
            {
                this.saveButton.bounds.X = (int)(this.ClickedTiles.Max(c => c.X) + 1) * Game1.tileSize - Game1.viewport.X;
                this.saveButton.bounds.Y = (int)(this.ClickedTiles.Max(c => c.Y) + 1) * Game1.tileSize - Game1.viewport.Y;
                this.deleteButton.bounds.X = (int)(this.ClickedTiles.Max(c => c.X) + 2) * Game1.tileSize - Game1.viewport.X;
                this.deleteButton.bounds.Y = (int)(this.ClickedTiles.Max(c => c.Y) + 1) * Game1.tileSize - Game1.viewport.Y;

                this.saveButton.draw(spriteBatch);
                this.deleteButton.draw(spriteBatch);
            }

            this.drawMouse(spriteBatch);
            if (this.hoverText.Length <= 0)
                return;
            IClickableMenu.drawHoverText(spriteBatch, this.hoverText, Game1.dialogueFont, 0, 0, -1, (string)null, -1, (string[])null, (Item)null, 0, -1, -1, -1, -1, 1f, (CraftingRecipe)null);
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

        public override void receiveLeftClick(int x, int y, bool playSound = true)
        {
            Vector2 tile = new Vector2((x + Game1.viewport.X) / Game1.tileSize, (y + Game1.viewport.Y) / Game1.tileSize);
            HashSet<Vector2> clickedTiles = new HashSet<Vector2>();
            if (this.ClickedTiles.Contains(tile))
                this.ClickedTiles.Remove(tile);
            else if (this.CanAddToGroup(tile, this.ClickedTiles))
                this.ClickedTiles.Add(tile);

            if (this.saveButton.containsPoint(x, y))
            {
                this.GroupsOfTiles.Add();
                this.ClickedTiles.Clear();
            }
            if (this.deleteButton.containsPoint(x, y))
            {
                this.ClickedTiles.Clear();
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
                    select pipe.GetSourceTile()
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

        //private HashSet<Vector2> GroupTiles(Vector2 tile) { }

        private bool CanAddToGroup(Vector2 tile, HashSet<Vector2> groupTiles)
        {
            // first tile
            if (!groupTiles.Any())
                return true;

            // adjacent to any tile in group
            return IsAdjacentTile(tile, groupTiles);
        }

        private bool IsAdjacentTile(Vector2 tile, HashSet<Vector2> groupTiles)
        {
            return Utility
                .getAdjacentTileLocationsArray(tile)
                .Intersect(groupTiles)
                .Any();
        }
    }
}
