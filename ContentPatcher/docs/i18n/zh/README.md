**Content Patcher** 是一款[星露谷物语](http://stardewvalley.net/)模组，用于加载内容包并以不更换游戏原有文件的形式更改游戏数据，贴图，和地图。
<!--

关于此汉化

原版文档更新时：
cd <repository>
rsync --update --recursive --exclude 'i18n' ContentPatcher/docs/ ContentPatcher/docs/i18n/zh/

不汉化以下文档：
release-notes.md - 长，而且内容跟正式文档重复
author-tokens-guide.md - 已转移到author-guide/tokens.md

不汉化以下名词：
任何需要写入模组json的词，如"Action"
token - 令牌/变量

Wiki链接换成中文wiki
https://zh.stardewvalleywiki.com/%E6%A8%A1%E7%BB%84:%E7%9B%AE%E5%BD%95

-->

## 目录
* [玩家指南](#for-players)
  * [安装](#install)
  * [兼容](#compatibility)
  * [内容包配置](#configure-content-packs)
  * [多人](#multiplayer)
* [模组作者指南](#for-mod-authors)
* [配置](#configure)
* [参见](#see-also)

## 玩家指南<a name="for-players"></a>
### 安装<a name="install"></a>
1. [安装最新版SMAPI](https://smapi.io/)。
2. 从[Nexus Mods](https://www.nexusmods.com/stardewvalley/mods/1915)安装此模组。
3. 解压任意Content Patcher内容包并放入`Mods`文件夹中以进行安装。
4. 使用SMAPI运行游戏。

完成这些步骤后，`Mods`文件夹中将被自动加载并应用。

### 兼容<a name="compatibility"></a>
Content Patcher与Linux/macOS/Windows星露谷物语1.6+版本兼容，包括单人游戏和多人游戏。

### 内容包配置<a name="configure-content-packs"></a>
许多内容包可以使用 `config.json` 文件进行配置，Content Patcher将在你安装了该内容包后首次启动游戏时创建该文件。（若没有出现 `config.json`，则该模组不提供配置选项。）

如果你安装了[Generic Mod Config Menu](https://www.nexusmods.com/stardewvalley/mods/5098)，
Content Patcher会自动将可配置的内容包添加到其游戏内菜单中：

![](screenshots/config-with-sections.png)

### 多人<a name="multiplayer"></a>
Content Patcher兼容多人游戏。最好所有玩家都拥有相同的内容包，但这不是必须的。
如果某些玩家没有安装内容包，则会出现以下现象:

更改类型    | 现象
---------- | ------
贴图        | 只有安装了该内容包的玩家才能看到贴图更改
地图       | 只有安装了该内容包的玩家才能看到地图更改。没有自定义地图的玩家将会看到原版地图，并且会受到普通界限的限制（例如，他们可能会看到其他玩家穿过墙壁，但他们无法跟随）。
数据       | 只有安装了该内容包的玩家才会受到影响。例如，当你在某个内容包在`Data/Objects`加入自定义值后生成一个物品, 其他玩家即使`Data/Objects`没有被内容包更改也会看到该物品的自定义值。

## 模组作者指南<a name="for-mod-authors"></a>
* 创建内容包请参阅[模组作者指南](author-guide.md)及其[tokens 子页面](author-guide/tokens.md)。
* 从SMAPI mod添加自定义Content Patcher tokens，请参阅[扩展性API](extensibility.md)。
* 从SMAPI mod调用Content Patcher conditions和token strings，请参阅[条件 API](conditions-api.md)和[token string API](token-strings-api.md)。

## 配置<a name="configure"></a>
Content Patcher在首次启动游戏时创建`config.json`文件。你可以用文本编辑器中打开该文件来配置此模组。

可更改以下配置：

<table>
<tr>
  <th>配置</th>
  <th>效果</th>
</tr>

<tr>
  <td><code>EnableDebugFeatures</code></td>
  <td>

默认`false`（否）。是否启用[专为内容包模组作者设计的调试功能](author-guide/troubleshooting.md#debug-mode)。

  </td>
</tr>

<tr>
  <td><code>Controls</code></td>
  <td>

配置的手柄、键盘和鼠标按钮（参见 [键绑定](https://stardewvalleywiki.com/Modding:Key_bindings)).
默认绑定为：

* `F3`显示[调试模式](author-guide/troubleshooting.md#debug-mode) (需启用调试功能);
* `LeftControl`和`RightControl`切换调试模式中的贴图。

可以用逗号分隔绑定键（例如`B,LeftShoulder`，绑定`B`或`LeftShoulder`），也可使用加号设置多键绑定（例如`LeftShift + B`）。

  </td>
</tr>
</table>

## 参见<a name="see-also"></a>
* [版本发布说明](../../release-notes.md)
* [Nexus mod](https://www.nexusmods.com/stardewvalley/mods/1915)
* [更多帮助](https://stardewvalleywiki.com/Modding:Help)
