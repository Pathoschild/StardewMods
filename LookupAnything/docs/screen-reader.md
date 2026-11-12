# Screen reader support

The lookup menu is read through Stardew Access. `Framework/LookupMenuScreenReader.cs` handles
navigation only; the spoken text comes from the fields themselves.

## Navigation

* The game's move up / move down keys step through the header and the fields.
* Move right on a field enters its entries (when it has two or more) or expands a collapsed field.
* Move left leaves the entries or collapses a field the player expanded.

## Where the text comes from

Every field implements `ICustomField.GetSpokenText()` (the whole value as one string) and
`ICustomField.GetSpokenEntries()` (the entries to step through, or none). `GenericField` speaks its
`Value` text pieces; fields that draw icons, colours, bars or checkboxes override both methods and
put what the drawing shows into words.

`Framework/SpokenText.cs` holds the shared helpers:

* `Clean` / `FromFormatted`: each line becomes a sentence, bullets and layout-only pieces (transparent
  indents, marker stars) are dropped.
* `Sentences`: joins parts so the screen reader pauses between them.
* `AndList`: "A, B and C".
* `Condition`: "text, yes right now" / "text, not right now" for conditions that depend on the current
  game state (fish spawn rules).
* `Progress`: "text, done" / "text, not done yet" for checkbox lists that track progress
  (upgrades, quests).
* `ItemWithCount`, `ItemWithQuality`, `Drop`: item text with count, quality, chance and conditions.

## What each visual cue is spoken as

* Faded or grey recipe: "you haven't learned this recipe yet".
* Faded monster drop: "might drop because you are wearing the Burglar's Ring"; crossed out: "will not drop this time".
* Crossed-out fish pond drop: "will not drop, an earlier guaranteed item comes first".
* Fish pond population group tick: "yes right now" / "not right now" after the group heading.
* Greyed fish locations in other seasons: the current season is marked "this season".
* Schedule colours: green stop is "happening now", grey stop is "already finished".
* Gift taste colours: green is "in your inventory", black "owned", grey "not owned"; bold is "not discovered yet".
* Hearts, locked hearts and the faint stardrop in friendship: hearts filled of total, hearts locked until dating, "stardrop not received yet".
* Dye swatches: colour name and strength (one to three swatches).
* Quality star on a recipe item: "gold quality Parsnip".
* Gold icon on a recipe price: "500 gold".

## Translations

All spoken wording is in `i18n/default.json` under the `screen-reader.` prefix. Other languages fall
back to the English text until translated.

## Not covered

Links (blue text that opens another lookup when clicked) are spoken as plain text. Following a link
by keyboard is not implemented.
