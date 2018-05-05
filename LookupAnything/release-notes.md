[← back to readme](README.md)

# Release notes
## 1.19
* Updated for Stardew Valley 1.3 (including multiplayer support).
* Added support for looking up other players.
* Added support for new friendship data.
* Improved Russian translations. (Thanks to fadedDexofan!)

## 1.18.1
* Fixed error when looking up something before the save is loaded (thanks to f4iTh!).

## 1.18
* Updated to SMAPI 2.4.
* Added support for furniture.
* Added support for custom machines and objects from Custom Farming Redux 2.3.6+.
* Fixed debug key working when a menu is open.
* Fixed typo in debug interface.
* Improved Chinese and German translations. (Thanks to yuwenlan and Husky110!)

## 1.17
* Updated to SMAPI 2.0.
* Switched to SMAPI unified controller/keyboard/mouse bindings in `config.json`.
* Switched to SMAPI update checks.
* Fixed errors in modded object data causing all lookups to fail.
* Fixed basic bat kills not counted towards Adventure Quest goal.
* Fixed `HideOnKeyUp` mode not returning to previous menu on close.
* Improved translations thanks to Dorian/[@d0x7](https://github.com/d0x7) (German) and [@TaelFayre](https://github.com/TaelFayre) (Portuguese).

## 1.16
* NPC gift tastes now list inventory and owned items first.
* Added warning when translation files are missing.
* Fixed items inside buildings constructed in custom locations not being found for gift taste info.
* Fixed lookup errors with some custom NPCs.

## 1.15.1
* Added Russian translations. (Thanks to SteaNN!)
* Fixed missing translation in child 'age' field.
* Fixed incorrect child age calculation.

## 1.15
* You can now look up your children.
* Added Portuguese and Spanish translations. (Thanks to Fabilows and ThomasGabrielDelavault respectively!)
* Improved lookup matching — if there's no sprite under the cursor, it now tries to look up the tile contents.
* Fixed animal 'complaint' field text when an animal was attacked overnight.
* Fixed item 'needed for' field incorrectly matching non-fish items for fishing bundles.
* Fixed item 'needed for' field not showing bundle area names in English.

## 1.14
* Updated to SMAPI 1.14.
* Added translation support.
* Added Chinese and German translations. (Thanks to yuwenlan and Sasara respectively!)
* You can now look up items from the Junimo bundle menu.
* Fixed a few lookup errors when playing in a language other than English.

## 1.13
* Updated for Stardew Valley 1.2.

## 1.12.1
* Fixed calendar lookup not working in Stardew Valley 1.2 beta.

## 1.12
* Updated to SMAPI 1.9.
* Backported to Stardew Valley 1.11 until 1.2 is released.
* Fixed incorrect sell price shown for equipment.
* Fixed incorrect fruit tree quality info.
* Fixed rare error caused by duplicate NPC names.
* Fixed furniture/wallpaper being shown as potential recipe ingredients.

## 1.11
* Updated for Stardew Valley 1.2.

## 1.10.1
See [log](https://github.com/Pathoschild/StardewMods/compare/ef72f731449a795f0a1b478fdcb98bdda80d8020...lookup-anything/1.10.1).

* Fixed tile lookups always enabled regardless of `config.json`.

## 1.10
See [log](https://github.com/Pathoschild/StardewMods/compare/lookup-anything/1.9...lookup-anything/1.10).

* You can now look up an item from the kitchen cooking menu.
* You can now look up map tile info (disabled by default).
* Updated to SMAPI 1.8.
* Updated new-version-available check.

## 1.9
See [log](https://github.com/Pathoschild/StardewMods/compare/lookup-anything/1.8...lookup-anything/1.8).

* Villager lookups now highlight gifts you carry or own.
* Added optional data mining fields which show raw game data.
* You can now click on the up/down arrows to scroll content.
* Improved controller support:
  * You can now look up what's directly in front of you using a separate hotkey. (Not bound by default.)
  * Fixed controller thumbsticks scrolling content too slowly.
  * Fixed controller button conventions not respected by lookup menu.
* Fixed a rare error caused by the game duplicating an NPC.
* Fixed fruit tree quality schedule being wrong in some cases.
* Fixed input bindings in `config.json` being case-sensitive.
* Fixed input bindings in `config.json` being discarded silently if invalid.

## 1.8
See [log](https://github.com/Pathoschild/StardewMods/compare/lookup-anything/1.7...lookup-anything/1.8).

* Added museum donations to item 'needed for' field.
* You can now look up things behind trees when you're behind them.
* You can now close the lookup UI by clicking outside it.
* Updated to SMAPI 1.3.
* Fixed incorrect farmer luck message when the spirits are feeling neutral.
* Fixed social menu lookup sometimes showing the wrong villager.

## 1.7
See [log](https://github.com/Pathoschild/StardewMods/compare/lookup-anything/1.6...lookup-anything/1.7).

* You can now look up a villager from the social page.
* You can now look up an item from the toolbar.
* Console logs are now less verbose.
* Updated to SMAPI 1.1.
* Fixed some cases where the item 'number owned' field was inacurate.
* Fixed iridium prices being shown for items that can't have iridium quality.
* `F2` debug mode is no longer suppressed (removed in latest version of SMAPI).

## 1.6
See [log](https://github.com/Pathoschild/StardewMods/compare/lookup-anything/1.5...lookup-anything/1.6).

* Added support for Linux and Mac.
* Added item 'needed for' field for community center bundles, full shipment achievement, and polyculture achievement.
* Added item 'sells to' field.
* Added item number owned field.
* Added fruit tree quality schedule.
* Added support for looking up shop items.
* Added `data.json` validation on startup.
* Disabled lookups when game rendering mode breaks Lookup Anything (only known to happen in the Stardew Valley Fair).
* Fixed sale price shown for unsellable items.
* Fixed update-check error on startup adding scary error text in console.
* Fixed incorrect gift tastes by deferring more to the game code (slower but more accurate).
* Fixed error when looking up a villager you haven't met.
* Fixed error when looking up certain NPCs with no social data.

## 1.5
See [log](https://github.com/Pathoschild/StardewMods/compare/lookup-anything/1.4...lookup-anything/1.5).

* You can now look up a villager from the calendar.
* You can now look up items from an open chest.
* Added cask aging schedule.
* Added better NPC friendship fields which account for dating and marriage.
* Added marriage stardrop to heart meter.
* Added support for new iridium quality.
* Added debug log.
* Added option to suppress SMAPI's `F2` debug hotkey, which can have unintended consequences like skipping an entire season or teleporting into walls.
* Fixed gift tastes not handling precedence when NPCs are conflicted about how they feel.
* Fixed error when screen resolution is too small to display lookup UI.
* Fixed error when calculating a day offset that wraps into the next year.
* Fixed errors crashing the game in rare cases.

## 1.4
See [log](https://github.com/Pathoschild/StardewMods/compare/lookup-anything/1.3...lookup-anything/1.4).

* Updated for Stardew Valley 1.1:
  * added new fertile weeds (forest farm) and geode stones (hilltop farm);
  * added new recipes for coffee, mead, sugar, void mayonnaise, and wheat flour;
  * updated for Gold Clock preventing fence decay;
  * updated to latest binaries & increased minimum versions.
* Fixed a few missing stones & weeds.

## 1.3
See [log](https://github.com/Pathoschild/StardewMods/compare/lookup-anything/1.2...lookup-anything/1.3).

* Added possible drops and their probability to monster lookup.
* Added item icons to crafting output, farm animal produce, and monster drops.
* Fixed item gift tastes being wrong in some cases.
* Fixed monster drops showing 'error item' in rare cases.
* Fixed fields being shown for dead crops.
* Internal refactoring.

## 1.2
See [log](https://github.com/Pathoschild/StardewMods/compare/lookup-anything/1.1...lookup-anything/1.2).

* On item lookup:
  * added crop info for seeds;
  * added recipes for the charcoal kiln, cheese press, keg, loom, mayonnaise machine, oil maker,
    preserves jar, recycling machine, and slime egg-press;
  * merged recipe fields;
  * fixed an error when displaying certain recipes.
* Added optional mode which hides the lookup UI when you release the button.
* `F1` now toggles the lookup UI (i.e. will close the lookup if it's already open).

## 1.1
See [log](https://github.com/Pathoschild/StardewMods/compare/lookup-anything/1.0...lookup-anything/1.1).

* On item lookup:
  * removed crafting recipe;
  * added crafting, cooking, and furnace recipes which use this item as an ingredient.
* Added error if game or SMAPI are out of date.

## 1.0
See [log](https://github.com/Pathoschild/StardewMods/compare/601d3c7964c5f2448f2791cd6f7205cb0b2f0835...lookup-anything/1.0).

* Initial version.
* Added support for NPCs (villagers, pets, farm animals, monsters, and players), items (crops and
   inventory), and map objects (crafting objects, fences, trees, and mine objects).
* Added controller support and configurable bindings.
* Added hidden debug mode.
* Added version check on load.
* Let players look up a target from any visible part of its sprite.

