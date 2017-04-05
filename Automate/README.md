**Automate** is a [Stardew Valley](http://stardewvalley.net/) mod which lets you place a chest
next to a machine (like a furnace, cheese press, bee house, etc), and the machine will
automatically pull raw items from the chest and push processed items into it.

Compatible with Stardew Valley 1.2+ on Linux, Mac, and Windows.

## Contents
* [Installation](#installation)
* [Usage](#usage)
* [Configuration](#configuration)
* [Versions](#versions)
* [See also](#see-also)

## Installation
1. [Install the latest version of SMAPI](http://canimod.com/guides/using-mods#installing-smapi).
3. Install <s>this mod from Nexus mods</s>.
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
* [slime egg-presses](http://stardewvalleywiki.com/Slime_Egg);
* [tappers](http://stardewvalleywiki.com/Tapper);
* and [worm bins](http://stardewvalleywiki.com/Worm_Bin).

### Factories
You can combine multiple machines with chests. For example:

* **Automatic crab pots**  
  A worm bin produces bait, which is fed into the crab pot, which harvests fish into the chest. Any
  trash is automatically recycled.
  ```
           recycling
            machine
               ⇅
  worm bin → chest ⇄ crab pot
  ```

* **Automatic refined quartz factory**  
  A crystalarium produces quartz every seven hours, which is smelted into refined quartz and stored
  in the chest.
  ```
  crystalarium (quartz) → chest ⇄ furnace
  ```

* **Automatic iridium mead factory**  
  A bee house produces honey every 4 days, which is turned into mead by the keg, which is then aged
  to iridium quality by a cask. You can link up to five casks to the same chest to increase
  production.
  ```
              cask
               ⇅
  bee hive → chest ⇄ keg
  ```

* **Semi-automatic iridium cheese factory**  
  Put your milk into the chest; the cheese press will turn it into cheese, and the cask will age it
  to iridium quality. You can link up to six casks to the same chest to increase production.
  ```
  cheese press ⇄ chest (containing milk) ⇄ cask
  ```

## Configuration
The mod will work fine out of the box, but you can tweak its settings by editing the `config.json`
file if you want. These are the available settings:

| setting           | what it affects
| ----------------- | -------------------
| `CheckForUpdates` | Default `true`. Whether the mod should check for a newer version when you load the game. If a new version is available, you'll see a small message at the bottom of the screen for a few seconds. This doesn't affect the load time even if your connection is offline or slow, because it happens in the background.

## Versions
See [release notes](release-notes.md).

## See also
* <s>Nexus mod</s>
* <s>Discussion thread</s>
