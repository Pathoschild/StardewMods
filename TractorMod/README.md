**Tractor Mod** is a [Stardew Valley](http://stardewvalley.net/) mod which lets you buy a tractor
(and tractor garage) to more efficiently till/fertilize/seed/water/harvest crops, clear rocks, etc.

Compatible with Stardew Valley 1.3+ on Linux/Mac/Windows, for single-player and multiplayer. See
[_compatibility_](#compatibility) for details.

## Contents
* [Install](#install)
* [Use](#use)
* [Configure](#configure)
* [Custom textures](#custom-textures)
* [Compatibility](#compatibility)
* [See also](#see-also)

## Install
1. [Install the latest version of SMAPI](https://smapi.io/).
2. Install [this mod from Nexus mods](http://www.nexusmods.com/stardewvalley/mods/1401).
3. Run the game using SMAPI.

## Use
Buy a tractor garage from Robin:
> ![](screenshots/buy-garage.png)

...choose where you want it built:
> ![](screenshots/build-garage.png)

...and Robin will build your garage overnight:
> ![](screenshots/final-garage.png)

Now just get on a tractor, choose a tool or seeds or fertilizer, and drive:
> ![](screenshots/tractor.png)

You can do these by default:

hold item  | default effects | optional effects (disabled by default)
---------- | --------------- | --------------------------------------
axe        | clear twigs; clear dead crops. | chop down trees; clear live crops.
fertiliser | fertilise dirt. | —
hoe        | till dirt; dig artifact spots. | —
melee weapon | clear dead crops; break mine containers. | attack monsters.
pickaxe    | break rocks; clear tilled dirt; clear dead crops. | break paths/flooring; break placed objects.
scythe     | harvest crops, flowers, bushes, fruit trees, forage; clear weeds and dead crops. | collect machine output.
seeds      | plant seeds in dirt. | —
slingshot  | — | shoot one projectile/tile/second in the aimed direction.
watering can | water crops. | —

Tractors use no stamina when using tools, and the watering can won't run out of water. They will
consume fertiliser or seeds when you sow those, though.

## Configure
The mod creates a `config.json` file in its mod folder the first time you run it. You can open that
file in a text editor to configure the mod.

These are the available settings:

setting | effect
:------ | :-----
`Controls` | The configured controller, keyboard, and mouse buttons (see [key bindings](https://stardewvalleywiki.com/Modding:Key_bindings)). You can separate multiple buttons with commas. The default keyboard bindings are `Backspace` to summon a tractor. Available inputs:<ul><li>`SummonTractor`: warp an available tractor to your position.</li><li>`DismissTractor`: return the tractor you're riding to its home.</li><li>`HoldToActivate`: if specified, the tractor will only do something while you're holding this button. If nothing is specified, tractor will work automatically.</li></ul>
`StandardAttachments` | Toggle features for all built-in attachments.
`BuildPrice` | The gold price to buy a tractor garage. Default 150,000g.
`BuildMaterials` | The materials needed to build a tractor garage (see [object IDs](https://stardewvalleywiki.com/Modding:Object_data)). Default 20 iron bars, 5 iridium bars, and 5 battery packs.

And set some advanced options:

setting | effect
:------ | :-----
`Distance` | The number of tiles on each side of the tractor to affect (in addition to the tile under it). Default 1, which is a 3 by 3 grid.
`TractorSpeed` | The speed modifier when riding a tractor. Default -2.
`MagneticRadius` | The item magnetism amount (higher values attract items from father away). Default 384.
`HighlightRadius` | Whether to highlight the tractor radius when riding one. Default `false`.
`CustomAttachments` | The custom items or tools to apply. If you specify something that's already supported (like the axe), this will override all limitations on its use. These must match the exact internal item/tool names (not the translated display names). For example: `"CustomAttachments": ["Axe"]`

## Custom textures
You can drop new PNGs into the `assets` folder to change the appearance of tractors or garages.
For a seasonal texture, just prefix the name with the season (like `spring_tractor.png`). The mod
will load the seasonal texture if present, else it'll load the default name (like `tractor.png`).

## Compatibility
Tractor Mod is compatible with Stardew Valley 1.3+ on Linux/Mac/Windows, both single-player and
multiplayer.

In multiplayer mode it must be installed by the host player, plus any farmhands who
want to use its features. Farmhands who don't have it installed won't have any issues, they just
won't see the tractor/garage textures or be able to use its features.

## See also
* [Release notes](release-notes.md)
* [Nexus mod](http://www.nexusmods.com/stardewvalley/mods/1401)
* [Discussion thread](http://community.playstarbound.com/threads/tractor-mod.136649/)
* Derived from [TractorMod](https://github.com/lambui/StardewValleyMod_TractorMod) by PhthaloBlue (@lambui), rewritten with their permission.
