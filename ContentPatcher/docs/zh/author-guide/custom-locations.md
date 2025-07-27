← [模组作者指南](../author-guide.md)

> [!WARNING]  
> **此功能已弃用，不应该使用于新内容包。**  
> 1.6 的新模组应该使用游戏内置的 `Data/Locations` 来添加自定义地点，请参阅[模组：地点数据](https://zh.stardewvalleywiki.com/模组:地点数据)。

----

`CustomLocations` 功能允许您添加新地点，包括它们内部的地图和传送点。Content Patcher 会自动处理 NPC 寻路，对象持久性等。

**仅在添加新地点时需要使用此功能。**若要编辑现有地点，请使用 [`EditMap`](action-editmap.md)。

**🌐 其他语言：[en (English)](../../author-guide/custom-locations.md)。**

## 目录
* [介绍](#introduction)
  * [地图和地点](#maps-vs-locations)
* [用法](#usage)
  * [格式](#format)
  * [示例](#examples)
* [常见问题](#faqs)
  * [游戏中如何抵达我的自定义地点？](#how-do-i-get-to-my-location-in-game)
  * [地点可以添加条件吗？](#can-i-make-the-location-conditional)
  * [地点可以重命名吗？](#can-i-rename-a-location)
* [参见](#see-also)

## 介绍<a name="introduction"></a>
### 地图和地点<a name="maps-vs-locations"></a>

虽然地图和地点经常互换使用，代码里的“地图（map）”和“地点（location）”是两个不同的概念。区别对于理解此功能的工作方式至关重要：

* [**地图**](https://zh.stardewvalleywiki.com/模组:地图)是一种描述图块、分布、图块表和地图/图块属性的素材资源。每当您加载游戏和每次模组更改地图时，整个地图会重新加载。
* [**地点**](https://zh.stardewvalleywiki.com/模组:制作指南/游戏基本架构#GameLocation_et_al)是游戏代码的一部分，用于管理某区域及其内部的所有对象（包括非地图实体，如玩家）。地点会被写入存档文件，并且仅在加载存档文件时加载。

换句话说，**地点**（游戏代码）包含**地图**（从 `Content` 加载的素材）：

```
┌─────────────────────────────────┐
│ 地点                             │
│   - 物品                         │
│   - 家具                         │
│   - 农作物                       │
│   - 灌木和树木                   │
│   - NPC和玩家                    │
│   - 其他                         │
│                                  │
│   ┌─────────────────────────┐   │
│   │ 地图资产                  │   │
│   │   - 图块排列              │   │
│   │   - 地图/图块属性         │   │
│   │   - 图块表                │   │
│   └─────────────────────────┘   │
└─────────────────────────────────┘
```

## 用法<a name="usage"></a>
### 格式<a name="format"></a>
一个 `CustomLocations` 补丁由 `Changes`（请参阅下文[示例](#examples)）下的一个模型组成，包含以下字段。

<table>
<tr>
<th>字段</th>
<th>用途</th>
</tr>
<tr>
<td><code>Name</code></td>
<td>

地点的唯一内部名称。

此名字：
* 必须仅包含字母、数字或下划线。
* 必须以您的模组的 [Manifest `UniqueId`](https://zh.stardewvalleywiki.com/模组:制作指南/APIs/Manifest)作为开头
  （例如 `Your.ModId_`）以防止命名冲突。出于旧版支持原因，您也可以用 `Custom_` 作为开头，但不推荐。
* 必须**具有唯一性**，所以强烈建议使用模组 ID 作为前缀。如果两个内容包添加了 `Name` 值相同的地点，那么两个地点都无法生效，并抛出错误提示。如果玩家此时忽略该提示并保存游戏，地点内的所有实体对象都将会永久丢失。

此字段不能包含[令牌](../author-guide.md#tokens)。

</td>
</tr>
<tr>
<td><code>FromMapFile</code></td>
<td>

地图素材在内容包文件夹里的相对路径（`.tmx`, `.tbin`, 或 `.xnb`）。

此字段不能包含[令牌](../author-guide.md#tokens)，但可以在加载后使用 [`EditMap`](action-editmap.md) 进行更改。

</td>
</tr>
<td><code>MigrateLegacyNames</code></td>
<td>

（可选）一个列表，包含可能出现在存档文件中的旧地点名称，对应 `Name`。此功能设计为用于迁移其他模组添加的地点，一般情况下不应该使用。请参阅[地点可以重命名吗？](#can-i-rename-a-location)

此字段不能包含[令牌](../author-guide.md#tokens)。

</td>
</tr>
</table>

### 示例<a name="examples"></a>

假设您想在阿比盖尔的房间内添加一个可以进入的橱柜。这个示例进行了三个更改：

1. 添加带有基础地图的游戏内地点；
2. 在阿比盖尔房间内添加一个传送点；
3. 添加一个地图编辑的条件（可选）。

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
      // 在阿比盖尔房间里加一个传送点；
      {
         "Action": "EditMap",
         "Target": "Maps/SeedShop",
         "AddWarps": [
            "8 10 {{ModId}}_AbigailCloset 7 20"
         ]
      },

      // 添加一个地图编辑的条件
      {
         "Action": "EditMap",
         "Target": "Maps/{{ModId}}_AbigailCloset",
         "FromFile": "assets/abigail-closet-clean.tmx",
         "When": {
            "HasFlag": "AbigailClosetClean" // 示例 mailflag
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

不能，移除地点会将其内部的所有信息全部删除。就如同原版游戏一样，即使玩家当前还不能够访问某地点，也会添加所有地点。

有许多方法可以决定玩家可否进入该地点，例如，您可以使用 [`EditMap`](action-editmap.md) 添加传送点或添加障碍。

### 地点可以重命名吗?<a name="can-i-rename-a-location"></a>

**若不慎重命名地点，可能会导致玩家永久丢失玩家在该地点内所做的更改！**

Content Patcher 允许您重命名原有地点名。当加载存档时，如果某地点没有对应 `Name` 但有旧名称，旧名称的数据会被加载到新地点中。当玩家保存游戏时，旧地点会永久重命名为新地点。

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

`MigrateLegacyNames` 可以是任意格式，但是有两个限制：

* 必须具有**全局唯一性**，不能和任意其他 `Name` 或 `MigrateLegacyNames` 重名，包括所有由玩家安装的模组添加的地点。
* 不能和原版游戏的地点名重复。

## 参见<a name="see-also"></a>
* 其他操作和选项请参见[模组作者指南](../author-guide.md)。
