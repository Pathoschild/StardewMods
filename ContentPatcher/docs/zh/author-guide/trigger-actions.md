← [模组作者指南](../author-guide.md)

此文档介绍 Content Patcher 所添加的自定义[触发动作](https://zh.stardewvalleywiki.com/模组:触发动作)。

**🌐 其他语言：[en (English)](../../author-guide/trigger-actions.md)。**

## 目录
* [`MigrateIds`](#migrateids)
* [参见](#see-also)

## `MigrateIds`
`Pathoschild.ContentPatcher_MigrateIds` [触发动作](https://zh.stardewvalleywiki.com/模组:触发动作)用于在更改事件、物品、邮件、食谱或歌曲的ID时更新现有存档。例如，这可以用于帮助旧存档迁移到新的[唯一字符串 ID](https://zh.stardewvalleywiki.com/模组:公共数据字段#唯一字符串ID)。

参数格式为 `<类型> [<旧ID> <新ID>]+`：

<table>
<tr>
<th>参数</th>
<th>用法</th>
</tr>
<tr>
<td><code>&lt;类型&gt;</code></td>
<td>

必须是以下之一：`CookingRecipes`、`CraftingRecipes`、`Events`、`Items`、`Mail`、`Songs`。

</td>
</tr>
<tr>
<td><code>&lt;旧 ID&gt;</code></td>
<td>

游戏数据中要查找并迁移的旧 ID。

如果类型是 `Items`，且它曾经被定义在……
* 数据素材，如 `Data/Objects` 中：
  使用[Qualified item ID](https://zh.stardewvalleywiki.com/模组:公共数据字段#物品ID)，例如 `(O)OldId`。
* **当前未安装**的 Json Assets 内容包中：
  使用 `"JsonAssets:<类型>:<名称>"` 形式的ID。可使用 `big-craftables`、`clothing`、`hats`、`objects` 或 `weapons`。例如一个原名为 Puffer Hat 的帽子 ID 表示为 `"JsonAssets:hats:Puffer Hat"`。
* **已安装**的 Json Assets 内容包中：
  使用 [Json Assets token](https://github.com/spacechase0/StardewValleyMods/blob/develop/JsonAssets/docs/author-guide.md#integration-with-content-patcher) 来获取其实际 ID，用作[Qualified item ID](https://zh.stardewvalleywiki.com/模组:公共数据字段#物品ID)。例如`(O){{spacechase0.JsonAssets/ObjectId: Puffer Hat}}`。

</td>
</tr>
<tr>
<td><code>&lt;新 ID&gt;</code></td>
<td>

迁移后的新 ID。

对于物品，建议使用 [Qualified item ID](https://zh.stardewvalleywiki.com/模组:公共数据字段#物品ID)，避免产生歧义。

</td>
</tr>
</table>

您可以使用不限数量的旧 ID/新 ID 对。

以下范例将 `Puffer Plush` 迁移到 `{{ModId}}_PufferPlush`，`Puffer Sofa` 迁移到 `{{ModId}}_PufferSofa`：

```js
{
    "Action": "EditData",
    "Target": "Data/TriggerActions",
    "Entries": {
        "{{ModId}}_MigrateIds": {
            "Id": "{{ModId}}_MigrateIds",
            "Trigger": "DayStarted",
            "Actions": [
                // 注：带有空格的参数必须使用双引号. 此范例的 Action 使用单引号，所以不需要转义其中的双引号。
                'Pathoschild.ContentPatcher_MigrateIds CraftingRecipes "Puffer Plush" {{ModId}}_PufferPlush "Puffer Sofa" {{ModId}}_PufferSofa'
            ],
            "HostOnly": true
        }
    }
}
```

> [!IMPORTANT]  
> Content Patcher 需要访问完整游戏状态才能执行此触发动作。如果出现以下情况，触发动作会报错：
>* 没有设置 `"Trigger": "DayStarted"` 和 `"HostOnly": true`
>* 触发动作被 `Data/TriggerActions` 以外的机制触发

## 参见<a name="see-also"></a>
* 其他操作和选项请参见[模组作者指南](../author-guide.md)；
* Wiki 上的[触发动作](https://zh.stardewvalleywiki.com/模组:触发动作)。
