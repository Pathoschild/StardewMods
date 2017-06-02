**Tractor Mor** is a [Stardew Valley](http://stardewvalley.net/) mod which lets you buy a tractor
(with accompanying tractor garage building) to quickly till, fertilize, seed, and fertilise your
crops.

Compatible with Stardew Valley 1.2.26+ on Linux, Mac, and Windows. Originally written by PhthaloBlue,
and now maintained by the community — pull requests are welcome!

## Contents
* [Install](#install)
* [Use](#use)
* [Versions](#versions)
* [See also](#see-also)

## Install
1. [Install the latest version of SMAPI](http://canimod.com/for-players/install-smapi).
2. Install <s>this mod from Nexus mods</s>.
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

## Configure
The mod creates a `config.json` file the first time you run it. You can open the file in a text
editor to configure the mod.

You can configure which tools can be used:

setting | default | effect
:------ | :------ | :-----
`ScytheHarvests` | `true` | Whether the tractor can harvest crops, fruit trees, or forage when the scythe is selected.
`HoeTillsDirt` | `true` | Whether the tractor can hoe dirt tiles when the hoe is selected.
`WateringCanWaters` | `true` | Whether the tractor can water tiles when the watering can is selected.
`PickaxeClearsDirt` | `true` | Whether the tractor can clear hoed dirt tiles when the pickaxe is selected.
`PickaxeBreaksRocks` | `true` | Whether the tractor can break rocks when the pickaxe is selected.
`CustomTools` | _(empty)_ | The custom tools to apply. These must match the exact in-game tool names.

Some general options:

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

## See also
* <s>Nexus mod</s>
* [Discussion thread](http://community.playstarbound.com/threads/gameplay-mod-tractor-mod.126955/)
* Thanks to [Pewtershmitz for the tractor sprites](http://community.playstarbound.com/threads/tractor-v-1-3-horse-replacement.108604/)!
