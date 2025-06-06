← [author guide](../author-guide.md)

This page documents various tools available to track down issues with your content pack.

**🌐 In other languages: [zh (中文)](../zh/author-guide/troubleshooting.md)**.

## Contents
* [Schema validator](#schema-validator)
* [Patch commands](#patch-commands)
  * [`summary`](#summary)
  * [`update`](#update)
  * [`reload`](#reload)
  * [`export`](#export)
  * [`parse`](#parse)
  * [`dump`](#dump)
  * [`invalidate`](#invalidate)
* [Debug mode](#debug-mode)
* [Verbose log](#verbose-log)
* [See also](#see-also)

## Schema validator
You can validate your `content.json` and `manifest.json` automatically to detect some common issues.
(You should still test your content pack in-game before releasing it, since the validator won't
detect all issues.)

To validate online:
1. Go to [smapi.io/json](https://smapi.io/json).
2. Set the format to 'Manifest' (for `manifest.json`) or 'Content Patcher' (for `content.json`).
3. Drag & drop the JSON file onto the textbox, or paste in the text.
4. Click the button to view the validation summary. You can optionally share the URL to let someone
   else see the result.

To validate in a text editor that supports JSON Schema, see
[_Using a schema file directly_](https://github.com/Pathoschild/SMAPI/blob/develop/docs/technical/web.md#using-a-schema-file-directly)
in the JSON validator documentation.

Tips:
* You should update your content pack to the latest format version whenever you update it, for best
  futureproofing.
* If you get an error like `Unexpected character`, your JSON syntax is invalid. Try checking the
  line mentioned (or the one above it) for a missing comma, bracket, etc.
* If you need help figuring out an error, see [_see also_ in the main readme](../README.md#see-also)
  for some links to places you can ask.

## Patch commands
Content Patcher adds several console commands for testing and troubleshooting. Enter `patch help`
directly into the SMAPI console for more info.

### `summary`
`patch summary` provides a comprehensive overview of what your content packs are doing. That
includes...

* global token values;
* local token values for each pack;
* custom locations added by each pack;
* and patches loaded from each pack along with their current values, what they changed, and
  (if applicable) the reasons they weren't applied.

For example:

```
=====================
==  Global tokens  ==
=====================
   Content Patcher:

      token name       | value
      ---------------- | -----
      Day              | [X] 5
      DayEvent         | [X]
      DayOfWeek        | [X] Friday
      DaysPlayed       | [X] 305
      FarmCave         | [X] Mushrooms
      FarmhouseUpgrade | [X] 1
      FarmName         | [X] River Coop
      FarmType         | [X] Riverland

      [snipped for simplicity]

=====================
== Content patches ==
=====================
The following patches were loaded. For each patch:
  - 'loaded' shows whether the patch is loaded and enabled (see details for the reason if not).
  - 'conditions' shows whether the patch matches with the current conditions (see details for the reason if not). If this is unexpectedly false, check (a) the conditions above and (b) your Where field.
  - 'applied' shows whether the target asset was loaded and patched. If you expected it to be loaded by this point but it's false, double-check (a) that the game has actually loaded the asset yet, and (b) your Targets field is correct.


Example Content Pack:
------------------------------

   Local tokens:
      token name        | value
      ----------------- | -----
      WeatherVariant    | [X] Wet

   Patches:
      loaded  | conditions | applied | name + details
      ------- | ---------- | ------- | --------------
      [X]     | [ ]        | [ ]     | Dry Palm Trees // conditions don't match: WeatherVariant
      [X]     | [X]        | [X]     | Wet Palm Trees

   Current changes:
      asset name                | changes
      ------------------------- | -------
      TerrainFeatures/tree_palm | edited image
```

You can specify these arguments in any order (e.g. `patch summary "LemonEx.HobbitHouse" "Another.Content.Pack" full`):

argument              | effect
:-------------------- | :-----
`"<content pack ID>"` | One or more content pack IDs for which to show data. If omitted, all content packs will be shown.
`asset "<asset>"`     | Only list changes to a given asset. This filters by base asset name, so `Data/furniture` would also show edits to `Data/furniture.fr-FR`. You can list multiple assets by repeating the flag (e.g. `asset Data/Crops asset Data/Objects`).
`full`                | Don't truncate very long token values.
`unsorted`            | Don't sort the values for display. This is mainly useful for checking the real order for `valueAt`.

### `update`
`patch update` immediately updates Content Patcher's condition context and rechecks all patches.
This is mainly useful if you change conditions through the console (like the date), and want to
update patches without going to bed.

### `reload`
`patch reload` reloads patches added by a specific content pack. That lets you change your content pack's JSON files
while the game is running, and see the changes in-game without restarting the game. Non-patch content (like config
schema and dynamic tokens) aren't reloaded.

For example:

```
> patch reload "LemonEx.HobbitHouse"
Content pack reloaded.
```

For content packs which use [`Include` patches](action-include.md), you can optionally only reload patches added by a
specific `Include` patch by passing the file's relative path as a second argument.

For example:

```
> patch reload "LemonEx.HobbitHouse" "assets/some-include.json"
Content pack reloaded.
```

### `export`
`patch export` saves a copy of a given asset to your game folder, which lets you see what it looks
like with all changes applied. This currently works for data, image, and map assets.

For example:

```
> patch export "Maps/springobjects"
Exported asset 'Maps/springobjects' to 'C:\Program Files (x86)\Steam\steamapps\common\Stardew Valley\patch export\Maps_springobjects.png'.
```

### `parse`
`patch parse` parses a tokenizable string and shows the resulting metadata, using the current
Content Patcher context (the same values used when applying patches).

This recognizes global tokens by default. You can use tokens for a specific content pack (including
dynamic tokens and config values) by giving the content pack ID from its `manifest.json` in the
optional second argument.

For example:

```
> patch parse "assets/{{Variant}}.{{Language}}.png" "Pathoschild.ExampleContentPack"

Metadata
----------------
   raw value:   assets/{{Variant}}.{{Language}}.png
   ready:       True
   mutable:     True
   has tokens:  True
   tokens used: Language, Variant

Diagnostic state
----------------
   valid:    True
   in scope: True
   ready:    True

Result
----------------
   The token string is valid and ready. Parsed value: "assets/wood.en.png"
```

This can also be used to troubleshoot token syntax:

```
> patch parse "assets/{{Season}.png"
[ERROR] Can't parse that token value: Reached end of input, expected end of token ('}}').
```

### `dump`
`patch dump` provides specialized reports about the internal Content Patcher state. These are meant
for technical troubleshooting; in most cases you should use `patch summary` instead.

Available reports:

* `patch dump order` shows the global definition order for all loaded patches.
* `patch dump applied` shows all active patches grouped by target in their apply order, including
  whether each patch is applied.

### `invalidate`
`patch invalidate` immediately removes a named asset from the game/SMAPI content cache. If it's an
asset handled by SMAPI, the asset will be reloaded immediately and Content Patcher will reapply its
changes to it. Otherwise the next code which loads the same asset will get a new instance.

For example:

```
> patch invalidate "Buildings/houses"

[Content Patcher] Requested cache invalidation for 'Portraits\Abigail'.
[SMAPI]           Invalidated 1 asset names (Portraits\Abigail).
[SMAPI]           Propagated 1 core assets (Portraits\Abigail).
[Content Patcher] Invalidated asset 'Portraits/Abigail'.
```

## Debug mode
Content Patcher has a 'debug mode' which lets you view loaded textures directly in-game with any
current changes. To enable it, open the mod's `config.json` file in a text editor and enable
`EnableDebugFeatures`.

Once enabled, press `F3` to display textures and left/right `CTRL` to cycle textures. Close and
reopen the debug UI to refresh the texture list.
> ![](../screenshots/debug-mode.png)

## Verbose log
Content Patcher doesn't log much info. You can change that by opening SMAPI's
`smapi-internal/StardewModdingAPI.config.json` in a text editor and enabling `VerboseLogging`.
**This may significantly slow down loading, and should normally be left disabled unless you need it.**

Once enabled, it will log significantly more information at three points:
1. when loading patches (e.g. whether each patch was enabled and which files were preloaded);
2. when SMAPI checks if Content Patcher can load/edit an asset;
3. and when the context changes (anytime the conditions change: different day, season, weather, etc).

If your changes aren't appearing in game, make sure you set a `LogName` field (see the [action
docs](../author-guide.md#actions)) and then search the SMAPI log file for that name. Particular
questions to ask:
* Did Content Patcher load the patch?  
  _If it doesn't appear, check that your `content.json` is correct. If it says 'skipped', check
  your `Enabled` value or `config.json`._
* When the context is updated, is the box ticked next to the patch name?  
  _If not, checked your `When` field._
* When SMAPI checks if it can load/edit the asset name, is the box ticked?  
  _If not, check your `When` and `Target` fields._

## See also
* [Author guide](../author-guide.md) for other actions and options
