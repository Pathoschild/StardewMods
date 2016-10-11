**No Debug Mode** is a [Stardew Valley](http://stardewvalley.net/) mod which simply disables
SMAPI's `F2` debug mode, which can cause unintended consequences like skipping an entire season
_and automatically saving_, or teleporting into walls. (See [SMAPI issue #120](https://github.com/cjsu/SMAPI/issues/120).)

## Installation
1. Install [SMAPI](https://github.com/ClxS/SMAPI) (any version).
3. Install [this mod from Nexus mods](http://www.nexusmods.com/stardewvalley/mods/593/).
4. Run the game using SMAPI.

## Usage
This is a passive mod — it'll simply disable debug mode immediately after you enable it.

## Versions
1.0:
* Initial release.

## Compiling the mod
[Installing a stable release from Nexus Mods](http://www.nexusmods.com/stardewvalley/mods/593/) is
recommended. If you really want to compile the mod yourself, just edit `Pathoschild.Stardew.NoDebugMode.csproj`
and set the `<GamePath>` setting to your Stardew Valley directory path. Launching the project in
Visual Studio will compile the code, package it into the mod directory, and start the game.

## See also
* [Nexus mod](http://www.nexusmods.com/stardewvalley/mods/593/)
* [Discussion thread](http://community.playstarbound.com/threads/no-debug-mode.125586/)
* My other Stardew Valley mods: [Chests Anywhere](http://www.nexusmods.com/stardewvalley/mods/518/), [Lookup Anything](https://github.com/Pathoschild/LookupAnything), and [Skip Intro](https://github.com/Pathoschild/StardewValley.SkipIntro)
