
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Pathoschild.Stardew.LookupAnything.Framework;

namespace LookupAnything.Framework;

/// <summary>Enum, which kind of IMenuBackground to create</summary>
internal enum MenuBackgroundCategory
{
    /// <summary><see cref="PlainBackground"/></summary>
    Plain,
    /// <summary>><see cref="LetterBackground"/></summary>
    Letter,
    /// <summary>><see cref="MenuBoxBackground"/></summary>
    MenuBox,
}

/// <summary>manage how the menu looks</summary>
/// <param name="Backgrounds"></param>
internal record ThemeManager(Dictionary<string, IMenuBackground> Backgrounds)
{
    /// <summary>Default menu background option</summary>
    internal const string DEFAULT_BACKGROUND = "Letter_TornPaper";

    /// <summary>Current background key inner field</summary>
    private string CurrentBackgroundKeyImpl = DEFAULT_BACKGROUND;

    /// <summary>Currently chosen menu background</summary>
    private IMenuBackground? CurrentBackgroundImpl = Backgrounds[DEFAULT_BACKGROUND];

    /// <summary>
    /// Current background property, setting this also updates <see cref="CurrentBackground"/>
    /// Rejects any value that is not a key of <see cref="Backgrounds"/>
    /// </summary>
    internal string CurrentBackgroundKey
    {
        get => this.CurrentBackgroundKey;
        set
        {
            if (this.Backgrounds.TryGetValue(value, out IMenuBackground? pickedBG))
            {
                this.CurrentBackgroundKeyImpl = value;
                this.CurrentBackgroundImpl = pickedBG;
            }
        }
    }

    /// <summary>The current <see cref="IMenuBackground"/> instance</summary>
    internal IMenuBackground CurrentBackground => this.CurrentBackgroundImpl ??= this.Backgrounds[this.CurrentBackgroundKeyImpl];

    /// <summary>Get background keys for GMCM purposes</summary>
    internal string[] BackgroundKeys => [.. this.Backgrounds.Keys];

    /// <summary>Get a theme manager with the built-in backgrounds</summary>
    /// <returns>new ThemeManager</returns>
    internal static ThemeManager FormBaseThemeManager()
    {
        return new ThemeManager(GetBuiltInBackgrounds());
    }

    /// <summary>Get dict of built-in backgrounds</summary>
    /// <returns>new Dictionary<string, IMenuBackground></returns>
    private static Dictionary<string, IMenuBackground> GetBuiltInBackgrounds()
    {
        Dictionary<string, IMenuBackground> backgrounds = [];

        // plain
        backgrounds["Plain"] = new PlainBackground(Color.Wheat, Color.BurlyWood);
        // letter
        backgrounds["Letter_TornPaper"] = new LetterBackground(0, 0);
        backgrounds["Letter_Notepad"] = new LetterBackground(320, 0);
        // magical and krobus skipped because they are dark BG
        backgrounds["Letter_Joja"] = new LetterBackground(0, 204);
        // menubox
        backgrounds["MenuBox_Border"] = new MenuBoxBackground(0, 256);
        backgrounds["MenuBox_Inset"] = new MenuBoxBackground(0, 316);
        backgrounds["MenuBox_Raised"] = new MenuBoxBackground(60, 316);

        return backgrounds;
    }
}