**Tractor Mod** is a [Stardew Valley](http://stardewvalley.net/) mod which lets you buy a tractor
(and tractor garage) to more efficiently till/fertilize/seed/water/harvest crops, clear rocks, etc.

## Contents
* [Install](#install)
* [Use](#use)
* [Versions](#versions)
* [See also](#see-also)

## Install
1. [Install the latest version of SMAPI](https://github.com/Pathoschild/SMAPI/releases).
2. Install [this mod from Nexus mods](http://www.nexusmods.com/stardewvalley/mods/1401).
3. Run the game using SMAPI.

## Use
Buy the tractor garage from Robin:
> ![](screenshots/buy-garage.png)

...choose where you want it built:
> ![](screenshots/build-garage.png)

...and Robin will build your garage overnight:
> ![](screenshots/final-garage.png)

Now just get on the tractor, choose a tool or seeds or fertilizer, and drive:
> ![](screenshots/tractor.png)

You can do these by default:

hold item  | effect
---------- | ------
axe        | clear branch debris and dead crops.
fertiliser | fertilise tilled dirt.
hoe        | till dirt.
pickaxe    | break rocks; clear tilled dirt and dead crops.
seeds      | plant seeds in tilled dirt.
scythe     | harvest crops, fruit trees, or forage; clear weeds and dead crops.
watering can | water crops.

## Configure
The mod creates a `config.json` file the first time you run it. You can open the file in a text
editor to configure the mod.

You can configure which tools can be used:

setting | default | effect
:------ | :------ | :-----
`AxeCutsBranches` | `true` | Whether the axe clears branch debris.
`AxeCutsTrees` | `false` | Whether the axe chops down non-fruit trees.
`AxeCutsFruitTrees` | `false` | Whether the axe chops down fruit trees.
`ScytheHarvests` | `true` | Whether the tractor can harvest crops, fruit trees, or forage when the scythe is selected.
`HoeTillsDirt` | `true` | Whether the tractor can hoe dirt tiles when the hoe is selected.
`WateringCanWaters` | `true` | Whether the tractor can water tiles when the watering can is selected.
`PickaxeClearsDirt` | `true` | Whether the tractor can clear hoed dirt tiles when the pickaxe is selected.
`PickaxeBreaksRocks` | `true` | Whether the tractor can break rocks when the pickaxe is selected.
`PickaxeBreaksFlooring` | `false` | Whether the tractor can break flooring and paths when the pickaxe is selected.
`CustomTools` | _(empty)_ | The custom tools to apply. These must match the exact in-game tool names.

And some general options:

setting | default | effect
:------ | :------ | :-----
`Distance` | 1 | The number of tiles on each side of the tractor to affect (in addition to the tile under it).
`TractorKey` | `B` | The button which summons the tractor to your position (see [valid keys](https://msdn.microsoft.com/en-us/library/microsoft.xna.framework.input.keys.aspx)).
`TractorSpeed` | -2 | The speed modifier when riding the tractor.

And change how the tractor is sold:

setting | default | effect
:------ | :------ | :-----
`BuildPrice` | 150000 | The gold price to buy a tractor garage.
`BuildUsesResources` | `true` | Whether you need to provide resources to build a tractor garage.

## Versions
See [release notes](release-notes.md).

## License
This mod is entirely released under the MIT license, **except** the tractor sprite which is unlicensed.

## See also
* [Nexus mod](http://www.nexusmods.com/stardewvalley/mods/1401)
* [Discussion thread](http://community.playstarbound.com/threads/tractor-mod.136649/)
* Derived from [TractorMod](https://github.com/lambui/StardewValleyMod_TractorMod) by PhthaloBlue (@lambui), rewritten with their permission.
