← [author guide](../author-guide.md)

Text operations let you change a text field based on its current value, instead of just setting the
new value. For example, you can append or prepend text without removing the current text.

They're set using the `TextOperations` field for an [`EditData`](action-editdata.md) or
[`EditMap`](action-editmap.md) patch.

**🌐 In other languages: [zh (中文)](../zh/author-guide/text-operations.md)**.

## Contents
* [Example](#example)
* [Format](#format)
  * [Common fields](#common-fields)
  * [`Append`](#append)
  * [`Prepend`](#prepend)
  * [`RemoveDelimited`](#removedelimited)
  * [`ReplaceDelimited`](#replacedelimited)
* [See also](#see-also)

## Example
Before we delve into the specifics, here's a quick example of how text operations work.

First, here's how you'd add an NPC gift taste **_without_** text operations:

```js
{
   "Action": "EditData",
   "Target": "Data/NPCGiftTastes",
   "Entries": {
      "Universal_Love": "74 446 797 373 279 127 128" // replaces current value
   }
}
```

This is pretty simple, but unfortunately it overwrites any previous values. That will remove any
changes from other mods or in future game updates. So instead, we can use the `Append`
operation to add new gift tastes without changing the other values:

```js
{
   "Action": "EditData",
   "Target": "Data/NPCGiftTastes",
   "TextOperations": [
      {
         "Operation": "Append",
         "Target": ["Entries", "Universal_Love"],
         "Value": "127 128",
         "Delimiter": " " // if the field isn't empty, add a space between the old & new text
      }
   ]
}
```

See the next section for more info on each operation type and their expected fields.

## Format
### Common fields
All text operations have these basic fields:

<table>
<tr>
<th>field</th>
<th>purpose</th>
</tr>
<tr>
<td><code>Operation</code></td>
<td>

The text operation to perform. See the sections below for a description of each operation.

</td>
</tr>
<tr>
<td><code>Target</code></td>
<td>

The specific text field to change, specified as a [breadcrumb path](https://en.wikipedia.org/wiki/Breadcrumb_navigation).
Each path value represents a field to navigate into. The possible path values depend on the patch
type; see the `TextOperations` field in the [`EditData`](action-editdata.md) or
[`EditMap`](action-editmap.md) docs for more info.

This field supports [tokens](../author-guide.md#tokens) and capitalisation doesn't matter.

</td>
</tr>
</table>

### `Append`
The `Append` operation adds text at the end of the current field, with an optional delimiter
between the old and new text.

This expects these fields:

<table>
<tr>
<th>field</th>
<th>purpose</th>
</tr>
<tr>
<td>&nbsp;</td>
<td>

See _[common fields](#common-fields)_ above.

</td>
</tr>
<tr>
<td><code>Value</code></td>
<td>

The text to append. Like most Content Patcher fields, **whitespace is trimmed from the start and
end**; use the `Delimiter` field if you need a space between the current and new values.

This field supports [tokens](../author-guide.md#tokens) and capitalisation doesn't matter.

</td>
</tr>
<tr>
<td><code>Delimiter</code></td>
<td>

_(Optional)_ The characters to add between the current and new text. If you don't specify the
delimiter, it'll default to `/` (most assets) or `^` (`Data/Achievements`).

For example, let's say the field contains `A/B` and you're appending `C`. Here's the result for
different delimiters:

delimiter          | result
------------------ | ------
_not specified_    | `A/B/C`
`"Delimiter": "/"` | `A/B/C`
`"Delimiter": " "` | `A/B C`
`"Delimiter": ""`  | `A/BC`

If the field is empty, the delimiter is ignored:

delimiter          | result
------------------ | ------
`"Delimiter": "/"` | `C`

</td>
</tr>
</table>

For example, this adds two item IDs to the list of universally loved gifts:

```js
{
   "Action": "EditData",
   "Target": "Data/NPCGiftTastes",
   "TextOperations": [
      {
         "Operation": "Append",
         "Target": ["Entries", "Universal_Love"],
         "Value": "127 128",
         "Delimiter": " "
      }
   ]
}
```

### `Prepend`
The `Prepend` operation adds text at the start of the current field, with an optional delimiter
between the old and new text.

This is exactly identical to the [`Append` operation](#append) otherwise.

### `RemoveDelimited`
The `RemoveDelimited` operation parses the target text into a set of values based on a delimiter,
then removes one or more values which match the given search text.

This expects these fields:

<table>
<tr>
<th>field</th>
<th>purpose</th>
</tr>
<tr>
<td>&nbsp;</td>
<td>

See _[common fields](#common-fields)_ above.

</td>
</tr>
<tr>
<td><code>Search</code></td>
<td>

The value to remove from the text. This must match the entire delimited value to remove, it won't
remove substrings within each delimited value.

This field supports [tokens](../author-guide.md#tokens), and capitalization **does** matter.

</td>
</tr>
<tr>
<td><code>Delimiter</code></td>
<td>

The characters which separate values within the target text.

For example, let's say the target text contains `A a/B/C`. Here's how that would be parsed with
different delimiters:

delimiter          | value 1 | value 2 | value 3
------------------ | ------- | ------- | -------
`"Delimiter": "/"` | `A a`   | `B`     | `C`
`"Delimiter": " "` | `A`     | `a/B/C` |

</td>
</tr>
<tr>
<td><code>ReplaceMode</code></td>
<td>

_(Optional)_ Which delimited values should be removed. The possible options are:

mode    | result
------- | ------
`First` | Remove the first value which matches the `Search`, and leave any others as-is.
`Last`  | Remove the last value which matches the `Search`, and leave any others as-is.
`All`   | Remove all values which match the `Search`.

Defaults to `All`.

</td>
</tr>
</table>

For example, this removes prismatic shard (item 74) from the list of universally loved gifts:

```js
{
   "Action": "EditData",
   "Target": "Data/NPCGiftTastes",
   "TextOperations": [
      {
         "Operation": "RemoveDelimited",
         "Target": ["Entries", "Universal_Love"],
         "Search": "74",
         "Delimiter": " "
      }
   ]
}
```

### `ReplaceDelimited`
The `ReplaceDelimited` operation parses the target text into a set of values based on a delimiter,
then replaces one or more values equal to the given search text with a new value.

This replaces delimited values, _not_ substrings within them.

This expects these fields:

<table>
<tr>
<th>field</th>
<th>purpose</th>
</tr>
<tr>
<td>&nbsp;</td>
<td>

See _[common fields](#common-fields)_ above.

</td>
</tr>
<tr>
<td><code>Search</code></td>
<td>

The value to replace in the text. This must match the entire delimited value to remove, it won't
remove substrings within each delimited value.

This field supports [tokens](../author-guide.md#tokens), and capitalization **does** matter.

</td>
</tr>
<tr>
<td><code>Value</code></td>
<td>

The text with which to replace the value.

This field supports [tokens](../author-guide.md#tokens), and capitalization doesn't matter. Like
most Content Patcher fields, whitespace is trimmed from the start and end.


</td>
</tr>
<tr>
<td><code>Delimiter</code></td>
<td>

The characters which separate values within the target text.

For example, let's say the target text contains `A a/B/C`. Here's how that would be parsed with
different delimiters:

delimiter          | value 1 | value 2 | value 3
------------------ | ------- | ------- | -------
`"Delimiter": "/"` | `A a`   | `B`     | `C`
`"Delimiter": " "` | `A`     | `a/B/C` |

</td>
</tr>
<tr>
<td><code>ReplaceMode</code></td>
<td>

_(Optional)_ Which delimited values should be replaced. The possible options are:

mode    | result
------- | ------
`First` | Replace the first value which matches the `Search`, and leave any others as-is.
`Last`  | Replace the last value which matches the `Search`, and leave any others as-is.
`All`   | Replace all values which match the `Search`.

Defaults to `All`.

</td>
</tr>
</table>

For example, this replaces Rabbit's Foot (item #446) in universal love gift tastes with pufferfish
(item #128):

```js
{
   "Action": "EditData",
   "Target": "Data/NPCGiftTastes",
   "TextOperations": [
      {
         "Operation": "ReplaceDelimited",
         "Target": ["Entries", "Universal_Love"],
         "Search": "446",
         "Value": "128",
         "Delimiter": " "
      }
   ]
}
```

## See also
* [Author guide](../author-guide.md) for other actions and options
