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
Press `F2` to show the data maps overlay, and then left/right `CTRL` to cycle between data maps
(buttons configurable). Below are the current data maps.

### Accessibility
Shows where you can walk and highlights warp tiles; useful for finding hidden paths and nooks.
> ![](docs/screenshots/accessibility.png)

### Junimo huts
Shows Junimo hut coverage, and highlights crops they won't reach. Also works when placing a Junimo
hut from the Wizard's build menu. Compatible with Pelican Fiber.
> ![](docs/screenshots/junimo-huts.png)

### Scarecrows
Shows scarecrow coverage, and highlights unprotected crops. Also works on scarecrows being placed.
> ![](docs/screenshots/scarecrows.png)

### Sprinklers
Shows sprinkler coverage, and highlights unsprinkled crops. Also works on sprinklers being placed.
Compatible with custom sprinkler shapes using Better Sprinklers.
> ![](docs/screenshots/sprinklers.png)

## Configuration
The mod will work fine out of the box, but you can tweak its settings by editing the `config.json`
file if you want. These are the available settings:

setting    | what it affects
---------- | -------------------
`Controls` | The configured controller, keyboard, and mouse buttons (see [key bindings](https://stardewvalleywiki.com/Modding:Key_bindings)). You can separate multiple buttons with commas. The default values are `F2` to toggle the overlay, left `CTRL` or left controller shoulder for the previous map, and right `CTRL` or right controller shoulder for the next one.

## Versions
See [release notes](release-notes.md).

## See also
* <s>Nexus mod</s>
* <s>Discussion thread</s>
