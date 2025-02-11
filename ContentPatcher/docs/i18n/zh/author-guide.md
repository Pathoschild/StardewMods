← [README](README.md)

This document helps mod authors create a content pack for Content Patcher.

**See the [main README](README.md) for other info**.

## 内容
* [介绍](#introduction)
  * [什么是Content Patcher？](#what-is-content-patcher)
  * [内容包长什么样？](#what-does-a-content-pack-look-like)
  * [什么是数据资产？](#what-is-an-asset)
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
  * [如何更改另一种语言的资产？](#how-do-i-change-assets-in-another-language)
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

你还可以对游戏进行动态调整。例如，除非玩家娶了阿比盖尔，否则冬天的周末晚上都会下雪。

[维基上的修改文档](https://zh.stardewvalleywiki.com/%E6%A8%A1%E7%BB%84:%E7%9B%AE%E5%BD%95)通常是为Content Patcher内容包作者编写的，所以你可以在那里找到很多例子。

### 内容包长什么样？<a name="what-is-content-patcher"></a>
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

For example, here's a `content.json` which replaces Abigail's portraits with your own image:

```js
{
    "Format": "2.5.0",
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

### 什么是数据资产？<a name="what-is-an-asset"></a>
 _资产(assets)_ 是指游戏从`Content`内容文件夹（或者从其他模组文件夹）中加载的图像、数据模型或地图。这些都是 Content Patcher 可以更改的内容。

资产名称不包括 "内容" ，即[语言代码](#how-do-i-change-assets-in-another-language)，或文件扩展名。例如，`Content/Maps/spring_beach.xnb`和`Content/Maps/spring_beach.fr-FR.xnb`是相同的`Maps/spring_beach`资产。

You can [unpack the game's content files](https://stardewvalleywiki.com/Modding:Editing_XNB_files#unpacking)
to see what they contain. Here's what `Portraits/Abigail` contains:

![](screenshots/sample-asset.png)

因此，如果你想更改阿比盖尔的肖像，可以使用Content Patcher加载或编辑`Portraits/Abigail`然后像前面的示例代码一样更改该图片。

## 开始<a name="get-started"></a>
### 创建内容包<a name="create-the-content-pack"></a>
1. 安装[SMAPI](https://www.nexusmods.com/stardewvalley/mods/2400)和[Content
   Patcher](https://www.nexusmods.com/stardewvalley/mods/1915).
2. Create an empty folder in your `Mods` folder, and name it `[CP] YourModName`. Replace
   `YourModName` with a unique name for your mod.
3. Create a `manifest.json` file with this content:
   ```js
   {
       "Name": "Your Mod Name",
       "Author": "Your Name",
       "Version": "1.0.0",
       "Description": "One or two sentences about the mod.",
       "UniqueID": "YourName.YourModName",
       "UpdateKeys": [], //当你发布模组时，更新键填写在这里详见 https://zh.stardewvalleywiki.com/%E6%A8%A1%E7%BB%84:%E5%88%B6%E4%BD%9C%E6%8C%87%E5%8D%97/APIs/Update_checks 来获取
       "ContentPackFor": {
           "UniqueID": "Pathoschild.ContentPatcher"
       }
   }
   ```
4. Change the `Name`, `Author`, `Description`, and `UniqueID` values to describe your mod. (Don't
   change the `UniqueID` under `ContentPackFor`!)
5. Create a `content.json` file with this content:
   ```js
   {
       "Format": "2.5.0",
       "Changes": [
           // 这里面是你要更改的内容
       ]
   }
   ```

好了，你现在已经创建了一个可以用的 Content Patcher 包，虽然它目前什么都做不到。

### 格式版本<a name="format-version"></a>
`Format`字段是内容包用的 Content Patcher 版本号。用来保持内容包的版本兼容。

你应该使用最新的格式版本（现在是`2.5.0`）来启用最新功能，避免某些被弃用的代码，减少加载模组的时间。

### 更改<a name="changes"></a>
`Changes`字段描述了你想更改的内容，每个条目都被称为**补丁**，并说明要执行的具体操作：编辑图片、更改对话等。
你可以列出任意数量的补丁，也可以对同一文件应用多个补丁（它们将按照所列顺序一个个使用）。

## 特征<a name="features"></a>
**注意：** 这些内容按内容包的使用频率排序。你不需要了解或使用所有特征。

### 操作<a name="actions"></a>
每个补丁都有一个`Action`操作字段，即你要进行更改的类型。
关于每个操作的更多信息，请参阅每个操作的文档页面（链接在下方）。

<table>
<tr>
<th>action</th>
<th>overview</th>
</tr>
<tr>
<td><code>Load</code></td>
<td>

`Load` replaces an entire asset with a version you provide.

For example, if you have an `assets/abigail.png` image with custom portraits for Abigail, this
would change her portraits in-game:

```js
{
   "Format": "2.5.0",
   "Changes": [
      {
         "Action": "Load",
         "Target": "Portraits/Abigail",
         "FromFile": "assets/abigail.png"
      }
   ]
}
```

This isn't recommended if you can use one of the `Edit*` actions instead.

See the [`Action: Load` documentation](author-guide/action-load.md) for more info.

</td>
</tr>
<tr>
<td><code>EditData</code></td>
<td>

`EditData` changes the data read from a data asset. This supports simple lookup assets like
`Data/Achievements`, or full data model assets like `Data/Objects`. Any number of content packs
can edit the same asset.

This lets you...
* add, edit, or delete entries;
* reorder entries in a list;
* or edit individual fields within an entry.

For example, this doubles the price of moss soup (see [object fields](https://stardewvalleywiki.com/Modding:Object_data)):

```js
{
    "Format": "2.5.0",
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

You can do much more using `EditData`, including add completely custom items, fruit trees, etc.
See the [`Action: EditData` documentation](author-guide/action-editdata.md) for more info.

</td>
</tr>
<tr>
<td><code>EditImage</code></td>
<td>

`EditImage` edits one of the game's image assets. Any number of content packs can edit the same
image.

This lets you...
* edit or replace any portion of the image;
* overlay a new image onto the existing one with transparency support;
* or extend the image size (e.g. to add more sprites to a spritesheet).

For example, if your content pack has an `assets/tuna.png` image with a custom
[tuna](https://stardewvalleywiki.com/Tuna) sprite, this would replace tuna sprites in-game:

```js
{
   "Format": "2.5.0",
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

See the [`Action: EditImage` documentation](author-guide/action-editimage.md) for more info.

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
    "Format": "2.5.0",
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

See the [`Action: EditMap` documentation](author-guide/action-editmap.md) for more info.

</td>
</tr>
<tr>
<td><code>Include</code></td>
<td>

`Include` adds patches from another file. This is just a way to organize your content pack into
multiple files, instead of having everything in one `content.json`. The included patches work
exactly as if they were directly in `content.json`.

例如，你可以将其与[Tokens和条件](#tokens)结合起来加载动态文件：
```js
{
    "Format": "2.5.0",
    "Changes": [
        {
            "Action": "Include",
            "FromFile": "assets/john_{{season}}.json"
        }
    ]
}
```

See the [`Action: Include` documentation](author-guide/action-include.md) for more info.

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
    "Format": "2.5.0",
    "Changes": [
        {
            "Action": "Load",
            "Target": "Portraits/Abigail",
            "FromFile": "assets/abigail-{{season}}.png"
        }
    ]
}
```

Or this gives her different seasonal portraits if you're married to her:

```js
{
    "Format": "2.5.0",
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
你可以让玩家使用`config.json`文件配置你的模组。如果玩家有[通用模组配置菜单GMCM](https://www.nexusmods.com/stardewvalley/mods/5098)，玩家还能
通过游戏内的选项菜单配置不同的更改。

例如，你可以将配置值用作[Tokens和条件](#tokens):

```js
{
    "Format": "2.5.0",
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
语言，Content Patcher将自动处理，显示默认（英文）文本。

例如，如果你的`i18n`文件包含一个关键字为`rainy-day`的翻译，你可以在任何支持[Tokens和条件](#tokens)的Content Patcher 字段里访问它：

```js
{
    "Format": "2.5.0",
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

See the [translation documentation](author-guide/translations.md) for more info.

### 文本操作<a name="text-operations"></a>
文本操作可让你根据文本字段的已有值进行修改，而不只是设置一个新的值。例如，你可以在当前值上添加其他文本。

For example, this adds pufferfish as a universally loved gift:

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

See the [text operations documentation](author-guide/text-operations.md) for more info.

### 触发动作<a name="trigger-actions"></a>
Content Patcher 为特定内容添加自定义[触发动作](https://zh.stardewvalleywiki.com/%E6%A8%A1%E7%BB%84:%E8%A7%A6%E5%8F%91%E5%8A%A8%E4%BD%9C)，例如更新存档的命名ID。

See [Content Patcher's trigger action documentation](author-guide/trigger-actions.md) for more info.

## 故障排除<a name="troubleshoot"></a>
详见[故障排除指南](author-guide/troubleshooting.md)。

## 常见问题<a name="faqs"></a>

### 多久应用一次补丁更改？<a name="update-rate"></a>
每次加载数据资产时，你的补丁都会**应用数据资产**，但它们会通过不同的条件**更新字段**。例如，假设你有这样一个补丁：
```js
{
    "Action": "EditMap",
    "Target": "Maps/Town",
    "SetProperties": {
        "CurrentTime": "{{Time}}"
    }
}
```

When the day starts, Content Patcher updates the patch so it contains `"CurrentTime": "600"`. It
doesn't matter if you reload the map it's applied to later in the day, the patch still contains
`"CurrentTime": "600"` until its fields are updated.

You can add the `Update` field to update more often if needed. The possible values are:

更新频率            | 作用
------------------ | ------
`OnDayStart`       | _(default)_ Update when the in-game day starts. This is always enabled even if you omit it.
`OnLocationChange` | update when the player warps to a new location.
`OnTimeChange`     | Update when the in-game clock changes.
_multiple_         | You can specify multiple values separated by commas, like `"Update": "OnLocationChange, OnTimeChange"`.

For example, this will update and reapply the patch when the in-game time changes:
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


### 如何更改另一种语言的资产？<a name="how-do-i-change-assets-in-another-language"></a>
**默认影响所有语言**

The asset name in the `Target` field doesn't include the language. For example,
`"Target": "Dialogue/Abigail"` (the asset name) will change the content loaded from
`Content/Dialogue/Abigail.de-DE.xnb` (the file path) when playing in German. If you want
to make the same change in every language, you don't need to do anything else.

To target a specific language, you can add a language condition:
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

You can also load the translated version automatically if it exists. That way you can just add
translated files to your content pack, and it'll default to the untranslated version if no
translation exists:

```js
// use translated version if it exists in the content pack
{
   "Action": "EditImage",
   "Target": "LooseSprites/Cursors",
   "FromFile": "assets/cursors.{{language}}.png",
   "When": {
      "HasFile:{{FromFile}}": true
   }
},

// otherwise use untranslated version
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
补丁会把修改过的数据资产合并并输入。

在一个内容包内，补丁会按照在`content.json`中列出的顺序应用。 
如果你有多个内容包，每个内容包都会按照 SMAPI 加载的顺序应用；如果你需要依赖另一个内容包，请参阅[manifest中的依赖](https://zh.stardewvalleywiki.com/%E6%A8%A1%E7%BB%84:%E5%88%B6%E4%BD%9C%E6%8C%87%E5%8D%97/APIs/Integrations).

### 已知限制<a name="known-limitations"></a>
某些游戏资产具有特殊逻辑。这不是Content Patcher特有的限制，但为了保险起见故在此列出这些限制。

asset | notes
----- | -----
`Characters/Dialogue/*` | 对话是在一天开始时设置的，因此设置[自定义更新速度](#update-rate)不会影响一天开始后的对话。（不过你可以用[特定位置对话键](https://zh.stardewvalleywiki.com/%E6%A8%A1%E7%BB%84:%E5%AF%B9%E8%AF%9D#%E5%9C%B0%E7%82%B9%E5%AF%B9%E8%AF%9D)来规避这个问题）。
`Characters/Farmer/accessories` | 附件的数量是硬编码，因此自定义附件需要替换现有附件。
`Characters/Farmer/skinColors` | 皮肤颜色的数量是硬编码，因此自定义颜色需要替换现有颜色。
`Data/SpecialOrders` | 游戏会在 _保存之前_ 缓存该资产的副本，并在首次打开会话的特殊订单板时加载单独的副本。有条件地添加/删除特殊命令时要非常小心，因为当玩家试图从新列表中接受缓存列表中不存在的特殊订单命令时，可能会导致游戏崩溃。
`Maps/*` | 参见维基上的[地图编辑中的潜在问题](https://zh.stardewvalleywiki.com/%E6%A8%A1%E7%BB%84:%E5%9C%B0%E5%9B%BE#%E6%BD%9C%E5%9C%A8%E9%97%AE%E9%A2%98)。

## 参见<a name="see-also"></a>
* [README](README.md)
* [帮助](https://zh.stardewvalleywiki.com/%E6%A8%A1%E7%BB%84:%E5%B8%AE%E5%8A%A9)
