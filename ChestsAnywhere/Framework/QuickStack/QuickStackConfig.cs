using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pathoschild.Stardew.ChestsAnywhere.Framework.QuickStack
{
    internal class QuickStackConfig
    {
        public bool ConsiderJunimoHuts { get; set; } = false;

        public bool ConsiderShippingBin { get; set; } = false;

        public bool ConsiderAutoGrabber { get; set; } = false;

        public bool MoveItemsToHiddenChests { get; set; } = false;
    }
}
