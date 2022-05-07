using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;

namespace Pathoschild.Stardew.LookupAnything.Framework.Lookups
{
    /// <summary>Positional metadata about an object in the world.</summary>
    /// <typeparam name="TValue">The underlying value type.</typeparam>
    internal abstract class GenericTarget<TValue> : ITarget
    {
        /*********
        ** Fields
        *********/
        /// <summary>Provides utility methods for interacting with the game code.</summary>
        protected GameHelper GameHelper { get; }


        /*********
        ** Accessors
        *********/
        /// <summary>The subject type.</summary>
        public SubjectType Type { get; protected set; }

        /// <summary>The object's tile position in the current location (if applicable).</summary>
        public Vector2 Tile { get; protected set; }

        /// <summary>The underlying in-game object.</summary>
        public TValue Value { get; }

        /// <summary>Get the subject info about the target.</summary>
        public Func<ISubject> GetSubject { get; protected set; }


        /*********
        ** Public methods
        *********/
        /// <summary>Get the sprite's source rectangle within its texture.</summary>
        public abstract Rectangle GetSpritesheetArea();

        /// <summary>Get a rectangle which roughly bounds the visible sprite relative the viewport.</summary>
        public virtual Rectangle GetWorldArea()
        {
            return this.GameHelper.GetScreenCoordinatesFromTile(this.Tile);
        }

        /// <summary>Get whether the visible sprite intersects the specified coordinate. This can be an expensive test.</summary>
        /// <param name="tile">The tile to search.</param>
        /// <param name="position">The viewport-relative coordinates to search.</param>
        /// <param name="spriteArea">The approximate sprite area calculated by <see cref="GetWorldArea"/>.</param>
        public virtual bool SpriteIntersectsPixel(Vector2 tile, Vector2 position, Rectangle spriteArea)
        {
            return this.Tile == tile;
        }


        /*********
        ** Protected methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="gameHelper">Provides utility methods for interacting with the game code.</param>
        /// <param name="type">The subject type.</param>
        /// <param name="value">The underlying in-game entity.</param>
        /// <param name="tilePosition">The object's tile position in the current location (if applicable).</param>
        /// <param name="getSubject">Get the subject info about the target.</param>
        protected GenericTarget(GameHelper gameHelper, SubjectType type, TValue value, Vector2 tilePosition, Func<ISubject> getSubject)
        {
            this.GameHelper = gameHelper;
            this.Type = type;
            this.Value = value;
            this.Tile = tilePosition;
            this.GetSubject = getSubject;
        }

        /// <summary>Get a rectangle which roughly bounds the visible sprite.</summary>
        /// <param name="boundingBox">The occupied 'floor space' at the bottom of the sprite in the world.</param>
        /// <param name="sourceRectangle">The sprite's source rectangle in the sprite sheet.</param>
        protected Rectangle GetSpriteArea(Rectangle boundingBox, Rectangle sourceRectangle)
        {
            int height = sourceRectangle.Height * Game1.pixelZoom;
            int width = sourceRectangle.Width * Game1.pixelZoom;
            int x = boundingBox.Center.X - (width / 2);
            int y = boundingBox.Y + boundingBox.Height - height;
            return new Rectangle(x - Game1.uiViewport.X, y - Game1.uiViewport.Y, width, height);
        }

        /// <summary>Get whether the visible sprite intersects the specified coordinate. This can be an expensive test.</summary>
        /// <param name="tile">The tile to search.</param>
        /// <param name="position">The viewport-relative coordinates to search.</param>
        /// <param name="spriteArea">The approximate sprite area calculated by <see cref="GetWorldArea"/>.</param>
        /// <param name="spriteSheet">The sprite sheet containing the displayed sprite.</param>
        /// <param name="spriteSourceRectangle">The coordinates and dimensions of the sprite within the sprite sheet.</param>
        /// <param name="spriteEffects">The transformation to apply on the sprite.</param>
        protected bool SpriteIntersectsPixel(Vector2 tile, Vector2 position, Rectangle spriteArea, Texture2D? spriteSheet, Rectangle spriteSourceRectangle, SpriteEffects spriteEffects = SpriteEffects.None)
        {
            if (spriteSheet is null)
                return false;

            // get sprite sheet coordinate
            Vector2 spriteSheetPosition = this.GameHelper.GetSpriteSheetCoordinates(position, spriteArea, spriteSourceRectangle, spriteEffects);
            if (!spriteSourceRectangle.Contains((int)spriteSheetPosition.X, (int)spriteSheetPosition.Y))
                return false;

            // check pixel
            Color pixel = this.GameHelper.GetSpriteSheetPixel<Color>(spriteSheet, spriteSheetPosition);
            return pixel.A != 0; // pixel not transparent
        }
    }
}
