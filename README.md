This repository contains my SMAPI mods for Stardew Valley. See the individual mods for
documentation and release notes.

## Mods
Active mods:
* **[Automate](http://www.nexusmods.com/stardewvalley/mods/1063)** <small>([source](Automate))</small>  
  _Place a chest next to a machine (like a furnace or crystalarium), and the machine will
  automatically pull raw items from the chest and push processed items into it. Connect multiple
  machines with a chest to create factories._

* **[Chests Anywhere](http://www.nexusmods.com/stardewvalley/mods/518)** <small>([source](ChestsAnywhere))</small>  
  _Access your chests from anywhere and organise them your way. Transfer items without having to
  run around, from the comfort of your bed to the deepest mine level._

* **[Content Patcher](https://www.nexusmods.com/stardewvalley/mods/1915)** <small>([source](ContentPatcher))</small>  
  _Load content packs that change the game's images and data without replacing XNB files. Unlike
  XNB mods, these content packs get automatic update checks and compatibility checks, are easy to
  install and uninstall, and are less likely to break due to game updates._

* **[Data Layers](https://www.nexusmods.com/stardewvalley/mods/1691)** <small>([source](DataLayers))</small>  
  _Overlays the world with overlays the world with visual data like accessibility,
  bee/Junimo/scarecrow/sprinkler coverage, etc. It automatically includes data from other mods if
  applicable._

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

* **[Tractor Mod](http://www.nexusmods.com/stardewvalley/mods/1401)** <small>([source](TractorMod))</small>  
  _Lets you buy a tractor to more efficiently till/fertilize/seed/water/harvest crops, clear rocks, etc._

Not released yet:
* **Crops In Any Season** <small>([source](CropsInAnySeason))</small>  
  _Lets you grow crops in any season, including winter._

Inactive mods:
* ~~No Debug Mode~~  
  _(deleted) Disables SMAPI's F2 debug mode, which can cause unintended effects like skipping an
  entire season or teleporting into walls. No longer needed after SMAPI 1.0._

* ~~[The Long Night](http://www.nexusmods.com/stardewvalley/mods/1369)~~ <small>([source](LongNight))</small>  
  _Disables collapsing. You just stay awake forever and the night never ends (until you go to bed).
  Broke permanently in Stardew Valley 1.3.20._

## Translating the mods
The mods can be translated into any language supported by the game, and SMAPI will automatically
use the right translations.

(❑ = untranslated, ↻ = partly translated, ✓ = fully translated)

&nbsp;     | Chests Anywhere                          | Data Layers                          | Debug Mode                          | Lookup Anything                          | Tractor Mod
---------- | :--------------------------------------- | :----------------------------------- | :---------------------------------- | :--------------------------------------- | :------------------------------
Chinese    | [↻ partial](ChestsAnywhere/i18n/zh.json) | [↻ partial](DataLayers/i18n/zh.json) | [✓](DebugMode/i18n/zh.json)        | [↻ partial](LookupAnything/i18n/zh.json) | [✓](TractorMod/i18n/zh.json)
German     | [↻ partial](ChestsAnywhere/i18n/de.json) | [↻ partial](DataLayers/i18n/de.json) | [✓](DebugMode/i18n/de.json)        | [↻ partial](LookupAnything/i18n/de.json) | [✓](TractorMod/i18n/de.json)
Japanese   | [✓](ChestsAnywhere/i18n/ja.json)        | [✓](DataLayers/i18n/ja.json)        | [✓](DebugMode/i18n/ja.json)        | [✓](LookupAnything/i18n/ja.json)        | [✓](TractorMod/i18n/ja.json)
Portuguese | [✓](ChestsAnywhere/i18n/pt.json)        | [✓](DataLayers/i18n/pt.json)        | [✓](DebugMode/i18n/pt.json)        | [✓](LookupAnything/i18n/pt.json)        | [✓](TractorMod/i18n/pt.json)
Russian    | [↻ partial](ChestsAnywhere/i18n/ru.json) | [↻ partial](DataLayers/i18n/ru.json) | ↻ [partial](DebugMode/i18n/ru.json) | [↻ partial](LookupAnything/i18n/ru.json) | [✓](TractorMod/i18n/ru.json)
Spanish    | [✓](ChestsAnywhere/i18n/es.json)        | [✓](DataLayers/i18n/es.json)        | [✓](DebugMode/i18n/es.json)        | [✓](LookupAnything/i18n/es.json)        | [✓](TractorMod/i18n/es.json)

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

These mods use the [crossplatform build config](https://www.nuget.org/packages/Pathoschild.Stardew.ModBuildConfig)
so they can be built on Linux, Mac, and Windows without changes. See [the build config documentation](https://www.nuget.org/packages/Pathoschild.Stardew.ModBuildConfig)
for troubleshooting.

### Compiling a mod for testing
To compile a mod and add it to your game's `Mods` directory:

1. Rebuild the project in [Visual Studio](https://www.visualstudio.com/vs/community/) or [MonoDevelop](http://www.monodevelop.com/).  
   <small>This will compile the code and package it into the mod directory.</small>
2. Launch the project with debugging.  
   <small>This will start the game through SMAPI and attach the Visual Studio debugger.</small>

### Compiling a mod for release
To package a mod for release:

1. Switch to `Release` build configuration.
2. Recompile the mod per the previous section.
3. Upload the generated `bin/Release/<mod name>-<version>.zip` file from the project folder.
