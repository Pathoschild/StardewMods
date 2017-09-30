using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;

namespace Pathoschild.Stardew.Automate.Framework
{
    internal class FactoryGroupData
    {
        public string Name { get; set; }
        public IList<Vector2> Tiles { get; set; }
    }
}
