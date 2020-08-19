[← back to readme](README.md)

# Release notes
## 1.17.3
Released 19 August 2020.

* Fixed seed maker not recognizing crops added after the initial `Data/Crops` load.

## 1.17.2
Released 08 August 2020.

* Fixed bushes harvested out of season in some cases.
* Fixed berry bushes not applying forage level bonus.

## 1.17.1
Released 02 August 2020.

* Fixed string sorting/comparison for some special characters.

## 1.17
Released 02 May 2020.

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

## 1.16
Released 08 March 2020.

* Added support for tea bushes (thanks to stellarashes!).
* Added support for multi-key bindings (like `LeftShift + U`).
* Added compatibility with Mega Storage (thanks to ImJustMatt!).

## 1.15.1
Released 02 February 2020.

* Chest options set through Chests Anywhere are now applied immediately (if SMAPI 3.3 is installed).
* Fixed reversed options set through Chests Anywhere in the last update.
* Internal refactoring.

**Breaking change:** if you already installed the previous update _and_ changed Automate options through Chests Anywhere after updating, this may reverse the ones you changed. If you didn't change any options after updating, your options will be back to normal.

## 1.15
Released 01 February 2020.

* Added 'take items from this chest first' option editable through Chests Anywhere (thanks to MadaraUchiha!).
* Improved error handling when a machine/chest has invalid items.
* Machines with invalid output are now paused for 30 seconds.
* Fixed infinite garbage hats bug.
* Fixed some machines allowing wrong item types as input.
* Fixed buildings automated while still under construction.
* Internal refactoring.

## 1.14.2
Released 27 December 2019.

* Updated trash can logic for Stardew Valley 1.4.

## 1.14.1
Released 15 December 2019.

* Added rice to automated mills.
* Workbenches are now treated as connectors by default (configurable). This doesn't affect players who already have a `config.json`.
* Fixed brick floors not usable as connectors.
* Fixed wood chippers not interactable after automation.
* Fixed automated kegs using roe.

## 1.14
Released 26 November 2019.

* Updated for Stardew Valley 1.4, including...
  * new machines (fish pond and wood chipper);
  * new recipes (aged roe, caviar, dinosaur mayonnaise, and green tea);
  * chance of double Loom output with higher quality input;
  * gemstones are no longer pulled from Junimo huts by default (configurable).
* Fixed auto-grabbers not showing empty sprite when emptied automatically.

## 1.13.2
Released 25 July 2019.

* Reduced performance impact when a location's objects or terrain features change.
* Improved error if `data.json` file is missing or invalid.
* Fixed machines not updated when an automatable building is added or removed.
* Fixed negative 'times crafted' value shown for some uncraftable items.

## 1.13.1
Released 12 June 2019.

* Fixed stepping stone path not being usable as a connector.

## 1.13
Released 09 June 2019.

* Added compatibility with Auto-Grabber Mod and Better Junimos. If installed, seeds/fertilizer in their machines will be ignored (configurable).
* Simplified configuring connectors.
* Fixed config parsing errors for some players.
* Fixed items with different item qualities being combined into one input stack.

**Breaking change:** connectors previously enabled in `config.json` will be removed next time you
launch the game; See [the readme](https://github.com/Pathoschild/StardewMods/tree/develop/Automate#connectors)
for the new format, or [download the example config file on Nexus](https://www.nexusmods.com/stardewvalley/mods/1063/?tab=files).

## 1.12
Released 06 April 2019.

* Significantly improved performance for large machine groups.
* Added support for disabling input/output for a chest (thanks to kice!).
* Machines can now output and input in the same automation cycle.
* ~~Added compatibility with Auto-Grabber Mod and Better Junimos.~~
* Fixed "don't use this chest for automation" option not taking effect immediately.

## 1.11.1
Released 04 January 2019.

* Improved Automate API to simplify custom machines.
* Fixed error when the game has invalid fish data.
* Fixed tree tapper automation issue in Automate 1.11.
* Fixed gates not working as connectors.

## 1.11
Released 08 December 2018.

* Added API to support custom machines, containers, and connectors.
* Updated for the upcoming SMAPI 3.0.
* Fixed fences not working as connectors.
* Fixed floor connectors not working if an object is placed over them.

## 1.10.6
Released 08 November 2018.

* Migrated verbose logs to SMAPI's verbose logging feature.

## 1.10.5
Released 13 October 2018.

* Updated for SMAPI 2.8-beta.5.
* Fixed error if a mill contains an unrecognized item.

## 1.10.4
Released 23 September 2018.

* Added option to disable shipping bin automation.
* Fixed compatibility issue with More Buildings mod.

## 1.10.2
Released 26 August 2018.

* Updated for Stardew Valley 1.3.29.

## 1.10.1
Released 01 August 2018.

* Fixed error with some machines if they have null output slots.

## 1.10
Released 01 August 2018.

* Updated for Stardew Valley 1.3, including...
  * multiplayer support;
  * support for auto-grabbers;
  * support for buildable shipping bins;
  * new fire quartz in furnace recipe.
* Added optional connectors (e.g. connect machines using paths).
* Added support for ignoring specific chests.
* Fixed various bugs related to multi-tile machines (e.g. buildings).

## 1.9.1
Released 14 February 2018.

* Updated to SMAPI 2.4.
* Fixed bee houses in custom locations not using nearby flowers.
* Fixed Jodi's trash can not being automated.
* Fixed crab pots not updating sprite when baited automatically.

## 1.9
Released 02 January 2018.

* Updated to SMAPI 2.3.
* Added a predictable order for chests receiving machine output. <small>(Items are now pushed into chests with `output` in the name first, then chests that already have that item type, then any other connected chest.)</small>
* Fixed chests with certain names being treated as machines.
* Fixed large machines not connecting to adjacent machines/chests in some cases.
* Fixed some item prefixes disappearing when not playing in English (e.g. blueberry wine → wine).

## 1.8
Released 03 December 2017.

* Updated to SMAPI 2.1.
* Added machine chaining. <small>(Chests now automate machines which are connected indirectly through other machines.)</small>
* Added chest pooling. <small>(When multiple chests are connected to the same machines, they'll be combined into a single inventory.)</small>
* Added overlay to visualise machine connections.
* Fixed mushroom box not changing sprite when emptied.
* Switched to SMAPI update checks.

## 1.7
Released 04 September 2017.

* Added support for egg incubators and slime incubators.
* Fixed machines inside buildings not being automated until you visit the building.
* Fixed fruit tree automation never producing better than silver quality.
* Fixed machines in a custom location that gets removed not being unloaded.

## 1.6
Released 18 August 2017.

* Rewrote machines so they process items in the order they're found.
* Improved performance for players with a large number of machines.
* Added `AutomationInterval` option to configure how often machines are automated.
* Added `VerboseLogging` option to enable more detailed log info.
* Fixed rare error when an item was recently removed from a chest.

## 1.5.1
Released 06 August 2017.

* Fixed shipping bin linking with chests that don't touch it on the right.

## 1.5
Released 13 June 2017.

* Added support for the shipping bin and trash cans.

## 1.4
Released 04 June 2017.

* Updated to SMAPI 1.14.
* Machines are now automated once per second, instead of once per in-game clock change.

## 1.3
Released 24 April 2017.

* Updated for Stardew Valley 1.2.

## 1.2.1
Released 22 April 2017.

* Fixed error when automating loom.

## 1.2
Released 17 April 2017.

* Added support for hay hoppers and silos.
* Added internal framework for transport pipes.
* Fixed mills not accepting input if all their slots are taken, even if some slots aren't full.
* Fixed seedmaker failing when another mod adds multiple seeds which produce the same crop.

## 1.1
Released 06 April 2017.


* Fixed worm bins not resetting correctly.

## 1.0
Released 05 April 2017.

* Initial version.
* Added support for bee houses, casks, charcoal kilns, cheese presses, crab pots, crystalariums,
  fruit trees, furnaces, Junimo huts, kegs, lighting rods, looms, mayonnaise machines, mills,
  mushroom boxes, oil makers, preserves jars, recycling machines, seed makers, slime egg-presses,
  soda machines, statues of endless fortune, statues of perfection, tappers, and worm bins.
