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


        /*********
        ** Accessors
        *********/
        /// <summary>The event raised when the search text changes.</summary>
        public event EventHandler<string> OnChanged;

        /// <summary>The X position of the rendered textbox.</summary>
        public int X
        {
            get => this.Textbox.X;
            set => this.Textbox.X = value;
        }

        /// <summary>The Y position of the rendered textbox.</summary>
        public int Y
        {
            get => this.Textbox.Y;
            set => this.Textbox.Y = value;
        }

        /// <summary>The width of the rendered textbox.</summary>
        public int Width
        {
            get => this.Textbox.Width;
            set => this.Textbox.Width = value;
        }

        /// <summary>The height of the rendered textbox.</summary>
        public int Height
        {
            get => this.Textbox.Height;
            set => this.Textbox.Height = value;
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
