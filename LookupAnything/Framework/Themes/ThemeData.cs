using System;
using Microsoft.Xna.Framework;

namespace Pathoschild.Stardew.LookupAnything.Framework.Themes;


/// <summary>Enum, how to draw menu background</summary>
public enum MenuBackgroundCategory
{
    /// <summary><see cref="PlainBackground"/></summary>
    PlainColor,
    /// <summary>><see cref="LetterBackground"/></summary>
    FixedSprite,
    /// <summary>><see cref="MenuBoxBackground"/></summary>
    MenuBox,
}


/// <summary>theme data</summary>
public sealed class ThemeData
{
    /// <summary>display name (used in the config menu)</summary>
    public string DisplayName { get; set; } = I18n.Config_Theme_MenuBackground_Value_LetterTornPaper();

    /// <summary>background draw category</summary>
    public MenuBackgroundCategory BackgroundCategory { get; set; } = MenuBackgroundCategory.FixedSprite;

    /// <summary>background texture</summary>
    public string BackgroundTexture { get; set; } = "LooseSprites\\letterBG";

    /// <summary>background source rect</summary>
    public Rectangle BackgroundSourceRect { get; set; } = Rectangle.Empty;

    /// <summary>background primary draw color</summary>
    public string BackgroundPrimaryColor { get; set; } = "White";

    /// <summary>background secondary draw color, used if <see cref="BackgroundCategory"/> is <see cref="MenuBackgroundCategory.PlainColor"></summary>
    public string BackgroundSecondaryColor { get; set; } = "Black";

    /// <summary>Pixel amount of padding around the background</summary>
    public int BackgroundPadding { get; set; } = 0;

    // TODO: add support for font colors
}