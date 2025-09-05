
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;

namespace Pathoschild.Stardew.LookupAnything.Framework.Themes;


/// <summary>manage how the menu looks</summary>
/// <param name="Backgrounds"></param>
internal record ThemeManager
{
    /// <summary>Default menu background option</summary>
    internal const string DEFAULT_BACKGROUND = "Letter_TornPaper";
    private const string LetterBG = "LooseSprites\\letterBG";
    private const string MenuTiles = "Maps\\MenuTiles";
    private const string Asset_Themes = "Pathoschild.LookupAnything/Themes";

    /// <summary>Theme data asset</summary>
    private Dictionary<string, ThemeData>? ThemeDataCached;

    internal Dictionary<string, ThemeData> ThemeData => this.ThemeDataCached ??= Game1.content.Load<Dictionary<string, ThemeData>>(Asset_Themes);

    /// <summary>Current background key inner field</summary>
    private string CurrentTheme = DEFAULT_BACKGROUND;

    /// <summary>Currently chosen menu background</summary>
    private IMenuBackground? CurrentBackgroundImpl;

    /// <summary>The current <see cref="IMenuBackground"/> instance</summary>
    internal IMenuBackground CurrentBackground
    {
        get
        {
            if (this.CurrentBackgroundImpl == null)
            {
                this.SetCurrentTheme(this.CurrentTheme);
            }
            return this.CurrentBackgroundImpl!;
        }
    }

    /// <summary>Get background keys for GMCM purposes</summary>
    internal string[] BackgroundKeys => [.. this.ThemeData.Keys];

    /// <summary>Mod helper instance</summary>
    private readonly IModHelper Helper;

    /// <summary>Create new theme manager instance</summary>
    /// <param name="Helper"></param>
    internal ThemeManager(IModHelper Helper)
    {
        this.Helper = Helper;
        this.Helper.Events.Content.AssetRequested += this.OnAssetRequested;
        this.Helper.Events.Content.AssetsInvalidated += this.OnAssetInvalidated;
    }

    /// <summary>
    /// Set current background this also updates <see cref="CurrentBackground"/>
    /// Rejects any value that is not a key of <see cref="ThemeDataCached"/>
    /// </summary>
    internal void SetCurrentTheme(string value)
    {
        if (this.ThemeData.TryGetValue(value, out ThemeData? theme))
        {
            this.CurrentTheme = value;
            this.CurrentBackgroundImpl = new MenuBackground(this.Helper.GameContent, theme);
        }
        else
        {
            this.CurrentTheme = DEFAULT_BACKGROUND;
            this.CurrentBackgroundImpl = new MenuBackground(this.Helper.GameContent, this.ThemeData[DEFAULT_BACKGROUND]);
        }
    }

    /// <summary>Populate menu theme data</summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void OnAssetRequested(object? sender, AssetRequestedEventArgs e)
    {
        if (e.Name.IsEquivalentTo(Asset_Themes))
        {
            e.LoadFrom(GetBuiltInBackgrounds, AssetLoadPriority.Exclusive);
        }
    }

    /// <summary>Clear cached <see cref="ThemeDataCached"/> and <see cref="CurrentBackgroundImpl"/> on invalidate</summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void OnAssetInvalidated(object? sender, AssetsInvalidatedEventArgs e)
    {
        if (e.Names.Any(name => name.IsEquivalentTo(Asset_Themes)))
        {
            this.CurrentBackgroundImpl = null;
            this.ThemeDataCached = null;
        }
    }

    /// <summary>Populate default values of theme asset</summary>
    /// <returns></returns>
    private static Dictionary<string, ThemeData> GetBuiltInBackgrounds()
    {
        return new()
        {
            ["Plain"] = new ThemeData()
            {
                DisplayName = I18n.Config_Theme_MenuBackground_Value_Plain(),
                BackgroundCategory = MenuBackgroundCategory.PlainColor,
                BackgroundPrimaryColor = "Wheat",
                BackgroundSecondaryColor = "BurlyWood",
                BackgroundPadding = 8
            },
            ["Letter_TornPaper"] = new ThemeData()
            {
                DisplayName = I18n.Config_Theme_MenuBackground_Value_LetterTornPaper(),
                BackgroundCategory = MenuBackgroundCategory.FixedSprite,
                BackgroundTexture = LetterBG,
                BackgroundSourceRect = new(0, 0, 320, 180)
            },
            ["Letter_Notepad"] = new ThemeData()
            {
                DisplayName = I18n.Config_Theme_MenuBackground_Value_LetterNotepad(),
                BackgroundCategory = MenuBackgroundCategory.FixedSprite,
                BackgroundTexture = LetterBG,
                BackgroundSourceRect = new(320, 0, 320, 180)
            },
            ["Letter_Joja"] = new ThemeData()
            {
                DisplayName = I18n.Config_Theme_MenuBackground_Value_LetterJoja(),
                BackgroundCategory = MenuBackgroundCategory.FixedSprite,
                BackgroundTexture = LetterBG,
                BackgroundSourceRect = new(0, 204, 320, 180),
                BackgroundPadding = 48
            },
            ["MenuBox_Border"] = new ThemeData()
            {
                DisplayName = I18n.Config_Theme_MenuBackground_Value_MenuBoxBorder(),
                BackgroundCategory = MenuBackgroundCategory.MenuBox,
                BackgroundTexture = MenuTiles,
                BackgroundSourceRect = new(0, 256, 60, 60),
                BackgroundPadding = 4
            },
            ["MenuBox_Inset"] = new ThemeData()
            {
                DisplayName = I18n.Config_Theme_MenuBackground_Value_MenuBoxInset(),
                BackgroundCategory = MenuBackgroundCategory.MenuBox,
                BackgroundTexture = MenuTiles,
                BackgroundSourceRect = new(0, 320, 60, 60),
                BackgroundPadding = 4
            },
            ["MenuBox_Raised"] = new ThemeData()
            {
                DisplayName = I18n.Config_Theme_MenuBackground_Value_MenuBoxRaised(),
                BackgroundCategory = MenuBackgroundCategory.MenuBox,
                BackgroundTexture = MenuTiles,
                BackgroundSourceRect = new(60, 320, 60, 60),
                BackgroundPadding = 4
            }
        };
    }

    /// <summary>Get display name for a theme key</summary>
    /// <param name="themeKey"></param>
    /// <returns></returns>
    internal string GetDisplayName(string themeKey)
    {
        if (this.ThemeData.TryGetValue(themeKey, out ThemeData? theme))
        {
            return theme.DisplayName ?? themeKey;
        }
        return themeKey;
    }
}
