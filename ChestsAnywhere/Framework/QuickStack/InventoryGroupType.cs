using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pathoschild.Stardew.ChestsAnywhere.Framework.QuickStack
{
    internal enum InventoryGroupType
    {
        /// <summary>
        /// Item group is identified by stackability
        /// </summary>
        StackableGroup,

        /// <summary>
        /// Item group is defined by having the same item name
        /// </summary>
        SameNameGroup
    }
}
