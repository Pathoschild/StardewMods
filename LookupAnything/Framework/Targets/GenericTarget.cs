using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;

namespace Pathoschild.Stardew.LookupAnything.Framework.Targets
{
    /// <summary>Positional metadata about an object in the world.</summary>
    internal abstract class GenericTarget : ITarget
    {
        /*********
        ** Accessors
        *********/
        /// <summary>The target type.</summary>
        public TargetType Type { get; set; }

        /// <summary>The underlying in-game object.</summary>
        public object Value { get; set; }

        /// <summary>The object's tile position in the current location (if applicable).</summary>
        public Vector2? Tile { get; set; }


        /*********
        ** Public methods
        *********/
        /// <summary>Get the target's tile position, or throw an exception if it doesn't have one.</summary>
        /// <exception cref="InvalidOperationException">The target doesn't have a tile position.</exception>
        public Vector2 GetTile()
        {
            if (this.Tile == null)
                throw new InvalidOperationException($"This {this.Type} target doesn't have a tile position.");
            return this.Tile.Value;
        }

        /// <summary>Get whether the object is at the specified map tile position.</summary>
        /// <param name="position">The map tile position.</param>
        public bool IsAtTile(Vector2 position)
        {
            return this.Tile != null && this.Tile == position;
        }

        /// <summary>Get a strongly-typed instance.</summary>
        /// <typeparam name="T">The expected value type.</typeparam>
        public T GetValue<T>()
        {
            return (T)this.Value;
        }

        /// <summary>Get a rectangle which roughly bounds the visible sprite relative the viewport.</summary>
        public virtual Rectangle GetSpriteArea()
        {
            return GameHelper.GetScreenCoordinatesFromTile(this.GetTile());
        }

        /// <summary>Get whether the visible sprite intersects the specified coordinate. This can be an expensive test.</summary>
        /// <param name="tile">The tile to search.</param>
        /// <param name="position">The viewport-relative coordinates to search.</param>
        /// <param name="spriteArea">The approximate sprite area calculated by <see cref="GetSpriteArea"/>.</param>
        public virtual bool SpriteIntersectsPixel(Vector2 tile, Vector2 position, Rectangle spriteArea)
        {
            return this.IsAtTile(tile);
        }


        /*********
        ** Protected methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="type">The target type.</param>
        /// <param name="obj">The underlying in-game object.</param>
        /// <param name="tilePosition">The object's tile position in the current location (if applicable).</param>
        protected GenericTarget(TargetType type, object obj, Vector2? tilePosition = null)
        {
            this.Type = type;
            this.Value = obj;
            this.Tile = tilePosition;
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
            return new Rectangle(x - Game1.viewport.X, y - Game1.viewport.Y, width, height);
        }

        /// <summary>Get whether the visible sprite intersects the specified coordinate. This can be an expensive test.</summary>
        /// <param name="tile">The tile to search.</param>
        /// <param name="position">The viewport-relative coordinates to search.</param>
        /// <param name="spriteArea">The approximate sprite area calculated by <see cref="GetSpriteArea"/>.</param>
        /// <param name="spriteSheet">The sprite sheet containing the displayed sprite.</param>
        /// <param name="spriteSourceRectangle">The coordinates and dimensions of the sprite within the sprite sheet.</param>
        /// <param name="spriteEffects">The transformation to apply on the sprite.</param>
        protected bool SpriteIntersectsPixel(Vector2 tile, Vector2 position, Rectangle spriteArea, Texture2D spriteSheet, Rectangle spriteSourceRectangle, SpriteEffects spriteEffects = SpriteEffects.None)
        {
            // get sprite sheet coordinate
            Vector2 spriteSheetPosition = GameHelper.GetSpriteSheetCoordinates(position, spriteArea, spriteSourceRectangle, spriteEffects);
            if (!spriteSourceRectangle.Contains((int)spriteSheetPosition.X, (int)spriteSheetPosition.Y))
                return false;

            // check pixel
            Color pixel = GameHelper.GetSpriteSheetPixel<Color>(spriteSheet, spriteSheetPosition);
            return pixel.A != 0; // pixel not transparent
        }
    }
}
