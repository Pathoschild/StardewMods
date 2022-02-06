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
    "Scenes": [ /* omitted for brevity * ],
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

### Edit a dictionary
The simplest edit for a [dictionary](#data-assets) is to create or overwrite an entry. For
example, this adds an item to `Data/ObjectInformation` (with the key `900`):

```js
{
    "Format": "1.24.0",
    "Changes": [
        {
            "Action": "EditData",
            "Target": "Data/ObjectInformation",
            "Entries": {
                "900": "Pufferchick/1200/100/Seeds -74/Pufferchick/An example object."
            }
        },
    ]
}
```

You can also edit a field within the entry. When the entry's value is a string, the value is
assumed to be a slash-delimited list of fields (each assigned a number starting at zero); otherwise
fields are entries directly within the given entry. For example, this edits the description field
for an item:

```js
{
    "Format": "1.24.0",
    "Changes": [
        {
            "Action": "EditData",
            "Target": "Data/ObjectInformation",
            "Fields": {
                "128": { // entry 128 (pufferfish)
                    5: "Weirdly similar to a pufferchick." // field 5 (description)
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
    "Format": "1.24.0",
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

### Edit a list
You can edit a [list](#data-assets) the same way too, with a few caveats.

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

The order is often important for list assets (e.g. the game will use the first entry in
`Data\MoviesReactions` that matches the NPC it's checking). You can change the order using the
`MoveEntries` field. For example, this moves the `Abigail` entry using each possible operation:
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

New entries are added at the bottom of the list by default.

### Edit a model
A _model_ is a predefined data structure. For content packs, it's essentially identical to a
dictionary except that you can't add new entries (only edit existing ones).

### Combining operations
You can perform any number of edit operations within the same patch. For example, you can add a new
entry and then move it into the right order at the same time. They'll be applied in this order:
`Entries`, `Fields`, `MoveEntries`, and `TextOperations`.


## See also
* [Author guide](../author-guide.md) for other actions and options
* [Documentation for data asset formats](https://stardewvalleywiki.com/Modding:Index#Advanced_topics) on the wiki
