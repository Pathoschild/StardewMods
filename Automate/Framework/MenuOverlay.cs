using System;
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
    }
}
