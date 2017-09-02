**Tractor Mod** is a [Stardew Valley](http://stardewvalley.net/) mod which lets you buy a tractor
(and tractor garage) to more efficiently till/fertilize/seed/water/harvest crops, clear rocks, etc.

## Contents
* [Install](#install)
* [Use](#use)
* [Configure](#configure)
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

hold item  | default effects | optional effects (disabled by default)
---------- | --------------- | --------------------------------------
axe        | clear twigs; clear dead crops. | chop down trees.
fertiliser | fertilise dirt. | —
hoe        | till dirt. | —
pickaxe    | break rocks; clear tilled dirt; clear dead crops. | break paths/flooring.
seeds      | plant seeds in dirt. | —
scythe     | harvest crops, fruit trees, or forage; clear weeds and dead crops. | —
watering can | water crops. | —

The tractor uses no stamina when using tools, and the watering can won't run out of water. It will
consume fertiliser or seeds when you sow those, though.

## Configure
The mod creates a `config.json` file the first time you run it. You can open the file in a text
editor to configure the mod.

You can set some general options:

setting | default | effect
:------ | :------ | :-----
`TractorKey` | `B` | The button which summons the tractor to your position (see [valid keys](https://msdn.microsoft.com/en-us/library/microsoft.xna.framework.input.keys.aspx)).
`HoldToActivateKey` | (none) | If set, the tractor won't do anything unless you hold this key (see [valid keys](https://msdn.microsoft.com/en-us/library/microsoft.xna.framework.input.keys.aspx)).
`Distance` | 1 | The number of tiles on each side of the tractor to affect (in addition to the tile under it).
`TractorSpeed` | -2 | The speed modifier when riding the tractor.
`MagneticRadius` | 384 | The item magnetism amount (higher values attract items from father away).

And toggle some tool features:

setting | default | effect
:------ | :------ | :-----
`AxeCutsFruitTrees` | `false` | Whether the axe chops down fruit trees.
`AxeCutsTrees` | `false` | Whether the axe chops down non-fruit trees.
`PickaxeClearsDirt` | `true` | Whether the tractor can clear hoed dirt tiles when the pickaxe is selected.
`PickaxeBreaksFlooring` | `false` | Whether the tractor can break flooring and paths when the pickaxe is selected.
`CustomTools` | _(empty)_ | The custom tools to apply. If you specify a tool that's already supported (like the axe), this will override all limitations on its use. These must match the exact in-game tool names. For example: `"CustomTools": ["Axe"]`

And change how the tractor is sold:

setting | default | effect
:------ | :------ | :-----
`BuildPrice` | 150000 | The gold price to buy a tractor garage.
`BuildUsesResources` | `true` | Whether you need to provide resources to build a tractor garage.

And set some advanced options:

setting | default | effect
:------ | :------ | :-----
`CheckForUpdates` | `true` | Whether the mod should check for a newer version when you load the game. If a new version is available, you'll see a small message at the bottom of the screen for a few seconds. This doesn't affect the load time even if your connection is offline or slow, because it happens in the background.
`HighlightRadius` | `false` | Whether to highlight the tractor radius when riding it.

## Versions
See [release notes](release-notes.md).

## See also
* [Nexus mod](http://www.nexusmods.com/stardewvalley/mods/1401)
* [Discussion thread](http://community.playstarbound.com/threads/tractor-mod.136649/)
* Derived from [TractorMod](https://github.com/lambui/StardewValleyMod_TractorMod) by PhthaloBlue (@lambui), rewritten with their permission.
