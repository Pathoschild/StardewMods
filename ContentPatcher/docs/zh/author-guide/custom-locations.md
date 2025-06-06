← [模组作者指南](../author-guide.md)

> [!WARNING]  
> **此功能已弃用，不应该使用于新内容包。**  
> 1.6的新模组应该使用游戏内置的`Data/Locations`来添加自定义地点，详见[模组:地点数据](https://zh.stardewvalleywiki.com/模组:地点数据)。

----

`CustomLocations`功能允许你添加新地点，并配有自己的地图和传送点。Content Patcher会自动处理NPC探索，对象持久性等。

**只有新增地点时才需要此功能。** 编辑原有地点用[`EditMap`](action-editmap.md)即可。

**🌐 其他语言： [en (English)](../../author-guide/custom-locations.md)。**

## 目录
* [介绍](#introduction)
  * [地图和地点](#maps-vs-locations)
* [用法](#usage)
  * [格式](#format)
  * [示例](#examples)
* [常见问题](#faqs)
  * [游戏中如何抵达我的自定义地点？](#how-do-i-get-to-my-location-in-game)
  * [地点可以添加条件吗?](#can-i-make-the-location-conditional)
  * [地点可重命名吗?](#can-i-rename-a-location)
* [参见](#see-also)

## 介绍<a name="introduction"></a>
### 地图和地点<a name="maps-vs-locations"></a>

虽然地图和地点经常互换使用，代码里的“地图（map）”和“地点（location）”是两个不同的概念。区别对于理解此功能的工作方式至关重要：

* [**地图**](https://zh.stardewvalleywiki.com/模组:地图)是一个种素材，描述图块，分布，图块表，和地图/图块属性。每当你加载游戏和每次模组更改地图时，整个地图会重新加载。
* [**地点**](https://zh.stardewvalleywiki.com/模组:制作指南/游戏基本架构#GameLocation_et_al)是游戏中管理某区域的代码（包括非地图实体，如玩家）。地点会保存到存档文件中，并只在加载存档时加载一次。

换句话说，_地点_(游戏代码) 包含 _地图_（从`Content`加载的素材）：

```
┌─────────────────────────────────┐
│ 地点                             │
│   - 物品                         │
│   - 家具                         │
│   - 农作物                       │
│   - 灌木和树木                    │
│   - NPC和玩家                    │
│   - 其他                         │
│                                 │
│   ┌─────────────────────────┐   │
│   │ 地图资产                  │   │
│   │   - 图块排列              │   │
│   │   - 地图/图块属性         │   │
│   │   - 图块表               │   │
│   └─────────────────────────┘   │
└─────────────────────────────────┘
```

## 用法<a name="usage"></a>
### 格式<a name="format"></a>

自定义地点使用`content.json`里的`CustomLocations`字段添加（在`Changes`字段以外）。这是一个模型列表，有一下字段：

<table>
<tr>
<th>字段</th>
<th>用途</th>
</tr>
<tr>
<td><code>Name</code></td>
<td>

地点的独有内置名。

此名字：
* 必须仅包含字母数字或下划线字符。
* 必须以[你模组的manifest `UniqueId`](https://zh.stardewvalleywiki.com/模组:制作指南/APIs/Manifest)作为开头
  (like `Your.ModId_`)，防止冲突。 出于旧版支持原因，你也可以用`Custom_`作为开头，但这不推荐。
* 必须 **全局独特** ，所以强烈推荐使用模组ID作为前缀。如果两个内容包添加了名称重复的地点，两个地点都不生效。如果玩家此时保存游戏，地点内的东西将会永久丢失。

此字段不能用[令牌](../author-guide.md#tokens).

</td>
</tr>
<tr>
<td><code>FromMapFile</code></td>
<td>

地图素材在内容包文件夹里的相对路径(`.tmx`, `.tbin`, 或 `.xnb`)。

此字段不能用[令牌](../author-guide.md#tokens)，但加载后可以用[`EditMap`](action-editmap.md)进行更改。

</td>
</tr>
<td><code>MigrateLegacyNames</code></td>
<td>

_(可选)_ 原来使用过并可能出现在存档里的的地点名，对应`Name`。这只是帮助迁移其他模组添加的地点的功能，平常不应该使用。详见[_地点可重命名吗?_](#can-i-rename-a-location)

此字段不能用[令牌](../author-guide.md#tokens)。

</td>
</tr>
</table>

### 示例<a name="examples"></a>

假设你想给阿比盖尔一个步入式壁橱。这个例子实现三个更改：

1. 添加新地点和基本地图；
2. 从阿比盖尔房间里加一个传送；
3. 特定条件下编辑地图 (可选).


```js
{
   "Format": "2.7.0",

   "CustomLocations": [
      // 添加新地点；
      {
         "Name": "{{ModId}}_AbigailCloset",
         "FromMapFile": "assets/abigail-closet.tmx"
      }
   ],

   "Changes": [
      // 从阿比盖尔房间里加一个传送；
      {
         "Action": "EditMap",
         "Target": "Maps/SeedShop",
         "AddWarps": [
            "8 10 {{ModId}}_AbigailCloset 7 20"
         ]
      },

      // 特定条件下编辑地图
      {
         "Action": "EditMap",
         "Target": "Maps/{{ModId}}_AbigailCloset",
         "FromFile": "assets/abigail-closet-clean.tmx",
         "When": {
            "HasFlag": "AbigailClosetClean" // 示例mailflag
         }
      }
   ]
}
```

## 常见问题<a name="faqs"></a>
### 游戏中如何抵达我的自定义地点？<a name="how-do-i-get-to-my-location-in-game"></a>

`CustomLocations`仅添加地点。不要忘记给玩家进入地点的方法，如用[`EditMap`](action-editmap.md)添加传送。测试时可用`debug warp <location name>` [console
command](https://zh.stardewvalleywiki.com/模组:控制台命令#控制台命令)

### 地点可以添加条件吗?<a name="can-i-make-the-location-conditional"></a>

不能，移除地点会将里面所有东西都删除。原版游戏也会不管玩家可否进入都加载所有地点。

有很多控制玩家可否进入的功能，如有条件的用[`EditMap`](action-editmap.md)添加传送，或添加障碍。

### 地点可重命名吗?<a name="can-i-rename-a-location"></a>

**重命名地点可能会导致玩家永久丢失里面的东西，请小心**

Content Patcher允许你定义原有地点名。当加载存档时，如果某地点没有对应`Name`但有旧版名，旧版名的数据会被加载到新地点中。当玩家保存游戏时，旧地点会永久重命名为新地点。

例如：

```js
{
   "Format": "2.7.0",
   "CustomLocations": [
      {
         "Name": "{{ModId}}_AbigailCloset",
         "FromMapFile": "assets/abigail-closet.tmx",
         "MigrateLegacyNames": [ "Custom_AbbyRoom" ]
      }
   ]
}
```

旧版名可以有任意格式，但是有两个限制：

* 必须 **全局独特** ，不能和任意其他`Name`或`MigrateLegacyNames`重叠，包括玩家所有安装过的模组。
* 不能和原版游戏的地点名一样。

## 参见<a name="see-also"></a>
* [模组作者指南](../author-guide.md)
