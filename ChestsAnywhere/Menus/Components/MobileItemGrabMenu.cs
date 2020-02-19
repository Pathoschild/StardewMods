using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StardewValley;
using StardewValley.Menus;

namespace Pathoschild.Stardew.ChestsAnywhere.Menus.Components
{
    internal class MobileItemGrabMenu : ItemGrabMenu
    {

        private behaviorOnOrganizeClick organizeFunction;

        public MobileItemGrabMenu(IList<Item> inventory, bool reverseGrab, bool showReceivingMenu, InventoryMenu.highlightThisItem highlightFunction, behaviorOnItemSelect behaviorOnItemSelectFunction, string message, behaviorOnItemSelect behaviorOnItemGrab = null, bool snapToBottom = false, bool canBeExitedWithKey = false, bool playRightClickSound = true, bool allowRightClick = true, bool showOrganizeButton = false, int source = 0, Item sourceItem = null, int whichSpecialButton = -1, object context = null, behaviorOnOrganizeClick organizeFunction = null) : base(inventory, reverseGrab, showReceivingMenu, highlightFunction, behaviorOnItemSelectFunction, message, behaviorOnItemGrab, snapToBottom, canBeExitedWithKey, playRightClickSound, allowRightClick, showOrganizeButton, source, sourceItem, whichSpecialButton, context)
        {
            this.organizeFunction = organizeFunction;
        }

        public override void receiveLeftClick(int x, int y, bool playSound = true)
        {
            if (((ClickableTextureComponent)base.ItemsToGrabMenu.GetType().GetField("organizeButton", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance).GetValue(base.ItemsToGrabMenu)).containsPoint(x, y))
            {
                this.organizeFunction();
                return;
            }
            base.receiveLeftClick(x, y, playSound);
        }

        public delegate void behaviorOnOrganizeClick();
    }
}
