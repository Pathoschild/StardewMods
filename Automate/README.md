**Automate** is a [Stardew Valley](http://stardewvalley.net/) mod which lets you place a chest
next to machines (like a furnace, cheese press, bee house, etc), and the machines will
automatically pull raw items from the chest and push processed items into it.

## Contents
* [Install](#install)
* [Use](#use)
* [Configure](#configure)
* [Compatibility](#compatibility)
* [FAQs](#faqs)
* [See also](#see-also)

## Install
1. [Install the latest version of SMAPI](https://smapi.io/).
3. Install [this mod from Nexus mods](http://www.nexusmods.com/stardewvalley/mods/1063).
4. Run the game using SMAPI.

## Use
### Basic automation
Place a chest next to a crafting machine (in any direction including diagonal) to connect it.
Machines connected to a chest will push their output into it, and pull ingredients to process out
of it. 

This can be used to automate...
* auto-grabbers;
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
* shipping bins;
* [silos](http://stardewvalleywiki.com/Silo);
* [slime egg-presses](http://stardewvalleywiki.com/Slime_Egg);
* [slime incubators](https://stardewvalleywiki.com/Slime_Incubator);
* [tappers](http://stardewvalleywiki.com/Tapper);
* and [worm bins](http://stardewvalleywiki.com/Worm_Bin).

Automated machines will give you the same XP, achievements, and items you'd get for using them
directly. If multiple chests are part of a group, they'll all be used in the automation. Input
will be taken from all the chests, and output will be saved to chests in this order:
1. chests marked as output chests (see _[Configure](#configure));
2. chests which already contain an item of the same type;
3. any chest.

You can combine any number of chests and machines by placing them adjacent to each other, and you
can press `U` (configurable) to highlight connected machines.

### Factories
A 'factory' is just a machine group which produces a certain output. Here are some example factories.
You can increase production by just adding more machines.

* **Automatic crab pots**  
  A worm bin produces bait, which is fed into the crab pots, which harvest fish and recycle trash.
  The final products are stored in the chest.
  > ![](screenshots/crab-pot-factory.png)

* **Automatic refined quartz factory**  
  A crystalarium produces quartz, which is smelted into refined quartz, which is stored in the
  chest.
  > ![](screenshots/refined-quartz-factory.png)

* **Automatic iridium mead factory**  
  A bee house produces honey, which is turned into mead, which is aged to iridium quality, which is
  stored in the chest.
  > ![](screenshots/iridium-mead-factory.png)

* **Automatic iridium bar factory**  
  A statue of perfection produces iridium ore, which is smelted into bars, which are stored in the
  chest.
  > ![](screenshots/iridium-bar-factory.png)

* **Semi-automatic iridium cheese factory**  
  Put your milk in the chest and it'll be turned into cheese, then aged to iridium quality, then
  put back in the chest.
  > ![](screenshots/iridium-cheese-factory.png)

### Connectors
Connectors are placed objects or flooring which connect adjacent machines together. Automate doesn't
have any connectors by default, but you can edit the `config.json` to specify what should be treated
as connectors (see _[custom connectors](#custom-connectors)_ below).

> ![](screenshots/connectors.png)

## Configure
### Overview
The mod will work fine out of the box, but you can tweak its settings by editing the `config.json`
file if you want. These are the available settings:

setting           | what it affects
----------------- | -------------------
`Controls` | The configured controller, keyboard, and mouse buttons (see [key bindings](https://stardewvalleywiki.com/Modding:Key_bindings)). You can separate multiple buttons with commas. The default value is `U` to toggle the automation overlay.
`AutomationInterval` | Default `60`. The number of update ticks between each automation cycle (one second is ≈60 ticks).
`Connectors` | Default empty. A list of world object to treat as [connectors](#connectors).
`VerboseLogging` | Default `false`. Whether to write more detailed information about what the mod is doing to the log file. This is useful for troubleshooting, but may impact performance and should generally be disabled.

### Custom connectors
[Connectors](#connectors) are placed objects or flooring which connect adjacent machines
together. Automate has no connectors by default, but you can edit the `Connectors` field in the
`config.json` file to configure any object, craftable, or floor as a connector. Each one should be
specified with a type (one of `Floor`, `BigCraftable`, or `Object`) and ID.

For example, this adds the Wood and Crystal Paths as connectors:
```js
"Connectors": [
   { "Type": "Floor", "ID": 6 },
   { "Type": "Floor", "ID": 7 }
]
```

Valid IDs:
* [list of object IDs](https://stardewvalleywiki.com/Modding:Object_data#Raw_data);
* [list of craftable IDs](https://stardewvalleywiki.com/Modding:Big_Craftables_data#Raw_data);
* floor IDs: 0 (Wood Floor), 1 (Stone Floor), 2 (Weathered Floor), 3 (Crystal Floor), 4 (Straw
  Floor), 5 (Gravel Path), 6 (Wood Path), 7 (Crystal Path), 8 (Cobblestone Path), and 9 (Stepping
  Stone Path).

### In-game settings
Installing [Chests Anywhere](https://www.nexusmods.com/stardewvalley/mods/518) too lets you set
per-chest options directly in-game:
> ![](screenshots/chests-anywhere-config.png)

This adds two options for automate:
* **Put items in this chest first:** when choosing a chest to place processed items, put them in
  this chest before any others (until it's full).
* **Don't use this chest for automation:** Automate will completely ignore the chest, so it won't
  be connected to any machines.

If you don't have Chests Anywhere installed, you can edit the chest names a different way and use
these substrings: `|automate:output|` (put items in this chest first) or `|automate:ignore|` (don't
use this chest in automation).

## Compatibility
Automate is compatible with Stardew Valley 1.3+ on Linux/Mac/Windows, both single-player and
multiplayer. In multiplayer mode, only the main player can automate machines; other players can
keep it installed and use the overlay, their mod just won't automate anything.

Automate can be used with custom machine mods, but only the standard machines will currently be
automated.

## FAQs
### What's the order of processed machines?
The order that machines are processed is essentially unpredictable for players. It depends on the
internal algorithm for finding machines, which is subject to change.

### What's the order of items taken from chests?
For each machine, the available chests are combined into one inventory (so items may be taken from
multiple chests simultaneously) and then scanned until Automate finds enough items to fill a recipe
for that machine. The order is difficult to predict with multiple chests, but fairly easy if there's
only one connected chest.

For example, let's say you only have one chest with these item slots:

1. [1 coal]
2. [3 copper ore]
3. [3 iron ore]
4. [2 copper ore]
5. [2 iron ore]

A furnace has two recipes with those ingredients: [1 coal + 5 copper ore] = copper bar, and
[1 coal + 5 iron ore] = iron bar. Automate will scan the items from left to right and top to bottom,
and collect items until it has a complete recipe. In this case, the furnace will start producing a
copper bar:

1. [❑ 1 coal + 0 copper ore] [❑ 1 coal + 0 iron ore]
2. [❑ 1 coal + 3 copper ore] [❑ 1 coal + 0 iron ore]
3. [❑ 1 coal + 3 copper ore] [❑ 1 coal + 3 iron ore]
4. [✓ 1 coal + 5 copper ore]

### Which chest will machine output go into?
The available chests are sorted by discovery order (which isn't predictable), then prioritised in
this order:

1. chests with the "Put items in this chest first" option (see _[In-game settings](#in-game-settings)_);
2. chests which already contain an item of the same type;
3. any chest.

### Is there a limit to how many machines can be connected?
Automate optimises machine connections internally, so there's no upper limit. The most I've tried is
[630 machines in one group](https://community.playstarbound.com/threads/automate.131913/page-11#post-3238142);
that didn't cause any issues, so you can just keep adding more if you want.

### What if I don't want a specific chest to be connected?
See _[In-game settings](#in-game-settings)_.

## See also
* [Release notes](release-notes.md)
* [Nexus mod](http://www.nexusmods.com/stardewvalley/mods/1063)
* [Discussion thread](http://community.playstarbound.com/threads/automate.131913)
