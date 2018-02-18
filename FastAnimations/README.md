**Fast Animations** is a [Stardew Valley](http://stardewvalley.net/) mod that lets you selectively
speed up many animations (currently eating, drinking, milking, shearing, and breaking geodes).

## Contents
* [Install](#install)
* [Configure](#configure)
* [See also](#see-also)

## Install
1. [Install the latest version of SMAPI](https://smapi.io/).
2. [Install this mod from Nexus mods](http://www.nexusmods.com/stardewvalley/mods/1089/).
3. Run the game using SMAPI.

## Configure
The mod will work fine out of the box, but you can tweak its settings by editing the `config.json`
file if you want.

You can choose how fast each animation runs. Each value is a multiple of the original speed (e.g. `1` for
normal speed or `2` for double speed):

setting              | default | what it affects
:------------------- | :------ | :------------------
`BreakGeodeSpeed`    | 20×     | How fast the blacksmiths breaks geodes for you.
`CasinoSlotsSpeed`   | 8×      | How fast the casino slots turn.
`EatAndDrinkSpeed`   | 10×     | How fast you eat and drink.
`MilkSpeed`          | 5×      | How fast you use the milk pail.
`ShearSpeed`         | 5×      | How fast you use the shears.
`FishingSpeed`       | 1×      | How fast you cast and reel when fishing (doesn't affect the minigame).<br /><small>(Suggested value: 2×.)</small>
`TreeFallingSpeed`   | 1×      | How fast trees fall after you chop them down.<br /><small>(Suggested value: 3×.)</small>

Other options:

setting              | default | what it affects
:------------------- | :------ | :------------------
`DisableEatAndDrinkConfirmation` | `false` | If `true`, the confirmation prompt before eating or drinking won't be shown.

## Versions
See [release notes](release-notes.md).

## See also
* [Nexus mod](http://www.nexusmods.com/stardewvalley/mods/1089/)
* [Discussion thread](http://community.playstarbound.com/threads/smapi-fast-animations.132074/)
