**Automate** is a [Stardew Valley](http://stardewvalley.net/) mod which lets you place a chest
next to a machine (like a furnace, cheese press, bee house, etc), and the machine will
automatically pull raw items from the chest and push processed items into it.

## Contents
* [Installation](#installation)
* [Usage](#usage)
* [Configuration](#configuration)
* [Versions](#versions)
* [See also](#see-also)

## Installation
1. [Install the latest version of SMAPI](https://github.com/Pathoschild/SMAPI/releases).
3. Install [this mod from Nexus mods](http://www.nexusmods.com/stardewvalley/mods/1063).
4. Run the game using SMAPI.

## Usage
### Basic automation
Once installed, crafting machines adjacent to chests will push their output into the chests, and
pull ingredients to process out of them. This can be used to automate...
* [bee houses](http://stardewvalleywiki.com/Bee_House);
* [casks](http://stardewvalleywiki.com/Cask) (including outside the cellar);
* [charcoal kilns](http://stardewvalleywiki.com/Charcoal_Kiln);
* [cheese presses](http://stardewvalleywiki.com/Cheese_Press);
* [crab pots](http://stardewvalleywiki.com/Crab_Pot);
* [crystalariums](http://stardewvalleywiki.com/Crystalarium);
* [fruit trees](http://stardewvalleywiki.com/Fruit_Trees);
* [furnaces](http://stardewvalleywiki.com/Furnace);
* [hay hoppers](http://stardewvalleywiki.com/Hay_Hopper);
* [Junimo huts](http://stardewvalleywiki.com/Junimo_Hut);
* [garbage cans](http://stardewvalleywiki.com/Garbage_Can);
* [incubators (for eggs)](https://stardewvalleywiki.com/Incubator);
* [kegs](http://stardewvalleywiki.com/Keg);
* [lightning rods](http://stardewvalleywiki.com/Lightning_Rod);
* [looms](http://stardewvalleywiki.com/Loom);
* [mayonnaise machines](http://stardewvalleywiki.com/Mayonnaise_Machine);
* [mills](http://stardewvalleywiki.com/Mill);
* [mushroom boxes](http://stardewvalleywiki.com/The_Cave#Mushrooms);
* [oil makers](http://stardewvalleywiki.com/Oil_Maker);
* [preserves jars](http://stardewvalleywiki.com/Preserves_Jar);
* [recycling machines](http://stardewvalleywiki.com/Recycling_Machine);
* [seed makers](http://stardewvalleywiki.com/Seed_Maker);
* shipping bin;
* [silos](http://stardewvalleywiki.com/Silo);
* [slime egg-presses](http://stardewvalleywiki.com/Slime_Egg);
* [slime incubators](https://stardewvalleywiki.com/Slime_Incubator);
* [tappers](http://stardewvalleywiki.com/Tapper);
* and [worm bins](http://stardewvalleywiki.com/Worm_Bin).

### Factories
You can combine multiple machines with chests. Each chest can be connected to a maximum of eight
machines (one in each direction), or seven machines so you can reach the chest.

Here are a few examples:

* **Automatic crab pots**  
  A worm bin produces bait, which is fed into the crab pot, which harvests fish into the chest. Any
  trash is automatically recycled. The final products are stored in the chest.
  ```
           recycling
            machine
               ⇅
   worm bin → chest ⇄ crab pot
  ```

* **Automatic refined quartz factory**  
  A crystalarium produces quartz every seven hours, which is smelted into refined quartz, which is
  stored in the chest.
  ```
  crystalarium (quartz) → chest (containing coal) ⇄ furnace
  ```

* **Automatic iridium mead factory**  
  A bee house produces honey every 4 days, which is turned into mead, which is aged to iridium
  quality, which is stored in the chest. You can link up to five casks to the same chest to
  increase production.
  ```
              cask
               ⇅
  bee hive → chest ⇄ keg
  ```

* **Automatic iridium bar factory**  
  A statue of perfection produces iridium ore every day, which is smelted into bars, which are
  stored in the chest.
  ```
   statue of perfection → chest (containing coal) ⇄ furnace
  ```

* **Semi-automatic iridium cheese factory**  
  Put your milk into the chest. The milk is turned into cheese, which is aged to iridium quality,
  which is stored in the chest. You can link up to six casks to the same chest to increase
  production.
  ```
  cheese press ⇄ chest (containing milk) ⇄ cask
  ```

## Configuration
The mod will work fine out of the box, but you can tweak its settings by editing the `config.json`
file if you want. These are the available settings:

setting           | what it affects
----------------- | -------------------
`AutomationInterval` | Default `60`. The number of update ticks between each automation cycle (one second is ≈60 ticks).
`CheckForUpdates` | Default `true`. Whether the mod should check for a newer version when you load the game. If a new version is available, you'll see a small message at the bottom of the screen for a few seconds. This doesn't affect the load time even if your connection is offline or slow, because it happens in the background.
`VerboseLogging` | Default `false`. Whether to write more detailed information about what the mod is doing to the log file. This is useful for troubleshooting, but may impact performance and should generally be disabled.

## Versions
See [release notes](release-notes.md).

## See also
* [Nexus mod](http://www.nexusmods.com/stardewvalley/mods/1063)
* [Discussion thread](http://community.playstarbound.com/threads/automate.131913)
