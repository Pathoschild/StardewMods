[← back to readme](README.md)

# Release notes
## Upcoming release
* Added support for custom building rules in the game's new `Data/Buildings` asset.
* Fixed support for custom trash cans outside the town map.
* Fixed trash can tracking for pre-1.6 town map mods.
* Improved translations. Thanks to burunduk (updated Ukrainian) and Curotar (updated Russian)!

## 2.0.7
Released 15 April 2024 for SMAPI 4.0.0 or later.

* Fixed Ginger Island shipping bin being automated before it's unlocked.
* Fixed Automate getting the wrong item from custom bushes (thanks to LeFauxMatt!).
* Improved translations. Thanks to Jualko (updated German), Kaian-Campos (updated Portuguese), and MakinDay (updated Italian)!

## 2.0.6
Released 08 April 2024 for SMAPI 4.0.0 or later.

* Added support for multiple machines per tile. For example, Automate can now simultaneously collect moss/seeds and run a tapper on the same tree.
* Set minimum game version to 1.6.3 to avoid confusing 'no longer compatible' message.
* Fixed buildings sometimes not automated if there's flooring under them.
* Fixed machine names not translated in Generic Mod Config Menu UI.
* Fixed typo in `automate help` output.
* Improved translations. Thanks to mc-kaishixiaxue (updated Chinese)!

## 2.0.5
Released 07 April 2024 for SMAPI 4.0.0 or later.

* Automate now ignores raisins in Junimo huts.
* Added config option to toggle whether Automate collects moss from trees.
* Fixed some machines missing in Generic Mod Config Menu UI.
* Fixed errors when an automated bush is destroyed.
* Fixed tree machines not always updated correctly if moss is collected manually before the machine does it.

## 2.0.4
Released 04 April 2024 for SMAPI 4.0.0 or later.

* Chests now collect moss from connected trees (thanks to skoliosaurus!).
* Improved translations. Thanks to EngurRuzgar (updated Turkish), JhonatanMedeiros (updated Portuguese), Jualko (updated German), and MakinDay (updated Italian)!

## 2.0.3
Released 21 March 2024 for SMAPI 4.0.0 or later.

* Fixed machines not pulling items from Junimo chests.
* Fixed error loading machines if another mod added an empty `Action` map tile property.
* Fixed error collecting output from data-based machines which provide XP.
* Fixed repeating errors when a custom automation factory throws an unhandled exception. Automate will now log a single error with the relevant info.

## 2.0.2
Released 21 March 2024 for SMAPI 4.0.0 or later.

* Fixed some `Data/Machines` logic not being applied. This caused issues like crystalariums and worm bins not resuming when their output was collected.

## 2.0.1
Released 20 March 2024 for SMAPI 4.0.0 or later.

* Fixed support for the new big chests in Stardew Valley 1.6.
* Fixed chests collecting endless seeds from trees.
* Fixed new fairy dust option not shown in Generic Mod Config Menu.

## 2.0.0
Released 19 March 2024 for SMAPI 4.0.0 or later.

* Updated for Stardew Valley 1.6.
* Added support for custom machines in the new `Data/Machines` asset.
* Added support for custom floors/paths as connectors.
* Added support for fairy dust. You can configure the minimum processing time for which to apply it in `config.json`.
* Added options in Generic Mod Config Menu to toggle or set the priority for all machines in `Data/Machines`.
* `automate summary` now shows each chest's automation options if edited.
* Removed the 'prevent empty stack' chest option. This is no longer feasible due to how machines work in Stardew Valley 1.6.
* Automating a cask will no longer let it work outside the cellar, due to changes in how the mod works for Stardew Valley 1.6.
* Improved translations. Thanks to EmWhyKay (updated Turkish)!
* Fixed errors if some config fields are set to null.

## 1.28.7
Released 01 December 2023 for SMAPI 3.14.0 or later.

* Improved translations. Thanks to angel4killer (added Russian)!

## 1.28.6
Released 03 October 2023 for SMAPI 3.14.0 or later.

* Improved compatibility with recent game updates on Android.
* Fixed Junimo Chest groups not always updated when a machine is placed or removed.

## 1.28.5
Released 27 August 2023 for SMAPI 3.14.0 or later.

* Improved translations. Thanks to MyEclipseyang (updated Chinese)!

## 1.28.4
Released 25 June 2023 for SMAPI 3.14.0 or later.

* Embedded `.pdb` data into the DLL, which fixes error line numbers in Linux/macOS logs.

## 1.28.3
Released 30 March 2023 for SMAPI 3.14.0 or later.

* Improved translations. Thanks to DAhrin (updated German), MakinDay (updated Italian), martin66789 (updated Hungarian), and Mysti57155 (added French)!

## 1.28.2
Released 09 January 2023 for SMAPI 3.14.0 or later.

* Improved split-screen support.
* Improved translations. Thanks to barnibasha (updated Hungarian) and wally232 (updated Korean)!

## 1.28.1
Released 30 October 2022 for SMAPI 3.14.0 or later.

* Updated integration with Generic Mod Config Menu.
* Improved translations. Thanks to ChulkyBow (updated Ukrainian) and watchakorn-18k (added Thai)!

## 1.28.0
Released 10 October 2022 for SMAPI 3.14.0 or later.

* Added option to transfer seeds & fertilizer into Junimo huts to automate replanting with [Better Junimos](https://www.nexusmods.com/stardewvalley/mods/2221) (thanks to kzawora)!
* Overhauled Junimo hut settings to be more flexible.
* Fixed items in Junimo Chests sometimes counted multiple times after 1.27.5.
* Fixed deconstructor accepting items it normally wouldn't.

## 1.27.5
Released 29 August 2022 for SMAPI 3.14.0 or later.

* The `automate summary` command now shows all the connected locations for a Junimo chest group.
* Fixed error using `automate summary` with an unlimited chest from the Chests and Pouches mod.
* Internal changes to simplify supporting custom linked chests in future versions.

## 1.27.4
Released 18 August 2022 for SMAPI 3.14.0 or later.

* Improved translations. Thanks to alpeerkaraca (updated Turkish), LeecanIt (added Italian), and NightFright2k19 (updated German)!

## 1.27.3
Released 04 July 2022 for SMAPI 3.14.0 or later.

* Fixed machine scan failing if another mod adds broken furniture or terrain features.
* Fixed overlay showing Junimo chest tiles from other locations.
* Improved translations. Thanks to martin66789 (updated Hungarian)!

## 1.27.2
Released 18 June 2022 for SMAPI 3.14.0 or later.

* Fixed machines not indexed correctly in 1.27.1.

## 1.27.1
Released 17 June 2022 for SMAPI 3.14.0 or later.

* Optimized to avoid rescan when a machine is removed from an inactive machine group.
* Fixed error when placing machines in a building which was upgraded on the same day after 1.27.0.
* Fixed automation overlay not showing groups connected to a Junimo chest after 1.27.0.
* Fixed automation overlay empty for farmhands in multiplayer after 1.27.0.

## 1.27.0
Released 05 June 2022 for SMAPI 3.14.0 or later.

* Rewrote location change tracking to fix lag in some cases.
* The automation overlay now shows a live view of the actual automation state, instead of rescanning machines.
* Added `automate reset` console command to force Automate to rescan for machines.
* Optimized edge case where many machines have output ready but their connected chests are all full.

## 1.26.1
Released 27 May 2022 for SMAPI 3.14.0 or later.

* Fixed some config values not loaded correctly in Automate 1.26.0.
* Improved translations. Thanks to mukers (added Russian)!

## 1.26.0
Released 09 May 2022 for SMAPI 3.14.0 or later.

* Updated for SMAPI 3.14.0.
* Added option to toggle whether Automate is enabled through [Generic Mod Config Menu](https://www.nexusmods.com/stardewvalley/mods/5098).
* Fixed machine overrides through Generic Mod Config Menu not saved correctly.
* Fixed error when a geode crusher produces a [golden helmet](https://stardewvalleywiki.com/Golden_Helmet).
* Improved translations. Thanks to ChulkyBow (updated Ukrainian), JingGongGi (added Chinese), and Pedrowser (added Portuguese)!

## 1.25.3
Released 27 February 2022 for SMAPI 3.13.0 or later.

* Optimized legacy save migration a bit (thanks to Michael Kuklinski / Ameisen!).
* Improved translations. Thanks to EmWhyKay (added Turkish), martin66789 (added Hungarian), and wally232 (added Korean)!

## 1.25.2
Released 14 January 2022 for SMAPI 3.13.0 or later.

* Fixed bushes in garden pots sometimes producing a second harvest when you enter the location.
* Improved translations. Thanks to ChulkyBow (added Ukrainian), Evexyron (added Spanish), and mezen (added German)!

## 1.25.1
Released 25 December 2021 for SMAPI 3.13.0 or later.

* Fixed load error in the previous update.

## 1.25.0
Released 25 December 2021 for SMAPI 3.13.0 or later.

* Added in-game config UI through [Generic Mod Config Menu](https://www.nexusmods.com/stardewvalley/mods/5098).
* Internal optimizations.

## 1.24.1
Released 30 November 2021 for SMAPI 3.13.0 or later.

* Updated for Stardew Valley 1.5.5 and SMAPI 3.13.0.

## 1.24.0
Released 27 November 2021 for SMAPI 3.12.5 or later.

* Added support for farmhouse/cabin fridges as automateable containers.
* Fixed tea bushes in indoor pots sometimes not automated correctly.
* Fixed unable to use indoor pots as custom connectors.

## 1.23.3
Released 31 October 2021 for SMAPI 3.12.5 or later.

* [Super Hoppers](https://www.nexusmods.com/stardewvalley/mods/9418) are now ignored.
* Machines are now rescanned each day to fix edge cases like building upgrades in multiplayer.
* Improved `automate summary` console command:
  * added machine states and chest capacity;
  * added a warning if common causes of lag are detected.
* Refactored console command handling.

**Update note for mod authors:**  
The `IContainer` interface has two new methods: `GetFilled()` and `GetCapacity()`. This probably
won't affect other mods, since only Automate is known to implement containers currently.

## 1.23.2
Released 24 July 2021 for SMAPI 3.9.5 or later.

* Removed integration with the deleted Auto-Grabber Mod.

## 1.23.1
Released 09 July 2021 for SMAPI 3.9.5 or later.

* Fixed issue where upgrading a building which contains machines wouldn't always update automation correctly until the save was reloaded.

## 1.23.0
Released 25 May 2021 for SMAPI 3.9.5 or later.

* In multiplayer mode, machines which give XP, update stats, or check professions now target the farmer who placed them instead of the host player where possible.  
  _(Due to a game bug, this only affects crab pots and fish ponds currently; see a [summary for each machine](README.md#in-multiplayer-who-gets-xp-and-whose-professions-apply).)_

## 1.22.1
Released 17 April 2021 for SMAPI 3.9.5 or later.

* The option to prevent removing the last item in a stack (added in Automate 1.22) is now [configured per-chest](README.md#in-game-settings) instead of globally.

## 1.22.0
Released 27 March 2021 for SMAPI 3.9.5 or later.

* Added option to prevent removing the last item in a stack from chests.
* Fixed compatibility with [unofficial 64-bit mode](https://stardewvalleywiki.com/Modding:Migrate_to_64-bit_on_Windows).

## 1.21.1
Released 07 March 2021 for SMAPI 3.9.0 or later.

* Trees that aren't full-grown are no longer automated, to avoid accidental machine links due to spreading seeds.

## 1.21.0
Released 06 February 2021 for SMAPI 3.9.0 or later.

* Added tree automation to collect seeds, golden coconuts, and Qi beans.
* You can now use furniture and floor dividers as connectors.
* Fixed tea bushes in garden pots not automated.
* Fixed bushes producing berries out of season once immediately after loading the save.

## 1.20.1
Released 23 January 2021 for SMAPI 3.9.0 or later.

* Updated for multi-key bindings in SMAPI 3.9.
* Fixed tappers on mahogany tree always producing one sap.
* Fixed errors when a chest contains an invalid item stack.

## 1.20.0
Released 16 January 2021 for SMAPI 3.8.0 or later.

* Added [full support for Junimo chests](README.md#junimo-chests), which enables distributed automation.
* Removed old `config.json` migrations. If you changed the mod options in Automate 1.17.3 or earlier, and haven't played since then, you may need to do it again.
* Moved the default machine priorities into `assets/data.json`. You can still override an entry in your `config.json`.
* Lowered priority for mini-shipping bins by default (similar to shipping bins).
* Fixed bushes not harvested out of season on the island farm.

## 1.19.1
Released 04 January 2021 for SMAPI 3.8.0 or later.

* When you join a multiplayer game, Automate shows a warning that it needs to be installed by the main player. That message is now clearer and only shown if they don't have it installed.
* Fixed slime egg incubators ignoring tiger slime eggs.
* Fixed deconstructor taking multiple inputs from the chest, but only producing output for the last one.

## 1.19.0
Released 21 December 2020 for SMAPI 3.8.0 or later.

* Updated for Stardew Valley 1.5, including...
  * split-screen mode and UI scaling;
  * new machines (bone mill, coffee maker, deconstructor, geode crusher, heavy tapper, hoppers, mini-shipping bin, ostrich incubator, solar panel, and statue of true perfection);
  * default shipping bin being moveable;
  * new chest and path types.

**Breaking changes for mod authors:**
* If you added a `chest.Capacity` property for Automate, you should patch or override the new
  `chest.GetActualCapacity()` method in 1.5 instead.
* Automate previously stored automation options in the chest name using tags like
  `|automate:no-store|`. These are now stored in the new `chest.modData` field added in 1.5; existing
  chests will be migrated automatically.

## 1.18.4
Released 05 December 2020 for SMAPI 3.7.3 or later.

* Internal changes to simplify upcoming updates.

## 1.18.3
Released 21 November 2020 for SMAPI 3.7.3 or later.

* When a mod adds custom machines but is missing something for Automate compatibility, Automate now shows a warning with instructions. This can be disabled in `config.json`.
* Fixed constructed shipping bins triggering the main shipping bin's animation.
* Fixed shipped items not added to existing stacks if possible.

## 1.18.2
Released 04 November 2020 for SMAPI 3.7.3 or later.

* Fixed machine priority inverted in 1.18.

## 1.18.1
Released 15 October 2020 for SMAPI 3.7.3 or later.

* Refactored to prepare for future game updates.

## 1.18.0
Released 12 September 2020 for SMAPI 3.7.2 or later.

* Added [`automate summary` console command](README.md#console-command).
* You can now [disable automation for specific machine types](README.md#per-machine-settings).
* Fixed shipping bins not having a lower priority than other machines by default as intended. Affected `config.json` will be corrected automatically.

## 1.17.3
Released 19 August 2020 for SMAPI 3.6.0 or later.

* Fixed seed maker not recognizing crops added after the initial `Data/Crops` load.

## 1.17.2
Released 08 August 2020 for SMAPI 3.6.0 or later.

* Fixed bushes harvested out of season in some cases.
* Fixed berry bushes not applying forage level bonus.

## 1.17.1
Released 02 August 2020 for SMAPI 3.6.0 or later.

* Fixed string sorting/comparison for some special characters.

## 1.17.0
Released 02 May 2020 for SMAPI 3.5.0 or later.

* Added machine type priority.
* Added support for berry bushes.
* Shipping bins now have a lower priority than other machines by default.
* Optimized machine scan when a location changes.
* Internal change to let certain mods patch Automate behaviour more easily.
* Fixed broken custom items in chests causing machines to stop working and spam errors. Automate now ignores broken items.
* Fixed bushes in garden pots not automated.
* Fixed furnaces not recognizing bouquets.
* Fixed automated trash cans producing different output than they would if checked manually in some cases.
* Fixed giftbox being automated as a chest.
* Fixed search error if you have broken XNB mods.
* Moved `data.json` into standard `assets` subfolder.

## 1.16.0
Released 08 March 2020 for SMAPI 3.3.0 or later.

* Added support for tea bushes (thanks to stellarashes!).
* Added support for multi-key bindings (like `LeftShift + U`).
* Added compatibility with Mega Storage (thanks to ImJustMatt!).

## 1.15.1
Released 02 February 2020 for SMAPI 3.3.0 or later.

* Chest options set through Chests Anywhere are now applied immediately (if SMAPI 3.3 is installed).
* Fixed reversed options set through Chests Anywhere in the last update.
* Internal refactoring.

**Breaking change:** if you already installed the previous update _and_ changed Automate options through Chests Anywhere after updating, this may reverse the ones you changed. If you didn't change any options after updating, your options will be back to normal.

## 1.15.0
Released 01 February 2020 for SMAPI 3.2.0 or later.

* Added 'take items from this chest first' option editable through Chests Anywhere (thanks to MadaraUchiha!).
* Improved error handling when a machine/chest has invalid items.
* Machines with invalid output are now paused for 30 seconds.
* Fixed infinite garbage hats bug.
* Fixed some machines allowing wrong item types as input.
* Fixed buildings automated while still under construction.
* Internal refactoring.

## 1.14.2
Released 27 December 2019 for SMAPI 3.0.0 or later.

* Updated trash can logic for Stardew Valley 1.4.

## 1.14.1
Released 15 December 2019 for SMAPI 3.0.0 or later.

* Added rice to automated mills.
* Workbenches are now treated as connectors by default (configurable). This doesn't affect players who already have a `config.json`.
* Fixed brick floors not usable as connectors.
* Fixed wood chippers not interactable after automation.
* Fixed automated kegs using roe.

## 1.14.0
Released 26 November 2019 for SMAPI 3.0.0 or later.

* Updated for Stardew Valley 1.4, including...
  * new machines (fish pond and wood chipper);
  * new recipes (aged roe, caviar, dinosaur mayonnaise, and green tea);
  * chance of double Loom output with higher quality input;
  * gems are no longer pulled from Junimo huts by default (configurable).
* Fixed auto-grabbers not showing empty sprite when emptied automatically.

## 1.13.2
Released 25 July 2019 for SMAPI 2.11.2 or later.

* Reduced performance impact when a location's objects or terrain features change.
* Improved error if `data.json` file is missing or invalid.
* Fixed machines not updated when an automatable building is added or removed.
* Fixed negative 'times crafted' value shown for some uncraftable items.

## 1.13.1
Released 12 June 2019 for SMAPI 2.11.1 or later.

* Fixed stepping stone path not being usable as a connector.

## 1.13.0
Released 09 June 2019 for SMAPI 2.11.1 or later.

* Added compatibility with Auto-Grabber Mod and Better Junimos. If installed, seeds/fertilizer in their machines will be ignored (configurable).
* Simplified configuring connectors.
* Fixed config parsing errors for some players.
* Fixed items with different item qualities being combined into one input stack.

**Breaking change:** connectors previously enabled in `config.json` will be removed next time you
launch the game; See [the readme](https://github.com/Pathoschild/StardewMods/tree/develop/Automate#connectors)
for the new format, or [download the example config file on Nexus](https://www.nexusmods.com/stardewvalley/mods/1063/?tab=files).

## 1.12.0
Released 06 April 2019 for SMAPI 2.11.0 or later.

* Significantly improved performance for large machine groups.
* Added support for disabling input/output for a chest (thanks to kice!).
* Machines can now output and input in the same automation cycle.
* ~~Added compatibility with Auto-Grabber Mod and Better Junimos.~~
* Fixed "don't use this chest for automation" option not taking effect immediately.

## 1.11.1
Released 04 January 2019 for SMAPI 2.10.1 or later.

* Improved Automate API to simplify custom machines.
* Fixed error when the game has invalid fish data.
* Fixed tree tapper automation issue in Automate 1.11.
* Fixed gates not working as connectors.

## 1.11.0
Released 08 December 2018 for SMAPI 2.9.0 or later.

* Added API to support custom machines, containers, and connectors.
* Updated for the upcoming SMAPI 3.0.
* Fixed fences not working as connectors.
* Fixed floor connectors not working if an object is placed over them.

## 1.10.6
Released 08 November 2018 for SMAPI 2.8.0 or later.

* Migrated verbose logs to SMAPI's verbose logging feature.

## 1.10.5
Released 13 October 2018 for SMAPI 2.8.0-beta.5 or later.

* Updated for SMAPI 2.8-beta.5.
* Fixed error if a mill contains an unrecognized item.

## 1.10.4
Released 23 September 2018 for SMAPI 2.8.0-beta or later.

* Added option to disable shipping bin automation.
* Fixed compatibility issue with More Buildings mod.

## 1.10.2
Released 26 August 2018 for SMAPI 2.8.0-beta or later.

* Updated for Stardew Valley 1.3.29.

## 1.10.1
Released 01 August 2018 for SMAPI 2.6.0 or later.

* Fixed error with some machines if they have null output slots.

## 1.10.0
Released 01 August 2018 for SMAPI 2.6.0 or later.

* Updated for Stardew Valley 1.3, including...
  * multiplayer support;
  * support for auto-grabbers;
  * support for buildable shipping bins;
  * new fire quartz in furnace recipe.
* Added optional connectors (e.g. connect machines using paths).
* Added support for ignoring specific chests.
* Fixed various bugs related to multi-tile machines (e.g. buildings).

## 1.9.1
Released 14 February 2018 for SMAPI 2.4.0 or later.

* Updated to SMAPI 2.4.
* Fixed bee houses in custom locations not using nearby flowers.
* Fixed Jodi's trash can not being automated.
* Fixed crab pots not updating sprite when baited automatically.

## 1.9.0
Released 02 January 2018 for SMAPI 2.3.0 or later.

* Updated to SMAPI 2.3.
* Added a predictable order for chests receiving machine output. <small>(Items are now pushed into chests with `output` in the name first, then chests that already have that item type, then any other connected chest.)</small>
* Fixed chests with certain names being treated as machines.
* Fixed large machines not connecting to adjacent machines/chests in some cases.
* Fixed some item prefixes disappearing when not playing in English (e.g. blueberry wine → wine).

## 1.8.0
Released 03 December 2017 for SMAPI 2.1.0 or later.

* Updated to SMAPI 2.1.
* Added machine chaining. <small>(Chests now automate machines which are connected indirectly through other machines.)</small>
* Added chest pooling. <small>(When multiple chests are connected to the same machines, they'll be combined into a single inventory.)</small>
* Added overlay to visualise machine connections.
* Fixed mushroom box not changing sprite when emptied.
* Switched to SMAPI update checks.

## 1.7.0
Released 04 September 2017 for SMAPI 2.0.0 or later.

* Added support for egg incubators and slime incubators.
* Fixed machines inside buildings not being automated until you visit the building.
* Fixed fruit tree automation never producing better than silver quality.
* Fixed machines in a custom location that gets removed not being unloaded.

## 1.6.0
Released 18 August 2017 for SMAPI 1.15.0 or later.

* Rewrote machines so they process items in the order they're found.
* Improved performance for players with a large number of machines.
* Added `AutomationInterval` option to configure how often machines are automated.
* Added `VerboseLogging` option to enable more detailed log info.
* Fixed rare error when an item was recently removed from a chest.

## 1.5.1
Released 06 August 2017 for SMAPI 1.15.0 or later.

* Fixed shipping bin linking with chests that don't touch it on the right.

## 1.5.0
Released 13 June 2017 for SMAPI 1.15.0 or later.

* Added support for the shipping bin and trash cans.

## 1.4.0
Released 04 June 2017 for SMAPI 1.14.0 or later.

* Updated to SMAPI 1.14.
* Machines are now automated once per second, instead of once per in-game clock change.

## 1.3.0
Released 24 April 2017 for SMAPI 1.10.0 or later.

* Updated for Stardew Valley 1.2.

## 1.2.1
Released 22 April 2017 for SMAPI 1.9.0 or later.

* Fixed error when automating loom.

## 1.2.0
Released 17 April 2017 for SMAPI 1.9.0 or later.

* Added support for hay hoppers and silos.
* Added internal framework for transport pipes.
* Fixed mills not accepting input if all their slots are taken, even if some slots aren't full.
* Fixed seedmaker failing when another mod adds multiple seeds which produce the same crop.

## 1.1.0
Released 06 April 2017 for SMAPI 1.9.0 or later.


* Fixed worm bins not resetting correctly.

## 1.0.0
Released 05 April 2017 for SMAPI 1.9.0 or later.

* Initial version.
* Added support for bee houses, casks, charcoal kilns, cheese presses, crab pots, crystalariums,
  fruit trees, furnaces, Junimo huts, kegs, lighting rods, looms, mayonnaise machines, mills,
  mushroom boxes, oil makers, preserves jars, recycling machines, seed makers, slime egg-presses,
  soda machines, statues of endless fortune, statues of perfection, tappers, and worm bins.
