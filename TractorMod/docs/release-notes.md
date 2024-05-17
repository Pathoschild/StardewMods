[← back to readme](README.md)

# Release notes
## Upcoming release
* Simplified tractor names to improve display in mods like NPC Map Locations.
* Improved translations. Thanks to burunduk (updated Ukrainian), Lexith98 (updated Turkish), mc-kaishixiaxue (updated Chinese), and mitekano23 (updated Japanese)!

**Note for mod authors:**
* Tractor Mod now sets tractor names to `Tractor` instead of `tractor/<horse id>`, for better compatibility with mods
  like NPC Map Locations. If you use the name to identify tractors, that was deprecated in 4.12.2 (January 2021); you
  can check for a `Pathoschild.TractorMod` entry in the horse's `modData` field instead.

## 4.17.3
Released 04 April 2024 for SMAPI 4.0.0 or later.

* Fixed tractor being shown in the animal social menu. (This will take effect in Stardew Valley 1.6.4.)
* Fixed fertilizer able to apply to grown crops using the tractor (thanks to foxwhite25!).
* Improved compatibility with mods which change fertilizer logic (thanks to foxwhite25!).
* Improved translations. Thanks to Scomar82 (updated German), JhonatanMedeiros (updated Portuguese), and Shi974 (updated French)!

## 4.17.2
Released 23 March 2024 for SMAPI 4.0.0 or later.

* Fixed 'no sounds' option not working.
* Fixed grass not dropping hay when scythed by the tractor.
* Fixed player visually stooping from tractor to grab crops.
* Fixed tractor sounds not always updated if you change the mid-game.
* Fixed tractors/garages built before Stardew Valley 1.6 turning into horses/stables.
* Fixed engine stop sound sometimes played when exiting to title or loading a save even if you weren't on the tractor.

**Migration note for players:**
* If you already saved after the tractor turned into a horse, unfortunately you'll need to rebuild the garage. You can
  make the new garage free by opening the mod's `config.json` file in a text editor and replacing these fields with:
  ```json
    "BuildPrice": 0,
    "BuildMaterials": { },
  ```

## 4.17.1
Released 20 March 2024 for SMAPI 4.0.0 or later.

* Fixed error collecting forage with the tractor.
* Fixed unable to break large stumps or boulders with the tractor.
* Fixed tractor sounds continuing if you exit to title while riding the tractor.

## 4.17.0
Released 19 March 2024 for SMAPI 4.0.0 or later.

* Updated for Stardew Valley 1.6.
* Added tractor sounds.  
  _You can disable them through Generic Mod Config Menu or by editing the `config.json` file if desired._
* Added custom buff icon + name.
* Improved translations. Thanks to EmWhyKay (updated Turkish) and MakinDay (updated Italian)!
* Fixed errors if some config fields are set to null.

## 4.16.6
Released 01 December 2023 for SMAPI 3.14.0 or later.

* Improved translations. Thanks to angel4killer (added Russian)!

## 4.16.5
Released 25 June 2023 for SMAPI 3.14.0 or later.

* You can now set the distance to zero in Generic Mod Config Menu (i.e. only under the tractor).
* Embedded `.pdb` data into the DLL, which fixes error line numbers in Linux/macOS logs.
* Improved translations. Thanks to kellykiller0816 (updated German)!

## 4.16.4
Released 30 March 2023 for SMAPI 3.14.0 or later.

* Improved translations. Thanks to martin66789 (updated Hungarian)!

## 4.16.3
Released 09 January 2023 for SMAPI 3.14.0 or later.

* Improved translations. Thanks to EngurRuzgar (updated Turkish), Mysti57155 (updated French), and wally232 (updated Korean)!

## 4.16.2
Released 30 October 2022 for SMAPI 3.14.0 or later.

* Updated integration with Generic Mod Config Menu.

## 4.16.1
Released 10 October 2022 for SMAPI 3.14.0 or later.

* Improved translations. Thanks to Becks723 (updated Chinese), ChulkyBow (updated Ukrainian), MakinDay (updated Italian), and watchakorn-18k (updated Thai)!

## 4.16.0
Released 29 August 2022 for SMAPI 3.14.0 or later.

* Added option to chop tree stumps with the axe (enabled by default) separately from chopping grown trees.

## 4.15.6
Released 18 August 2022 for SMAPI 3.14.0 or later.

* Improved translations. Thanks to LeecanIt (updated Italian)!

## 4.15.5
Released 04 July 2022 for SMAPI 3.14.0 or later.

* Added compatibility warning when Harvest With Scythe is installed.
* Fixed error when using a tool on a custom terrain feature from Farm Type Manager if you haven't used the tool manually yet.

## 4.15.4
Released 17 June 2022 for SMAPI 3.14.0 or later.

* Improved translations. Thanks to mukers (updated Russian)!

## 4.15.3
Released 05 June 2022 for SMAPI 3.14.0 or later.

* Fixed hoe not digging artifact spots if you disable all the other hoe features.
* Improved translations. Thanks to mukers (updated Russian)!

## 4.15.2
Released 22 May 2022 for SMAPI 3.14.0 or later.

* Internal changes to support Content Patcher.

## 4.15.1
Released 09 May 2022 for SMAPI 3.14.0 or later.

* Updated for SMAPI 3.14.0.
* Fixed pickaxe breaking objects that are placed on dirt, even if breaking objects is disabled.
* Improved translations. Thanks to Becks723 (updated Chinese)!

## 4.15.0
Released 27 February 2022 for SMAPI 3.13.0 or later.

* [Content packs can now edit the Tractor Mod textures](README.md#custom-textures) (e.g. for recolors or tractor texture packs).
* Improved translations. Thanks to EmWhyKay (updated Turkish) and martin66789 (added Hungarian)!

## 4.14.10
Released 14 January 2022 for SMAPI 3.13.0 or later.

* Fixed [enrichers](https://stardewvalleywiki.com/Enricher) not affecting seeds planted with the tractor.
* Fixed support for custom locations with farm animals.
* Improved translations. Thanks to ChulkyBow (added Ukrainian), Evexyron + Yllelder (updated Spanish), and Zangorr (added Polish)!

## 4.14.9
Released 25 December 2021 for SMAPI 3.13.0 or later.

* Fixed load error in the previous update.

## 4.14.8
Released 25 December 2021 for SMAPI 3.13.0 or later.

* Fixed minimum supported Generic Mod Config Menu version.
* Internal optimizations.

## 4.14.7
Released 30 November 2021 for SMAPI 3.13.0 or later.

* Updated for Stardew Valley 1.5.5 and SMAPI 3.13.0.

## 4.14.6
Released 12 November 2021 for SMAPI 3.12.6 or later.

* Improved translations. Thanks to wally232 (updated Korean)!

## 4.14.5
Released 31 October 2021 for SMAPI 3.12.5 or later.

* Improved integration with [Generic Mod Config Menu](https://www.nexusmods.com/stardewvalley/mods/5098):
  * All config options are now translatable.
  * Updated for Generic Mod Config Menu 1.5.0.

## 4.14.4
Released 18 September 2021 for SMAPI 3.12.5 or later.

* Improved translations. Thanks to ellipszist (added Thai)!  
  _Note: Thai requires Stardew Valley 1.5.5 and the [Thai mod](https://www.nexusmods.com/stardewvalley/mods/7052)._

## 4.14.3
Released 09 July 2021 for SMAPI 3.9.5 or later.

* Fixed heavy tappers not detected consistently due to a game bug.
* Fixed tractor watering can not playing any sound.
* Fixed crash if mod is installed incorrectly and its textures files don't exist.

## 4.14.2
Released 25 May 2021 for SMAPI 3.9.5 or later.

* Fixed [horse flute](https://stardewvalleywiki.com/Horse_Flute) summoning the tractor if you built
  a tractor garage before the stable.

**How to fix affected saves:**  
The fix isn't retroactive; here's how to fix an existing save if your horse flute summons the
tractor. In multiplayer, this must be done by the main player.

1. _Install [Horse Flute Anywhere](https://www.nexusmods.com/stardewvalley/mods/7500) 1.1.5 or later._
2. _Load the save in-game._
3. _Run this command in the SMAPI console window:_
   ```
   reset_horses
   ```
4. _That will reset the ownership for **all** horses in the game. Each player should then interact
   with their horse to name it and take ownership._
5. _After saving in-game, you can safely remove Horse Flute Anywhere if you don't need it._

## 4.14.1
Released 17 April 2021 for SMAPI 3.9.5 or later.

* Improved translations. Thanks to J3yEreN (updated Turkish!).

## 4.14.0
Released 27 March 2021 for SMAPI 3.9.5 or later.

* The dagger/sword can now harvest grass (disabled by default).
* Split the `MeleeWeapon` attachment into `MeleeBlunt`, `MeleeDagger`, and `MeleeSword`. If you changed melee weapon options, you'll need to reconfigure them.
* Holding a non-golden scythe to harvest grass now applies the golden scythe bonus if you've found it.
* When using [Generic Mod Config Menu](https://www.nexusmods.com/stardewvalley/mods/5098), you can now...
  * configure the mod after loading the save file;
  * set [multi-key bindings](https://stardewvalleywiki.com/Modding:Player_Guide/Key_Bindings).
* Fixed the five [golden walnuts](https://stardewvalleywiki.com/Golden_Walnut) from island crops not dropped when harvested by the tractor.
* Fixed compatibility with [unofficial 64-bit mode](https://stardewvalleywiki.com/Modding:Migrate_to_64-bit_on_Windows).

## 4.13.3
Released 06 February 2021 for SMAPI 3.9.0 or later.

* Fixed support for Seed Bag mod.

## 4.13.2
Released 23 January 2021 for SMAPI 3.9.0 or later.

* Updated for multi-key bindings in SMAPI 3.9.

## 4.13.1
Released 17 January 2021 for SMAPI 3.8.0 or later.

* Fixed issues when farmhands ride tractors in split-screen mode.

## 4.13.0
Released 16 January 2021 for SMAPI 3.8.0 or later.

* The watering can now cools lava in the volcano dungeon.
* Fixed tractor affecting other players' tools in split-screen mode.

## 4.12.2
Released 04 January 2021 for SMAPI 3.8.0 or later.

* Updated to use `modData` field in Stardew Valley 1.5.
* Internal refactoring to legacy data migrations.
* Fixed tractor not summonable if it's currently in the volcano dungeon.

**Note for mod authors:**
* Tractor Mod previously identified horses by setting their name to `tractor/<horse id>`. It still
  sets the name, but it now identifies horses by adding a `Pathoschild.TractorMod` entry to the new
  `horse.modData` field instead. Existing tractors will be migrated automatically.

## 4.12.1
Released 21 December 2020 for SMAPI 3.8.0 or later.

* Updated for Stardew Valley 1.5, including split-screen mode and UI scaling.

## 4.12.0
Released 05 December 2020 for SMAPI 3.7.3 or later.

* The pickaxe now collects spawned mine items like quartz.
* The scythe now harvests palm tree coconuts and tree seeds.
* Fixed scythe harvesting spring onions based on 'harvest crops' option instead of 'harvest forage'.

## 4.11.3
Released 15 October 2020 for SMAPI 3.7.3 or later.

* Refactored translation handling.

## 4.11.2
Released 12 September 2020 for SMAPI 3.7.3 or later.

* Fixed error in some cases when a multiplayer farmhand warps while riding a tractor.

## 4.11.1
Released 28 August 2020 for SMAPI 3.6.0 or later.

* Fixed incorrectly mapped options in Generic Mod Config Menu.

## 4.11.0
Released 04 July 2020 for SMAPI 3.6.1 or later.

* You can now configure the mod in-game if you have [Generic Mod Config Menu](https://www.nexusmods.com/stardewvalley/mods/5098) installed (thanks to collaboration with NexusFlight!).
* Added compatibility with boulders and stumps added by Farm Type Manager.
* Fixed warp loop when riding the tractor onto the Witch's Hut warp runes.
* Fixed broken event when riding the tractor into the event for the Cryptic Note quest.

## 4.10.1
Released 02 May 2020 for SMAPI 3.5.0 or later.

* Improved translations. Thanks to D0n-A (updated Russian)!

## 4.10.0
Released 09 March 2020 for SMAPI 3.3.0 or later.

* Added support for multi-key bindings (like `LeftShift + BackSpace`).
* Fixed slingshot enabled by default.
* Fixed scythe not harvesting bushes in garden pots.
* Fixed tea bushes affected by `HarvestForage` option instead of `HarvestCrops`.
* Fixed compatibility with Yet Another Harvest With Scythe Mod (thanks to bcmpinc!).
* Improved translations. Thanks to therealmate (added Hungarian)!

## 4.9.2
Released 01 February 2020 for SMAPI 3.2.0 or later.

* Fixed `HarvestFlowers` scythe option not working for some custom mod flowers.
* Fixed fertilizer not working if applied to crops after planting.

## 4.9.1
Released 15 December 2019 for SMAPI 3.0.0 or later.

* Added tree fertilizer support.
* Improved translations. Thanks to LeecanIt (added Italian)!

## 4.9.0
Released 26 November 2019 for SMAPI 3.0.0 or later.

* Updated for Stardew Valley 1.4, including...
  * fertiliser can now be applied to planted crops;
  * harvesting tea bushes;
  * using the golden scythe.
* Added attachment features:
  * axe: cut giant crops; cut choppable bushes (disabled by default).
  * milk pail: collect milk from animals.
  * shears: shear wool from animals.
* Added support for summoning a temporary tractor without building a garage first (disabled by default).
* Tractor effects are now applied radially from the tractor (e.g. trees now fall away from the tractor).
* Updated for compatibility with the latest version of Pelican Fiber.
* Fixed scythe sometimes interacting with incorrect targets (e.g. shipping bin or farm animals).
* Fixed scythe showing item pickup animation when collecting forage items.
* Fixed scythe incorrectly having a 100% chance to drop hay. The probability now matches the vanilla 50% (scythe) or 75% (golden scythe).
* Fixed incorrect speed changes when a different speed buff expires while riding the tractor (via Stardew Valley 1.4 change).
* Fixed lag when using the hoe attachment while surrounded by untillable tiles.
* Fixed issue where a dismissed tractor would return to the previous garage position if the garage was moved that day.
* Fixed error when using tools as custom attachments to attack monsters.
* Improved translations. Thanks to Hesperusrus (updated Russian)!

## 4.8.4
Released 25 July 2019 for SMAPI 2.11.2 or later.

* Improved translations. Thanks to cilekli-link (added Turkish)!

## 4.8.3
Released 09 June 2019 for SMAPI 2.11.1 or later.

* Fixed config parsing errors for some players.
* Fixed tractor not working in Deep Woods after the first screen.
* Fixed issue where pressing the dismiss-tractor key while riding a horse caused the horse to disappear.

## 4.8.2
Released 07 March 2019 for SMAPI 2.11.0 or later.

* Fixed scythe no longer harvesting in 4.8.1.

## 4.8.1
Released 06 April 2019 for SMAPI 2.11.0 or later.

* Fixed chests being treated as weeds in some cases.
* Fixed scythe and some melee weapons not clearing dead crops if enabled.
* Improved translations. Thanks to Mysti57 (added French)!

## 4.8.0
Released 05 March 2019 for SMAPI 2.11.0 or later.

* Added attachment features:
  * pickaxe: break mine containers (enabled by default), break placed objects (disabled by default).
  * scythe: collect machine output (disabled by default).
* Added attachment options:
  * axe: can now configure based on tree maturity.
  * scythe: can now configure crops and flowers separately.
* Added option to configure build resources.
* Added button to send tractor back home.
* Fixed compatibility with Deep Woods.
* Fixed farmhands unable to summon a tractor if all available tractors are in non-synced locations.
* Fixed tractor unable to break the Cindersnap forest log.
* Fixed garage texture not applied in the Farm Expansion mod's carpenter menu.
* Fixed tractor range affected when you charge a tool before mounting.
* Improved translations. Thanks to S2SKY (added Korean)!

## 4.7.3
Released 04 January 2019 for SMAPI 2.10.1 or later.

* Fixed stable no longer in Robin's carpenter menu after building a tractor garage.
* Fixed fertilizer applied to existing crops.
* Fixed compatibility with Tool Geodes mod.

## 4.7.2
Released 09 December 2018 for SMAPI 2.9.0 or later.

* Fixed horses becoming tractors in some cases. (If you saved after that happened, [see this forum comment](https://community.playstarbound.com/threads/tractor-mod.136649/page-14#post-3319770)).
* Fixed tractors sometimes not in their garage after loading a save.

## 4.7.1
Released 08 December 2018 for SMAPI 2.9.0 or later.

* Fixed tractor getting lost in the mines when players cruelly abandon it.
* Fixed tractor/garage looking like a horse/stable in some cases.
* Fixed mines not spawning a ladder on infestation levels if the tractor is present.
* Fixed old tractor data not deleted after migration, which caused duplicate garages in some cases.

## 4.7.0
Released 14 November 2018 for SMAPI 2.6.0 or later.

* Added support for multiplayer mode. The mod must be installed by the host player, and installed by any farmhand that wants to use the tractor features.
* Added support for custom mod locations.
* Added support for buying multiple tractors.
* Added attachment features:
  * tools now recognize garden pots;
  * hoe now digs artifact spots;
  * scythe now harvests bush berries.
* Changed default summon key from `T` to `Backspace` for multiplayer compatibility.
* Fixed scythe, seeds, and fertilizer not ignoring tilled dirt that has an object on it (like sprinklers or scarecrows).
* Fixed scythe shaking fruit trees when they have no fruit.
* Fixed custom attachments being able to use items past their stack size.

**Update notes:**
* If Robin started building your tractor garage in an earlier version, it will be completed when you update.
* Any current tractors/garages will be migrated to a new format next time you save. If you install an older version of Tractor Mod later, those tractors/garages will turn into horses/stables.

## 4.6.0
Released 26 August 2018 for SMAPI 2.8.0 or later.

* Updated for Stardew Valley 1.3.29.
* Tractor data is now safely stored in the save file, so you no longer need to keep the `data` subfolder when updating (once your save is migrated).
* Melee weapons now break mine containers.
* Fixed 'instant build' option in CJB Cheats Menu not working with the tractor garage.
* Removed support for legacy tractor data. (If you haven't loaded the mod in the last year, you'll need to rebuild the garage.)
* Improved translations. Thanks to pomepome (added Japanese)!

**Update note:** if you built a tractor before Tractor Mod 4.0 and never played with 4.0&ndash;4.5,
the previous tractor won't be migrated. You can edit the `config.json` to make the tractor free and
rebuild it.

## 4.5.0
Released 02 August 2018 for SMAPI 2.6.0 or later.

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
Released 09 March 2018 for SMAPI 2.5.0 or later.

* Fixed error opening the game menu if the tractor hasn't been bought yet (thanks to f4iTh!).

## 4.4.0
Released 02 March 2018 for SMAPI 2.5.0 or later.

* Added support for using the tractor in the mines.
* Added options to toggle all standard features (thanks to Fox536!).
* Fixed tractor appearing in social menu.
* Fixed tractor not appearing in Farm Expansion's carpenter menu (requires Farm Expansion 3.3+).

## 4.3.0
Released 14 February 2018 for SMAPI 2.4.0 or later.

* Updated to SMAPI 2.4.
* Added support for grass starters.
* Added support for any placeable item in `config.json` (e.g. `Mega Bomb`).
* Changed default summon key to `T`.
* Fixed summon key working when a menu is open.
* Fixed seeds and fertilizer being placed under giant crops.
* Improved tractor and garage sprites. (Thanks to allanbachti!)
* Improved translations. Thanks to Husky110 (German)!

## 4.2.0
Released 03 December 2017 for SMAPI 2.1.0 or later.

* Updated to SMAPI 2.1.
* Added support for axe clearing weeds (default) and live crops (optional).
* Added support for seasonal textures.
* Added compatibility with Farm Expansion and Seed Bag.
* Added support for controller & mouse bindings.
* Switched to SMAPI update checks.
* Improved translations. Thanks to Fabilows & TaelFayre (added Portuguese)!

## 4.1.0
Released 04 September 2017 for SMAPI 1.15.0 or later.

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
* Fixed seeds, fertilizer, and speed-gro not working if you don't play in English.
* Fixed harvesting crops, breaking rocks, and foraging not providing XP.
* Fixed harvesting hay not showing hay-gained message.
* Fixed harvesting hay giving you double hay.
* Fixed scythe not harvesting wild-seed crops.
* Fixed Robin letting you build multiple tractor garages.
* Fixed only the first garage being restored when you load the save (if you manage to build multiple).
* Fixed tractor interaction grid being offset in some cases.
* Fixed visual bugs in tractor sprite.

## 4.0.0
Released 18 August 2017 for SMAPI 1.15.0 or later.

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

## 3.2.1
Released 26 April 2017 for SMAPI 1.10.0 or later [by lambui](https://github.com/lambui/StardewValleyMod_TractorMod).

* Fixed initialization errors.

## 3.2.0
Released 26 April 2017 for SMAPI 1.10.0 or later [by lambui](https://github.com/lambui/StardewValleyMod_TractorMod).

* Updated for Stardew Valley 1.2 and SMAPI 1.10.

## 3.1.1
Released 19 December 2016 for SMAPI 1.4.0 or later [by lambui](https://github.com/lambui/StardewValleyMod_TractorMod).

* Fixed being able to call while another menu is open, which caused an error.
* Fixed hang-up dialogue not being shown after closing building menu.
* Fixed tractor spawning when garage is under construction.
* Fixed tool quality turning iridium quality after using tractor.
* Fixed tractor getting stuck when harvesting hand-harvested crops.

## 3.1.0
Released 15 December 2016 for SMAPI 1.4.0 or later [by lambui](https://github.com/lambui/StardewValleyMod_TractorMod).

* The tractor is now one tile wide.
* Fixed tractor unable to spawn in garage on start of new day.

## 3.0.1
Released 14 December 2016 for SMAPI 1.4.0 or later [by lambui](https://github.com/lambui/StardewValleyMod_TractorMod).

* Fixed issue where two tractors appeared.
* Fixed error when events happen after sleep (like the fairy or witch).

## 3.0.0
Released 08 December 2016 for SMAPI 1.3.0 or later [by lambui](https://github.com/lambui/StardewValleyMod_TractorMod).

* Added ability to buy a new tractor and garage through a phone menu, with configurable phone key + tractor price.
* Fixed tractor spawning behind shipping box each morning.
* Fixed infinite spring onions when harvested in tractor mode.
* Fixed weird hoeing or watering area in tractor mode after player charges those tools up.
* Removed global tractor mode from config (now default).

## 2.1.3
Released 03 December 2016 for SMAPI 1.2.0 or later [by lambui](https://github.com/lambui/StardewValleyMod_TractorMod).

* Fixed game freezing if player uses tools in tractor mode on objects that need a higher-level tool to break.

## 2.1.2
Released 02 December 2016 for SMAPI 1.2.0 or later [by lambui](https://github.com/lambui/StardewValleyMod_TractorMod).

* Added setting to customize tool use frequency to reduce performance impact.
* Added item radius setting (for seeding and fertilizing).
* Added ability to reload configuration in-game.
* Fixed a bug that prevents game from saving.

## 2.1.1
Released 02 December 2016 for SMAPI 1.2.0 or later [by lambui](https://github.com/lambui/StardewValleyMod_TractorMod).

* Fixed issue which prevents game from saving if player left tractor outside farm.
* Fixed issue which prevents player from summoning horse if the horse is outside farm.

## 2.1.0
Released 02 December 2016 for SMAPI 1.2.0 or later [by lambui](https://github.com/lambui/StardewValleyMod_TractorMod).

* Added per-tool settings.
* Improved algorithm and reduce performance impact.
* Removed unneeded settings from config (`WTFMode`, `harvestMode`, `harvestRadius`, `minToolPower`, `mapWidth`, `mapHeight`).

## 2.0.0
Released 01 December 2016 for SMAPI 1.2.0 or later [by lambui](https://github.com/lambui/StardewValleyMod_TractorMod).

* Add tractor:
  - Now you have a tractor separate from your horse ([sprite and animation by Pewtershmitz](http://community.playstarbound.com/threads/tractor-v-1-3-horse-replacement.108604/)).
  - Tractor will return to the spot behind your shipping box each morning.
  - Riding the tractor automatically turns on tractor mode.
  - Can summon tractor with a configurable key.
* Added option to change mouse-activation hotkey (to activate tractor mode while not on tractor).
* Added key to summon horse.
* Reduced speed in tractor mode (configurable).
* Remove horse tractor mode.

## 1.3.0
Released 29 November 2016 for SMAPI 1.2.0 or later [by lambui](https://github.com/lambui/StardewValleyMod_TractorMod).

* Added global option which lets you use tractor mode everywhere (not just on the farm).
* Added ability to harvest weeds.

## 1.2.4
Released 28 November 2016  for SMAPI 1.2.0 or later[by lambui](https://github.com/lambui/StardewValleyMod_TractorMod).

* Harvesting animal produce now drops items instead of adding them directly to inventory.
* Fixed harvesting sunflower not yielding seeds.

## 1.2.3
Released 27 November 2016 for SMAPI 1.2.0 or later [by lambui](https://github.com/lambui/StardewValleyMod_TractorMod).

* Added ability to harvest grass into hay.
* Fixed young non-regrowable crops being harvestable.

## 1.2.2
Released 27 November 2016 for SMAPI 1.2.0 or later [by lambui](https://github.com/lambui/StardewValleyMod_TractorMod).

* Fixed infinite harvest when harvesting non-regrowable crops.

## 1.2.1
Released 21 November 2016 for SMAPI 1.1.1 or later [by lambui](https://github.com/lambui/StardewValleyMod_TractorMod).

* Fixed occasional crash when player runs out of fertilizer/seeds while fertilizing/planting.

## 1.2.0
Released 20 November 2016 for SMAPI 1.1.1 or later [by lambui](https://github.com/lambui/StardewValleyMod_TractorMod).

* Updated to SMAPI 1.1.1.
* Added ability to harvest crops, fruit trees, and dropped items (like truffles or eggs) when holding scythe.
* Now work in farm buildings.

## 1.1.0
Released 17 November 2016 for SMAPI 1.1.0 or later [by lambui](https://github.com/lambui/StardewValleyMod_TractorMod).

* Added horse tractor mode.
* Added WTF mode (which lets you use your pickaxe and axe with tractor mode).

## 1.0.2
Released 16 November 2016 for SMAPI 1.0.0 or later [by lambui](https://github.com/lambui/StardewValleyMod_TractorMod).

* Added activation by holding right mouse.
* Removed activation by keyboard toggle.
* Tractor mode now automatically turns itself off outside the farm.

## 1.0.0
Released 16 November 2016 for SMAPI 1.0.0 or later [by lambui](https://github.com/lambui/StardewValleyMod_TractorMod).

* Initial release.
