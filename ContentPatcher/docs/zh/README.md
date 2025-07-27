**Content Patcher** 是[星露谷物语](http://stardewvalley.net/)的一款模组，用于加载内容包并以不更换游戏原有文件的形式更改游戏数据、贴图和地图。

**🌐 其他语言：[en (English)](../README.md)。**

## 目录
* [玩家指南](#for-players)
  * [安装](#install)
  * [兼容性](#compatibility)
  * [内容包设置](#configure-content-packs)
  * [多人](#multiplayer)
* [模组作者指南](#for-mod-authors)
* [配置](#configure)
* [参见](#see-also)

## 玩家指南<a name="for-players"></a>
### 安装<a name="install"></a>
1. 安装最新版本的 [SMAPI](https://smapi.io/)；
2. 从 [Nexus Mods](https://www.nexusmods.com/stardewvalley/mods/1915) 安装此模组；
3. 将任意 Content Patcher 内容包解压并放入 `Mods` 文件夹中以进行安装。
4. 使用 SMAPI 运行游戏。

完成这些步骤后，`Mods` 文件夹中的内容包将被自动加载并应用。

### 兼容性<a name="compatibility"></a>
Content Patcher 兼容 Stardew Valley 1.6+ 版本，在 Linux/macOS/Windows 均可使用，支持单人游戏和多人游戏。

### 内容包配置<a name="configure-content-packs"></a>
许多内容包可以使用 `config.json` 文件进行配置，Content Patcher 将在您安装了该内容包后首次启动游戏时创建该文件。（若没有出现 `config.json`，则代表该模组不提供配置选项。）

如果您安装了 [Generic Mod Config Menu](https://www.nexusmods.com/stardewvalley/mods/5098)，Content Patcher 会自动将可设置的内容包添加到其游戏内菜单中：

![](../screenshots/config-with-sections.png)

### 多人<a name="multiplayer"></a>
Content Patcher 兼容多人游戏。理想条件下所有玩家都应当安装有相同的内容包，但这不是必须的。如果某些玩家没有安装内容包，则会出现以下现象:

更改类型    | 现象
--------- | ------
贴图       | 只有安装了该内容包的玩家才能看到贴图更改
地图       | 只有安装了该内容包的玩家才能看到地图更改。没有自定义地图的玩家将会看到原版地图，并且会受到正常界限的限制（例如，他们可能会看到其他玩家穿过墙壁，但他们无法跟随）。
数据       | 仅直接影响已安装的玩家，但可能间接影响其他玩家。例如，当某个内容包更改了 `Data/Objects` 并生成一个物品, 即使其他玩家的 `Data/Objects` 没有这些更改，他们也会看到该物品的自定义值。

## 模组作者指南<a name="for-mod-authors"></a>
* 要创建内容包，请参阅[模组作者指南](author-guide.md)及其[Tokens 子页面](author-guide/tokens.md)；
* 要在 SMAPI 模组中添加自定义 Content Patcher Tokens，请参阅[扩展性 API](extensibility.md)；
* 要在 SMAPI 模组中调用 Content Patcher Conditions 和 Token strings，请参阅[条件 API](conditions-api.md)和[token string API](token-strings-api.md)。

## 配置<a name="configure"></a>
Content Patcher 在首次运行时会在模组文件夹中创建一个 `config.json` 文件。您可以用文本编辑器中打开该文件来配置本模组。

可更改以下设置：

<table>
<tr>
  <th>配置</th>
  <th>效果</th>
</tr>

<tr>
  <td><code>EnableDebugFeatures</code></td>
  <td>

默认为 `false`（否）。是否启用专为内容包模组作者设计的[调试功能](author-guide/troubleshooting.md#debug-mode)。

  </td>
</tr>

<tr>
  <td><code>Controls</code></td>
  <td>

设置的手柄、键盘和鼠标按键绑定（参见[按键绑定](https://zh.stardewvalleywiki.com/模组:使用指南/按键绑定)）。默认的按键绑定为：

* 按 `F3` 显示[调试模式](author-guide/troubleshooting.md#debug-mode)（需启用调试功能）；
* 按 `LeftControl` 和 `RightControl` 切换调试模式中的贴图。

可以用逗号分隔绑定按键（例如 `B,LeftShoulder` 将同时绑定 `B` 和 `LeftShoulder`），也可使用加号设置多键绑定（例如`LeftShift + B`）。

  </td>
</tr>
</table>

## 参见<a name="see-also"></a>
* [版本发布说明](../release-notes.md)
* [Nexus mod](https://www.nexusmods.com/stardewvalley/mods/1915)
* [更多帮助](https://zh.stardewvalleywiki.com/模组:帮助)
