← [模组作者指南](../author-guide.md)

文本操作允许您在保留部分当前值的情况下更改某文本字段数据，而非覆盖整个值。例如您可以在不删除当前文本的情况下新增末尾值（Append）或新增头部值（Prepend）。

[`EditData`](action-editdata.md) 和 [`EditMap`](action-editmap.md) 类型的补丁均支持使用 `TextOperations` 字段定义文本操作。

**🌐 其他语言：[en (English)](../../author-guide/text-operations.md)。**

## 目录
* [示例](#example)
* [格式](#format)
  * [公共字段](#common-fields)
  * [`Append`](#append)
  * [`Prepend`](#prepend)
  * [`RemoveDelimited`](#removedelimited)
  * [`ReplaceDelimited`](#replacedelimited)
* [参见](#see-also)

## 示例<a name="example"></a>
在深入具体细节之前，让我们先看一个简单案例。

当您不使用文本操作时，这是添加 NPC 普遍最爱的礼物的方法：
```js
{
   "Action": "EditData",
   "Target": "Data/NPCGiftTastes",
   "Entries": {
      "Universal_Love": "74 446 797 373 279 127 128" // 替换现有值
   }
}
```

这个补丁很简单，但是它会覆盖掉原本的数据。这也使得由其他模组或未来游戏更新中对 `Universal_Love` 的任何更改都会被这个补丁抹除掉。所以我们应当使用 `Append` 操作，在不改变原有数据值的情况下添加新的普遍最爱的礼物：

```js
{
   "Action": "EditData",
   "Target": "Data/NPCGiftTastes",
   "TextOperations": [
      {
         "Operation": "Append",
         "Target": ["Entries", "Universal_Love"],
         "Value": "127 128",
         "Delimiter": " " // 如果原字段已经有数据，在原有值之后&新增值之前添加一个空格
      }
   ]
}
```

请参阅下章节以了解更多关于每种操作类型及其预期字段的信息。

## 格式<a name="format"></a>
### 公共字段<a name="common-fields"></a>

所有文本操作都包含以下基本字段：

<table>
<tr>
<td>字段</td>
<td>用途</td>
</tr>
<tr>
<td><code>Operation</code></td>
<td>

需执行的文本操作。请参阅下文了解每种操作的信息。

</td>
</tr>
<tr>
<td><code>Target</code></td>
<td>

要更改的具体文本字段，以[面包屑路径](https://zh.wikipedia.org/wiki/面包屑导航)格式表示。每一个路径值代表要导航到的一个字段，可使用的字段取决于补丁的类型，请参阅 [`EditData`](action-editdata.md) 和 [`EditMap`](action-editmap.md) 文档中对 `TextOperations` 字段的描述以获取更多信息。

该字段支持[令牌](../author-guide.md#tokens)，不区分大小写。

</td>
</tr>
</table>

### `Append` <a name="append"></a>
`Append` 操作将文本新增到原字符串的末尾，并可在原值和新值之间添加分割定界符（`Delimiter`）。

所需要的字段包括：

<table>
<tr>
<th>字段</th>
<th>用途</th>
</tr>
<tr>
<td>&nbsp;</td>
<td>

以上的[公共字段](#common-fields)

</td>
</tr>
<tr>
<td><code>Value</code></td>
<td>

需新增的文本。和大部分 Content Patcher 的字段一样，开头和末尾的空格字符会被删除，如果您需要在原有值和新增值之间添加空格，请使用 `Delimiter` 字段。

该字段支持[令牌](../author-guide.md#tokens)，不区分大小写。

</td>
</tr>
<tr>
<td><code>Delimiter</code></td>
<td>

（可选）原有值之后/新增值之前添加的文本。如果没有填写，则默认使用 `/`（对于绝大部分素材）或 `^`(对于 `Data/Achievements`)。

例如，假设字段原有值为 `A/B`，而您想追加 `C`，那么对于不同的分隔符，会产生以下不同结果：

分割定界符          | 结果
------------------ | ------
空                 | `A/B/C`
`"Delimiter": "/"` | `A/B/C`
`"Delimiter": " "` | `A/B C`
`"Delimiter": ""`  | `A/BC`

如果字段原有值为空字符串，则分割定界符将不生效：

分割定界符          | 结果
------------------ | ------
`"Delimiter": "/"` | `C`

</td>
</tr>
</table>

此示例将两个新物品 ID 添加到 NPC 的普遍最爱的礼物列表中：
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

### `Prepend` <a name="prepend"></a>
`Prepend` 操作将文本新增到原字符串的开头，并可在新值和原值之间添加分割定界符（`Delimiter`）。

此操作的其他属性和 [`Append` 操作](#append)完全相同。

### `RemoveDelimited` <a name="removedelimited"></a>

`RemoveDelimited` 操作根据给定的分隔符将目标文本解析为一组值，然后移除一个或多个与给定搜索文本匹配的值。

所需要的字段包括：

<table>
<tr>
<th>字段</th>
<th>用途</th>
</tr>
<tr>
<td>&nbsp;</td>
<td>

以上的[公共字段](#common-fields)

</td>
</tr>
<tr>
<td><code>Search</code></td>
<td>

需从原文本中移除的值。必须完全匹配分隔后的值才能移除，不会移除每个分隔值内的子字符串。

该字段支持[令牌](../author-guide.md#tokens)，且**区分大小写** 。

</td>
</tr>
<tr>
<td><code>Delimiter</code></td>
<td>

分割原文本所用的字符。

例如，假设原文本为`A a/B/C`，以下为不同分割定界符会产生的结果：

割定界符          | 值1 | 值2 | 值3
------------------ | ------- | ------- | -------
`"Delimiter": "/"` | `A a`   | `B`     | `C`
`"Delimiter": " "` | `A`     | `a/B/C` |

</td>
</tr>
<tr>
<td><code>ReplaceMode</code></td>
<td>

（可选）应移除哪些分隔值。支持的选项是：

模式    | 结果
------- | ------
`First` | 移除第一个等于 `Search` 的值，其他值不变。
`Last`  | 移除最后一个等于 `Search` 的值，其他值不变。
`All`   | 移除所有等于 `Search` 的值。

默认为 `All`。

</td>
</tr>
</table>

例如，此补丁将五彩碎片（物品 ID 为 74）从普遍最爱的礼物中移除：

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

### `ReplaceDelimited` <a name="replacedelimited"></a>

`RemoveDelimited`操作把原文本值用分割定界符分成一组值，然后替换等于搜索文本的值。

这只替换分割后的值，不替换部分文本。

所需要的字段包括：

<table>
<tr>
<th>字段</th>
<th>用途</th>
</tr>
<tr>
<td>&nbsp;</td>
<td>

以上的[公共字段](#common-fields)

</td>
</tr>
<tr>
<td><code>Search</code></td>
<td>

需从原文本中替换掉的值。必须完全匹配分隔后的值才能替换，不会替换每个分隔值内的子字符串。

该字段支持[令牌](../author-guide.md#tokens)，且**区分大小写** 。

</td>
</tr>
<tr>
<td><code>Value</code></td>
<td>

用于替换该值的文本。

该字段支持[令牌](../author-guide.md#tokens)，不区分大小写。和大部分 Content Patcher 的字段一样，开头和末尾的空格字符会被删除。

</td>
</tr>
<tr>
<td><code>Delimiter</code></td>
<td>

分割原文本所用的字符。

例如，假设原文本为`A a/B/C`，以下为不同分割定界符会产生的结果：

割定界符          | 值1 | 值2 | 值3
------------------ | ------- | ------- | -------
`"Delimiter": "/"` | `A a`   | `B`     | `C`
`"Delimiter": " "` | `A`     | `a/B/C` |

</td>
</tr>
<tr>
<td><code>ReplaceMode</code></td>
<td>

（可选）应替换哪些分隔值。支持的选项是：

模式    | 结果
------- | ------
`First` | 替换第一个等于 `Search` 的值，其他值不变。
`Last`  | 替换最后一个等于 `Search` 的值，其他值不变。
`All`   | 替换所有等于 `Search` 的值。

默认为 `All`。

</td>
</tr>
</table>

例如，此补丁将普遍最爱的礼物中的兔子的脚（物品 ID 为 446）替换为河豚（物品 ID 为 128）：
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

## 参见 <a name="see-also"></a>
* 其他操作和选项请参考[模组作者指南](../author-guide.md)
