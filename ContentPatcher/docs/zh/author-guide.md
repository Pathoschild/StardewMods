← [README](README.md)

此文档描述如何创建一个Content Patcher的内容包。

**其他信息请参见[主README](README.md)**。

**🌐 其他语言： [en (English)](../author-guide.md)。**

## 目录
* [介绍](#introduction)
  * [什么是Content Patcher？](#what-is-content-patcher)
  * [内容包长什么样？](#what-does-a-content-pack-look-like)
  * [什么是数据素材？](#what-is-an-asset)
* [开始](#get-started)
  * [创建内容包](#create-the-content-pack)
  * [格式版本](#format-version)
  * [更改](#changes)
* [特征](#features)
  * [操作](#actions)
  * [自定义地点](#custom-locations)
  * [Tokens和条件](#tokens)
  * [玩家设置](#player-config)
  * [翻译](#translations)
  * [文本操作](#text-operations)
  * [触发动作](#trigger-actions)
* [故障排除](#troubleshoot)
* [常见问题](#faqs)
  * [多久应用一次补丁更改？](#update-rate)
  * [Content Patcher更新是否与旧版本兼容？](#are-content-patcher-updates-backwards-compatible)
  * [如何更改另一种语言的素材？](#how-do-i-change-assets-in-another-language)
  * [多个补丁如何交互？](#how-do-multiple-patches-interact)
  * [已知限制](#known-limitations)
* [参见](#see-also)

## 介绍<a name="introduction"></a>
### 什么是Content Patcher？<a name="what-is-content-patcher"></a>
Content Patcher 可让你只使用 JSON 文件更改游戏内容。JSON 是一种文本格式，
因此不需要学会编程也能使用。

你可以对游戏进行各种修改：

* 更改图像、对话、地图等；
* 添加自定义项目、果树、地点等；
* 改变商店库存；
* 还有很多其他的修改。

你还可以对游戏进行动态调整。例如，在玩家还没有娶阿比盖尔时，每冬日周末晚上下雪天时提高咖啡价格。

[维基上的模组制作文档](https://zh.stardewvalleywiki.com/模组:目录)通常是为Content Patcher内容包作者编写的，所以你可以在那里找到很多例子。

### 内容包长什么样？<a name="what-does-a-content-pack-look-like"></a>
内容包只是一个文件夹，其中包含两个文本文件： `manifest.json` （里面有很多信息，比如你的模组名）和 `content.json` （里面写着你要修改的内容）。你的
文件夹中还可能有图像或其他文件，这些文件通常被放在 `assets` 子文件夹中。
```
📁 Mods/
   📁 [CP] YourModName/
      🗎 content.json
      🗎 manifest.json
      📁 assets/
         🗎 example.png
```

例如，这个`content.json`替代阿比盖尔的肖像：

```js
{
    "Format": "2.7.0",
    "Changes": [
        {
            "Action": "Load",
            "Target": "Portraits/Abigail",
            "FromFile": "assets/abigail.png"
        }
    ]
}
```

通过使用动作、令牌、条件等功能，你可以使用 Content Patcher 做 _更多_ 的事。本文档将在接下来一一介绍：

### 什么是数据素材？<a name="what-is-an-asset"></a>
 _素材(assets)_ 是指游戏从`Content`内容文件夹（或者从其他模组文件夹）中加载的图像、数据模型或地图。这些都是 Content Patcher 可以更改的内容。

素材名称不包括 `Content` ，[语言代码](#how-do-i-change-assets-in-another-language)，或文件扩展名。例如，`Content/Maps/spring_beach.xnb`和`Content/Maps/spring_beach.fr-FR.xnb`是相同的`Maps/spring_beach`素材。

You can [unpack the game's content files](https://zh.stardewvalleywiki.com/模组:编辑_XNB_文件#unpacking)
你可以[解包游戏的内容文件](https://zh.stardewvalleywiki.com/模组:编辑_XNB_文件#解包游戏文件)并查看内容。以下是`Portraits/Abigail`包含的内容：

![](../screenshots/sample-asset.png)

因此，如果你想更改阿比盖尔的肖像，你可以使用Content Patcher加载或编辑`Portraits/Abigail`然后像前面的示例代码一样更改该图片。

## 开始<a name="get-started"></a>
### 创建内容包<a name="create-the-content-pack"></a>
1. 安装[SMAPI](https://www.nexusmods.com/stardewvalley/mods/2400)和[Content
   Patcher](https://www.nexusmods.com/stardewvalley/mods/1915).
2. 在`Mods`文件夹以下创建一个新文件夹，并命名为`[CP] YourModName` （把`YourModName`替换成你的模组名字）。
3. 创建一个新`manifest.json`文件，包含以下内容：
   ```js
   {
       "Name": "Your Mod Name",
       "Author": "Your Name",
       "Version": "1.0.0",
       "Description": "One or two sentences about the mod.",
       "UniqueID": "YourName.YourModName",
       "UpdateKeys": [], //当你发布模组时，更新键填写在这里详见 https://zh.stardewvalleywiki.com/模组:制作指南/APIs/Update_checks 来获取
       "ContentPackFor": {
           "UniqueID": "Pathoschild.ContentPatcher"
       }
   }
   ```
4. 把`Name`, `Author`, `Description`, 和`UniqueID` 改成对应你的模组的值. (不要改`ContentPackFor`下的`UniqueID`！)
5. 创建一个新`content.json`文件，包含以下内容：
   ```js
   {
       "Format": "2.7.0",
       "Changes": [
           // 这里面是你要更改的内容
       ]
   }
   ```

好了，你现在已经创建了一个可以用的Content Patcher内容包，虽然它目前什么都做不到。

### 格式版本<a name="format-version"></a>
`Format`字段是内容包用的 Content Patcher 版本号。用来保持内容包的版本兼容。

你应该使用最新的格式版本（现在是`2.5.0`）来启用最新功能，避免某些被弃用的代码，减少加载模组的时间。

### 更改<a name="changes"></a>
`Changes`字段描述了你想更改的内容，每个条目都被称为 **补丁**，并说明要执行的具体操作：编辑图片、更改对话等。
你可以列出任意数量的补丁，也可以对同一文件应用多个补丁（它们将按照所列顺序一个个使用）。

## 特征<a name="features"></a>
**注意：** 这些内容按内容包的使用频率排序。你不需要了解或使用所有特征。

### 操作<a name="actions"></a>
每个补丁都有一个`Action`操作字段，即你要进行更改的类型。
关于每个操作的更多信息，请参阅每个操作的文档页面（链接在下方）。

<table>
<tr>
<th>操作</th>
<th>概述</th>
</tr>
<tr>
<td><code>Load</code></td>
<td>

`Load`操作替换一整个素材。

此例子将阿比盖尔原有的肖像替换成你提供的你提供的`assets/abigail.png`图像：
```js
{
   "Format": "2.7.0",
   "Changes": [
      {
         "Action": "Load",
         "Target": "Portraits/Abigail",
         "FromFile": "assets/abigail.png"
      }
   ]
}
```

出于兼容性考虑，能使用`Edit*`操作时不推荐使用`Load`。

详见[`Action: Load`文档](author-guide/action-load.md)。

</td>
</tr>
<tr>
<td><code>EditData</code></td>
<td>

`EditData` 编辑数据素材中的字段和条目。这支持简单字符串到字符串形的素材，如`Data/Achievements`，也支持模型类素材，如`Data/Objects`. 多个内容包可以编辑同一个素材。

你可以：
* 添加，编辑或删除条目；
* 在列表中重新排序条目；
* 或在条目中编辑单个字段。

此例子把苔藓汤的价格改到80（详见[物体字段](https://zh.stardewvalleywiki.com/模组:物体)。

```js
{
    "Format": "2.7.0",
    "Changes": [
        {
            "Action": "EditData",
            "Target": "Data/Objects",
            "Fields": {
                "MossSoup": {
                    "Price": 80
                }
            }
        }
    ]
}
```

`EditData`可实现的效果还有很多，包括添加完全自定义的物品，果树等。
详见[`Action: EditData`文档](author-guide/action-editdata.md)。

</td>
</tr>
<tr>
<td><code>EditImage</code></td>
<td>

`EditImage` 更改游戏中的图像素材。多个内容包可以编辑同一个素材。

你可以：
* 编辑或替换图像的任何部分；
* 将新图像叠加到现有图像上，支持透明度；
* 或扩展图像大小（例如，将更多的贴图添加到贴图集）。

此例子把原版[吞拿鱼](https://zh.stardewvalleywiki.com/金枪鱼)替换成你提供的`assets/tuna.png`图片。

```js
{
   "Format": "2.7.0",
   "Changes": [
      {
         "Action": "EditImage",
         "Target": "Maps/springobjects",
         "FromFile": "assets/fish-object.png",
         "ToArea": { "X": 160, "Y": 80, "Width": 16, "Height": 16 }
      }
   ]
}
```

详见[`Action: EditImage`文档](author-guide/action-editimage.md)。

</td>
</tr>
<tr>
<td><code>EditMap</code></td>
<td>

`EditMap`可更改游戏中地图的一部分。多个内容包可以编辑同一张地图。

这让你可以——
* 更改地图属性和图块属性；
* 将本地地图覆盖到游戏地图的一部分（有多种合并选项）；
* 添加自定义图块；
* 调整地图大小（例如在现有游戏地点添加更多内容）。

例如，将城镇广场替换为内容文件夹中的自定义版本：
```js
{
    "Format": "2.7.0",
    "Changes": [
        {
            "Action": "EditMap",
            "Target": "Maps/Town",
            "FromFile": "assets/town.tmx",
            "ToArea": { "X": 22, "Y": 61, "Width": 16, "Height": 13 }
        }
    ]
}
```

详见[`Action: EditMap`文档](author-guide/action-editmap.md)。

</td>
</tr>
<tr>
<td><code>Include</code></td>
<td>

`Include`从另外一个JSON文件里加载更多补丁。这只是将内容包组织到子文件的方式，而不是将所有内容都放在一个`content.json`中。被引用的补丁和`content.json`里的补丁功能一致。

例如，你可以将其与[Tokens和条件](#tokens)结合起来加载动态文件：
```js
{
    "Format": "2.7.0",
    "Changes": [
        {
            "Action": "Include",
            "FromFile": "assets/john_{{season}}.json"
        }
    ]
}
```

详见[`Action: Include`文档](author-guide/action-include.md)。

</td>
</tr>
</table>

### 自定义地点<a name="custom-locations"></a>
`CustomLocations`可让你添加新的游戏地点，并为其配备自己的地图和传送点。 Content Patcher会自动处理NPC寻路，对象持续时间等问题。

详见[自定义地点指南](author-guide/custom-locations.md)。

### Tokens和条件<a name="tokens"></a><a name="conditions"></a>
前面的章节介绍了如何进行静态更改，但你也可以使用tokens和条件来进行 _动态_ 更改。

例如，你可以——
* 根据季节、回答的对话问题等多种因素更换补丁、游戏进度等。
* 使用随机、算术和动态查询；
* 还有更多别的动态更改。

例如，这让阿比盖尔在每个季节都有不同的肖像：

```js
{
    "Format": "2.7.0",
    "Changes": [
        {
            "Action": "Load",
            "Target": "Portraits/Abigail",
            "FromFile": "assets/abigail-{{season}}.png"
        }
    ]
}
```

这让阿比盖尔结婚跟你结婚后使用不同的肖像

```js
{
    "Format": "2.7.0",
    "Changes": [
        {
            "Action": "Load",
            "Target": "Portraits/Abigail",
            "FromFile": "assets/abigail-married.png",
            "When": {
                "Spouse": "Abigail"
            }
        }
    ]
}
```

详见[tokens和条件指南](author-guide/tokens.md)。

### 玩家设置<a name="player-config"></a>
你可以让玩家使用`config.json`文件设置你的模组。如果玩家有[通用模组设置菜单GMCM](https://www.nexusmods.com/stardewvalley/mods/5098)，玩家还能
通过游戏内的选项菜单设置不同的更改。

例如，你可以将设置值用作[Tokens和条件](#tokens):

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

详见[玩家设置指南](author-guide/config.md)。

### 翻译<a name="translations"></a>
你可以在模组中添加翻译文件，并通过`i18n` token访问翻译文件。如果某些内容没有翻译成当前的
语言，Content Patcher将自动处理，显示默认（`default`）文本。

例如，如果你的`i18n`文件包含一个关键字为`rainy-day`的翻译，你可以在任何支持[Tokens和条件](#tokens)的Content Patcher 字段里访问它：

```js
{
    "Format": "2.7.0",
    "Changes": [
        {
            "Action": "EditData",
            "Target": "Characters/Dialogue/MarriageDialogueAbigail",
            "Entries": {
                "Rainy_Day_4": "{{i18n: rainy-day}}"
            }
        }
    ]
}
```

更多信息详见[翻译功能](author-guide/translations.md)。

### 文本操作<a name="text-operations"></a>
文本操作可让你根据文本字段的已有值进行修改，而不只是设置一个新的值。例如，你可以在当前值上添加其他文本。

此例子将河豚添加到村民的普遍喜好中：

```js
{
    "Action": "EditData",
    "Target": "Data/NPCGiftTastes",
    "TextOperations": [
        {
            "Operation": "Append",
            "Target": ["Entries", "Universal_Love"],
            "Value": "127",
            "Delimiter": " "
        }
    ]
}
```

更多信息详见[文本操作](author-guide/text-operations.md)。

### 触发动作<a name="trigger-actions"></a>
Content Patcher 为特定使用场景添加自定义[触发动作](https://zh.stardewvalleywiki.com/模组:触发动作)，例如更新已有存档内容中的内置ID。

详见[Content Patcher的触发动作指南](author-guide/trigger-actions.md)。

## 故障排除<a name="troubleshoot"></a>
详见[故障排除指南](author-guide/troubleshooting.md)。

## 常见问题<a name="faqs"></a>

### 多久应用一次补丁更改？<a name="update-rate"></a>
每次加载数据素材时，你的补丁都会 **应用数据素材** ，但它们会通过不同的条件 **更新字段** 。例如，假设你有这样一个补丁：
```js
{
    "Action": "EditMap",
    "Target": "Maps/Town",
    "SetProperties": {
        "CurrentTime": "{{Time}}"
    }
}
```

当游戏中的一天开始时，Content Patcher会将此补丁更新为`"CurrentTime": "600"`。如果此补丁在当天晚些时候重新生效，它依然是`"CurrentTime": "600"`。

你可以使用`Update`字段实现更频繁的更新。

更新频率            | 作用
------------------ | ------
`OnDayStart`       | _(default)_ 游戏中的一天开始时更新。没有`Update`字段的补丁将默认使用此更新频率。
`OnLocationChange` | 玩家传送到另一地图时更新。
`OnTimeChange`     | 时间变换时更新（如6:10到6:20）。
_多种更新频率_         | 您可以指定由逗号分隔的多个值，如`"Update": "OnLocationChange, OnTimeChange"`。

此例子在游戏内时间变换时把补丁中`CurrentTime`的值更新为现在时间，然后重新生效。
```js
{
    "Action": "EditMap",
    "Target": "Maps/Town",
    "SetProperties": {
        "CurrentTime": "{{Time}}"
    },
    "Update": "OnTimeChange"
}
```

### Content Patcher更新是否与旧版本兼容？<a name="are-content-patcher-updates-backwards-compatible"></a>
兼容。详见[作者迁移指南](author-migration-guide.md)。


### 如何更改另一种语言的素材？<a name="how-do-i-change-assets-in-another-language"></a>
**默认影响所有语言**

`Target`字段里的素材名称不包含语言。如果你使用`"Target": "Dialogue/Abigail"`并把游戏语言设为德语，`Content/Dialogue/Abigail.de-DE.xnb`的内容将会被编辑。如果你希望某个更改在所有语言中生效，你不需要做任何特殊处理。

如果你想编辑特定语言，你可以加一个语言条件，如下：
```js
{
   "Action": "EditImage",
   "Target": "LooseSprites/Cursors",
   "FromFile": "assets/cursors.de.png",
   "When": {
      "Language": "de"
   }
}
```

你也可以在某语言有翻译版文档存在时自动加载它，这样可以在有翻译版时自动使用翻译版，无翻译版时默认使用原语言版。

```js
// 如果它存在于内容包中，使用翻译版图片
{
   "Action": "EditImage",
   "Target": "LooseSprites/Cursors",
   "FromFile": "assets/cursors.{{language}}.png",
   "When": {
      "HasFile:{{FromFile}}": true
   }
},

// 默认使用未翻译的版本
{
   "Action": "EditImage",
   "Target": "LooseSprites/Cursors",
   "FromFile": "assets/cursors.png",
   "When": {
      "HasFile: assets/cursors.{{language}}.png": false
   }
},
```


### 多个补丁如何交互？<a name="how-do-multiple-patches-interact"></a>
同一文件可使用任意数量的补丁。`Action: Load`总是先于其他操作。
但除此之外，每个补丁都是按顺序应用的。每个补丁完成后，下一个
补丁会把修改过的数据素材合并并输入。

在一个内容包内，补丁会按照在`content.json`中列出的顺序应用。 
如果你有多个内容包，每个内容包都会按照 SMAPI 加载的顺序应用；如果你需要依赖另一个内容包，请参阅[manifest中的依赖](https://zh.stardewvalleywiki.com/模组:制作指南/APIs/Integrations).

### 已知限制<a name="known-limitations"></a>
某些游戏素材具有特殊逻辑。这不是Content Patcher特有的限制，但为了保险起见故在此列出这些限制。

asset | notes
----- | -----
`Characters/Dialogue/*` | 对话是在一天开始时设置的，因此设置[自定义更新速度](#update-rate)不会影响一天开始后的对话。（不过你可以用[特定位置对话键](https://zh.stardewvalleywiki.com/模组:对话#地点对话)来规避这个问题）。
`Characters/Farmer/accessories` | 附件的数量是硬编码，因此自定义附件需要替换现有附件。
`Characters/Farmer/skinColors` | 皮肤颜色的数量是硬编码，因此自定义颜色需要替换现有颜色。
`Data/SpecialOrders` | 游戏会在 _保存之前_ 缓存该素材的副本，并在首次打开会话的特殊订单板时加载单独的副本。有条件地添加/删除特殊命令时要非常小心，因为当玩家试图从新列表中接受缓存列表中不存在的特殊订单命令时，可能会导致游戏崩溃。
`Maps/*` | 参见维基上的[地图编辑中的潜在问题](https://zh.stardewvalleywiki.com/模组:地图#潜在问题)。

## 参见<a name="see-also"></a>
* 其他信息详见[README](README.md)
* [帮助](https://zh.stardewvalleywiki.com/模组:帮助)
