<<<<<<< HEAD
This repository contains my SMAPI mods for Stardew Valley. See the individual mods for
documentation and release notes:

* [Chests Anywhere](ChestsAnywhere)
* [Lookup Anything](LookupAnything)

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
**Skip Intro** is a minimal [Stardew Valley](http://stardewvalley.net/) mod that skips the game's loading intro.

## Installation
1. [Install SMAPI 1.3+](https://github.com/Pathoschild/SMAPI).
2. Install [this mod from Nexus mods](http://www.nexusmods.com/stardewvalley/mods/533).
3. Run the game using SMAPI.

## Versions
1.0:
* Initial release.

1.1:
* Updated for Stardew Valley 1.1.

1.2:
* Added support for Linux and Mac.
* Updated to SMAPI 1.3.

## Compiling the mod
[Installing a stable release from Nexus Mods](http://www.nexusmods.com/stardewvalley/mods/533/) is
recommended. If you really want to compile the mod yourself, just edit `Pathoschild.StardewValley.SkipIntro.csproj`
and set the `<GamePath>` setting to your Stardew Valley directory path. Launching the project in Visual
Studio will compile the code, package it into the mod directory, and start the game.

## See also
* [Nexus mod](http://www.nexusmods.com/stardewvalley/mods/533)
* [Discussion thread](http://community.playstarbound.com/threads/smapi-skip-intro.123673/)
* My other Stardew Valley mods: [Lookup Anything](https://github.com/Pathoschild/LookupAnything) and [Chests Anywhere](https://github.com/Pathoschild/ChestsAnywhere)
>>>>>>> skipintro/master
