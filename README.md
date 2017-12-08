This repository contains my SMAPI mods for Stardew Valley. See the individual mods for
documentation and release notes.

All mods are compatible with Stardew Valley 1.2+ on Linux, Mac, and Windows.

## Mods
Active mods:
* **[Automate](http://www.nexusmods.com/stardewvalley/mods/1063)** <small>([source](Automate))</small>  
  _Place a chest next to a machine (like a furnace or crystalarium), and the machine will
  automatically pull raw items from the chest and push processed items into it. Connect multiple
  machines with a chest to create factories._

* **[Chests Anywhere](http://www.nexusmods.com/stardewvalley/mods/518)** <small>([source](ChestsAnywhere))</small>  
  _Access your chests from anywhere and organise them your way. Transfer items without having to
  run around, from the comfort of your bed to the deepest mine level._

* **[Debug Mode](http://www.nexusmods.com/stardewvalley/mods/679)** <small>([source](DebugMode))</small>  
  _Press a button to view debug information and unlock the game's built-in debug commands
  (including teleportation and time manipulation)._

* **[Fast Animations](http://www.nexusmods.com/stardewvalley/mods/1089)** <small>([source](FastAnimations))</small>  
  _Speed up many animations in the game (currently eating, drinking, milking, shearing, and
  breaking geodes). Optionally configure the speed for each animation._

* **[Lookup Anything](http://www.nexusmods.com/stardewvalley/mods/541)** <small>([source](LookupAnything))</small>  
  _See live info about whatever's under your cursor when you press F1. Learn a villager's favourite
  gifts, when a crop will be ready to harvest, how long a fence will last, why your farm animals
  are unhappy, and more._

* **[Rotate Toolbar](http://www.nexusmods.com/stardewvalley/mods/1100)** <small>([source](RotateToolbar))</small>  
  _Rotate the top inventory row for the toolbar by pressing Tab (configurable)._

* **[Skip Intro](http://www.nexusmods.com/stardewvalley/mods/533)** <small>([source](SkipIntro))</small>  
  _Skip straight to the title screen or load screen (configurable) when you start the game. It also
  skips the screen transitions, so starting the game is much faster._

* **[The Long Night](http://www.nexusmods.com/stardewvalley/mods/1369)** <small>([source](LongNight))</small>  
  _Disables collapsing. You just stay awake forever and the night never ends (until you go to bed)._

* **[Tractor Mod](http://www.nexusmods.com/stardewvalley/mods/1401)** <small>([source](TractorMod))</small>  
  _Lets you buy a tractor to more efficiently till/fertilize/seed/water/harvest crops, clear rocks, etc._

Inactive mods:
* **Data Maps** <small>([source](DataMaps))</small>  
  _(in development) Overlays the world with data maps to show accessibility, sprinkler coverage, etc._

* **No Debug Mode** <small>([source](NoDebugMode))</small>  
  _(obsolete) Disables SMAPI's F2 debug mode, which can cause unintended effects like skipping an
  entire season or teleporting into walls._

## Translating the mods
The mods can be translated into any language supported by the game, and SMAPI will automatically
use the right translations.

(❑ = untranslated, ↻ = partly translated, ✓ = fully translated)

&nbsp;     | Chests Anywhere | Data Maps | Debug Mode | Lookup Anything | Tractor Mod
---------- | :-------------- | :-------- | :--------- | :-------------- | :----------
Chinese    | ✓ [zh.json](ChestsAnywhere/i18n/zh.json) | ✓ [zh.json](DebugMode/i18n/zh.json) | ❑ | ✓ [zh.json](LookupAnything/i18n/zh.json) | ✓ [zh.json](TractorMod/i18n/zh.json)
German     | ↻ [de.json](ChestsAnywhere/i18n/de.json) | ✓ [de.json](DebugMode/i18n/de.json) | ❑ | ✓ [de.json](LookupAnything/i18n/de.json) | ✓ [de.json](TractorMod/i18n/de.json)
Japanese   | ❑ | ❑ | ❑ | ❑ | ❑
Portuguese | ✓ [pt.json](ChestsAnywhere/i18n/pt.json) | ✓ [pt.json](DebugMode/i18n/pt.json) | ❑ | ✓ [pt.json](LookupAnything/i18n/pt.json) | ✓ [pt.json](TractorMod/i18n/pt.json)
Russian    | ↻ [ru.json](ChestsAnywhere/i18n/ru.json) | ❑ | ❑ | ✓ [ru.json](LookupAnything/i18n/ru.json) | ✓ [ru.json](TractorMod/i18n/ru.json)
Spanish    | ↻ [es.json](ChestsAnywhere/i18n/es.json) | ✓ [es.json](DebugMode/i18n/es.json) | ❑ | ✓ [es.json](LookupAnything/i18n/de.json) | ❑

Here's how to translate one of my mods:

1. Copy `default.json` into a new file with the right name:

   language   | file name
   ---------- | ---------
   Chinese    | `zh.json`
   German     | `de.json`
   Japanese   | `ja.json`
   Portuguese | `pt.json`
   Spanish    | `es.json`

2. Translate the second part on each line:
   ```json
   "example-key": "some text here"
                   ^-- translate this
   ```
   Don't change the quote characters, and don't translate the text inside `{{these brackets}}`.
3. Launch the game to try your translations.  
   _You can edit translations without restarting the game; just type `reload_i18n` in the SMAPI console to reload the translation files._

Create an issue or pull request here with your translations, or send them to me privately. :)

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
