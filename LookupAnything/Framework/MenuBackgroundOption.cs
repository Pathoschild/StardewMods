namespace Pathoschild.Stardew.LookupAnything.Framework;

/// <summary>As part of <see cref="ModConfig"/>, options for the menu background.</summary>
internal enum MenuBackgroundOption
{
    // Default color: Wheat BG + BurlyWood Border
    Plain = 0b000000,
    // The 5 base game LooseSprites/letterBG options
    LetterBG = 0b010000,
    LetterBG_A = LetterBG | 0b0001,
    LetterBG_B = LetterBG | 0b0010,
    LetterBG_C = LetterBG | 0b0011,
    LetterBG_D = LetterBG | 0b0100,
    LetterBG_E = LetterBG | 0b0101,
    // Border at Maps/MenuTiles
    MenuBox = 0b100000,
    MenuBox_A = MenuBox | 0b0001,
    MenuBox_B = MenuBox | 0b0010,
    MenuBox_C = MenuBox | 0b0011,
}
