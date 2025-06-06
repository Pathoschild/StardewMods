← [author guide](../author-guide.md)

A patch with **`"Action": "EditData"`** edits fields and entries inside a data asset. Any number of
content packs can edit the same asset.

**🌐 In other languages: [zh (中文)](../zh/author-guide/action-editdata.md)**.

## Contents
* [Basic concepts](#basic-concepts)
  * [Data assets](#data-assets)
  * [Entries and fields](#entries)
  * [Target fields](#target-fields)
* [Usage](#usage)
  * [Overview](#overview)
  * [Edit a dictionary](#edit-a-dictionary)
  * [Edit a list](#edit-a-list)
  * [Moving list entries](#moving-list-entries)
  * [Edit a model](#edit-a-model)
  * [Combining operations](#combining-operations)
* [Target field](#target-field)
  * [Format](#format)
  * [Examples](#examples)
* [See also](#see-also)

## Basic concepts
The game has many types of data, which Content Patcher abstracts into a few common concepts.
The rest of this page will make much more sense once you understand the concepts explained in this
section, so don't skip it!

### Data assets
A _data asset_ contains info loaded by the game from a content file: events for a location,
dialogue for an NPC, etc. For example, `Data/Objects` is an asset which has info for many items in
the game. The [format for many assets is documented on the wiki](https://stardewvalleywiki.com/Modding:Index#Advanced_topics).

There are three main types of asset:

<table>
<tr>
<th>asset type</th>
<th>usage</th>
</tr>
<tr>
<td>dictionary</td>
<td>

A _dictionary_ is a list of key/value pairs, where the key is unique within the list. These are
surrounded by `{` and `}`.

For example, `Data/Boots` is a dictionary of strings:

```js
{
    // format is "key": "value"
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

A _model_ is a predefined data structure. For content packs, it's identical to a dictionary except
that you can't add new entries (only edit existing ones).

</td>
</tr>
</table>

### Entries and fields<span id="entries"></span>
An _entry_ is a top-level block of data in the target data (i.e. a key/value pair in a dictionary
or a value in a list).

For example, in this snippet from `Data/Objects`, `"MossSoup": { ...}` and `"PetLicense": { ... }`
are two separate entries:
```js
{
    "MossSoup": {
        "Name": "Moss Soup",
        "Type": "Cooking",
        "Category": -7,
        "Price": 80,
        "ContextTags": [ "color_green" ]
        ...
    },
    "PetLicense": {
        "Name": "Pet License",
        "Type": "Basic",
        "Category": 0,
        "Price": 0,
        ...
    },
    ...
}
```

A _field_ is a sub-block of data inside an entry. In the previous example:
- `"MossSoup": { ... }` is an entry;
- `"Name": "Moss Soup"` is a field inside the `"MossSoup": { ... }` entry.

### Target fields
In the previous section, we said that an entry is "_a top-level block of data in the target data_".
A target field lets you change what "target data" means.

For example, let's say we set the target field to the `ContextTags` field above. Then the data your
patch sees is this:
```json
[ "color_green" ]
```
/
That means each value in the `ContextTags` is now an entry, so you can add/replace/remove context
tags without editing the rest of the object data.

(This is covered in more detail under [_Target field_](#target-field) below.)


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

For list values, see also `MoveEntries` below.

</td>
</tr>
<td><code>MoveEntries</code></td>
<td>

_(List assets only)_ Change the entry order in a list asset like `Data/MoviesReactions`. (Using
this with a non-list asset will cause an error, since those have no order.)

See [_moving list entries_](#moving-list-entries) for more info.

</td>
</tr>
<td><code>TextOperations</code></td>
<td>

Add or change the value of an existing string entry or field; see _[text
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
`LogName`     | _(optional)_ A name for this patch to show in log messages. This can be useful for understanding errors. If omitted, it defaults to a name like `EditData Data/Achievements`.
`Update`      | _(optional)_ How often the patch fields should be updated for token changes. See [update rate](../author-guide.md#update-rate) for more info.
`LocalTokens` | _(Optional)_ A set of [local tokens](../author-guide/tokens.md#local-tokens) which can be used within this patch's field.

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

  </tr>
  <tr>
  <td><code>TargetLocale</code></td>
  <td>

_(optional)_ The locale code to match in the asset name. For example, setting `"TargetLocale": "fr-FR"`
will only edit the French localized form of the asset (e.g. `Data/Achievements.fr-FR`). This can be
an empty string to only edit the base unlocalized asset.

If omitted, it's applied to all localized and unlocalized variants of the asset.

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
    "Format": "2.7.0",
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
        }
    ]
}
```

You can also edit a field within the entry. When the entry's value is a string, the value is
assumed to be a slash-delimited list of fields (each assigned a number starting at zero); otherwise
fields are entries directly within the given entry.

For example, this edits the description field for an item:

```js
{
    "Format": "2.7.0",
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
    "Format": "2.7.0",
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
You can edit a [list](#data-assets) the same way too.

Lists don't have keys in the original asset, but they still have a 'key' in Content Patcher which
identifies each entry for features like `Entries` and `MoveEntries`. In other words, the patch to
edit a list looks just like one to edit a dictionary above.

For a list of models (blocks of `{ ... }`), the key is the `Id` field within each model. For
example, this snippet from `Data\LocationContexts` shows one `Music` entry whose ID is `spring1`:
```js
{
    "Default": {
        "SeasonOverride": null,
        "DefaultMusic": null,
        "DefaultMusicCondition": null,
        "DefaultMusicDelayOneScreen": true,
        "Music": [
            {
                "Id": "spring1",
                "Track": "spring1",
                "Condition": "SEASON Spring"
            }
            ...
        ]
    }
}
```

To edit that music entry in a content pack, you'd use the ID as the key. For example:
```js
{
    "Format": "2.7.0",
    "Changes": [
        {
            "Action": "EditData",
            "Target": "Data/LocationContexts",
            "TargetField": [ "Default", "Music" ],
            "Entries": {
                "spring1": {
                    "Id": "spring1",
                    "Track": "spring1",
                    "Condition": "SEASON Spring, YEAR 2"
                }
            }
        }
    ]
}
```

Editing a list of simple strings works exactly the same way, except that the string itself is the
key. See the [example for editing object context tags](#edit-object-context-tags) below.

### Moving list entries
The order is often important for list assets (e.g. the game will use the first entry in
`Data\MoviesReactions` that matches the NPC it's checking). You can change the order using the
`MoveEntries` field. For example, this moves the `Abigail` entry using each possible operation:
```js
{
    "Format": "2.7.0",
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
Your changes normally apply to the top-level entries, but `TargetField` lets you choose a sub-block
of data to edit instead. This affects all of the edit patch options (e.g. `Fields`, `Entries`,
`TextOperations`, etc).

### Format
`TargetField` is a list of field names to 'drill into' (see examples below). Each value in the list
is within the previous value, and can be one of these:

type       | effect
---------- | ------
ID         | A [dictionary key](#edit-a-dictionary) or [list key](#edit-a-list) within the data (e.g. `"Goby"` in the example below).
field name | The name of a field on a data model.
list value | For a simple list of strings or values, the value to target (see examples below).
list index | The position of a value within the list (like `#0` for the first value). This must be prefixed with `#`, otherwise it'll be treated as an ID instead. This is fragile since it depends on the list order not changing from what you expect; consider using an ID or field name instead when possible.

### Examples
#### Edit object context tags
The `Data/Objects` asset has entries like this:
```js
"Goby": {
    "Name": "Goby",
    "Type": "Fish",
    "ContextTags": [ "color_brown", "fish_river", "season_fall", "season_spring", "season_summer" ]
    ...
},
```

Let's say we want to change the context tags, without redefining the whole item or losing changes
from other mods. We can do that with `"TargetField": [ "Goby", "ContextTags" ]`, so the patch now
applies to this data:
```json
[ "color_brown", "fish_river", "season_fall", "season_spring", "season_summer" ]
```

Then we can add, replace, or remove entries within that list as if it was a data asset:
```js
{
    "Format": "2.7.0",
    "Changes": [
        {
            "Action": "EditData",
            "Target": "Data/Objects",
            "TargetField": [ "Goby", "ContextTags" ],
            "Entries": {
                "season_winter": "season_winter", // add a value
                "season_spring": null,            // remove a value
                "color_brown": "color_green"      // replace a value
            }
        },
    ]
}
```

#### Edit a deeply nested field
The above example edited a field at the top of the model, but we can drill down to an arbitrary
depth.

For example, consider this entry in `Data/Objects`:
```json
"791": {
    "Name": "Golden Coconut",
    "GeodeDrops": [
        {
            "Id": "Default",
            "RandomItemId": [ "(O)69", "(O)835", "(O)833", "(O)831", "(O)820", "(O)292", "(O)386" ],
            "StackModifiers": [
                {
                    "Id": "PineappleSeeds",
                    "Condition": "ITEM_ID Target (O)833",
                    "Modification": "Set",
                    "Amount": 5
                },
                ...
            ],
            ...
        }
    ]
}
```

Let's say we want to change pineapple seeds to drop 20 instead of 5. Let's look at the hierarchy of
those fields:

* entry: `791`
  * field: `GeodeDrops`
    * list value with ID: `Default`
      * field: `StackModifiers`
        * list value with ID: `PineappleSeeds`
          * field: `Amount`

So we just need to 'drill down' that hierarchy to edit the field we want:

```json
{
    "Format": "2.7.0",
    "Changes": [
        {
            "Action": "EditData",
            "Target": "Data/Objects",
            "TargetField": [ "791", "GeodeDrops", "Default", "StackModifiers", "PineappleSeeds" ],
            "Entries": {
                "Amount": 20
            }
        }
    ]
}
```

## See also
* [Author guide](../author-guide.md) for other actions and options
* [Documentation for data asset formats](https://stardewvalleywiki.com/Modding:Index#Advanced_topics) on the wiki
