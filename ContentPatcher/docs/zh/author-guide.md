← [README](README.md)

此文档帮助模组作者创建基于 Content Patcher 的内容包。

**其他信息请参见[主 README](README.md)**。

**🌐 其他语言：[en (English)](../author-guide.md)。**

## 目录
* [简介](#introduction)
  * [什么是 Content Patcher？](#what-is-content-patcher)
  * [内容包长什么样？](#what-does-a-content-pack-look-like)
  * [什么是素材？](#what-is-an-asset)
* [开始](#get-started)
  * [创建内容包](#create-the-content-pack)
  * [格式版本](#format-version)
  * [更改](#changes)
* [功能](#features)
  * [操作](#actions)
  * [自定义地点](#custom-locations)
  * [Tokens 和条件](#tokens)
  * [玩家配置](#player-config)
  * [翻译](#translations)
  * [文本操作](#text-operations)
  * [触发动作](#trigger-actions)
* [故障排除](#troubleshoot)
* [常见问题](#faqs)
  * [补丁更新频率](#update-rate)
  * [Content Patcher 更新是否与旧版本兼容？](#are-content-patcher-updates-backwards-compatible)
  * [如何更改其他语言的素材？](#how-do-i-change-assets-in-another-language)
  * [多个补丁如何交互？](#how-do-multiple-patches-interact)
  * [已知限制](#known-limitations)
* [参见](#see-also)

## 简介<a name="introduction"></a>
### 什么是 Content Patcher？<a name="what-is-content-patcher"></a>

Content Patcher 可让您只使用 JSON 文件来更改游戏内容。JSON 只是一种文本格式，因此不需要编程经验。

您可以对游戏进行各种修改，包括：

* 更改图像、对话、地图等；
* 添加自定义物品、果树、地点等；
* 更改商店库存；
* 以及更多其他修改。

您还可以对游戏进行动态调整。例如，可以在冬季周末晚上下雪时提高咖啡价格，除非玩家与阿比盖尔结婚。

[Wiki 上的模组制作文档](https://zh.stardewvalleywiki.com/模组:目录)通常是为 Content Patcher 内容包作者编写的，所以您可以在那里找到很多示例。

### 内容包长什么样？<a name="what-does-a-content-pack-look-like"></a>

内容包只是一个包含两个文本文件的文件夹：`manifest.json`（其中包含模组名称等信息）和 `content.json`（告诉 Content Patcher 您想更改什么）。您的文件夹中可能还包含图像或其他文件，这些文件通常被放在 `assets` 子文件夹中：
```plaintext
📁 Mods/
   📁 [CP] YourModName/
      🗎 content.json
      🗎 manifest.json
      📁 assets/
         🗎 example.png
```

例如，以下是一个 `content.json`，它使用您自己的图像替换阿比盖尔的肖像：
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

使用 Content Patcher 的功能（如操作、标记、条件等），您可以实现更多动态变化。本指南将在下面详细介绍。

### 什么是素材？<a name="what-is-an-asset"></a>

**素材**（Assets）是游戏从其 `Content` 文件夹（或从其它模组文件夹）中加载的图像、数据模型或地图。这些是 Content Patcher 允许您更改的内容。

素材名称通常不包括 "Content"、[语言代码](#how-do-i-change-assets-in-another-language) 或文件扩展名。例如，`Content/Maps/spring_beach.xnb` 和 `Content/Maps/spring_beach.fr-FR.xnb` 都是相同的 `Maps/spring_beach` 素材。

您可以[解压游戏的内容文件](https://zh.stardewvalleywiki.com/模组:编辑_XNB_文件#解包游戏文件)以查看它们包含的内容。以下是 `Portraits/Abigail` 包含的内容：

![](../screenshots/sample-asset.png)

因此，如果您想更改阿比盖尔的肖像，可以使用 Content Patcher 加载或编辑 `Portraits/Abigail` 并更改该图像，如前面的示例代码所示。

## 开始<a name="get-started"></a>
### 创建内容包<a name="create-the-content-pack"></a>
1. 安装 [SMAPI](https://www.nexusmods.com/stardewvalley/mods/2400) 和 [Content Patcher](https://www.nexusmods.com/stardewvalley/mods/1915)；
2. 在 `Mods` 文件夹中创建一个空文件夹，并将其命名为 `[CP] YourModName`。（请将 `YourModName` 替换为您的模组名称）；
3. 创建一个新的 `manifest.json` 文件，内容如下：
   ```js
   {
       "Name": "Your Mod Name",
       "Author": "Your Name",
       "Version": "1.0.0",
       "Description": "One or two sentences about the mod.",
       "UniqueID": "YourName.YourModName",
       "UpdateKeys": [], // 将您发布模组时的更新键填写在这里。请参阅 https://zh.stardewvalleywiki.com/模组:制作指南/APIs/Update_checks
       "ContentPackFor": {
           "UniqueID": "Pathoschild.ContentPatcher"
       }
   }
   ```
4. 将 `Name`、`Author`、`Description` 和 `UniqueID` 值更改为您的模组对应的值。（请不要更改 `ContentPackFor` 下的 `UniqueID`！）
5. 创建一个 `content.json` 文件，内容如下：
   ```js
   {
       "Format": "2.7.0",
       "Changes": [
           // 这里面是您要更改的内容
       ]
   }
   ```

好了，您现在已经创建了一个可以正常使用的 Content Patcher 内容包，虽然它目前没有任何作用。

### 格式版本<a name="format-version"></a>
`Format` 字段是您设计内容包所使用的 Content Patcher 版本。此字段用于保持内容包与未来版本的兼容性。

您应该始终使用最新格式版本（当前为 `2.7.0`）以启用最新功能、避免使用被弃用的代码、减少加载模组的时间。

### 更改<a name="changes"></a>
`Changes` 字段描述了您想在游戏中更改的内容。列表中的每个条目都被称为**补丁**，每个补丁都描述了一个特定的操作：编辑图像、更改对话等。您可以列出任意数量的补丁，也可以对同一文件应用多个补丁（它们将按所列的顺序依次应用）。

## 功能<a name="features"></a>
**注意**：这些功能按照内容包使用的频率排序。您不需要知道或使用所有这些功能。

### 操作<a name="actions"></a>
每个补丁都有一个 `Action` 字段，表示您想进行的更改类型。有关每种操作的更多信息，请参阅相应的文档页面（链接如下）。

<table>
<tr>
<th>操作</th>
<th>概述</th>
</tr>
<tr>
<td><code>Load</code></td>
<td>

`Load` 使用您提供的版本替换整个素材。

例如，如果您有一个 `assets/abigail.png` 图像，包含自定义的阿比盖尔肖像，这会更改她的游戏内肖像：
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

出于兼容性考虑，能使用 `Edit*` 操作时不推荐使用 `Load`。

有关更多信息，请参阅 [`Action: Load` 文档](author-guide/action-load.md)。

</td>
</tr>
<tr>
<td><code>EditData</code></td>
<td>

`EditData` 更改从数据素材读取的数据。这支持简单字符串到字符串形成的素材（如 `Data/Achievements`）或完整的数据模型素材（如 `Data/Objects`）。多个内容包可以编辑同一素材。

这让您可以...
* 添加、编辑或删除条目；
* 重新排序列表中的条目；
* 或编辑条目中的单个字段。

例如将苔藓汤的价格改为 80（请参阅[物体字段](https://zh.stardewvalleywiki.com/模组:物体)。
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

`EditData` 可实现的效果还有很多，包括添加完全自定义的物品、果树等。有关更多信息，请参阅 [`Action: EditData` 文档](author-guide/action-editdata.md)。

</td>
</tr>
<tr>
<td><code>EditImage</code></td>
<td>

`EditImage` 更改游戏中的图像素材。多个内容包可以编辑同一个素材。

这让您可以...
* 编辑或替换图像的任何部分；
* 使用透明度支持覆盖新图像到现有图像上；
* 或扩展图像大小（例如，向精灵图添加更多精灵）。

例如，如果您的内容包有一个 `assets/tuna.png` 图像，包含自定义的 [金枪鱼](https://zh.stardewvalleywiki.com/金枪鱼) 精灵，这会替换游戏内的金枪鱼精灵图：
```js
{
   "Format": "2.7.0",
   "Changes": [
      {
         "Action": "EditImage",
         "Target": "Maps/springobjects",
         "FromFile": "assets/fish-object.png",
         "ToArea": { "X": 160, "Y": 80, "Width": 16, "Height": 16 } // 替换金枪鱼精灵图
      }
   ]
}
```

有关更多信息，请参阅 [`Action: EditImage` 文档](author-guide/action-editimage.md)。

</td>
</tr>
<tr>
<td><code>EditMap</code></td>
<td>

`EditMap` 可更改游戏中地图的某一部分。多个内容包可以编辑同一张地图。

这让您可以...
* 更改地图属性和图块集属性；
* 使用本地地图覆盖游戏地图的一部分（具有各种合并选项）；
* 添加自定义图块集；
* 调整地图大小（例如向现有游戏地点添加更多内容）。

例如，将鹈鹕镇广场替换为自定义的样式：
```js
{
    "Format": "2.7.0",
    "Changes": [
        {
            "Action": "EditMap",
            "Target": "Maps/Town",
            "FromFile": "assets/town.tmx",
            "ToArea": { "X": 22, "Y": 61, "Width": 16, "Height": 13 } // 替换鹈鹕镇广场
        }
    ]
}
```

有关更多信息，请参阅 [`Action: EditMap` 文档](author-guide/action-editmap.md)。

</td>
</tr>
<tr>
<td><code>Include</code></td>
<td>

`Include` 从另一个 JSON 文件中加载补丁。这只是将内容包组织成多个文件的一种方式，而不是将所有内容都放在一个 `content.json` 中。包含的补丁和直接放在 `content.json` 中的补丁功能一致

例如，您可以将其与 [Tokens 和条件](#tokens)结合以动态加载文件：
```js
{
    "Format": "2.7.0",
    "Changes": [
        {
            "Action": "Include",
            "FromFile": "assets/john_{{season}}.json" // 根据季节加载不同的文件
        }
    ]
}
```

有关更多信息，请参阅 [`Action: Include` 文档](author-guide/action-include.md)。

</td>
</tr>
</table>

### 自定义地点<a name="custom-locations"></a>
`CustomLocations` 可以让您添加新的游戏地点，包括自己的地图和传送点。Content Patcher 自动处理 NPC 寻路、对象持续时间等问题。

有关更多信息，请参阅 [自定义位置文档](author-guide/custom-locations.md)。

### Tokens 和条件<a name="tokens"></a><a name="conditions"></a>
前面的章节介绍了如何进行静态更改，但您也可以使用 Tokens 和条件来进行**动态**更改。

例如，您可以——
* 根据季节、回答的对话问题、游戏进度等多种因素更改补丁；
* 使用随机、算术和动态查询；
* 以及更多。

例如，这让阿比盖尔在每个季节都有不同的肖像：
```js
{
    "Format": "2.7.0",
    "Changes": [
        {
            "Action": "Load",
            "Target": "Portraits/Abigail",
            "FromFile": "assets/abigail-{{season}}.png" // 根据季节加载不同肖像
        }
    ]
}
```

或者当玩家与她结婚后，使用不同的肖像：
```js
{
    "Format": "2.7.0",
    "Changes": [
        {
            "Action": "Load",
            "Target": "Portraits/Abigail",
            "FromFile": "assets/abigail-married.png",
            "When": {
                "Spouse": "Abigail" // 如果玩家她结婚，加载结婚后的肖像
            }
        }
    ]
}
```

有关更多信息，请参阅 [Tokens 和条件指南](author-guide/tokens.md)。

### 玩家配置<a name="player-config"></a>
您可以让玩家通过 `config.json` 文件配置您的模组。如果玩家安装了 [Generic Mod Config Menu 模组](https://www.nexusmods.com/stardewvalley/mods/5098)，那么他们也可以通过游戏内选项菜单配置模组。

例如，您可以使用配置值作为 [Tokens 和条件](#tokens)：
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
                "EnableJohn": true // 当 EnableJohn 为真时加载 john.json
            }
        }
    ]
}
```

有关更多信息，请参阅 [配置文档指南](author-guide/config.md)。

### 翻译<a name="translations"></a>
您可以为模组添加翻译文件并通过 `i18n` 标记访问它们。Content Patcher 会自动处理在当前语言中未翻译内容，并使用 `default.json` 中的内容显示。

例如，如果您的 `i18n` 文件包含键为 `rainy-day` 的翻译，您可以在任何支持[Tokens 和条件](#tokens) 的任何 Content Patcher 字段中访问它：
```js
{
    "Format": "2.7.0",
    "Changes": [
        {
            "Action": "EditData",
            "Target": "Characters/Dialogue/MarriageDialogueAbigail",
            "Entries": {
                "Rainy_Day_4": "{{i18n: rainy-day}}" // 使用翻译后的文本
            }
        }
    ]
}
```

有关更多信息，请参阅 [翻译文档](author-guide/translations.md)。

### 文本操作<a name="text-operations"></a>
文本操作让您根据当前值更改文本字段，而不只是设置一个新的值。例如，您可以在文本当前值上添加其他文本。

例如，这将河豚添加到村民普遍喜爱的礼物中：
```js
{
    "Action": "EditData",
    "Target": "Data/NPCGiftTastes",
    "TextOperations": [
        {
            "Operation": "Append",
            "Target": ["Entries", "Universal_Love"],
            "Value": "127", // 在 Universal_Love 列表中添加河豚
            "Delimiter": " "
        }
    ]
}
```

有关更多信息，请参阅 [文本操作文档](author-guide/text-operations.md)。

### 触发动作<a name="trigger-actions"></a>
Content Patcher 为特殊情况添加了自定义 [触发操作](https://zh.stardewvalleywiki.com/模组:触发动作)，例如将现有存档内容中旧的内置 ID 重命名为新的 ID。

有关更多信息，请参阅 [Content Patcher 的触发操作文档](author-guide/trigger-actions.md)。

## 故障排除<a name="troubleshoot"></a>
有关更多信息，请参阅 [故障排除指南](author-guide/troubleshooting.md)。

## 常见问题<a name="faqs"></a>
### 补丁更新频率<span id="update-rate"></span>
每次加载数据素材时，您的补丁都会 **应用数据素材** ，但它们会通过不同的条件**更新字段**。例如，假设您有这样一个补丁：
```js
{
    "Action": "EditMap",
    "Target": "Maps/Town",
    "SetProperties": {
        "CurrentTime": "{{Time}}" // 设置当前时间
    }
}
```

当游戏中的一天开始时，Content Patcher会将此补丁更新为`"CurrentTime": "600"`。即使您在一天中的晚些时候重新加载应用了补丁的地图，补丁仍然包含 `"CurrentTime": "600"`，直到其字段被更新。

您可以使用 `Update` 字段以更频繁地更新，如果需要的话。可能的值是：

更新频率            | 效果 
------------------ | ------ 
`OnDayStart`       | **（默认）** 在游戏中的一天开始时更新。没有该字段的补丁将默认使用此更新频率。 
`OnLocationChange` | 在玩家传送到另一地图时更新。
`OnTimeChange`     | 在游戏时间变化时更新。
多种更新频率         | 您可以指定多个由逗号分隔的值，例如 `"Update": "OnLocationChange, OnTimeChange"`。 

例如，这将在游戏时间变化时更新并重新应用补丁：
```js
{
    "Action": "EditMap",
    "Target": "Maps/Town",
    "SetProperties": {
        "CurrentTime": "{{Time}}" // 设置当前时间
    },
    "Update": "OnTimeChange"
}
```

### Content Patcher 更新是否与旧版本兼容？<a name="are-content-patcher-updates-backwards-compatible"></a>
是的。有关更多信息，请参阅 [迁移指南](author-migration-guide.md)。

### 如何更改其他语言的素材？<a name="how-do-i-change-assets-in-another-language"></a>
**您的补丁默认影响所有语言。**

素材名称在 `Target` 字段中不包含语言。例如 `"Target": "Dialogue/Abigail"`（素材名称）将更改从 `Content/Dialogue/Abigail.de-DE.xnb`（文件路径）加载的内容，当玩家使用德语时。如果您想在同一素材上对所有语言进行相同的更改，您不需要做任何额外的操作。

要针对特定语言进行更改，您可以添加一个语言条件：
```js
{
   "Action": "EditImage",
   "Target": "LooseSprites/Cursors",
   "FromFile": "assets/cursors.de.png",
   "When": {
      "Language": "de" // 仅在德语时应用此补丁
   }
}
```

您还可以自动加载已翻译的版本（如果存在）。这样您只需将翻译文件添加到内容包中，它会默认使用翻译版，如果没有翻译文件，则使用原版：

```js
// 如果内容包中有翻译版本，则使用翻译版本
{
   "Action": "EditImage",
   "Target": "LooseSprites/Cursors",
   "FromFile": "assets/cursors.{{language}}.png", // 使用 {{language}} 标记自动选择语言版本
   "When": {
    "HasFile:{{FromFile}}": true // 确认文件存在
   }
},

// 否则使用未翻译版本
{
   "Action": "EditImage",
   "Target": "LooseSprites/Cursors",
   "FromFile": "assets/cursors.png",
   "When": {
      "HasFile: assets/cursors.{{language}}.png": false // 确认没有翻译版本
   }
},
```

### 多个补丁如何交互？<a name="how-do-multiple-patches-interact"></a>
多个补丁可以应用于同一文件。`Action: Load` 总是先于其他操作之前执行，但除此之外，每个补丁都是按顺序依次应用的。每个补丁完成后，下一个补丁将会把修改过的数据素材作为输入。

在一个内容包内，补丁会按照它们在 `content.json` 中列出的顺序应用。当您有多个内容包时，每个内容包都会按照 SMAPI 加载它们的顺序应用；如果您的内容包依赖另一个内容包，则需要显式地在另一个内容包之后进行补丁，请参阅 [manifest 中的依赖](https://zh.stardewvalleywiki.com/模组:制作指南/APIs/Integrations)。

### 已知限制<a name="known-limitations"></a>
某些游戏素材具有特殊逻辑。这并非 Content Patcher 特有的问题，但为了方便起见在此处进行了记录。

 素材  | 注意事项 
 ---- | ------- 
 `Characters/Dialogue/*` | 对话是在每天开始时设置的，因此设置[自定义更新频率](#update-rate)不会影响当天开始后对话的变化。（您可以使用[位置特定的对话键](https://zh.stardewvalleywiki.com/模组:对话#地点对话)来绕过这一限制。） 
 `Characters/Farmer/accessories` | 配饰的数量是硬编码的，因此自定义配饰需要替换现有的配饰。
 `Characters/Farmer/skinColors` | 皮肤颜色的值是硬编码的，因此自定义颜色需要替换现有的颜色。
 `Data/SpecialOrders` | 游戏会在**保存游戏前**缓存此素材的一个副本，并在第一次打开会话的特殊订单板时加载一个单独的副本。在有条件地添加或移除特殊订单时要非常小心，因为这可能会在玩家尝试在新列表中接受缓存列表中不存在的特殊订单时导致游戏崩溃。 
 `Maps/*` | 参见 Wiki [地图编辑中的潜在问题](https://zh.stardewvalleywiki.com/模组:地图#潜在问题)。 

## 参见<a name="see-also"></a>
* [README](README.md) 中的其他信息
* [帮助](https://zh.stardewvalleywiki.com/模组:帮助)
