← [模组作者指南](../author-guide.md)

文本操作允许你在保留部分已有值的情况下更改某文本字段数据，而非覆盖整个值。例如你可以在已存在文本新增末尾值（Append）或新增头部值（Prepend）。

[`EditData`](action-editdata.md)和[`EditMap`](action-editmap.md)类型的补丁支持用`TextOperations`字段定义文本操作。

**🌐 其他语言： [en (English)](../../author-guide/text-operations.md)。**

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
在详细解释细节前，让我们先看一个简单案例。

当你没有用文本操作时，这是添加NPC礼物喜好的方法：

```js
{
   "Action": "EditData",
   "Target": "Data/NPCGiftTastes",
   "Entries": {
      "Universal_Love": "74 446 797 373 279 127 128" // 替换现有值
   }
}
```

此补丁很简单，但是它会覆盖掉原本的数据。其他模组的`Universal_Love`更改和未来的游戏更新都会被这个补丁抹除掉。所以我们应该用`Append`操作来实现保留原有数据同时添加新的礼物喜好：

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

其他操作类型和需要的字段详见一下分段。

## 格式<a name="format"></a>
### 公共字段<a name="common-fields"></a>

所有文本操作都有以下基本字段：

<table>
<tr>
<td>字段</td>
<td>用途</td>
</tr>
<tr>
<td><code>Operation</code></td>
<td>

需执行的文本操作。详见每个操作的分段。

</td>
</tr>
<tr>
<td><code>Target</code></td>
<td>

需更改的文本，以[面包屑路径](https://zh.wikipedia.org/wiki/面包屑导航)格式表示。

每一个路径值代表要导航到的一个字段。可使用的字段根据补丁类型会变化，详见[`EditData`](action-editdata.md)和[`EditMap`](action-editmap.md)的文档。

该字段支持[令牌](../author-guide.md#tokens)，不区分大小写。

</td>
</tr>
</table>

### `Append` <a name="append"></a>
`Append`操作将文本新增到原字段的末尾，并可在原值和新值之间添加分割定界符（`Delimiter`）。

所需要的字段包括：

<table>
<tr>
<th>字段</th>
<th>用途</th>
</tr>
<tr>
<td>&nbsp;</td>
<td>

以上的 _[公共字段](#common-fields)_.

</td>
</tr>
<tr>
<td><code>Value</code></td>
<td>

需新增的文本。和大部分Content Patcher的字段一样，开头和末尾的空格字符会被删除；如果你需要在原有值和新增值之间添加空格，请用`Delimiter`字段。

该字段支持[令牌](../author-guide.md#tokens)，不区分大小写。


</td>
</tr>
<tr>
<td><code>Delimiter</code></td>
<td>

_(可选)_ 需在原有值之后&新增值之前添加的文本。如果没有填写，默认使用`/`（大部分素材）或`^`(`Data/Achievements`)。

例如，假设字段原有值为`A/B`而你想追加`C`，以下为不同分割定界符会产生的结果：

分割定界符          | 结果
------------------ | ------
_未填写_    | `A/B/C`
`"Delimiter": "/"` | `A/B/C`
`"Delimiter": " "` | `A/B C`
`"Delimiter": ""`  | `A/BC`

假设字段原有值为空，分割定界符不生效：

分割定界符          | 结果
------------------ | ------
`"Delimiter": "/"` | `C`

</td>
</tr>
</table>

此例子把两个新物品ID添加到NPC的普遍喜好列表：

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

`Prepend`操作将文本新增到原字段的开头，并可在新值和原值之间添加分割定界符（`Delimiter`）。

除了添加到开头的特性以外，此操作和[`Append`操作](#append)一模一样。

### `RemoveDelimited` <a name="removedelimited"></a>

`RemoveDelimited`操作把原文本值用分割定界符分成一组值，然后剔除等于搜索文本的值。

所需要的字段包括：

<table>
<tr>
<th>字段</th>
<th>用途</th>
</tr>
<tr>
<td>&nbsp;</td>
<td>

以上的 _[公共字段](#common-fields)_.

</td>
</tr>
<tr>
<td><code>Search</code></td>
<td>

需从原文本中剔除的值。这必须等于整个分割后的值，不会替换部分值。

该字段支持[令牌](../author-guide.md#tokens)，**区分大小写** 。

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

_(可选)_ 指定需剔除的值，可填写：

模式    | 结果
------- | ------
`First` | 剔除第一个等于`Search`的值，其他值不变。
`Last`  | 剔除最后一个等于`Search`的值，其他值不变。
`All`   | 剔除所有等于`Search`的值。

默认为`All`.

</td>
</tr>
</table>

例如，此补丁将五彩碎片（物品ID 74）从普遍喜好中剔除：

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

这只替换分割后的值，_不替换_ 部分文本。

所需要的字段包括：

<table>
<tr>
<th>字段</th>
<th>用途</th>
</tr>
<tr>
<td>&nbsp;</td>
<td>

以上的 _[公共字段](#common-fields)_.

</td>
</tr>
<tr>
<td><code>Search</code></td>
<td>

需从原文本中替换掉的值。这必须等于整个分割后的值，不会替换部分值。

该字段支持[令牌](../author-guide.md#tokens)，**区分大小写** 。

</td>
</tr>
<tr>
<td><code>Value</code></td>
<td>

需替换到文本里的值。

该字段支持[令牌](../author-guide.md#tokens)，不区分大小写。和大部分Content Patcher的字段一样，开头和末尾的空格字符会被删除。

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

_(可选)_ 指定需替换的值，可填写：

模式    | 结果
------- | ------
`First` | 替换第一个等于`Search`的值，其他值不变。
`Last`  | 替换最后一个等于`Search`的值，其他值不变。
`All`   | 替换所有等于`Search`的值。

默认为`All`.

</td>
</tr>
</table>

例如，此补丁将遍喜好中的兔子的脚（物品ID 446）替代为河豚 （物品ID 128）：


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
