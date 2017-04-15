**Fast Animations** is a [Stardew Valley](http://stardewvalley.net/) mod that lets you selectively
speed up many animations (currently eating, drinking, milking, shearing, and breaking geodes).

Compatible with Stardew Valley 1.1+ on Linux, Mac, and Windows.

## Contents
* [Install](#install)
* [Configure](#configure)
* [See also](#see-also)

## Install
1. [Install the latest version of SMAPI](https://github.com/Pathoschild/SMAPI/releases).
2. [Install this mod from Nexus mods](http://www.nexusmods.com/stardewvalley/mods/1089/).
3. Run the game using SMAPI.

## Configure
The mod will work fine out of the box, but you can tweak its settings by editing the `config.json`
file if you want.

You can choose how fast each animation runs. Each value is a multiple of the original speed (e.g. `1` for
normal speed or `2` for double speed):

setting           | what it affects
:---------------- | :------------------
`BreakGeodeSpeed` | Default 20× speed. How fast the blacksmiths breaks geodes for you.
`EatAndDrinkSpeed` | Default 10× speed. How fast you eat and drink.
`MilkSpeed` | Default 5× speed. How fast you use the milk pail.
`ShearSpeed` | Default 5× speed. How fast you use the shears.

Other available settings:

setting           | what it affects
:---------------- | :------------------
`CheckForUpdates` | Default `true`. Whether the mod should check for a newer version when you load the game. If a new version is available, you'll see a message in the SMAPI console. This doesn't affect the load time even if your connection is offline or slow, because it happens in the background.

## Versions
See [release notes](release-notes.md).

## See also
* [Nexus mod](http://www.nexusmods.com/stardewvalley/mods/1089/)
* [Discussion thread](http://community.playstarbound.com/threads/smapi-fast-animations.132074/)
