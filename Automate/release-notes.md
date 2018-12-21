[← back to readme](README.md)

# Release notes
## Upcoming release
* Extended Automate API to simplify custom machines.
* Fixed error when the game has invalid fish data.
* Fixed tree tappers having the wrong automation position in Automate 1.11.
* Fixed gates not working as connectors.

## 1.11
* Added API to support custom machines, containers, and connectors.
* Updated for the upcoming SMAPI 3.0.
* Fixed fences not working as connectors.
* Fixed floor connectors not working if an object is placed over them.

## 1.10.6
* Migrated verbose logs to SMAPI's verbose logging feature.

## 1.10.5
* Updated for SMAPI 2.8-beta.5.
* Fixed error if a mill contains an unrecognised item.

## 1.10.4
* Added option to disable shipping bin automation.
* Fixed compatibility issue with More Buildings mod.

## 1.10.2
* Updated for Stardew Valley 1.3.29.

## 1.10.1
* Fixed error with some machines if they have null output slots.

## 1.10
* Updated for Stardew Valley 1.3, including...
  * multiplayer support;
  * support for auto-grabbers;
  * support for buildable shipping bins;
  * new fire quartz in furnace recipe.
* Added optional connectors (e.g. connect machines using paths).
* Added support for ignoring specific chests.
* Fixed various bugs related to multi-tile machines (e.g. buildings).
* **Breaking change:** to prefer a chest for output, add `|automate:output|` to the chest name instead of just `output`.

## 1.9.1
* Updated to SMAPI 2.4.
* Fixed bee houses in custom locations not using nearby flowers.
* Fixed Jodi's trash can not being automated.
* Fixed crab pots not updating sprite when baited automatically.

## 1.9
* Updated to SMAPI 2.3.
* Added a predictable order for chests receiving machine output. <small>(Items are now pushed into chests with `output` in the name first, then chests that already have that item type, then any other connected chest.)</small>
* Fixed chests with certain names being treated as machines.
* Fixed large machines not connecting to adjacent machines/chests in some cases.
* Fixed some item prefixes disappearing when not playing in English (e.g. blueberry wine → wine).

## 1.8
* Updated to SMAPI 2.1.
* Added machine chaining. <small>(Chests now automate machines which are connected indirectly through other machines.)</small>
* Added chest pooling. <small>(When multiple chests are connected to the same machines, they'll be combined into a single inventory.)</small>
* Added overlay to visualise machine connections.
* Fixed mushroom box not changing sprite when emptied.
* Switched to SMAPI update checks.

## 1.7
* Added support for egg incubators and slime incubators.
* Fixed machines inside buildings not being automated until you visit the building.
* Fixed fruit tree automation never producing better than silver quality.
* Fixed machines in a custom location that gets removed not being unloaded.

## 1.6
* Rewrote machines so they process items in the order they're found.
* Improved performance for players with a large number of machines.
* Added `AutomationInterval` option to configure how often machines are automated.
* Added `VerboseLogging` option to enable more detailed log info.
* Fixed rare error when an item was recently removed from a chest.

## 1.5.1
* Fixed shipping bin linking with chests that don't touch it on the right.

## 1.5
* Added support for the shipping bin and trash cans.

## 1.4
* Updated to SMAPI 1.14.
* Machines are now automated once per second, instead of once per in-game clock change.

## 1.3
* Updated for Stardew Valley 1.2.

## 1.2.1
* Fixed error when automating loom.

## 1.2
* Added support for hay hoppers and silos.
* Added internal framework for transport pipes.
* Fixed mills not accepting input if all their slots are taken, even if some slots aren't full.
* Fixed seedmaker failing when another mod adds multiple seeds which produce the same crop.

## 1.1
* Fixed worm bins not resetting correctly.

## 1.0
* Initial version.
* Added support for bee houses, casks, charcoal kilns, cheese presses, crab pots, crystalariums,
  fruit trees, furnaces, Junimo huts, kegs, lighting rods, looms, mayonnaise machines, mills,
  mushroom boxes, oil makers, preserves jars, recycling machines, seed makers, slime egg-presses,
  soda machines, statues of endless fortune, statues of perfection, tappers, and worm bins.
