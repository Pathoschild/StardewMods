using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pathoschild.Stardew.ChestsAnywhere.Framework.QuickStack
{
    internal class QuickStackConfig
    {
        /// <summary>Whether items should be placed in junimo huts.</summary>
        public bool ConsiderJunimoHuts { get; set; } = false;

        /// <summary>Whether items should be placed in the shipping bin.</summary>
        public bool ConsiderShippingBin { get; set; } = false;

        /// <summary>Whether items should be placed in mini shipping bins.</summary>
        public bool ConsiderMiniShippingBins { get; set; } = false;

        /// <summary>Whether items should be placed in auto grabbers.</summary>
        public bool ConsiderAutoGrabber { get; set; } = false;
    }
}
