**Automate** is a [Stardew Valley](http://stardewvalley.net/) mod which lets you place a chest
next to machines (like a furnace, cheese press, bee house, etc), and the machines will
automatically pull raw items from the chest and push processed items into it.

## Contents
* [Install](#install)
* [Use](#use)
  * [Basic automation](#basic-automation)
  * [Factories](#factories)
  * [Connectors](#connectors)
  * [Machine priority](#machine-priority)
* [Configure](#configure)
  * [config.json](#configjson)
  * [In-game settings](#in-game-settings)
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
* [auto-grabbers](https://stardewvalleywiki.com/Auto-Grabber);
* [bee houses](http://stardewvalleywiki.com/Bee_House);
* bushes (including [blackberry](https://stardewvalleywiki.com/Blackberry), [salmonberry](https://stardewvalleywiki.com/Salmonberry), and [tea](https://stardewvalleywiki.com/Tea_Bush) bushes);
* [casks](http://stardewvalleywiki.com/Cask) (including outside the cellar);
* [charcoal kilns](http://stardewvalleywiki.com/Charcoal_Kiln);
* [cheese presses](http://stardewvalleywiki.com/Cheese_Press);
* [crab pots](http://stardewvalleywiki.com/Crab_Pot);
* [crystalariums](http://stardewvalleywiki.com/Crystalarium);
* [fish ponds](http://stardewvalleywiki.com/Fish_Pond);
* [fruit trees](http://stardewvalleywiki.com/Fruit_Trees);
* [furnaces](http://stardewvalleywiki.com/Furnace);
* [garbage cans](http://stardewvalleywiki.com/Garbage_Can);
* [hay hoppers](http://stardewvalleywiki.com/Hay_Hopper);
* [Junimo huts](http://stardewvalleywiki.com/Junimo_Hut);
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
* shipping bins (can be disabled in `config.json`);
* [silos](http://stardewvalleywiki.com/Silo);
* [slime egg-presses](http://stardewvalleywiki.com/Slime_Egg);
* [slime incubators](https://stardewvalleywiki.com/Slime_Incubator);
* [soda machines](https://stardewvalleywiki.com/Soda_Machine);
* [statues of endless fortune](https://stardewvalleywiki.com/Statue_Of_Endless_Fortune);
* [statues of perfection](https://stardewvalleywiki.com/Statue_of_Perfection);
* [tappers](http://stardewvalleywiki.com/Tapper);
* [wood chippers](http://stardewvalleywiki.com/Wood_Chipper);
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
You can optionally configure objects or paths as connectors, which link machines together. For
example, here are wooden paths used as connectors:

> ![](screenshots/connectors.png)

Workbenches are the only connectors by default. You can edit the `config.json` to add connectors
(see _[configure](#configure)_ below).

### Machine priority
<dl>
<dt>overview</dt>
<dd>

The default order that machines are processed is unpredictable and subject to change, except that
shipping bins are processed last by default.

You can change that by setting machine priority in [the `config.json`](#configure). All machines
have a default priority of 0, and higher values are processed first (for both input and output).

</dd>

<dt>example</dt>
<dd>

For example, let's say you have this machine setup and you place two tomatoes in the chest:
```
┌──────────┐┌──────────┐┌──────────┐┌──────────┐
│  chest   ││   keg    ││   keg    ││ preserves│
│          ││          ││          ││   jar    │
└──────────┘└──────────┘└──────────┘└──────────┘
┌──────────┐
│ shipping │
│   bin    │
└──────────┘
```

By default, all of the tomatoes will go into the kegs or preserves jar (since the shipping bin has
a lower priority), but you won't know which ones.

If you wanted kegs to process input before preserves jars, you'd set this in the `config.json`:
```js
"MachinePriority": {
   "Keg": 1,
   "ShippingBin": -1
}
```

Now the two tomatoes would always go into the kegs. If you put five tomatoes at once into the chest,
the kegs and preserves jar would each get one, and the remaining tomatoes would go into the
shipping bin.

</dd>

<dt>machine codes</dt>
<dd>

The `MachinePriority` option needs the unique machine code. Here are the codes for the default
machines:

<details><summary>expand</summary>

machine type | code
------------ | ----
auto-grabbers | `AutoGrabber`
bee houses | `BeeHouse`
bushes | `Bush`
casks | `Cask`
charcoal kilns | `CharcoalKiln`
cheese presses | `CheesePress`
crab pots | `CrabPot`
crystalariums | `Crystalarium`
fish ponds | `FishPond`
fruit trees | `FruitTree`
furnaces | `Furnace`
garbage cans | `TrashCan`
hay hoppers | `FeedHopper`
Junimo huts | `JunimoHut`
incubators (for eggs) | `CoopIncubator`
kegs | `Keg`
lightning rods | `LightningRod`
looms | `Loom`
mayonnaise machines | `MayonnaiseMachine`
mills | `Mill`
mushroom boxes | `MushroomBox`
oil makers | `OilMaker`
preserves jars | `PreservesJar`
recycling machines | `RecyclingMachine`
seed makers | `SeedMaker`
shipping bins | `ShippingBin`
silos | `FeedHopper` (same as hay hoppers)
slime egg-presses | `SlimeEggPress`
slime incubators | `SlimeIncubator`
soda machines | `SodaMachine`
statues of endless fortune | `StatueOfEndlessFortune`
statues of perfection | `StatueOfPerfection`
tappers | `Tapper`
wood chippers | `WoodChipper`
worm bins | `WormBin`

</details>
</dd>
</dl>

For custom machines added by other mods, see their documentation or ask their mod authors.

## Configure
### config.json
The mod creates a `config.json` file in its mod folder the first time you run it. You can open that
file in a text editor to configure the mod.

These are the available settings:

<table>
<tr>
  <th>setting</th>
  <th>what it affects</th>
</tr>
<tr>
<tr>
  <td><code>Controls</code></td>
  <td>

The configured controller, keyboard, and mouse buttons (see [key bindings](https://stardewvalleywiki.com/Modding:Key_bindings)).
The default value is `U` to toggle the automation overlay.

You can separate bindings with commas (like `U, LeftShoulder` for either one), and set multi-key
bindings with plus signs (like `LeftShift + U`).

  </td>
</tr>
<tr>
  <td><code>AutomateShippingBin</code></td>
  <td>

Whether the shipping bin should automatically pull items out of connected chests. Default `true`.

  </td>
</tr>
<tr>
  <td><code>PullGemstonesFromJunimoHuts</code></td>
  <td>

Whether to pull gemstones out of Junimo huts. If true, you won't be able to change Junimo colors by
placing gemstones in their hut. Default `false`.

  </td>
</tr>
<tr>
  <td><code>AutomationInterval</code></td>
  <td>

The number of update ticks between each automation cycle (one second is ≈60 ticks). Default `60`.

  </td>
</tr>
<tr>
  <td><code>ConnectorNames</code></td>
  <td>

A list of placed item names to treat as [connectors](#connectors) which connect adjacent machines
together. You must specify the exact _English_ names for any in-game items to use. For example:

```js
"ConnectorNames": [
   "Wood Path",
   "Crystal Path"
]
```

Contains `Workbench` by default.

  </td>
</tr>
<tr>
  <td><code>MachinePriority</code></td>
  <td>

The relative priority with which to process machine inputs and outputs; see
_[machine priority](#machine-priority)_ for more info. Defaults to `ShippingBin` at -1 priority.
  </td>
</tr>
<tr>
  <td><code>ModCompatibility</code></td>
  <td>

Enables compatibility with other mods. All values are enabled by default.

field | result
----- | ------
`AutoGrabberMod` | If [Auto-Grabber Mod](https://www.nexusmods.com/stardewvalley/mods/2783) is installed, auto-grabbers won't output fertilizer and seeds.
`BetterJunimos` | If [Better Junimos](https://www.nexusmods.com/stardewvalley/mods/2221) is installed, Junimo huts won't output fertilizer and seeds.

  </td>
</tr>
</table>

### In-game settings
Installing [Chests Anywhere](https://www.nexusmods.com/stardewvalley/mods/518) lets you set
per-chest options directly in-game:
> ![](screenshots/chests-anywhere-config.png)

This adds four options for automate:
* **Store items in this chest:** Automate will push machine output into this chest.
* **Store items in this chest first:** Automate will push machine output into this chest first, and
  only try other chests if it's full.
* **Take items from this chest:** Automate will take machine input from this chest.
* **Take items from this chest first:** Automate will take machine input from this chest first, and
  only try other chests if it doesn't have any input for a machine.

(To do this without Chests Anywhere, see the [technical documentation](technical.md).)

## Compatibility
Automate is compatible with Stardew Valley 1.4+ on Linux/Mac/Windows, both single-player and
multiplayer. In multiplayer mode, only the main player can automate machines; other players can
keep it installed and use the overlay, their mod just won't automate anything.

Automate is compatible with...

* [Auto-Grabber Mod](https://www.nexusmods.com/stardewvalley/mods/2783) (seeds/fertilizer in
  auto-grabbers will be ignored).
* [Better Junimos](https://www.nexusmods.com/stardewvalley/mods/2221) (seeds/fertilizer in Junimo
  huts will be ignored).
* [Custom Farming Redux](https://www.nexusmods.com/stardewvalley/mods/991) (see its optional
  'CFAutomate' download to enable automation).
* [Producer Framework Mod](https://www.nexusmods.com/stardewvalley/mods/4970) (with the
  [PFMAutomate](https://www.nexusmods.com/stardewvalley/mods/5038) addon).

## FAQs
### Why did my chests/machines disappear?
Some common reasons:
* NPCs destroy items placed in their path, so you shouldn't place anything where they can walk.
  (You can use [path connectors](#connectors) to connect crab pots and trash cans to out-of-the-way
  chests or machines.)
* Festivals and the Night Market use temporary maps, so items placed there may disappear when the
  map is switched back to normal.

Automate doesn't remove placed objects, so it's never at fault for disappearing chests or machines.

### Is there a limit to how many machines can be connected?
Automate optimises machine connections internally, so there's no upper limit. The most I've tried is
[630 machines in one group](https://community.playstarbound.com/threads/automate.131913/page-11#post-3238142);
that didn't cause any issues, so you can just keep adding more if you want.

### What's the order for chest/machine handling?
When storing items, Automate prefers chests which either have the "Put items in this chest first" option (see
[_in-game settings_ in the README](README.md#in-game-settings)) or already have an item of the
same type. The order when taking items is a bit more complicated. For more info, see the
[technical documentation](technical.md).

For machines, see [machine priority](#machine-priority).

### What if I don't want a specific chest to be connected?
See _[In-game settings](#in-game-settings)_.

### Can other mods extend Automate?
Yep. Automate provides APIs that let other mods add custom machines/containers/connectors or make
other changes. For more info, see the [technical documentation](technical.md).

## See also
* [Technical documentation](technical.md)
* [Release notes](release-notes.md)
* [Nexus mod](http://www.nexusmods.com/stardewvalley/mods/1063)
