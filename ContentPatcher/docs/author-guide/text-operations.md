← [author guide](../author-guide.md)

Text operations is an advanced feature which let you change a text field based on its current value,
instead of just setting the new value. For example, you can append or prepend text to the current
value.

## Contents
* [Usage](#usage)
  * [Overview](#overview)
  * [Format](#format)
  * [Example](#example)
* [See also](#see-also)

## Usage
### Overview
Text operations let you change a text field based on its current value, instead of just setting the
new value. For example, you can append or prepend text to the current value. They're set using the
`TextOperations` field for an `EditData` or `EditMap` patch.

Text operations are only recommended when setting the value directly isn't suitable, since they
complicate your content pack and have less validation than other fields (since Content Patcher
can't precalculate the result ahead of time).

### Format
Each text operation is represented by a model with these fields:

<table>
<tr>
<th>field</th>
<th>purpose</th>
</tr>
<tr>
<td><code>Operation</code></td>
<td>

The text operation to perform. One of `Append` (add text after the current value), `Prepend` (add text before the current value),
`RemoveFirstOccurrence` (remove the first occurrence of the text in the current value),
`RemoveLastOccurrence` (remove the last occurrence of the text in the current value)
or `RemoveAllOccurrences` (remove all the occurrences of the text in the current value).

</td>
</tr>
<tr>
<td><code>Target</code></td>
<td>

The specific text field to change as a [breadcrumb path](https://en.wikipedia.org/wiki/Breadcrumb_navigation).
Each path value represents a field to navigate into. The possible path values depend on the patch
type; see the `TextOperations` field for [the patch action](../author-guide.md#actions) for more info.

This field supports [tokens](../author-guide.md#tokens) and capitalisation doesn't matter.

</td>
</tr>
<tr>
<td><code>Value</code></td>
<td>

The value to append or prepend. Just like all other Content Patcher fields, **whitespace is trimmed
from the start and end**; use the `Delimiter` field if you need a space between the current and new
values.

This field supports [tokens](../author-guide.md#tokens) and capitalisation doesn't matter.

</td>
</tr>
<tr>
<td><code>Delimiter</code></td>
<td>

_(optional / required with the `Remove*` text operations)_ Text that separates the entries in the value.
This is the text that will be added between the previous and inserted values when appending/prepending, or the text that will be used to separate entries in the value when removing.

</td>
</tr>
</table>
</dd>
</dl>

### Example
First, here's an example of adding a universally-loved gift **_without_** text operations. This
overwrites any previous value, which will break compatibility with other mods (or future game
updates) which add gift tastes:

```js
{
   "Action": "EditData",
   "Target": "Data/NPCGiftTastes",
   "Entries": {
      "Universal_Love": "74 446 797 373 279 127 128" // replaces current value
   }
}
```

Here's the same example, but appending to the existing entry using a text operation instead:

```js
{
   "Action": "EditData",
   "Target": "Data/NPCGiftTastes",
   "TextOperations": [
      {
         "Operation": "Append",
         "Target": ["Entries", "Universal_Love"],
         "Value": "127 128",
         "Delimiter": " " // if there are already values, add a space between them and the new ones
      }
   ]
}
```

Yet another example, this time removing a part of an existing entry using a text operation:

```js
{
   "Action": "EditData",
   "Target": "Data/NPCGiftTastes",
   "TextOperations": [
      {
         "Operation": "RemoveAllOccurrences",
         "Target": ["Entries", "Universal_Love"],
         "Value": "74", // Prismatic Shard is no longer a universal love
         "Delimiter": " " // the delimiter in this case is required
      }
   ]
}
```

## See also
* [Author guide](../author-guide.md) for other actions and options
