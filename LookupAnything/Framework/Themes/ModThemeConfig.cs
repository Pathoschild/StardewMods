namespace Pathoschild.Stardew.LookupAnything.Framework.Themes;

/// <summary>As part of <see cref="ModConfig"/>, the visual theme to apply to the menu.</summary>
internal sealed class ModThemeConfig
{
    /// <summary>The theme ID to apply.</summary>
    public string ThemeId { get; set; } = ThemeManager.DefaultThemeId;
}
