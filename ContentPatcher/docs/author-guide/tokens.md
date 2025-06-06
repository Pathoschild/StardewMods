← [author guide](../author-guide.md)

This document lists the tokens available in Content Patcher packs.

**See the [main README](../README.md) for other info**.

**🌐 In other languages: [zh (中文)](../zh/author-guide/tokens.md)**.

## Contents
* [Introduction](#introduction)
  * [Overview](#overview)
  * [Token types](#token-types)
  * [Set theory](#set-theory)
* [Global tokens](#global-tokens)
  * [Date and weather](#date-and-weather)
  * [Player](#player)
  * [Relationships](#relationships)
  * [World](#world)
  * [Number manipulation](#number-manipulation)
  * [String manipulation](#string-manipulation)
  * [Metadata](#metadata)
  * [Field references](#field-references)
  * [Specialized](#specialized)
* [Config tokens](#config-tokens)
* [Dynamic tokens](#dynamic-tokens)
* [Local tokens](#local-tokens)
* [Input arguments](#input-arguments)
  * [Overview](#overview-1)
  * [Global input arguments](#global-input-arguments)
  * [Custom input value separator](#custom-input-value-separator)
* [Randomization](#randomization)
* [Advanced](#advanced)
  * [Query expressions](#query-expressions)
  * [Mod-provided tokens](#mod-provided-tokens)
  * [Aliases](#aliases)
* [Common values](#common-values)
* [See also](#see-also)

## Introduction
### Overview
A **token** is just a named set of values. For example, the `season` token would contain the value
`summer` during the in-game summer.

There are two main ways to use tokens:

#### Placeholders
You can use tokens in text by putting two curly brackets around the token name, which will be
replaced with the actual value automatically.

Token placeholders can be used in most fields (the documentation for each field will specify), and
they're not case-sensitive (so `{{season}}` and `{{SEASON}}` are the same thing). Patches will be
disabled automatically if a token they use isn't currently available.

For example, this gives the farmhouse a different appearance in each season:

```js
{
    "Action": "EditImage",
    "Target": "Buildings/houses",
    "FromFile": "assets/{{season}}_house.png" // assets/spring_house.png, assets/summer_house.png, etc
}
```

Tokens which return a single value (like `{{season}}`) are most useful in placeholders, but
multi-value tokens will work too (they'll show a comma-delimited list).

#### Conditions
You can make a patch conditional by adding a `When` field, which can list any number of conditions.
Each condition has...
* A key containing a [token](#introduction) without the outer curly braces, like
  `Season` or `HasValue:{{spouse}}`. The key is not case-sensitive.
* A value containing the comma-separated values to match, like `spring, summer`. If the key token
  returns any of these values, the condition matches. This field supports
  [tokens](#introduction) and is not case-sensitive.

For example, this changes the house texture only in spring or summer in the first year:

```js
{
    "Action": "EditImage",
    "Target": "Buildings/houses",
    "FromFile": "assets/{{season}}_house.png",
    "When": {
        "Season": "spring, summer",
        "Year": "1"
    }
}
```

Each condition is true if _any_ of its values match, and the patch is applied if _all_ of its
conditions match.

### Token types
There are several types of tokens, but they're all used exactly the same way.

You don't need to learn all of these. They each serve a different purpose, and most content packs
will only use one or two types at most.

The token types are (in order of most to least commonly used):
* [Global tokens](#global-tokens) contain common values like the season, weather, friendships, etc.
  These are provided by Content Patcher itself.
* [Config tokens](#config-tokens) contain mod options you define, which the player can choose from.
* [Dynamic tokens](#dynamic-tokens) contain arbitrary values you define. These let you reuse values
  without redefining them each time, or build complex values from simpler ones.
* _(Advanced)_ [Local tokens](#local-tokens) are just like dynamic tokens, but limited to one patch
  (or all the patches loaded through an `Include` patch) instead of the whole content pack. These
  are mainly used to avoid repetition for a set of values.
* _(Advanced)_ [Mod-provided tokens](#mod-provided-tokens) are provided by other mods installed by
  the player.

### Set theory
Internally, tokens are [_sets_ in the mathematical sense](https://en.wikipedia.org/wiki/Set_(abstract_data_type)).
In practical terms, that just means that they...
- can't have duplicate values (i.e. each distinct value will appear once in the set);
- are usually non-sorted;
- are very efficient when comparing values.

## Global tokens
Global token values are defined by Content Patcher, so you can use them without doing anything else.

### Date and weather
<table>
<tr>
<th>condition</th>
<th>purpose</th>
<th>&nbsp;</th>
</tr>

<tr valign="top" id="Day">
<td>Day</td>
<td>The day of month. Possible values: any integer from 1 through 28.</td>
<td><a href="#Day">#</a></td>
</tr>

<tr valign="top" id="DayEvent">
<td>DayEvent</td>
<td>

The festival or wedding happening today. Possible values:
* `wedding` (current player is getting married);
* `dance of the moonlight jellies`;
* `egg festival`;
* `feast of the winter star`;
* `festival of ice`;
* `flower dance`;
* `luau`;
* `stardew valley fair`;
* `spirit's eve`;
* a custom festival name.

</td>
<td><a href="#DayEvent">#</a></td>
</tr>

<tr valign="top" id="DayOfWeek">
<td>DayOfWeek</td>
<td>

The day of week. Possible values: `Monday`, `Tuesday`, `Wednesday`, `Thursday`, `Friday`,
`Saturday`, and `Sunday`.

</td>
<td><a href="#DayOfWeek">#</a></td>
</tr>

<tr valign="top" id="DaysPlayed">
<td>DaysPlayed</td>
<td>The total number of in-game days played for the current save (starting from one when the first day starts).</td>
<td><a href="#DaysPlayed">#</a></td>
</tr>

<tr valign="top" id="Season">
<td>Season</td>
<td>

The season name. Possible values: `Spring`, `Summer`, `Fall`, and `Winter`.

</td>
<td><a href="#Season">#</a></td>
</tr>

<tr valign="top" id="Time">
<td>Time</td>
<td>

The in-game time of day, as a numeric value between `0600` (6am) and `2600` (2am before sleeping).
This can also be used with range tokens:
```js
"When": {
   "Time": "{{Range: 0600, 2600}}"
}
```

ℹ See _[update rate](../author-guide.md#update-rate)_ before using this token.

</td>
<td><a href="#Time">#</a></td>
</tr>

<tr valign="top" id="Weather">
<td>Weather</td>
<td>

The weather type in the current world area (or the area specified with a
[`LocationContext`](#location-context) argument). Possible values:

value       | meaning
----------- | -------
`Sun`       | The weather is sunny (including festival/wedding days). This is the default weather if no other value applies.
`Rain`      | Normal rain is falling, but without lightning.
`Storm`     | Normal rain is falling with lightning.
`GreenRain` | [Green rain](https://stardewvalleywiki.com/Weather#Green_Rain) is falling.
`Snow`      | Snow is falling.
`Wind`      | The wind is blowing with visible debris (e.g. flower petals in spring and leaves in fall).
_custom_    | For custom weathers defined by a mod, the weather ID.

ℹ See _[update rate](../author-guide.md#update-rate)_ before using this token without specifying a
location context.

</td>
<td><a href="#Weather">#</a></td>
</tr>

<tr valign="top" id="Year">
<td>Year</td>
<td>

The year number (like `1` or `2`).

</td>
<td><a href="#Year">#</a></td>
</tr>
</table>

### Player
<table>
<tr>
<th>condition</th>
<th>purpose</th>
<th>&nbsp;</th>
</tr>

<tr valign="top" id="DailyLuck">
<td>DailyLuck</td>
<td>

The [daily luck](https://stardewvalleywiki.com/Luck) for the [current or specified
player](#target-player).

This is a decimal value usually between -0.1 and 0.1. This **cannot** be compared using the
`{{Range}}` token, which produces a range of integer values. The value can only be safely compared
using [query expressions](#query-expressions). For example:

```js
"When": {
   "Query: {{DailyLuck}} < 0": true // spirits unhappy today
}
```

</td>
<td><a href="#DailyLuck">#</a></td>
</tr>

<tr valign="top" id="FarmhouseUpgrade">
<td>FarmhouseUpgrade</td>
<td>

The [farmhouse upgrade level](https://stardewvalleywiki.com/Farmhouse#Upgrades) for the [current or
specified player](#target-player). The normal values are 0 (initial farmhouse), 1 (adds kitchen), 2
(add children's bedroom), and 3 (adds cellar). Mods may add upgrade levels beyond that.

</td>
<td><a href="#FarmhouseUpgrade">#</a></td>
</tr>

<tr valign="top" id="HasActiveQuest">
<td>HasActiveQuest</td>
<td>

The active quest IDs in the [current or specified player](#target-player)'s quest list. See
[Modding:Quest data](https://stardewvalleywiki.com/Modding:Quest_data) on the wiki for valid quest
IDs.

</td>
<td><a href="#HasActiveQuest">#</a></td>
</tr>

<tr valign="top" id="HasCaughtFish">
<td>HasCaughtFish</td>
<td>

The fish IDs caught by the [current or specified player](#target-player). See [object
IDs](https://stardewvalleywiki.com/Modding:Object_data) on the wiki.

</td>
<td><a href="#HasCaughtFish">#</a></td>
</tr>

<tr valign="top" id="HasConversationTopic">
<td>HasConversationTopic</td>
<td>

The active [conversation topics](https://stardewvalleywiki.com/Modding:Dialogue#Conversation_topics)
for the [current or specified player](#target-player).

</td>
<td><a href="#HasConversationTopic">#</a></td>
</tr>

<tr valign="top" id="HasCookingRecipe">
<td>HasCookingRecipe</td>
<td>

The [cooking recipes](https://stardewvalleywiki.com/Cooking) known by the [current or specified
player](#target-player).

</td>
<td><a href="#HasCookingRecipe">#</a></td>
</tr>

<tr valign="top" id="HasCraftingRecipe">
<td>HasCraftingRecipe</td>
<td>

The [crafting recipes](https://stardewvalleywiki.com/Crafting) known by the [current or specified
player](#target-player).

</td>
<td><a href="#HasCraftingRecipe">#</a></td>
</tr>

<tr valign="top" id="HasDialogueAnswer">
<td>HasDialogueAnswer</td>
<td>

The [response IDs](https://stardewvalleywiki.com/Modding:Dialogue#Response_IDs) for answers to
question dialogues by the [current or specified player](#target-player).

</td>
<td><a href="#HasDialogueAnswer">#</a></td>
</tr>

<tr valign="top" id="HasFlag">
<td>HasFlag</td>
<td>

The flags set for the [current or specified player](#target-player). That includes...

* letter IDs sent to the player (including letters they haven't read, or those added to the mailbox for tomorrow);
* non-letter mail flags (used to track game info);
* world state IDs.

See [useful flags on the wiki](https://stardewvalleywiki.com/Modding:Mail_data#List).

</td>
<td><a href="#HasFlag">#</a></td>
</tr>

<tr valign="top" id="HasProfession">
<td>HasProfession</td>
<td>

The [professions](https://stardewvalleywiki.com/Skills) learned by the [current or specified
player](#target-player).

Possible values:

* Combat skill: `Acrobat`, `Brute`, `Defender`, `Desperado`, `Fighter`, `Scout`.
* Farming skill: `Agriculturist`, `Artisan`, `Coopmaster`, `Rancher`, `Shepherd`, `Tiller`.
* Fishing skill: `Angler`, `Fisher`, `Mariner`, `Pirate`, `Luremaster`, `Trapper`.
* Foraging skill: `Botanist`, `Forester`, `Gatherer`, `Lumberjack`, `Tapper`, `Tracker`.
* Mining skill: `Blacksmith`, `Excavator`, `Gemologist`, `Geologist`, `Miner`, `Prospector`.

Custom professions added by a mod are represented by their integer profession ID.

</td>
<td><a href="#HasProfession">#</a></td>
</tr>

<tr valign="top" id="HasReadLetter">
<td>HasReadLetter</td>
<td>

The letter IDs opened by the [current or specified player](#target-player). A letter is considered
'opened' if the letter UI was shown.

</td>
<td><a href="#HasReadLetter">#</a></td>
</tr>

<tr valign="top" id="HasSeenEvent">
<td>HasSeenEvent</td>
<td>

The event IDs seen by the [current or specified player](#target-player), matching IDs in the
`Data/Events` files.

You can use [Debug Mode](https://www.nexusmods.com/stardewvalley/mods/679) to see event IDs in-game.

</td>
<td><a href="#HasSeenEvent">#</a></td>
</tr>

<tr valign="top" id="HasVisitedLocation">
<td>HasVisitedLocation</td>
<td>

The location internal names which the [current or specified player](#target-player) have previously
visited, matching IDs in the `Data/Locations` file.

You can use [Debug Mode](https://www.nexusmods.com/stardewvalley/mods/679) to see location names
in-game.

</td>
<td><a href="#HasVisitedLocation">#</a></td>
</tr>

<tr valign="top" id="HasWalletItem">
<td>HasWalletItem</td>
<td>

The [special wallet items](https://stardewvalleywiki.com/Wallet) for the current player.

Possible values:

flag                       | meaning
-------------------------- | -------
`DwarvishTranslationGuide` | Unlocks speaking to Dwarf in the mine and the dwarf in the volcano shop.
`RustyKey`                 | Unlocks the sewers.
`ClubCard`                 | Unlocks the desert casino.
`KeyToTheTown`             | Allows access to all buildings in town, at any time of day.
`SpecialCharm`             | Permanently increases daily luck.
`SkullKey`                 | Unlocks the [Skull Cavern](https://stardewvalleywiki.com/Skull_Cavern) and the Saloon's Junimo Kart machine.
`MagnifyingGlass`          | Unlocks the ability to find secret notes.
`DarkTalisman`             | Unlocks the Witch's Swamp.
`MagicInk`                 | Unlocks [magical buildings](https://stardewvalleywiki.com/Wizard%27s_Tower#Buildings) and [dark shrines](https://stardewvalleywiki.com/Witch%27s_Hut).
`BearsKnowledge`           | Increases sell price of blackberries and salmonberries.
`SpringOnionMastery`       | Increases sell price of spring onions.

</td>
<td><a href="#HasWalletItem">#</a></td>
</tr>

<tr valign="top" id="IsMainPlayer">
<td>IsMainPlayer</td>
<td>

Whether the [current or specified player](#target-player) is the main player. Possible values:
`true`, `false`.

</td>
<td><a href="#IsMainPlayer">#</a></td>
</tr>

<tr valign="top" id="IsOutdoors">
<td>IsOutdoors</td>
<td>

Whether the [current or specified player](#target-player) is outdoors. Possible values: `true`,
`false`.

ℹ See _[update rate](../author-guide.md#update-rate)_ before using this token.

</td>
<td><a href="#IsOutdoors">#</a></td>
</tr>

<tr valign="top" id="LocationContext">
<td>LocationContext</td>
<td>

The general world area recognized by the game containing the [current or specified
player](#target-player).

Possible values:

* `Default` (in the valley);
* `Desert` (in the [desert](https://stardewvalleywiki.com/Desert));
* `Island` (on [Ginger Island](https://stardewvalleywiki.com/Ginger_Island));
* or the ID of a custom context [in `Data/LocationContexts`](https://stardewvalleywiki.com/Modding:Migrate_to_Stardew_Valley_1.6#Custom_location_contexts).

ℹ See _[update rate](../author-guide.md#update-rate)_ before using this token.

</td>
<td><a href="#LocationContext">#</a></td>
</tr>

<tr valign="top" id="LocationName">
<td id="LocationUniqueName">LocationName<br />LocationUniqueName</td>
<td>

The internal name of the [current or specified player](#target-player)'s current location, like
`FarmHouse` or `Town`. You can see the name for the current location using
[Debug Mode](https://www.nexusmods.com/stardewvalley/mods/679) or [`patch
summary`](../author-guide.md#patch-summary).

Notes:
* Temporary festival maps always have the location name "Temp".
* `LocationName` and `LocationUniqueName` are identical except inside constructed buildings, cabins,
  and farmhand cellars. For example, a coop might have `LocationName` "Deluxe Coop" and
  `LocationUniqueName` "Coop7379e3db-1c12-4963-bb93-23a1323a25f7". The `LocationUniqueName` can be
  used as the target location for warp properties.

ℹ See _[update rate](../author-guide.md#update-rate)_ before using this token.

</td>
<td><a href="#LocationName">#</a></td>
</tr>

<tr valign="top" id="LocationOwnerId">
<td>LocationOwnerId</td>
<td>

The [unique ID of the player](#target-player) who owns the [current or specified player](#target-player)'s
location, if applicable.

This works for these locations:

location      | owner
:------------ | :----
farmhouse     | main player
island house  | main player
cabin         | linked farmhand
cellar        | same as the cabin/farmhouse it's linked to
farm building | player who constructed it

This can be used to get other info for the owner, like `{{PlayerName: {{LocationOwnerId}}}}`.

ℹ See _[update rate](../author-guide.md#update-rate)_ before using this token.

</td>
<td><a href="#LocationOwnerId">#</a></td>
</tr>

<tr valign="top" id="PlayerGender">
<td>PlayerGender</td>
<td>

The [current or specified player](#target-player)'s gender. Possible values: `Female`, `Male`.

</td>
<td><a href="#PlayerGender">#</a></td>
</tr>

<tr valign="top" id="PlayerName">
<td>PlayerName</td>
<td>

The [current or specified player](#target-player)'s name.

</td>
<td><a href="#PlayerName">#</a></td>
</tr>

<tr valign="top" id="PreferredPet">
<td>PreferredPet</td>
<td>

The current player's preferred pet. Possible values: `Cat`, `Dog`.

</td>
<td><a href="#PreferredPet">#</a></td>
</tr>

<tr valign="top" id="SkillLevel">
<td>SkillLevel</td>
<td>

The current player's skill levels. You can specify the skill level as an input argument like this:

```js
"When": {
   "SkillLevel:Combat": "1, 2, 3" // combat level 1, 2, or 3
}
```

The valid skills are `Combat`, `Farming`, `Fishing`, `Foraging`, `Luck` (unused in the base game),
and `Mining`.

</td>
<td><a href="#SkillLevel">#</a></td>
</tr>
</table>

### Relationships
<table>
<tr>
<th>condition</th>
<th>purpose</th>
<th>&nbsp;</th>
</tr>

<tr valign="top" id="ChildNames">
<td id="ChildGenders">ChildNames<br />ChildGenders</td>
<td>

The names and genders (`Female` or `Male`) for the [current or specified player](#target-player)'s
children.

These are listed in order of birth for use with the [`valueAt` argument](#valueAt). For example,
`{{ChildNames |valueAt=0}}` and `{{ChildGenders |valueAt=0}}` is the name and gender of the oldest
child.

</td>
<td><a href="#ChildNames">#</a></td>
</tr>

<tr valign="top" id="Hearts">
<td>Hearts</td>
<td>

The player's heart level with a given NPC. You can specify the character name as an input argument
(using their internal name regardless of translations), like this:

```js
"When": {
   "Hearts:Abigail": "10, 11, 12, 13"
}
```

</td>
<td><a href="#Hearts">#</a></td>
</tr>

<tr valign="top" id="Relationship">
<td>Relationship</td>
<td>

The player's relationship with a given NPC or player. You can specify the character name as part
of the key (using their internal name regardless of translations), like this:

```js
"When": {
   "Relationship:Abigail": "Married"
}
```

The valid relationship types are...

value    | meaning
-------- | -------
Unmet    | The player hasn't talked to the NPC yet.
Friendly | The player talked to the NPC at least once, but hasn't reached one of the other stages yet.
Dating   | The player gave them a bouquet.
Engaged  | The player gave them a mermaid's pendant, but the marriage hasn't happened yet.
Married  | The player married them.
Divorced | The player married and then divorced them.

</td>
<td><a href="#Relationship">#</a></td>
</tr>

<tr valign="top" id="Roommate">
<td>Roommate</td>
<td>

The name of the [current or specified player](#target-player)'s NPC roommate (using their internal
name regardless of translations).

</td>
<td><a href="#Roommate">#</a></td>
</tr>


<tr valign="top" id="Spouse">
<td>Spouse</td>
<td>

The name of the [current or specified player](#target-player)'s NPC spouse (using their internal
name regardless of translations).

</td>
<td><a href="#Spouse">#</a></td>
</tr>
</table>

### World
<table>
<tr>
<th>condition</th>
<th>purpose</th>
<th>&nbsp;</th>
</tr>

<tr valign="top" id="FarmCave">
<td>FarmCave</td>
<td>

The [farm cave](https://stardewvalleywiki.com/The_Cave) type. Possible values: `None`, `Bats`,
`Mushrooms`.

</td>
<td><a href="#FarmCave">#</a></td>
</tr>

<tr valign="top" id="FarmMapAsset">
<td>FarmMapAsset</td>
<td>

The farm type's map asset name relative to the game's `Content/Maps` folder.

This is usually one of:

farm type    | value
------------ | -----
Standard     | `Farm`
Beach        | `Farm_Island`
Forest       | `Farm_Foraging`
Four Corners | `Farm_FourCorners`
Hill-top     | `Farm_Mining`
Meadowlands  | `Farm_Ranching`
Riverland    | `Farm_Fishing`
Wilderness   | `Farm_Combat`
_custom type_  | The `MapName` value in `Data/AdditionalFarms`.
_invalid type_ | `Farm`

</td>
<td><a href="#FarmMapAsset">#</a></td>
</tr>

<tr valign="top" id="FarmName">
<td>FarmName</td>
<td>The name of the current farm.</td>
<td><a href="#FarmName">#</a></td>
</tr>

<tr valign="top" id="FarmType">
<td>FarmType</td>
<td>

The [farm type](https://stardewvalleywiki.com/The_Farm#Farm_Maps). This will be one of...

value | description
----- | -----------
`Standard`<br />`Beach`<br />`FourCorners`<br />`Forest`<br />`Hilltop`<br />`Riverland`<br />`Wilderness` | A farm type from the base game.
_custom farm ID_ | A custom farm type, using the `ID` value from the mod's farm data.
`Custom` | _(rare)_ A custom farm type from mods using an old approach for custom farm types.

</td>
<td><a href="#FarmType">#</a></td>
</tr>

<tr valign="top" id="IsCommunityCenterComplete">
<td>IsCommunityCenterComplete</td>
<td>

Whether all bundles in the community center are completed. Possible values: `true`, `false`.

</td>
<td><a href="#IsCommunityCenterComplete">#</a></td>
</tr>

<tr valign="top" id="IsJojaMartComplete">
<td>IsJojaMartComplete</td>
<td>

Whether the player bought a Joja membership and completed all Joja bundles. Possible values: `true`
 `false`.

</td>
<td><a href="#IsJojaMartComplete">#</a></td>
</tr>

<tr valign="top" id="HavingChild">
<td>HavingChild</td>
<td>

The names of players and NPCs whose relationship has an active pregnancy or adoption. Player names
are prefixed with `@` to avoid ambiguity with NPC names. For example, to check if the current
player is having a child:

```js
"When": {
    "HavingChild": "{{spouse}}"
}
```

Usage notes:
* `"HavingChild": "@{{playerName}}"` and `"HavingChild": "{{spouse}}"` are equivalent for this token.
* See also the `Pregnant` token.

</td>
<td><a href="#HavingChild">#</a></td>
</tr>

<tr valign="top" id="Pregnant">
<td>Pregnant</td>
<td>

The players or NPCs who are currently pregnant. This is a subset of `HavingChild` that only applies
to the female partner in heterosexual relationships. (Same-sex partners adopt a child instead.)

</td>
<td><a href="#Pregnant">#</a></td>
</tr>
</table>

### Number manipulation
<table>
<tr>
<th>condition</th>
<th>purpose</th>
<th>&nbsp;</th>
</tr>

<tr valign="top" id="Count">
<td>Count</td>
<td>

Get the number of values currently contained by a token. For example, `{{Count:{{HasActiveQuest}}}}`
is the number of currently active quests.

</td>
<td><a href="#Count">#</a></td>
</tr>

<tr valign="top" id="Query">
<td>Query</td>
<td>

Evaluate arbitrary arithmetic and logical operations; see [_query expressions_](#query-expressions)
for more info.

</td>
<td><a href="#Query">#</a></td>
</tr>

<tr valign="top" id="Range">
<td>Range</td>
<td>

A list of integers between the specified min/max integers (inclusive). This is mainly meant for
comparing values; for example:

```js
"When": {
   "Hearts:Abigail": "{{Range: 6, 14}}" // equivalent to "6, 7, 8, 9, 10, 11, 12, 13, 14"
}
```

You can use tokens for the individual numbers (like `{{Range:6, {{MaxHearts}}}}`) or both (like
`{{Range:{{FriendshipRange}}}})`, as long as the final parsed input has the form `min, max`.

To minimise the possible performance impact, the range can't exceed 5000 numbers and should be much
smaller if possible.

</td>
<td><a href="#Range">#</a></td>
</tr>

<tr valign="top" id="Round">
<td>Round</td>
<td>

An approximation of the input number with fewer fractional digits.

In its default form, this just rounds to the nearest whole number. For example,
`{{Round: 2.1 }}` results in `2`, and `{{Round: 2.5555 }}` results in `3`.

The token takes optional arguments to change the rounding logic:

usage | result | description
----- | ------ | -----------
`Round(2.5555)` | `3` | Round to the nearest whole number.
`Round(2.5555, 2)` | `2.56` | Round to the nearest value with the given number of fractional digits.
`Round(2.5555, 2, down)` | `2.55` | Round `up` or `down` (defaults to [half rounded to even](https://en.wikipedia.org/wiki/Rounding#Round_half_to_even) if not specified).

This is mainly useful in combination with [query expressions](#query-expressions). For example,
monster HP must be a whole number, so this rounds the result of a calculation to the nearest whole
number:

```js
{
  "Action": "EditData",
  "Target": "Data/Monsters",
  "Fields": {
    "Green Slime": {
      "0": "{{Round: {{Query: {{multiplier}} * 2.5 }} }}",
    }
  }
}
```

You can use tokens in the individual fields (like `{{Round: {{value}}, 2}}`) or for multiple fields
at once (like `{{Round: {{Settings}}}}` where `{{Settings}}` = `2.5, 3, up`), as long as the final
parsed input matches one of the above forms.

</td>
<td><a href="#Round">#</a></td>
</tr>
</table>

### String manipulation
<table>
<tr>
<th>condition</th>
<th>purpose</th>
<th>&nbsp;</th>
</tr>

<tr valign="top" id="Lowercase">
<td id="Uppercase">Lowercase<br />Uppercase</td>
<td>

Convert the input text to a different letter case:

<dl>
<dt>Lowercase</dt>
<dd>

Change to all small letters.<br />Example: `{{Lowercase:It's a warm {{Season}} day!}}` &rarr; `it's a warm summer day!`

</dd>
<dt>Uppercase</dt>
<dd>

Change to all capital letters.<br />Example: `{{Uppercase:It's a warm {{Season}} day!}}` &rarr; `IT'S A WARM SUMMER DAY!`

</dd>
</dl>
</td>
<td><a href="#Lowercase">#</a></td>
</tr>

<tr valign="top" id="Merge">
<td>Merge</td>
<td>

Combine any number of input values into one token. This can be used to search multiple tokens in a
`When` block:

```js
"When": {
   "Merge: {{Roommate}}, {{Spouse}}": "Krobus"
}
```

Or combined with [`valueAt`](#valueat) to get the first non-empty value from a list of tokens:

```js
"{{Merge: {{TokenA}}, {{TokenB}}, {{TokenC}} |valueAt=0 }}"
```

Note that you can also add literal values to the list, like `{{Merge: {{Roommate}}, Krobus, Abigail }}`.

</td>
<td><a href="#Merge">#</a></td>
</tr>

<tr valign="top" id="PathPart">
<td>PathPart</td>
<td>

Get part of a file/asset path, in the form `{{PathPart: <path>, <part to get>}}`. For
example:

```js
{
   "Action": "Load",
   "Target": "Portraits/Abigail",
   "FromFile": "assets/{{PathPart: {{Target}}, Filename}}.png" // assets/Abigail.png
}
```

Given the path `assets/portraits/Abigail.png`, you can specify...

* A fragment type:

  part value      | description | example
  --------------- | ----------- | ------
  `DirectoryPath` | The path without the file name. | `assets/portraits`
  `FileName`      | The file name (including the extension, if any). | `Abigail.png`
  `FileNameWithoutExtension` | The file name (excluding the extension). | `Abigail`

* Or an index position from the left:

  part value | example
  ---------- | -------
  `0`        | `assets`
  `1`        | `portraits`
  `2`        | `Abigail.png`
  `3`        | _empty value_

* Or a negative index to search from the right:

  part value | example
  ---------- | -------
  `-1`       | `Abigail.png`
  `-2`       | `portraits`
  `-3`       | `assets`
  `-4`       | _empty value_

See also [`TargetPathOnly`](#TargetPathOnly) and [`TargetWithoutPath`](#TargetWithoutPath), which
simplify a very common version of this.

</td>
<td><a href="#PathPart">#</a></td>
</tr>

<tr valign="top" id="Render">
<td>Render</td>
<td>

Get the string representation of the input argument. This is mainly useful in `When` blocks to
compare the rendered value directly (instead of comparing token set values):

```js
"When": {
   "Render:{{season}} {{day}}": "spring 14"
}
```

This isn't needed in other contexts, where you can use token placeholders directly. For example,
these two entries are equivalent:

```js
"Entries": {
   "Mon": "It's a lovely {{season}} {{day}}!",
   "Mon": "It's a lovely {{Render: {{season}} {{day}} }}!",
}
```

</td>
<td><a href="#Render">#</a></td>
</tr>
</table>

### Metadata
These tokens provide meta info about tokens, content pack files, installed mods, and the game state.

<table>
<tr>
<th>condition</th>
<th>purpose</th>
<th>&nbsp;</th>
</tr>

<tr valign="top" id="FirstValidFile">
<td>FirstValidFile</td>
<td>

Get the first path which matches a file in the content pack folder, given a list of file paths. You
can specify any number of files.

Each file path must be relative to the content pack's main folder, and can't contain `../`.

For example:

```js
// from `assets/<language>.json` if it exists, otherwise `assets/default.json`
"FromFile": "{{FirstValidFile: assets/{{language}}.json, assets/default.json }}"
```

</td>
<td><a href="#FirstValidFile">#</a></td>
</tr>

<tr valign="top" id="HasMod">
<td>HasMod</td>
<td>

The installed mod IDs (matching the `UniqueID` field in their `manifest.json`).

</td>
<td><a href="#HasMod">#</a></td>
</tr>

<tr valign="top" id="HasFile">
<td>HasFile</td>
<td>

Whether a file exists in the content pack folder given its path. Returns `true` or `false`.

The file path must be relative to the content pack's main folder, and can't contain `../`.

For example:

```js
"When": {
  "HasFile:assets/{{season}}.png": "true"
}
```

If the input has commas like `HasFile: a, b.png`, they're treated as part of the filename.

</td>
<td><a href="#HasFile">#</a></td>
</tr>

<tr valign="top" id="HasValue">
<td>HasValue</td>
<td>

Whether the input argument is non-blank. For example, to check if the player is married to anyone:

```js
"When": {
  "HasValue:{{spouse}}": "true"
}
```

This isn't limited to a single token. You can pass in any tokenized string, and `HasValue` will
return true if the resulting string is non-blank:

```js
"When": {
  "HasValue:{{spouse}}{{LocationName}}": "true"
}
```

</td>
<td><a href="#HasValue">#</a></td>
</tr>

<tr valign="top" id="I18n">
<td>i18n</td>
<td>

Get text from the content pack's `i18n` translation files. See the [translation
documentation](translations.md) for more info.

</td>
<td><a href="#I18n">#</a></td>
</tr>

<tr valign="top" id="Language">
<td>Language</td>
<td>

The game's current language. Possible values:

code | meaning
---- | -------
`de` | German
`en` | English
`es` | Spanish
`fr` | French
`hu` | Hungarian
`it` | Italian
`ja` | Japanese
`ko` | Korean
`pt` | Portuguese
`ru` | Russian
`tr` | Turkish
`zh` | Chinese

For custom languages added via `Data/AdditionalLanguages`, the token will contain their
`LanguageCode` value.

</td>
<td><a href="#Language">#</a></td>
</tr>

<tr valign="top" id="ModId">
<td>ModId</td>
<td>

The current content pack's unique ID (from the `UniqueID` field in its `manifest.json`).

This is typically used to build [unique string IDs](https://stardewvalleywiki.com/Modding:Common_data_field_types#Unique_string_ID).
For example:
```json
"Id": "{{ModId}}_ExampleItem"
```

</td>
<td><a href="#ModId">#</a></td>
</tr>
</table>

### Field references
These tokens contain field values for the current patch. For example, `{{FromFile}}` is the current
value of the `FromFile` patch field.

These have some restrictions:
* They're only available in a patch block directly (e.g. they won't work in dynamic tokens).
* They can't be used in their source field. For example, you can't use `{{Target}}` in the `Target`
  field.
* You can't create circular references. For example, you can use `{{FromFile}}` in the `Target`
  field and `{{Target}}` in the `FromFile` field, but not both at once.

<table>
<tr>
<th>condition</th>
<th>purpose</th>
<th>&nbsp;</th>
</tr>

<tr valign="top" id="FromFile">
<td>FromFile</td>
<td>

The patch's `FromFile` field value for the current asset. Path separators are normalized for the OS.
This is mainly useful for checking if the path exists:

```js
{
   "Action": "EditImage",
   "Target": "Characters/Abigail",
   "FromFile": "assets/{{Season}}_abigail.png",
   "When": {
      "HasFile:{{FromFile}}": true
   }
}
```

</td>
<td><a href="#FromFile">#</a></td>
</tr>

<tr valign="top" id="Target">
<td id="TargetPathOnly">Target<br />TargetPathOnly<br />TargetWithoutPath</td>
<td id="TargetWithoutPath">

The patch's `Target` field value for the current asset. Path separators are normalized for the OS.
This is mainly useful for patches which specify multiple targets:

```js
{
   "Action": "EditImage",
   "Target": "Characters/Abigail, Characters/Sam",
   "FromFile": "assets/{{TargetWithoutPath}}.png" // assets/Abigail.png *or* assets/Sam.png
}
```

The difference between the three tokens is the part they return. For example, given the target value
`Characters/Dialogue/Abigail`:

token               | part returned | example
------------------- | ------------- | ------
`Target`            | The full path. | `Characters/Dialogue/Abigail`
`TargetPathOnly`    | The part before the last separator. | `Characters/Dialogue`
`TargetWithoutPath` | The part after the last separator. | `Abigail`

See also [`PathPart`](#PathPart) for more advanced scenarios.

</td>
<td><a href="#Target">#</a></td>
</tr>
</table>

### Specialized
These are advanced tokens meant to support some specific situations.

<table>
<tr>
<th>condition</th>
<th>purpose</th>
<th>&nbsp;</th>
</tr>

<tr valign="top" id="AbsoluteFilePath">
<td>AbsoluteFilePath</td>
<td>

Get the absolute path for a file in your content pack's folder, given its path relative to the
content pack's main folder (which can't contain `../`).

For example, for a player with a default Windows Steam install, `{{AbsoluteFilePath: assets/portraits.png}}`
will return a value similar to
`C:\Program Files (x86)\Steam\steamapps\common\Stardew Valley\Mods\[CP] YourMod\assets\portraits.png`.

</td>
<td><a href="#AbsoluteFilePath">#</a></td>
</tr>

<tr valign="top" id="FormatAssetName">
<td>FormatAssetName</td>
<td>

Normalize an asset name into the form expected by the game. For example,
`{{FormatAssetName: Data/\\///Achievements/}}` returns a value like `Data/Achievements`.

This has one optional argument:

argument    | effect
----------- | ------
`separator` | The folder separator to use in the asset name instead of the default `/`. This is only needed when adding the path to a `/`-delimited field, like `{{FormatAssetName: {{assetKey}} |separator=\\}}`.

There's no need to use this in `Target` fields, which are normalized automatically.

</td>
<td><a href="#FormatAssetName">#</a></td>
</tr>

<tr valign="top" id="InternalAssetKey">
<td>InternalAssetKey</td>
<td>

Get a special asset key which lets the game load a file directly from your content pack, without
needing to `Load` it into a new `Content` asset.

For example, you can use this to provide the textures for a custom farm type:

```js
{
    "Format": "2.7.0",
    "Changes": [
        {
            "Action": "EditData",
            "Target": "Data/AdditionalFarms",
            "Entries": {
                "{{ModId}}_FarmId": {
                    "IconTexture": "{{InternalAssetKey: assets/icon.png}}",
                    …
                }
            }
        }
    ]
}
```

Note that other content packs can't target an internal asset key (which is why it's internal). If
you need to let other content packs edit it, you can use [`Action: Load`](action-load.md) to create
a new asset for it, then use that asset name instead. When doing this, using the [unique string
ID](https://stardewvalleywiki.com/Modding:Common_data_field_types#Unique_string_ID)
convention is strongly recommended to avoid conflicts. For example:
```js
{
    "Format": "2.7.0",
    "Changes": [
        {
            "Action": "EditData",
            "Target": "Data/AdditionalFarms",
            "Entries": {
                "{{ModId}}_FarmId": {
                    "IconTexture": "Mods/{{ModId}}/FarmIcon",
                    …
                }
            }
        },
        {
            "Action": "Load",
            "Target": "Mods/{{ModId}}/FarmIcon",
            "FromFile": "assets/icon.png"
        }
    ]
}
```

</td>
<td><a href="#InternalAssetKey">#</a></td>
</tr>
</table>

### Config tokens
You can let players configure your mod using a `config.json` file. If the player has [Generic Mod
Config Menu](https://www.nexusmods.com/stardewvalley/mods/5098) installed, they'll also be able to
configure the mod through an in-game options menu.

For example, you can use config values as tokens and conditions:

```js
{
    "Format": "2.7.0",
    "ConfigSchema": {
        "EnableJohn": {
            "AllowValues": "true, false",
            "Default": true
        }
    },
    "Changes": [
        {
            "Action": "Include",
            "FromFile": "assets/john.json",
            "When": {
                "EnableJohn": true
            }
        }
    ]
}
```

See the [player config documentation](config.md) for more info.

### Dynamic tokens
Dynamic tokens are defined in a `DynamicTokens` section of your `content.json` (see example below).
Each block in this section defines the value for a token using these fields:

field   | purpose
------- | -------
`Name`  | The name of the token to use for [tokens & condition](#introduction).
`Value` | The value(s) to set. This can be a comma-delimited value to give it multiple values. This field supports [tokens](#introduction), including dynamic tokens defined before this entry.
`When`  | _(optional)_ Only set the value if the given [conditions](#introduction) match. If not specified, always matches.

Some usage notes:
* You can list any number of dynamic token blocks.
* If you list multiple blocks for the same token name, the last one whose conditions match will be
  used.
* You can use tokens in the `Value` and `When` fields. That includes dynamic tokens if they're
  defined earlier in the list (in which case the last applicable value _defined before this block_
  will be used). Using a token in the value implicitly adds a `When` condition (so the block is
  skipped if the token is unavailable, like `{{season}}` when a save isn't loaded).
* Dynamic tokens can't have the same name as an existing global token or player config field.

For example, this `content.json` defines a custom `{{style}}` token and uses it to load different
crop sprites depending on the weather:

```js
{
   "Format": "2.7.0",
   "DynamicTokens": [
      {
         "Name": "Style",
         "Value": "dry"
      },
      {
         "Name": "Style",
         "Value": "wet",
         "When": {
            "Weather": "rain, storm"
         }
      }
   ],
   "Changes": [
      {
         "Action": "Load",
         "Target": "TileSheets/crops",
         "FromFile": "assets/crop-{{style}}.png"
      }
   ]
}
```

### Local tokens
Local tokens are defined for a specific patch via its `LocalTokens` field, and can be used in its
other fields. The token names must be a plain string, but the values can contain tokens.

**These have two important restrictions:**
* Local tokens defined directly on a patch can't be used in the `FromFile` and `Target` fields
  (since they can use `{{FromFile}}` and `{{Target}}`). However, local tokens inherited from a
  parent `Include` patch can be used in those fields too.
* Local tokens are always considered dynamic text, so they can't be used in data model fields that
  _only_ allow booleans or numbers. This will be improved in upcoming iterations of the feature.

For example:
```json
{
   "Action": "EditData",
   "Target": "Data/Buildings",
   "Entries": {
      "{{BuildingId}}": {
          "Name": "Deluxe Stable",
          "Texture": "Buildings/{{BuildingId}}",
          ...
      }
   },
   "LocalTokens": {
      "BuildingId": "{{ModId}}_DeluxeStable"
   }
}
```

When set on an `Include` patch, local tokens are inherited by all patches loaded through it. This
can be used to implement template behavior, where a set of patches is applied for each set of values.

For example, you can do this in `content.json`:
```json
{
   "Action": "Include",
   "FromFile": "assets/add-hat.json",
   "LocalTokens": {
      "IdSuffix": "PufferHat",
      "DisplayName": "Puffer Hat",
      "Description": "A hat that puffs up when you're threatened.",
      "Price": 500
   }
}
```

And then add an `assets/add-hat.json` file like this:
```json
{
   /*
   This file is loaded once per hat, with these tokens:
      {{IdSuffix}}: the unique portion of the item ID, like "DeerAntlers".
      {{DisplayName}}: the translated display name for the hat item.
      {{Description}}: the translated description for the hat item.
      {{Price}}: the sell price for the hat item.
   */
   "Changes": [
      // add hat data
      {
         "Action": "EditData",
         "Target": "Data/hats",
         "Entries": {
            "{{ModId}}_{{IdSuffix}}": "{{ModId}}_{{IdSuffix}}/{{Description}}/true/true//{{DisplayName}}/4/{{TextureNameInData}}"
         }
      },

      // add to shop
      {
         "Action": "EditData",
         "Target": "Data/Shops",
         "TargetField": [ "HatMouse", "Items" ],
         "Entries": {
            "{{ModId}}_{{IdSuffix}}": {
               "ItemId": "{{ModId}}_{{IdSuffix}}",
               "Price": "{{Price}}"
            }
         }
      }
   ]
}
```

## Input arguments
### Overview
An **input argument** is a value you give to the token within the `{{...}}` braces. Input can be
_positional_ (an unnamed list of values) or _named_. Argument values are comma-separated, and named
arguments are pipe-separated.

For example, `{{Random: a, b, c |key=some, value |example }}` has five arguments: three positional
values `a`, `b`, `c`; a named `key` argument with values `some` and `value`; and a named `example`
argument with an empty value.

Some tokens recognise input arguments to change their output, which are documented in their own
sections. For example, the `Uppercase` token makes its input uppercase:
```js
"Entries": {
   "fri": "It's a beautiful {{uppercase: {{season}}}} day!" // It's a beautiful SPRING day!
}
```

### Global input arguments
Global input arguments are handled by Content Patcher itself, so they work with
all tokens (including mod-provided tokens). If you use multiple input arguments, they're applied
sequentially in left-to-right order.

#### `contains`
`contains` lets you search a token's values. It works with any token, regardless of its source.

It returns `true` or `false` depending on whether the token contains any of the given values. This
is mainly useful for logic in [conditions](#conditions):

```js
// player has blacksmith OR gemologist
"When": {
   "HasProfession": "Blacksmith, Gemologist"
}

// player has blacksmith AND gemologist
"When": {
   "HasProfession |contains=Blacksmith": "true",
   "HasProfession |contains=Gemologist": "true"
}

// NOT year 1
"When": {
   "Year |contains=1": "false"
}
```

This can also be used in placeholders. For example, this will load a different file depending on
whether the player has the `Gemologist` profession:
```js
{
    "Action": "EditImage",
    "Target": "Buildings/houses",
    "FromFile": "assets/gems-{{HasProfession |contains=Gemologist}}.png" // assets/gems-true.png
}
```

You can specify multiple values, in which case it returns whether _any_ of them match:
```js
// player has blacksmith OR gemologist
"When": {
   "HasProfession |contains=Blacksmith, Gemologist": "true"
}

// player has neither blacksmith NOR gemologist
"When": {
   "HasProfession |contains=Blacksmith, Gemologist": "false"
}
```

#### `valueAt`
The `valueAt` argument gets one value from a token at the given position (starting at zero for the
first value). If the index is outside the list, this returns an empty list.

This depends on the token's order, which you can check with the [`patch summary unsorted` console
command](troubleshooting.md#summary). Some tokens like `ChildNames` have a consistent order (which
will be documented in the info for each token); most others like `HasFlag` are listed in the order
they're defined in the game data, which may change from one save to the next.

For example:

<table>
  <tr>
    <th>token</th>
    <th>value</th>
  </tr>
  <tr>
    <td><code>{{ChildNames}}</code></td>
    <td><code>Angus, Bob, Carrie</code></td>
  </tr>
  <tr>
    <td><code>{{ChildNames |valueAt=0}}</code></td>
    <td><code>Angus</code></td>
  </tr>
  <tr>
    <td><code>{{ChildNames |valueAt=1}}</code></td>
    <td><code>Bob</code></td>
  </tr>
  <tr>
    <td><code>{{ChildNames |valueAt=2}}</code></td>
    <td><code>Carrie</code></td>
  </tr>
  <tr>
    <td><code>{{ChildNames |valueAt=3}}</code></td>
    <td><em>empty list</em></td>
  </tr>
</table>

You can use a negative index to get a value starting from the _end_ of the list, where -1 is
the last item. For example:

<table>
  <tr>
    <th>token</th>
    <th>value</th>
  </tr>
  <tr>
    <td><code>{{ChildNames}}</code></td>
    <td><code>Angus, Bob, Carrie</code></td>
  </tr>
    <td><code>{{ChildNames |valueAt=-1}}</code></td>
    <td><code>Carrie</code></td>
  </tr>
    <td><code>{{ChildNames |valueAt=-2}}</code></td>
    <td><code>Bob</code></td>
  </tr>
    <td><code>{{ChildNames |valueAt=-3}}</code></td>
    <td><code>Angus</code></td>
  </tr>
    <td><code>{{ChildNames |valueAt=-4}}</code></td>
    <td><em>empty list</em></td>
  </tr>
</table>

### Custom input value separator
By default input arguments are comma-separated, but sometimes it's useful to allow commas in the
input values. You can use the `inputSeparator` argument to use a different separator (which can be
one or multiple characters).

For example, this can allow commas in random dialogue:

```json
"Entries": {
   "fri": "{{Random: Hey, how are you? @@ Hey, what's up? |inputSeparator=@@}}"
```

**Note:** you should avoid the `{}|=:` characters in separators, even if they're technically valid.
The behavior when separators conflict with token syntax depends on implementation details that may
change from one Content Patcher version to the next.

## Randomization
### Overview
You can randomize values using the `Random` token:
```js
{
   "Action": "Load",
   "Target": "Characters/Abigail",
   "FromFile": "assets/abigail-{{Random:hood, jacket, raincoat}}.png"
}
```

And you can optionally use pinned keys to keep multiple `Random` tokens in sync (see below for more
info):
```js
{
   "Action": "Load",
   "Target": "Characters/Abigail",
   "FromFile": "assets/abigail-{{Random:hood, jacket, raincoat |key=outfit}}.png",
   "When": {
      "HasFile:assets/abigail-{{Random:hood, jacket, raincoat |key=outfit}}.png": true
   }
}
```

This token is dynamic and may behave in unexpected ways; see below to avoid surprises.

### Unique properties
`Random` tokens are...

<ol>
<li>

**Dynamic.** Random tokens rechoose when they're evaluated, generally when a new day starts. The
randomness is seeded with the game seed + in-game date + input string, so reloading the save won't
change which choices were made.

</li>
<li>

**Independent**. Each `Random` token changes separately. In particular:

* If a patch has multiple `Target` values, `Random` may have a different value for each target.
* If a `FromFile` field has a `Random` token, you can't just copy its value into a `HasFile` field
  to check if the file exists, since `Random` may return a different choice in each field.

To keep multiple `Random` tokens in sync, see _pinned keys_ below.
</li>
<li>

**Fair**. Each option has an equal chance of being chosen. To load the dice, just specify a value multiple
times. For example, 'red' is twice as likely as 'blue' in this patch:
```js
{
   "Action": "Load",
   "Target": "Characters/Abigail",
   "FromFile": "assets/abigail-{{Random:red, red, blue}}.png"
}
```

</li>
<li>

**Bounded** if the choices don't contain tokens. For example, you can use it in true/false contexts
if all the choices are 'true' or 'false', or numeric contexts if all the choices are numbers.

</li>
</ul>

### Update rate
A `Random` token changes its choices on day start by default. If you want randomization to change
within a day, you need to make two changes:

* Specify a [patch update rate](../author-guide.md#update-rate) so the patch itself updates more often.
* Use a [pinned key](#pinned-keys) to set the seed to a value which changes more often. For example,
  this would change every time the in-game time changes:
  ```
  {{Random: a, b, c |key={{Time}} }}
  ```
  Note that `{{Random}}` tokens with the same key will synchronize their values. You can make the
  key unique to avoid that:
  ```
  {{Random: a, b, c |key=Abigail portraits {{Time}} }}
  ```

### Pinned keys
<dl>
<dt>Basic pinned keys:</dt>
<dd>

If you need multiple `Random` tokens to make the same choices (e.g. to keep an NPC's portrait and
sprite in sync), you can specify a 'pinned key'. This is like a name for the random; every `Random`
token with the same pinned key will make the same choice. (Note that list order does matter.)

For example, this keeps Abigail's sprite and portrait in sync using `abigail-outfit` as the pinned
key:
```js
{
   "Action": "Load",
   "Target": "Characters/Abigail, Portraits/Abigail",
   "FromFile": "assets/{{Target}}-{{Random:hood, jacket, raincoat |key=abigail-outfit}}.png"
}
```

You can use tokens in a pinned key. For example, this synchronizes values separately for each NPC:
```js
{
   "Action": "Load",
   "Target": "Characters/Abigail, Portraits/Abigail, Characters/Haley, Portraits/Haley",
   "FromFile": "assets/{{Target}}-{{Random:hood, jacket, raincoat |key={{TargetWithoutPath}}-outfit}}.png"
}
```

<dt>Advanced pinned keys:</dt>
<dd>

The pinned key affects the internal random number used to make a choice, not the choice itself. You
can use it with `Random` tokens containing different values (even different numbers of values) for
more interesting features.

For example, this gives Abigail and Haley random outfits but ensures they never wear the same one:
```js
{
   "Action": "Load",
   "Target": "Characters/Abigail, Portraits/Abigail",
   "FromFile": "assets/{{Target}}-{{Random:hood, jacket, raincoat |key=outfit}}.png"
},
{
   "Action": "Load",
   "Target": "Characters/Haley, Portraits/Haley",
   "FromFile": "assets/{{Target}}-{{Random:jacket, raincoat, hood |key=outfit}}.png"
}
```

</dd>

<dt>Okay, I'm confused. What the heck are pinned keys?</dt>
<dd>

Without pinned keys, each token will randomly choose its own value:
```txt
{{Random: hood, jacket, raincoat}} = raincoat
{{Random: hood, jacket, raincoat}} = hood
{{Random: hood, jacket, raincoat}} = jacket
```

If they have the same pinned key, they'll always be in sync:
```txt
{{Random: hood, jacket, raincoat |key=outfit}} = hood
{{Random: hood, jacket, raincoat |key=outfit}} = hood
{{Random: hood, jacket, raincoat |key=outfit}} = hood
```

For basic cases, you just need to know that same options + same key = same value.

If you want to get fancy, then the way it works under the hood comes into play. Setting a pinned
key doesn't sync the choice, it syncs the _internal number_ used to make that choice:
```txt
{{Random: hood, jacket, raincoat |key=outfit}} = 217437 modulo 3 choices = index 0 = hood
{{Random: hood, jacket, raincoat |key=outfit}} = 217437 modulo 3 choices = index 0 = hood
{{Random: hood, jacket, raincoat |key=outfit}} = 217437 modulo 3 choices = index 0 = hood
```

You can use that in interesting ways. For example, shifting the values guarantees they'll never
choose the same value (since same index = different value):
```txt
{{Random: hood, jacket, raincoat |key=outfit}} = 217437 modulo 3 choices = index 0 = hood
{{Random: jacket, raincoat, hood |key=outfit}} = 217437 modulo 3 choices = index 0 = jacket
```
</dd>
</dl>

## Advanced
### Query expressions
A _query expression_ is an arbitrary set of arithmetic and logical expressions which can be
evaluated into a number, `true`/`false` value, or text.

#### Usage
Query expressions are evaluated using the `Query` token. It can be used as a placeholder or condition,
and can include nested tokens. Here's an example which includes all of those:
```js
{
   "Format": "2.7.0",
   "Changes": [
      {
         "Action": "EditData",
         "Target": "Characters/Dialogue/Abigail",
         "Entries": {
            "Mon": "Hard to imagine you only arrived {{query: {{DaysPlayed}} / 28}} months ago!"
         },
         "When": {
            "query: {{Hearts:Abigail}} >= 10": true
         }
      }
   ]
}
```

You can use text values in expressions if they're single-quoted (including tokens which return text):
```js
"Query: '{{Season}}' = 'spring'": true
```

Expressions are case-insensitive, including when comparing text values.

#### Caveats
Query expressions are very powerful, but you should be aware of the caveats:

* Query expressions have **very little validation**. An invalid expression generally won't show
  warnings ahead of time, it'll just fail when the patch is applied. Make sure to carefully test
  any content pack features which use expressions, and check new or edited expressions with
  [`patch parse`](troubleshooting.md#parse).
* Query expressions **evaluate the expanded text**. For example, if the player name contains a
  single-quote like `D'Artagnan`, then this expression will fail due to a syntax error:
  ```js
  "Query: '{{PlayerName}}' LIKE 'D*'": true // 'D'Artagnan' LIKE 'D*'
  ```
* Query expressions may return obscure or technical error messages when invalid.
* Query expressions may make your content pack harder to read and understand.

Consider using non-expression features instead where possible. For example:

<table>
<tr>
<th>With query expression</th>
<th>Without query expression</th>
</tr>
<tr>
<td>

```js
"When": {
   "Query: {{Time}} >= 0600 AND {{Time}} <= 1200": true
}
```

</td>
<td>

```js
"When": {
   "Time": "{{Range: 0600, 1200}}"
}
```

</td>
</tr>
</table>

#### Operators
The supported operators are listed below.

* Perform arithmetic on numeric values (like `5 + 5`):

  operator | effect
  -------- | ---------
  \+       | addition
  \-       | subtraction
  \*       | multiplication
  /        | division
  %        | modulus
  ()       | grouping

* Compare two values (like `5 < 10`):

  operator | effect
  -------- | ---------
  `<`      | less than
  `<=`     | less than or equal
  `>`      | more than
  `>=`     | more than or equal
  `=`      | equal
  `<>`     | not equal

* Combine expressions using logical operators:

  operator | effect
  -------- | ------
  `AND`    | both expressions are true, like `{{Time}} >= 0600 AND {{Time}} <= 1200`.
  `OR`     | one or both expressions are true, like `{{Time}} <= 1200 OR {{Time}} >= 2400`.
  `NOT`    | negate the following expression, like `NOT {{Time}} > 1200`.

* Group sub-expressions using `()` to avoid an ambiguous order of operations:

  ```js
  "Query: ({{Time}} >= 0600 AND {{Time}} <= 1200) OR {{Time}} > 2400": true
  ```

* Check whether a value is `IN` or `NOT IN` a list:

  ```js
  "Query: '{{spouse}}' IN ('Abigail', 'Leah', 'Maru')": true
  ```

* Check text against a prefix/postfix using the `LIKE` or `NOT LIKE` operator. The wildcard `*` can
  only be at the start/end of the string, and it can only be used with quoted text (e.g. `LIKE '1'` will work but `LIKE 1` will return an error).

  ```js
  "Query: '{{spouse}}' LIKE 'Abig*'": true
  ```

### Mod-provided tokens
SMAPI mods can add new tokens for content packs to use (see [_extensibility for modders_](../extensibility.md)),
which work just like normal Content Patcher tokens. For example, this patch uses a token from Json
Assets:
```js
{
   "Format": "2.7.0",
   "Changes": [
      {
         "Action": "EditData",
         "Target": "Data/NpcGiftTastes",
         "Entries": {
            "Universal_Love": "74 446 797 373 {{spacechase0.jsonAssets/ObjectId:item name}}",
         }
      }
   ]
}
```

To use a mod-provided token, at least one of these must be true:
* The mod which provides the token is a [required dependency](https://stardewvalleywiki.com/Modding:Modder_Guide/APIs/Manifest#Dependencies)
  of your content pack.
* Or the patch using the token has an immutable (i.e. not using any tokens) `HasMod` condition
  which lists the mod:
  ```js
  {
     "Format": "2.7.0",
     "Changes": [
        {
           "Action": "EditData",
           "Target": "Data/NpcGiftTastes",
           "Entries": {
              "Universal_Love": "74 446 797 373 {{spacechase0.jsonAssets/ObjectId:item name}}",
           },
           "When": {
              "HasMod": "spacechase0.jsonAssets"
           }
        }
     ]
  }
  ```

### Aliases
An _alias_ adds an optional alternate name for an existing token. This only affects your content
pack, and you can use both the alias name and the original token name. This is mostly useful for
custom tokens provided by other mods, which often have longer names.

Aliases are defined by the `AliasTokenNames` field in `content.json`, where each key is the
alternate name and the value is the original token name. For example:

```js
{
    "Format": "2.7.0",
    "AliasTokenNames": {
        "ItemID": "spacechase0.jsonAssets/ObjectId",
        "ItemSprite": "spacechase0.jsonAssets/ObjectSpriteSheetIndex"
    },
    "Changes": [
        {
            "Action": "EditData",
            "Target": "Data/NpcGiftTastes",
            "Entries": {
                "Universal_Love": "74 446 797 373 {{ItemID: pufferchick}}"
            }
        }
    ]
}
```

When using `Include` patches, aliases automatically work in the included files too.

The alias name can't match a global token or config token.

**Note:** this aliases the token _name_, but you can alias the token _value_ by using a [dynamic
token](#dynamic-tokens):

```js
{
    "Format": "2.7.0",
    "DynamicTokens": [
        {
            "Name": "PufferchickId",
            "Value": "{{spacechase0.jsonAssets/ObjectId: pufferchick}}"
        }
    ],
    "Changes": [
        {
            "Action": "EditData",
            "Target": "Data/NpcGiftTastes",
            "Entries": {
                "Universal_Love": "74 446 797 373 {{PufferchickId}}"
            }
        }
    ]
}
```

## Common values
These are predefined values used in tokens, linked from the token documentation above as needed.

### Location context
Some tokens let you choose which world area to get info for using an [input
argument](#input-arguments) like this:

example | meaning
------- | -------
`{{Weather}}`<br />`{{Weather: current}}` | Get weather for the current location.
`{{Weather: island}}` | Get the weather on Ginger Island.
`{{Weather: valley}}` | Get the weather in the valley.

The possible contexts are:

value     | meaning
--------- | -------
`current` | The context the current player is in. This is the default and doesn't need to be specified.
`island`  | Locations on the [Ginger Island](https://stardewvalleywiki.com/Ginger_Island).
`valley`  | Any other location.

### Target player
Some tokens let you choose which player's info to get using an [input argument](#input-arguments)
like this:

example                                  | meaning
---------------------------------------- | -------
`{{HasFlag}}`<br />`{{HasFlag: currentPlayer}}` | Get flags for the current player.
`{{HasFlag: hostPlayer}}`                | Get flags for the host player.
`{{HasFlag: currentPlayer, hostPlayer}}` | Get flags for the current _and_ host player(s).
`{{HasFlag: anyPlayer}}`                 | Get flags which any one or more players have.
`{{HasFlag: 3864039824286870457}}`       | Get flags for the player with the unique multiplayer ID `3864039824286870457`.

The possible player types are:

value | meaning
----- | -------
`currentPlayer` | The current player who has the mod installed.
`hostPlayer` | The player hosting the multiplayer world. This is the same as `currentPlayer` in single-player or if the current player is hosting.
`anyPlayer` | The combined values for all players, regardless of whether they're online.
_player ID_ | The unique multiplayer ID for a specific player, like `3864039824286870457`.

## See also
* [Author guide](../author-guide.md) for other actions and options
