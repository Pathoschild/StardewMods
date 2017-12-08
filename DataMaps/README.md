**Data Maps** is a [Stardew Valley](http://stardewvalley.net/) mod that overlays the world with
metadata maps.

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
Press `F2` by default to show the data maps overlay, and then `Tab` to cycle between data maps.

These overlays are supported:

overlay       | information shown
------------- | -----------------
Accessibility | Shows where the player can walk and warp tiles. Useful for finding hidden paths or nooks.
Junimo huts   | Shows Junimo hut coverage, and highlights crops they won't reach.
Scarecrows    | Shows scarecrow coverage, and highlights unprotected crops.
Sprinklers    | Shows sprinkler coverage, and highlights unsprinkled crops.

## Configuration
The mod will work fine out of the box, but you can tweak its settings by editing the `config.json`
file if you want. These are the available settings:

setting    | what it affects
---------- | -------------------
`Controls` | The configured controller, keyboard, and mouse buttons (see [key bindings](https://stardewvalleywiki.com/Modding:Key_bindings)). You can separate multiple buttons with commas. The default values are `F2` to toggle the overlay, `Tab` or right controller shoulder to see the next data map, and left controller shoulder to see the previous one.

## Versions
### 1.0
* Initial version.
* Added Junimo huts, scarecrows, sprinklers, and accessibility data maps.

## See also
* <s>Nexus mod</s>
* <s>Discussion thread</s>
