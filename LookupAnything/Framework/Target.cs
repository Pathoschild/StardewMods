using System;
using Microsoft.Xna.Framework;

namespace Pathoschild.LookupAnything.Framework
{
    /// <summary>An in-game element that can be looked up, including position metadata.</summary>
    /// <typeparam name="T">The target type.</typeparam>
    public class Target<T>
    {
        /*********
        ** Accessors
        *********/
        /// <summary>The target type.</summary>
        public TargetType Type { get; set; }

        /// <summary>The underlying in-game object.</summary>
        public T Value { get; set; }

        /// <summary>The object's tile position in the current location (if applicable).</summary>
        public Vector2? Tile { get; set; }


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="type">The target type.</param>
        /// <param name="obj">The underlying in-game object.</param>
        /// <param name="tilePosition">The object's tile position in the current location (if applicable).</param>
        public Target(TargetType type, T obj, Vector2? tilePosition = null)
        {
            this.Type = type;
            this.Value = obj;
            this.Tile = tilePosition;
        }

        /// <summary>Get the target's tile position, or throw an exception if it doesn't have one.</summary>
        /// <exception cref="InvalidOperationException">The target doesn't have a tile position.</exception>
        public Vector2 GetTile()
        {
            if (this.Tile == null)
                throw new InvalidOperationException($"This {this.Type} target doesn't have a tile position.");
            return this.Tile.Value;
        }

        /// <summary>Get whether the object is at a specified map tile position.</summary>
        /// <param name="position">The map tile position.</param>
        public bool IsAtTile(Vector2 position)
        {
            return this.Tile != null && this.Tile == position;
        }

        /// <summary>Get a strongly-typed instance.</summary>
        /// <typeparam name="TNew">The expected object type.</typeparam>
        public Target<TNew> ForType<TNew>()
        {
            return new Target<TNew>(this.Type, (TNew)(object)this.Value, this.Tile);
        }
    }

    /// <summary>An in-game element that can be looked up, including position metadata.</summary>
    public class Target : Target<object>
    {
        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="type">The target type.</param>
        /// <param name="obj">The underlying in-game object.</param>
        /// <param name="tilePosition">The object's tile position in the current location (if applicable).</param>
        public Target(TargetType type, object obj, Vector2? tilePosition = null)
            : base(type, obj, tilePosition) { }
    }
}