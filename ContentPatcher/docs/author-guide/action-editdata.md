← [author guide](../author-guide.md)

A patch with **`"Action": "EditData"`** edits fields and entries inside a data asset. Any number of
content packs can edit the same asset.

## Contents
* [Introduction](#introduction)
  * [Data assets](#data-assets)
  * [Entries vs fields](#entries-vs-fields)
* [Usage](#usage)
  * [Overview](#overview)
  * [Basic changes](#basic-changes)
  * [Edit dictionary of models](#edit-dictionary-of-models)
  * [Edit list of models](#edit-list-of-models)
  * [Combining operations](#combining-operations)
* [See also](#see-also)

## Introduction
### Data assets
A _data asset_ contains information loaded by the game: events, dialogue, item info, etc. The
[format for each asset is documented on the wiki](https://stardewvalleywiki.com/Modding:Index#Advanced_topics),
but the game has three general types of data asset:

<table>
<tr>
<th>asset type</th>
<th>usage</th>
</tr>
<tr>
<td>dictionary of strings</td>
<td>

A _dictionary of strings_ is a list of string values, where each value is identified by a unique
key. For example, here's what `Data/Boots` looks like when it's
[unpacked](https://stardewvalleywiki.com/Modding:Editing_XNB_files#unpacking):

```js
{
    "504": "Sneakers/A little flimsy... but fashionable!/50/1/0/0/Sneakers",
    "505": "Rubber Boots/Protection from the elements./50/0/1/1/Rubber Boots",
    "506": "Leather Boots/The leather is very supple./50/1/1/2/Leather Boots",
    // other entries omitted for brevity
}
```

In that example, `"504"` is the key, and `"Sneakers/A little flimsy... but fashionable!/50/1/0/0/Sneakers"`
is the value. Both of those together (i.e. the whole line) is called an _entry_.

The value is often a delimited list of fields, split using a certain character (like `/` above).
In the example above, the entry has 6 fields: "_Sneakers_" (the name), "_A little flimsy... but
fashionable!_" (the description), _50_ (the price), etc.

</td>
</tr>
<tr>
<td>dictionary of models</td>
<td>

A _dictionary of models_ is just like a dictionary of strings, except that the delimited string is
an expanded data structure instead. For example, here's what `Data/Concessions` looks like when
it's unpacked:

```js
{
    "fall_movie_0": {
        "ID": null,
        "SheetIndex": 1,
        "Title": "Mysterium",
        "Description": "Peer behind the midnight veil... You must experience to believe!",
        "Tags": [ "horror", "art" ],
        "Scenes": [ /* omitted for brevity * ],
    },
    // other entries omitted for brevity
]
```

So instead of splitting a string like "`0/Cotton Candy/...`", the game can just read each field
directly.

</td>
</tr>
<tr>
<td>list of models</td>
<td>

A _list of models_ is just like a dictionary of models, except that there's no unique key for each
entry. For example, here's what `Data/Concessions` looks like when it's unpacked:

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

</td>
</tr>
</table>

### Entries vs fields
An _entry_ is the entire record (with both the key and value) within a [data asset](#data-assets),
while a _field_ is one piece within the value. `EditData` patches can change both the full entry
(e.g. add, replace, or delete it) or to individual fields within it.

For example:

<table>
<tr>
<th>value type</th>
<th>entry</th>
<th>example field</th>
</tr>
<tr>
<td>string</td>
<td>

```js
"504": "Sneakers/A little flimsy... but fashionable!/50/1/0/0/Sneakers"
```

<td><code>Sneakers</code></td>
</tr>
<tr>
<td>model</td>
<td>

```js
"fall_movie_0": {
    "ID": null,
    "SheetIndex": 1,
    "Title": "Mysterium",
    "Description": "Peer behind the midnight veil... You must experience to believe!",
    "Tags": [ "horror", "art" ],
    "Scenes": [ /* omitted for brevity * ],
}
```

</td>
<td><code>"Title": "Mysterium"</code></td>
</tr>
</table>

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

field      | purpose
---------- | -------
`Fields`   | The individual fields you want to change for existing entries. This field supports [tokens](../author-guide.md#tokens) in field keys and values. The key for each field is the field index (starting at zero) for a slash-delimited string, or the field name for an object.
`Entries`  | The entries in the data file you want to add/replace/delete, indexed by ID. If you only want to change a few fields, use `Fields` instead for best compatibility with other mods. To add an entry, just specify a key that doesn't exist; to delete an entry, set the value to `null` (like `"some key": null`). This field supports [tokens](../author-guide.md#tokens) in entry keys and values.<br /><br />For list values, see also `AppendEntries` and `MoveEntries`.
`MoveEntries` | _(List assets only)_ Change the entry order in a list asset like `Data/MoviesReactions`. (Using this with a non-list asset will cause an error, since those have no order.)
`TextOperations` | <p>Change the value of an existing string entry or field; see _[text operations](../author-guide.md#text-operations)_ for more info.</p><p>To change an entry, use the format `["Entries", "entry key"]` and replace `"entry key"` with the key you'd specify for `Entries` above. If the entry doesn't exist, it'll be created and the text operation will be applied as if it was an empty string.</p><p>To change a field, use the format `["Fields", "entry key", "field key"]` and replace `"entry key"` and `"field key"` with the keys you'd specify for `Fields` above. If the entry doesn't exist, the operation will fail with an error message. If the field doesn't exist, it'll be created if the entry is an object, or fail with an error if the entry is a delimited string. Currently you can only target top-level fields.</p>

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

### Basic changes
This example patch creates a new item (by adding a new entry), and edits the description field for
an existing item:

```js
{
    "Format": "1.24.0",
    "Changes": [
        {
            "Action": "EditData",
            "Target": "Data/ObjectInformation",
            "Entries": {
                "900": "Pufferchick/1200/100/Seeds -74/Pufferchick/An example object."
            },
            "Fields": {
                "128": { // item #128 (pufferfish)
                    5: "Weirdly similar to a pufferchick." // field 5 (description)
                }
            }
        },
    ]
}
```

You can also delete an entry by setting its value to `null`. For example, that can be used to
change event conditions in the key:
```js
{
    "Format": "1.24.0",
    "Changes": [
        {
            "Action": "EditData",
            "Target": "Data/Events/Beach",
            "Entries": {
                "733330/f Sam 750/w sunny/t 700 1500/z winter/y 1": null,
                "733330/f Sam 750/w sunny/t 700 1500/z winter": "[snipped: long event script here]"
            }
        }
    ]
}
```

### Edit dictionary of models
You can edit a [dictionary of models](#data-assets) the same way, except that fields are referenced by
name instead of index.

For example, this renames a movie to _The Brave Little Pikmin_ and adds a new movie:
```js
{
    "Format": "1.24.0",
    "Changes": [
        {
            "Action": "EditData",
            "Target": "Data/Movies",
            "Fields": {
                "spring_movie_0": {
                    "Title": "The Brave Little Pikmin"
                },
            },
            "Entries": {
                "spring_movie_3": {
                    "ID": "spring_movie_3",
                    "Title": "The Brave Little Pikmin II: Now I'm a Piktree",
                    "Description": "Follow the continuing adventures of our pikmin hero as he absorbs nutrients from the soil!",
                    "SheetIndex": 6,
                    "Tags": [ "documentary", "family" ],
                    "Scenes": [
                        {
                            "Image": 0,
                            "MessageDelay": 500,
                            "Music": "movie_nature",
                            "Text": "'The Brave Little Pikmin II: Now I'm a Piktree', sponsored by Joja Cola"
                        }
                    ]
                },
            }
        }
    ]
}
```

### Edit list of models
You can edit a [list of models](#data-assets) the same way too, with a few caveats.

Although there's no unique key, you can still use the `Entries` field to target a specific entry as
if it did. Content Patcher will find the entry based on a unique value in the data model:

asset | field used as the key
----- | ---------------------
_default_ | `ID` if it exists.
`Data/ConcessionTastes` | `Name`
`Data/FishPondData` | The `RequiredTags` field with comma-separated tags (like `fish_ocean,fish_crab_pot`). The key is space-sensitive.
`Data/MoviesReactions` | `NPCName`
`Data/RandomBundles` | `AreaName`
`Data/TailoringRecipes` | `FirstItemTags` and `SecondItemTags`, with comma-separated tags and a pipe between them (like <code>item_cloth&#124;category_fish,fish_semi_rare</code>). The key is space-sensitive.

List assets are often ordered (e.g. the game will use the first entry in `Data\MoviesReactions`
that matches the NPC it's checking). You can change the order using the `MoveEntries` field. For
example, this moves the `Abigail` entry using each possible operation (one move after the other):
```js
{
    "Format": "1.24.0",
    "Changes": [
        {
            "Action": "EditData",
            "Target": "Data/MoviesReactions",
            "MoveEntries": [
                { "ID": "Abigail", "BeforeID": "Leah" }, // move entry so it's right before Leah
                { "ID": "Abigail", "AfterID": "Leah" }, // move entry so it's right after Leah
                { "ID": "Abigail", "ToPosition": "Top" }, // move entry to the top of the list
                { "ID": "Abigail", "ToPosition": "Bottom" }, // move entry to the bottom of the list
            ]
        },
    ]
}
```

When you add a new entry, it's added at the bottom of the list by default.

### Combining operations
You can perform any number of edit operations within the same patch. For example, you can add a new
entry and then move it into the right order at the same time. They'll be applied in this order:
`Entries`, `Fields`, `MoveEntries`, and `TextOperations`.


## See also
* [Author guide](../author-guide.md) for other actions and options
* [Documentation for data asset formats](https://stardewvalleywiki.com/Modding:Index#Advanced_topics) on the wiki
