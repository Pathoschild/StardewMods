← [author guide](../author-guide.md)

A patch with **`"Action": "EditData"`** edits fields and entries inside a data asset. Any number of
content packs can edit the same asset.

## Contents
* [Introduction](#introduction)
  * [What is a data asset?](#data-assets)
  * [What is an entry?](#entries)
* [Usage](#usage)
  * [Overview](#overview)
  * [Edit a dictionary](#edit-a-dictionary)
  * [Edit a list](#edit-a-list)
  * [Edit a model](#edit-a-model)
  * [Combining operations](#combining-operations)
* [Target field](#target-field)
  * [Format](#format)
  * [Example](#example)
* [See also](#see-also)

## Introduction
### What is a data asset?<span id="data-assets"></span>
A _data asset_ contains information loaded by the game: events, dialogue, item info, etc. The
[format for many assets is documented on the wiki](https://stardewvalleywiki.com/Modding:Index#Advanced_topics).

There are three types of asset:

<table>
<tr>
<th>asset type</th>
<th>usage</th>
</tr>
<tr>
<td>dictionary</td>
<td>

A _dictionary_ is a list of key/value pairs, where the key is unique within the list. The key is
always an integer or string, but the value can be any data type.

For example, `Data/Boots` is a dictionary of strings:

```js
{
    // "key": value
    "504": "Sneakers/A little flimsy... but fashionable!/50/1/0/0/Sneakers",
    "505": "Rubber Boots/Protection from the elements./50/0/1/1/Rubber Boots",
    "506": "Leather Boots/The leather is very supple./50/1/1/2/Leather Boots"
}
```

</td>
</tr>
<tr>
<td>list</td>
<td>

A _list_ is a non-unique set of values which don't have an explicit key. These are surrounded by
`[` and `]`.

For example, `Data/Concessions` is a list of models:

```js
[
    {
       "ID": 0,
       "Name": "Cotton Candy",
       "DisplayName": "Cotton Candy",
       "Description": "A large pink cloud of spun sugar.",
       "Price": 50,
       "ItemTags": [ "Sweet", "Candy" ]
    },
    // other entries omitted for brevity
]
```

Although lists don't have keys, Content Patcher often assigns one field as a unique identifer which
can be used as the key (see [_edit a list_](#edit-a-list)).

</td>
</tr>
<tr>
<td>model</td>
<td>

A _model_ is a predefined data structure. For content packs, it's essentially identical to a
dictionary except that you can't add new entries (only edit existing ones).

</td>
</tr>
</table>

### What is an entry?<span id="entries"></span>
An _entry_ is a key/value pair in a dictionary, or a value in a list. The key is always a number or
string.

For example, each line in `Data/Boots` is an entry (where the number before the colon is the key,
and the string after it is the value):
```js
{
    "504": "Sneakers/A little flimsy... but fashionable!/50/1/0/0/Sneakers",
    "505": "Rubber Boots/Protection from the elements./50/0/1/1/Rubber Boots"
}
```

That also applies to data models. For example, this is one entry from `Data/Movies`:
```js
"fall_movie_0": {
    "ID": null,
    "SheetIndex": 1,
    "Title": "Mysterium",
    "Description": "Peer behind the midnight veil... You must experience to believe!",
    "Tags": [ "horror", "art" ],
    "Scenes": [ /* omitted for brevity */ ]
}
```

A _field_ is just an entry directly within an entry. In the previous example, `"SheetIndex": 1` is
a field.

## Usage
### Overview
An `EditData` patch consists of a model under `Changes` (see examples below) with these fields:

<dl>
<dt>Required fields:</dt>
<dd>

You must specify both of these fields:

field     | purpose
--------- | -------
`Action`  | The kind of change to make. Set to `EditData` for this action type.
`Target`  | The [game asset name](../author-guide.md#what-is-an-asset) to replace (or multiple comma-delimited asset names), like `Characters/Dialogue/Abigail`. This field supports [tokens](../author-guide.md#tokens), and capitalisation doesn't matter.

And at least one of these:

<table>
<tr>
<th>field</th>
<th>purpose</th>
</tr>
<tr>
<td><code>Fields</code></td>
<td>

The individual fields you want to change for existing entries. This field supports
[tokens](../author-guide.md#tokens) in field keys and values. The key for each field is the field
index (starting at zero) for a slash-delimited string, or the field name for an object.

</td>
</tr>
<td><code>Entries</code></td>
<td>

The entries in the data file you want to add/replace/delete, indexed by ID. If you only want to
change a few fields, use `Fields` instead for best compatibility with other mods. To add an entry,
just specify a key that doesn't exist; to delete an entry, set the value to `null` (like
`"some key": null`). This field supports [tokens](../author-guide.md#tokens) in entry keys and
values.

For list values, see also `MoveEntries`.

</td>
</tr>
<td><code>MoveEntries</code></td>
<td>

_(List assets only)_ Change the entry order in a list asset like `Data/MoviesReactions`. (Using
this with a non-list asset will cause an error, since those have no order.)

</td>
</tr>
<td><code>TextOperations</code></td>
<td>

Change the value of an existing string entry or field; see _[text
operations](../author-guide.md#text-operations)_ for more info.

To change an entry, use the format `["Entries", "entry key"]` and replace `"entry key"` with the
key you'd specify for `Entries` above. If the entry doesn't exist, it'll be created and the text
operation will be applied as if it was an empty string.

To change a field, use the format `["Fields", "entry key", "field key"]` and replace `"entry key"`
and `"field key"` with the keys you'd specify for `Fields` above. If the entry doesn't exist, the
operation will fail with an error message. If the field doesn't exist, it'll be created if the
entry is an object, or fail with an error if the entry is a delimited string. Currently you can
only target top-level fields.

</td>
</tr>
</table>
</dd>
<dt>Optional fields:</dt>
<dd>

field         | purpose
------------- | -------
`TargetField` | _(optional)_ When targeting a [list or dictionary](#data-assets), the field within the value to set as the root scope; see [_target field_](#target-field) below. This field supports [tokens](../author-guide.md#tokens).
`When`        | _(optional)_ Only apply the patch if the given [conditions](../author-guide.md#conditions) match.
`LogName`     | _(optional)_ A name for this patch to show in log messages. This is useful for understanding errors; if not specified, it'll default to a name like `entry #14 (EditImage Animals/Dinosaurs)`.
`Update`      | _(optional)_ How often the patch fields should be updated for token changes. See [update rate](../author-guide.md#update-rate) for more info.

</dd>
<dt>Advanced fields:</dt>
<dd>

<table>
  <tr>
    <td>field</td>
    <td>purpose</td>
  </tr>
  <tr>
  <td><code>Priority</code></td>
  <td>

_(optional)_ When multiple patches or mods edit the same asset, the order in which they should be
applied. The possible values are `Early`, `Default`, and `Late`. The default value is `Default`.

The patches for an asset (across all mods) are applied in this order:

1. by earliest to latest priority;
2. then by mod load order (e.g. based on dependencies);
3. then by the order the patches are listed in your `content.json`.

If you need a more specific order, you can use a simple offset like `"Default + 2"` or `"Late - 10"`.
The default levels are -1000 (early), 0 (default), and 1000 (late).

This field does _not_ support tokens, and capitalization doesn't matter.

> [!TIP]  
> Priorities can make your changes harder to follow and troubleshoot. Suggested best practices:
> * Consider only using very general priorities when possible (like `Late` for a cosmetic overlay
>   meant to be applied over base edits from all mods).
> * There's no need to set priorities relative to _your own_ patches, since you can just list them
>   in the order they should be applied.

</td>
</table>

</dd>
</dl>

### Edit a dictionary
The simplest edit for a [dictionary](#data-assets) is to create or overwrite an entry. For
example, this [adds a new item](https://stardewvalleywiki.com/Modding:Items) with the ID
`{{ModId}}_Pufferchick` to `Data/Objects`:

```js
{
    "Format": "2.0.0",
    "Changes": [
        {
            "Action": "EditData",
            "Target": "Data/Objects",
            "Entries": {
                "{{ModId}}_Pufferchick": {
                    "Name": "{{ModId}}_Pufferchick",
                    "DisplayName": "Pufferchick",
                    "Description": "An example object.",
                    "Type": "Seeds",
                    "Category": -74,
                    "Price": 1200,
                    "Texture": "Mods/{{ModId}}/Objects"
            }
        },
    ]
}
```

You can also edit a field within the entry. When the entry's value is a string, the value is
assumed to be a slash-delimited list of fields (each assigned a number starting at zero); otherwise
fields are entries directly within the given entry.

For example, this edits the description field for an item:

```js
{
    "Format": "2.0.0",
    "Changes": [
        {
            "Action": "EditData",
            "Target": "Data/Objects",
            "Fields": {
                "MossSoup": { // entry with ID 'MossSoup'
                    "Description": "Maybe a pufferchick would like this."
                }
            }
        },
    ]
}
```

You can also delete an entry by setting its value to `null`. For example, this deletes an event to
recreate it with different conditions:
```js
{
    "Format": "2.0.0",
    "Changes": [
        {
            "Action": "EditData",
            "Target": "Data/Events/Beach",
            "Entries": {
                "733330/f Sam 750/w sunny/t 700 1500/z winter/y 1": null,
                "733330/f Sam 750/w sunny/t 700 1500/z winter": "event script would go here"
            }
        }
    ]
}
```

When the value has nested entries, you can use [`TargetField`](#target-field) to edit a specific
one.

### Edit a list
You can edit a [list](#data-assets) the same way too. Each entry in a list has an `Id` field, which
is the entry key you'd use to change it.

The order is often important for list assets (e.g. the game will use the first entry in
`Data\MoviesReactions` that matches the NPC it's checking). You can change the order using the
`MoveEntries` field. For example, this moves the `Abigail` entry using each possible operation:
```js
{
    "Format": "2.0.0",
    "Changes": [
        {
            "Action": "EditData",
            "Target": "Data/MoviesReactions",
            "MoveEntries": [
                { "ID": "Abigail", "BeforeID": "Leah" },     // move entry so it's right before Leah
                { "ID": "Abigail", "AfterID": "Leah" },      // move entry so it's right after Leah
                { "ID": "Abigail", "ToPosition": "Top" },    // move entry to the top of the list
                { "ID": "Abigail", "ToPosition": "Bottom" }, // move entry to the bottom of the list
            ]
        },
    ]
}
```

New entries are added at the bottom of the list by default.

### Edit a model
A _model_ is a predefined data structure. For content packs, it's essentially identical to a
dictionary except that you can't add new entries (only edit existing ones).

### Combining operations
You can perform any number of edit operations within the same patch. For example, you can add a new
entry and then move it into the right order at the same time. They'll be applied in this order:
`Entries`, `Fields`, `MoveEntries`, and `TextOperations`.

## Target field
Your changes normally apply to the top-level entries, but `TargetField` lets you apply changes to a
nested subset of the entry instead. For example, you can change one field within a field within the
entry.

This affects all of the change fields (e.g. `Fields`, `Entries`, `TextOperations`, etc).

### Format
`TargetField` describes a path to 'drill into' relative to the entire data asset.

For example, `"TargetField": [ "Crafts Room", "BundleSets", "#0", "Bundles", "#0" ]` will...
1. select the `Crafts Room` entry;
2. select the `BundleSets` field on that entry;
3. select the first bundle set in that `BundleSets` field;
3. select the `Bundles` field on that bundle set;
4. and select the first value in that `Bundles` array.

At that point any changes will be applied within the selected value, instead of the entire model.

Each value in the list can be one of these:

type        | effect
----------- | ------
ID          | A dictionary key or [list key](#edit-a-list) within a dictionary/list (e.g. `"Crafts Room"` in the example below).
field name  | The name of a field on the data model (e.g. `"BundleSets"` and `"Bundles"` in the example below).
array index | The position of a value within the list (e.g. `#0` in the example below). This must be prefixed with `#`, otherwise it'll be treated as an ID instead.

### Example
The `Data/RandomBundles` asset has entries like this:
```js
[
    {
        "AreaName": "Crafts Room",
        "Keys": "13 14 15 16 17 19",
        "BundleSets": [
            {
                "Bundles": [
                    {
                        "Name": "Spring Foraging",
                        "Index": 0,
                        "Sprite": "13",
                        "Color": "Green",
                        "Items": "1 Wild Horseradish, 1 Daffodil, 1 Leek, 1 Dandelion, 1 Spring Onion",
                        "Pick": 4,
                        "RequiredItems": -1,
                        "Reward": "30 Spring Seeds"
                    },
                    ...
                ]
            }
        ],
        ...
    }
]
```

Using `TargetField`, you can edit the bundle reward within the entry without needing to redefine
the entire entry:

```js
{
    "Format": "2.0.0",
    "Changes": [
        {
            "Action": "EditData",
            "Target": "Data/RandomBundles",
            "TargetField": [ "Crafts Room", "BundleSets", "#0", "Bundles", "#0" ],
            "Entries": {
                "Reward": "60 Spring Seeds" // double normal reward
            }
        },
    ]
}
```

The `TargetField` sets the selected value as the scope, so the rest of the fields work as if the
entire data asset looked like this:
```js
{
    "Name": "Spring Foraging",
    "Index": 0,
    "Sprite": "13",
    "Color": "Green",
    "Items": "1 Wild Horseradish, 1 Daffodil, 1 Leek, 1 Dandelion, 1 Spring Onion",
    "Pick": 4,
    "RequiredItems": -1,
    "Reward": "30 Spring Seeds"
}
```

## See also
* [Author guide](../author-guide.md) for other actions and options
* [Documentation for data asset formats](https://stardewvalleywiki.com/Modding:Index#Advanced_topics) on the wiki
