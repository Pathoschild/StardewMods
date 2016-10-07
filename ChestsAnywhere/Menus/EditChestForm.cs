using System;
using System.Linq;
using ChestsAnywhere.Common;
using ChestsAnywhere.Framework;
using ChestsAnywhere.Menus.Components;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.Menus;

namespace ChestsAnywhere.Menus
{
    /// <summary>A UI which lets the player edit a chest's data.</summary>
    internal class EditChestForm
    {
        /*********
        ** Properties
        *********/
        /// <summary>The chest to edit.</summary>
        private readonly ManagedChest Chest;

        /// <summary>The bounds within which to draw the form.</summary>
        private Rectangle Bounds;

        /// <summary>The editable chest name.</summary>
        private readonly ValidatedTextBox NameField;

        /// <summary>The field order.</summary>
        private readonly ValidatedTextBox OrderField;

        /// <summary>The editable category name.</summary>
        private readonly ValidatedTextBox CategoryField;

        /// <summary>The checkbox which indicates whether to hide the chest.</summary>
        private readonly Checkbox HideChestField;

        /// <summary>The button which saves the settings.</summary>
        private readonly ClickableTextureComponent SaveButton;


        /*********
        ** Accessors
        *********/
        /// <summary>Whether the edit form is active.</summary>
        public bool IsActive { get; private set; } = true;


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="chest">The chest to edit.</param>
        public EditChestForm(ManagedChest chest)
        {
            // initialise
            this.Chest = chest;
            this.CalculateDimensions();

            // build components
            int longTextWidth = (int)Game1.smallFont.MeasureString("A sufficiently, reasonably long string").X;
            this.NameField = new ValidatedTextBox(Game1.smallFont, Color.Black, ch => ch != '|') { Width = longTextWidth, Text = this.Chest.Name };
            this.CategoryField = new ValidatedTextBox(Game1.smallFont, Color.Black, ch => ch != '|') { Width = longTextWidth, Text = this.Chest.Category };
            this.OrderField = new ValidatedTextBox(Game1.smallFont, Color.Black, char.IsDigit) { Width = (int)Game1.smallFont.MeasureString("9999999").X, Text = this.Chest.Order?.ToString() };
            this.HideChestField = new Checkbox(this.Chest.IsIgnored);
            this.SaveButton = new ClickableTextureComponent("save-chest", new Rectangle(0, 0, Game1.tileSize, Game1.tileSize), null, "OK", Game1.mouseCursors, Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, IClickableMenu.borderWithDownArrowIndex), 1f);
        }

        /// <summary>The method invoked when the player left-clicks on the form.</summary>
        /// <param name="x">The X-position of the cursor.</param>
        /// <param name="y">The Y-position of the cursor.</param>
        /// <returns>Returns whether the edit UI is still active.</returns>
        public void ReceiveLeftClick(int x, int y)
        {
            // name field
            if (this.NameField.GetBounds().Contains(x, y))
                this.NameField.Select();

            // category field
            else if (this.CategoryField.GetBounds().Contains(x, y))
                this.CategoryField.Select();

            // order field
            else if (this.OrderField.GetBounds().Contains(x, y))
                this.OrderField.Select();

            // 'hide chest' checkbox
            else if (this.HideChestField.GetBounds().Contains(x, y))
                this.HideChestField.Toggle();

            // save button
            else if (this.SaveButton.containsPoint(x, y))
            {
                this.Save();
                this.IsActive = false;
            }
        }

        /// <summary>Render the UI.</summary>
        /// <param name="sprites">The sprites to render.</param>
        public void Draw(SpriteBatch sprites)
        {
            // get initial measurements
            SpriteFont font = Game1.smallFont;
            const int gutter = 10;
            int padding = Game1.tileSize / 2;
            float topOffset = padding;
            int maxLabelWidth = (int)new[] { "Location:", "Name:", "Category:", "Order:" }.Select(p => font.MeasureString(p).X).Max();


            // background
            sprites.DrawMenuBackground(this.Bounds);

            // Location name
            {
                Vector2 labelSize = sprites.DrawTextBlock(font, $"Location:", new Vector2(this.Bounds.X + padding + (int)(maxLabelWidth - font.MeasureString("Location:").X), this.Bounds.Y + topOffset), this.Bounds.Width);
                sprites.DrawTextBlock(font, this.Chest.LocationName, new Vector2(this.Bounds.X + padding + maxLabelWidth + gutter, this.Bounds.Y + topOffset), this.Bounds.Width);
                topOffset += labelSize.Y;
            }

            // editable text fields
            var fields = new[] { Tuple.Create("Name:", this.NameField), Tuple.Create("Category:", this.CategoryField), Tuple.Create("Order:", this.OrderField) }.Where(p => p != null);
            foreach (var field in fields)
            {
                // get data
                string label = field.Item1;
                ValidatedTextBox textbox = field.Item2;

                // draw label
                Vector2 labelSize = sprites.DrawTextBlock(font, label, new Vector2(this.Bounds.X + padding + (int)(maxLabelWidth - font.MeasureString(label).X), this.Bounds.Y + topOffset + 7), this.Bounds.Width);
                textbox.X = this.Bounds.X + padding + maxLabelWidth + gutter;
                textbox.Y = this.Bounds.Y + (int)topOffset;
                textbox.Draw(sprites);

                // update offset
                topOffset += Math.Max(labelSize.Y + 7, textbox.Height);
            }

            // hide chest checkbox
            {
                this.HideChestField.X = this.Bounds.X + padding;
                this.HideChestField.Y = this.Bounds.Y + (int)topOffset;
                this.HideChestField.Width = 24;
                this.HideChestField.Draw(sprites);
                Vector2 labelSize = sprites.DrawTextBlock(font, "Hide this chest" + (this.HideChestField.Value ? " (irreversible until you replace chest!)" : ""), new Vector2(this.Bounds.X + padding + 7 + this.HideChestField.Width, this.Bounds.Y + topOffset), this.Bounds.Width, this.HideChestField.Value ? Color.Red : Color.Black);
                topOffset += Math.Max(this.HideChestField.Width, labelSize.Y);
            }

            // save button
            this.SaveButton.bounds = new Rectangle(this.Bounds.X + padding, this.Bounds.Y + (int)topOffset, this.SaveButton.bounds.Width, this.SaveButton.bounds.Height);
            this.SaveButton.draw(sprites);
        }


        /*********
        ** Private methods
        *********/
        /****
        ** Behaviour
        ****/
        /// <summary>Save the form input.</summary>
        private void Save()
        {
            // get order value
            int? order = null;
            {
                int parsed;
                if (int.TryParse(this.OrderField.Text, out parsed))
                    order = parsed;
            }

            // update chest
            this.Chest.Update(this.NameField.Text, this.CategoryField.Text, order, this.HideChestField.Value);
        }

        /****
        ** Drawing
        ****/
        /// <summary>Build the UI layout.</summary>
        private void CalculateDimensions()
        {
            int width = Game1.tileSize * 13;
            int height = Game1.tileSize * 7 + Game1.tileSize / 2;
            int x = Game1.viewport.Width / 2 - width / 2;
            int y = Game1.viewport.Height / 2 - height / 2 + Game1.tileSize;
            this.Bounds = new Rectangle(x, y, width, height);
        }
    }
}
