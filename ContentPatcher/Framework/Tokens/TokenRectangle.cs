using System;
using Microsoft.Xna.Framework;

namespace ContentPatcher.Framework.Tokens
{
    /// <summary>A rectangle that can hold tokens for its values.</summary>
    internal class TokenRectangle : TokenPosition
    {
        /*********
        ** Accessors
        *********/
        /// <summary>The width of the area.</summary>
        public ITokenString Width { get; }

        /// <summary>The height of the area.</summary>
        public ITokenString Height { get; }


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="x">The X coordinate value of the top-left corner.</param>
        /// <param name="y">The Y coordinate value of the top-left corner.</param>
        /// <param name="width">The width of the area.</param>
        /// <param name="height">The height of the area.</param>
        public TokenRectangle(IManagedTokenString x, IManagedTokenString y, IManagedTokenString width, IManagedTokenString height)
            : base(x, y)
        {
            this.Width = width ?? throw new ArgumentNullException(nameof(width));
            this.Height = height ?? throw new ArgumentNullException(nameof(height));

            this.Contextuals
                .Add(width)
                .Add(height);
        }

        /// <summary>Try to create a rectangle value.</summary>
        /// <param name="rectangle">The rectangle value, if valid.</param>
        /// <param name="error">An error phrase indicating why the rectangle can't be constructed.</param>
        /// <returns>Returns whether the rectangle value was successfully created.</returns>
        public bool TryGetRectangle(out Rectangle rectangle, out string error)
        {
            if (
                !this.TryGetNumber(this.X, nameof(this.X), out int x, out error)
                || !this.TryGetNumber(this.Y, nameof(this.Y), out int y, out error)
                || !this.TryGetNumber(this.Width, nameof(this.Width), out int width, out error)
                || !this.TryGetNumber(this.Height, nameof(this.Height), out int height, out error)
            )
            {
                rectangle = Rectangle.Empty;
                return false;
            }

            rectangle = new Rectangle(x, y, width, height);
            return true;
        }
    }
}
