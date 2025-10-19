using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;

namespace Pathoschild.Stardew.LookupAnything.Framework.Themes;

/// <summary>Manages the menu appearance.</summary>
internal record ThemeManager
{
    /*********
    ** Fields
    *********/
    /****
    ** Constants
    ****/
    /// <summary>The default theme ID.</summary>
    internal const string DefaultThemeId = "Letter_TornPaper";

    /// <summary>The asset name for the default 'torn paper' background.</summary>
    private const string TornPaperBackgroundAssetName = "LooseSprites\\letterBG";

    /// <summary>The asset name for the menu box background.</summary>
    private const string MenuBoxBackgroundAssetName = "Maps\\MenuTiles";

    /// <summary>The asset name for the theme data.</summary>
    private const string DataAssetName = "Mods/Pathoschild.LookupAnything/Themes";

    /****
    ** State
    ****/
    /// <summary>The SMAPI content API with which to manage assets.</summary>
    private readonly IGameContentHelper GameContent;

    /// <summary>The backing field for <see cref="ThemeData"/>.</summary>
    private Dictionary<string, ThemeData>? ThemeDataImpl;

    /// <summary>The available theme data.</summary>
    private Dictionary<string, ThemeData> ThemeData => this.ThemeDataImpl ??= Game1.content.Load<Dictionary<string, ThemeData>>(DataAssetName);

    /// <summary>The backing field for <see cref="Background"/>.</summary>
    private MenuBackground? BackgroundImpl;



    /*********
    ** Accessors
    *********/
    /// <summary>The selected theme ID.</summary>
    public string ThemeId { get; private set; } = DefaultThemeId;

    /// <summary>The theme background settings.</summary>
    public MenuBackground Background
    {
        get
        {
            if (this.BackgroundImpl == null)
                this.SetCurrentTheme(this.ThemeId);

            return this.BackgroundImpl!;
        }
    }

    /// <summary>An event called when the theme asset is invalidated.</summary>
    public event Action? OnThemeDataChanged;


    /*********
    ** Public methods
    *********/
    /// <summary>Construct an instance.</summary>
    /// <param name="events">The SMAPI events API with which to manage assets.</param>
    /// <param name="gameContent">The SMAPI content API with which to manage assets.</param>
    public ThemeManager(IModEvents events, IGameContentHelper gameContent)
    {
        this.GameContent = gameContent;

        events.Content.AssetRequested += this.OnAssetRequested;
        events.Content.AssetsInvalidated += this.OnAssetInvalidated;
    }

    /// <summary>Get the available theme IDs.</summary>
    public IEnumerable<string> GetAvailableThemeIds()
    {
        return this.ThemeData.Keys;
    }

    /// <summary>Get the display name for a theme.</summary>
    /// <param name="themeId">The theme ID.</param>
    public string GetDisplayName(string themeId)
    {
        return this.ThemeData.GetValueOrDefault(themeId)?.DisplayName ?? themeId;
    }

    /// <summary>Set the current theme.</summary>
    /// <param name="themeId">The theme ID to select.</param>
    /// <remarks>Setting an invalid theme ID will switch to the default theme instead.</remarks>
    public void SetCurrentTheme(string themeId)
    {
        if (this.ThemeData.TryGetValue(themeId, out ThemeData? theme))
        {
            this.ThemeId = themeId;
            this.BackgroundImpl = new MenuBackground(this.GameContent, theme);
        }
        else
        {
            this.ThemeId = DefaultThemeId;
            this.BackgroundImpl = new MenuBackground(this.GameContent, this.ThemeData[DefaultThemeId]);
        }
    }


    /*********
    ** Private methods
    *********/
    /// <summary>Load the theme data when requested.</summary>
    /// <inheritdoc cref="IContentEvents.AssetRequested" />
    private void OnAssetRequested(object? sender, AssetRequestedEventArgs e)
    {
        if (e.Name.IsEquivalentTo(DataAssetName))
            e.LoadFrom(GetBuiltInBackgrounds, AssetLoadPriority.Exclusive);
    }

    /// <summary>Reset the cached theme data when the asset is invalidated.</summary>
    /// <inheritdoc cref="IContentEvents.AssetsInvalidated" />
    private void OnAssetInvalidated(object? sender, AssetsInvalidatedEventArgs e)
    {
        if (e.Names.Any(name => name.IsEquivalentTo(DataAssetName)))
        {
            this.BackgroundImpl = null;
            this.ThemeDataImpl = null;

            this.OnThemeDataChanged?.Invoke();
        }
    }

    /// <summary>Get the themes provided by the base mod.</summary>
    private static Dictionary<string, ThemeData> GetBuiltInBackgrounds()
    {
        return new Dictionary<string, ThemeData>
        {
            ["Plain"] = new()
            {
                DisplayName = I18n.Config_Theme_MenuBackground_Values_Plain(),
                BackgroundType = MenuBackgroundType.PlainColor,
                BackgroundColor = "Wheat",
                BorderColor = "BurlyWood",
                BackgroundPadding = 8
            },
            [DefaultThemeId] = new()
            {
                DisplayName = I18n.Config_Theme_MenuBackground_Values_LetterTornPaper(),
                BackgroundType = MenuBackgroundType.FixedSprite,
                BackgroundTexture = TornPaperBackgroundAssetName,
                BackgroundSourceRect = new Rectangle(0, 0, 320, 180)
            },
            ["Letter_Notepad"] = new()
            {
                DisplayName = I18n.Config_Theme_MenuBackground_Values_LetterNotepad(),
                BackgroundType = MenuBackgroundType.FixedSprite,
                BackgroundTexture = TornPaperBackgroundAssetName,
                BackgroundSourceRect = new Rectangle(320, 0, 320, 180)
            },
            ["Letter_Joja"] = new()
            {
                DisplayName = I18n.Config_Theme_MenuBackground_Values_LetterJoja(),
                BackgroundType = MenuBackgroundType.FixedSprite,
                BackgroundTexture = TornPaperBackgroundAssetName,
                BackgroundSourceRect = new Rectangle(0, 204, 320, 180),
                BackgroundPadding = 48
            },
            ["MenuBox_Border"] = new()
            {
                DisplayName = I18n.Config_Theme_MenuBackground_Values_MenuBoxBorder(),
                BackgroundType = MenuBackgroundType.MenuBox,
                BackgroundTexture = MenuBoxBackgroundAssetName,
                BackgroundSourceRect = new Rectangle(0, 256, 60, 60),
                BackgroundPadding = 4
            },
            ["MenuBox_Inset"] = new()
            {
                DisplayName = I18n.Config_Theme_MenuBackground_Values_MenuBoxInset(),
                BackgroundType = MenuBackgroundType.MenuBox,
                BackgroundTexture = MenuBoxBackgroundAssetName,
                BackgroundSourceRect = new Rectangle(0, 320, 60, 60),
                BackgroundPadding = 4
            },
            ["MenuBox_Raised"] = new()
            {
                DisplayName = I18n.Config_Theme_MenuBackground_Values_MenuBoxRaised(),
                BackgroundType = MenuBackgroundType.MenuBox,
                BackgroundTexture = MenuBoxBackgroundAssetName,
                BackgroundSourceRect = new Rectangle(60, 320, 60, 60),
                BackgroundPadding = 4
            }
        };
    }
}
