<<<<<<< HEAD
This repository contains my SMAPI mods for Stardew Valley. See the individual mods for
documentation and release notes:

* [Chests Anywhere](ChestsAnywhere)
* [Debug Mode](DebugMode)
* [Lookup Anything](LookupAnything)
* [Skip Intro](SkipIntro)

## Compiling the mods
Installing stable releases from Nexus Mods is recommended for most users. If you really want to
compile the mod yourself, read on.

These mods use the [crossplatform build config](https://github.com/Pathoschild/Stardew.ModBuildConfig#readme)
so they can be built on Linux, Mac, and Windows without changes. See [the build config documentation](https://github.com/Pathoschild/Stardew.ModBuildConfig#readme)
for troubleshooting.

### Compiling a mod for testing
To compile a mod and add it to your game's `Mods` directory:

1. Rebuild the project in [Visual Studio](https://www.visualstudio.com/vs/community/) or [MonoDevelop](http://www.monodevelop.com/).  
   <small>This will compile the code and package it into the mod directory.</small>
2. Launch the project with debugging.  
   <small>This will start the game through SMAPI and attach the Visual Studio debugger.</small>

### Compiling a mod for release
To package a mod for release:

1. Delete the mod's directory in `Mods`.  
   <small>(This ensures the package is clean and has default configuration.)</small>
2. Recompile the mod per the previous section.
3. Launch the game through SMAPI to generate the default `config.json` (if any).
4. Delete the `.cache` in the mod folder.
2. Create a zip file of the mod's folder in the `Mods` folder. The zip name should include the
   mod name and version. For example:

   ```
   LookupAnything-1.0.zip
      LookupAnything/
         LookupAnything.dll
         LookupAnything.pdb
         config.json
         manifest.json
   ```
=======
﻿**No Debug Mode** is a [Stardew Valley](http://stardewvalley.net/) mod which simply disables
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
>>>>>>> nodebugmode/master
