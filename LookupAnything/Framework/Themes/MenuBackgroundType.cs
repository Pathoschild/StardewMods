namespace Pathoschild.Stardew.LookupAnything.Framework.Themes;

/// <summary>A type which indicates how a menu background should be drawn.</summary>
public enum MenuBackgroundType
{
    /// <summary>Draw a background texture over the entire area.</summary>
    FixedSprite,

    /// <summary>Draw a menu box by taking specific sprites from the texture background for the corners, edges, and center.</summary>
    MenuBox,

    /// <summary>Draw a plain colored background with no texture.</summary>
    PlainColor
}
