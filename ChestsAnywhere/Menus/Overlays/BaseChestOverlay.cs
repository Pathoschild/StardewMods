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
    /// <summary>The base overlay for a menu which lets the player navigate and edit containers.</summary>
    internal abstract class BaseChestOverlay : BaseOverlay, IStorageOverlay
    {
        /*********
        ** Fields
        *********/
        /****
        ** Data
        ****/
        /// <summary>Provides translations stored in the mod's folder.</summary>
        protected readonly ITranslationHelper Translations;

        /// <summary>The available chests.</summary>
        private readonly ManagedChest[] Chests;

        /// <summary>The selected chest.</summary>
        private readonly ManagedChest Chest;

        /// <summary>Whether to show Automate options.</summary>
        private readonly bool ShowAutomateOptions;

        /// <summary>The number of draw cycles since the menu was initialized.</summary>
        private int DrawCount;

        /// <summary>Get whether the menu and its components have been initialized.</summary>
        protected bool IsInitialized => this.DrawCount > 1;

        /// <summary>The backing field for <see cref="ActiveElement"/>; shouldn't be edited directly.</summary>
        private Element _activeElement;

        /// <summary>The overlay element which should receive input.</summary>
        protected Element ActiveElement
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
        protected ModConfig Config { get; }

        /// <summary>The configured key bindings.</summary>
        private readonly ModConfigKeys Keys;

        /// <summary>Whether to show the category dropdown.</summary>
        protected bool ShowCategoryDropdown => this.Categories.Length > 1;

        /****
        ** Menu management
        ****/
        /// <summary>The underlying chest menu.</summary>
        private readonly IClickableMenu Menu;

        /// <summary>Whether the chest menu is ready to close.</summary>
        protected bool CanCloseChest => this.Menu.readyToClose();

        /****
        ** Access UI
        ****/
        /// <summary>The font with which to render text.</summary>
        private readonly SpriteFont Font = Game1.smallFont;

        /// <summary>The chest selector tab.</summary>
        protected Tab ChestTab;

        /// <summary>The category selector tab.</summary>
        protected Tab CategoryTab;

        /// <summary>The chest selector dropdown.</summary>
        protected DropList<ManagedChest> ChestSelector;

        /// <summary>The category selector dropdown.</summary>
        protected DropList<string> CategorySelector;

        /// <summary>The edit button.</summary>
        protected ClickableTextureComponent EditButton;

        /// <summary>The Y offset to apply relative to <see cref="IClickableMenu.yPositionOnScreen"/> when drawing the top UI elements.</summary>
        private readonly int TopOffset;

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

        /// <summary>A checkbox which indicates whether Automate should store items in this chest.</summary>
        private Checkbox EditAutomateStoreItems;

        /// <summary>A checkbox which indicates whether Automate should store items in this chest first.</summary>
        private Checkbox EditAutomateStoreItemsPreferred;

        /// <summary>A checkbox which indicates whether Automate should take items from this chest.</summary>
        private Checkbox EditAutomateTakeItems;

        /// <summary>A checkbox which indicates whether Automate should take items from this chest first.</summary>
        private Checkbox EditAutomateTakeItemsPreferred;

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

        /// <summary>An event raised when the Automate options for a chest change.</summary>
        public event Action<ManagedChest> OnAutomateOptionsChanged;


        /*********
        ** Public methods
        *********/
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
        /// <summary>Construct an instance.</summary>
        /// <param name="menu">The underlying chest menu.</param>
        /// <param name="chest">The selected chest.</param>
        /// <param name="chests">The available chests.</param>
        /// <param name="config">The mod configuration.</param>
        /// <param name="keys">The configured key bindings.</param>
        /// <param name="events">The SMAPI events available for mods.</param>
        /// <param name="input">An API for checking and changing input state.</param>
        /// <param name="reflection">Simplifies access to private code.</param>
        /// <param name="translations">Provides translations stored in the mod's folder.</param>
        /// <param name="showAutomateOptions">Whether to show Automate options.</param>
        /// <param name="keepAlive">Indicates whether to keep the overlay active. If <c>null</c>, the overlay is kept until explicitly disposed.</param>
        /// <param name="topOffset">The Y offset to apply relative to <see cref="IClickableMenu.yPositionOnScreen"/> when drawing the top UI elements.</param>
        protected BaseChestOverlay(IClickableMenu menu, ManagedChest chest, ManagedChest[] chests, ModConfig config, ModConfigKeys keys, IModEvents events, IInputHelper input, IReflectionHelper reflection, ITranslationHelper translations, bool showAutomateOptions, Func<bool> keepAlive, int topOffset = 0)
            : base(events, input, reflection, keepAlive)
        {
            // data
            this.ForMenuInstance = menu;
            this.ShowAutomateOptions = showAutomateOptions;
            this.TopOffset = topOffset;

            // helpers
            this.Translations = translations;

            // menu
            this.Menu = menu;

            // chests & config
            this.Chest = chest;
            this.Chests = chests;
            this.Categories = chests.Select(p => p.DisplayCategory).Distinct().OrderBy(p => p).ToArray();
            this.Config = config;
            this.Keys = keys;
        }

        /// <summary>Draw the overlay to the screen.</summary>
        /// <param name="batch">The sprite batch being drawn.</param>
        protected override void Draw(SpriteBatch batch)
        {
            if (this.DrawCount == 0)
                this.ReinitializeComponents();

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
                int indent = Game1.pixelZoom * 3;
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
                    topOffset += this.DrawAndPositionCheckbox(batch, font, this.EditAutomateStoreItems, bounds.X + padding, bounds.Y + (int)topOffset, "label.automate-store", defaultValue: true).Y;
                    if (this.EditAutomateStoreItems.Value)
                        topOffset += this.DrawAndPositionCheckbox(batch, font, this.EditAutomateStoreItemsPreferred, bounds.X + padding + indent, bounds.Y + (int)topOffset, "label.automate-store-first").Y;
                    topOffset += this.DrawAndPositionCheckbox(batch, font, this.EditAutomateTakeItems, bounds.X + padding, bounds.Y + (int)topOffset, "label.automate-take", defaultValue: true).Y;
                    if (this.EditAutomateTakeItems.Value)
                        topOffset += this.DrawAndPositionCheckbox(batch, font, this.EditAutomateTakeItemsPreferred, bounds.X + padding + indent, bounds.Y + (int)topOffset, "label.automate-take-first").Y;
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
        /// <summary>The method invoked when the player resizes the game window.</summary>
        /// <param name="oldBounds">The previous game window bounds.</param>
        /// <param name="newBounds">The new game window bounds.</param>
        protected override void ReceiveGameWindowResized(xTile.Dimensions.Rectangle oldBounds, xTile.Dimensions.Rectangle newBounds)
        {
            this.ReinitializeComponents();
        }

        /// <summary>The method invoked when the player presses a button.</summary>
        /// <param name="input">The button that was pressed.</param>
        /// <returns>Whether the event has been handled and shouldn't be propagated further.</returns>
        protected override bool ReceiveButtonPress(SButton input)
        {
            if (!this.IsInitialized)
                return false;

            bool canNavigate = this.CanCloseChest;
            ModConfigKeys keys = this.Keys;
            switch (this.ActiveElement)
            {
                case Element.Menu:
                    if (keys.Toggle.JustPressedUnique() || input == SButton.Escape || input == SButton.ControllerB)
                    {
                        if (canNavigate)
                            this.Exit();
                    }
                    else if (keys.PrevChest.JustPressedUnique() && canNavigate)
                        this.SelectPreviousChest();
                    else if (keys.NextChest.JustPressedUnique() && canNavigate)
                        this.SelectNextChest();
                    else if (keys.PrevCategory.JustPressedUnique() && canNavigate)
                        this.SelectPreviousCategory();
                    else if (keys.NextCategory.JustPressedUnique() && canNavigate)
                        this.SelectNextCategory();
                    else if (this.Chest.CanEdit && keys.EditChest.JustPressedUnique() && canNavigate)
                        this.OpenEdit();
                    else if (keys.SortItems.JustPressedUnique())
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
            if (!this.IsInitialized)
                return false;

            switch (this.ActiveElement)
            {
                case Element.Menu:
                    bool scrollNext = amount < 0;

                    // scroll dropdowns
                    if (this.Keys.HoldToMouseWheelScrollCategories.IsDown())
                    {
                        if (scrollNext)
                            this.SelectNextCategory();
                        else
                            this.SelectPreviousCategory();
                        return true;
                    }
                    if (this.Keys.HoldToMouseWheelScrollChests.IsDown())
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
            if (!this.IsInitialized)
                return false;

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
                    else if (this.EditAutomateStoreItems.GetBounds().Contains(x, y))
                        this.EditAutomateStoreItems.Toggle();
                    else if (this.EditAutomateStoreItems.Value && this.EditAutomateStoreItemsPreferred.GetBounds().Contains(x, y)) // hidden if store items disabled
                        this.EditAutomateStoreItemsPreferred.Toggle();
                    else if (this.EditAutomateTakeItems.GetBounds().Contains(x, y))
                        this.EditAutomateTakeItems.Toggle();
                    else if (this.EditAutomateTakeItems.Value && this.EditAutomateTakeItemsPreferred.GetBounds().Contains(x, y)) // hidden if take items disabled
                        this.EditAutomateTakeItemsPreferred.Toggle();

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
                            this.ReinitializeComponents();
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
                            this.ReinitializeComponents();
                        }
                    }
                    return true; // handle all clicks while open

                // buttons & dropdown
                default:
                    bool canNavigate = this.CanCloseChest;
                    if (this.EditButton.containsPoint(x, y) && canNavigate)
                        this.OpenEdit();
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
            if (!this.IsInitialized)
                return false;

            switch (this.ActiveElement)
            {
                case Element.Menu:
                    this.EditButton.tryHover(x, y);
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
        /// <summary>Initialize the edit-chest overlay for rendering.</summary>
        protected virtual void ReinitializeComponents()
        {
            Rectangle bounds = new Rectangle(this.Menu.xPositionOnScreen, this.Menu.yPositionOnScreen, this.Menu.width, this.Menu.height);

            // category dropdown
            if (this.ShowCategoryDropdown)
            {
                // tab
                Vector2 tabSize = Tab.GetTabSize(this.Font, this.SelectedCategory);
                this.CategoryTab = Constants.TargetPlatform == GamePlatform.Android
                    ? new Tab(this.SelectedCategory, bounds.Right - (int)tabSize.X - Game1.tileSize, bounds.Y, true, this.Font)
                    : new Tab(this.SelectedCategory, bounds.Right - (int)tabSize.X - Game1.tileSize, bounds.Y - (int)tabSize.Y + this.TopOffset, true, this.Font);

                // dropdown
                this.CategorySelector = new DropList<string>(this.SelectedCategory, this.Categories, category => category, this.CategoryTab.bounds.Right, this.CategoryTab.bounds.Bottom, false, this.Font);
            }

            // chest dropdown
            {
                // tab
                Vector2 tabSize = Tab.GetTabSize(this.Font, this.Chest.DisplayName);
                this.ChestTab = Constants.TargetPlatform == GamePlatform.Android
                    ? new Tab(this.Chest.DisplayName, bounds.X, bounds.Y, true, this.Font)
                    : new Tab(this.Chest.DisplayName, bounds.X, bounds.Y - (int)tabSize.Y + this.TopOffset, true, this.Font);

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

            // edit form
            int longTextWidth = (int)Game1.smallFont.MeasureString("A sufficiently, reasonably long string").X;
            this.EditNameField = new ValidatedTextBox(Game1.smallFont, Color.Black, ch => ch != '|', this.Reflection) { Width = longTextWidth };
            this.EditCategoryField = new ValidatedTextBox(Game1.smallFont, Color.Black, ch => ch != '|', this.Reflection) { Width = longTextWidth };
            this.EditOrderField = new ValidatedTextBox(Game1.smallFont, Color.Black, char.IsDigit, this.Reflection) { Width = (int)Game1.smallFont.MeasureString("9999999").X };
            this.EditHideChestField = new Checkbox();
            this.EditAutomateStoreItems = new Checkbox();
            this.EditAutomateStoreItemsPreferred = new Checkbox();
            this.EditAutomateTakeItems = new Checkbox();
            this.EditAutomateTakeItemsPreferred = new Checkbox();
            this.FillForm();

            this.EditSaveButtonArea = new ClickableComponent(new Rectangle(0, 0, Game1.tileSize, Game1.tileSize), "save-chest");
            this.EditResetButtonArea = new ClickableComponent(new Rectangle(0, 0, Game1.tileSize, Game1.tileSize), "reset-chest");
            this.EditExitButton = new ClickableTextureComponent(new Rectangle(bounds.Right - 9 * Game1.pixelZoom, bounds.Y - Game1.pixelZoom * 2, Sprites.Icons.ExitButton.Width * Game1.pixelZoom, Sprites.Icons.ExitButton.Height * Game1.pixelZoom), Sprites.Icons.Sheet, Sprites.Icons.ExitButton, Game1.pixelZoom);
        }

        /// <summary>Set the form values to match the underlying chest.</summary>
        private void FillForm()
        {
            this.EditNameField.Text = this.Chest.DisplayName;
            this.EditCategoryField.Text = this.Chest.DisplayCategory;
            this.EditOrderField.Text = this.Chest.Order?.ToString();
            this.EditHideChestField.Value = this.Chest.IsIgnored;
            this.EditAutomateStoreItems.Value = this.Chest.AutomateStoreItems.IsAllowed();
            this.EditAutomateStoreItemsPreferred.Value = this.Chest.AutomateStoreItems.IsPreferred();
            this.EditAutomateTakeItems.Value = this.Chest.AutomateTakeItems.IsAllowed();
            this.EditAutomateTakeItemsPreferred.Value = this.Chest.AutomateTakeItems.IsPreferred();
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
            ContainerAutomatePreference automateStore = this.GetAutomatePreference(allow: this.EditAutomateStoreItems.Value, prefer: this.EditAutomateStoreItemsPreferred.Value);
            ContainerAutomatePreference automateTake = this.GetAutomatePreference(allow: this.EditAutomateTakeItems.Value, prefer: this.EditAutomateTakeItemsPreferred.Value);
            bool automateChanged = this.Chest.CanConfigureAutomate && (automateStore != this.Chest.AutomateStoreItems || automateTake != this.Chest.AutomateTakeItems);
            this.Chest.Update(
                name: this.EditNameField.Text,
                category: this.EditCategoryField.Text,
                order: order,
                ignored: this.EditHideChestField.Value,
                automateStoreItems: automateStore,
                automateTakeItems: automateTake
            );
            this.OnChestSelected?.Invoke(this.Chest);
            if (automateChanged)
                this.OnAutomateOptionsChanged?.Invoke(this.Chest);
        }

        /// <summary>Exit the chest menu.</summary>
        protected void Exit()
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
            this.EditAutomateStoreItems.Value = this.Chest.AutomateStoreItems.IsAllowed();
            this.EditAutomateStoreItemsPreferred.Value = this.Chest.AutomateStoreItems.IsPreferred();
            this.EditAutomateTakeItems.Value = this.Chest.AutomateTakeItems.IsAllowed();
            this.EditAutomateTakeItemsPreferred.Value = this.Chest.AutomateTakeItems.IsPreferred();

            this.ActiveElement = Element.EditForm;
        }

        /// <summary>Get the chests in a given category.</summary>
        /// <param name="category">The chest category.</param>
        private ManagedChest[] GetChestsFromCategory(string category)
        {
            return this.Chests.Where(chest => chest.DisplayCategory == category).ToArray();
        }

        /// <summary>Set whether the chest or inventory items should be clickable.</summary>
        /// <param name="clickable">Whether items should be clickable.</param>
        protected abstract void SetItemsClickable(bool clickable);

        /// <summary>Draw a checkbox to the screen, including any position updates needed.</summary>
        /// <param name="batch">The sprite batch being drawn.</param>
        /// <param name="font">The font to use for the checkbox label.</param>
        /// <param name="checkbox">The checkbox to draw.</param>
        /// <param name="x">The top-left X position to start drawing from.</param>
        /// <param name="y">The top-left Y position to start drawing from.</param>
        /// <param name="textKey">The translation key for the checkbox label.</param>
        /// <param name="defaultValue">The default value.</param>
        private Vector2 DrawAndPositionCheckbox(SpriteBatch batch, SpriteFont font, Checkbox checkbox, int x, int y, string textKey, bool defaultValue = false)
        {
            checkbox.X = x;
            checkbox.Y = y;
            checkbox.Width = 24;
            checkbox.Draw(batch);
            string label = this.Translations.Get(textKey);
            Vector2 labelSize = batch.DrawTextBlock(font, label, new Vector2(x + 7 + checkbox.Width, y), this.Menu.width, checkbox.Value != defaultValue ? Color.Red : Color.Black);

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

        /// <summary>Get an Automate IO preference.</summary>
        /// <param name="allow">Whether IO is allowed.</param>
        /// <param name="prefer">Whether IO is preferred.</param>
        private ContainerAutomatePreference GetAutomatePreference(bool allow, bool prefer)
        {
            if (allow && prefer)
                return ContainerAutomatePreference.Prefer;

            if (allow)
                return ContainerAutomatePreference.Allow;

            return ContainerAutomatePreference.Disable;
        }
    }
}
