using Microsoft.Xna.Framework;

namespace Pathoschild.Stardew.Common.Messages
{
    /// <summary>A network message that indicates a specific chest has changed options and should be updated.</summary>
    internal class AutomateUpdateChestMessage
    {
        /// <summary>The location name containing the chest.</summary>
        public string LocationName { get; set; }

        /// <summary>The chest's tile position.</summary>
        public Vector2 Tile { get; set; }
    }
}
