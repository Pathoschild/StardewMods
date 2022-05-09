﻿← [author guide](../author-guide.md)

A patch with **`"Action": "Load"`** replaces an entire asset with your own version.

## Contents
* [Caveats](#caveats)
* [Usage](#usage)
  * [Format](#format)
  * [Examples](#examples)
* [See also](#see-also)

## Caveats
`Load` is very simple, but each asset can only be replaced by one patch, so your content pack won't
be compatible with other content packs that `Load` the same one. (It'll work fine with content
packs that only edit the file, though.)

It's fine to use if you really need it, but consider using one of the [other action
types](../author-guide.md#actions) if possible.

## Usage
### Format
A `Load` patch consists of a model under `Changes` (see examples below) with these fields:

<dl>
<dt>Required fields:</dt>
<dd>

field     | purpose
--------- | -------
`Action`  | The kind of change to make. Set to `Load` for this action type.
`Target`  | The [game asset name](../author-guide.md#what-is-an-asset) to replace (or multiple comma-delimited asset names), like `Portraits/Abigail`. This field supports [tokens](../author-guide.md#tokens), and capitalisation doesn't matter.
`FromFile` | The relative file path in your content pack folder to load instead (like `assets/dinosaur.png`). This can be a `.json` (data), `.png` (image), `.tbin` or `.tmx` (map), or `.xnb` file. This field supports [tokens](../author-guide.md#tokens) and capitalisation doesn't matter.

</dd>
<dt>Optional fields:</dt>
<dd>

field     | purpose
--------- | -------
`When`    | _(optional)_ Only apply the patch if the given [conditions](../author-guide.md#conditions) match.
`LogName` | _(optional)_ A name for this patch to show in log messages. This is useful for understanding errors; if not specified, it'll default to a name like `entry #14 (EditImage Animals/Dinosaurs)`.
`Update`  | _(optional)_ How often the patch fields should be updated for token changes. See [update rate](../author-guide.md#update-rate) for more info.

</dd>
</dl>

### Examples
This replaces Abigail's portraits with your own image (see [NPC modding](https://stardewvalleywiki.com/Modding:NPC_data)):
```js
{
    "Format": "1.26.0",
    "Changes": [
        {
            "Action": "Load",
            "Target": "Portraits/Abigail",
            "FromFile": "assets/abigail.png"
        },
    ]
}
```

You can list any number of load patches, as long as each asset is only loaded by one patch:

```js
{
    "Format": "1.26.0",
    "Changes": [
        {
            "Action": "Load",
            "Target": "Portraits/Abigail",
            "FromFile": "assets/abigail.png"
        },
        {
            "Action": "Load",
            "Target": "Portraits/Penny",
            "FromFile": "assets/penny.png"
        },
    ]
}
```

You can also use [tokens](../author-guide.md#tokens) like `{{TargetWithoutPath}}` to edit several
files at once:

```js
{
    "Format": "1.26.0",
    "Changes": [
        {
            "Action": "Load",
            "Target": "Portraits/Abigail, Portraits/Penny",
            "FromFile": "assets/{{TargetWithoutPath}}.png" // assets/Abigail.png, assets/Penny.png
        },
    ]
}
```

## See also
* [Author guide](../author-guide.md) for other actions and options
