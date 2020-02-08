using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley.Menus;

namespace Pathoschild.Stardew.LookupAnything.Components
{
    /// <summary>A textbox fires events while searching.</summary>
    internal class SearchTextBox : IDisposable
    {
        /*********
        ** Properties
        *********/
        /// <summary>The underlying textbox.</summary>
        private readonly TextBox Textbox;

        /// <summary>The last search text received for change detection.</summary>
        private string LastText = string.Empty;

        /// <summary>The rendered textbox's pixel area on-screen.</summary>
        private Rectangle BoundsImpl;


        /*********
        ** Accessors
        *********/
        /// <summary>The event raised when the search text changes.</summary>
        public event EventHandler<string> OnChanged;

        public Rectangle Bounds
        {
            get => this.BoundsImpl;
            set
            {
                this.BoundsImpl = value;
                this.Textbox.X = value.X;
                this.Textbox.Y = value.Y;
                this.Textbox.Width = value.Width;
                this.Textbox.Height = value.Height;
            }
        }


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="font">The text font.</param>
        /// <param name="textColor">The text color.</param>
        public SearchTextBox(SpriteFont font, Color textColor)
        {
            this.Textbox = new TextBox(Sprites.Textbox.Sheet, null, font, textColor);
            this.Bounds = new Rectangle(this.Textbox.X, this.Textbox.Y, this.Textbox.Width, this.Textbox.Height);
        }

        /// <summary>Set the input focus to this control.</summary>
        public void Select()
        {
            this.Textbox.Selected = true;
        }

        /// <summary>Draw the textbox.</summary>
        /// <param name="batch">The sprite batch.</param>
        public void Draw(SpriteBatch batch)
        {
            this.NotifyChange();
            this.Textbox.Draw(batch);
        }

        /// <summary>Detect updated search text and notify listeners.</summary>
        private void NotifyChange()
        {
            if (this.Textbox.Text != this.LastText)
            {
                this.OnChanged?.Invoke(this, this.Textbox.Text);
                this.LastText = this.Textbox.Text;
            }
        }

        /// <summary>Release all resources.</summary>
        public void Dispose()
        {
            this.OnChanged = null;
            this.Textbox.Selected = false;
        }
    }
}
