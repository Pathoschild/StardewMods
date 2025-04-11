<<<<<<< HEAD
﻿← [author guide](../author-guide.md)

The config feature lets you make dynamic changes in your content pack that depends on settings
selected by the player.

## Contents
* [Basic config](#basic-config)
  * [Overview](#overview)
  * [Define your config](#define-your-config)
  * [Examples](#examples)
* [Config UI](#config-ui)
  * [Display options](#display-options)
  * [Sections](#sections)
  * [Translations](#translations)
* [See also](#see-also)

## Basic config
## Overview
You can define your content pack's settings using the `ConfigSchema` field, then Content Patcher
will automatically add a `config.json` file and [in-game config UI](#config-ui) to let players edit
your options.

In your content pack code, you can then use config options as [tokens &
conditions](../author-guide.md#tokens) to make dynamic changes.

### Define your config
First you need to describe your config options for Content Patcher. You do that by adding a
`ConfigSchema` field (outside the `Changes` field which has your patches). Each config option has
a key used as the token name, and a data model containing these fields:

field               | meaning
------------------- | -------
`AllowValues`       | _(optional)_ The values the player can provide, as a comma-delimited string. If omitted, any value is allowed.<br />**Tip:** use `"true, false"` for a field that can be enabled or disabled, and Content Patcher will recognize it as a boolean (e.g. to represent as a checkbox in the [config UI](#config-ui)).
`AllowBlank`        | _(optional)_ Whether the field can be left blank. If false or omitted, blank fields will be replaced with the default value.
`AllowMultiple`     | _(optional)_ Whether the player can specify multiple comma-delimited values. Default false.
`Default`           | _(optional unless `AllowBlank` is false)_ The default values when the field is missing. Can contain multiple comma-delimited values if `AllowMultiple` is true. If omitted, blank fields are left blank.

Config names and fields are not case-sensitive.

### Examples
This `content.json` defines a `BillboardMaterial` config field and uses it to change which patch is
applied:

```js
{
   "Format": "2.5.0",
=======
﻿← [模组作者指南](../author-guide.md)

设置选项功能让你向玩家提供可更改的设置，并基于设置实现动态。

## 内容
* [基本设置](#basic-config)
  * [概述](#overview)
  * [设置定义](#define-your-config)
  * [示例](#examples)
* [设置菜单](#config-ui)
  * [显示选项](#display-options)
  * [分段](#sections)
  * [翻译](#translations)
* [参见](#see-also)

## 基本设置<a name="basic-config"></a>
## 概述<a name="overview"></a>

你可以使用`ConfigSchema`字段定义内容包的设置选项。Content Patcher将自动添加`config.json`文件和[游戏内设置菜单](#config-ui)并允许玩家更改你提供的设置。

在内容包内你可以把设置选项当作[Tokens和条件](#../author-guide.md#tokens)使用，从而实现动态改变。

### 设置定义<a name="define-your-config"></a>

使用设置选项的第一步用`ConfigSchema`字段来描述你的内容包所提供的选项。`ConfigSchema`是与`Format`和`Changes`同级的字段。每一个设置选项有一个作为token的键，和一个包含一下字段的模型：

一个以逗号分隔的字符串，代表玩家可选的值。如果省略，则允许任何值。

类型                 | 作用
------------------- | -------
`AllowValues`       | _（可选）_ 一个以逗号分隔的字符串，代表玩家可选的值。如果省略，则允许任何值。<br />**Tip:** 当你用`"true, false"`定义可启用/禁用的选项时，Content Patcher会认出它是布尔(并在[设置菜单](#config-ui)以复选框显示此设置).
`AllowBlank`        | _（可选）_ 该字段是否可以留空。如果false或省略，则将用默认值（`Default`）替换空白字段。
`AllowMultiple`     | _（可选）_ 玩家是否可以指定多个以逗号分隔的值。默认false。
`Default`           | _(可选，除非`AllowBlank`为false)_ 此设置的默认值。如果`AllowMultiple`为true的，则可以包含多个以逗号分隔的值。如果省略，默认值为空白。

设置选项的名称和字段不区分大小写。

### 示例<a name="examples"></a>

此`content.json`定义了一个名为`BillboardMaterial`的设置，并使用此设置token控制补丁效果。

```js
{
   "Format": "2.6.0",
>>>>>>> c036414e8861adc25a9c6a2a7c1fac76501d9f05
   "ConfigSchema": {
      "Material": {
         "AllowValues": "Wood, Metal",
         "Default": "Wood"
      }
   },
   "Changes": [
<<<<<<< HEAD
      // as a token
=======
      // 作为token
>>>>>>> c036414e8861adc25a9c6a2a7c1fac76501d9f05
      {
         "Action": "Load",
         "Target": "LooseSprites/Billboard",
         "FromFile": "assets/material_{{Material}}.png"
      },

<<<<<<< HEAD
      // as a condition
=======
      // 作为条件
>>>>>>> c036414e8861adc25a9c6a2a7c1fac76501d9f05
      {
         "Action": "Load",
         "Target": "LooseSprites/Billboard",
         "FromFile": "assets/material_wood.png",
         "When": {
            "Material": "Wood"
         }
      }
   ]
}
```

<<<<<<< HEAD
When you run the game, a `config.json` file will appear automatically with text like this:
=======
当你运行游戏时，Content Patcher会自动生成一个`config.json`文件：
>>>>>>> c036414e8861adc25a9c6a2a7c1fac76501d9f05

```js
{
  "Material": "Wood"
}
```

<<<<<<< HEAD
Players can edit that file to configure your content pack, or use the in-game
[config UI](#config-ui).

## Config UI
Content Patcher will automatically add an in-game UI to let players edit your settings, currently
using [Generic Mod Config Menu](https://www.nexusmods.com/stardewvalley/mods/5098). You can
optionally provide extra info to improve the config UI.

### Display options
There's two extra fields to customize how config UIs are rendered:

field         | meaning
------------- | -------
`Description` | _(optional)_ An explanation of the config option for the player, usually shown in the config UI as a tooltip.
`Section`     | _(optional)_ A section title to group related sections. See [_sections_](#sections) below.

### Sections
You can group your options into sections using the `Section` field. Options with no section are
always listed first, followed by sections in the order they first appeared in `ConfigSchema`.

For example, this adds two sections:

```js
{
    "Format": "2.5.0",
    "ConfigSchema": {
        // appearance section
=======
玩家可以编辑此`config.json`文件来改变设置，或使用游戏内设置[设置菜单](#config-ui)。

## 设置菜单<a name="config-ui"></a>
当你的内容包有设置选项时，Content Patcher会为你的内容包自动添加一个游戏内的设置菜单（现在基于[Generic Mod Config Menu](https://www.nexusmods.com/stardewvalley/mods/5098)）。你可以为此设置菜单提供一些可选的字段来进一步优化它。

### 显示选项<a name="display-options"></a>
现有两个控制设置菜单的字段：

类型           | 作用
------------- | -------
`Description` | _（可选）_ 配置选项的说明，在配置UI中显示为工具提示。
`Section`     | _（可选）_ 一个分段的标题。 详见[_分段_](#sections) below.

### 分段<a name="sections"></a>

你可以用`Section`字段把你的设置选项分段。没有分段的选项会先显示，之后以`ConfigSchema`中出现顺序显示有分段的选项。

例如，这些选项分到两个分段下：

```js
{
    "Format": "2.6.0",
    "ConfigSchema": {
        // 外观分段
>>>>>>> c036414e8861adc25a9c6a2a7c1fac76501d9f05
        "Material": {
            "AllowValues": "Wood, Metal",
            "Default": "Wood",
            "Section": "Appearance"
        },
        "Texture": {
            "AllowValues": "Grainy, Smooth",
            "Default": "Grainy",
            "Section": "Appearance"
        },

<<<<<<< HEAD
        // behavior section
=======
        // 行为分段
>>>>>>> c036414e8861adc25a9c6a2a7c1fac76501d9f05
        "Enabled": {
            "AllowValues": "true, false",
            "Default": "true",
            "Section": "Behavior"
        }
    },
    "Changes": [ ]
}
```

<<<<<<< HEAD
Which would look something like this in-game:

![](../screenshots/config-with-sections.png)

### Translations
By default your config options are shown as-is in the config UI, with no display names or tooltips
or translations:
=======
游戏内显示如下：

![](../screenshots/config-with-sections.png)

### 翻译<a name="translations"></a>
默认情况下，你的配置选项会显示为内置名，没有工具提示或翻译。
>>>>>>> c036414e8861adc25a9c6a2a7c1fac76501d9f05

![](../screenshots/config-plain.png)

You can add [translation files](https://stardewvalleywiki.com/Modding:Translations) for your config
<<<<<<< HEAD
to have a more user-friendly UI. To do that, create an `i18n/default.json` for your default text.
For each field, add any combination of these translation keys:

key format                             | description
:------------------------------------- | :----------
`config.<name>.name`                   | The field name.
`config.<name>.description`            | The field description (usually shown as a tooltip).
`config.<name>.values.<value>`         | The display text for an `AllowValues` value when shown in a dropdown or checkbox list.
`config.section.<section>.name`        | The [section](#sections) name.
`config.section.<section>.description` | The [section](#sections) description (usually shown as a tooltip).

All translation keys are optional, and they're not case-sensitive.

For example, let's add some translations for the previous screenshot:

```js
// in i18n/default.json
=======
to have a more . To do that, create an `i18n/default.json` for your default text.
For each field, add any combination of these translation keys:

你可以为设置添加[翻译文档]，从而实现更用户友好的UI。当你创建一个`i18n/default.json`后你可以给每一个设置提供这些翻译键（任何组合）：

键格式                             | 描述
:------------------------------------- | :----------
`config.<name>.name`                   | 设置选项名。
`config.<name>.description`            | 设置选项描述，一般在工具提示中显示。
`config.<name>.values.<value>`         | 对应每一个`AllowValues`的描述，显示于下拉列表或复选框列表中。
`config.section.<section>.name`        | [分段](#sections)名称。
`config.section.<section>.description` | [分段](#sections)描述，一般在工具提示中显示。

所有翻译键都是可选的，不区分大小写。

这个例子为以上的设置添加法语翻译：

```js
// i18n/default.json（英文）
>>>>>>> c036414e8861adc25a9c6a2a7c1fac76501d9f05
{
    "config.Material.name": "Material",
    "config.Material.description": "The material style for the billboard background.",
    "config.Material.values.Wood": "wood",
    "config.Material.values.Metal": "metal"
}

<<<<<<< HEAD
// in i18n/fr.json
=======
// i18n/fr.json（法语）
>>>>>>> c036414e8861adc25a9c6a2a7c1fac76501d9f05
{
    "config.Material.name": "Matériel",
    "config.Material.description": "Le style du matériel pour l'arrière-plan du panneau d'affichage.",
    "config.Material.values.Wood": "bois",
    "config.Material.values.Metal": "métal"
}
```

<<<<<<< HEAD
And now the config UI would look something like this for a French player:

![](../screenshots/config-with-translations.png)

See [_translations_ on the wiki](https://stardewvalleywiki.com/Modding:Translations) for more info.

## See also
* [Author guide](../author-guide.md) for other actions and options
=======
添加后法语玩家会看到以下界面：

![](../screenshots/config-with-translations.png)

详见[维基上的_翻译模组_页](https://zh.stardewvalleywiki.com/%E6%A8%A1%E7%BB%84:%E7%BF%BB%E8%AF%91%E6%A8%A1%E7%BB%84)。

## 参见<a name="see-also"></a>
* 其他操作和选项请参考[模组作者指南](../author-guide.md)
>>>>>>> c036414e8861adc25a9c6a2a7c1fac76501d9f05
