<<<<<<< HEAD
﻿← [author guide](../author-guide.md)

Text operations let you change a text field based on its current value, instead of just setting the
new value. For example, you can append or prepend text without removing the current text.

They're set using the `TextOperations` field for an [`EditData`](action-editdata.md) or
[`EditMap`](action-editmap.md) patch.

## Contents
* [Example](#example)
* [Format](#format)
  * [Common fields](#common-fields)
=======
﻿← [模组作者指南](../author-guide.md)

文本操作允许你在保留部分已有值的情况下更改某文本字段数据，而非覆盖整个值。例如你可以在已存在文本新增末尾值（Append）或新增头部值（Prepend）。

[`EditData`](action-editdata.md)和[`EditMap`](action-editmap.md)类型的补丁支持用`TextOperations`字段定义文本操作。

## 内容
* [示例](#example)
* [格式](#format)
  * [公共字段](#common-fields)
>>>>>>> c036414e8861adc25a9c6a2a7c1fac76501d9f05
  * [`Append`](#append)
  * [`Prepend`](#prepend)
  * [`RemoveDelimited`](#removedelimited)
  * [`ReplaceDelimited`](#replacedelimited)
<<<<<<< HEAD
* [See also](#see-also)

## Example
Before we delve into the specifics, here's a quick example of how text operations work.

First, here's how you'd add an NPC gift taste **_without_** text operations:
=======
* [参见](#see-also)

## 示例<a name="caveats"></a>
在详细解释细节前，让我们先看一个简单案例。

当你没有用文本操作时，这是添加NPC礼物喜好的方法：
>>>>>>> c036414e8861adc25a9c6a2a7c1fac76501d9f05

```js
{
   "Action": "EditData",
   "Target": "Data/NPCGiftTastes",
   "Entries": {
<<<<<<< HEAD
      "Universal_Love": "74 446 797 373 279 127 128" // replaces current value
=======
      "Universal_Love": "74 446 797 373 279 127 128" // 替换现有值
>>>>>>> c036414e8861adc25a9c6a2a7c1fac76501d9f05
   }
}
```

<<<<<<< HEAD
This is pretty simple, but unfortunately it overwrites any previous values. That will remove any
changes from other mods or in future game updates. So instead, we can use the `Append`
operation to add new gift tastes without changing the other values:
=======
此补丁很简单，但是它会覆盖掉原本的数据。其他模组的`Universal_Love`更改和未来的游戏更新都会被这个补丁抹除掉。所以我们应该用`Append`操作来实现保留原有数据同时添加新的礼物喜好：
>>>>>>> c036414e8861adc25a9c6a2a7c1fac76501d9f05

```js
{
   "Action": "EditData",
   "Target": "Data/NPCGiftTastes",
   "TextOperations": [
      {
         "Operation": "Append",
         "Target": ["Entries", "Universal_Love"],
         "Value": "127 128",
<<<<<<< HEAD
         "Delimiter": " " // if the field isn't empty, add a space between the old & new text
=======
         "Delimiter": " " // 如果原字段已经有数据，在原有值之后&新增值之前添加一个空格
>>>>>>> c036414e8861adc25a9c6a2a7c1fac76501d9f05
      }
   ]
}
```

<<<<<<< HEAD
See the next section for more info on each operation type and their expected fields.

## Format
### Common fields
All text operations have these basic fields:

<table>
<tr>
<th>field</th>
<th>purpose</th>
=======
其他操作类型和需要的字段详见一下分段。

## 格式<a name="format"></a>
### 公共字段<a name="common-fields"></a>

所有文本操作都有以下基本字段：

<table>
<tr>
<td>字段</td>
<td>用途</td>
>>>>>>> c036414e8861adc25a9c6a2a7c1fac76501d9f05
</tr>
<tr>
<td><code>Operation</code></td>
<td>

<<<<<<< HEAD
The text operation to perform. See the sections below for a description of each operation.
=======
需执行的文本操作。详见每个操作的分段。
>>>>>>> c036414e8861adc25a9c6a2a7c1fac76501d9f05

</td>
</tr>
<tr>
<td><code>Target</code></td>
<td>

<<<<<<< HEAD
The specific text field to change, specified as a [breadcrumb path](https://en.wikipedia.org/wiki/Breadcrumb_navigation).
Each path value represents a field to navigate into. The possible path values depend on the patch
type; see the `TextOperations` field in the [`EditData`](action-editdata.md) or
[`EditMap`](action-editmap.md) docs for more info.

This field supports [tokens](../author-guide.md#tokens) and capitalisation doesn't matter.
=======
需更改的文本，以[面包屑路径](https://en.wikipedia.org/wiki/Breadcrumb_navigation)格式表示。

每一个路径值代表要导航到的一个字段。可使用的字段根据补丁类型会变化，详见[`EditData`](action-editdata.md)和[`EditMap`](action-editmap.md)的文档。

该字段支持[tokens](../author-guide.md#tokens)，不区分大小写。
>>>>>>> c036414e8861adc25a9c6a2a7c1fac76501d9f05

</td>
</tr>
</table>

<<<<<<< HEAD
### `Append`
The `Append` operation adds text at the end of the current field, with an optional delimiter
between the old and new text.

This expects these fields:

<table>
<tr>
<th>field</th>
<th>purpose</th>
=======
### `Append` <a name="append"></a>
`Append`操作将文本新增到原字段的末尾，并可在原值和新值之间添加分割定界符（`Delimiter`）。

所需要的字段包括：

<table>
<tr>
<th>字段</th>
<th>用途</th>
>>>>>>> c036414e8861adc25a9c6a2a7c1fac76501d9f05
</tr>
<tr>
<td>&nbsp;</td>
<td>

<<<<<<< HEAD
See _[common fields](#common-fields)_ above.
=======
以上的 _[公共字段](#common-fields)_.
>>>>>>> c036414e8861adc25a9c6a2a7c1fac76501d9f05

</td>
</tr>
<tr>
<td><code>Value</code></td>
<td>

<<<<<<< HEAD
The text to append. Like most Content Patcher fields, **whitespace is trimmed from the start and
end**; use the `Delimiter` field if you need a space between the current and new values.

This field supports [tokens](../author-guide.md#tokens) and capitalisation doesn't matter.
=======
需新增的文本。和大部分Content Patcher的字段一样，开头和末尾的空格字符会被删除；如果你需要在原有值和新增值之间添加空格，请用`Delimiter`字段。

该字段支持[tokens](../author-guide.md#tokens)，不区分大小写。

>>>>>>> c036414e8861adc25a9c6a2a7c1fac76501d9f05

</td>
</tr>
<tr>
<td><code>Delimiter</code></td>
<td>

<<<<<<< HEAD
_(Optional)_ The characters to add between the current and new text. If you don't specify the
delimiter, it'll default to `/` (most assets) or `^` (`Data/Achievements`).

For example, let's say the field contains `A/B` and you're appending `C`. Here's the result for
different delimiters:

delimiter          | result
------------------ | ------
_not specified_    | `A/B/C`
=======
_(可选)_ 需在原有值之后&新增值之前添加的文本。如果没有填写，默认使用`/`（大部分素材）或`^`(`Data/Achievements`)。

例如，假设字段原有值为`A/B`而你想追加`C`，以下为不同分割定界符会产生的结果：

分割定界符          | 结果
------------------ | ------
_未填写_    | `A/B/C`
>>>>>>> c036414e8861adc25a9c6a2a7c1fac76501d9f05
`"Delimiter": "/"` | `A/B/C`
`"Delimiter": " "` | `A/B C`
`"Delimiter": ""`  | `A/BC`

<<<<<<< HEAD
If the field is empty, the delimiter is ignored:

delimiter          | result
=======
假设字段原有值为空，分割定界符不生效：

分割定界符          | 结果
>>>>>>> c036414e8861adc25a9c6a2a7c1fac76501d9f05
------------------ | ------
`"Delimiter": "/"` | `C`

</td>
</tr>
</table>

<<<<<<< HEAD
For example, this adds two item IDs to the list of universally loved gifts:
=======
此例子把两个新物品ID添加到NPC的普遍喜好列表：
>>>>>>> c036414e8861adc25a9c6a2a7c1fac76501d9f05

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

<<<<<<< HEAD
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
=======
### `Prepend` <a name="prepend"></a>

`Prepend`操作将文本新增到原字段的开头，并可在新值和原值之间添加分割定界符（`Delimiter`）。

除了添加到开头的特性以外，此操作和[`Append`操作](#append)一模一样。

### `RemoveDelimited` <a name="removedelimited"></a>

`RemoveDelimited`操作把原文本值用分割定界符分成一组值，然后剔除等于搜索文本的值。

所需要的字段包括：

<table>
<tr>
<th>字段</th>
<th>用途</th>
>>>>>>> c036414e8861adc25a9c6a2a7c1fac76501d9f05
</tr>
<tr>
<td>&nbsp;</td>
<td>

<<<<<<< HEAD
See _[common fields](#common-fields)_ above.
=======
以上的 _[公共字段](#common-fields)_.
>>>>>>> c036414e8861adc25a9c6a2a7c1fac76501d9f05

</td>
</tr>
<tr>
<td><code>Search</code></td>
<td>

<<<<<<< HEAD
The value to remove from the text. This must match the entire delimited value to remove, it won't
remove substrings within each delimited value.

This field supports [tokens](../author-guide.md#tokens), and capitalization **does** matter.
=======
需从原文本中剔除的值。这必须等于整个分割后的值，不会替换部分值。

该字段支持[tokens](../author-guide.md#tokens)，**区分大小写**。
>>>>>>> c036414e8861adc25a9c6a2a7c1fac76501d9f05

</td>
</tr>
<tr>
<td><code>Delimiter</code></td>
<td>

<<<<<<< HEAD
The characters which separate values within the target text.

For example, let's say the target text contains `A a/B/C`. Here's how that would be parsed with
different delimiters:

delimiter          | value 1 | value 2 | value 3
=======
分割原文本所用的字符。

例如，假设原文本为`A a/B/C`，以下为不同分割定界符会产生的结果：

割定界符          | 值1 | 值2 | 值3
>>>>>>> c036414e8861adc25a9c6a2a7c1fac76501d9f05
------------------ | ------- | ------- | -------
`"Delimiter": "/"` | `A a`   | `B`     | `C`
`"Delimiter": " "` | `A`     | `a/B/C` |

</td>
</tr>
<tr>
<td><code>ReplaceMode</code></td>
<td>

<<<<<<< HEAD
_(Optional)_ Which delimited values should be removed. The possible options are:

mode    | result
------- | ------
`First` | Remove the first value which matches the `Search`, and leave any others as-is.
`Last`  | Remove the last value which matches the `Search`, and leave any others as-is.
`All`   | Remove all values which match the `Search`.

Defaults to `All`.
=======
_(可选)_ 指定需剔除的值，可填写：

模式    | 结果
------- | ------
`First` | 剔除第一个等于`Search`的值，其他值不变。
`Last`  | 剔除最后一个等于`Search`的值，其他值不变。
`All`   | 剔除所有等于`Search`的值。

默认为`All`.
>>>>>>> c036414e8861adc25a9c6a2a7c1fac76501d9f05

</td>
</tr>
</table>

<<<<<<< HEAD
For example, this removes prismatic shard (item 74) from the list of universally loved gifts:
=======
例如，此补丁将五彩碎片（物品ID 74）从普遍喜好中剔除：
>>>>>>> c036414e8861adc25a9c6a2a7c1fac76501d9f05

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

<<<<<<< HEAD
### `ReplaceDelimited`
The `ReplaceDelimited` operation parses the target text into a set of values based on a delimiter,
then replaces one or more values equal to the given search text with a new value.

This replaces delimited values, _not_ substrings within them.

This expects these fields:

<table>
<tr>
<th>field</th>
<th>purpose</th>
=======
### `ReplaceDelimited` <a name="replacedelimited"></a>

`RemoveDelimited`操作把原文本值用分割定界符分成一组值，然后替换等于搜索文本的值。

这只替换分割后的值，_不替换_ 部分文本。

所需要的字段包括：

<table>
<tr>
<th>字段</th>
<th>用途</th>
>>>>>>> c036414e8861adc25a9c6a2a7c1fac76501d9f05
</tr>
<tr>
<td>&nbsp;</td>
<td>

<<<<<<< HEAD
See _[common fields](#common-fields)_ above.
=======
以上的 _[公共字段](#common-fields)_.
>>>>>>> c036414e8861adc25a9c6a2a7c1fac76501d9f05

</td>
</tr>
<tr>
<td><code>Search</code></td>
<td>

<<<<<<< HEAD
The value to replace in the text. This must match the entire delimited value to remove, it won't
remove substrings within each delimited value.

This field supports [tokens](../author-guide.md#tokens), and capitalization **does** matter.
=======
需从原文本中替换掉的值。这必须等于整个分割后的值，不会替换部分值。

该字段支持[tokens](../author-guide.md#tokens)，**区分大小写**。
>>>>>>> c036414e8861adc25a9c6a2a7c1fac76501d9f05

</td>
</tr>
<tr>
<td><code>Value</code></td>
<td>

<<<<<<< HEAD
The text with which to replace the value.

This field supports [tokens](../author-guide.md#tokens), and capitalization doesn't matter. Like
most Content Patcher fields, whitespace is trimmed from the start and end.

=======
需替换到文本里的值。

该字段支持[tokens](../author-guide.md#tokens)，不区分大小写。和大部分Content Patcher的字段一样，开头和末尾的空格字符会被删除。
>>>>>>> c036414e8861adc25a9c6a2a7c1fac76501d9f05

</td>
</tr>
<tr>
<td><code>Delimiter</code></td>
<td>

<<<<<<< HEAD
The characters which separate values within the target text.

For example, let's say the target text contains `A a/B/C`. Here's how that would be parsed with
different delimiters:

delimiter          | value 1 | value 2 | value 3
=======
分割原文本所用的字符。

例如，假设原文本为`A a/B/C`，以下为不同分割定界符会产生的结果：

割定界符          | 值1 | 值2 | 值3
>>>>>>> c036414e8861adc25a9c6a2a7c1fac76501d9f05
------------------ | ------- | ------- | -------
`"Delimiter": "/"` | `A a`   | `B`     | `C`
`"Delimiter": " "` | `A`     | `a/B/C` |

</td>
</tr>
<tr>
<td><code>ReplaceMode</code></td>
<td>

<<<<<<< HEAD
_(Optional)_ Which delimited values should be replaced. The possible options are:

mode    | result
------- | ------
`First` | Replace the first value which matches the `Search`, and leave any others as-is.
`Last`  | Replace the last value which matches the `Search`, and leave any others as-is.
`All`   | Replace all values which match the `Search`.

Defaults to `All`.
=======
_(可选)_ 指定需替换的值，可填写：

模式    | 结果
------- | ------
`First` | 替换第一个等于`Search`的值，其他值不变。
`Last`  | 替换最后一个等于`Search`的值，其他值不变。
`All`   | 替换所有等于`Search`的值。

默认为`All`.
>>>>>>> c036414e8861adc25a9c6a2a7c1fac76501d9f05

</td>
</tr>
</table>

<<<<<<< HEAD
For example, this replaces Rabbit's Foot (item #446) in universal love gift tastes with pufferfish
(item #128):
=======
例如，此补丁将遍喜好中的兔子的脚（物品ID 446）替代为河豚 （物品ID 128）：

>>>>>>> c036414e8861adc25a9c6a2a7c1fac76501d9f05

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

<<<<<<< HEAD
## See also
* [Author guide](../author-guide.md) for other actions and options
=======
## 参见 <a name="see-also"></a>
* 其他操作和选项请参考[模组作者指南](../author-guide.md)
>>>>>>> c036414e8861adc25a9c6a2a7c1fac76501d9f05
