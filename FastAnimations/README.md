**Fast Animations** is a [Stardew Valley](http://stardewvalley.net/) mod that lets you selectively
speed up many animations (currently eating, drinking, milking, shearing, and breaking geodes).

## Contents
* [Install](#install)
* [Configure](#configure)
* [Compatibility](#compatibility)
* [See also](#see-also)

## Install
1. [Install the latest version of SMAPI](https://smapi.io/).
2. [Install this mod from Nexus mods](http://www.nexusmods.com/stardewvalley/mods/1089/).
3. Run the game using SMAPI.

## Configure
The mod will work fine out of the box, but you can tweak its settings by editing the `config.json`
file if you want.

* You can choose how fast each animation runs. Each value is a multiple of the original speed (e.g. `1` for
normal speed or `2` for double speed).

  Player animations:

  setting              | default | what it affects
  :------------------- | :------ | :------------------
  `EatAndDrinkSpeed`   | 10×     | How fast you eat and drink.
  `FishingSpeed`       | 1×      | How fast you cast and reel when fishing (doesn't affect the minigame).<br /><small>(Suggested value: 2×.)</small>
  `MilkSpeed`          | 5×      | How fast you use the milk pail.
  `ShearSpeed`         | 5×      | How fast you use the shears.

  World animations:

  setting              | default | what it affects
  :------------------- | :------ | :------------------
  `BreakGeodeSpeed`    | 20×     | How fast the blacksmiths breaks geodes for you.
  `CasinoSlotsSpeed`   | 8×      | How fast the casino slots turn.
  `PamBusSpeed`        | 6×      | How fast Pam drives her bus to and from the desert.
  `TreeFallingSpeed`   | 1×      | How fast trees fall after you chop them down.<br /><small>(Suggested value: 3×.)</small>

  UI animations:

  setting              | default | what it affects
  :------------------- | :------ | :------------------
  `TitleMenuTransitionSpeed` | 10× | How fast the title menu transitions between screens.
  `LoadGameBlinkSpeed` | 2×      | How fast the blinking-slot delay happens after you click a load-save slot.

* Other options:

  setting              | default | what it affects
  :------------------- | :------ | :------------------
  `DisableEatAndDrinkConfirmation` | `false` | If `true`, the confirmation prompt before eating or drinking won't be shown.

## Compatibility
Fast Animations is compatible with Stardew Valley 1.3+ on Linux/Mac/Windows, both single-player and
multiplayer.

Multiplayer notes:
* Animations will be sped up smoothly for you, but other players may see them skip frames.
* If multiple players have it installed, some animation speeds may stack.

## See also
* [Release notes](release-notes.md)
* [Nexus mod](http://www.nexusmods.com/stardewvalley/mods/1089/)
* [Discussion thread](http://community.playstarbound.com/threads/smapi-fast-animations.132074/)
