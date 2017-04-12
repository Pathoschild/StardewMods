**Fast Animations** is a [Stardew Valley](http://stardewvalley.net/) mod that lets you selectively
make many animations much faster (including eating, drinking, breaking geodes, milking, etc).

Compatible with Stardew Valley 1.11+ on Linux, Mac, and Windows.

**This is a pre-release prototype.**

## Contents
* [Install](#install)
* [Use](#use)
* [Configure](#configure)
* [See also](#see-also)

## Install
1. [Install the latest version of SMAPI](https://github.com/Pathoschild/SMAPI/releases).
2. <s>Install this mod from Nexus mods</s>.
3. Run the game using SMAPI.

## Use
The mod will automatically make some animations in the game instant. You can optionally configure
which animations are instance (see next section).

## Configure
The mod will work fine out of the box, but you can tweak its settings by editing the `config.json`
file if you want.

You can multiply the speed for each animation by a given number (for example, `1` for normal speed
and `2` for double speed):

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
* <s>Nexus mod</s>
* <s>Discussion thread</s>
