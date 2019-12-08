using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;

namespace ContentPatcher.Framework.Tokens
{
    /// <summary>A rectangle that can hold tokens for its values.</summary>
    internal class TokenRectangle
    {
        internal readonly ITokenString X;
        internal readonly ITokenString Y;
        internal readonly ITokenString Width;
        internal readonly ITokenString Height;

        public TokenRectangle(ITokenString x, ITokenString y, ITokenString width, ITokenString height)
        {
            this.X = x;
            this.Y = y;
            this.Width = width;
            this.Height = height;
        }

        public Rectangle ToRectangle()
        {
            if (this.X == null || this.Y == null || this.Width == null || this.Height == null)
                return Rectangle.Empty;

            int x = int.Parse(this.X.Value);
            int y = int.Parse(this.Y.Value);
            int width = int.Parse(this.Width.Value);
            int height = int.Parse(this.Height.Value);
            return new Rectangle(x, y, width, height);
        }
    }
}
