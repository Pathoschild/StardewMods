← [author guide](../author-guide.md)

This page documents the custom [trigger actions](https://stardewvalleywiki.com/Modding:Trigger_actions) added by
Content Patcher.

## Contents
* [`MigrateIds`](#migrateids)
* [See also](#see-also)

## `MigrateIds`
The `Pathoschild.ContentPatcher_MigrateIds` [trigger action](https://stardewvalleywiki.com/Modding:Trigger_actions)
lets you update existing saves when you change IDs for your events, items, mail, recipes, or songs. For example, this
can be used to migrate to [unique string IDs](https://stardewvalleywiki.com/Modding:Common_data_field_types#Unique_string_ID).

The argument format is `<type> [<old id> <new id>]+`:

<table>
<tr>
<th>argument</th>
<th>usage</th>
</tr>
<tr>
<td><code>&lt;type&gt;</code></td>
<td>

One of `CookingRecipes`, `CraftingRecipes`, `Events`, `Items`, `Mail`, or `Songs`.

</td>
</tr>
<tr>
<td><code>&lt;old id&gt;</code></td>
<td>

The former ID to find in the game data.

If this is an item and it was previously defined...
* In a data asset like `Data/Objects`:  
  Use the [qualified item ID](https://stardewvalleywiki.com/Modding:Common_data_field_types#Item_ID), like `(O)OldId`.
* In a **non-installed** Json Assets content pack:  
  Use an ID in the form `"JsonAssets:<type>:<name>"`. The valid types are `big-craftables`, `clothing`, `hats`,
  `objects`, and `weapons`. For example, a hat named _Puffer Hat_ would be `"JsonAssets:hats:Puffer Hat"`.
* In an **installed** Json Assets content pack:  
  Use a [Json Assets token](https://github.com/spacechase0/StardewValleyMods/blob/develop/JsonAssets/docs/author-guide.md#integration-with-content-patcher)
  to get the real item ID, and then use it as a [qualified item ID](https://stardewvalleywiki.com/Modding:Common_data_field_types#Item_ID).
  For example, `(O){{spacechase0.JsonAssets/ObjectId: Puffer Hat}}`.

</td>
</tr>
<tr>
<td><code>&lt;new id&gt;</code></td>
<td>

The new ID to change it to.

For an item, using a [qualified item ID](https://stardewvalleywiki.com/Modding:Common_data_field_types#Item_ID) is
recommended to avoid ambiguity.

</td>
</tr>
</table>

You can have any number old/new ID pairs.

For example, this changes the ID for two crafting recipes: `Puffer Plush` renamed to `{{ModId}}_PufferPlush`, and `Puffer
Sofa` renamed to `{{ModId}}_PufferSofa`:

```js
{
    "Action": "EditData",
    "Target": "Data/TriggerActions",
    "Entries": {
        "{{ModId}}_MigrateIds": {
            "Id": "{{ModId}}_MigrateIds",
            "Trigger": "DayStarted",
            "Actions": [
                // Note: use double-quotes around an argument if it contains spaces. This example has single-quotes for
                // the action itself, so we don't need to escape the double-quotes inside it.
                'Pathoschild.ContentPatcher_MigrateIds CraftingRecipes "Puffer Plush" {{ModId}}_PufferPlush "Puffer Sofa" {{ModId}}_PufferSofa'
            ],
            "HostOnly": true
        }
    }
}
```

> [!IMPORTANT]  
> Content Patcher needs full access to the whole game state to do this. The action will log an error if:
>* it isn't set to `"Trigger": "DayStarted"` and `"HostOnly": true`.
>* or it's not being run from `Data/TriggerActions`.

## See also
* [Author guide](../author-guide.md) for other actions and options
* [_Trigger actions_ on the wiki](https://stardewvalleywiki.com/Modding:Trigger_actions) for more info
