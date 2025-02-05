← [模组作者指南](../author-guide.md)

使用 **`"Action": "EditData"`** 可以编辑数据资产中的字段和条目。多个内容包可以编辑同一个资产。

## 目录
* [基本概念](#基本概念)
  * [数据资产](#数据资产)
  * [字段和条目](#字段和条目)
  * [目标字段](#目标字段1)
* [用法](#用法)
  * [概述](#概述)
  * [编辑字典](#编辑字典)
  * [编辑列表](#编辑列表)
  * [编辑模型](#编辑模型)
  * [组合操作](#组合操作)
* [目标字段](#目标字段2)
  * [格式](#格式)
  * [例子](#例子)
* [另见](#另见)

## 基本概念
游戏中有许多类型的数据，Content Patcher将其转化成一些常用概念。
只有理解了本页所述的概念，你才能理解其余部分，所以请不要跳过该部分！


### 数据资产
 _数据资产（data asset）_ 包括从游戏中加载的: 时间地点，角色对话等数据。比如 `Data/Objects` 包括游戏内所有物品的数据。每种数据的格式都在[维基](https://zh.stardewvalleywiki.com/%E6%A8%A1%E7%BB%84:%E7%9B%AE%E5%BD%95)上有阐述。

以下是三种主要的数据资产：

<table>
<tr>
<th>类型</th>
<th>用法</th>
</tr>
<tr>
<td>字典</td>
<td>

 _字典（dictionary）_ 是包含许多键值对的列表。其中的键必须有唯一ID。同一个字典中的键值对必须写在 `{` 和 `}` 之中。

比如， `Data/Boots` 就是一个包含了许多键值对的字典：

```js
{
    //格式是 "键": "值" （所有引号必须用英文的半角符号）
    "504": "Sneakers/A little flimsy... but fashionable!/50/1/0/0/Sneakers",
    "505": "Rubber Boots/Protection from the elements./50/0/1/1/Rubber Boots",
    "506": "Leather Boots/The leather is very supple./50/1/1/2/Leather Boots"
}
```

</td>
</tr>
<tr>
<td>列表</td>
<td>

 _列表（list）_ 是一组没有明确键的非唯一值。列表中的键值对必须写在 `[` 和 `]`之中。

比如， `Data/Concessions` 就是一个典型的列表：

```js
[
    {
       "ID": 0,
       "Name": "Cotton Candy",
       "DisplayName": "Cotton Candy",
       "Description": "A large pink cloud of spun sugar.",
       "Price": 50,
       "ItemTags": [ "Sweet", "Candy" ]
    },
    // 篇幅有限，其他数据省略
]
```

虽然列表没有键，但Content Patcher通常会指定一个字段作为唯一标识符（详见[_编辑列表_](#编辑列表)）。

</td>
</tr>
<tr>
<td>模型</td>
<td>

 _模型（model）_ 是一种预定义的数据结构。对于内容包来说，它和字典相同，只是你不能添加新字段（只能编辑已有字段）。

</td>
</tr>
</table>

### 字段和条目<a name="entries"></a>
 _条目（entry）_ 是目标数据中的顶层数据块（即字典中的键值对或列表中的值）

比如， `Data/Objects` 中， `"MossSoup": { ...}` 和 `"PetLicense": { ... }`是两个独立的条目：
```js
{
    "MossSoup": {
        "Name": "Moss Soup",
        "Type": "Cooking",
        "Category": -7,
        "Price": 80,
        "ContextTags": [ "color_green" ]
        ...
    },
    "PetLicense": {
        "Name": "Pet License",
        "Type": "Basic",
        "Category": 0,
        "Price": 0,
        ...
    },
    ...
}
```

 _字段（field）_ 是条目中的一个子块。在前面的例子中：
- `"MossSoup": { ... }` 是一个条目；
- `"Name": "Moss Soup"` 是 `"MossSoup": { ... }`这个条目中的一个字段。

### 目标字段<span id="目标字段1"></span>
在上一部分中，我们说“_目标数据中的一个顶层数据块_”。
目标字段可以让你改变目标条目的含义。

比如，假设我们将目标字段设置为上面的`ContextTags`字段。那么你会看到的数据是这样的：
```json
[ "color_green" ]
```
/
这意味着`ContextTags`中的每一个值都是一个条目，所以你可以添加/替换/删除上下文标记，而无需编辑对象数据的其他部分。

([_目标字段_](#目标字段2)会详细介绍这一点)


## 用法
### 概述
`EditData`由`Changes`下的模型组成，其中包含（见下面的示例）：

<dl>
<dt>必填字段</dt>
<dd>

你必须同时指定这两个字段：

字段     | 用途
--------- | -------
`Action`  | 操作类型。此操作类型设置为`EditData`。
`Target`  | [游戏资产名](../author-guide.md#数据资产是什么)替换（由逗号分隔的资产名），比如`Characters/Dialogue/Abigail`。该字段支持[tokens](../author-guide.md#tokens)，不用区分大小写。

至少有下列一项：

<table>
<tr>
<th>字段</th>
<th>用途</th>
</tr>
<tr>
<td><code>Fields</code></td>
<td>

你要更改的现有条目的单个字段。该字段的键和值都支持
[tokens](../author-guide.md#tokens)。每个字段的键是以/分隔的字段索引（从0开始），或对象的字段名。

</td>
</tr>
<td><code>Entries</code></td>
<td>

你要添加/替换/删除的用ID索引的数据文件中的条目。如果你只想改几个字段，用`Fields`才更能和别的模组兼容。要添加条目，只需要指定一个不存在的键； 要删除条目，可以把它的值设为`null`（比如
`"some key": null`）。可以在该字段条目的键和值中用[tokens](../author-guide.md#tokens)。

对于列表的值，查看下面的`MoveEntries`。

</td>
</tr>
<td><code>MoveEntries</code></td>
<td>

 _只支持列表资产_ 更改列表资产（如`Data/MoviesReactions`）中的条目顺序。（在非列表资产中使用会出错，因为那些资产没有顺序）

详见[_移动列表条目_](#移动列表条目)。

</td>
</tr>
<td><code>TextOperations</code></td>
<td>

更改现有字符串条目或字段的值；详见 _[文本操作](../author-guide.md#文本操作)_ 。

要更改条目，请使用格式`["Entries", "条目对应的值"]`并将`"条目对应的值"`替换为上文你想指定的`Entries`。
如果该条目不存在，它会自动新建。文本操作会把它当做一个空字符串生效。


要更改字段，请使用格式`["Fields", "条目对应的值", "字段对应的值"]`并用上文你想为`Fields`指定的键替换`"条目对应的值"`和`"字段对应的值"。
如果条目不存在，这个文本操作会报错；如果该字段不存在，在条目是一个对象时它会自动新建，条目是一个分隔字符串时会报错。
目前只能针对顶层字段。

</td>
</tr>
</table>
</dd>
<dt>可选条目：</dt>
<dd>

条目         | 用途
------------- | -------
`TargetField` | _（可选）_ 以[列表或字典](#数据资产)为目标时，值中的字段会被设为根作用域；详见[_目标字段_](#目标字段2)。该字段支持[tokens](../author-guide.md#tokens)。
`When`        | _（可选）_ 当给定的[条件](../author-guide.md#条件)匹配时才应用这个内容补丁。
`LogName`     | _（可选）_ 在日志中显示的补丁名称。这有助于查找错误。如果省略，默认名称为`EditData Data/Achievements`。
`Update`      | _（可选）_ 补丁字段多久更新一次。详见[更新速率](../author-guide.md#更新速率)。
`LocalTokens` | _（可选）_ 可在本补丁字段中使用的[本地tokens](../author-guide/tokens.md#本地tokens) 。

</dd>
<dt>高级字段：</dt>
<dd>

<table>
  <tr>
    <td>字段</td>
    <td>用途</td>
  </tr>
  <tr>
  <td><code>Priority</code></td>
  <td>

 _（可选）_ 当多个补丁修改编辑同一数据资产时，他们应用的顺序。可能的值有`Early`（高优先级），`Default`（默认），还有`Late`（低优先级）。默认值为`Default`。

补丁（包括所有模组）按以下顺序应用：

1. 优先级从高到低；
2. 按照模组加载顺序（比如基于依赖关系）；
3. 按照补丁在`content.json`中列出的顺序。

如果需要更具体的顺序，可以使用简单的偏移量，如`"Default + 2"`或者`"Late - 10"`。
默认值为-1000 （高优先级），0（默认）和1000（低优先级）。

此字段 _不_ 支持tokens，不区分大小写。

> [!提示]
> 优先级会让你的更改更难排除故障。建议最好：
> * 如果可以的话，考虑使用上述的优先级（比如直接设为`Late`，用于外观覆盖）
> * 在 _你自己_ 的补丁里不需要用优先级，因为你可以自己在content.json排列好补丁应用的顺序。

  </tr>
  <tr>
  <td><code>TargetLocale</code></td>
  <td>

 _（可选）_ 资产名称中要匹配的地区代码，比如设置`"TargetLocale": "fr-FR"`只编辑法语形式的资产（比如`Data/Achievements.fr-FR`）。可以为空，只有只编辑没有地域区分的基本资产。

如果省略，它将应用于所有资产，不管有没有本地化。

</td>
</table>
</dd>
</dl>

### 编辑字典
[字典](#数据资产)最简单的编辑方法是创建或覆盖一个条目。比如，以下操作会[添加一个新物品](https://stardewvalleywiki.com/Modding:Items)。
ID为`{{ModId}}_Pufferchick`的物品，会被添加到`Data/Objects`中：

```js
{
    "Format": "2.5.0",
    "Changes": [
        {
            "Action": "EditData",
            "Target": "Data/Objects",
            "Entries": {
                "{{ModId}}_Pufferchick": {
                    "Name": "{{ModId}}_Pufferchick",
                    "DisplayName": "Pufferchick",
                    "Description": "An example object.",
                    "Type": "Seeds",
                    "Category": -74,
                    "Price": 1200,
                    "Texture": "Mods/{{ModId}}/Objects"
                }
            },
        }
    ]
}
```

你还可以编辑条目中的字段。当条目是字符串时，该值会被假定为一个以/分隔的字段列表（每个字段都有一个从0开始的数字），否则字段是给定条目中的直接条目。

比如，这将编辑一个条目的描述字段：

```js
{
    "Format": "2.5.0",
    "Changes": [
        {
            "Action": "EditData",
            "Target": "Data/Objects",
            "Fields": {
                "MossSoup": { //它的条目是"MossSoup"
                    "Description": "Maybe a pufferchick would like this."
                }
            }
        },
    ]
}
```

你可以用把字段设为`null`的方式把某个条目删除。比如，下列事件会被替换为一个前置条件不同的另一个事件：
```js
{
    "Format": "2.5.0",
    "Changes": [
        {
            "Action": "EditData",
            "Target": "Data/Events/Beach",
            "Entries": {
                "733330/f Sam 750/w sunny/t 700 1500/z winter/y 1": null,
                "733330/f Sam 750/w sunny/t 700 1500/z winter": "事件脚本"
            }
        }
    ]
}
```

当一个值有嵌套的条目时，你可以用[`目标字段`](#目标字段2)来编辑特定的那一个。

### 编辑列表
你可以用同样的方法编辑[列表](#数据资产)。

在游戏原版的数据资产中，列表没有键，但在Content Patcher中，他们仍有一个键来实现`Entries`和`MoveEntries`。也就是说，编辑列表和编辑字典差不多。

对于模型列表（`{ ... }`块），键是每个每个模型的`Id`字段。比如，`Data\LocationContexts`就展示了ID为`spring1`的`Music`条目：
```js
{
    "Default": {
        "SeasonOverride": null,
        "DefaultMusic": null,
        "DefaultMusicCondition": null,
        "DefaultMusicDelayOneScreen": true,
        "Music": [
            {
                "Id": "spring1",
                "Track": "spring1",
                "Condition": "SEASON Spring"
            }
            ...
        ]
    }
}
```

为了更改那个内容包中的音乐，你必须使用它的ID：
```js
{
    "Format": "2.5.0",
    "Changes": [
        {
            "Action": "EditData",
            "Target": "Data/LocationContexts",
            "TargetField": [ "Default", "Music" ],
            "Entries": {
                "spring1": {
                    "Id": "spring1",
                    "Track": "spring1",
                    "Condition": "SEASON Spring, YEAR 2"
                }
            }
        }
    ]
}
```

编辑简单字符串列表的方法和上述完全相同，只是字符串本身就是键。详见[编辑对象上下文标签示例](#编辑对象上下文标签示例)。

### 移动列表条目
对于列表来说，顺序很重要（比如，游戏会使用`Data\MoviesReactions`的第一个条件适合的条目来匹配NPC反应。你可以用`MoveEntries`字段来更改顺序。比如，这会移动`Abigail`的每一个条目：
```js
{
    "Format": "2.5.0",
    "Changes": [
        {
            "Action": "EditData",
            "Target": "Data/MoviesReactions",
            "MoveEntries": [
                { "ID": "Abigail", "BeforeID": "Leah" },     //移动字段让阿比盖尔在莉亚前面
                { "ID": "Abigail", "AfterID": "Leah" },      //移动字段让阿比盖尔在莉亚后面
                { "ID": "Abigail", "ToPosition": "Top" },    //移动字段让它位于最顶层
                { "ID": "Abigail", "ToPosition": "Bottom" }, //移动字段让它位于最底层
            ]
        },
    ]
}
```

新条目默认增加到列表底部。

### 编辑模型
_模型_ 是一种预定义的数据结构。对于内容包来说，除了不能添加新条目（只能编辑已有条目）以外，它与字典相同。

### 组合操作
你可以在同一个补丁中执行任意数量的操作。例如，你可以添加一个新条目，然后同时将其移动到正确的顺序。它们按照`Entries`， `Fields`， `MoveEntries`， `TextOperations`的顺序编辑。

## 目标字段 <span id="目标字段2"></span>
更改通常适用于顶层条目，但`TargetField`让你可以选择一个字块编辑。这会影响所有编辑补丁（比如`Fields`， `Entries`，
`TextOperations`等）。

### 格式
`TargetField`是要“钻取“的字段名列表。列表中的每个值都在前一个值的范围内，可以是其中之一：

类型       | 作用
---------- | ------
ID         | 数据内的[字典键](#编辑字典)或者[列表键](#编辑列表)（比如：下面例子中的`"Goby"`）。
字段名      | 数据模型中的字段名。
列表值      | 简单字符串或数值列表的目标值。
列表索引    | 值在列表中的位置（如`#0`表示第一个值）。前缀必须是`#`，否则会被视为ID。这种用法很容易出错，因为它取决于列表的顺序；最好用ID或者字段名代替。

### 例子
#### 编辑对象上下文标签示例
`Data/Objects`中有以下条目：
```js
"Goby": {
    "Name": "Goby",
    "Type": "Fish",
    "ContextTags": [ "color_brown", "fish_river", "season_fall", "season_spring", "season_summer" ]
    ...
},
```

如果我们要更改上下文标签，又不想重新定义整个项目，或者丢失其他模组的更改，可以用`"TargetField": [ "Goby", "ContextTags" ]`：
```json
[ "color_brown", "fish_river", "season_fall", "season_spring", "season_summer" ]
```

然后我们就可以像数据资产一样在列表中添加，替换，删除条目。
```js
{
    "Format": "2.5.0",
    "Changes": [
        {
            "Action": "EditData",
            "Target": "Data/Objects",
            "TargetField": [ "Goby", "ContextTags" ],
            "Entries": {
                "season_winter": "season_winter", //添加值
                "season_spring": null,            //删除值
                "color_brown": "color_green"      //替换值
            }
        },
    ]
}
```

#### 编辑深嵌套字段
上面的示例编辑了模型顶部的字段，但我们可以向下钻取任意深度的字段。

例如`Data/Objects`中的条目：
```json
"791": {
    "Name": "Golden Coconut",
    "GeodeDrops": [
        {
            "Id": "Default",
            "RandomItemId": [ "(O)69", "(O)835", "(O)833", "(O)831", "(O)820", "(O)292", "(O)386" ],
            "StackModifiers": [
                {
                    "Id": "PineappleSeeds",
                    "Condition": "ITEM_ID Target (O)833",
                    "Modification": "Set",
                    "Amount": 5
                },
                ...
            ],
            ...
        }
    ]
}
```

比如我们想把菠萝种子从5个改成20个，那么这些字段的层次结构是：

* entry: `791`
  * field: `GeodeDrops`
    * list value with ID: `Default`
      * field: `StackModifiers`
        * list value with ID: `PineappleSeeds`
          * field: `Amount`

我们只需要“深入”这个层次结构，编辑我们需要的字段：

```json
{
    "Format": "2.5.0",
    "Changes": [
        {
            "Action": "EditData",
            "Target": "Data/Objects",
            "TargetField": [ "791", "GeodeDrops", "Default", "StackModifiers", "PineappleSeeds" ],
            "Entries": {
                "Amount": 20
            }
        }
    ]
}
```

## 另见
* [模组作者指南](../author-guide.md)
* 维基上的[数据资产格式文档](https://zh.stardewvalleywiki.com/%E6%A8%A1%E7%BB%84:%E7%9B%AE%E5%BD%95)
