# Release notes
## 3.3
* The tractor garage is now sold by Robin, requires some building materials, and takes a few days for her to build. The price and whether resources are needed can be changed in `config.json`.
* The tractor garage has a new sprite that better matches the game style.
* Removed summon-horse key.
* Removed tractor mode activated by holding mouse button (now only when riding the tractor).
* Overhauled how data is saved. (Any previous data will be migrated automatically.)
* Simplified the `config.json` by replacing per-tool settings with flags and common settings.
* Can no longer summon tractor before building a tractor garage.
* Fixed tractor speed debuff lasting a second after you dismount tractor.
* Fixed hoe destroying objects.

## 3.2.1
* Fixed initialisation errors.

## 3.2
* Updated for Stardew Valley 1.2 and SMAPI 1.10.

## 3.1.1
* Fixed being able to call while another menu is open, which caused an error.
* Fixed hang-up dialogue not being shown after closing building menu.
* Fixed tractor spawning when garage is under construction.
* Fixed tool quality turning iridium quality after using tractor.
* Fixed tractor getting stuck when harvesting hand-harvested crops.

## 3.1
* The tractor is now one tile wide.
* Fixed tractor unable to spawn in garage on start of new day.

## 3.0.1
* Fixed issue where two tractors appeared.
* Fixed error when events happen after sleep (like the fairy or witch).

## 3.0
* Added ability to buy a new tractor and garage through a phone menu, with configurable phone key + tractor price.
* Fixed tractor spawning behind shipping box each morning.
* Fixed infinite spring onions when harvested in tractor mode.
* Fixed weird hoeing or watering area in tractor mode after player charges those tools up.
* Removed global tractor mode from config (now default).

## 2.1.3
* Fixed game freezing if player uses tools in tractor mode on objects that need a higher-level tool to break.

## 2.1.2
* Added setting to customise tool use frequency to reduce performance impact.
* Added item radius setting (for seeding and fertilizing).
* Added ability to reload configuration in-game.
* Fixed a bug that prevents game from saving.

## 2.1.1
* Fixed issue which prevents game from saving if player left tractor outside farm.
* Fixed issue which prevents player from summoning horse if the horse is outside farm.

## 2.1
* Added per-tool settings.
* Improved algorithm and reduce performance impact.
* Removed unneeded settings from config (`WTFMode`, `harvestMode`, `harvestRadius`, `minToolPower`, `mapWidth`, `mapHeight`).

## 2.0
* Add tractor:
  - Now you have a tractor separate from your horse ([sprite and animation by Pewtershmitz](http://community.playstarbound.com/threads/tractor-v-1-3-horse-replacement.108604/)).
  - Tractor will return to the spot behind your shipping box each morning.
  - Riding the tractor automatically turns on tractor mode.
  - Can summon tractor with a configurable key.
* Added option to change mouse-activation hotkey (to activate tractor mode while not on tractor).
* Added key to summon horse.
* Reduced speed in tractor mode (configurable).
* Remove horse tractor mode.

## 1.3
* Added global option which lets you use tractor mode everywhere (not just on the farm).
* Added ability to harvest weeds.

## 1.2.4
* Harvesting animal produce now drops items instead of adding them directly to inventory.
* Fixed harvesting sunflower not yielding seeds.

## 1.2.3
* Added ability to harvest grass into hay.
* Fixed young non-regrowable crops being harvestable.

## 1.2.2
* Fixed infinite harvest when harvesting non-regrowable crops.

## 1.2.1
* Fixed occasional crash when player runs out of fertilizer/seeds while fertilizing/planting.

## 1.2
* Updated to SMAPI 1.1.1.
* Added ability to harvest crops, fruit trees, and dropped items (like truffles or eggs) when holding scythe.
* Now work in farm buildings.
  
## 1.1
* Added horse tractor mode.
* Added WTF mode (which lets you use your pickaxe and axe with tractor mode).

## 1.0.2
* Added activation by holding right mouse.
* Removed activation by keyboard toggle.
* Tractor mode now automatically turns itself off outside the farm.
