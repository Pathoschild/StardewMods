← [author guide](../author-guide.md)

A patch with **`"Action": "Load"`** replaces an entire asset with your own version.

**🌐 In other languages: [zh (中文)](../zh/author-guide/action-load.md)**.

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
`LogName` | _(optional)_ A name for this patch to show in log messages. This can be useful for understanding errors. If omitted, it defaults to a name like `Load Data/Achievements`.
`Update`  | _(optional)_ How often the patch fields should be updated for token changes. See [update rate](../author-guide.md#update-rate) for more info.
`LocalTokens` | _(Optional)_ A set of [local tokens](../author-guide/tokens.md#local-tokens) which can be used within this patch's field.

</dd>
<dt>Advanced fields:</dt>
<dd>

<table>
  <tr>
    <th>field</th>
    <th>purpose</th>
  </tr>
  <tr>
    <td><code>Priority</code></td>
    <td>

_(optional)_ When multiple patches or mods load the same asset, the priority which decides which
one is applied. Default `Exclusive`.

The possible values are:

* `Low`, `Medium`, or `High`: the highest-priority patch is applied. If multiple patches have the
  same priority, the first one in the list (by load order + patch order) is applied.
* `Exclusive`: all or nothing. If one patch uses it, it's applied and all other load patches are
  ignored. If multiple patches use it, then _no patches_ are applied and an error message is shown.

  Avoid using `Exclusive` when possible, since it significantly reduces mod compatibility. This is
  only the default value because Content Patcher can't know whether your content pack will still
  work if another patch is loaded instead.

If you need very specific ordering, you can use a simple offset like `"High + 2"` or `"Medium - 10"`.
The default levels are -1000 (low), 0 (medium), and 1000 (high). This isn't usually needed though,
since patches with the same priority are already sorted by the order they're listed in your content
pack.

This field does _not_ support tokens, and capitalization doesn't matter.

  </tr>
  <tr>
  <td><code>TargetLocale</code></td>
  <td>

_(optional)_ The locale code to match in the asset name. For example, setting `"TargetLocale": "fr-FR"`
will only load the French localized form of the asset (e.g. `Data/Achievements.fr-FR`). This can be
an empty string to only load the base unlocalized asset.

If omitted, it's applied to all localized and unlocalized variants of the asset.

</td>
</table>
</dd>
</dl>

### Examples
This replaces Abigail's portraits with your own image (see [NPC modding](https://stardewvalleywiki.com/Modding:NPC_data)):
```js
{
    "Format": "2.7.0",
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
    "Format": "2.7.0",
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
    "Format": "2.7.0",
    "Changes": [
        {
            "Action": "Load",
            "Target": "Portraits/Abigail, Portraits/Penny",
            "FromFile": "assets/{{TargetWithoutPath}}.png" // assets/Abigail.png, assets/Penny.png
        },
    ]
}
```

You can use `Priority` to have an optional load (e.g. if it'll still work when another mod loads it first):
```js
{
    "Format": "2.7.0",
    "Changes": [
        {
            "Action": "Load",
            "Target": "Data/Events/AdventureGuild",
            "FromFile": "assets/empty-event-file.json",
            "Priority": "Low"
        }
    ]
}
```

## See also
* [Author guide](../author-guide.md) for other actions and options
