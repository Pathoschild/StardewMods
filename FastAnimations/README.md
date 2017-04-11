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
file if you want. These are the available settings:

setting           | what it affects
:---------------- | :------------------
`InstantEatAndDrink` | Default `true`. Whether to make eating & drinking instant (including no confirmation dialogue).
`InstantBreakGeodes` | Default `true`. Whether to make the blacksmith break geodes instantly.
`InstantMilkPail` | Default `true`. Whether to make milking instant.
`InstantShears` | Default `true`. Whether to make shearing wool instant.
`CheckForUpdates` | Default `true`. Whether the mod should check for a newer version when you load the game. If a new version is available, you'll see a message in the SMAPI console. This doesn't affect the load time even if your connection is offline or slow, because it happens in the background.

## Versions
See [release notes](release-notes.md).

## See also
* <s>Nexus mod</s>
* <s>Discussion thread</s>
