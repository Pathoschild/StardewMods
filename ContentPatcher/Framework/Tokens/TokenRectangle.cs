using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace ContentPatcher.Framework.Tokens
{
    /// <summary>A rectangle that can hold tokens for its values.</summary>
    internal class TokenRectangle : IContextual
    {
        /*********
        ** Accessors
        *********/
        /// <summary>The underlying contextual values.</summary>
        private readonly AggregateContextual Contextuals = new AggregateContextual();


        /*********
        ** Accessors
        *********/
        /// <summary>The X coordinate value of the top-left corner.</summary>
        public ITokenString X { get; }

        /// <summary>The Y coordinate value of the top-left corner.</summary>
        public ITokenString Y { get; }

        /// <summary>The width of the area.</summary>
        public ITokenString Width { get; }

        /// <summary>The height of the area.</summary>
        public ITokenString Height { get; }

        /// <summary>Whether the instance may change depending on the context.</summary>
        public bool IsMutable => this.Contextuals.IsMutable;

        /// <summary>Whether the instance is valid for the current context.</summary>
        public bool IsReady => this.Contextuals.IsReady;


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="x">The X coordinate value of the top-left corner.</param>
        /// <param name="y">The Y coordinate value of the top-left corner.</param>
        /// <param name="width">The width of the area.</param>
        /// <param name="height">The height of the area.</param>
        public TokenRectangle(ITokenString x, ITokenString y, ITokenString width, ITokenString height)
        {
            this.X = x ?? throw new ArgumentNullException(nameof(x));
            this.Y = y ?? throw new ArgumentNullException(nameof(y));
            this.Width = width ?? throw new ArgumentNullException(nameof(width));
            this.Height = height ?? throw new ArgumentNullException(nameof(height));

            this.Contextuals
                .Add(x)
                .Add(y)
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

        /// <summary>Update the instance when the context changes.</summary>
        /// <param name="context">Provides access to contextual tokens.</param>
        /// <returns>Returns whether the instance changed.</returns>
        public bool UpdateContext(IContext context)
        {
            return this.Contextuals.UpdateContext(context);
        }

        /// <summary>Get the token names used by this patch in its fields.</summary>
        public IEnumerable<string> GetTokensUsed()
        {
            return this.Contextuals.GetTokensUsed();
        }

        /// <summary>Get diagnostic info about the contextual instance.</summary>
        public IContextualState GetDiagnosticState()
        {
            return this.Contextuals.GetDiagnosticState();
        }


        /*********
        ** Private methods
        *********/
        /// <summary>Parse a number field.</summary>
        /// <param name="raw">The raw field to parse.</param>
        /// <param name="name">The field name for errors.</param>
        /// <param name="parsed">The parsed number, if the input was valid.</param>
        /// <param name="error">An error phrase indicating why the value can't be parsed, if applicable.</param>
        private bool TryGetNumber(ITokenString raw, string name, out int parsed, out string error)
        {
            if (!raw.IsReady)
            {
                parsed = -1;
                error = $"{name} is not ready";
                return false;
            }

            if (!int.TryParse(raw.Value, out parsed))
            {
                error = $"{name} value '{raw.Value}' can't be parsed as an integer";
                return false;
            }

            error = null;
            return true;
        }
    }
}
