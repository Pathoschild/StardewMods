using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Pathoschild.Stardew.Common;
using StardewValley;
using StardewValley.Menus;

namespace Pathoschild.Stardew.Automate.Framework
{
    class MenuOverlay : IClickableMenu
    {
        private readonly IEnumerable<MachineMetadata> Machines;

        public MenuOverlay(IEnumerable<MachineMetadata> machines)
        {
            this.Machines = machines;
            Game1.viewportFreeze = true;
            Game1.displayHUD = false;
            this.exitFunction = this.ReturnToPlayer;
        }

        public override void draw(SpriteBatch spriteBatch)
        {
            //spriteBatch.DrawLine(0, 0, new Vector2(Game1.viewport.Width, Game1.viewport.Height), Color.Black * 0.5f);
            //DrawMachinesTiles(spriteBatch);
            IDictionary<Vector2, bool> machineTilesHasPipe = GetMachineTiles();

            for (int x = Game1.viewport.X / Game1.tileSize; x < (Game1.viewport.X + Game1.viewport.Width) / Game1.tileSize; x++)
            {
                for (int y = Game1.viewport.Y / Game1.tileSize; y < (Game1.viewport.Y + Game1.viewport.Height) / Game1.tileSize; y++)
                {
                    Vector2 tile = new Vector2(x, y);
                    Color color = Color.Black * 0.75f;
                    if (machineTilesHasPipe.TryGetValue(tile, out bool hasPipe))
                    {
                        if (hasPipe)
                            color = Color.Green * 0.5f;
                        if (!hasPipe)
                            color = Color.DarkGreen * 0.5f;
                    }

                    Rectangle area = new Rectangle((int)tile.X * Game1.tileSize - Game1.viewport.X, (int)tile.Y * Game1.tileSize - Game1.viewport.Y, Game1.tileSize, Game1.tileSize);
                    spriteBatch.DrawLine(area.X, area.Y, new Vector2(area.Width, area.Height), color * 0.2f);

                    // draw border
                    int borderSize = 1;
                    Color borderColor = color * 0.5f;
                    spriteBatch.DrawLine(area.X, area.Y, new Vector2(area.Width, borderSize), borderColor); // top
                    spriteBatch.DrawLine(area.X, area.Y, new Vector2(borderSize, area.Height), borderColor); // left
                    spriteBatch.DrawLine(area.X + area.Width, area.Y, new Vector2(borderSize, area.Height), borderColor); // right
                    spriteBatch.DrawLine(area.X, area.Y + area.Height, new Vector2(area.Width, borderSize), borderColor); // bottom
                }
            }
        }

        public IDictionary<Vector2, bool> GetMachineTiles()
        {
            IDictionary<Vector2, bool> machineTilesHasPipe = new Dictionary<Vector2, bool>();

            foreach (MachineMetadata machine in this.Machines)
            {
                // get tile area in screen pixels
                Vector2 tile = new Vector2((int)(machine.TileBounds.X * Game1.tileSize - Game1.viewport.X), (int)(machine.TileBounds.Y * Game1.tileSize - Game1.viewport.Y));
                machineTilesHasPipe[tile] = machine.Connected.Any();
            }

            return machineTilesHasPipe;
        }


        public override void receiveKeyPress(Keys key)
        {
            base.receiveKeyPress(key);
            if (Game1.options.SnappyMenus)
                return;
            if (Game1.options.doesInputListContain(Game1.options.moveDownButton, key))
                Game1.panScreen(0, 4);
            else if (Game1.options.doesInputListContain(Game1.options.moveRightButton, key))
                Game1.panScreen(4, 0);
            else if (Game1.options.doesInputListContain(Game1.options.moveUpButton, key))
            {
                Game1.panScreen(0, -4);
            }
            else
            {
                if (!Game1.options.doesInputListContain(Game1.options.moveLeftButton, key))
                    return;
                Game1.panScreen(-4, 0);
            }
        }

        public override void update(GameTime time)
        {
            base.update(time);
            int num1 = Game1.getOldMouseX() + Game1.viewport.X;
            int num2 = Game1.getOldMouseY() + Game1.viewport.Y;
            if (num1 - Game1.viewport.X < Game1.tileSize)
                Game1.panScreen(-8, 0);
            else if (num1 - (Game1.viewport.X + Game1.viewport.Width) >= -Game1.tileSize * 2)
                Game1.panScreen(8, 0);
            if (num2 - Game1.viewport.Y < Game1.tileSize)
                Game1.panScreen(0, -8);
            else if (num2 - (Game1.viewport.Y + Game1.viewport.Height) >= -Game1.tileSize)
                Game1.panScreen(0, 8);
            foreach (Keys pressedKey in Game1.oldKBState.GetPressedKeys())
                this.receiveKeyPress(pressedKey);
        }

        public override void receiveRightClick(int x, int y, bool playSound = true) { }

        public void ReturnToPlayer()
        {
            Game1.viewportFreeze = false;
            Game1.displayHUD = true;
        }

        public void DrawMachinesTiles(SpriteBatch spriteBatch)
        {
            foreach (MachineMetadata machine in this.Machines)
            {
                // get tile area in screen pixels
                Rectangle area = new Rectangle((int)(machine.TileBounds.X * Game1.tileSize - Game1.viewport.X), (int)(machine.TileBounds.Y * Game1.tileSize - Game1.viewport.Y), Game1.tileSize, Game1.tileSize);
                Color color;

                if (machine.Connected.Any())
                {
                    color = Color.Green;
                    // draw background
                    spriteBatch.DrawLine(area.X, area.Y, new Vector2(area.Width, area.Height), color * 0.2f);
                }
                else
                {
                    color = Color.DarkGreen;
                    // draw background
                    spriteBatch.DrawLine(area.X, area.Y, new Vector2(area.Width, area.Height), color * 0.2f);
                }

                // draw border
                int borderSize = 1;
                Color borderColor = color * 0.5f;
                spriteBatch.DrawLine(area.X, area.Y, new Vector2(area.Width, borderSize), borderColor); // top
                spriteBatch.DrawLine(area.X, area.Y, new Vector2(borderSize, area.Height), borderColor); // left
                spriteBatch.DrawLine(area.X + area.Width, area.Y, new Vector2(borderSize, area.Height), borderColor); // right
                spriteBatch.DrawLine(area.X, area.Y + area.Height, new Vector2(area.Width, borderSize), borderColor); // bottom
            }
        }
    }
}
