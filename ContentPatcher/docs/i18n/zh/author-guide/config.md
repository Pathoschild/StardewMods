← [author guide](../author-guide.md)

设置选项功能让你向玩家提供可更改的设置，并基于设置实现动态。

## Contents
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
   "Format": "2.5.0",
   "ConfigSchema": {
      "Material": {
         "AllowValues": "Wood, Metal",
         "Default": "Wood"
      }
   },
   "Changes": [
      // 作为token
      {
         "Action": "Load",
         "Target": "LooseSprites/Billboard",
         "FromFile": "assets/material_{{Material}}.png"
      },

      // 作为条件
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

当你运行游戏时，Content Patcher会自动生成一个`config.json`文件：

```js
{
  "Material": "Wood"
}
```

玩家可以编辑此`config.json`文件来改变设置，或使用游戏内设置[设置菜单](#config-ui)。

## 设置菜单<a name="config-ui"></a>
当你的内容包有设置选项时，Content Patcher会为你的内容包自动添加一个游戏内的设置菜单（现在基于[Generic Mod Config Menu](https://www.nexusmods.com/stardewvalley/mods/5098)）。你可以为此设置菜单提供一些可选的字段来进一步优化它。

### 显示选项<a name="display-options"></a>
现有两个控制设置菜单的字段：

类型           | 作用
------------- | -------
`Description` | _（可选）_ 配置选项的说明，在配置UI中显示为提示框。
`Section`     | _（可选）_ 一个分段的标题。 详见[_分段_](#sections) below.

### 分段<a name="sections"></a>
You can group your options into sections using the `Section` field. Options with no section are
always listed first, followed by sections in the order they first appeared in `ConfigSchema`.

For example, this adds two sections:

```js
{
    "Format": "2.5.0",
    "ConfigSchema": {
        // appearance section
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

        // behavior section
        "Enabled": {
            "AllowValues": "true, false",
            "Default": "true",
            "Section": "Behavior"
        }
    },
    "Changes": [ ]
}
```

Which would look something like this in-game:

![](../screenshots/config-with-sections.png)

### 翻译<a name="translations"></a>
By default your config options are shown as-is in the config UI, with no display names or tooltips
or translations:

![](../screenshots/config-plain.png)

You can add [translation files](https://stardewvalleywiki.com/Modding:Translations) for your config
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
{
    "config.Material.name": "Material",
    "config.Material.description": "The material style for the billboard background.",
    "config.Material.values.Wood": "wood",
    "config.Material.values.Metal": "metal"
}

// in i18n/fr.json
{
    "config.Material.name": "Matériel",
    "config.Material.description": "Le style du matériel pour l'arrière-plan du panneau d'affichage.",
    "config.Material.values.Wood": "bois",
    "config.Material.values.Metal": "métal"
}
```

And now the config UI would look something like this for a French player:

![](../screenshots/config-with-translations.png)

See [_translations_ on the wiki](https://stardewvalleywiki.com/Modding:Translations) for more info.

## 参见<a name="see-also"></a>
* 其他操作和选项请参考[模组作者指南](../author-guide.md)
