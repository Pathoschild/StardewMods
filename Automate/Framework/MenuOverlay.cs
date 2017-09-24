using System;
using System.Collections.Generic;
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
    class MenuOverlay : IClickableMenu
    {
        private readonly IEnumerable<MachineMetadata> Machines;
        private readonly IDictionary<Vector2, bool> ChestTileHasPipe;

        public MenuOverlay(IEnumerable<MachineMetadata> machines, IDictionary<Vector2, bool> chestTileHasPipe)
        {
            this.Machines = machines;
            this.ChestTileHasPipe = chestTileHasPipe;

            Game1.viewportFreeze = true;
            Game1.displayHUD = false;
            this.exitFunction = this.ReturnToPlayer;
        }

        public override void draw(SpriteBatch spriteBatch)
        {
            IDictionary<Vector2, bool> machineTilesHasPipe = GetMachineTiles();

            int tileXOnScreen = Game1.viewport.X / Game1.tileSize;
            int tileYOnScreen = Game1.viewport.Y / Game1.tileSize;
            int maximumTilesXOnScreen = (int)Math.Ceiling((Game1.viewport.X + Game1.viewport.Width) / (decimal)Game1.tileSize);
            int maximumTilesYOnScreen = (int)Math.Ceiling((Game1.viewport.Y + Game1.viewport.Height) / (decimal)Game1.tileSize);

            for (int x = tileXOnScreen; x < maximumTilesXOnScreen; x++)
            {
                for (int y = tileYOnScreen; y < maximumTilesYOnScreen; y++)
                {
                    Vector2 tile = new Vector2(x, y);
                    Rectangle area = new Rectangle(x * Game1.tileSize - Game1.viewport.X, y * Game1.tileSize - Game1.viewport.Y, Game1.tileSize, Game1.tileSize);
                    Color color = Color.Black * 0.5f;
                    if (machineTilesHasPipe.TryGetValue(tile, out bool hasPipe))
                    {
                        if (hasPipe)
                            color = Color.Green * 0.2f;
                        else
                            color = Color.Red * 0.2f;
                    }

                    if (this.GetAllChestTiles().TryGetValue(tile, out bool hasConnection))
                    {
                        if (hasConnection)
                            color = Color.Green * 0.2f;
                        else
                            color = Color.Red * 0.2f;
                    }

                    spriteBatch.DrawLine(area.X, area.Y, new Vector2(area.Width, area.Height), color);// draw border

                    if (color != Color.Black * 0.5f)
                    {
                        int borderSize = 5;
                        Color borderColor = color * 0.75f;
                        spriteBatch.DrawLine(area.X, area.Y, new Vector2(area.Width, borderSize), borderColor); // top
                        spriteBatch.DrawLine(area.X, area.Y, new Vector2(borderSize, area.Height), borderColor); // left
                        spriteBatch.DrawLine(area.X + area.Width, area.Y, new Vector2(borderSize, area.Height), borderColor); // right
                        spriteBatch.DrawLine(area.X, area.Y + area.Height, new Vector2(area.Width, borderSize), borderColor); // bottom
                    }
                }
            }
        }

        public IDictionary<Vector2, bool> GetMachineTiles()
        {
            IDictionary<Vector2, bool> machineTilesHasPipe = new Dictionary<Vector2, bool>();

            foreach (MachineMetadata machine in this.Machines)
            {
                for(int tileX = 0; tileX < machine.TileBounds.Width; tileX++)
                    for(int tileY = 0; tileY < machine.TileBounds.Height; tileY++)
                    {
                        Vector2 tile = new Vector2(machine.TileBounds.X + tileX, machine.TileBounds.Y + tileY);
                        machineTilesHasPipe[tile] = machine.Connected.Any();
                    }
            }

            return machineTilesHasPipe;
        }

        public IDictionary<Vector2, bool> GetAllChestTiles()
        {
            // get all unconnected chests
            foreach (var pair in Game1.currentLocation.objects)
            {
                if (pair.Value is Chest && !this.ChestTileHasPipe.ContainsKey(pair.Key))
                    this.ChestTileHasPipe[pair.Key] = false;
            }

            return this.ChestTileHasPipe;
        }


        public override void receiveKeyPress(Keys key)
        {
            base.receiveKeyPress(key);
            if (Game1.options.SnappyMenus)
                return;
            if (Game1.options.doesInputListContain(Game1.options.moveDownButton, key))
                PanScreen(0, 4);
            else if (Game1.options.doesInputListContain(Game1.options.moveRightButton, key))
                PanScreen(4, 0);
            else if (Game1.options.doesInputListContain(Game1.options.moveUpButton, key))
            {
                PanScreen(0, -4);
            }
            else
            {
                if (!Game1.options.doesInputListContain(Game1.options.moveLeftButton, key))
                    return;
                PanScreen(-4, 0);
            }
        }

        public override void update(GameTime time)
        {
            base.update(time);
            int num1 = Game1.getOldMouseX() + Game1.viewport.X;
            int num2 = Game1.getOldMouseY() + Game1.viewport.Y;
            if (num1 - Game1.viewport.X < Game1.tileSize)
                PanScreen(-8, 0);
            else if (num1 - (Game1.viewport.X + Game1.viewport.Width) >= -Game1.tileSize * 2)
                PanScreen(8, 0);
            if (num2 - Game1.viewport.Y < Game1.tileSize)
                PanScreen(0, -8);
            else if (num2 - (Game1.viewport.Y + Game1.viewport.Height) >= -Game1.tileSize)
                PanScreen(0, 8);
            foreach (Keys pressedKey in Game1.oldKBState.GetPressedKeys())
                this.receiveKeyPress(pressedKey);
        }

        public static void PanScreen(int x, int y)
        {
            Game1.previousViewportPosition.X = (float)Game1.viewport.Location.X;
            Game1.previousViewportPosition.Y = (float)Game1.viewport.Location.Y;
            Game1.viewport.X += x;
            Game1.viewport.Y += y;
            if (Game1.currentLocation.IsOutdoors)
                Game1.clampViewportToGameMap(); 
            Game1.updateRaindropPosition();
        }

        // Not used, might change in the future if we want limit the bounds of the cameraPanning inside.
        //public static void ClampViewportToGameMap()
        //{
        //    if (Game1.viewport.X < 0)
        //        Game1.viewport.X = 0;
        //    if (Game1.viewport.X > Game1.currentLocation.map.DisplayWidth - Game1.viewport.Width)
        //        Game1.viewport.X = Game1.currentLocation.map.DisplayWidth - Game1.viewport.Width;
        //    if (Game1.viewport.Y < 0)
        //        Game1.viewport.Y = 0;
        //    if (Game1.viewport.Y <= Game1.currentLocation.map.DisplayHeight - Game1.viewport.Height)
        //        return;
        //    Game1.viewport.Y = Game1.currentLocation.map.DisplayHeight - Game1.viewport.Height;
        //}

        public override void receiveRightClick(int x, int y, bool playSound = true) { }

        public void ReturnToPlayer()
        {
            Game1.viewportFreeze = false;
            Game1.displayHUD = true;
        }
    }
}
