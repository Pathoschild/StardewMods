using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Pathoschild.Stardew.ChestsAnywhere.Framework;
using Pathoschild.Stardew.ChestsAnywhere.Menus.Components;
using Pathoschild.Stardew.Common;
using Pathoschild.Stardew.Common.UI;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Menus;

namespace Pathoschild.Stardew.ChestsAnywhere.Menus.Overlays
{
    /// <summary>An overlay for <see cref="ItemGrabMenu"/> which lets the player navigate and edit chests.</summary>
    internal class ManageChestOverlay : BaseOverlay
    {
        /*********
        ** Fields
        *********/
        /****
        ** Data
        ****/
        /// <summary>Provides translations stored in the mod's folder.</summary>
        private readonly ITranslationHelper Translations;

        /// <summary>The available chests.</summary>
        private readonly ManagedChest[] Chests;

        /// <summary>The selected chest.</summary>
        private readonly ManagedChest Chest;

        /// <summary>Whether to show Automate options.</summary>
        private readonly bool ShowAutomateOptions;

        /// <summary>The number of draw cycles since the menu was initialised.</summary>
        private int DrawCount;

        /// <summary>The backing field for <see cref="ActiveElement"/>; shouldn't be edited directly.</summary>
        private Element _activeElement;

        /// <summary>The overlay element which should receive input.</summary>
        private Element ActiveElement
        {
            get => this._activeElement;
            set
            {
                this._activeElement = value;
                this.SetItemsClickable(this._activeElement == Element.Menu);
            }
        }

        /// <summary>The unique chest categories.</summary>
        private readonly string[] Categories;

        /// <summary>The name of the selected category.</summary>
        private string SelectedCategory => this.Chest.DisplayCategory;

        /// <summary>The mod configuration.</summary>
        private readonly ModConfig Config;

        /// <summary>The configured key bindings.</summary>
        private readonly ModConfigKeys Keys;

        /// <summary>Whether to show the category dropdown.</summary>
        private bool ShowCategoryDropdown => this.Categories.Length > 1;

        /****
        ** Menu management
        ****/
        /// <summary>The underlying chest menu.</summary>
        private readonly ItemGrabMenu Menu;

        /// <summary>The underlying menu's player inventory submenu.</summary>
        private readonly InventoryMenu MenuInventoryMenu;

        /// <summary>The default highlight function for the chest items.</summary>
        private readonly InventoryMenu.highlightThisItem DefaultChestHighlighter;

        /// <summary>The default highlight function for the player inventory items.</summary>
        private readonly InventoryMenu.highlightThisItem DefaultInventoryHighlighter;

        /// <summary>Whether the chest menu is ready to close.</summary>
        private bool CanCloseChest => this.Menu.readyToClose();

        /****
        ** Access UI
        ****/
        /// <summary>The font with which to render text.</summary>
        private readonly SpriteFont Font = Game1.smallFont;

        /// <summary>The chest selector tab.</summary>
        private Tab ChestTab;

        /// <summary>The category selector tab.</summary>
        private Tab CategoryTab;

        /// <summary>The chest selector dropdown.</summary>
        private DropList<ManagedChest> ChestSelector;

        /// <summary>The category selector dropdown.</summary>
        private DropList<string> CategorySelector;

        /// <summary>The edit button.</summary>
        private ClickableTextureComponent EditButton;

        /// <summary>The button which sorts the player inventory.</summary>
        private ClickableTextureComponent SortInventoryButton;

        /****
        ** Edit UI
        ****/
        /// <summary>The editable chest name.</summary>
        private ValidatedTextBox EditNameField;

        /// <summary>The field order.</summary>
        private ValidatedTextBox EditOrderField;

        /// <summary>The editable category name.</summary>
        private ValidatedTextBox EditCategoryField;

        /// <summary>A checkbox which indicates whether to hide the chest.</summary>
        private Checkbox EditHideChestField;

        /// <summary>A checkbox which indicates whether Automate should should put output in this chest first.</summary>
        private Checkbox EditAutomateOutput;

        /// <summary>A checkbox which indicates whether Automate should allow getting items form this chest.</summary>
        private Checkbox EditAutomateNoInput;

        /// <summary>A checkbox which indicates whether Automate should allow sending items to this chest.</summary>
        private Checkbox EditAutomateNoOutput;

        /// <summary>A checkbox which indicates whether Automate should ignore this chest.</summary>
        private Checkbox EditAutomateIgnore;

        /// <summary>The clickable area which saves the edit form.</summary>
        private ClickableComponent EditSaveButtonArea;

        /// <summary>The clickable area which clears data for the edit form.</summary>
        private ClickableComponent EditResetButtonArea;

        /// <summary>The top-right button which closes the edit form.</summary>
        private ClickableTextureComponent EditExitButton;


        /*********
        ** Accessors
        *********/
        /// <summary>The menu instance for which the overlay was created.</summary>
        public IClickableMenu ForMenuInstance { get; }

        /// <summary>An event raised when the player selects a chest.</summary>
        public event Action<ManagedChest> OnChestSelected;


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="menu">The underlying chest menu.</param>
        /// <param name="chest">The selected chest.</param>
        /// <param name="chests">The available chests.</param>
        /// <param name="config">The mod configuration.</param>
        /// <param name="keys">The configured key bindings.</param>
        /// <param name="events">The SMAPI events available for mods.</param>
        /// <param name="input">An API for checking and changing input state.</param>
        /// <param name="translations">Provides translations stored in the mod's folder.</param>
        /// <param name="showAutomateOptions">Whether to show Automate options.</param>
        public ManageChestOverlay(ItemGrabMenu menu, ManagedChest chest, ManagedChest[] chests, ModConfig config, ModConfigKeys keys, IModEvents events, IInputHelper input, ITranslationHelper translations, bool showAutomateOptions)
            : base(events, input, keepAlive: () => Game1.activeClickableMenu is ItemGrabMenu)
        {
            this.ForMenuInstance = menu;
            this.ShowAutomateOptions = showAutomateOptions;

            // helpers
            this.Translations = translations;

            // menu
            this.Menu = menu;
            this.MenuInventoryMenu = menu.ItemsToGrabMenu;
            this.DefaultChestHighlighter = menu.inventory.highlightMethod;
            this.DefaultInventoryHighlighter = this.MenuInventoryMenu.highlightMethod;

            // chests & config
            this.Chest = chest;
            this.Chests = chests;
            this.Categories = chests.Select(p => p.DisplayCategory).Distinct().OrderBy(p => p).ToArray();
            this.Config = config;
            this.Keys = keys;

            // components
            this.ReinitialiseComponents();
        }

        /// <summary>Sort the player's inventory.</summary>
        public void SortInventory()
        {
            ItemGrabMenu.organizeItemsInList(Game1.player.Items);
            Game1.playSound("Ship");
        }

        /// <summary>Switch to the specified chest.</summary>
        /// <param name="chest">The chest to select.</param>
        public void SelectChest(ManagedChest chest)
        {
            this.OnChestSelected?.Invoke(chest);
        }

        /// <summary>Release all resources.</summary>
        public override void Dispose()
        {
            this.OnChestSelected = null;
            base.Dispose();
        }


        /*********
        ** Protected methods
        *********/
        /// <summary>Draw the overlay to the screen.</summary>
        /// <param name="batch">The sprite batch being drawn.</param>
        protected override void Draw(SpriteBatch batch)
        {
            this.DrawCount++;
            Rectangle bounds = new Rectangle(this.Menu.xPositionOnScreen, this.Menu.yPositionOnScreen, this.Menu.width, this.Menu.height);

            // access mode
            if (!this.ActiveElement.HasFlag(Element.EditForm))
            {
                float navOpacity = this.CanCloseChest ? 1f : 0.5f;

                // tabs
                this.ChestTab.Draw(batch, navOpacity);
                this.CategoryTab?.Draw(batch, navOpacity);

                // tab dropdowns
                if (this.ActiveElement == Element.ChestList)
                    this.ChestSelector.Draw(batch, navOpacity);
                if (this.ActiveElement == Element.CategoryList)
                    this.CategorySelector.Draw(batch, navOpacity);

                // edit button
                if (this.Chest.CanEdit)
                    this.EditButton.draw(batch, Color.White * navOpacity, 1f);

                // sort inventory button
                this.SortInventoryButton?.draw(batch, Color.White * navOpacity, 1f);
            }

            // edit mode
            else
            {
                // get translations
                string locationLabel = this.Translations.Get("label.location") + ":";
                string nameLabel = this.Translations.Get("label.name") + ":";
                string categoryLabel = this.Translations.Get("label.category") + ":";
                string orderLabel = this.Translations.Get("label.order") + ":";

                // get initial measurements
                SpriteFont font = Game1.smallFont;
                const int gutter = 10;
                int padding = Game1.pixelZoom * 10;
                float topOffset = padding;
                int maxLabelWidth = (int)new[] { locationLabel, nameLabel, categoryLabel, orderLabel }.Select(p => font.MeasureString(p).X).Max();

                // background
                batch.DrawMenuBackground(new Rectangle(bounds.X, bounds.Y, bounds.Width, bounds.Height));

                // Location name
                {
                    string locationName = this.Chest.Location.Name;
                    if (this.Chest.Tile != Vector2.Zero)
                        locationName += " (" + this.Translations.Get("label.location.tile", new { x = this.Chest.Tile.X, y = this.Chest.Tile.Y }) + ")";

                    Vector2 labelSize = batch.DrawTextBlock(font, locationLabel, new Vector2(bounds.X + padding + (int)(maxLabelWidth - font.MeasureString(locationLabel).X), bounds.Y + topOffset), bounds.Width);
                    batch.DrawTextBlock(font, locationName, new Vector2(bounds.X + padding + maxLabelWidth + gutter, bounds.Y + topOffset), bounds.Width);
                    topOffset += labelSize.Y;
                }

                // editable text fields
                var fields = new[] { Tuple.Create(nameLabel, this.EditNameField), Tuple.Create(categoryLabel, this.EditCategoryField), Tuple.Create(orderLabel, this.EditOrderField) }.Where(p => p != null);
                foreach (var field in fields)
                {
                    // get data
                    string label = field.Item1;
                    ValidatedTextBox textbox = field.Item2;

                    // draw label
                    Vector2 labelSize = batch.DrawTextBlock(font, label, new Vector2(bounds.X + padding + (int)(maxLabelWidth - font.MeasureString(label).X), bounds.Y + topOffset + 7), bounds.Width);
                    textbox.X = bounds.X + padding + maxLabelWidth + gutter;
                    textbox.Y = bounds.Y + (int)topOffset;
                    textbox.Draw(batch);

                    // update offset
                    topOffset += Math.Max(labelSize.Y + 7, textbox.Height);
                }

                // checkboxes
                topOffset += this.DrawAndPositionCheckbox(batch, font, this.EditHideChestField, bounds.X + padding, bounds.Y + (int)topOffset, this.EditHideChestField.Value ? "label.hide-chest-hidden" : "label.hide-chest").Y;
                if (this.ShowAutomateOptions)
                {
                    topOffset += this.DrawAndPositionCheckbox(batch, font, this.EditAutomateOutput, bounds.X + padding, bounds.Y + (int)topOffset, "label.automate-prefer-output").Y;
                    topOffset += this.DrawAndPositionCheckbox(batch, font, this.EditAutomateNoInput, bounds.X + padding, bounds.Y + (int)topOffset, "label.automate-no-input").Y;
                    topOffset += this.DrawAndPositionCheckbox(batch, font, this.EditAutomateNoOutput, bounds.X + padding, bounds.Y + (int)topOffset, "label.automate-no-output").Y;
                    topOffset += this.DrawAndPositionCheckbox(batch, font, this.EditAutomateIgnore, bounds.X + padding, bounds.Y + (int)topOffset, "label.automate-ignore").Y;
                }

                // buttons
                this.DrawButton(batch, Game1.smallFont, this.EditSaveButtonArea, bounds.X + padding, bounds.Y + (int)topOffset, "button.save", Color.DarkGreen, out Rectangle saveButtonBounds);
                this.DrawButton(batch, Game1.smallFont, this.EditResetButtonArea, bounds.X + padding + saveButtonBounds.Width + 2, bounds.Y + (int)topOffset, "button.reset", Color.DarkRed, out _);

                // exit button
                this.EditExitButton.draw(batch);
            }

            // cursor
            this.DrawCursor();
        }

        /****
        ** Event handlers
        ****/
        /// <summary>The method invoked when the player resizes the game windoww.</summary>
        /// <param name="oldBounds">The previous game window bounds.</param>
        /// <param name="newBounds">The new game window bounds.</param>
        protected override void ReceiveGameWindowResized(xTile.Dimensions.Rectangle oldBounds, xTile.Dimensions.Rectangle newBounds)
        {
            this.ChestSelector.ReceiveGameWindowResized();
            this.CategorySelector?.ReceiveGameWindowResized();
            this.ReinitialiseComponents();
        }

        /// <summary>The method invoked when the player presses a button.</summary>
        /// <param name="input">The button that was pressed.</param>
        /// <returns>Whether the event has been handled and shouldn't be propagated further.</returns>
        protected override bool ReceiveButtonPress(SButton input)
        {
            if (this.IsInitialising())
                return false;

            bool canNavigate = this.CanCloseChest;
            ModConfigKeys keys = this.Keys;
            switch (this.ActiveElement)
            {
                case Element.Menu:
                    if (keys.Toggle.Contains(input) || input == SButton.Escape || input == SButton.ControllerB)
                    {
                        if (canNavigate)
                            this.Exit();
                    }
                    else if (keys.PrevChest.Contains(input) && canNavigate)
                        this.SelectPreviousChest();
                    else if (keys.NextChest.Contains(input) && canNavigate)
                        this.SelectNextChest();
                    else if (keys.PrevCategory.Contains(input) && canNavigate)
                        this.SelectPreviousCategory();
                    else if (keys.NextCategory.Contains(input) && canNavigate)
                        this.SelectNextCategory();
                    else if (this.Chest.CanEdit && keys.EditChest.Contains(input) && canNavigate)
                        this.OpenEdit();
                    else if (keys.SortItems.Contains(input))
                        this.SortInventory();
                    else
                        return false;
                    return true;

                case Element.ChestList:
                case Element.CategoryList:
                case Element.EditForm:
                    if (input == SButton.Escape || input == SButton.ControllerB)
                        this.ActiveElement = Element.Menu;
                    return true;

                default:
                    return false;
            }
        }

        /// <summary>The method invoked when the player scrolls the dropdown using the mouse wheel.</summary>
        /// <param name="amount">The scroll direction.</param>
        /// <returns>Whether the event has been handled and shouldn't be propagated further.</returns>
        protected override bool ReceiveScrollWheelAction(int amount)
        {
            switch (this.ActiveElement)
            {
                case Element.Menu:
                    bool scrollNext = amount > 0;

                    // scroll dropdowns
                    if (this.Keys.HoldToMouseWheelScrollCategories.Any(p => this.InputHelper.IsDown(p)))
                    {
                        if (scrollNext)
                            this.SelectNextCategory();
                        else
                            this.SelectPreviousCategory();
                        return true;
                    }
                    if (this.Keys.HoldToMouseWheelScrollChests.Any(p => this.InputHelper.IsDown(p)))
                    {
                        if (scrollNext)
                            this.SelectNextChest();
                        else
                            this.SelectPreviousChest();
                    }
                    return false;

                case Element.ChestList:
                    this.ChestSelector.ReceiveScrollWheelAction(amount);
                    return true;

                case Element.CategoryList:
                    this.CategorySelector?.ReceiveScrollWheelAction(amount);
                    return true;

                case Element.EditForm:
                    return true; // suppress input

                default:
                    return false;
            }
        }

        /// <summary>The method invoked when the player left-clicks.</summary>
        /// <param name="x">The X-position of the cursor.</param>
        /// <param name="y">The Y-position of the cursor.</param>
        /// <returns>Whether the event has been handled and shouldn't be propagated further.</returns>
        protected override bool ReceiveLeftClick(int x, int y)
        {
            switch (this.ActiveElement)
            {
                // edit form
                case Element.EditForm:
                    // name field
                    if (this.EditNameField.GetBounds().Contains(x, y))
                        this.EditNameField.Select();

                    // category field
                    else if (this.EditCategoryField.GetBounds().Contains(x, y))
                        this.EditCategoryField.Select();

                    // order field
                    else if (this.EditOrderField.GetBounds().Contains(x, y))
                        this.EditOrderField.Select();

                    // checkbox
                    else if (this.EditHideChestField.GetBounds().Contains(x, y))
                        this.EditHideChestField.Toggle();
                    else if (this.EditAutomateOutput.GetBounds().Contains(x, y))
                        this.EditAutomateOutput.Toggle();
                    else if (this.EditAutomateNoInput.GetBounds().Contains(x, y))
                        this.EditAutomateNoInput.Toggle();
                    else if (this.EditAutomateNoOutput.GetBounds().Contains(x, y))
                        this.EditAutomateNoOutput.Toggle();
                    else if (this.EditAutomateIgnore.GetBounds().Contains(x, y))
                        this.EditAutomateIgnore.Toggle();

                    // save button
                    else if (this.EditSaveButtonArea.containsPoint(x, y))
                    {
                        this.SaveEdit();
                        this.ActiveElement = Element.Menu;
                    }

                    // reset button
                    else if (this.EditResetButtonArea.containsPoint(x, y))
                        this.ResetEdit();

                    // exit button
                    else if (this.EditExitButton.containsPoint(x, y))
                        this.ActiveElement = Element.Menu;

                    return true; // handle all clicks while open

                // chest list
                case Element.ChestList:
                    // close dropdown
                    this.ActiveElement = Element.Menu;

                    // select chest
                    if (this.ChestSelector.containsPoint(x, y))
                    {
                        ManagedChest chest = this.ChestSelector.Select(x, y);
                        if (chest != null)
                        {
                            this.SelectChest(chest);
                            this.ReinitialiseComponents();
                        }
                    }
                    return true; // handle all clicks while open

                // category list
                case Element.CategoryList:
                    // close dropdown
                    this.ActiveElement = Element.Menu;

                    // select category
                    if (this.CategorySelector.containsPoint(x, y))
                    {
                        string category = this.CategorySelector.Select(x, y);
                        if (category != null && category != this.SelectedCategory)
                        {
                            this.SelectChest(this.Chests.First(chest => chest.DisplayCategory == category));
                            this.ReinitialiseComponents();
                        }
                    }
                    return true; // handle all clicks while open

                // buttons & dropdown
                default:
                    bool canNavigate = this.CanCloseChest;
                    if (this.Menu.okButton.containsPoint(x, y) && canNavigate)
                        this.Exit(); // in some cases the game won't handle this correctly (e.g. Stardew Valley Fair fishing minigame)
                    else if (this.EditButton.containsPoint(x, y) && canNavigate)
                        this.OpenEdit();
                    else if (this.SortInventoryButton?.containsPoint(x, y) == true)
                        this.SortInventory();
                    else if (this.ChestTab.containsPoint(x, y) && canNavigate)
                        this.ActiveElement = Element.ChestList;
                    else if (this.CategoryTab?.containsPoint(x, y) == true && canNavigate)
                        this.ActiveElement = Element.CategoryList;
                    else
                        return false;
                    return true;
            }
        }

        /// <summary>The method invoked when the cursor is hovered.</summary>
        /// <param name="x">The cursor's X position.</param>
        /// <param name="y">The cursor's Y position.</param>
        /// <returns>Whether the event has been handled and shouldn't be propagated further.</returns>
        protected override bool ReceiveCursorHover(int x, int y)
        {
            switch (this.ActiveElement)
            {
                case Element.Menu:
                    this.EditButton.tryHover(x, y);
                    this.SortInventoryButton?.tryHover(x, y);
                    return false;

                case Element.EditForm:
                    this.EditExitButton.tryHover(x, y);
                    return true;

                case Element.ChestList:
                case Element.CategoryList:
                    return true; // suppress menu hover

                default:
                    return false;
            }
        }


        /*********
        ** Private methods
        *********/
        /// <summary>Initialise the edit-chest overlay for rendering.</summary>
        private void ReinitialiseComponents()
        {
            Rectangle bounds = new Rectangle(this.Menu.xPositionOnScreen, this.Menu.yPositionOnScreen, this.Menu.width, this.Menu.height);

            // category dropdown
            if (this.ShowCategoryDropdown)
            {
                // tab
                Vector2 tabSize = Tab.GetTabSize(this.Font, this.SelectedCategory);
                this.CategoryTab = new Tab(this.SelectedCategory, bounds.Right - (int)tabSize.X - Game1.tileSize, bounds.Y - Game1.pixelZoom * 25, true, this.Font);

                // dropdown
                this.CategorySelector = new DropList<string>(this.SelectedCategory, this.Categories, category => category, this.CategoryTab.bounds.Right, this.CategoryTab.bounds.Bottom, false, this.Font);
            }

            // chest dropdown
            {
                // tab
                this.ChestTab = new Tab(this.Chest.DisplayName, bounds.X, bounds.Y - Game1.pixelZoom * 25, true, this.Font);

                // dropdown
                ManagedChest[] chests = this.Chests.Where(chest => !this.ShowCategoryDropdown || chest.DisplayCategory == this.SelectedCategory).ToArray();
                this.ChestSelector = new DropList<ManagedChest>(this.Chest, chests, chest => chest.DisplayName, this.ChestTab.bounds.X, this.ChestTab.bounds.Bottom, true, this.Font);
            }

            // edit chest button overlay (based on chest dropdown position)
            {
                Rectangle sprite = Sprites.Icons.SpeechBubble;
                float zoom = Game1.pixelZoom / 2f;
                Rectangle buttonBounds = new Rectangle(this.ChestTab.bounds.X + this.ChestTab.bounds.Width, this.ChestTab.bounds.Y, (int)(sprite.Width * zoom), (int)(sprite.Height * zoom));
                this.EditButton = new ClickableTextureComponent("edit-chest", buttonBounds, null, this.Translations.Get("button.edit-chest"), Sprites.Icons.Sheet, sprite, zoom);
            }

            // sort inventory button overlay (based on OK button position)
            if (this.Config.AddOrganisePlayerInventoryButton)
            {
                Rectangle sprite = Sprites.Buttons.Organize;
                ClickableTextureComponent okButton = this.Menu.okButton;
                float zoom = Game1.pixelZoom;
                Rectangle buttonBounds = new Rectangle(okButton.bounds.X, (int)(okButton.bounds.Y - sprite.Height * zoom - 5 * zoom), (int)(sprite.Width * zoom), (int)(sprite.Height * zoom));
                this.SortInventoryButton = new ClickableTextureComponent("sort-inventory", buttonBounds, null, this.Translations.Get("button.sort-inventory"), Sprites.Icons.Sheet, sprite, zoom);
            }
            else
                this.SortInventoryButton = null;

            // edit form
            int longTextWidth = (int)Game1.smallFont.MeasureString("A sufficiently, reasonably long string").X;
            this.EditNameField = new ValidatedTextBox(Game1.smallFont, Color.Black, ch => ch != '|') { Width = longTextWidth };
            this.EditCategoryField = new ValidatedTextBox(Game1.smallFont, Color.Black, ch => ch != '|') { Width = longTextWidth };
            this.EditOrderField = new ValidatedTextBox(Game1.smallFont, Color.Black, char.IsDigit) { Width = (int)Game1.smallFont.MeasureString("9999999").X };
            this.EditHideChestField = new Checkbox();
            this.EditAutomateOutput = new Checkbox();
            this.EditAutomateNoInput = new Checkbox();
            this.EditAutomateNoOutput = new Checkbox();
            this.EditAutomateIgnore = new Checkbox();
            this.FillForm();

            this.EditSaveButtonArea = new ClickableComponent(new Rectangle(0, 0, Game1.tileSize, Game1.tileSize), "save-chest");
            this.EditResetButtonArea = new ClickableComponent(new Rectangle(0, 0, Game1.tileSize, Game1.tileSize), "reset-chest");
            this.EditExitButton = new ClickableTextureComponent(new Rectangle(bounds.Right - 9 * Game1.pixelZoom, bounds.Y - Game1.pixelZoom * 2, Sprites.Icons.ExitButton.Width * Game1.pixelZoom, Sprites.Icons.ExitButton.Height * Game1.pixelZoom), Sprites.Icons.Sheet, Sprites.Icons.ExitButton, Game1.pixelZoom);

            // adjust menu to fit
            if (this.Config.AddOrganisePlayerInventoryButton)
                this.Menu.trashCan.bounds.Y = this.SortInventoryButton.bounds.Y - this.Menu.trashCan.bounds.Height - 2 * Game1.pixelZoom;
        }

        /// <summary>Set the form values to match the underlying chest.</summary>
        private void FillForm()
        {
            this.EditNameField.Text = this.Chest.DisplayName;
            this.EditCategoryField.Text = this.Chest.DisplayCategory;
            this.EditOrderField.Text = this.Chest.Order?.ToString();
            this.EditHideChestField.Value = this.Chest.IsIgnored;
            this.EditAutomateOutput.Value = this.Chest.ShouldAutomatePreferForOutput;
            this.EditAutomateNoInput.Value = this.Chest.ShouldAutomateNoInput;
            this.EditAutomateNoOutput.Value = this.Chest.ShouldAutomateNoOutput;
            this.EditAutomateIgnore.Value = this.Chest.ShouldAutomateIgnore;
        }

        /// <summary>Reset the edit form to the default values.</summary>
        private void ResetEdit()
        {
            this.Chest.Reset();
            this.FillForm();
        }

        /// <summary>Save the form input.</summary>
        private void SaveEdit()
        {
            // get order value
            int? order = null;
            {
                if (int.TryParse(this.EditOrderField.Text, out int parsed))
                    order = parsed;
            }

            // update chest
            this.Chest.Update(
                name: this.EditNameField.Text,
                category: this.EditCategoryField.Text,
                order: order,
                ignored: this.EditHideChestField.Value,
                shouldAutomateIgnore: this.EditAutomateIgnore.Value,
                shouldAutomatePreferForOutput: this.EditAutomateOutput.Value,
                shouldAutomateNoInput: this.EditAutomateNoInput.Value,
                shouldAutomateNoOutput: this.EditAutomateNoOutput.Value
            );
            this.OnChestSelected?.Invoke(this.Chest);
        }

        /// <summary>Exit the chest menu.</summary>
        private void Exit()
        {
            this.Dispose();
            this.Menu.exitThisMenu();
        }

        /// <summary>Get the index of a chest in the selected category.</summary>
        /// <param name="chest">The chest to find.</param>
        /// <param name="chests">The chests to search.</param>
        private int GetChestIndex(ManagedChest chest, IEnumerable<ManagedChest> chests)
        {
            int i = 0;
            foreach (ManagedChest cur in chests)
            {
                if (cur.Container.IsSameAs(chest.Container))
                    return i;
                i++;
            }
            return -1;
        }

        /// <summary>Switch to the previous chest in the list.</summary>
        private void SelectPreviousChest()
        {
            ManagedChest[] chests = this.GetChestsFromCategory(this.SelectedCategory);
            int curIndex = this.GetChestIndex(this.Chest, chests);
            this.SelectChest(chests[curIndex != 0 ? curIndex - 1 : chests.Length - 1]);
        }

        /// <summary>Switch to the next chest in the list.</summary>
        private void SelectNextChest()
        {
            ManagedChest[] chests = this.GetChestsFromCategory(this.SelectedCategory);
            int curIndex = this.GetChestIndex(this.Chest, chests);
            this.SelectChest(chests[(curIndex + 1) % chests.Length]);
        }

        /// <summary>Switch to the previous category.</summary>
        private void SelectPreviousCategory()
        {
            int curIndex = Array.IndexOf(this.Categories, this.SelectedCategory);
            string category = this.Categories[curIndex != 0 ? curIndex - 1 : this.Categories.Length - 1];
            this.SelectChest(this.Chests.First(chest => chest.DisplayCategory == category));
        }

        /// <summary>Switch to the next category.</summary>
        private void SelectNextCategory()
        {
            int curIndex = Array.IndexOf(this.Categories, this.SelectedCategory);
            string category = this.Categories[(curIndex + 1) % this.Categories.Length];
            this.SelectChest(this.Chests.First(chest => chest.DisplayCategory == category));
        }

        /// <summary>Reset and display the edit screen.</summary>
        private void OpenEdit()
        {
            this.EditNameField.Text = this.Chest.DisplayName;
            this.EditCategoryField.Text = this.Chest.DisplayCategory;
            this.EditOrderField.Text = this.Chest.Order?.ToString();
            this.EditHideChestField.Value = this.Chest.IsIgnored;
            this.EditAutomateOutput.Value = this.Chest.ShouldAutomatePreferForOutput;
            this.EditAutomateNoInput.Value = this.Chest.ShouldAutomateNoInput;
            this.EditAutomateNoOutput.Value = this.Chest.ShouldAutomateNoOutput;
            this.EditAutomateIgnore.Value = this.Chest.ShouldAutomateIgnore;

            this.ActiveElement = Element.EditForm;
        }

        /// <summary>Get the chests in a given category.</summary>
        /// <param name="category">The chest category.</param>
        private ManagedChest[] GetChestsFromCategory(string category)
        {
            return this.Chests.Where(chest => chest.DisplayCategory == category).ToArray();
        }

        /// <summary>Get whether the menu is initialising itself.</summary>
        private bool IsInitialising()
        {
            return this.DrawCount < 10;
        }

        /// <summary>Set whether the chest or inventory items should be clickable.</summary>
        private void SetItemsClickable(bool clickable)
        {
            if (clickable)
            {
                this.Menu.inventory.highlightMethod = this.DefaultChestHighlighter;
                this.MenuInventoryMenu.highlightMethod = this.DefaultInventoryHighlighter;
            }
            else
            {
                this.Menu.inventory.highlightMethod = item => false;
                this.MenuInventoryMenu.highlightMethod = item => false;
            }
        }

        /// <summary>Draw a checkbox to the screen, including any position updates needed.</summary>
        /// <param name="batch">The sprite batch being drawn.</param>
        /// <param name="font">The font to use for the checkbox label.</param>
        /// <param name="checkbox">The checkbox to draw.</param>
        /// <param name="x">The top-left X position to start drawing from.</param>
        /// <param name="y">The top-left Y position to start drawing from.</param>
        /// <param name="textKey">The translation key for the checkbox label.</param>
        private Vector2 DrawAndPositionCheckbox(SpriteBatch batch, SpriteFont font, Checkbox checkbox, int x, int y, string textKey)
        {
            checkbox.X = x;
            checkbox.Y = y;
            checkbox.Width = 24;
            checkbox.Draw(batch);
            string label = this.Translations.Get(textKey);
            Vector2 labelSize = batch.DrawTextBlock(font, label, new Vector2(x + 7 + checkbox.Width, y), this.Menu.width, checkbox.Value ? Color.Red : Color.Black);

            return new Vector2(checkbox.Width + 7 + checkbox.Width + labelSize.X, Math.Max(checkbox.Width, labelSize.Y));
        }

        /// <summary>Draw a checkbox to the screen, including any position updates needed.</summary>
        /// <param name="batch">The sprite batch being drawn.</param>
        /// <param name="font">The font to use for the checkbox label.</param>
        /// <param name="clickArea">The clickable area to draw.</param>
        /// <param name="x">The top-left X position to start drawing from.</param>
        /// <param name="y">The top-left Y position to start drawing from.</param>
        /// <param name="textKey">The translation key for the checkbox label.</param>
        /// <param name="color">The text color to draw.</param>
        /// <param name="bounds">The button's outer bounds.</param>
        private Vector2 DrawButton(SpriteBatch batch, SpriteFont font, ClickableComponent clickArea, int x, int y, string textKey, in Color color, out Rectangle bounds)
        {
            // get text
            string label = this.Translations.Get(textKey);
            Vector2 labelSize = font.MeasureString(label);

            // draw button
            CommonHelper.DrawButton(batch, new Vector2(x, y), labelSize, out Vector2 contentPos, out bounds);
            Utility.drawBoldText(batch, label, font, contentPos, color);

            // align clickable area
            clickArea.bounds = bounds;

            // return size
            return new Vector2(bounds.Width, bounds.Height);
        }
    }
}
