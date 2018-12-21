# Release notes
## Upcoming release
* Fixed stable no longer in Robin's carpenter menu after building a tractor garage.

## 4.7.2
* Fixed horses becoming tractors in some cases. (If you saved after that happened, [see this forum comment](https://community.playstarbound.com/threads/tractor-mod.136649/page-14#post-3319770)).
* Fixed tractors sometimes not in their garage after loading a save.

## 4.7.1
* Fixed tractor getting lost in the mines when players cruelly abandon it.
* Fixed tractor/garage looking like a horse/stable in some cases.
* Fixed mines not spawning a ladder on infestation levels if the tractor is present.
* Fixed old tractor data not deleted after migration, which caused duplicate garages in some cases.

## 4.7
* Added support for multiplayer mode. The mod must be installed by the host player, and installed by any farmhand that wants to use the tractor features.
* Added support for custom mod locations.
* Added support for buying multiple tractors.
* Added attachment features:
  * tools now recognise garden pots;
  * hoe now digs artifact spots;
  * scythe now harvests bush berries.
* Changed default summon key from `T` to `Backspace` for multiplayer compatibility.
* Fixed scythe, seeds, and fertilizer not ignoring tilled dirt that has an object on it (like sprinklers or scarecrows).
* Fixed scythe shaking fruit trees when they have no fruit.
* Fixed custom attachments being able to use items past their stack size.

**Update notes:**
* If Robin started building your tractor garage in an earlier version, it will be completed when you update.
* Any current tractors/garages will be migrated to a new format next time you save. If you install an older version of Tractor Mod later, those tractors/garages will turn into horses/stables.

## 4.6
* Updated for Stardew Valley 1.3.29.
* Tractor data is now safely stored in the save file, so you no longer need to keep the `data` subfolder when updating (once your save is migrated).
* Melee weapons now break mine containers.
* Fixed 'instant build' option in CJB Cheats Menu not working with the tractor garage.
* Removed support for legacy tractor data. (If you haven't loaded the mod in the last year, you'll need to rebuild the garage.)
* Improved translations. Thanks to pomepome (added Japanese)!

**Update note:** if you built a tractor before Tractor Mod 4.0 and never played with 4.0&ndash;4.5,
the previous tractor won't be migrated. You can edit the `config.json` to make the tractor free and
rebuild it.

## 4.5
* Updated for Stardew Valley 1.3 (disabled in multiplayer mode).
* Player is now invincible while riding the tractor (configurable).
* Added more attachment features:
  * melee weapons: clear dead crops, attack monsters (disabled by default).
  * slingshot: fires one projectile/tile/second in the aimed direction (disabled by default).
  * hoe: clear weeds.
  * pickaxe: clear weeds.
* Added support for tractor hats.
* Fixed pickaxe not breaking boulders in the mines.
* Fixed summon key working while riding a horse.
* Removed experimental 'pass through trellis' option; consider using a mod like [Walk Through Trellis](https://www.nexusmods.com/stardewvalley/mods/1958) instead.
* Improved translations. Thanks to alca259 (added Spanish)!

## 4.4.1
* Fixed error opening the game menu if the tractor hasn't been bought yet (thanks to f4iTh!).

## 4.4
* Added support for using the tractor in the mines.
* Added options to toggle all standard features (thanks to Fox536!).
* Fixed tractor appearing in social menu.
* Fixed tractor not appearing in Farm Expansion's carpenter menu (requires Farm Expansion 3.3+).

## 4.3
* Updated to SMAPI 2.4.
* Added support for grass starters.
* Added support for any placeable item in `config.json` (e.g. `Mega Bomb`).
* Changed default summon key to `T`.
* Fixed summon key working when a menu is open.
* Fixed seeds and fertiliser being placed under giant crops.
* Improved tractor and garage sprites. (Thanks to allanbachti!)
* Improved translations. Thanks to Husky110 (German)!

## 4.2
* Updated to SMAPI 2.1.
* Added support for axe clearing weeds (default) and live crops (optional).
* Added support for seasonal textures.
* Added compatibility with Farm Expansion and Seed Bag.
* Added support for controller & mouse bindings.
* Switched to SMAPI update checks.
* Improved translations. Thanks to Fabilows & TaelFayre (added Portuguese)!

## 4.1
New features:
* Added an optional hold-to-activate mode (tractor won't do anything unless you hold a key).
* Added default axe support (cutting trees disabled by default).
* Added tractor buff which pulls nearby objects towards you.
* Added compatibility with...
  * CJB Cheats Menu (instant build now works);
  * Pelican Fiber (garage now appears in its shops);
  * Horse Whistle (no longer sometimes summons the tractor);
  * custom farm locations (no longer breaks saves).
* Added option to highlight tractor radius.
* Added update check.
* Added an experimental feature which lets the tractor pass through trellis crops (disabled by default).

Feature changes:
* Tractor no longer uses tools unless needed (no more spamming pickaxe sounds).
* Pickaxe no longer destroys live crops, paths, and flooring.
* Pickaxe no longer breaks boulders if your pickaxe isn't upgraded enough.
* Scythe now clears dead crops.
* Hoe now digs up artifact spots.
* Added warning when translation files are missing.
* Improved translations. Thanks to Ereb (added Russian) and yuwenlan (added Chinese)!

Fixes:
* Fixed tractor letting you phase through objects in some cases.
* Fixed game logic treating tractor as the player's horse in some cases where it shouldn't.
* Fixed seeds, fertiliser, and speed-gro not working if you don't play in English.
* Fixed harvesting crops, breaking rocks, and foraging not providing XP.
* Fixed harvesting hay not showing hay-gained message.
* Fixed harvesting hay giving you double hay.
* Fixed scythe not harvesting wild-seed crops.
* Fixed Robin letting you build multiple tractor garages.
* Fixed only the first garage being restored when you load the save (if you manage to build multiple).
* Fixed tractor interaction grid being offset in some cases.
* Fixed visual bugs in tractor sprite.

## 4.0
This is a rewrite of the original TractorMod by PhthaloBlue (@lambui).

* The tractor garage is now sold by Robin, requires some building materials, and takes a few days for her to build. The price and whether resources are needed can be changed in `config.json`.
* The tractor and garage have a new look to match the game style thanks to [@Zero-ui9](https://github.com/Zero-ui9) and [@Acerbicon](https://github.com/Acerbicon) respectively.
* Simplified the `config.json`.
* Added support for clearing dirt and breaking rocks with the pickaxe.
* Added translation support.
* Removed summon-horse key (but kept summon-tractor key).
* Removed tractor mode activated by holding mouse button (now only when riding the tractor).
* Overhauled data saving (previous data will be migrated automatically).
* Fixed hoe destroying objects.
* Fixed tractor being summonable before a tractor garage is built.
* Fixed tractor speed debuff lasting a second after you dismount tractor.
* Improved translations. Thanks to Sasara (added German)!

## 3.2.1 (lambui)
* Fixed initialisation errors.

## 3.2 (lambui)
* Updated for Stardew Valley 1.2 and SMAPI 1.10.

## 3.1.1 (lambui)
* Fixed being able to call while another menu is open, which caused an error.
* Fixed hang-up dialogue not being shown after closing building menu.
* Fixed tractor spawning when garage is under construction.
* Fixed tool quality turning iridium quality after using tractor.
* Fixed tractor getting stuck when harvesting hand-harvested crops.

## 3.1 (lambui)
* The tractor is now one tile wide.
* Fixed tractor unable to spawn in garage on start of new day.

## 3.0.1 (lambui)
* Fixed issue where two tractors appeared.
* Fixed error when events happen after sleep (like the fairy or witch).

## 3.0 (lambui)
* Added ability to buy a new tractor and garage through a phone menu, with configurable phone key + tractor price.
* Fixed tractor spawning behind shipping box each morning.
* Fixed infinite spring onions when harvested in tractor mode.
* Fixed weird hoeing or watering area in tractor mode after player charges those tools up.
* Removed global tractor mode from config (now default).

## 2.1.3 (lambui)
* Fixed game freezing if player uses tools in tractor mode on objects that need a higher-level tool to break.

## 2.1.2 (lambui)
* Added setting to customise tool use frequency to reduce performance impact.
* Added item radius setting (for seeding and fertilizing).
* Added ability to reload configuration in-game.
* Fixed a bug that prevents game from saving.

## 2.1.1 (lambui)
* Fixed issue which prevents game from saving if player left tractor outside farm.
* Fixed issue which prevents player from summoning horse if the horse is outside farm.

## 2.1 (lambui)
* Added per-tool settings.
* Improved algorithm and reduce performance impact.
* Removed unneeded settings from config (`WTFMode`, `harvestMode`, `harvestRadius`, `minToolPower`, `mapWidth`, `mapHeight`).

## 2.0 (lambui)
* Add tractor:
  - Now you have a tractor separate from your horse ([sprite and animation by Pewtershmitz](http://community.playstarbound.com/threads/tractor-v-1-3-horse-replacement.108604/)).
  - Tractor will return to the spot behind your shipping box each morning.
  - Riding the tractor automatically turns on tractor mode.
  - Can summon tractor with a configurable key.
* Added option to change mouse-activation hotkey (to activate tractor mode while not on tractor).
* Added key to summon horse.
* Reduced speed in tractor mode (configurable).
* Remove horse tractor mode.

## 1.3 (lambui)
* Added global option which lets you use tractor mode everywhere (not just on the farm).
* Added ability to harvest weeds.

## 1.2.4 (lambui)
* Harvesting animal produce now drops items instead of adding them directly to inventory.
* Fixed harvesting sunflower not yielding seeds.

## 1.2.3 (lambui)
* Added ability to harvest grass into hay.
* Fixed young non-regrowable crops being harvestable.

## 1.2.2 (lambui)
* Fixed infinite harvest when harvesting non-regrowable crops.

## 1.2.1 (lambui)
* Fixed occasional crash when player runs out of fertilizer/seeds while fertilizing/planting.

## 1.2 (lambui)
* Updated to SMAPI 1.1.1.
* Added ability to harvest crops, fruit trees, and dropped items (like truffles or eggs) when holding scythe.
* Now work in farm buildings.

## 1.1 (lambui)
* Added horse tractor mode.
* Added WTF mode (which lets you use your pickaxe and axe with tractor mode).

## 1.0.2 (lambui)
* Added activation by holding right mouse.
* Removed activation by keyboard toggle.
* Tractor mode now automatically turns itself off outside the farm.
