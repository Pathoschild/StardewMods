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

* **[Crops Anytime Anywhere](https://www.nexusmods.com/stardewvalley/mods/3000)** <small>([source](CropsAnytimeAnywhere))</small>  
  _Lets you grow crops in any season and location, including on grass/dirt tiles you normally
  couldn't till._

* **[Data Layers](https://www.nexusmods.com/stardewvalley/mods/1691)** <small>([source](DataLayers))</small>  
  _Overlays the world with visual data like accessibility, bee/Junimo/scarecrow/sprinkler coverage,
  etc. It automatically includes data from other mods if applicable._

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

* **Noclip Mode** <small>([source](NoclipMode))</small>  
  _Toggle [noclip mode](https://en.wikipedia.org/wiki/Noclip_mode) at the press of a button,
  letting you walk through anything (even map boundaries)._

* **[Rotate Toolbar](http://www.nexusmods.com/stardewvalley/mods/1100)** <small>([source](RotateToolbar))</small>  
  _Rotate the top inventory row for the toolbar by pressing Tab (configurable)._

* **[Skip Intro](http://www.nexusmods.com/stardewvalley/mods/533)** <small>([source](SkipIntro))</small>  
  _Skip straight to the title screen or load screen (configurable) when you start the game. It also
  skips the screen transitions, so starting the game is much faster._

* **[Small Beach Farm](http://www.nexusmods.com/stardewvalley/mods/3750)** <small>([source](SmallBeachFarm))</small>  
  _Replaces the riverlands farm with a fertile pocket beach, suitable for slower or challenge runs._

* **[Tractor Mod](http://www.nexusmods.com/stardewvalley/mods/1401)** <small>([source](TractorMod))</small>  
  _Lets you buy a tractor to more efficiently till/fertilize/seed/water/harvest crops, clear rocks, etc._

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

&nbsp;     | Chests Anywhere                          | Data Layers                          | Debug Mode                          | Lookup Anything                          | Noclip Mode | Tractor Mod
---------- | :--------------------------------------- | :----------------------------------- | :---------------------------------- | :--------------------------------------- | ----------- | :------------------------------
Chinese    | [✓](ChestsAnywhere/i18n/zh.json)        | [✓](DataLayers/i18n/zh.json)        | [✓](DebugMode/i18n/zh.json)        | [↻ partial](LookupAnything/i18n/zh.json) | ❑          | [✓](TractorMod/i18n/zh.json)
French     | [↻ partial](ChestsAnywhere/i18n/fr.json) | [↻ partial](DataLayers/i18n/fr.json) | [✓](DebugMode/i18n/fr.json)        | [↻ partial](LookupAnything/i18n/fr.json) | ❑          |  [✓](TractorMod/i18n/fr.json)
German     | [✓](ChestsAnywhere/i18n/de.json)        | [✓](DataLayers/i18n/de.json)        | [✓](DebugMode/i18n/de.json)        | [↻ partial](LookupAnything/i18n/de.json) | ❑          |  [✓](TractorMod/i18n/de.json)
Hungarian  | ❑                                       | ❑                                   | ❑                                  | ❑                                       | ❑          | ❑
Italian    | ❑                                       | ❑                                   | ❑                                  | ❑                                       | ❑          | ❑
Japanese   | [↻ partial](ChestsAnywhere/i18n/ja.json) | [↻ partial](DataLayers/i18n/ja.json) | [✓](DebugMode/i18n/ja.json)        | [↻ partial](LookupAnything/i18n/ja.json) | ❑          | [✓](TractorMod/i18n/ja.json)
Korean     | [✓](ChestsAnywhere/i18n/ko.json)        | [✓](DataLayers/i18n/ko.json)        | [✓](DebugMode/i18n/ko.json)        | [↻ partial](LookupAnything/i18n/ko.json) | ❑          | [✓](TractorMod/i18n/ko.json)
Portuguese | [↻ partial](ChestsAnywhere/i18n/pt.json) | [↻ partial](DataLayers/i18n/pt.json) | [✓](DebugMode/i18n/pt.json)        | [↻ partial](LookupAnything/i18n/pt.json) | ❑          | [✓](TractorMod/i18n/pt.json)
Russian    | [✓](ChestsAnywhere/i18n/ru.json)        | [✓](DataLayers/i18n/ru.json)        | [✓](DebugMode/i18n/ru.json)        | [↻ partial](LookupAnything/i18n/ru.json) | ❑          | [✓](TractorMod/i18n/ru.json)
Spanish    | [✓](ChestsAnywhere/i18n/es.json)        | [↻ partial](DataLayers/i18n/es.json) | [✓](DebugMode/i18n/es.json)        | [↻ partial](LookupAnything/i18n/es.json) | ❑          | [✓](TractorMod/i18n/es.json)
Turkish    | ❑                                       | ❑                                   | ❑                                  | ❑                                       | ❑          | ❑

Contributions are welcome! See [Modding:Translations](https://stardewvalleywiki.com/Modding:Translations)
on the wiki for help contributing translations.

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
