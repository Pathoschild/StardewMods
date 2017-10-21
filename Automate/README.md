**Automate** is a [Stardew Valley](http://stardewvalley.net/) mod which lets you place a chest
next to machines (like a furnace, cheese press, bee house, etc), and the machines will
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
Once installed, crafting machines connected to a chest will push their output into it, and pull
ingredients to process out of it.

This can be used to automate...
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
Each chest can be connected to any number of machines, and the machines don't all need to be of the
same type. A chest will connect to any machine that's touching it, any machines touching those
machines, etc.

Here are a few examples:

* **Automatic crab pots**  
  A worm bin produces bait, which is fed into the crab pots, which harvest fish into the chest. Any
  trash is automatically recycled. The final products are stored in the chest.
  ```
  ┌────────────┐ ┌────────────┐
  │ recycling  │ │  crab pot  │
  │  machine   │ │            │
  └────────────┘ └────────────┘
  ╔════════════╗ ┌────────────┐
  ║   chest    ║ │  crab pot  │
  ║            ║ │            │
  ╚════════════╝ └────────────┘
  ┌────────────┐ ┌────────────┐
  │  worm bin  │ │  crab pot  │
  │            │ │            │
  └────────────┘ └────────────┘
  ```

* **Automatic refined quartz factory**  
  A crystalarium produces quartz every seven hours, which is smelted into refined quartz, which is
  stored in the chest.
  ```
  ┌────────────┐ ╔════════════╗ ┌────────────┐
  │crystalarium│ ║   chest    ║ │  furnace   │
  │            │ ║ (with coal)║ │            │
  └────────────┘ ╚════════════╝ └────────────┘
  ```

* **Automatic iridium mead factory**  
  A bee house produces honey every 4 days, which is turned into mead, which is aged to iridium
  quality, which is stored in the chest. You can link up to five casks to the same chest to
  increase production.
  ```
                 ┌────────────┐
                 │    cask    │
                 │            │
                 └────────────┘
  ┌────────────┐ ╔════════════╗ ┌────────────┐
  │  bee hive  │ ║   chest    ║ │    keg     │
  │            │ ║            ║ │            │
  └────────────┘ ╚════════════╝ └────────────┘
  ```

* **Automatic iridium bar factory**  
  A statue of perfection produces iridium ore every day, which is smelted into bars, which are
  stored in the chest.
  ```
  ┌────────────┐ ╔════════════╗ ┌────────────┐
  │ statue of  │ ║   chest    ║ │  furnace   │
  │ perfection │ ║ (with coal)║ │            │
  └────────────┘ ╚════════════╝ └────────────┘
  ```

* **Semi-automatic iridium cheese factory**  
  Put your milk into the chest. The milk is turned into cheese, which is aged to iridium quality,
  which is stored in the chest. You can link up to six casks to the same chest to increase
  production.
  ```
  ┌────────────┐ ╔════════════╗ ┌────────────┐
  │   cheese   │ ║   chest    ║ │    cask    │
  │   press    │ ║ (with milk)║ │            │
  └────────────┘ ╚════════════╝ └────────────┘
  ```

## Configuration
The mod will work fine out of the box, but you can tweak its settings by editing the `config.json`
file if you want. These are the available settings:

setting           | what it affects
----------------- | -------------------
`Controls` | The configured controller, keyboard, and mouse buttons (see [key bindings](https://stardewvalleywiki.com/Modding:Key_bindings)). You can separate multiple buttons with commas. The default value is `U` to toggle the automation overlay.
`AutomationInterval` | Default `60`. The number of update ticks between each automation cycle (one second is ≈60 ticks).
`VerboseLogging` | Default `false`. Whether to write more detailed information about what the mod is doing to the log file. This is useful for troubleshooting, but may impact performance and should generally be disabled.

## Versions
See [release notes](release-notes.md).

## See also
* [Nexus mod](http://www.nexusmods.com/stardewvalley/mods/1063)
* [Discussion thread](http://community.playstarbound.com/threads/automate.131913)
