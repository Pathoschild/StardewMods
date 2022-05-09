using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Pathoschild.Stardew.Automate.Framework;
using Pathoschild.Stardew.ChestsAnywhere.Framework;
using Pathoschild.Stardew.ChestsAnywhere.Menus.Components;
using Pathoschild.Stardew.Common;
using Pathoschild.Stardew.Common.UI;
using Pathoschild.Stardew.Common.Utilities;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
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

        /// <summary>The unique chest categories.</summary>
        private readonly string[] Categories;

        /// <summary>The name of the selected category.</summary>
        private string SelectedCategory => this.Chest.DisplayCategory;

        /// <summary>The mod configuration.</summary>
        protected ModConfig Config { get; }

        /// <summary>The configured key bindings.</summary>
        private readonly ModConfigKeys Keys;

        /// <summary>The keybind for escaping the active element or menu.</summary>
        private readonly KeybindList EscapeKeybind = KeybindList.Parse($"{SButton.Escape}, {SButton.ControllerB}");

        /// <summary>Whether to show the category dropdown.</summary>
        [MemberNotNullWhen(true, nameof(BaseChestOverlay.ChestDropdown))]
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

        /// <summary>The chest dropdown.</summary>
        protected Dropdown<ManagedChest> ChestDropdown;

        /// <summary>The category dropdown.</summary>
        protected Dropdown<string>? CategoryDropdown;

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

        /// <summary>A dropdown which configures how Automate stores items in this chest.</summary>
        private SimpleDropdown<AutomateContainerPreference> EditAutomateStorage;

        /// <summary>A dropdown which configures how Automate takes items from this chest.</summary>
        private SimpleDropdown<AutomateContainerPreference> EditAutomateFetch;

        /// <summary>A checkbox which configures whether Automate should avoid removing the last item in a stack.</summary>
        private Checkbox EditAutomatePreventRemovingStacks;

        /// <summary>The clickable area which saves the edit form.</summary>
        private ClickableComponent EditSaveButtonArea;

        /// <summary>The clickable area which clears data for the edit form.</summary>
        private ClickableComponent EditResetButtonArea;

        /// <summary>The top-right button which closes the edit form.</summary>
        private ClickableTextureComponent EditExitButton;

        /// <summary>The textboxes managed by Chests Anywhere.</summary>
        private IEnumerable<ValidatedTextBox> ManagedTextboxes => new[] { this.EditNameField, this.EditOrderField, this.EditCategoryField };


        /*********
        ** Accessors
        *********/
        /// <inheritdoc />
        public Element ActiveElement
        {
            get => this._activeElement;
            protected set
            {
                this._activeElement = value;
                this.OnActiveElementChanged(value);
            }
        }

        /// <inheritdoc />
        public event Action<ManagedChest>? OnChestSelected;

        /// <inheritdoc />
        public event Action<ManagedChest>? OnAutomateOptionsChanged;


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
            this.OnAutomateOptionsChanged = null;
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
        /// <param name="showAutomateOptions">Whether to show Automate options.</param>
        /// <param name="keepAlive">Indicates whether to keep the overlay active. If <c>null</c>, the overlay is kept until explicitly disposed.</param>
        /// <param name="topOffset">The Y offset to apply relative to <see cref="IClickableMenu.yPositionOnScreen"/> when drawing the top UI elements.</param>
        protected BaseChestOverlay(IClickableMenu menu, ManagedChest chest, ManagedChest[] chests, ModConfig config, ModConfigKeys keys, IModEvents events, IInputHelper input, IReflectionHelper reflection, bool showAutomateOptions, Func<bool> keepAlive, int topOffset = 0)
            : base(events, input, reflection, keepAlive, assumeUiMode: true)
        {
            // data
            this.ShowAutomateOptions = showAutomateOptions;
            this.TopOffset = topOffset;

            // menu
            this.Menu = menu;

            // chests & config
            this.Chest = chest;
            this.Chests = chests;
            this.Categories = chests.Select(p => p.DisplayCategory).Distinct().OrderBy(p => p, HumanSortComparer.DefaultIgnoreCase).ToArray();
            this.Config = config;
            this.Keys = keys;

            this.ReinitializeBaseComponents();
        }

        /// <summary>Draw the overlay to the screen over the UI.</summary>
        /// <param name="batch">The sprite batch being drawn.</param>
        protected override void DrawUi(SpriteBatch batch)
        {
            if (this.DrawCount == 0)
                this.ReinitializeComponents();

            this.DrawCount++;
            Rectangle bounds = new Rectangle(this.Menu.xPositionOnScreen, this.Menu.yPositionOnScreen, this.Menu.width, this.Menu.height);

            // access mode
            if (!this.ActiveElement.HasFlag(Element.EditForm))
            {
                float navOpacity = this.CanCloseChest ? 1f : 0.5f;

                // dropdowns
                this.ChestDropdown.Draw(batch, navOpacity);
                this.CategoryDropdown?.Draw(batch, navOpacity);

                // edit button
                this.EditButton.draw(batch, Color.White * navOpacity, 1f);
            }

            // edit mode
            else
            {
                // get translations
                string locationLabel = I18n.Label_Location() + ":";
                string nameLabel = I18n.Label_Name() + ":";
                string categoryLabel = I18n.Label_Category() + ":";
                string orderLabel = I18n.Label_Order() + ":";

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
                        locationName += " (" + I18n.Label_Location_Tile(x: this.Chest.Tile.X, y: this.Chest.Tile.Y) + ")";

                    Vector2 labelSize = batch.DrawTextBlock(font, locationLabel, new Vector2(bounds.X + padding + (int)(maxLabelWidth - font.MeasureString(locationLabel).X), bounds.Y + topOffset), bounds.Width);
                    batch.DrawTextBlock(font, locationName, new Vector2(bounds.X + padding + maxLabelWidth + gutter, bounds.Y + topOffset), bounds.Width);
                    topOffset += labelSize.Y;
                }

                // editable text fields
                var fields = new[] { Tuple.Create(nameLabel, this.EditNameField), Tuple.Create(categoryLabel, this.EditCategoryField), Tuple.Create(orderLabel, this.EditOrderField) };
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
                topOffset += this.DrawAndPositionCheckbox(batch, font, this.EditHideChestField, bounds.X + padding, bounds.Y + (int)topOffset, this.EditHideChestField.Value ? I18n.Label_HideChestHidden() : I18n.Label_HideChest()).Y;

                // buttons
                void DrawButtons(int yOffset)
                {
                    this.DrawButton(batch, Game1.smallFont, this.EditSaveButtonArea, bounds.X + padding, bounds.Y + (int)topOffset + yOffset, I18n.Button_Save(), Color.DarkGreen, out Rectangle saveButtonBounds);
                    this.DrawButton(batch, Game1.smallFont, this.EditResetButtonArea, bounds.X + padding + saveButtonBounds.Width + 2, bounds.Y + (int)topOffset + yOffset, I18n.Button_Reset(), Color.DarkRed, out _);
                }

                // Automate options
                // note: must be drawn in reverse order (including the buttons), so dropdowns overlay the elements below them
                if (this.ShowAutomateOptions)
                {
                    // label
                    topOffset += padding;
                    topOffset += batch.DrawTextBlock(Game1.smallFont, I18n.Label_AutomateOptions(), new Vector2(bounds.X + padding, bounds.Y + topOffset), wrapWidth: bounds.Width - bounds.X - padding, bold: true).Y;

                    // checkboxes
                    topOffset += this.DrawAndPositionCheckbox(batch, font, this.EditAutomatePreventRemovingStacks, bounds.X + padding, bounds.Y + (int)topOffset, I18n.Label_AutomatePreventRemoveStacks()).Y;

                    // buttons
                    DrawButtons(yOffset: this.EditAutomateStorage.Bounds.Height + this.EditAutomateFetch.Bounds.Height + this.EditAutomatePreventRemovingStacks.GetBounds().Height + padding);

                    // dropdowns
                    this.EditAutomateFetch.Draw(batch, bounds.X + padding, bounds.Y + (int)topOffset + this.EditAutomateStorage.Bounds.Height);
                    this.EditAutomateStorage.Draw(batch, bounds.X + padding, bounds.Y + (int)topOffset);
                }
                else
                    DrawButtons(yOffset: 0);


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
        protected override void ReceiveGameWindowResized()
        {
            this.ReinitializeComponents();
        }

        /// <inheritdoc cref="IInputEvents.ButtonsChanged"/>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event data.</param>
        protected override void ReceiveButtonsChanged(object? sender, ButtonsChangedEventArgs e)
        {
            if (!this.IsInitialized)
                return;

            // check for textbox focus
            bool anyTextboxSelected = Game1.game1.HasKeyboardFocus() && Game1.game1.instanceKeyboardDispatcher?.Subscriber != null;

            // handle keys
            bool canNavigate = this.CanCloseChest && !anyTextboxSelected;
            ModConfigKeys keys = this.Keys;
            switch (this.ActiveElement)
            {
                case Element.Menu:
                    if (keys.Toggle.JustPressed() || this.EscapeKeybind.JustPressed())
                    {
                        this.InputHelper.SuppressActiveKeybinds(keys.Toggle);
                        this.InputHelper.SuppressActiveKeybinds(this.EscapeKeybind);

                        if (canNavigate)
                            this.Exit();
                    }
                    else if (keys.PrevChest.JustPressed() && canNavigate)
                        this.SelectPreviousChest();
                    else if (keys.NextChest.JustPressed() && canNavigate)
                        this.SelectNextChest();
                    else if (keys.PrevCategory.JustPressed() && canNavigate)
                        this.SelectPreviousCategory();
                    else if (keys.NextCategory.JustPressed() && canNavigate)
                        this.SelectNextCategory();
                    else if (keys.EditChest.JustPressed() && canNavigate)
                        this.OpenEdit();
                    else if (keys.SortItems.JustPressed())
                        this.SortInventory();
                    else
                        return; // don't suppress if the key wasn't handled

                    this.SuppressAll(e.Pressed);
                    break;

                case Element.ChestList:
                case Element.CategoryList:
                case Element.EditForm:
                    if (this.EscapeKeybind.JustPressed())
                        this.ActiveElement = Element.Menu;

                    this.SuppressAll(e.Pressed); // always suppress in this context
                    break;
            }
        }

        /// <summary>Suppress all pressed buttons.</summary>
        /// <param name="buttons">The buttons to suppress.</param>
        protected void SuppressAll(IEnumerable<SButton> buttons)
        {
            foreach (SButton button in buttons)
                this.InputHelper.Suppress(button);
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
                    this.ChestDropdown.ReceiveScrollWheelAction(amount);
                    return true;

                case Element.CategoryList:
                    this.CategoryDropdown?.ReceiveScrollWheelAction(amount);
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
                    // textboxes
                    if (this.TryClickTextbox(x, y))
                    {
                        // handled internally
                    }

                    // hide chest checkbox
                    else if (this.EditHideChestField.GetBounds().Contains(x, y))
                        this.EditHideChestField.Toggle();

                    // Automate options
                    else if (this.EditAutomatePreventRemovingStacks.GetBounds().Contains(x, y))
                        this.EditAutomatePreventRemovingStacks.Toggle();
                    else if (this.EditAutomateStorage.TryClick(x, y) || this.EditAutomateFetch.TryClick(x, y))
                    {
                        // handled internally
                    }

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
                    {
                        // select chest
                        if (this.ChestDropdown.TryClick(x, y, out bool itemClicked, out bool dropdownToggled))
                        {
                            if (itemClicked)
                            {
                                this.SelectChest(this.ChestDropdown.Selected);
                                this.ReinitializeComponents();
                            }

                            if (dropdownToggled)
                                this.ActiveElement = this.ChestDropdown.IsExpanded ? Element.ChestList : Element.Menu;
                        }

                        // close dropdown
                        else
                            this.ActiveElement = Element.Menu;
                    }
                    return true; // handle all clicks while open

                // category list
                case Element.CategoryList:
                    {
                        // select category
                        if (this.CategoryDropdown!.TryClick(x, y, out bool itemClicked, out bool dropdownToggled))
                        {
                            if (itemClicked)
                            {
                                string category = this.CategoryDropdown.Selected;
                                if (category != this.SelectedCategory)
                                {
                                    this.SelectChest(this.Chests.First(chest => chest.DisplayCategory == category));
                                    this.ReinitializeComponents();
                                }
                            }

                            if (dropdownToggled)
                                this.ActiveElement = this.CategoryDropdown.IsExpanded ? Element.CategoryList : Element.Menu;
                        }

                        // close dropdown
                        else
                            this.ActiveElement = Element.Menu;
                    }
                    return true; // handle all clicks while open

                // buttons & dropdown
                default:
                    bool canNavigate = this.CanCloseChest;
                    if (this.EditButton.containsPoint(x, y) && canNavigate)
                        this.OpenEdit();
                    else if (this.ChestDropdown.TryClick(x, y) && canNavigate)
                    {
                        this.ChestDropdown.IsExpanded = true;
                        this.ActiveElement = Element.ChestList;
                    }
                    else if (this.CategoryDropdown?.TryClick(x, y) == true && canNavigate)
                    {
                        this.CategoryDropdown.IsExpanded = true;
                        this.ActiveElement = Element.CategoryList;
                    }
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
                    this.EditAutomateStorage.TryHover(x, y);
                    this.EditAutomateFetch.TryHover(x, y);
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
        /// <summary>Initialize the base edit-chest overlay for rendering.</summary>
        [MemberNotNull(
            nameof(BaseChestOverlay.ChestDropdown),
            nameof(BaseChestOverlay.EditButton),
            nameof(BaseChestOverlay.EditNameField),
            nameof(BaseChestOverlay.EditCategoryField),
            nameof(BaseChestOverlay.EditOrderField),
            nameof(BaseChestOverlay.EditHideChestField),
            nameof(BaseChestOverlay.EditAutomatePreventRemovingStacks),
            nameof(BaseChestOverlay.EditAutomateStorage),
            nameof(BaseChestOverlay.EditAutomateFetch),
            nameof(BaseChestOverlay.EditSaveButtonArea),
            nameof(BaseChestOverlay.EditResetButtonArea),
            nameof(BaseChestOverlay.EditExitButton)
        )]
        private void ReinitializeBaseComponents()
        {
            Rectangle bounds = new Rectangle(this.Menu.xPositionOnScreen, this.Menu.yPositionOnScreen, this.Menu.width, this.Menu.height);

            // category dropdown
            if (this.ShowCategoryDropdown)
            {
                this.CategoryDropdown = new Dropdown<string>(bounds.Right - Game1.tileSize, bounds.Y, this.Font, this.SelectedCategory, this.Categories, category => category);

                if (Constants.TargetPlatform != GamePlatform.Android)
                    this.CategoryDropdown.bounds.Y = bounds.Y - this.CategoryDropdown.bounds.Height + this.TopOffset;
                this.CategoryDropdown.bounds.X -= this.CategoryDropdown.bounds.Width; // right-align
                this.CategoryDropdown.ReinitializeComponents();
            }

            // chest dropdown
            {
                ManagedChest[] chests = this.Chests.Where(chest => !this.ShowCategoryDropdown || chest.DisplayCategory == this.SelectedCategory).ToArray();
                ManagedChest? selected = ChestFactory.GetBestMatch(chests, this.Chest);
                this.ChestDropdown = new Dropdown<ManagedChest>(bounds.X, bounds.Y, this.Font, selected, chests, chest => chest.DisplayName);

                if (Constants.TargetPlatform != GamePlatform.Android)
                {
                    this.ChestDropdown.bounds.Y = bounds.Y - this.ChestDropdown.bounds.Height + this.TopOffset;
                    this.ChestDropdown.ReinitializeComponents();
                }
            }

            // edit chest button overlay (based on chest dropdown position)
            {
                Rectangle sprite = CommonSprites.Icons.SpeechBubble;
                float zoom = Game1.pixelZoom / 2f;
                Rectangle buttonBounds = new Rectangle(this.ChestDropdown.bounds.X + this.ChestDropdown.bounds.Width, this.ChestDropdown.bounds.Y, (int)(sprite.Width * zoom), (int)(sprite.Height * zoom));
                this.EditButton = new ClickableTextureComponent("edit-chest", buttonBounds, null, I18n.Button_EditChest(), CommonSprites.Icons.Sheet, sprite, zoom);
            }

            // edit form
            int longTextWidth = (int)Game1.smallFont.MeasureString("A sufficiently, reasonably long string").X;
            this.EditNameField = new ValidatedTextBox(Game1.smallFont, Color.Black, ch => ch != '|', this.Reflection) { Width = longTextWidth };
            this.EditCategoryField = new ValidatedTextBox(Game1.smallFont, Color.Black, ch => ch != '|', this.Reflection) { Width = longTextWidth };
            this.EditOrderField = new ValidatedTextBox(Game1.smallFont, Color.Black, char.IsDigit, this.Reflection) { Width = (int)Game1.smallFont.MeasureString("9999999").X };
            this.EditHideChestField = new Checkbox();
            this.EditAutomatePreventRemovingStacks = new Checkbox();
            this.EditAutomateStorage = new SimpleDropdown<AutomateContainerPreference>(
                this.Reflection,
                options: new[]
                {
                    new KeyValuePair<AutomateContainerPreference, string>(AutomateContainerPreference.Allow, I18n.Label_AutomateStore()),
                    new KeyValuePair<AutomateContainerPreference, string>(AutomateContainerPreference.Prefer, I18n.Label_AutomateStoreFirst()),
                    new KeyValuePair<AutomateContainerPreference, string>(AutomateContainerPreference.Disable, I18n.Label_AutomateStoreDisabled())
                }
            );
            this.EditAutomateFetch = new SimpleDropdown<AutomateContainerPreference>(
                this.Reflection,
                options: new[]
                {
                    new KeyValuePair<AutomateContainerPreference, string>(AutomateContainerPreference.Allow, I18n.Label_AutomateTake()),
                    new KeyValuePair<AutomateContainerPreference, string>(AutomateContainerPreference.Prefer, I18n.Label_AutomateTakeFirst()),
                    new KeyValuePair<AutomateContainerPreference, string>(AutomateContainerPreference.Disable, I18n.Label_AutomateTakeDisabled())
                }
            );
            this.FillForm();

            this.EditSaveButtonArea = new ClickableComponent(new Rectangle(0, 0, Game1.tileSize, Game1.tileSize), "save-chest");
            this.EditResetButtonArea = new ClickableComponent(new Rectangle(0, 0, Game1.tileSize, Game1.tileSize), "reset-chest");
            this.EditExitButton = new ClickableTextureComponent(new Rectangle(bounds.Right - 9 * Game1.pixelZoom, bounds.Y - Game1.pixelZoom * 2, CommonSprites.Icons.ExitButton.Width * Game1.pixelZoom, CommonSprites.Icons.ExitButton.Height * Game1.pixelZoom), CommonSprites.Icons.Sheet, CommonSprites.Icons.ExitButton, Game1.pixelZoom);
        }

        /// <summary>Initialize the edit-chest overlay for rendering.</summary>
        protected virtual void ReinitializeComponents()
        {
            this.ReinitializeBaseComponents();
        }

        /// <summary>Set the form values to match the underlying chest.</summary>
        private void FillForm()
        {
            this.EditNameField.Text = this.Chest.DisplayName;
            this.EditCategoryField.Text = this.Chest.DisplayCategory;
            this.EditOrderField.Text = this.Chest.Order?.ToString() ?? string.Empty;
            this.EditHideChestField.Value = this.Chest.IsIgnored;
            this.EditAutomatePreventRemovingStacks.Value = this.Chest.PreventRemovingStacks;
            this.EditAutomateStorage.TrySelect(this.Chest.AutomateStoreItems);
            this.EditAutomateFetch.TrySelect(this.Chest.AutomateTakeItems);
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
            AutomateContainerPreference automateStore = this.EditAutomateStorage.SelectedKey;
            AutomateContainerPreference automateTake = this.EditAutomateFetch.SelectedKey;
            bool automatePreventRemovingStacks = this.EditAutomatePreventRemovingStacks.Value;
            bool automateChanged = this.Chest.CanConfigureAutomate && (automatePreventRemovingStacks != this.Chest.PreventRemovingStacks || automateStore != this.Chest.AutomateStoreItems || automateTake != this.Chest.AutomateTakeItems);
            this.Chest.Update(
                name: this.EditNameField.Text,
                category: this.EditCategoryField.Text,
                order: order,
                ignored: this.EditHideChestField.Value,
                automatePreventRemovingStacks: automatePreventRemovingStacks,
                automateStoreItems: automateStore,
                automateTakeItems: automateTake
            );
            this.OnChestSelected?.Invoke(this.Chest);
            if (automateChanged)
                this.OnAutomateOptionsChanged?.Invoke(this.Chest);
        }

        /// <summary>Handle the active element changing.</summary>
        /// <param name="value">The new value.</param>
        private void OnActiveElementChanged(Element value)
        {
            // disable readonly mode
            this.SetItemsClickable(value == Element.Menu);

            // close open dropdowns
            if (value != Element.CategoryList && value != Element.ChestList)
            {
                this.ChestDropdown.IsExpanded = false;
                if (this.CategoryDropdown != null)
                    this.CategoryDropdown.IsExpanded = false;
            }

            // deselect textboxes
            if (value != Element.EditForm)
                this.DeselectManagedTextboxes();
        }

        /// <summary>Exit the chest menu.</summary>
        protected void Exit()
        {
            this.Dispose();
            this.Menu.exitThisMenu();
        }

        /// <summary>Update textboxes on the edit form for a click, if applicable.</summary>
        /// <param name="x">The X-position of the cursor.</param>
        /// <param name="y">The Y-position of the cursor.</param>
        /// <returns>Returns whether a textbox was clicked.</returns>
        private bool TryClickTextbox(int x, int y)
        {
            if (this.ActiveElement != Element.EditForm)
                return false;

            // select textbox
            foreach (ValidatedTextBox textbox in this.ManagedTextboxes)
            {
                if (!textbox.GetBounds().Contains(x, y))
                    continue;

                if (!textbox.Selected)
                {
                    this.DeselectManagedTextboxes();
                    textbox.Select();
                }

                return true;
            }

            // else deselect any current textbox
            this.DeselectManagedTextboxes();
            return false;
        }

        /// <summary>Get the index of a chest in the selected category.</summary>
        /// <param name="chest">The chest to find.</param>
        /// <param name="chests">The chests to search.</param>
        private int GetChestIndex(ManagedChest chest, ManagedChest[] chests)
        {
            ManagedChest? match = ChestFactory.GetBestMatch(chests, chest);
            return match != null
                ? Array.IndexOf(chests, match)
                : -1;
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
            this.EditOrderField.Text = this.Chest.Order?.ToString() ?? string.Empty;
            this.EditHideChestField.Value = this.Chest.IsIgnored;
            this.EditAutomatePreventRemovingStacks.Value = this.Chest.PreventRemovingStacks;
            this.EditAutomateStorage.TrySelect(this.Chest.AutomateStoreItems);
            this.EditAutomateFetch.TrySelect(this.Chest.AutomateTakeItems);

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

        /// <summary>Deselect textboxes managed by Chests Anywhere.</summary>
        private void DeselectManagedTextboxes()
        {
            foreach (ValidatedTextBox textbox in this.ManagedTextboxes)
                textbox.Selected = false;
        }

        /// <summary>Draw a checkbox to the screen, including any position updates needed.</summary>
        /// <param name="batch">The sprite batch being drawn.</param>
        /// <param name="font">The font to use for the checkbox label.</param>
        /// <param name="checkbox">The checkbox to draw.</param>
        /// <param name="x">The top-left X position to start drawing from.</param>
        /// <param name="y">The top-left Y position to start drawing from.</param>
        /// <param name="label">The translation for the checkbox label.</param>
        /// <param name="defaultValue">The default value.</param>
        private Vector2 DrawAndPositionCheckbox(SpriteBatch batch, SpriteFont font, Checkbox checkbox, int x, int y, string label, bool defaultValue = false)
        {
            checkbox.X = x;
            checkbox.Y = y;
            checkbox.Width = 24;
            checkbox.Draw(batch);
            Vector2 labelSize = batch.DrawTextBlock(font, label, new Vector2(x + 7 + checkbox.Width, y), this.Menu.width, checkbox.Value != defaultValue ? Color.Red : Color.Black);

            return new Vector2(checkbox.Width + 7 + checkbox.Width + labelSize.X, Math.Max(checkbox.Width, labelSize.Y));
        }

        /// <summary>Draw a checkbox to the screen, including any position updates needed.</summary>
        /// <param name="batch">The sprite batch being drawn.</param>
        /// <param name="font">The font to use for the checkbox label.</param>
        /// <param name="clickArea">The clickable area to draw.</param>
        /// <param name="x">The top-left X position to start drawing from.</param>
        /// <param name="y">The top-left Y position to start drawing from.</param>
        /// <param name="label">The translation for the checkbox label.</param>
        /// <param name="color">The text color to draw.</param>
        /// <param name="bounds">The button's outer bounds.</param>
        private Vector2 DrawButton(SpriteBatch batch, SpriteFont font, ClickableComponent clickArea, int x, int y, string label, in Color color, out Rectangle bounds)
        {
            // get text
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
