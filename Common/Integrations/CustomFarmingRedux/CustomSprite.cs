using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Pathoschild.Stardew.Common.Integrations.CustomFarmingRedux
{
    /// <summary>A custom object's sprite info.</summary>
    internal class CustomSprite
    {
        /*********
        ** Accessors
        *********/
        /// <summary>The spritesheet texture.</summary>
        public Texture2D Spritesheet { get; }

        /// <summary>The area in the spritesheet containing the sprite.</summary>
        public Rectangle SourceRectangle { get; }


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="spritesheet">The spritesheet texture.</param>
        /// <param name="sourceRectangle">The area in the spritesheet containing the sprite.</param>
        public CustomSprite(Texture2D spritesheet, Rectangle sourceRectangle)
        {
            this.Spritesheet = spritesheet;
            this.SourceRectangle = sourceRectangle;
        }
    }
}
