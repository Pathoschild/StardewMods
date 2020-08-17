← [README](../README.md) • [author guide](author-guide.md)

This document lists the tokens available in Content Patcher packs.

**See the [main README](../README.md) for other info**.

## Contents
* [Introduction](#introduction)
  * [Overview](#overview)
  * [Placeholders](#placeholders)
  * [Conditions](#conditions)
  * [Input arguments](#input-arguments)
* [Global tokens](#global-tokens)
  * [Date and weather](#date-and-weather)
  * [Player](#player)
  * [Relationships](#relationships)
  * [World](#world)
  * [Number manipulation](#number-manipulation)
  * [String manipulation](#string-manipulation)
  * [Metadata](#metadata)
  * [Field references](#field-references)
* [Global input arguments](#global-input-arguments)
  * [Token search](#token-search)
  * [Custom input value separator](#custom-input-value-separator)
* [Arithmetic](#arithmetic)
* [Randomization](#randomization)
* [Dynamic tokens](#dynamic-tokens)
* [Player config](#player-config)
* [Mod-provided tokens](#mod-provided-tokens)
* [Constants](#constants)
* [See also](#see-also)

## Introduction
### Overview
A **token** is a named container which has predefined values.

For example, `season` (the token) may contain `spring`, `summer`, `fall`, or `winter` (the value).
Here's a dialogue which includes the current season name:
```js
"Entries": {
   "fri": "It's a beautiful {{season}} day!" // It's a beautiful Spring day!
}
```

Tokens can be used as _placeholders_ in text (like the above example) or as patch _conditions_. You
can use [player config](#player-config), [global token values](#global-tokens), and
[dynamic token values](#dynamic-tokens) as tokens.

### Placeholders
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

### Conditions
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

### Input arguments
An **input argument** or **input** is a value you give to the token within the `{{...}}` braces.
Input can be _positional_ (an unnamed list of values) or _named_. Argument values are
comma-separated, and named arguments are pipe-separated.

For example, `{{Random: a, b, c |key=some, value |example }}` has five arguments: three positional
values `a`, `b`, `c`; a named `key` argument with values `some` and `value`; and a named `example`
argument with an empty value.

Some tokens recognise input arguments to change their output, which are documented in the sections
below. For example, the `Uppercase` token makes its input uppercase:
```js
"Entries": {
   "fri": "It's a beautiful {{uppercase: {{season}}}} day!" // It's a beautiful SPRING day!
}
```

There are also [**global input arguments**](#global-input-arguments) which are handled by Content
Patcher, so they work with any token.

## Global tokens
Global token values are defined by Content Patcher, so you can use them without doing anything else.

### Date and weather
<table>
<tr>
<th>condition</th>
<th>purpose</th>
</tr>

<tr valign="top">
<td>Day</td>
<td>The day of month. Possible values: any integer from 1 through 28.</td>
</tr>

<tr valign="top">
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
</tr>

<tr valign="top">
<td>DayOfWeek</td>
<td>

The day of week. Possible values: `Monday`, `Tuesday`, `Wednesday`, `Thursday`, `Friday`,
`Saturday`, and `Sunday`.

</td>
</tr>

<tr valign="top">
<td>DaysPlayed</td>
<td>The total number of in-game days played for the current save (starting from one when the first day starts).</td>
</tr>

<tr valign="top">
<td>Season</td>
<td>

The season name. Possible values: `Spring`, `Summer`, `Fall`, and `Winter`.

</td>
</tr>

<tr valign="top">
<td>Weather</td>
<td>

The weather type. Possible values:

value   | meaning
------- | -------
`Sun`   | The weather is sunny (including festival/wedding days). This is the default weather if no other value applies.
`Rain`  | Rain is falling, but without lightning.
`Storm` | Rain is falling with lightning.
`Snow`  | Snow is falling.
`Wind`  | The wind is blowing with visible debris (e.g. flower petals in spring and leaves in fall).

</td>
</tr>

<tr valign="top">
<td>Year</td>
<td>

The year number (like `1` or `2`).

</td>
</tr>
</table>

### Player
<table>
<tr>
<th>condition</th>
<th>purpose</th>
</tr>

<tr valign="top">
<td>HasConversationTopic</td>
<td>

The active [conversation topics](https://stardewvalleywiki.com/Modding:Dialogue#Conversation_topics)
for the current player (or the player specified with a [`PlayerType`](#playertype) argument).

</td>
</tr>

<tr valign="top">
<td>HasDialogueAnswer</td>
<td>

The [response IDs](https://stardewvalleywiki.com/Modding:Dialogue#Response_IDs) for answers to
question dialogues by the current player (or the player specified with a [`PlayerType`](#playertype)
argument).

</td>
</tr>

<tr valign="top">
<td>HasFlag</td>
<td>

The flags set for the current player (or the player specified with a [`PlayerType`](#playertype)
argument), including letters received and world state IDs.

Some useful flags:

flag | meaning
---- | -------
`abandonedJojaMartAccessible` | The [abandoned JojaMart](https://stardewvalleywiki.com/Bundles#Abandoned_JojaMart) is accessible.
`artifactFound` | The player has found at least one artifact.
`Beat_PK` | The player has beaten the Prairie King arcade game.
`beenToWoods` | The player has entered the Secret Woods at least once.
`beachBridgeFixed` | The bridge to access the second beach area is repaired.
`canReadJunimoText` | The player can read the language of Junimos (i.e. the plaques in the Community Center).
`ccIsComplete` | The player has completed the Community Center. Note that this isn't set reliably; see the `IsCommunityCenterComplete` and `IsJojaMartComplete` tokens instead.  See also flags for specific sections: `ccBoilerRoom`, `ccBulletin`, `ccCraftsRoom`, `ccFishTank`, `ccPantry`, and `ccVault`. The equivalent section flags for the Joja warehouse are `jojaBoilerRoom`, `jojaCraftsRoom`, `jojaFishTank`, `jojaPantry`, and `jojaVault`.
`ccMovieTheater`<br />`ccMovieTheaterJoja` | The movie theater has been constructed, either through the community path (only `ccMovieTheater` is set) or through Joja (both are set).
`doorUnlockAbigail` | The player has unlocked access to Abigail's room. See also flags for other NPCs: `doorUnlockAlex`, `doorUnlockCaroline`, `doorUnlockEmily`, `doorUnlockHaley`, `doorUnlockHarvey`, `doorUnlockJas`, `doorUnlockJodi`, `doorUnlockMarnie`, `doorUnlockMaru`, `doorUnlockPenny`, `doorUnlockPierre`, `doorUnlockRobin`, `doorUnlockSam`, `doorUnlockSebastian`, `doorUnlockVincent`.
`galaxySword` | The player has acquired the Galaxy Sword.
`geodeFound` | The player has found at least one geode.
`guildMember` | The player is a member of the Adventurer's Guild.
`jojaMember` | The player bought a Joja membership.
`JunimoKart` | The player has beaten the Junimo Kart arcade game.
`landslideDone` | The landside blocking access to the mines has been cleared.
`museumComplete` | The player has completed the Museum artifact collection.
`openedSewer` | The player has unlocked the sewers.
`qiChallengeComplete` | The player completed the Qi's Challenge quest by reaching level 25 in the Skull Cavern.

</td>
</tr>

<tr valign="top">
<td>HasProfession</td>
<td>

The [professions](https://stardewvalleywiki.com/Skills) learned by the current player (or the
player specified with a [`PlayerType`](#playertype) argument).

Possible values:

* Combat skill: `Acrobat`, `Brute`, `Defender`, `Desperado`, `Fighter`, `Scout`.
* Farming skill: `Agriculturist`, `Artisan`, `Coopmaster`, `Rancher`, `Shepherd`, `Tiller`.
* Fishing skill: `Angler`, `Fisher`, `Mariner`, `Pirate`, `Luremaster`, `Trapper`.
* Foraging skill: `Botanist`, `Forester`, `Gatherer`, `Lumberjack`, `Tapper`, `Tracker`.
* Mining skill: `Blacksmith`, `Excavator`, `Gemologist`, `Geologist`, `Miner`, `Prospector`.

Custom professions added by a mod are represented by their integer profession ID.

</td>
</tr>

<tr valign="top">
<td>HasReadLetter</td>
<td>

The letter IDs opened by the current player (or the player specified with a
[`PlayerType`](#playertype) argument). A letter is considered 'opened' if the letter UI was shown.

</td>
</tr>

<tr valign="top">
<td>HasSeenEvent</td>
<td>

The event IDs seen by the current player (or the player specified with a [`PlayerType`](#playertype)
argument), matching IDs in the `Data/Events` files.

You can use [Debug Mode](https://www.nexusmods.com/stardewvalley/mods/679) to see event IDs in-game.

</td>
</tr>

<tr valign="top">
<td>HasWalletItem</td>
<td>

The [special wallet items](https://stardewvalleywiki.com/Wallet) for the current player.

Possible values:

flag                       | meaning
-------------------------- | -------
`DwarvishTranslationGuide` | Unlocks speaking to the Dwarf.
`RustyKey`                 | Unlocks the sewers.
`ClubCard`                 | Unlocks the desert casino.
`SpecialCharm`             | Permanently increases daily luck.
`SkullKey`                 | Unlocks the Skull Cavern in the desert, and the Junimo Kart machine in the Stardrop Saloon.
`MagnifyingGlass`          | Unlocks the ability to find secret notes.
`DarkTalisman`             | Unlocks the Witch's Swamp.
`MagicInk`                 | Unlocks magical buildings through the Wizard, and the dark shrines in the Witch's Swamp.
`BearsKnowledge`           | Increases sell price of blackberries and salmonberries.
`SpringOnionMastery`       | Increases sell price of spring onions.

</td>
</tr>

<tr valign="top">
<td>IsMainPlayer</td>
<td>

Whether the player is the main player. Possible values: `true`, `false`.

</td>
</tr>

<tr valign="top">
<td>IsOutdoors</td>
<td>

Whether the player is outdoors. Possible values: `true`, `false`.

ℹ See _[update rate](author-guide.md#update-rate)_ before using this token.

</td>
</tr>

<tr valign="top">
<td>LocationName</td>
<td>

The internal name of the player's current location (visible using [Debug Mode](https://www.nexusmods.com/stardewvalley/mods/679)).

ℹ See _[update rate](author-guide.md#update-rate)_ before using this token.

</td>
</tr>

<tr valign="top">
<td>PlayerGender</td>
<td>

The player's gender. Possible values: `Female`, `Male`.

</td>
</tr>

<tr valign="top">
<td>PlayerName</td>
<td>The player's name.</td>
</tr>

<tr valign="top">
<td>PreferredPet</td>
<td>

The player's preferred pet. Possible values: `Cat`, `Dog`.

</td>
</tr>

<tr valign="top">
<td>SkillLevel</td>
<td>

The player's skill levels. You can specify the skill level as an input argument like this:

```js
"When": {
   "SkillLevel:Combat": "1, 2, 3" // combat level 1, 2, or 3
}
```

The valid skills are `Combat`, `Farming`, `Fishing`, `Foraging`, `Luck` (unused in the base game),
and `Mining`.

</td>
</tr>
</table>

### Relationships
<table>
<tr>
<th>condition</th>
<th>purpose</th>
</tr>

<tr valign="top">
<td>Hearts</td>
<td>

The player's heart level with a given NPC. You can specify the character name as an input argument
(using their English name regardless of translations), like this:

```js
"When": {
   "Hearts:Abigail": "10, 11, 12, 13"
}
```

**Note:** this is only available once the save is fully loaded, so it may not reliably affect
conditional map spawn logic.

</td>
</tr>

<tr valign="top">
<td>Relationship</td>
<td>

The player's relationship with a given NPC or player. You can specify the character name as part
of the key (using their English name regardless of translations), like this:

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

**Note:** this is only available once the save is fully loaded, so it may not reliably affect
conditional map spawn logic.

</td>
</tr>

<tr valign="top">
<td>Spouse</td>
<td>The player's spouse name (using their English name regardless of translations).</td>
</tr>

</table>

### World
<table>
<tr>
<th>condition</th>
<th>purpose</th>
</tr>

<tr valign="top">
<td>FarmCave</td>
<td>

The [farm cave](https://stardewvalleywiki.com/The_Cave) type. Possible values: `None`, `Bats`,
`Mushrooms`.

</td>
</tr>

<tr valign="top">
<td>FarmhouseUpgrade</td>
<td>

The [farmhouse upgrade level](https://stardewvalleywiki.com/Farmhouse#Upgrades). The normal values
are 0 (initial farmhouse), 1 (adds kitchen), 2 (add children's bedroom), and 3 (adds cellar). Mods
may add upgrade levels beyond that.

</td>
</tr>

<tr valign="top">
<td>FarmName</td>
<td>The name of the current farm.</td>
</tr>

<tr valign="top">
<td>FarmType</td>
<td>

The [farm type](https://stardewvalleywiki.com/The_Farm#Farm_Maps). Possible values: `Standard`,
`FourCorners`, `Forest`, `Hilltop`, `Riverland`, `Wilderness`, `Custom`.

</td>
</tr>

<tr valign="top">
<td>IsCommunityCenterComplete</td>
<td>

Whether all bundles in the community center are completed. Possible values: `true`, `false`.

</td>
</tr>

<tr valign="top">
<td>IsJojaMartComplete</td>
<td>

Whether the player bought a Joja membership and completed all Joja bundles. Possible values: `true`
 `false`.

</td>
</tr>

<tr valign="top">
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
</td>
</tr>

<tr valign="top">
<td>Pregnant</td>
<td>

The players or NPCs who are currently pregnant. This is a subset of `HavingChild` that only applies
to the female partner in heterosexual relationships. (Same-sex partners adopt a child instead.)

</td>
</tr>
</table>

### Number manipulation
<table>
<tr>
<th>condition</th>
<th>purpose</th>

<tr valign="top">
<td>Query</td>
<td>

Perform arbitrary arithmetic operations; see [_arithmetic_](#arithmetic) for more info.

</td>
</tr>

<tr valign="top">
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
</tr>

<tr valign="top">
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
`Round(2.5555, 2, down)` | `2.55` | Round `up` or `down` to the given number of fractional digits. This overrides the default [half rounded to even](https://en.wikipedia.org/wiki/Rounding#Round_half_to_even) behavior.

This is mainly useful in combination with the [`Query`](#query) token. For example, monster HP must
be a whole number, so this rounds the result of a calculation to the nearest whole number:
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
</tr>
</table>

### String manipulation
<table>
<tr>
<th>condition</th>
<th>purpose</th>

<tr valign="top">
<td>Lowercase<br />Uppercase</td>
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
</tr>
</table>

### Metadata
These tokens provide meta info about tokens, content pack files, installed mods, and the game state.

<table>
<tr>
<th>condition</th>
<th>purpose</th>
</tr>

<tr valign="top">
<td>HasMod</td>
<td>

The installed mod IDs (matching the `UniqueID` field in their `manifest.json`).

</td>
</tr>

<tr valign="top">
<td>HasFile</td>
<td>

Whether a file exists in the content pack folder. The file path must be specified as an input
argument. Returns `true` or `false`. For example:

```js
"When": {
  "HasFile:assets/{{season}}.png": "true"
}
```

If the input has commas like `HasFile: a, b.png`, they're treated as part of the filename.

</td>
</tr>

<tr valign="top">
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
</tr>

<tr valign="top">
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

</td>
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
</tr>

<tr valign="top">
<td>FromFile</td>
<td>

The patch's `FromFile` field value for the current asset. Path separators are normalized for the OS.
This is mainly useful for checking if the path exists:

```js
{
   "Action": "EditImage",
   "Target": "Characters/Abigail",
   "FromFile": "assets/Schedule/{{Season}}_{{Day}}.json",
   "When": {
      "HasFile:{{FromFile}}": true
   }
}
```

</td>
</tr>

<tr valign="top">
<td>Target</td>
<td>

The patch's `Target` field value for the current asset. Path separators are normalized for the OS.
This is mainly useful for patches which specify multiple targets:

```js
{
   "Action": "EditImage",
   "Target": "Characters/Abigail, Characters/Sam",
   "FromFile": "assets/{{Target}}.png" // assets/Characters/Abigail.png *or* assets/Characters/Sam.png
}
```

</td>
</tr>

<tr valign="top">
<td>TargetPathOnly</td>
<td>

Equivalent to `Target`, but only the part before the last path separator:

```js
{
   "Action": "EditImage",
   "Target": "Characters/Abigail, Portraits/Abigail",
   "FromFile": "assets/{{TargetPathOnly}}/recolor.png" // assets/Characters/recolor.png *or* assets/Portraits/recolor.png
}
```

</td>
</tr>

<tr valign="top">
<td>TargetWithoutPath</td>
<td>

Equivalent to `Target`, but only the part after the last path separator:

```js
{
   "Action": "EditImage",
   "Target": "Characters/Abigail, Characters/Sam",
   "FromFile": "assets/{{TargetWithoutPath}}.png" // assets/Abigail.png *or* assets/Sam.png
}
```

</td>
</tr>
</table>

## Global input arguments
Global [input arguments](#input-arguments) are handled by Content Patcher itself, so they work with
all tokens (including mod-provided tokens).

### Token search
The `contains` argument returns `true` or `false` depending on whether the token contains any of
the given values. This is mainly useful for logic in [conditions](#conditions):

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

## <span id="query"></span>Arithmetic
_See also [number manipulation tokens](#number-manipulation)_.

You can calculate mathematical expressions in patches using the `query` token (including over
tokens which return a number):
```js
{
   "Format": "1.17.0",
   "Changes": [
      {
         "Action": "EditData",
         "Target": "Characters/Dialogue/Abigail",
         "Entries": {
            "Mon": "You've played roughly {{query: {{DaysPlayed}} * 12}} minutes on this save!"
         }
      }
   ]
}
```

This also works in conditions:
```js
{
   "Action": "Load",
   "Target": "Characters/Abigail",
   "FromFile": "assets/abigail-friendly.png",
   "When": {
      "query: {{Hearts:Abigail}} + {{Hearts:Caroline}}": "20"
   }
}
```

These operators are supported:

symbol | operation
------ | ---------
\+     | addition
\-     | subtraction
\*     | multiplication
/      | division
%      | modulus
()     | grouping

**Caution:** the query syntax allows some operations that aren't documented here. These are
intended for future use, and may change without warning. Undocumented features shouldn't be used to
avoid breaking changes.

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
      "HasFile": "assets/abigail-{{Random:hood, jacket, raincoat |key=outfit}}.png"
   }
}
```

This token is dynamic and may behave in unexpected ways; see below to avoid surprises.

### Unique properties
`Random` tokens are...

<ol>
<li>

**Dynamic.** Random tokens rechoose each time they're evaluated, generally when a new day starts.

The randomness is seeded with the save ID and in-game date, so reloading the save won't change
which choices were made.

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

## Dynamic tokens
Dynamic tokens are defined in a `DynamicTokens` section of your `content.json` (see example below).
Each block in this section defines the value for a token using these fields:

field   | purpose
------- | -------
`Name`  | The name of the token to use for [tokens & condition](#introduction).
`Value` | The value(s) to set. This can be a comma-delimited value to give it multiple values. If _any_ block for a token name has multiple values, it will only be usable in conditions. This field supports [tokens](#introduction), including dynamic tokens defined before this entry.
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
   "Format": "1.17.0",
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

## Player config
You can let players configure your mod using a `config.json` file. Content Patcher will
automatically create and load the file, and you can use the config values as
[tokens & conditions](#introduction). Config fields are not case-sensitive.

To do this, you add a `ConfigSchema` section which defines your config fields and how to validate
them (see below for an example).
Available fields for each field:

   field               | meaning
   ------------------- | -------
   `AllowValues`       | _(optional.)_ The values the player can provide, as a comma-delimited string. If omitted, any value is allowed.<br />**Tip:** for a boolean flag, use `"true, false"`.
   `AllowBlank`        | _(optional.)_ Whether the field can be left blank. If false or omitted, blank fields will be replaced with the default value.
   `AllowMultiple`     | _(optional.)_ Whether the player can specify multiple comma-delimited values. Default false.
   `Default`           | _(optional unless `AllowBlank` is false.)_ The default values when the field is missing. Can contain multiple comma-delimited values if `AllowMultiple` is true. If omitted, blank fields are left blank.

For example: this `content.json` defines a `Material` config field and uses it to change which
patch is applied. See below for more details.

```js
{
   "Format": "1.17.0",
   "ConfigSchema": {
      "Material": {
         "AllowValues": "Wood, Metal",
         "Default": "Wood"
      }
   },
   "Changes": [
      // as a token
      {
         "Action": "Load",
         "Target": "LooseSprites/Billboard",
         "FromFile": "assets/material_{{material}}.png"
      },

      // as a condition
      {
         "Action": "Load",
         "Target": "LooseSprites/Billboard",
         "FromFile": "assets/material_wood.png",
         "When": {
            "Material": "Wood"
         }
      }
   ]
}
```

When you run the game, a `config.json` file will appear automatically with text like this:

```js
{
  "Material": "Wood"
}
```

Players can edit it to configure your content pack.

## Mod-provided tokens
SMAPI mods can add new tokens for content packs to use (see [_extensibility for modders_](extensibility.md)),
which work just like normal Content Patcher tokens. For example, this patch uses a token from Json
Assets:
```js
{
   "Format": "1.17.0",
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
     "Format": "1.17.0",
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

## Constants
These are predefined values used in tokens.

### `PlayerType`
value | meaning
----- | -------
`currentPlayer` | The current player who has the mod installed.
`hostPlayer` | The player hosting the multiplayer world. This is the same as `currentPlayer` in single-player or if the current player is hosting.

The player type can be specified as an [input argument](#input-arguments) for tokens that support it,
defaulting to the current player. For example:

example | meaning
------- | -------
`{{HasFlag}}` | Get flags for the current player.
`{{HasFlag: hostPlayer}}` | Get flags for the host player.
`{{HasFlag: currentPlayer, hostPlayer}}` | Get flags for the current _and_ host player(s).

## See also
* [README](../README.md) for other info
* [Author guide](author-guide.md)
