using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Pathoschild.Stardew.ChestsAnywhere.Framework;
using Pathoschild.Stardew.ChestsAnywhere.Menus.Components;
using Pathoschild.Stardew.Common.UI;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Menus;

namespace Pathoschild.Stardew.ChestsAnywhere.Menus.Overlays;

/// <summary>An <see cref="ItemGrabMenu"/> overlay which lets the player navigate and edit chests.</summary>
internal class ChestOverlay : BaseChestOverlay
{
    /*********
    ** Fields
    *********/
    /// <summary>The underlying chest menu.</summary>
    private readonly ItemGrabMenu Menu;

    /// <summary>The underlying menu's player inventory submenu.</summary>
    private readonly InventoryMenu MenuInventoryMenu;

    /// <summary>The default highlight function for the chest items.</summary>
    private readonly InventoryMenu.highlightThisItem DefaultChestHighlighter;

    /// <summary>The default highlight function for the player inventory items.</summary>
    private readonly InventoryMenu.highlightThisItem DefaultInventoryHighlighter;

    /// <summary>The button which sorts the player inventory.</summary>
    private ClickableTextureComponent? SortInventoryButton;

    /// <summary>The hover text to draw if the menu isn't already drawing hover text.</summary>
    private string? HoverText;

    /// <summary>Whether the color picker was visible when we last initialized the components.</summary>
    private bool WasColorPickerShown;


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
    /// <param name="reflection">Simplifies access to private code.</param>
    /// <param name="showAutomateOptions">Whether to show Automate options if applicable for this chest type.</param>
    public ChestOverlay(ItemGrabMenu menu, ManagedChest chest, ManagedChest[] chests, ModConfig config, ModConfigKeys keys, IModEvents events, IInputHelper input, IReflectionHelper reflection, bool showAutomateOptions)
        : base(menu, chest, chests, config, keys, events, input, reflection, showAutomateOptions, keepAlive: () => Game1.activeClickableMenu is ItemGrabMenu)
    {
        this.Menu = menu;
        this.MenuInventoryMenu = menu.ItemsToGrabMenu;
        this.DefaultChestHighlighter = menu.inventory.highlightMethod;
        this.DefaultInventoryHighlighter = this.MenuInventoryMenu.highlightMethod;
        this.WasColorPickerShown = this.IsColorPickerShown(menu);
    }


    /*********
    ** Protected methods
    *********/
    /// <inheritdoc />
    protected override void Update()
    {
        base.Update();

        // update positions when the color picker is toggled
        bool colorPickerShown = this.IsColorPickerShown(this.Menu);
        if (colorPickerShown != this.WasColorPickerShown)
        {
            this.WasColorPickerShown = colorPickerShown;
            this.ReinitializeComponents();
        }
    }

    /// <inheritdoc />
    protected override bool ReceiveLeftClick(int x, int y)
    {
        if (this.IsInitialized)
        {
            switch (this.ActiveElement)
            {
                case Element.EditForm:
                case Element.ChestList:
                case Element.CategoryList:
                    break;

                default:
                    bool canNavigate = this.CanCloseChest;

                    // handle exit
                    // (This fixes an issue where the game doesn't handle it correctly in some contexts, like the
                    // Stardew Valley Fair fishing minigame. Note that the button may be null if removed by a mod like
                    // ChestEx.)
                    if (canNavigate && this.Menu.okButton?.containsPoint(x, y) == true)
                    {
                        this.Exit();
                        return true;
                    }

                    // handle sort
                    else if (this.SortInventoryButton?.containsPoint(x, y) == true)
                    {
                        this.SortInventory();
                        return true;
                    }
                    break;
            }
        }

        return base.ReceiveLeftClick(x, y);
    }

    /// <inheritdoc />
    protected override bool ReceiveCursorHover(int x, int y)
    {
        this.HoverText = null;

        if (this.IsInitialized)
        {
            if (this.ActiveElement == Element.Menu)
            {
                this.SortInventoryButton?.tryHover(x, y);

                if (this.EditButton.containsPoint(x, y))
                    this.HoverText = this.EditButton.hoverText;
                else if (this.SortInventoryButton?.containsPoint(x, y) is true)
                    this.HoverText = this.SortInventoryButton.hoverText;
            }
        }

        return base.ReceiveCursorHover(x, y);
    }

    /// <inheritdoc />
    protected override void DrawUi(SpriteBatch batch)
    {
        if (!this.ActiveElement.HasFlag(Element.EditForm))
        {
            float navOpacity = this.CanCloseChest ? 1f : 0.5f;
            this.SortInventoryButton?.draw(batch, Color.White * navOpacity, 1f);
        }

        if (this.HoverText != null && string.IsNullOrEmpty(this.Menu.hoverText))
            IClickableMenu.drawHoverText(batch, this.HoverText, Game1.smallFont);

        base.DrawUi(batch); // run base logic last, to draw cursor over everything else
    }

    /// <inheritdoc />
    protected override void ReinitializeComponents()
    {
        base.ReinitializeComponents();

        // sort inventory button
        if (this.Config.AddOrganizePlayerInventoryButton)
        {
            // add button
            Rectangle sprite = Sprites.Buttons.Organize;
            ClickableTextureComponent okButton = this.Menu.okButton;
            float zoom = Game1.pixelZoom;
            Rectangle buttonBounds = new Rectangle(okButton.bounds.X, (int)(okButton.bounds.Y - sprite.Height * zoom - 5 * zoom), (int)(sprite.Width * zoom), (int)(sprite.Height * zoom));
            this.SortInventoryButton = new ClickableTextureComponent("sort-inventory", buttonBounds, "", I18n.Button_SortInventory(), CommonSprites.Icons.Sheet, sprite, zoom);

            // adjust menu to fit
            this.Menu.trashCan.bounds.Y = this.SortInventoryButton.bounds.Y - this.Menu.trashCan.bounds.Height - 2 * Game1.pixelZoom;
        }
        else
            this.SortInventoryButton = null;
    }

    /// <inheritdoc />
    protected override void SetItemsClickable(bool clickable)
    {
        if (clickable)
        {
            this.Menu.inventory.highlightMethod = this.DefaultChestHighlighter;
            this.MenuInventoryMenu.highlightMethod = this.DefaultInventoryHighlighter;
        }
        else
        {
            this.Menu.inventory.highlightMethod = _ => false;
            this.MenuInventoryMenu.highlightMethod = _ => false;
        }
    }

    /// <inheritdoc />
    protected override int GetTopOffset(IClickableMenu menu)
    {
        // get grab menu info
        bool isBigChest = false;
        bool hasColorPicker = false;
        if (menu is ItemGrabMenu grabMenu)
        {
            isBigChest = grabMenu.ItemsToGrabMenu.rows >= 5;
            hasColorPicker = this.IsColorPickerShown(grabMenu);
        }

        // get offset
        int offset = (isBigChest ? -20 : -9) * Game1.pixelZoom;
        if (hasColorPicker)
        {
            offset += 2 * Game1.pixelZoom;
            if (isBigChest)
                offset += 2; // tweak so positions match between small & big chests
        }

        return offset;
    }

    /// <summary>Get whether the color picker is visible.</summary>
    /// <param name="menu">The menu to check.</param>
    private bool IsColorPickerShown(ItemGrabMenu menu)
    {
        return menu.chestColorPicker?.visible ?? false;
    }
}
