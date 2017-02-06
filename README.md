<<<<<<< HEAD
This repository contains my SMAPI mods for Stardew Valley. See the individual mods for
documentation and release notes:

* [Chests Anywhere](ChestsAnywhere)
* [Debug Mode](DebugMode)
* [Lookup Anything](LookupAnything)
* [No Debug Mode](NoDebugMode) (obsolete)
* [Skip Intro](SkipIntro)

## Compiling the mods
Installing stable releases from Nexus Mods is recommended for most users. If you really want to
compile the mod yourself, read on.

These mods use the [crossplatform build config](https://github.com/Pathoschild/Stardew.ModBuildConfig#readme)
so they can be built on Linux, Mac, and Windows without changes. See [the build config documentation](https://github.com/Pathoschild/Stardew.ModBuildConfig#readme)
for troubleshooting.

### Compiling a mod for testing
To compile a mod and add it to your game's `Mods` directory:
=======
﻿**Data Maps** is a [Stardew Valley](http://stardewvalley.net/) mod that overlays the world with
metadata maps.

Compatible with Stardew Valley 1.11+ on Linux, Mac, and Windows.

**This is in the early prototype stage.**

## Contents
* [Installation](#installation)
* [Usage](#usage)
* [Examples](#examples)
* [Configuration](#configuration)
* [Versions](#versions)
* [Compiling the mod](#compiling-the-mod)
* [See also](#see-also)

## Installation
1. [Install the latest version of SMAPI](https://github.com/Pathoschild/SMAPI/releases).
2. <s>Install this mod from Nexus mods</s>.
3. Run the game using SMAPI.

## Usage
_TO DO_

## Examples
_TO DO_

## Configuration
_TO DO_

## Versions
_TO DO_

## Compiling the mod
<s>Installing a stable release from Nexus Mods</s> is
recommended for most users. If you really want to compile the mod yourself, read on.

This mod uses the [crossplatform build config](https://github.com/Pathoschild/Stardew.ModBuildConfig#readme)
so it can be built on Linux, Mac, and Windows without changes. See [its documentation](https://github.com/Pathoschild/Stardew.ModBuildConfig#readme)
for troubleshooting.

### Compiling the mod for testing
To compile the mod and add it to the mods directory:
>>>>>>> datamaps/master

1. Rebuild the project in [Visual Studio](https://www.visualstudio.com/vs/community/) or [MonoDevelop](http://www.monodevelop.com/).  
   <small>This will compile the code and package it into the mod directory.</small>
2. Launch the project with debugging.  
   <small>This will start the game through SMAPI and attach the Visual Studio debugger.</small>

<<<<<<< HEAD
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
### Compiling the mod for release
To package the mod for release:

1. Delete the game's `Mods/DataMaps` directory.  
   <small>(This ensures the package is clean and has default configuration.)</small>
2. Recompile the mod per the previous section.
3. Launch the game through SMAPI to generate the default `config.json`.
2. Create a zip file of the game's `Mods/DataMaps` folder. The zip name should include the
   mod name and version. For example:

   ```
   DataMaps-1.0.zip
      DataMaps/
         Pathoschild.Stardew.DataMaps.dll
         Pathoschild.Stardew.DataMaps.pdb
         config.json
         manifest.json
   ```

## See also
* <s>Nexus mod</s>
* <s>Discussion thread</s>
* My other Stardew Valley mods: [Chests Anywhere](https://github.com/Pathoschild/ChestsAnywhere), [Lookup Anything](https://github.com/Pathoschild/LookupAnything), [No Debug Mode](https://github.com/Pathoschild/Stardew.NoDebugMode), and [Skip Intro](https://github.com/Pathoschild/StardewValley.SkipIntro)
>>>>>>> datamaps/master
