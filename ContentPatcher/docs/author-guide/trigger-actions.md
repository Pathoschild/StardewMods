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

The argument format is `<ID type> [<old id> <new id>]+`, where:
- `<ID type>` is one of `CookingRecipes`, `CraftingRecipes`, `Events`, `Items`, `Mail`, or `Songs`;
- `<old id>` is the current ID to find in the game data;
- `<new id>` is the new ID to change it to.

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
