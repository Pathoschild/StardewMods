using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StardewValley;

namespace Pathoschild.Stardew.ChestsAnywhere.Framework.QuickStack
{
    internal abstract class ItemGroup
    {
        /// <summary>
        /// Logic how this item group determines any given item belonging to this group
        /// </summary>
        /// <returns></returns>
        public abstract bool ItemBelongsToGroup(Item item);
    }
}
