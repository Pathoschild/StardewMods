← [模组作者指南](../author-guide.md)

此文档描述Content Patcher里可使用的令牌。

**详见[README](../README.md)**。

**🌐 其他语言： [en (English)](../../author-guide/tokens.md)。**

## 目录
* [介绍](#introduction)
  * [概述](#overview)
  * [令牌类型](#token-types)
  * [集理论](#set-theory)
* [全局令牌](#global-tokens)
  * [日期和天气](#date-and-weather)
  * [玩家](#player)
  * [关系](#relationships)
  * [世界](#world)
  * [数字操纵](#number-manipulation)
  * [字符串操纵](#string-manipulation)
  * [元数据](#metadata)
  * [字段引用](#field-references)
  * [特定场合](#specialized)
* [设置令牌](#config-tokens)
* [动态令牌](#dynamic-tokens)
* [局部令牌](#local-tokens)
* [输入参数](#input-arguments)
  * [概述](#overview-1)
  * [全局输入参数](#global-input-arguments)
  * [自定义参数分割符号](#custom-input-value-separator)
* [随机](#randomization)
* [进阶](#advanced)
  * [查询表达式](#query-expressions)
  * [模组提供令牌](#mod-provided-tokens)
  * [别名](#aliases)
* [共同值](#common-values)
* [参见](#see-also)

## 介绍<a name="introduction"></a>
### 概述<a name="overview"></a>
一个 **令牌** （token）是有名字的一组值。比如说，名为`season`的令牌在游戏季节为夏天时的值为`"summer"`.

令牌主要有两种使用方法：

#### 占位符<a name="placeholders"></a>
在文本中把令牌名称放入两层大括號即可调用令牌的值，运行时会自动将令牌占位符替换成对应的令牌值。

大部分字段都可以可使用令牌占位符（每个字段的文档会注释可否使用令牌），不区分大小写（`{{season}}`和`{{SEASON}}`是同一个令牌）。一个含有当前不可用的补丁将不生效。

此例子让农舍在每个季节都更改外观。

```js
{
    "Action": "EditImage",
    "Target": "Buildings/houses",
    "FromFile": "assets/{{season}}_house.png" // assets/spring_house.png, assets/summer_house.png, 诸如此类。
}
```

只有一个值的令牌最适合以占位符形式使用，但多值的令牌也可以用（显示为以逗号分割的列表）。

#### 条件<a name="conditions"></a>
您可以用`When`字段给补丁添加生效条件，`When`里字段可包含多个条件。

每个条件里含有：
* 一个含有[令牌](#introduction)的键，不需要双大括号，如`Season`或`HasValue:{{spouse}}`。此键不区分大小写。
* 一个含有以逗号分割的列表的值，如`spring, summer`。若键里的令牌含有列表中任意值，此条件成立。值本身也支持[令牌](#introduction)，不区分大小写。


此例子让农舍在第一年的春天（spring）和夏天（summer）更改外观。

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

一个条件里值只要有一个值对应令牌那整个条件成立。而一个补丁只有在`When`里所有的条件都成立时才会生效。

### 令牌类型<a name="token-types"></a>
令牌有很多类型，但使用方式都一样。

您不需要学会所有令牌。每一种令牌有不同的目的，而大部分内容包只用一两种令牌。

令牌类型包括（从最常用到最罕见）：
* [全局令牌](#global-tokens)是各种常见的值，如季节，天气，友情等。这些是Content Patcher自带的令牌。
* [设置令牌](#config-tokens)是玩家在模组设置选项中可选的值。
* [动态令牌](#dynamic-tokens)是您定义的值，用来重复利用某些值或利用多个简单令牌构造复杂的令牌。
* _(高级)_ [局部令牌](#local-tokens)和动态令牌类似，但仅限于一个补丁（或一个`Include`中的补丁）而不是整个内容包。主要用来重复使用很多类似的补丁。
* _(高级)_ [模组提供令牌](#mod-provided-tokens)由玩家安装的其他模组提供。

## 集理论<a name="set-theory"></a>
Content Patcher里的令牌是[集合](https://zh.wikipedia.org/wiki/集合_(数学))，在实践层面上这意味它们：
- 不能有重复值（每个独特的值只会出现一次）
- 大部分时候无排列
- 执行值对比的效率很高

## 全局令牌<a name="global-tokens"></a>
全局令牌的值由Content Patcher定义，因此无需额外操作即可直接使用。

### 日期与天气<a name="date-and-weather"></a>
<table>
<tr>
<th>条件</th>
<th>用法</th>
<th>&nbsp;</th>
</tr>

<tr valign="top" id="Day">
<td>Day</td>
<td>当前月份中的日期，值为1至28之间的整数。</td>
<td><a href="#Day">#</a></td>
</tr>

<tr valign="top" id="DayEvent">
<td>DayEvent</td>
<td>

今天会发生的节日或者婚礼，可取值为：
* `wedding` (当前玩家结婚事件)；
* `dance of the moonlight jellies`；
* `egg festival`；
* `feast of the winter star`；
* `festival of ice`；
* `flower dance`；
* `luau`；
* `stardew valley fair`；
* `spirit's eve`；
* 其他自定义节日名称。

</td>
<td><a href="#DayEvent">#</a></td>
</tr>

<tr valign="top" id="DayOfWeek">
<td>DayOfWeek</td>
<td>

一周中的一天. 可取值为： `Monday`, `Tuesday`, `Wednesday`, `Thursday`, `Friday`,
`Saturday`, 和 `Sunday`。

</td>
<td><a href="#DayOfWeek">#</a></td>
</tr>

<tr valign="top" id="DaysPlayed">
<td>DaysPlayed</td>
<td>当前保存的游戏内总天数（存档开始的第一天为1）。</td>
<td><a href="#DaysPlayed">#</a></td>
</tr>

<tr valign="top" id="Season">
<td>Season</td>
<td>

季节名称。可取值为： `Spring`, `Summer`, `Fall`, 和`Winter`。

</td>
<td><a href="#Season">#</a></td>
</tr>

<tr valign="top" id="Time">
<td>Time</td>
<td>

游戏中一天的时间, 作为一个处于 `0600`（早上六点）和 `2600`（凌晨两点）的数值。
这也可以用于表示一定范围的令牌：
```js
"When": {
   "Time": "{{Range: 0600, 2600}}"
}
```

ℹ 在使用这个令牌前请参见 _[更新速率](../author-guide.md#update-rate)_ 。

</td>
<td><a href="#Time">#</a></td>
</tr>

<tr valign="top" id="Weather">
<td>Weather</td>
<td>

当前世界范围（或是一部分特殊地区的[`地点上下文`](#location-context)参数）的天气类型
 可取值为：

值       | 含义
----------- | -------
`Sun`       | 晴天 (包括节日或者婚礼)。 如果没有指定其它值，这将是默认天气。
`Rain`      | 没有雷电的雨天。
`Storm`     | 有雷电的雨天。
`GreenRain` | [苔雨天](https://zh.stardewvalleywiki.com/天气#苔雨)。
`Snow`      | 下雪。
`Wind`      | 起风了，参杂着可见碎片（比如：春天的樱花和秋天的枯叶）。
_自定义天气_    | 模组自定义的天气，需使用ID表示。

ℹ 在未指定地点上下文时请参见 _[更新速率](../author-guide.md#update-rate)_ 。

</td>
<td><a href="#Weather">#</a></td>
</tr>

<tr valign="top" id="Year">
<td>Year</td>
<td>

年份编号（例如`1`或者`2`）。

</td>
<td><a href="#Year">#</a></td>
</tr>
</table>

### 玩家<a name="player"></a>
<table>
<tr>
<th>条件</th>
<th>用法</th>
<th>&nbsp;</th>
</tr>

<tr valign="top" id="DailyLuck">
<td>DailyLuck</td>
<td>

这个[运气](https://zh.stardewvalleywiki.com/运气)需指定[当前或特定玩家](#target-player)。

这是一个处于-0.1和0.1之间的小数值。它 **不能** 用
`{{Range}}`令牌比较，它会产生一系列整数值。这个数值只能在使用
[查询表达式](#query-expressions)时被稳定地比较。例如：

```js
"When": {
   "Query: {{DailyLuck}} < 0": true //精灵今天十分不满。
}
```

</td>
<td><a href="#DailyLuck">#</a></td>
</tr>

<tr valign="top" id="FarmhouseUpgrade">
<td>FarmhouseUpgrade</td>
<td>

[农舍等级](https://zh.stardewvalleywiki.com/农舍#升级)需指定[当前或特定玩家](#target-player)。正常值为0（初始农舍），1（增加厨房），2（增加婴儿房），和3（增加地窖）。其他模组可能会增加超过这个级别的等级。

</td>
<td><a href="#FarmhouseUpgrade">#</a></td>
</tr>

<tr valign="top" id="HasActiveQuest">
<td>HasActiveQuest</td>
<td>

[当前或特定玩家](#target-player)的任务列表中的任务ID。
参见[任务数据](https://zh.stardewvalleywiki.com/模组:任务数据)获取有效的任务ID。

</td>
<td><a href="#HasActiveQuest">#</a></td>
</tr>

<tr valign="top" id="HasCaughtFish">
<td>HasCaughtFish</td>
<td>

[当前或特定玩家](#target-player)钓到的鱼的ID。
另见[物品ID](https://zh.stardewvalleywiki.com/模组:物体)

</td>
<td><a href="#HasCaughtFish">#</a></td>
</tr>

<tr valign="top" id="HasConversationTopic">
<td>HasConversationTopic</td>
<td>

[当前或特定玩家](#target-player)正在进行的[对话主题](https://zh.stardewvalleywiki.com/模组:对话#对话主题)。

</td>
<td><a href="#HasConversationTopic">#</a></td>
</tr>

<tr valign="top" id="HasCookingRecipe">
<td>HasCookingRecipe</td>
<td>

[当前或特定玩家](#target-player)学过的[菜谱](https://zh.stardewvalleywiki.com/烹饪)。

</td>
<td><a href="#HasCookingRecipe">#</a></td>
</tr>

<tr valign="top" id="HasCraftingRecipe">
<td>HasCraftingRecipe</td>
<td>

[当前或特定玩家](#target-player)学过的[合成制造品](https://zh.stardewvalleywiki.com/打造)。

</td>
<td><a href="#HasCraftingRecipe">#</a></td>
</tr>

<tr valign="top" id="HasDialogueAnswer">
<td>HasDialogueAnswer</td>
<td>

[当前或特定玩家](#target-player)在问题中选过的[回答ID](https://zh.stardewvalleywiki.com/模组:对话#回答ID)。

</td>
<td><a href="#HasDialogueAnswer">#</a></td>
</tr>

<tr valign="top" id="HasFlag">
<td>HasFlag</td>
<td>

[当前或特定玩家](#target-player)有过的各种ID。包括：

* 发送给玩家的信件ID（包括未读或明天将到邮箱的信件）；
* 非信件邮件ID（用于追踪游戏信息的）；
* 世界状态ID。

另见[维基上的可用ID列表](https://zh.stardewvalleywiki.com/模组:信件数据#列表)。

</td>
<td><a href="#HasFlag">#</a></td>
</tr>

<tr valign="top" id="HasProfession">
<td>HasProfession</td>
<td>

[当前或特定玩家](#target-player)学过的[技能](https://zh.stardewvalleywiki.com/技能)。

可能的值有：

* 战斗技能： `Acrobat`（野蛮人）， `Brute`（特技者）， `Defender`（防御者）， `Desperado`（亡命徒）， `Fighter`（战士）， `Scout`（侦查员）。
* 耕种技能： `Agriculturist`（农业学家）， `Artisan`（工匠）， `Coopmaster`（鸡舍大师）， `Rancher`（畜牧人）， `Shepherd`（牧羊人）， `Tiller`（农耕人）。
* 钓鱼技能： `Angler`（垂钓者）， `Fisher`（渔夫）， `Mariner`（水手）， `Pirate`（海盗）， `Luremaster`（诱饵大师）， `Trapper`（捕猎者）。
* 采集技能： `Botanist`（植物学家）， `Forester`（护林人）， `Gatherer`（收集者）， `Lumberjack`（伐木工人）， `Tapper`（萃取者）， `Tracker`（追踪者）。
* 挖矿技能： `Blacksmith`（铁匠）， `Excavator`（挖掘者）， `Gemologist`（宝石专家）， `Geologist`（地质学家）， `Miner`（矿工）， `Prospector`（勘探者）。

模组添加的自定义职业需用它们的整型ID表示。

</td>
<td><a href="#HasProfession">#</a></td>
</tr>

<tr valign="top" id="HasReadLetter">
<td>HasReadLetter</td>
<td>

[当前或特定玩家](#target-player)打开过的信件ID。邮箱页面一旦出现，这封信就已经被看作已读了。

</td>
<td><a href="#HasReadLetter">#</a></td>
</tr>

<tr valign="top" id="HasSeenEvent">
<td>HasSeenEvent</td>
<td>

[当前或特定玩家](#target-player)看过的事件的ID。与 `Data/Events` 里事件的ID匹配。

您可以用[调试模式](https://www.nexusmods.com/stardewvalley/mods/679)来查看游戏里的事件ID。

</td>
<td><a href="#HasSeenEvent">#</a></td>
</tr>

<tr valign="top" id="HasVisitedLocation">
<td>HasVisitedLocation</td>
<td>

[当前或特定玩家](#target-player)去过的地点的内部ID，与 `Data/Locations` 里的地点ID匹配。

您可以用[调试模式](https://www.nexusmods.com/stardewvalley/mods/679)查看更多地点ID。

</td>
<td><a href="#HasVisitedLocation">#</a></td>
</tr>

<tr valign="top" id="HasWalletItem">
<td>HasWalletItem</td>
<td>

当前玩家的[钱包里的特殊物品](https://zh.stardewvalleywiki.com/特殊物品与能力)。

可能的值为：

标志                       | 含义
-------------------------- | -------
`DwarvishTranslationGuide`（矮人语教程） | 可以与矿洞中的矮人和火山地牢商店的矮人交流。
`RustyKey`（生锈的钥匙）                 | 解锁下水道。
`ClubCard`（会员卡）                | 解锁沙漠俱乐部。
`KeyToTheTown` （小镇钥匙）            | 允许玩家在绝大多数时间内无视建筑物开关门时间段进入小镇上的任何建筑物。
`SpecialCharm` （特殊的魅力）            | 永久提升每天的运气。
`SkullKey` （头骨钥匙）                | 解锁[骷髅洞穴](https://zh.stardewvalleywiki.com/骷髅洞穴)和星之果实餐吧里的祝尼魔赛车游戏机。
`MagnifyingGlass` （放大镜）         | 获得找到秘密纸条的能力。
`DarkTalisman` （黑暗护身符）            | 解锁巫婆沼泽。
`MagicInk` （魔法墨水）                | 解锁[魔法建筑](https://zh.stardewvalleywiki.com/法师塔#建筑)和[黑暗神龛](https://zh.stardewvalleywiki.com/女巫小屋#神龛)。
`BearsKnowledge` （熊的知识）          | 提升美洲大树莓及黑莓3倍的售出价格。
`SpringOnionMastery` （青葱技术）      | 提升大葱5倍的售出价格。

</td>
<td><a href="#HasWalletItem">#</a></td>
</tr>

<tr valign="top" id="IsMainPlayer">
<td>IsMainPlayer</td>
<td>

[当前或特定玩家](#target-player)是否是房主。
可取值：
`true`，`false`。

</td>
<td><a href="#IsMainPlayer">#</a></td>
</tr>

<tr valign="top" id="IsOutdoors">
<td>IsOutdoors</td>
<td>

[当前或特定玩家](#target-player)是否在户外。
可取值：`true`，
`false`。

ℹ 使用此令牌前请参考 _[更新速率](../author-guide.md#update-rate)_ 。

</td>
<td><a href="#IsOutdoors">#</a></td>
</tr>

<tr valign="top" id="LocationContext">
<td>LocationContext</td>
<td>

[当前或特定玩家](#target-player)所在位置的内部名称。

可能的值有：

* `Default` （就在小镇里）；
* `Desert` （在[沙漠](https://zh.stardewvalleywiki.com/沙漠)）；
* `Island` （在[姜岛](https://zh.stardewvalleywiki.com/姜岛)）；
* 或者 `Data/LocationContexts` 里的[自定义地点ID](https://zh.stardewvalleywiki.com/模组:迁移至游戏本体1.6#自定义地点)。

ℹ 使用这个令牌前请参考 _[更新速率](../author-guide.md#update-rate)_ 。

</td>
<td><a href="#LocationContext">#</a></td>
</tr>

<tr valign="top" id="LocationName">
<td id="LocationUniqueName">LocationName<br />LocationUniqueName</td>
<td>

[当前或特定玩家](#target-player)所在地点的内部ID，比如 `FarmHouse` 或者 `Town`。您可以用
[调试模式](https://www.nexusmods.com/stardewvalley/mods/679)或者[`patch
summary`](../author-guide.md#patch-summary)查看地点的内部ID。

注意：
* 所有临时节日地点都叫“Temp”。
* `LocationName` 和 `LocationUniqueName` 一般都是相同的，除了可移动的建筑，小屋，酒窖。比如鸡舍的 `LocationName` 是"Deluxe Coop" 但是`LocationUniqueName` 是"Coop7379e3db-1c12-4963-bb93-23a1323a25f7"。`LocationUniqueName` 可以用作传送时的目标地点。

ℹ 使用此令牌前请参考 _[更新速率](../author-guide.md#update-rate)_ 。

</td>
<td><a href="#LocationName">#</a></td>
</tr>

<tr valign="top" id="LocationOwnerId">
<td>LocationOwnerId</td>
<td>

[当前或特定玩家](#target-player)所处地点的所有者的[唯一ID](#target-player)。

只在下列地点生效：

地点      | 所有者
:------------ | :----
农舍     | 房主
姜岛农舍  | 房主
联机小屋         | 其他联机玩家
酒窖        | 和这个酒窖所在农舍或联机小屋的所有者一样
农场建筑 | 建造它的玩家

这个令牌也能用于获取所有者的其他信息，如`{{PlayerName: {{LocationOwnerId}}}}`。

ℹ 使用此令牌前请参考 _[更新速率](../author-guide.md#update-rate)_ 。

</td>
<td><a href="#LocationOwnerId">#</a></td>
</tr>

<tr valign="top" id="PlayerGender">
<td>PlayerGender</td>
<td>

[当前或特定玩家](#target-player)的性别。可取值为： `Female`， `Male`。

</td>
<td><a href="#PlayerGender">#</a></td>
</tr>

<tr valign="top" id="PlayerName">
<td>PlayerName</td>
<td>

[当前或特定玩家](#target-player)的名字。

</td>
<td><a href="#PlayerName">#</a></td>
</tr>

<tr valign="top" id="PreferredPet">
<td>PreferredPet</td>
<td>

当前玩家偏爱的宠物类型，可取值：`Cat`， `Dog`。

</td>
<td><a href="#PreferredPet">#</a></td>
</tr>

<tr valign="top" id="SkillLevel">
<td>SkillLevel</td>
<td>

当前玩家的技能等级。
可指定技能作为输入参数，例如：

```js
"When": {
   "SkillLevel:Combat": "1, 2, 3" // 战斗技能等级1, 2, 或者 3
}
```

有效的取值：`Combat`（战斗）， `Farming`（耕种）， `Fishing`（钓鱼）， `Foraging`（采集）， `Luck` （幸运等级，游戏中未实装）
和`Mining`（挖矿）。

</td>
<td><a href="#SkillLevel">#</a></td>
</tr>
</table>

### 人际关系<a name="relationships"></a>
<table>
<tr>
<th>条件</th>
<th>用途</th>
<th>&nbsp;</th>
</tr>

<tr valign="top" id="ChildNames">
<td id="ChildGenders">ChildNames<br />ChildGenders</td>
<td>

[当前或特定玩家](#target-player)孩子的名字和性别（`Female` 或者 `Male`）。

按出生顺序排列，可用[`valueAt` 参数](#valueAt)指定。比如 
`{{ChildNames |valueAt=0}}` 和 `{{ChildGenders |valueAt=0}}` 分别是最年长的孩子的名字和性别。

</td>
<td><a href="#ChildNames">#</a></td>
</tr>

<tr valign="top" id="Hearts">
<td>Hearts</td>
<td>

玩家与指定角色的心数。可将角色名称作为输入参数
（只能使用角色的内置名）。比如：

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

玩家与指定角色或玩家的关系。可将角色名称作为输入参数（只能使用角色的内置名），例如：

```js
"When": {
   "Relationship:Abigail": "Married"
}
```

有效的关系类型有：

值    | 含义
-------- | -------
Unmet    | 玩家尚未与该角色对话过。
Friendly | 玩家至少与该角色对话过，但仅此而已。
Dating   | 玩家给了角色花束。
Engaged  | 玩家给了角色美人鱼吊坠，但婚礼还没到。
Married  | 玩家已和角色结婚。
Divorced | 玩家和角色结婚然后又离了。

</td>
<td><a href="#Relationship">#</a></td>
</tr>

<tr valign="top" id="Roommate">
<td>Roommate</td>
<td>

[当前或特定玩家](#target-player)的室友（只能用内置名）。

</td>
<td><a href="#Roommate">#</a></td>
</tr>


<tr valign="top" id="Spouse">
<td>Spouse</td>
<td>

[当前或特定玩家](#target-player)的配偶（只能用内置名）。

</td>
<td><a href="#Spouse">#</a></td>
</tr>
</table>

### 世界<a name="world"></a>
<table>
<tr>
<th>条件</th>
<th>用途</th>
<th>&nbsp;</th>
</tr>

<tr valign="top" id="FarmCave">
<td>FarmCave</td>
<td>

[农场洞穴](https://zh.stardewvalleywiki.com/山洞)的类型。可取值：`None`（无）， `Bats`（蝙蝠洞），
`Mushrooms`（蘑菇洞）。

</td>
<td><a href="#FarmCave">#</a></td>
</tr>

<tr valign="top" id="FarmMapAsset">
<td>FarmMapAsset</td>
<td>

`Content/Maps` 文件夹中展示的农场类型。

一般只有以下取值：

农场类型    | 值
------------ | -----
标准农场     | `Farm`
沙滩农场     | `Farm_Island`
森林农场     | `Farm_Foraging`
四角农场     | `Farm_FourCorners`
山顶农场     | `Farm_Mining`
草原农场     | `Farm_Ranching`
河边农场     | `Farm_Fishing`
荒野农场     | `Farm_Combat`
_自定义类型_  | `Data/AdditionalFarms` 文件夹里的 `MapName`字段。
_无效类型_ | `Farm`

</td>
<td><a href="#FarmMapAsset">#</a></td>
</tr>

<tr valign="top" id="FarmName">
<td>FarmName</td>
<td>当前农场的名字。</td>
<td><a href="#FarmName">#</a></td>
</tr>

<tr valign="top" id="FarmType">
<td>FarmType</td>
<td>

[农场类型](https://zh.stardewvalleywiki.com/农场#农场地图)。有以下取值：

值 | 描述
----- | -----------
`Standard`<br />`Beach`<br />`FourCorners`<br />`Forest`<br />`Hilltop`<br />`Riverland`<br />`Wilderness` | 原游戏的农场类型。
_自定义农场ID_ | 模组里自定义农场类型的`ID`。
`Custom` | _（很少见）_ 用过时的方法创建自定义农场类型的模组。

</td>
<td><a href="#FarmType">#</a></td>
</tr>

<tr valign="top" id="IsCommunityCenterComplete">
<td>IsCommunityCenterComplete</td>
<td>

是否完成献祭路线。可取值：`true` 或者 `false`。

</td>
<td><a href="#IsCommunityCenterComplete">#</a></td>
</tr>

<tr valign="top" id="IsJojaMartComplete">
<td>IsJojaMartComplete</td>
<td>

是否完成JOJA路线。可取值：`true` 或者 `false`。

</td>
<td><a href="#IsJojaMartComplete">#</a></td>
</tr>

<tr valign="top" id="HavingChild">
<td>HavingChild</td>
<td>

正在怀孕或领养孩子的玩家和NPC的姓名。玩家姓名用`@`做前缀避免与NPC重名。例如，检查当前玩家是否在生育孩子：

```js
"When": {
    "HavingChild": "{{spouse}}"
}
```

注意：
* `"HavingChild": "@{{playerName}}"`和`"HavingChild": "{{spouse}}"`等效。
* 另见 `Pregnant` 令牌。

</td>
<td><a href="#HavingChild">#</a></td>
</tr>

<tr valign="top" id="Pregnant">
<td>Pregnant</td>
<td>

当前怀孕的玩家或NPC。这是 `HavingChild` 的子集，仅适用于异性关系中的女性伴侣（因为同性伴侣只能收养不能怀孕）。

</td>
<td><a href="#Pregnant">#</a></td>
</tr>
</table>

### 数字操纵<a name="number-manipulation"></a>
<table>
<tr>
<th>条件</th>
<th>用途</th>
<th>&nbsp;</th>
</tr>

<tr valign="top" id="Count">
<td>Count</td>
<td>

获取令牌当前包含的值的数量。例如，`{{Count:{{HasActiveQuest}}}}`是任务列表里接取但未完成的任务数。

</td>
<td><a href="#Count">#</a></td>
</tr>

<tr valign="top" id="Query">
<td>Query</td>
<td>

进行任意算术和逻辑运算，详见 [_查询表达式_](#query-expressions)。

</td>
<td><a href="#Query">#</a></td>
</tr>

<tr valign="top" id="Range">
<td>Range</td>
<td>

最小值和最大值之间的所有整数（闭区间）。主要是用于比较数值，例如：

```js
"When": {
   "Hearts:Abigail": "{{Range: 6, 14}}" //等同于"6, 7, 8, 9, 10, 11, 12, 13, 14"
}
```

可在单个数值上使用令牌（比如`{{Range:6, {{MaxHearts}}}}`）或者一整个都用令牌（比如
`{{Range:{{FriendshipRange}}}})`）只要符合`最小值, 最大值`的格式。

为减少卡顿，不能超过5000个数字，应尽可能更少。

</td>
<td><a href="#Range">#</a></td>
</tr>

<tr valign="top" id="Round">
<td>Round</td>
<td>

将输入数值近似为更少的小数位。

默认情况下，四舍五入到最接近的整数。例如，
`{{Round: 2.1 }}`结果是`2`，`{{Round: 2.5555 }}`结果是`3`。

该令牌可以改变近似位数：

用法 | 结果 | 含义
----- | ------ | -----------
`Round(2.5555)` | `3` | 近似为整数。
`Round(2.5555, 2)` | `2.56` | 近似为两位小数。
`Round(2.5555, 2, down)` | `2.55` | `up`向上取整，或者`down`向下取整。（不指定就默认[四舍五入](https://en.wikipedia.org/wiki/Rounding#Round_half_to_even)）。

主要为了匹配[查询表达式](#query-expressions)。例如，怪物生命值必须为整数，因此将计算结果四舍五入到最近的整数：

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

可以在单个数值上用令牌（比如`{{Round: {{value}}, 2}}`）或同时用一整个令牌（比如`{{Round: {{Settings}}}}`，其中`{{Settings}}` = `2.5, 3, up`），只要最终输入符合上述形式。

</td>
<td><a href="#Round">#</a></td>
</tr>
</table>

### 字符串操纵<a name="string-manipulation"></a>
<table>
<tr>
<th>条件</th>
<th>用途</th>
<th>&nbsp;</th>
</tr>

<tr valign="top" id="Lowercase">
<td id="Uppercase">Lowercase<br />Uppercase</td>
<td>

转换输入文本的大小写：

<dl>
<dt>Lowercase</dt>
<dd>

转为全小写。<br />例如：`{{Lowercase:It's a warm {{Season}} day!}}` &rarr; `it's a warm summer day!`，仅适用于拉丁文字母。

</dd>
<dt>Uppercase</dt>
<dd>

转为全大写：<br />例如： `{{Uppercase:It's a warm {{Season}} day!}}` &rarr; `IT'S A WARM SUMMER DAY!`，仅适用于拉丁文字母。

</dd>
</dl>
</td>
<td><a href="#Lowercase">#</a></td>
</tr>

<tr valign="top" id="Merge">
<td>Merge</td>
<td>

合并任意数量的输入值为一个令牌。可用于在
`When`块中搜索多个令牌：

```js
"When": {
   "Merge: {{Roommate}}, {{Spouse}}": "Krobus"
}
```

或与[`valueAt`](#valueat)结合获取列表中的第一个非空值：

```js
"{{Merge: {{TokenA}}, {{TokenB}}, {{TokenC}} |valueAt=0 }}"
```

注意可添加令牌，如 `{{Merge: {{Roommate}}, Krobus, Abigail }}`。

</td>
<td><a href="#Merge">#</a></td>
</tr>

<tr valign="top" id="PathPart">
<td>PathPart</td>
<td>

获取文件路径的某个部分，格式为 `{{PathPart: 路径, 需获取的部分}}`。例如：

```js
{
   "Action": "Load",
   "Target": "Portraits/Abigail",
   "FromFile": "assets/{{PathPart: {{Target}}, Filename}}.png" // 结果是 assets/Abigail.png
}
```

给定路径`assets/portraits/Abigail.png`，可指定……

* 片段类型：

  值      | 描述 | 例子
  --------------- | ----------- | ------
  `DirectoryPath` | 不含文件名的路径。 | `assets/portraits`
  `FileName`      | 文件名（含扩展名）。 | `Abigail.png`
  `FileNameWithoutExtension` | 文件名（不含扩展名）。 | `Abigail`

* 或从左开始的索引位置：

  值 | 例子
  ---------- | -------
  `0`        | `assets`
  `1`        | `portraits`
  `2`        | `Abigail.png`
  `3`        | _空值_

* 或从右开始的负索引：

  值 | 例子
  ---------- | -------
  `-1`       | `Abigail.png`
  `-2`       | `portraits`
  `-3`       | `assets`
  `-4`       | _空值_

另见[`TargetPathOnly`](#TargetPathOnly)和[`TargetWithoutPath`](#TargetWithoutPath)，简化后的更常见用法。

</td>
<td><a href="#PathPart">#</a></td>
</tr>

<tr valign="top" id="Render">
<td>Render</td>
<td>

获取输入参数的字符串表示。主要用于`When`块直接比较渲染后的值（而非比较令牌集合值）：

```js
"When": {
   "Render:{{season}} {{day}}": "spring 14"
}
```

除When以外，其他上下文中无需使用，可直接使用令牌。例如以下两项是等效的：

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

### 元数据<a name="metadata"></a>
这些令牌提供有关令牌、内容包文件、安装模组和游戏状态的信息。

<table>
<tr>
<th>条件</th>
<th>用途</th>
<th>&nbsp;</th>
</tr>

<tr valign="top" id="FirstValidFile">
<td>FirstValidFile</td>
<td>

获取内容包文件夹中存在的第一个文件路径，给定文件路径列表。可指定任意数量的文件。

每个文件路径必须相对于内容包主文件夹，且不能包含`../`（也就是只能读取您这个模组文件夹里面的东西）。

例如：

```js
// 存在则使用`assets/<language>.json`，否则使用`assets/default.json`
"FromFile": "{{FirstValidFile: assets/{{language}}.json, assets/default.json }}"
```

</td>
<td><a href="#FirstValidFile">#</a></td>
</tr>

<tr valign="top" id="HasMod">
<td>HasMod</td>
<td>

已安装模组的ID（`manifest.json`里的的`UniqueID`字段）。

</td>
<td><a href="#HasMod">#</a></td>
</tr>

<tr valign="top" id="HasFile">
<td>HasFile</td>
<td>

内容包文件夹中是否存在指定路径的文件。返回`true`或者`false`。

文件路径必须相对于内容包主文件夹，且不能包含`../`。

例如：

```js
"When": {
  "HasFile:assets/{{season}}.png": "true"
}
```

若输入包含逗号，如`HasFile: a, b.png`，逗号会视为文件名的一部分。

</td>
<td><a href="#HasFile">#</a></td>
</tr>

<tr valign="top" id="HasValue">
<td>HasValue</td>
<td>

输入参数是否非空。例如，检查玩家是否已婚：

```js
"When": {
  "HasValue:{{spouse}}": "true"
}
```

不仅限于单个令牌。可传入任意令牌化字符串，`HasValue`在结果字符串非空时返回`true`：

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

从内容包的`i18n`翻译文件获取文本。详见[翻译文档](translations.md)。

</td>
<td><a href="#I18n">#</a></td>
</tr>

<tr valign="top" id="Language">
<td>Language</td>
<td>

游戏当前语言。可取值：

代码 | 含义
---- | -------
`de` | 德语
`en` | 英语
`es` | 西班牙语
`fr` | 法语
`hu` | 匈牙利语
`it` | 意大利语
`ja` | 日语
`ko` | 韩语
`pt` | 葡萄牙语
`ru` | 俄语
`tr` | 土耳其语
`zh` | 中文

通过`Data/AdditionalLanguages`添加的自定义语言，令牌将包含其
`LanguageCode`值。

</td>
<td><a href="#Language">#</a></td>
</tr>

<tr valign="top" id="ModId">
<td>ModId</td>
<td>

当前内容包的唯一ID（`manifest.json`的`UniqueID`字段）。

通常用于构建[唯一字符串ID](https://zh.stardewvalleywiki.com/模组:公共数据字段#唯一字符串ID)。
例如：
```json
"Id": "{{ModId}}_ExampleItem"
```

</td>
<td><a href="#ModId">#</a></td>
</tr>
</table>

### 字段引用<a name="field-references"></a>
这些令牌包含当前补丁的字段值。例如，`{{FromFile}}`为当前`FromFile`字段的值。

限制：
* 仅能在补丁块中可用（如不可在动态令牌中使用）。
* 不可用于其源字段。例如，`Target`字段中不可使用`{{Target}}`。
* 不可创建循环引用。例如，`Target`字段可用`{{FromFile}}`，`FromFile`字段可用`{{Target}}`，但不能同时用。

<table>
<tr>
<th>条件</th>
<th>用途</th>
<th>&nbsp;</th>
</tr>

<tr valign="top" id="FromFile">
<td>FromFile</td>
<td>

当前素材的补丁`FromFile`字段值。路径分隔符按操作系统规范化。
主要用于检查路径是否存在：

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

当前素材的补丁`Target`字段值。路径分隔符按操作系统规范化。
主要用于指定多个目标的补丁：

```js
{
   "Action": "EditImage",
   "Target": "Characters/Abigail, Characters/Sam",
   "FromFile": "assets/{{TargetWithoutPath}}.png" // assets/Abigail.png *或者* assets/Sam.png
}
```

三者区别在于返回的部分不同。例如，目标值是
`Characters/Dialogue/Abigail`的情况下：

令牌               | 返回部分 | 示例
------------------- | ------------- | ------
`Target`            | 完整路径。 | `Characters/Dialogue/Abigail`
`TargetPathOnly`    | 最后一个分隔符前的部分。 | `Characters/Dialogue`
`TargetWithoutPath` | 最后一个分隔符后的部分。 | `Abigail`

另见[`PathPart`](#PathPart)以处理更高级的场景。

</td>
<td><a href="#Target">#</a></td>
</tr>
</table>

### 特定场合<a name="specialized"></a>
这些是高级令牌，用于支持特定场景。

<table>
<tr>
<th>条件</th>
<th>用途</th>
<th>&nbsp;</th>
</tr>

<tr valign="top" id="AbsoluteFilePath">
<td>AbsoluteFilePath</td>
<td>

获取内容包文件夹中文件的绝对路径，给定相对于内容包主文件夹的路径（不能包含`../`）。

例如，在默认Windows的Steam安装下，`{{AbsoluteFilePath: assets/portraits.png}}`会返回类似`C:\Program Files (x86)\Steam\steamapps\common\Stardew Valley\Mods\[CP] YourMod\assets\portraits.png`的值。

</td>
<td><a href="#AbsoluteFilePath">#</a></td>
</tr>

<tr valign="top" id="FormatAssetName">
<td>FormatAssetName</td>
<td>

将素材名称规范化为游戏预期的形式。例如，
`{{FormatAssetName: Data/\\///Achievements/}}`返回类似`Data/Achievements`的值。

可选参数：

参数    | 效果
----------- | ------
`separator` | 替换默认`/`的文件夹分隔符。仅在将路径添加到以`/`分隔的字段时需使用，如`{{FormatAssetName: {{assetKey}} |separator=\\}}`。

无需在`Target`字段使用，该字段自动规范化。

</td>
<td><a href="#FormatAssetName">#</a></td>
</tr>

<tr valign="top" id="InternalAssetKey">
<td>InternalAssetKey</td>
<td>

获取特殊素材键，允许游戏直接从内容包加载文件，无需将其`Load`到新`Content`素材里。

例如，用于自定义农场类型的图像：

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

注意其他内容包无法定位内部素材键（因此称为内部）。若需允许其他内容包编辑，可使用[`Action: Load`](action-load.md)创建新素材，并使用该素材名。强烈建议使用[唯一字符串ID](https://zh.stardewvalleywiki.com/模组:公共数据字段#唯一字符串ID)以避免冲突：
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

### 设置令牌<a name="config-tokens"></a>
可通过`config.json`文件让玩家配置模组。若玩家安装[通用模组设置菜单](https://www.nexusmods.com/stardewvalley/mods/5098)，可通过游戏内的菜单设置不同的值。

例如，使用配置值作为令牌和条件：

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

详见[玩家配置文档](config.md)。

### 动态令牌<a name="dynamic-tokens"></a>
动态令牌在`content.json`的`DynamicTokens`部分定义。每块定义使用以下字段：

字段   | 用途
------- | -------
`Name`  | 用于[令牌和条件](#introduction)的令牌名称。
`Value` | 设置的值（多个值用逗号分隔）。此字段支持[令牌](#introduction)，包括之前定义的动态令牌。
`When`  | _(可选)_ 仅当给定[条件](#introduction)匹配时设置值。未指定则始终匹配。

注意：
* 可列出任意数量的动态令牌。
* 同一名称的多个块中，最后一个匹配条件的块生效。
* 在`Value`和`When`字段可使用令牌。若使用 _之前定义的动态令牌_
  ，则使用之前块的最新有效值。使用令牌隐式添加`When`条件（若令牌不可用。如`{{season}}`则跳过块）。
* 动态令牌不可与全局令牌或玩家设置字段同名。

例如，`content.json`定义自定义`{{style}}`令牌，并根据天气加载不同作物贴图：

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

### 局部令牌<a name="local-tokens"></a>
局部令牌通过补丁的`LocalTokens`字段定义。令牌名称必须为纯字符串，但值可包含其他令牌。

**重要限制：**
* 直接在补丁定义的局部令牌不可用于`FromFile`和`Target`字段（因这些字段可使用`{{FromFile}}`和`{{Target}}`）。但通过父`Include`补丁继承的局部令牌可用于这些字段。
* 局部令牌始终视为动态文本，因此不可用于
  _仅_ 允许布尔值或数值的数据模型字段。该限制将在未来版本改进。

例如：
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

在`Include`补丁上设置时，局部令牌由所有加载的补丁继承。可用于实现模板化行为，为一组值应用补丁集。

例如，在`content.json`中：
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

添加`assets/add-hat.json`文件：
```json
{
   /*
   此文件每顶帽子加载一次，使用以下令牌：
      {{IdSuffix}}: 物品的唯一ID，如"DeerAntlers"。
      {{DisplayName}}: 帽子的翻译显示名称。
      {{Description}}: 帽子的翻译描述。
      {{Price}}: 帽子的售价。
   */
   "Changes": [
      // 添加帽子数据
      {
         "Action": "EditData",
         "Target": "Data/hats",
         "Entries": {
            "{{ModId}}_{{IdSuffix}}": "{{ModId}}_{{IdSuffix}}/{{Description}}/true/true//{{DisplayName}}/4/{{TextureNameInData}}"
         }
      },

      // 添加到商店里
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

## 输入参数<a name="input-arguments"></a>
### 概述<a name="overview-1"></a>
**输入参数** 是在`{{...}}`大括号内传递给令牌的值。输入可以是
_位置参数_（未命名值列表）或 _命名参数_。参数值以逗号分隔，命名参数以竖线分隔。

例如，`{{Random: a, b, c |key=some, value |example }}`有五个参数：三个位置参数`a`，`b`，`c`；命名参数`key`值为`some` 和 `value`；命名参数`example`值为空。

部分令牌识别输入参数以改变输出，具体见各令牌文档。例如，`Uppercase`将输入转为大写：
```js
"Entries": {
   "fri": "It's a beautiful {{uppercase: {{season}}}} day!" // 结果是It's a beautiful SPRING day!
}
```

### 全局输入参数<a name="global-input-arguments"></a>
全局输入参数由Content Patcher处理，适用于所有令牌（包括模组提供的令牌）。多个参数按从左到右顺序应用。

#### `contains`
`contains`用于搜索令牌的值。适用于任何令牌。

根据令牌是否包含给定值返回`true`或者`false`。主要用于[条件](#conditions)：

```js
// 玩家有铁匠或宝石学家职业
"When": {
   "HasProfession": "Blacksmith, Gemologist"
}

// 玩家同时有铁匠和宝石学家职业
"When": {
   "HasProfession |contains=Blacksmith": "true",
   "HasProfession |contains=Gemologist": "true"
}

// 非第一年
"When": {
   "Year |contains=1": "false"
}
```

也可用于占位符。例如，根据玩家是否有`Gemologist`宝石学家职业加载不同文件：
```js
{
    "Action": "EditImage",
    "Target": "Buildings/houses",
    "FromFile": "assets/gems-{{HasProfession |contains=Gemologist}}.png" // assets/gems-true.png
}
```

可指定多个值，返回是否有 _任意值_ 匹配： 
```js
// 玩家有铁匠或宝石学家职业
"When": {
   "HasProfession |contains=Blacksmith, Gemologist": "true"
}

// 玩家既无铁匠也无宝石学家职业
"When": {
   "HasProfession |contains=Blacksmith, Gemologist": "false"
}
```

#### `valueAt`
`valueAt`参数获取令牌中指定位置的值（首项为0）。若索引超出范围，返回空列表。

顺序取决于令牌，可通过[`patch summary unsorted`命令](troubleshooting.md#summary)查看。如`ChildNames`有固定顺序，大多数如`HasFlag`按游戏数据顺序排列，可能随存档变化。

例如

<table>
  <tr>
    <th>令牌</th>
    <th>值</th>
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
    <td><em>空列表</em></td>
  </tr>
</table>

负索引从列表末尾开始，-1为最后一项：

<table>
  <tr>
    <th>令牌</th>
    <th>值</th>
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
    <td><em>空列表</em></td>
  </tr>
</table>

### 自定义参数分割符号<a name="custom-input-value-separator"></a>
默认输入参数以逗号分隔，但有时需允许逗号出现在值中。可使用`inputSeparator`参数指定不同分隔符（可为一个或多个字符）。

例如，允许随机对话中的分隔符不为逗号：

```json
"Entries": {
   "fri": "{{Random: Hey, how are you? @@ Hey, what's up? |inputSeparator=@@}}"
}
```

**注意：** 应避免在分隔符中使用`{}|=:`，即使理论上技术有效。分隔符可能与令牌语法冲突，可能随Content Patcher版本改进。

## 随机<a name="randomization"></a>
### 概述
可使用`Random`令牌来随机化：
```js
{
   "Action": "Load",
   "Target": "Characters/Abigail",
   "FromFile": "assets/abigail-{{Random:hood, jacket, raincoat}}.png"
}
```

可使用固定键保持多个`Random`令牌同步（详见下文）：
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

此令牌是动态的，可能有意外冲突，详见下文。

### 独特属性
`Random`令牌是……

<ol>
<li>

**动态的。** Random令牌在每次加载补丁时重新选择，通常在新一天开始时。随机更新速率基于游戏+游戏内日期+输入字符串，因此重载存档不会改变已选随机值。

</li>
<li>

**独立的。** 每个`Random`变化是独立的。例如：

* 若补丁有多个`Target`，每个目标可能得到不同的`Random`值。
* 若`FromFile`字段的`Random`令牌在`HasFile`字段中使用，可能因不同选择导致文件加载失败。

需同步多个`Random`时，使用 _固定键_ 。
</li>
<li>

**公平的。** 每个选项概率相等。可通过重复值调整概率。例如，'red'出现概率是'blue'的两倍：
```js
{
   "Action": "Load",
   "Target": "Characters/Abigail",
   "FromFile": "assets/abigail-{{Random:red, red, blue}}.png"
}
```

</li>
<li>

**选项不含令牌时为有界的。** 。例如，若所有选项为'true'或'false'，可在布尔类型的条目中使用；若为数字，可在数值类型的条目中使用。

</li>
</ul>

### 更新频率
默认`Random`在每天开始时变化。若需在一天内变化，需：

* 指定[补丁更新频率](../author-guide.md#update-rate)使补丁更频繁更新。
* 使用[固定键](#pinned-keys)设置随更频繁变化的值作为固定键。例如，随时间变化：
  ```
  {{Random: a, b, c |key={{Time}} }}
  ```
  注意相同键的`{{Random}}`会同步值。可设置唯一值避免：
  ```
  {{Random: a, b, c |key=Abigail portraits {{Time}} }}
  ```

### 固定键<a name="pinned-keys"></a>
<dl>
<dt>基础固定键：</dt>
<dd>

若需多个`Random`同步选择（如保持角色肖像与像素小人贴图一致），可指定'固定键'。相同固定键的`Random`会做出相同选择（注意列表顺序需一致）。

例如，使用`abigail-outfit`固定键同步阿比盖尔的贴图和肖像：
key:
```js
{
   "Action": "Load",
   "Target": "Characters/Abigail, Portraits/Abigail",
   "FromFile": "assets/{{Target}}-{{Random:hood, jacket, raincoat |key=abigail-outfit}}.png"
}
```

可在固定键中使用令牌。例如，为每个角色单独同步：
```js
{
   "Action": "Load",
   "Target": "Characters/Abigail, Portraits/Abigail, Characters/Haley, Portraits/Haley",
   "FromFile": "assets/{{Target}}-{{Random:hood, jacket, raincoat |key={{TargetWithoutPath}}-outfit}}.png"
}
```

<dt>高级固定键：</dt>
<dd>

固定键影响内部随机数，而非选择本身。可配合不同值（甚至不同数量）实现更有趣的功能。

例如，确保阿比盖尔和海莉的服装不同：
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

<dt>固定键到底是什么？</dt>
<dd>

无固定键时，每个令牌独立选择：
```txt
{{Random: hood, jacket, raincoat}} = raincoat
{{Random: hood, jacket, raincoat}} = hood
{{Random: hood, jacket, raincoat}} = jacket
```

相同固定键时，保持同步：
```txt
{{Random: hood, jacket, raincoat |key=outfit}} = hood
{{Random: hood, jacket, raincoat |key=outfit}} = hood
{{Random: hood, jacket, raincoat |key=outfit}} = hood
```

固定键同步内部随机数，相同选项+相同键=相同值。

```txt
{{Random: hood, jacket, raincoat |key=outfit}} = 217437 modulo 3 choices = index 0 = hood
{{Random: hood, jacket, raincoat |key=outfit}} = 217437 modulo 3 choices = index 0 = hood
{{Random: hood, jacket, raincoat |key=outfit}} = 217437 modulo 3 choices = index 0 = hood
```

若选项顺序不同，相同索引对应不同值：
```txt
{{Random: hood, jacket, raincoat |key=outfit}} = 217437 modulo 3 choices = index 0 = hood
{{Random: jacket, raincoat, hood |key=outfit}} = 217437 modulo 3 choices = index 0 = jacket
```
</dd>
</dl>

## 进阶<a name="advanced"></a>
### 查询表达式<a name="query-expressions"></a>
_查询表达式_ 是一组可计算为数字，`true`/`false`或文本的算术和逻辑表达式。

#### 用法
使用`Query`执行查询表达式。可用作占位符或条件，可包含嵌套令牌：
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

文本值需用单引号包裹（包括返回文本的令牌）：
```js
"Query: '{{Season}}' = 'spring'": true
```

表达式不区分大小写，包括文本比较。

#### 注意事项
查询表达式功能强大，但需注意：

* 查询表达式 **很难验证** 无效表达式通常不会预先警告，仅在应用补丁时失败。需仔细测试表达式的功能，可以用[`patch parse`](troubleshooting.md#parse)检查新表达式。
* 查询表达式 **会计算扩展后的文本** 例如，玩家名含单引号`D'Artagnan`，时，以下表达式会因语法错误失败：
  ```js
  "Query: '{{PlayerName}}' LIKE 'D*'": true // 'D'Artagnan' LIKE 'D*'
  ```
* 可能返回技术性错误信息。
* 可能降低内容包可读性。

尽可能使用非表达式功能。例如：

<table>
<tr>
<th>使用查询表达式</th>
<th>不使用查询表达式</th>
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

#### 运算符
支持的运算符如下：

* 算术运算（如`5 + 5`）：

  运算符 | 效果
  -------- | ---------
  \+       | 加
  \-       | 减
  \*       | 乘
  /        | 除
  %        | 取模
  ()       | 组合

* 比较值（如`5 < 10`）：

  运算符 | 效果
  -------- | ---------
  `<`      | 小于
  `<=`     | 小于等于
  `>`      | 大于
  `>=`     | 大于等于
  `=`      | 等于
  `<>`     | 不等于

* 逻辑运算符：

  运算符 | 效果
  -------- | ------
  `AND`    | 与，两者都为真，如`{{Time}} >= 0600 AND {{Time}} <= 1200`。
  `OR`     | 或，至少一者为真，如`{{Time}} <= 1200 OR {{Time}} >= 2400`。
  `NOT`    | 非，取反，如`NOT {{Time}} > 1200`。

* 使用`()`组合运算避免歧义：

  ```js
  "Query: ({{Time}} >= 0600 AND {{Time}} <= 1200) OR {{Time}} > 2400": true
  ```

* 检查值是否存在于（`IN`）或不存在于（`NOT IN`）列表中：

  ```js
  "Query: '{{spouse}}' IN ('Abigail', 'Leah', 'Maru')": true
  ```

* 使用`LIKE`或`NOT LIKE`检查文本前缀/后缀。通配符`*`只能在字符串开始/结尾，且仅用于引号文本（例如 `LIKE '1'`会生效，但`LIKE 1`会报错）。

  ```js
  "Query: '{{spouse}}' LIKE 'Abig*'": true
  ```

### 模组提供令牌<a name="mod-provided-tokens"></a>
SMAPI模组可添加新令牌供内容包使用（见[_模组拓展性_](../extensibility.md)），用法与Content Patcher的令牌相同。例如，使用Json Assets的令牌：
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

使用模组提供的令牌需满足以下至少一项：
* 提供令牌的模组是您的内容包的[必需依赖](https://zh.stardewvalleywiki.com/模组:制作指南/APIs/Manifest#Dependencies_属性)。
* 或使用令牌的补丁中含有对于令牌提供模组的不可变（不使用任何令牌）`HasMod`条件：
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

### 别名<a name="aliases"></a>
_别名_ 为现有令牌添加可选替代名称，仅影响此内容包，可使用别名和原名。主要用于其他模组提供的长名令牌。

在`content.json`的`AliasTokenNames`字段定义别名，键为别名，值为原名：

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

`Include`补丁中的文件自动继承别名。

别名不可与全局令牌或设置令牌重名。

**注意：** 您也可以使用[动态令牌](#dynamic-tokens)给别的令牌取别名：

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

## 共同值<a name="common-values"></a>
这些是令牌中使用的预定义值，根据令牌文档的需要引用。

### 位置上下文<a name="location-context"></a>
部分令牌允许通过[输入参数](#input-arguments)选择世界区域：

例子 | 含义
------- | -------
`{{Weather}}`<br />`{{Weather: current}}` | 当前区域的天气。
`{{Weather: island}}` | 姜岛的天气。
`{{Weather: valley}}` | 鹈鹕镇的天气。

可能的上下文是：

值     | 含义
--------- | -------
`current` | 当前玩家所在区域，默认，无需指定。
`island`  | [姜岛](https://zh.stardewvalleywiki.com/姜岛).
`valley`  | 其他区域。

### 目标玩家<a name="target-player"></a>
部分令牌允许通过[输入参数](#input-arguments)选择玩家信息：

例子                                  | 含义
---------------------------------------- | -------
`{{HasFlag}}`<br />`{{HasFlag: currentPlayer}}` | 当前玩家的标志。
`{{HasFlag: hostPlayer}}`                | 房主玩家的标志。
`{{HasFlag: currentPlayer, hostPlayer}}` | 当前玩家 _和_ 主玩家的标志。
`{{HasFlag: anyPlayer}}`                 | 任意玩家的标志。
`{{HasFlag: 3864039824286870457}}`       | 指定ID的玩家的标志（该例子的ID是`3864039824286870457`）。

可能玩家类型：

值 | 含义
----- | -------
`currentPlayer` | 当前安装模组的玩家。
`hostPlayer` | 多人游戏主机玩家，单机或当前玩家为主机时同`currentPlayer`。
`anyPlayer` | 所有玩家的组合值，无论是否在线。
_玩家ID_ | 指定玩家的唯一多人ID，如`3864039824286870457`.

## 另见<a name="see-also"></a>
* 其他操作和选项请参考[模组作者指南](../author-guide.md)
