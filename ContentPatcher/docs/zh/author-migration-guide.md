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

兼容，甚至给 1.0.0 版写的内容包也可生效。Content Patcher 使用[`Format` 字段](author-guide.md#format)在需要时自动将内容包转换为新的版本（不更改文件本身）。

</dd>

<dt>我需要更新我的内容包吗？</dt>
<dd>

大部分时候您的内容包不需要手动更新也可无限期使用。游戏本体更改可能会使得某些内容包无法使用（但 Content Patcher 会试图重写这些内容包）。

但是，使用旧 `Format` 版本有诸多弊端。您的内容包：

* 无法使用新功能；
* 可能具有与当前文档不符的旧行为；
* 可能会增加启动时间或导致游戏内卡顿。重写代码有时很复杂且效率低下，因此，使用已经更新的代码要快得多；
* 可能有更多错误。例如，`Format` 1.0 版的内容包具有数十个自动化应用迁移，这增加了某些东西会被错误迁移的可能性。

强烈建议在更新内容包时迁移到最新格式。

</dd>

<dt>如何更新我的内容包？</dt>
<dd>

您只需将 `Format` 字段设为[作者指南](author-guide.md)中显示的最新版本，之后阅读以下章节来确定您需要更改的内容。如果某个版本未在此页中列出，那么您不需要为这一版本进行任何修改。

</dd>

<dt>
  为什么我的内容包在 SMAPI 控制台显示“reported warnings when applying runtime migration 2.0.0”？
</dt>
<dd>

您的内容包中有一个早于 Stardew Valley 1.6 的 `Format` 版本，因此 Content Patcher 在试图自动迁移您的内容包至新的资源格式时失败了。

您可以通过以下措施来修复此问题
1. 在 `content.json` 里设置 `"Format": "2.0.0"`；
2. 更新您的内容包到最新的 Content Patcher 和 Stardew Valley 格式（请参阅下文）。

</dd>
</dl>

> [!TIP]
> 如果您需要帮助，可以随时在 [Discord](https://smapi.io/community#Discord) 上询问。

## 迁移指南<a name="migration-guides"></a>

这些更改只有在您将 `Format` 设置为列出的版本或更高版本时适用。全部更改请参见[发行说明](../release-notes.md)（未翻译）。

### 2.1
于 2024 年 5 月 22 日发布。

* `"Action": "Load"` 补丁**只有**在原版游戏中有本地化形式素材时才会自动加载到所有本地化素材中：

  例如，此补丁现在只会加载 `Characters/Toddler`，而不会试图加载如 `Characters/Toddler.fr-FR` 这样的本地化变体：
  ```json
  {
      "Action": "Load",
      "Target": "Characters/Toddler",
      "FromFile": "assets/toddler.png"
  }
  ```

  这对于大部分内容包没有影响，除了修复一些非英语玩家的编辑问题。

### 2.0
于 2024 年 3 月 19 日发行。

<ul>
<li>

游戏本身的内容更改请参阅[迁移至游戏本体 1.6](https://zh.stardewvalleywiki.com/模组:迁移至游戏本体1.6)。

</li>
<li>

为 [`Load` 补丁](author-guide/action-load.md)新增了一个 `Priority` 字段。此字段是可选的，但您可以在合适的时候使用它来提高模组兼容性。

</li>
<li>

[`CustomLocations`](author-guide/custom-locations.md) 已弃用。您应该将自定义地点添加到 1.6 版游戏本体的[新的 `Data/Locations` 素材](https://zh.stardewvalleywiki.com/模组:地点数据)。

例如，如果您有一个自定义位置如下：

```js
"CustomLocations": [
    {
        "Name": "Custom_ExampleMod_AbigailCloset",
        "FromMapFile": "assets/abigail-closet.tmx"
    }
]
```

您可以直接将此地点添加到游戏中：

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

游戏使用标准的[唯一字符串 ID](https://zh.stardewvalleywiki.com/模组:公共数据字段#唯一字符串ID) 格式作为地点名称。以上示例使用了新格式（`{{ModId}}_AbigailCloset`）并把旧名字（`Custom_ExampleMod_AbigailCloset`）添加到 `FormerLocationNames` 字段中，以便将此地点在现有存档中自动迁移为新的名称。

Content Patcher 会自动将 {{ModId}} 替换为您的模组 [Manifest 中的 `UniqueId`](https://zh.stardewvalleywiki.com/模组:制作指南/APIs/Manifest)。

**已知限制：**
* 您不能直接将 `TMXL Map Toolkit` 提供的地点迁移到 `Data/Locations`。如果您需要继续支持 `TMXL`，您可以继续使用依旧支持 `TMXL` 地点的 `CustomLocations`，然后使用 `EditData` 来编辑 `Data/Locations` 来更改您地点的数据。

</li>
</ul>

### 1.25
于 2022 年 2 月 27 日发行。
* **`Enabled` 字段不再被支持**，您应该使用 `When` 来实现条件化补丁。

### 1.24
于 2021 年 10 月 31 日发行。

* **`Spouse` 令牌不再包括室友**：如果您想同时检查室友和配偶，可使用 `{{Merge: {{Roommate}}, {{Spouse}}}}` 匹配之前的效果。
* **有些令牌的返回顺序有更改**到对应游戏的排列。绝大部分内容包不被此更改影响，除非有在 `HasActiveQuest`、`HasCaughtFish`、`HasDialogueAnswer`、`HasFlag`、`HasProfession` 和 `HasSeenEvent` 使用 `valueAt`。

### 1.21
于 2021 年 3 月 7 日发行。
* **`Enabled` 字段不再支持令牌**，您应该使用 `When` 来实现条件化补丁。

### 1.20
于 2021 年 2 月 6 日发行。

* `Weather` 令牌默认返回当前地点上下文（如 island 或 valley）的天气。您可以使用 `{{Weather: Valley}}` 匹配之前的效果。

### 1.18
于2020年9月12日发行。

* **不再支持使用 `FromFile` 的 `EditData` 补丁**：这个格式和其他补丁中的 `FromFile` 不一样，经常造成混淆，所以此功能从 1.16 版开始已弃用。

  这不影响非 `EditData` 补丁的 `FromFile`，和不使用 `FromFile` 的 `EditData`。

  如果您有这样的补丁：

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

  您可以迁移到这个格式：

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
于 2020 年 8 月 16 日发行。

* **地点变更时的补丁更新**：使用 `LocationName` 或 `IsOutdoors` 令牌/条件不再会使补丁在玩家更换地点时更新。您可以添加此字段来开启更新频率。

  ```js
  "Update": "OnLocationChange"
  ```

### 1.15
于 2020 年 7 月 4 日发布。

* **令牌查找语法**：之前您可以使用`{{Season: Spring}}`在某些令牌里查找某一值。现在此操作需要写成 `{{Season |contains=Spring}}`，而且任何令牌都支持此操作。

  此更改影响所有令牌，**除了** `HasFile`、`HasValue`、`Hearts`、`Lowercase`/`Uppercase`、`Query`、`Random`、`Range`、`Round`、`Relationship`、`SkillLevel` 和模组提供的令牌。

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

* **随机固定键**：`Random` 令牌可使用一个固定键。原格式为 `{{Random: choices | pinned-key}}`；这应该更改为 `{{Random: choices |key=pinned-key}}`。

### 1.7
2019 年 5 月 8 日发布。

* `ConfigSchema`字段更改：
  * `AllowValues`不再是必须的字段。如果省略它，设置字段将允许 _任何_ 值。
  * 如果省略`Default`默认值为空，而不是第一个`AllowValues`值。

### 1.6
2018 年 12 月 8 日发布。

* `Weather` 令牌在有风的天返回 `Wind` 而不是 `Sun`。

## 参见<a name="see-also"></a>
* 其他信息请参阅[README](README.md)
* [寻求帮助](https://zh.stardewvalleywiki.com/模组:帮助)
