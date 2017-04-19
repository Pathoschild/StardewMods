This repository contains my SMAPI mods for Stardew Valley. See the individual mods for
documentation and release notes.

## Mods
Active mods:
* **[Automate](http://www.nexusmods.com/stardewvalley/mods/1063)** <small>([source](Automate))</small>  
  _Place a chest next to a machine (like a furnace or crystalarium), and the machine will automatically pull raw items from the chest and push processed items into it. Connect multiple machines with a chest to create factories._

* **[Chests Anywhere](http://www.nexusmods.com/stardewvalley/mods/518)** <small>([source](ChestsAnywhere))</small>  
  _Access your chests from anywhere and organise them your way. Transfer items without having to run around, from the comfort of your bed to the deepest mine level._

* **[Debug Mode](http://www.nexusmods.com/stardewvalley/mods/679)** <small>([source](DebugMode))</small>  
  _Press a button to view debug information and unlock the game's built-in debug commands (including teleportation and time manipulation)._

* **[Fast Animations](http://www.nexusmods.com/stardewvalley/mods/1089)** <small>([source](FastAnimations))</small>  
  _Speed up many animations in the game (currently eating, drinking, milking, shearing, and breaking geodes). Optionally configure the speed for each animation._

* **[Lookup Anything](http://www.nexusmods.com/stardewvalley/mods/541)** <small>([source](LookupAnything))</small>  
  _See live info about whatever's under your cursor when you press F1. Learn a villager's favourite gifts, when a crop will be ready to harvest, how long a fence will last, why your farm animals are unhappy, and more._

* **[Rotate Toolbar](http://www.nexusmods.com/stardewvalley/mods/1100)** <small>([source](RotateToolbar))</small>  
  _Rotate the top inventory row for the toolbar by pressing Tab (configurable)._

* **[Skip Intro](http://www.nexusmods.com/stardewvalley/mods/533)** <small>([source](SkipIntro))</small>  
  _Skip straight to the load screen when you start the game, bypassing the intro logos and title screen._

Inactive mods:
* **Data Maps** <small>([source](DataMaps))</small>  
  _(in development) Overlays the world with metadata maps._

* **No Debug Mode** <small>([source](NoDebugMode))</small>  
  _(obsolete) Disables SMAPI's F2 debug mode, which can cause unintended effects like skipping an entire season or teleporting into walls._

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
4. Create a zip file of the mod's folder in the `Mods` folder. The zip name should include the
   mod name and version (like `LookupAnything-1.0.zip`).
