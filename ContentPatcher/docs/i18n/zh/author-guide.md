← [README](README.md)

<<<<<<< HEAD
本文档可帮助模组制作者为 Content Patcher 创建内容包。

**其他信息请参见[README](README.md)**。

## 内容
* [介绍](#介绍)
  * [什么是Content Patcher？](#what-cp-is)
  * [内容包长什么样？](#what-pack-is)
  * [什么是数据资产？](#what-asset-is)
* [开始](#开始)
  * [创建内容包](#创建内容包)
  * [格式版本](#格式版本)
  * [更改](#更改)
* [特征](#特征)
  * [操作](#操作)
  * [自定义地点](#自定义地点)
  * [Tokens和条件](#tokens)
  * [玩家设置](#玩家设置)
  * [翻译](#翻译)
  * [文本操作](#文本操作)
  * [触发动作](#触发动作)
* [故障排除](#故障排除)
* [常见问题](#常见问题)
  * [多久应用一次补丁更改？](#when-to-change)
  * [Content Patcher更新是否与旧版本兼容？](#compatibility)
  * [如何更改另一种语言的资产？](#change-language)
  * [多个补丁如何交互？](#how-interact)
  * [已知限制](#已知限制)
* [另见](#另见)

## 介绍<a name="what-cp-is"></a>
### 什么是Content Patcher？
=======
此文档描述如何创建一个Content Patcher的内容包。

**其他信息请参见[主README](README.md)**.

## 内容
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
>>>>>>> c036414e8861adc25a9c6a2a7c1fac76501d9f05
Content Patcher 可让你只使用 JSON 文件更改游戏内容。JSON 是一种文本格式，
因此不需要学会编程也能使用。

你可以对游戏进行各种修改：

* 更改图像、对话、地图等；
* 添加自定义项目、果树、地点等；
* 改变商店库存；
* 还有很多其他的修改。

<<<<<<< HEAD
您还可以对游戏进行动态调整。例如，除非玩家娶了阿比盖尔，否则冬天的周末晚上都会下雪。

[维基上的修改文档](https://zh.stardewvalleywiki.com/%E6%A8%A1%E7%BB%84:%E7%9B%AE%E5%BD%95)是 Content Patcher 的作者编写的，所以你可以在那里找到很多例子。

<a name="what-pack-is"></a>

### 内容包长什么样？
=======
你还可以对游戏进行动态调整。例如，除非玩家娶了阿比盖尔，否则冬天的周末晚上都会下雪。

[维基上的模组制作文档](https://zh.stardewvalleywiki.com/%E6%A8%A1%E7%BB%84:%E7%9B%AE%E5%BD%95)通常是为Content Patcher内容包作者编写的，所以你可以在那里找到很多例子。

### 内容包长什么样？<a name="what-is-content-patcher"></a>
>>>>>>> c036414e8861adc25a9c6a2a7c1fac76501d9f05
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

<<<<<<< HEAD
例如，这里的`content.json` 会将阿比盖尔的肖像替换为你自己选择的图像：

```js
{
    "Format": "2.5.0",
=======
例如，这个`content.json`替代阿比盖尔的肖像：

```js
{
    "Format": "2.6.0",
>>>>>>> c036414e8861adc25a9c6a2a7c1fac76501d9f05
    "Changes": [
        {
            "Action": "Load",
            "Target": "Portraits/Abigail",
            "FromFile": "assets/abigail.png"
        }
    ]
}
```

<<<<<<< HEAD
通过使用动作、令牌、条件等功能，您可以使用 Content Patcher 做 _更多_ 的事。本文档将在接下来一一介绍：
<a name="what-asset-is"></a>

### 什么是数据资产？
 _资产(assets)_ 是指游戏从`Content`内容文件夹（或者从其他模组文件夹）中加载的图像、数据模型或地图。这些都是 Content Patcher 可以更改的内容。

资产名称不包括 "内容" ，即[语言代码](#如何用另一种语言更改资产？)，或文件扩展名。例如，`Content/Maps/spring_beach.xnb`和`Content/Maps/spring_beach.fr-FR.xnb`是相同的`Maps/spring_beach`资产。

你可以[解包游戏文件](https://zh.stardewvalleywiki.com/%E6%A8%A1%E7%BB%84:%E7%BC%96%E8%BE%91_XNB_%E6%96%87%E4%BB%B6#%E8%A7%A3%E5%8C%85%E6%B8%B8%E6%88%8F%E6%96%87%E4%BB%B6)查看它们包含的内容。下面是`Portraits/Abigail`的内容：

![](screenshots/sample-asset.png)

因此，如果您想更改阿比盖尔的肖像，可以使用Content Patcher加载或编辑`Portraits/Abigail`然后像前面的示例代码一样更改该图片。

## 开始
### 创建内容包
1. 安装[SMAPI](https://www.nexusmods.com/stardewvalley/mods/2400)和[Content
   Patcher](https://www.nexusmods.com/stardewvalley/mods/1915).
2. 在`Mods`文件夹中创建一个空文件夹，并命名为`[CP]模组名`，自己取一个名字来替换
   `模组名`。
3. 用这些内容创建一个`manifest.json`文件：
   ```js
   {
       "Name": "模组名",
       "Author": "你的名字",
       "Version": "1.0.0",
       "Description": "模组的简单描述。",
       "UniqueID": "YourName.YourModName",
       "UpdateKeys": [], //当你发布模组时，查看 https://zh.stardewvalleywiki.com/%E6%A8%A1%E7%BB%84:%E5%88%B6%E4%BD%9C%E6%8C%87%E5%8D%97/APIs/Update_checks 来获取更新键
=======
通过使用动作、令牌、条件等功能，你可以使用 Content Patcher 做 _更多_ 的事。本文档将在接下来一一介绍：

### 什么是数据素材？<a name="what-is-an-asset"></a>
 _素材(assets)_ 是指游戏从`Content`内容文件夹（或者从其他模组文件夹）中加载的图像、数据模型或地图。这些都是 Content Patcher 可以更改的内容。

素材名称不包括 "内容" ，即[语言代码](#how-do-i-change-assets-in-another-language)，或文件扩展名。例如，`Content/Maps/spring_beach.xnb`和`Content/Maps/spring_beach.fr-FR.xnb`是相同的`Maps/spring_beach`素材。

You can [unpack the game's content files](https://stardewvalleywiki.com/Modding:Editing_XNB_files#unpacking)
你可以[解包游戏的内容文件](https://zh.stardewvalleywiki.com/%E6%A8%A1%E7%BB%84:%E7%BC%96%E8%BE%91_XNB_%E6%96%87%E4%BB%B6#.E8.A7.A3.E5.8C.85.E6.B8.B8.E6.88.8F.E6.96.87.E4.BB.B6)并查看内容。以下是`Portraits/Abigail`包含的内容：

![](screenshots/sample-asset.png)

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
       "UpdateKeys": [], //当你发布模组时，更新键填写在这里详见 https://zh.stardewvalleywiki.com/%E6%A8%A1%E7%BB%84:%E5%88%B6%E4%BD%9C%E6%8C%87%E5%8D%97/APIs/Update_checks 来获取
>>>>>>> c036414e8861adc25a9c6a2a7c1fac76501d9f05
       "ContentPackFor": {
           "UniqueID": "Pathoschild.ContentPatcher"
       }
   }
   ```
<<<<<<< HEAD
4. 修改`Name`, `Author`, `Description`, `UniqueID`后面的字段，其中UniqueID必须用英文，可以包括小数点，下划线符号。（别改`ContentPackFor`下面的`UniqueID`！）
5. 用下面的代码创建`content.json`文件：
   ```js
   {
       "Format": "2.5.0",
       "Changes": [
           // 这里面是你要更改的代码
=======
4. 把`Name`, `Author`, `Description`, 和`UniqueID` 改成对应你的模组的值. (不要改`ContentPackFor`下的`UniqueID`！)
5. 创建一个新`content.json`文件，包含以下内容：
   ```js
   {
       "Format": "2.6.0",
       "Changes": [
           // 这里面是你要更改的内容
>>>>>>> c036414e8861adc25a9c6a2a7c1fac76501d9f05
       ]
   }
   ```

<<<<<<< HEAD
好了，你现在已经创建了一个可以用的 Content Patcher 包，虽然它目前什么都做不到。

### 格式版本
=======
好了，你现在已经创建了一个可以用的Content Patcher内容包，虽然它目前什么都做不到。

### 格式版本<a name="format-version"></a>
>>>>>>> c036414e8861adc25a9c6a2a7c1fac76501d9f05
`Format`字段是内容包用的 Content Patcher 版本号。用来保持内容包的版本兼容。

你应该使用最新的格式版本（现在是`2.5.0`）来启用最新功能，避免某些被弃用的代码，减少加载模组的时间。

<<<<<<< HEAD
### 更改
`Changes`字段描述了你想更改的内容，每个条目都被称为**补丁**，并说明要执行的具体操作：编辑图片、更改对话等。
你可以列出任意数量的补丁，也可以对同一文件应用多个补丁（它们将按照所列顺序一个个使用）。

## 特征
**注意：** 这些内容按内容包的使用频率排序。你不需要了解或使用所有特征。

### 操作
=======
### 更改<a name="changes"></a>
`Changes`字段描述了你想更改的内容，每个条目都被称为**补丁**，并说明要执行的具体操作：编辑图片、更改对话等。
你可以列出任意数量的补丁，也可以对同一文件应用多个补丁（它们将按照所列顺序一个个使用）。

## 特征<a name="features"></a>
**注意：** 这些内容按内容包的使用频率排序。你不需要了解或使用所有特征。

### 操作<a name="actions"></a>
>>>>>>> c036414e8861adc25a9c6a2a7c1fac76501d9f05
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

<<<<<<< HEAD
`Load`将整个资产替换为你写的版本。

例如，如果你有一张 `assets/abigail.png` 图像，其中包含阿比盖尔的自定义肖像，那么下面的命令会改变她在游戏中的肖像：

```js
{
   "Format": "2.5.0",
=======
`Load`操作替换一整个素材。

此例子将阿比盖尔原有的肖像替换成你提供的你提供的`assets/abigail.png`图像：
```js
{
   "Format": "2.6.0",
>>>>>>> c036414e8861adc25a9c6a2a7c1fac76501d9f05
   "Changes": [
      {
         "Action": "Load",
         "Target": "Portraits/Abigail",
         "FromFile": "assets/abigail.png"
      }
   ]
}
```

<<<<<<< HEAD
更建议使用`Edit*`操作。

详见[`Action: Load`指南](author-guide/action-load.md)。
=======
出于兼容性考虑，能使用`Edit*`操作时不推荐使用`Load`。

详见[`Action: Load`文档](author-guide/action-load.md)。
>>>>>>> c036414e8861adc25a9c6a2a7c1fac76501d9f05

</td>
</tr>
<tr>
<td><code>EditData</code></td>
<td>

<<<<<<< HEAD
`EditData`可更改从数据资产读取的数据。这支持简单的查找资产，如`Data/Achievements`，或完整的数据模型资产，如`Data/Objects`。多个内容包可以编辑同一资产。

这让你可以——
* 添加、编辑或删除条目；
* 对列表中的条目重新排序；
* 编辑条目中的个别字段。

例如，苔藓汤的价格会翻倍(详见[物品字段](https://zh.stardewvalleywiki.com/%E6%A8%A1%E7%BB%84:%E7%89%A9%E4%BD%93)):

```js
{
    "Format": "2.5.0",
=======
`EditData` 编辑数据素材中的字段和条目。这支持简单字符串到字符串形的素材，如`Data/Achievements`，也支持模型类素材，如`Data/Objects`. 多个内容包可以编辑同一个素材。

你可以：
* 添加，编辑或删除条目；
* 在列表中重新排序条目；
* 或在条目中编辑单个字段。

此例子把苔藓汤的价格改到80（详见[物体字段](https://zh.stardewvalleywiki.com/%E6%A8%A1%E7%BB%84:%E7%89%A9%E4%BD%93)。

```js
{
    "Format": "2.6.0",
>>>>>>> c036414e8861adc25a9c6a2a7c1fac76501d9f05
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

<<<<<<< HEAD
可以使用 `EditData` 进行更多操作，包括添加完全自定义的物品和果树等。
详见[`Action: EditData`指南](author-guide/action-editdata.md)。
=======
`EditData`可实现的效果还有很多，包括添加完全自定义的物品，果树等。
详见[`Action: EditData`文档](author-guide/action-editdata.md)。
>>>>>>> c036414e8861adc25a9c6a2a7c1fac76501d9f05

</td>
</tr>
<tr>
<td><code>EditImage</code></td>
<td>

<<<<<<< HEAD
`EditImage` 可编辑游戏中的图像资产。多个内容包可以编辑同一个图像。

这让你可以——
* 编辑或替换图像的任何部分；
* 在现有图像上叠加新图像，支持设置透明度；
* 或扩展图像大小（例如在行走图中添加更多动作）。

例如，如果内容包中的`assets/tuna.png`图像带有自定义的[金枪鱼](https://zh.stardewvalleywiki.com/%E9%87%91%E6%9E%AA%E9%B1%BC)图片，这将替换游戏中的金枪鱼图片：

```js
{
   "Format": "2.5.0",
=======
`EditImage` 更改游戏中的图像素材。多个内容包可以编辑同一个素材。

你可以：
* 编辑或替换图像的任何部分；
* 将新图像叠加到现有图像上，支持透明度；
* 或扩展图像大小（例如，将更多的贴图添加到贴图集）。

此例子把原版[吞拿鱼](https://zh.stardewvalleywiki.com/%E9%87%91%E6%9E%AA%E9%B1%BC)替换成你提供的`assets/tuna.png`图片。

```js
{
   "Format": "2.6.0",
>>>>>>> c036414e8861adc25a9c6a2a7c1fac76501d9f05
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

<<<<<<< HEAD
详见[`Action: EditImage`指南](author-guide/action-editimage.md)。
=======
详见[`Action: EditImage`文档](author-guide/action-editimage.md)。
>>>>>>> c036414e8861adc25a9c6a2a7c1fac76501d9f05

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

<<<<<<< HEAD
例如，将小镇替换为内容文件夹中的自定义版本：
```js
{
    "Format": "2.5.0",
=======
例如，将城镇广场替换为内容文件夹中的自定义版本：
```js
{
    "Format": "2.6.0",
>>>>>>> c036414e8861adc25a9c6a2a7c1fac76501d9f05
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

<<<<<<< HEAD
详见[`Action: EditMap`指南](author-guide/action-editmap.md)。
=======
详见[`Action: EditMap`文档](author-guide/action-editmap.md)。
>>>>>>> c036414e8861adc25a9c6a2a7c1fac76501d9f05

</td>
</tr>
<tr>
<td><code>Include</code></td>
<td>

<<<<<<< HEAD
`Include`添加另一个文件中的补丁。这只是一种将内容包分成
多个文件，而不是将所有内容都放在一个`content.json`文件中的方法。包含的补丁就像直接在`content.json`一样应用。

例如，您可以将其与[Tokens和条件](#Tokens和条件)结合起来加载动态文件：
```js
{
    "Format": "2.5.0",
=======
`Include`从另外一个JSON文件里加载更多补丁。这只是将内容包组织到子文件的方式，而不是将所有内容都放在一个`content.json`中。被引用的补丁和`content.json`里的补丁功能一致。

例如，你可以将其与[Tokens和条件](#tokens)结合起来加载动态文件：
```js
{
    "Format": "2.6.0",
>>>>>>> c036414e8861adc25a9c6a2a7c1fac76501d9f05
    "Changes": [
        {
            "Action": "Include",
            "FromFile": "assets/john_{{season}}.json"
        }
    ]
}
```

<<<<<<< HEAD
详见[`Action: Include`指南](author-guide/action-include.md)。
=======
详见[`Action: Include`文档](author-guide/action-include.md)。
>>>>>>> c036414e8861adc25a9c6a2a7c1fac76501d9f05

</td>
</tr>
</table>

<<<<<<< HEAD
### 自定义地点
`CustomLocations`可让你添加新的游戏地点，并为其配备自己的地图和传送点。 Content Patcher 会自动处理NPC寻路，对象持续时间等问题。

详见[自定义地点指南](author-guide/custom-locations.md)。
<a name="tokens"></a>

### Tokens和条件
=======
### 自定义地点<a name="custom-locations"></a>
`CustomLocations`可让你添加新的游戏地点，并为其配备自己的地图和传送点。 Content Patcher会自动处理NPC寻路，对象持续时间等问题。

详见[自定义地点指南](author-guide/custom-locations.md)。

### Tokens和条件<a name="tokens"></a><a name="conditions"></a>
>>>>>>> c036414e8861adc25a9c6a2a7c1fac76501d9f05
前面的章节介绍了如何进行静态更改，但你也可以使用tokens和条件来进行 _动态_ 更改。

例如，你可以——
* 根据季节、回答的对话问题等多种因素更换补丁、游戏进度等。
* 使用随机、算术和动态查询；
* 还有更多别的动态更改。

例如，这让阿比盖尔在每个季节都有不同的肖像：

```js
{
<<<<<<< HEAD
    "Format": "2.5.0",
=======
    "Format": "2.6.0",
>>>>>>> c036414e8861adc25a9c6a2a7c1fac76501d9f05
    "Changes": [
        {
            "Action": "Load",
            "Target": "Portraits/Abigail",
            "FromFile": "assets/abigail-{{season}}.png"
        }
    ]
}
```

<<<<<<< HEAD
如果你和她结婚了，这个条件可以让她有不同季节的肖像：

```js
{
    "Format": "2.5.0",
=======
这让阿比盖尔结婚跟你结婚后使用不同的肖像

```js
{
    "Format": "2.6.0",
>>>>>>> c036414e8861adc25a9c6a2a7c1fac76501d9f05
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

<<<<<<< HEAD
### 玩家设置
你可以让玩家使用`config.json`文件配置你的模组。如果玩家有[通用模组配置菜单GMCM](https://www.nexusmods.com/stardewvalley/mods/5098)，玩家还能
通过游戏内的选项菜单配置不同的更改。

例如，你可以将配置值用作[Tokens和条件](#Tokens和条件):

```js
{
    "Format": "2.5.0",
=======
### 玩家设置<a name="player-config"></a>
你可以让玩家使用`config.json`文件设置你的模组。如果玩家有[通用模组设置菜单GMCM](https://www.nexusmods.com/stardewvalley/mods/5098)，玩家还能
通过游戏内的选项菜单设置不同的更改。

例如，你可以将设置值用作[Tokens和条件](#tokens):

```js
{
    "Format": "2.6.0",
>>>>>>> c036414e8861adc25a9c6a2a7c1fac76501d9f05
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

<<<<<<< HEAD
### 翻译
你可以在模组中添加翻译文件，并通过`i18n` 访问翻译文件。如果某些内容没有翻译成当前的
语言， Content Patcher 将自动处理，显示默认（英文）文本。

例如，如果你的 `i18n`文件包含一个关键字为 `rainy-day` 的翻译，你可以在任何支持[Tokens和条件](#Tokens和条件)的Content Patcher 字段里访问它：

```js
{
    "Format": "2.5.0",
=======
### 翻译<a name="translations"></a>
你可以在模组中添加翻译文件，并通过`i18n` token访问翻译文件。如果某些内容没有翻译成当前的
语言，Content Patcher将自动处理，显示默认（英文）文本。

例如，如果你的`i18n`文件包含一个关键字为`rainy-day`的翻译，你可以在任何支持[Tokens和条件](#tokens)的Content Patcher 字段里访问它：

```js
{
    "Format": "2.6.0",
>>>>>>> c036414e8861adc25a9c6a2a7c1fac76501d9f05
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

<<<<<<< HEAD
详见[翻译指南](author-guide/translations.md)。

### 文本操作
文本操作可让你根据文本字段的已有值进行修改，而不只是设置一个新的值。例如，您可以在当前值上添加其他文本。

例如，这增加了河豚作为普遍喜爱的礼物：
=======
See the [translation documentation](author-guide/translations.md) for more info.

### 文本操作<a name="text-operations"></a>
文本操作可让你根据文本字段的已有值进行修改，而不只是设置一个新的值。例如，你可以在当前值上添加其他文本。

For example, this adds pufferfish as a universally loved gift:
>>>>>>> c036414e8861adc25a9c6a2a7c1fac76501d9f05

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

<<<<<<< HEAD
详见[文本操作指南](author-guide/text-operations.md)。

### 触发动作
Content Patcher 为特定内容添加自定义[触发动作](https://zh.stardewvalleywiki.com/%E6%A8%A1%E7%BB%84:%E8%A7%A6%E5%8F%91%E5%8A%A8%E4%BD%9C)，例如更新存档的命名ID。

详见[Content Patcher的触发动作指南](author-guide/trigger-actions.md)。

## 故障排除
详见[故障排除指南](author-guide/troubleshooting.md)。

## 常见问题
<a name="when-to-change"></a>

### 多久应用一次补丁更改？
每次加载数据资产时，你的补丁都会**应用数据资产**，但它们会通过不同的条件**更新字段**。例如，假设你有这样一个补丁：
=======
See the [text operations documentation](author-guide/text-operations.md) for more info.

### 触发动作<a name="trigger-actions"></a>
Content Patcher 为特定内容添加自定义[触发动作](https://zh.stardewvalleywiki.com/%E6%A8%A1%E7%BB%84:%E8%A7%A6%E5%8F%91%E5%8A%A8%E4%BD%9C)，例如更新存档的命名ID。

See [Content Patcher's trigger action documentation](author-guide/trigger-actions.md) for more info.

## 故障排除<a name="troubleshoot"></a>
详见[故障排除指南](author-guide/troubleshooting.md)。

## 常见问题<a name="faqs"></a>

### 多久应用一次补丁更改？<a name="update-rate"></a>
每次加载数据素材时，你的补丁都会**应用数据素材**，但它们会通过不同的条件**更新字段**。例如，假设你有这样一个补丁：
>>>>>>> c036414e8861adc25a9c6a2a7c1fac76501d9f05
```js
{
    "Action": "EditMap",
    "Target": "Maps/Town",
    "SetProperties": {
        "CurrentTime": "{{Time}}"
    }
}
```

<<<<<<< HEAD
当一天开始时， Content Patcher 会更新补丁，使其包含`"CurrentTime": "600"`。它
不管你是否在当天晚些时候重新加载应用了该补丁的地图，该补丁仍然包含`"CurrentTime": "600"` 直到其字段被更新。

可以添加`Update`字段以便更频繁地更新。可能的值有：

更新速度            | 作用
------------------ | ------
`OnDayStart`       | _(默认)_ 当游戏中的一天开始时更新。即使省略该字段也会始终启用。
`OnLocationChange` | 当玩家传送到一个新地点时更新。
`OnTimeChange`     | 当游戏内时间变化时更新。
_使用多种更新速度_  | 你可以指定多个以逗号分隔的值，如`"Update": "OnLocationChange, OnTimeChange"`.

例如，当游戏中的时间发生变化时，它会更新并重新应用补丁：
=======
当游戏中的一天开始时，Content Patcher会将此补丁更新为`"CurrentTime": "600"`。如果此补丁在当天晚些时候重新生效，它依然是`"CurrentTime": "600"`。

你可以使用`Update`字段实现更频繁的更新。

更新频率            | 作用
------------------ | ------
`OnDayStart`       | _(default)_ 游戏中的一天开始时更新。没有`Update`字段的补丁将默认使用此更新频率。
`OnLocationChange` | 玩家传送到另一地图时更新。
`OnTimeChange`     | 时间变换时更新（如6:10到6:20）。
_多种更新频率_         | 您可以指定由逗号分隔的多个值，如`"Update": "OnLocationChange, OnTimeChange"`。

此例子在游戏内时间变换时把补丁中`CurrentTime`的值更新为现在时间，然后重新生效。
>>>>>>> c036414e8861adc25a9c6a2a7c1fac76501d9f05
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

<<<<<<< HEAD
<a name="compatibility"></a>

### Content Patcher更新是否与旧版本兼容？
兼容。详见[作者迁移指南](author-migration-guide.md)。

<a name="change-language"></a>

### 如何更改另一种语言的资产？
**默认影响所有语言**

`Target`字段中的资产名称不包括语言。例如`"Target": "Dialogue/Abigail"`（资产名称）将改变德语版游戏的`Content/Dialogue/Abigail.de-DE.xnb`（文件路径）。如果你想在每种语言中做同样的更改，就不需要做其他任何事情了。

要针对特定语言，可以添加语言条件：
=======

### Content Patcher更新是否与旧版本兼容？<a name="are-content-patcher-updates-backwards-compatible"></a>
兼容。详见[作者迁移指南](author-migration-guide.md)。


### 如何更改另一种语言的素材？<a name="how-do-i-change-assets-in-another-language"></a>
**默认影响所有语言**

The asset name in the `Target` field doesn't include the language. For example,
`"Target": "Dialogue/Abigail"` (the asset name) will change the content loaded from
`Content/Dialogue/Abigail.de-DE.xnb` (the file path) when playing in German. If you want
to make the same change in every language, you don't need to do anything else.


`Target`字段里的素材名称不包含语言。如果你使用`"Target": "Dialogue/Abigail"`并把游戏语言设为德语，`Content/Dialogue/Abigail.de-DE.xnb`的内容将会被编辑。如果你希望某个更改在所有语言中生效，你不需要做任何特殊处理。


如果你想编辑特定语言，你可以加一个语言条件，如下：
>>>>>>> c036414e8861adc25a9c6a2a7c1fac76501d9f05
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

<<<<<<< HEAD
如果存在翻译文件，它可以自动加载。这样，你只需添加翻译文件到你的内容包；如果没有翻译文件，它将默认为未翻译版本的语言。
例如：

```js
// 如果内容包中存在翻译版本，则使用该版本
=======
You can also load the translated version automatically if it exists. That way you can just add
translated files to your content pack, and it'll default to the untranslated version if no
translation exists:
你也可以在某语言有翻译版文档存在时自动加载它，这样可以在有翻译版时自动使用翻译版，无翻译版时默认使用原语言版。

```js
// 如果它存在于内容包中，使用翻译版图片
>>>>>>> c036414e8861adc25a9c6a2a7c1fac76501d9f05
{
   "Action": "EditImage",
   "Target": "LooseSprites/Cursors",
   "FromFile": "assets/cursors.{{language}}.png",
   "When": {
      "HasFile:{{FromFile}}": true
   }
},

<<<<<<< HEAD
// 否则使用未翻译版本
=======
// 默认使用未翻译的版本
>>>>>>> c036414e8861adc25a9c6a2a7c1fac76501d9f05
{
   "Action": "EditImage",
   "Target": "LooseSprites/Cursors",
   "FromFile": "assets/cursors.png",
   "When": {
      "HasFile: assets/cursors.{{language}}.png": false
   }
},
```

<<<<<<< HEAD
<a name="how-interact"></a>

### 多个补丁如何交互？
同一文件可使用任意数量的补丁。`Action: Load`总是先于其他操作。
但除此之外，每个补丁都是按顺序应用的。每个补丁完成后，下一个
补丁会把修改过的数据资产合并并输入。
=======

### 多个补丁如何交互？<a name="how-do-multiple-patches-interact"></a>
同一文件可使用任意数量的补丁。`Action: Load`总是先于其他操作。
但除此之外，每个补丁都是按顺序应用的。每个补丁完成后，下一个
补丁会把修改过的数据素材合并并输入。
>>>>>>> c036414e8861adc25a9c6a2a7c1fac76501d9f05

在一个内容包内，补丁会按照在`content.json`中列出的顺序应用。 
如果你有多个内容包，每个内容包都会按照 SMAPI 加载的顺序应用；如果你需要依赖另一个内容包，请参阅[manifest中的依赖](https://zh.stardewvalleywiki.com/%E6%A8%A1%E7%BB%84:%E5%88%B6%E4%BD%9C%E6%8C%87%E5%8D%97/APIs/Integrations).

<<<<<<< HEAD
### 已知限制
某些游戏资产具有特殊逻辑。这不是 Content Patcher 特有的限制，但为了保险起见故在此列出这些限制。

资产 | 说明
----- | -----
`Characters/Dialogue/*` | 对话是在一天开始时设置的，因此设置[自定义更新速度](#多久应用一次补丁更改？)不会影响一天开始后的对话。（不过你可以用[特定位置对话键](https://zh.stardewvalleywiki.com/%E6%A8%A1%E7%BB%84:%E5%AF%B9%E8%AF%9D#%E5%9C%B0%E7%82%B9%E5%AF%B9%E8%AF%9D)来规避这个问题）。
`Characters/Farmer/accessories` | 附件的数量是硬编码，因此自定义附件需要替换现有附件。
`Characters/Farmer/skinColors` | 皮肤颜色的数量是硬编码，因此自定义颜色需要替换现有颜色。
`Data/SpecialOrders` | 游戏会在 _保存之前_ 缓存该资产的副本，并在首次打开会话的特殊订单板时加载单独的副本。有条件地添加/删除特殊命令时要非常小心，因为当玩家试图从新列表中接受缓存列表中不存在的特殊订单命令时，可能会导致游戏崩溃。
`Maps/*` | 参见维基上的[地图编辑中的潜在问题](https://zh.stardewvalleywiki.com/%E6%A8%A1%E7%BB%84:%E5%9C%B0%E5%9B%BE#%E6%BD%9C%E5%9C%A8%E9%97%AE%E9%A2%98)。

## 另见
=======
### 已知限制<a name="known-limitations"></a>
某些游戏素材具有特殊逻辑。这不是Content Patcher特有的限制，但为了保险起见故在此列出这些限制。

asset | notes
----- | -----
`Characters/Dialogue/*` | 对话是在一天开始时设置的，因此设置[自定义更新速度](#update-rate)不会影响一天开始后的对话。（不过你可以用[特定位置对话键](https://zh.stardewvalleywiki.com/%E6%A8%A1%E7%BB%84:%E5%AF%B9%E8%AF%9D#%E5%9C%B0%E7%82%B9%E5%AF%B9%E8%AF%9D)来规避这个问题）。
`Characters/Farmer/accessories` | 附件的数量是硬编码，因此自定义附件需要替换现有附件。
`Characters/Farmer/skinColors` | 皮肤颜色的数量是硬编码，因此自定义颜色需要替换现有颜色。
`Data/SpecialOrders` | 游戏会在 _保存之前_ 缓存该素材的副本，并在首次打开会话的特殊订单板时加载单独的副本。有条件地添加/删除特殊命令时要非常小心，因为当玩家试图从新列表中接受缓存列表中不存在的特殊订单命令时，可能会导致游戏崩溃。
`Maps/*` | 参见维基上的[地图编辑中的潜在问题](https://zh.stardewvalleywiki.com/%E6%A8%A1%E7%BB%84:%E5%9C%B0%E5%9B%BE#%E6%BD%9C%E5%9C%A8%E9%97%AE%E9%A2%98)。

## 参见<a name="see-also"></a>
>>>>>>> c036414e8861adc25a9c6a2a7c1fac76501d9f05
* [README](README.md)
* [帮助](https://zh.stardewvalleywiki.com/%E6%A8%A1%E7%BB%84:%E5%B8%AE%E5%8A%A9)
