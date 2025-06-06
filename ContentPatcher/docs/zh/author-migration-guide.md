← [README](README.md)

此文档帮助模组作者将旧版本的Content Patcher内容包迁移到新版本。

**其他信息请参考[主README](README.md)**

**🌐 其他语言： [en (English)](../author-migration-guide.md)。**

## 目录
* [常见问题](#faqs)
* [迁移指南](#migration-guides)
  * [2.1](#21)
  * [2.0](#20)
  * [1.25](#125)
  * [1.24](#124)
  * [1.21](#121)
  * [1.20](#120)
  * [1.18](#118)
  * [1.17](#117)
  * [1.15](#115)
  * [1.7](#17)
  * [1.6](#16)
* [参见](#see-also)

## 常见问题<a name="faqs"></a>
<dl>
<dt>这些信息会影响作为玩家的我吗？</dt>
<dd>

不，这仅适用于内容包作者。现有的内容包应该可以正常工作。

</dd>

<dt>Content Patcher更新是否向后兼容？</dt>
<dd>

兼容，甚至给1.0.0版写的内容包也可生效。Content Patcher使用[`Format`字段](author-guide.md#format)在有需要时自动转换你的内容包（不更改文件本身）。

</dd>

<dt>我需要更新我的内容包吗？</dt>
<dd>

大部分时候你的内容包不需要手动更新也可无限期使用。游戏本体更改可能会使得某些内容包无法使用（但Content Patcher会试图重写这些内容包）。

但是，使用旧`Format`版本有很多弊处。你的内容包：

* 无法使用新功能。
* 可能具有与当前文档不符的旧行为。
* 可能会增加启动时间或导致游戏内滞后。重写代码有时后很复杂且效率低下，因此，已经更新的代码要快得多。
* 可能有更多错误。例如，`Format` 1.0版的内容包具有数十个自动化应用迁移，这增加了某些东西会被错误迁移的机会。

强烈鼓励你更新内容包时迁移到最新格式。

</dd>

<dt>如何更新我的内容包？</dt>
<dd>

将`Format`字段设为[作者指南](author-guide.md)中显示的最新版本，之后阅读以下分段并更改所需要的部分。如果某版本没有列在此页中，你不需要为那一版本更改任何东西。

</dd>

<dt>
  为什么我的内容包在SMAPI控制台显示"reported warnings when applying runtime migration 2.0.0"？
</dt>
<dd>

你的内容包有一个来自游戏本体1.6版本以前的`Format`版本，所以Content Patcher试图自动迁移你的内容包，并且失败了。

您可以通过一下措施来修复此问题
1. `content.json`里设`"Format": "2.0.0"`。
2. 将你的内容包更新到最新版Content Patcher和游戏本体的格式（详见以下）。

</dd>
</dl>

> [!TIP]
> 如果你有疑问可以在[Discord](https://smapi.io/community#Discord)上询问

## 迁移指南<a name="migration-guides"></a>

这些更改只有在你将`Format`设置到某版本或更高时才有用。全部更改请参见[（未翻译）发行说明](../release-notes.md)。

### 2.1
于2024年5月22日发布。

* `"Action": "Load"` 补丁 _只有_ 在原版游戏中有地域区分时才会自动加载到所有地域版本：

  例如，此补丁现在只会加载`Characters/Toddler`而不会试图加载如`Characters/Toddler.fr-FR`的地域版本：
  ```json
  {
      "Action": "Load",
      "Target": "Characters/Toddler",
      "FromFile": "assets/toddler.png"
  }
  ```

  这对于大部分内容包没有影响，除了修复一些非英语玩家的问题。

### 2.0
于2024年3月19日发行。

<ul>
<li>

游戏本身的内容更改请详见 _[迁移至游戏本体1.6](https://zh.stardewvalleywiki.com/模组:迁移至游戏本体1.6)_

</li>
<li>

[`Load`补丁](author-guide/action-load.md)有新的`Priority`字段。此补丁可选，但你可以在合适的时候使用它来加强兼容性。

</li>
<li>

[`CustomLocations`](author-guide/custom-locations.md)已弃用。你应该将地点添加到1.6版游戏本体的
[新的 `Data/Locations` 素材](https://zh.stardewvalleywiki.com/模组:地点数据)。

例如，如果你有这样的`CustomLocations`：

```js
"CustomLocations": [
    {
        "Name": "Custom_ExampleMod_AbigailCloset",
        "FromMapFile": "assets/abigail-closet.tmx"
    }
]
```

你可以直接将此地点添加到游戏中：

```js
"Changes": [
    // 添加地图
    {
        "Action": "Load",
        "Target": "Maps/{{ModId}}_AbigailCloset",
        "FromFile": "assets/abigail-closet.tmx"
    },

    // 添加地点
    {
        "Action": "EditData",
        "Target": "Data/Locations",
        "Entries": {
            "{{ModId}}_AbigailCloset": {
                "CreateOnLoad": { "MapPath": "Maps/{{ModId}}_AbigailCloset" },
                "FormerLocationNames": [ "Custom_ExampleMod_AbigailCloset" ]
            }
        }
    }
]
```

游戏地点名使用标准[唯一字符串ID](https://zh.stardewvalleywiki.com/模组:公共数据字段#唯一字符串ID)格式。以上例子使用了新格式(`{{ModId}}_AbigailCloset`)并把旧名字(`Custom_ExampleMod_AbigailCloset`)添加到`FormerLocationNames`将此地点自动在现有存档中迁移到新名字。

Content Patcher会自动将{{ModId}}替换为[你模组manifest中的`UniqueId`](https://zh.stardewvalleywiki.com/模组:制作指南/APIs/Manifest)。

**已知限制：**
* 你不能直接将`TMXL Map Toolkit`提供的地点迁移到`Data/Locations`。如果你需要继续支持`TMXL`，你可以继续使用依旧支持`TMXL`地点的`CustomLocations`，然后用`EditData`来编辑`Data/Locations`来更改你地点的数据。

</li>
</ul>

### 1.25
于2022年2月27日发行。
* **`Enabled`字段不再被支持**，你应该用`When`来实现条件化补丁。

### 1.24
于2021年10月31日发行。

* **`Spouse`令牌不再包括室友** 如果你想同时检查室友和配偶，可使用`{{Merge: {{Roommate}}, {{Spouse}}}}`匹配之前的效果。
* **有些令牌的返回顺序有更改**到对应游戏的排列。绝大部分内容包不被此更改影响，除非有在`HasActiveQuest`，`HasCaughtFish`，`HasDialogueAnswer`，`HasFlag`，`HasProfession`，和`HasSeenEvent`使用`valueAt`。

### 1.21
于2021年3月7日发行。
* **`Enabled`字段不再支持令牌**，你应该用`When`来实现条件化补丁。

### 1.20
于2021年2月6日发行。

* `Weather`令牌默认返回当前 _地点上下文_ （如island或valley）的天气。你可以用`{{Weather: Valley}}`匹配之前的效果。

### 1.18
于2020年9月12日发行。

* **不再支持使用`FromFile`的`EditData`补丁** 这个格式和其他补丁中的`FromFile`不一样，经常造成混淆，所以此功能从1.16版开始已弃用。

  这不影响非`EditData`补丁的`FromFile`，和不使用`FromFile`的`EditData`。

  如果你有这样的补丁：

  ```js
  // content.json里
  {
     "Action": "EditData",
     "Target": "Characters/Dialogue/Abigail",
     "FromFile": "assets/abigail.json"
  }

  // assets/abigail.json
  {
     "Entries": {
        "4": "Oh, hi.",
        "Sun_17": "Hmm, interesting..."
     }
  }
  ```

  你可以迁移到这个格式：

  ```js
  // content.json里
  {
     "Action": "Include",
     "FromFile": "assets/abigail.json"
  }

  // assets/abigail.json
  {
     "Changes": [
        {
           "Action": "EditData",
           "Target": "Characters/Dialogue/Abigail",
           "Entries": {
              "4": "Oh, hi.",
              "Sun_17": "Hmm, interesting..."
           }
        }
     ]
  }
  ```

### 1.17
于2020年8月16日发行。

* **地点变更时的补丁更新:** 使用`LocationName`或`IsOutdoors`令牌/条件不再会使补丁在玩家更换地点时更新。你可以添加此字段来开启更新频率。

  ```js
  "Update": "OnLocationChange"
  ```

### 1.15
于2020年7月4日发布。

* **令牌查找语法：** 之前你可以用`{{Season: Spring}}`在某些令牌里查找某一值。现在此操作需要写成`{{Season |contains=Spring}}`而且任何令牌都支持此操作。

  此更改影响所有令牌，_除了_ `HasFile`, `HasValue`, `Hearts`, `Lowercase`/`Uppercase`,
  `Query`, `Random`, `Range`, `Round` `Relationship`, `SkillLevel`，和模组提供令牌。

  这也影响条件：
  ```js
  "When": {
    "Season: Spring": "true" // 应该改成 "Season |contains=Spring": "true"
  }
  ```

  这种条件没有影响：
  ```js
  // still okay!
  "When": {
    "Season": "Spring"
  }
  ```

* **随机固定键:** `Random`令牌可使用一个固定键。原格式为 `{{Random: choices | pinned-key}}`；这应该更改为`{{Random: choices |key=pinned-key}}`.

### 1.7
2019年5月8日发布。

* `ConfigSchema`字段更改：
  * `AllowValues`不再是必须的字段。如果省略它，设置字段将允许 _任何_ 值。
  * 如果省略`Default`默认值为空，而不是第一个`AllowValues`值。

### 1.6
2018年12月8日发布。

* `Weather`令牌在有风的天返回`Wind`而不是`Sun`。

## 参见<a name="see-also"></a>
* 其他信息详见[README](README.md)
* [寻求帮助](https://zh.stardewvalleywiki.com/模组:帮助)
