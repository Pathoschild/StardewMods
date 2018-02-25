**Tractor Mod** is a [Stardew Valley](http://stardewvalley.net/) mod which lets you buy a tractor
(and tractor garage) to more efficiently till/fertilize/seed/water/harvest crops, clear rocks, etc.

## Contents
* [Install](#install)
* [Use](#use)
* [Configure](#configure)
* [Custom textures](#custom-textures)
* [Versions](#versions)
* [See also](#see-also)

## Install
1. [Install the latest version of SMAPI](https://smapi.io/).
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
axe        | clear twigs; clear dead crops. | chop down trees; clear live crops.
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
`Controls` | | The configured controller, keyboard, and mouse buttons (see [key bindings](https://stardewvalleywiki.com/Modding:Key_bindings)). You can separate multiple buttons with commas. The default keyboard bindings are `T` to summon the tractor. Available inputs:<ul><li>`SummonTractor`: warp the tractor to your position.</li><li>`HoldToActivate`: if specified, the tractor will only do something while you're holding this button. If nothing is specified, tractor will work automatically.</li></ul>
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
`CustomAttachments` | _(empty)_ | The custom items or tools to apply. If you specify something that's already supported (like the axe), this will override all limitations on its use. These must match the exact internal item/tool names (not the translated display names). For example: `"CustomTools": ["Axe"]`

And change how the tractor is sold:

setting | default | effect
:------ | :------ | :-----
`BuildPrice` | 150000 | The gold price to buy a tractor garage.
`BuildUsesResources` | `true` | Whether you need to provide resources to build a tractor garage.

And set some advanced options:

setting | default | effect
:------ | :------ | :-----
`HighlightRadius` | `false` | Whether to highlight the tractor radius when riding it.
`PassThroughTrellisCrops` | `false` | Whether the tractor can pass through trellis crops like grapes. This is an experimental feature.

## Custom textures
You can drop new PNGs into the `assets` folder to change the appearance of the tractor or garage.
For a seasonal texture, just prefix the name with the season (like `spring_tractor.png`). The mod
will load the seasonal texture if present, else it'll load the default name (like `tractor.png`).

## Versions
See [release notes](release-notes.md).

## See also
* [Nexus mod](http://www.nexusmods.com/stardewvalley/mods/1401)
* [Discussion thread](http://community.playstarbound.com/threads/tractor-mod.136649/)
* Derived from [TractorMod](https://github.com/lambui/StardewValleyMod_TractorMod) by PhthaloBlue (@lambui), rewritten with their permission.
