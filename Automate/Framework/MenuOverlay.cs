using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using StardewValley;
using StardewValley.Menus;

namespace Pathoschild.Stardew.Automate.Framework
{
    class MenuOverlay : IClickableMenu
    {
        private readonly IEnumerable<MachineMetadata> Machines;
        public MenuOverlay(IEnumerable<MachineMetadata> machines)
        {
            //this.Machines = from machine in machines
            //                where machine.TileLocation != Vector2.Zero
            //                orderby
            //                machine.TileLocation.Y ascending,
            //                machine.TileLocation.X ascending
            //                select machine;

            Game1.viewportFreeze = true;
            Game1.displayHUD = false;
            this.exitFunction = this.ReturnToPlayer;
        }

        public override void draw(SpriteBatch spriteBatch)
        {

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

        public void DrawMachinesIn(SpriteBatch spriteBatch)
        {

        }
    }
}
