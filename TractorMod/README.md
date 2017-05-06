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
Buy the tractor garage from Robin (a licensed PhthaloBlue Corp distributor):
> ![](screenshots/buy-garage.png)

...choose where you want it built:
> ![](screenshots/build-garage.png)

...and PhthaloBlue Corp will build your garage overnight:
> ![](screenshots/final-garage.png)

Now just get on the tractor, choose a tool or seeds or fertilizer, and drive:
> ![](screenshots/tractor.png)

## Configure
The mod creates a `config.json` file the first time you run it. You can open the file in a text
editor to configure the mod.

setting | effect
:------ | :-----
`ToolConfig` | Configure the tools to use with the tractor (see below).
`ItemRadius` | The number of tiles on each side of the tractor to affect when seeding or fertilising (in addition to the tile under it).
`TractorKey` | The button which summons the tractor to your position (see [valid keys](https://msdn.microsoft.com/en-us/library/microsoft.xna.framework.input.keys.aspx)).
`TractorSpeed` | The speed modifier when riding the tractor.
`TractorHousePrice` | The gold price to buy a tractor garage.
`UpdateConfig` | The button which reloads the mod configuration.

The `ToolConfig` field contains multiple entries with these fields:

setting | effect
:------ | :-----
`Name` | The exact name of the tool to use (case-sensitive).
`MinLevel` | The minimum tool upgrade level to use this tool in tractor mode (0 for basic tool, 1 for copper tool, etc).
`EffectRadius` | The number of tiles on each side of the tractor to affect in addition to the tile under it (e.g. 1 for 3x3 grid or 2 for 5x5 grid).
`ActiveEveryTickAmount` | The cooldown time between each use of the tool in tractor mode, in ticks. This is mainly for performance. There are ≈60 ticks per second.

## Versions
See [release notes](release-notes.md).

## See also
* <s>Nexus mod</s>
* [Discussion thread](http://community.playstarbound.com/threads/gameplay-mod-tractor-mod.126955/)
* Thanks to [Pewtershmitz for the tractor sprites](http://community.playstarbound.com/threads/tractor-v-1-3-horse-replacement.108604/)!
