using System;
using Microsoft.Xna.Framework;

namespace Pathoschild.LookupAnything.Framework.Targets
{
    /// <summary>Positional metadata about an object in the world.</summary>
    public abstract class GenericTarget : ITarget
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
    }
}