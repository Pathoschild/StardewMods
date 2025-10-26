using Microsoft.Xna.Framework;
using StardewValley;

namespace Pathoschild.Stardew.LookupAnything.Framework.Themes;

/// <summary>The data for a visual menu theme.</summary>
public sealed class ThemeData
{
    /*********
    ** Accessors
    *********/
    /****
    ** Metadata
    ****/
    /// <summary>A translated display name shown in UIs.</summary>
    public string DisplayName { get; set; } = I18n.Config_Theme_Values_Parchment();

    /****
    ** Background
    ****/
    /// <summary>How the menu background should be drawn.</summary>
    public MenuBackgroundType BackgroundType { get; set; }

    /// <summary>The background texture to draw, if applicable based on the <see cref="BackgroundType"/>.</summary>
    public string BackgroundTexture { get; set; } = "LooseSprites/letterBG";

    /// <summary>The pixel area within the <see cref="BackgroundTexture"/> to draw.</summary>
    public Rectangle BackgroundSourceRect { get; set; }

    /// <summary>The background color tint, as a value which can be parsed by <see cref="Utility.StringToColor"/>.</summary>
    public string BackgroundColor { get; set; } = "White";

    /// <summary>The pixel spacing between the edge of the <see cref="BackgroundTexture"/> and the inner content.</summary>
    public int BackgroundPadding { get; set; }

    /****
    ** Border
    ****/
    /// <summary>The border color tint, as a value which can be parsed by <see cref="Utility.StringToColor"/>.</summary>
    public string BorderColor { get; set; } = "Black";
}
