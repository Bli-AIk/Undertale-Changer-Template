# UCT更新计划

本文档所写的是UCT长期的更新计划。

注意，这些更新计划并没有任何排列顺序，且有可能变动、搁置或废弃。

关于UCT的长期更新计划，请查阅[PLAN.md](PLAN.md)、[PLAN_zh-CN.md](PLAN_zh-CN.md)或[PLAN_zh-TW.md](PLAN_zh-TW.md)。

关于UCT的具体更新内容，请查阅[CHANGELOG.md](CHANGELOG.md)、[CHANGELOG_zh-CN.md](CHANGELOG_zh-CN.md)或[CHANGELOG_zh-TW.md](CHANGELOG_zh-TW.md)。

简体中文的更新计划/更新日志优先更新，其他语言会在**版本号变更**时使用GPT进行翻译。

## 目录

[通用类](#通用类)：包含对模板内基础/通用功能的改进计划。

[OW 类](#OW 类)：包含对 Overworld 场景涉及的各系统的重做计划。

[战斗类](#战斗类)：包含对战斗系统的拓展和重构计划。

[新增类](#新增类)：包含对新增场景、功能的计划。

[其他类](#其他类)：包含以上四类之外的计划。

[搁置类](#搁置类)：包含一些曾被计划，但因种种原因现已搁置的计划。

## 通用类
### 现有系统细化
- [x] 细化BGM 控制器[^1]
- [x] 使用Builder重构物品系统


### UI优化与细化
- [ ] 添加低分辨率放大显示功能[^2]
- [ ] 分离 MENU 界面、设置界面 与 OW 存档界面 的 UI 绘制逻辑与具体执行逻辑
- [ ] 细化 MENU 界面、设置界面 与 OW 存档界面 功能

### 性能优化
- [x] 添加DebugLogger脚本[^3]
- [ ] 使用协程调用语言包，而非同时全部调用

## OW 类
### 事件系统
- [x] 重做 OW 事件系统
- [ ] 添加 Dr 风格的追逐战系统
- [ ] 将事件系统数据存档

### 剧情系统
- [x] 丰富对话系统[^4]
- [ ] 添加所有文本的校对场景[^5]

## 战斗类
### 数据存储
- [x] 存储弹幕数据至 ScriptableObject
- [ ] 读取Json回合数据

### 回合系统重做
- [ ] 添加可视化回合系统编辑器（另设项目），生成Json回合数据
- [ ] 添加路径弹幕生成器

### 战斗系统扩展
- [ ] 添加在回合中插入字幕 / 对话的功能
- [ ] 添加多战斗[^6]

### 修复与优化
- [x] 优化3D 背景
- [x] 修复蓝心

### 异形框
- [ ] 修复异形框
- [x] 重做碰撞系统以适配异形框

### 判定与碰撞
- [x] 添加怪物死亡判定
- [x] 补全玩家魂色


## 新增类
### 场景拓展
- [x] 添加起名场景中的多语言拓展
- [ ] 添加战斗结算场景
- [x] 添加BGM场景
- [ ] 添加制作人员名单/致谢场景
- [ ] 添加商店场景

### 渲染拓展
- [x] 添加内置投影框
- [x] 添加3D 渲染器

### 功能扩展
- [ ] 添加midi检测系统
- [ ] 添加历史文本系统
- [ ] 添加存储系统/额外背包
- [ ] 添加CC字幕系统[^7]
- [ ] 添加成就系统
- [ ] 添加Buff系统[^8]
- [ ] 添加 UI 管理器
- [x] 添加武器/防具的自定义拓展支持
- [ ] 读取原作存档数据

### 无障碍
- [ ] 添加讲述人
- [ ] 添加色盲滤镜

## 其他类
### 项目本地化
- [x] 合并中英文分支
- [ ] 通过脚本替换注释文本

### 富文本

- [ ] 添加富文本TAG - 自定义可选译名（用于人名选择性翻译等）
- [ ] 添加富文本\<waitForConfirm\> - 暂停，按Z继续但不清屏
- [ ] 添加富文本\<directUpdate\> - 不按Z直接清屏并播放

### 清理与维护
- [x] 删除多余的原作贴图

### 教程
- [ ] 新的wiki与视频教程



## 搁置类

- [ ] 添加像素完美后处理
- [ ] 引入联机库
- [ ] 添加多主角团队系统

---

[^1]: 主要会为它添加计算节拍数等功能。
[^2]: 会尝试在设置选项中额外添加一项，允许将游戏画面设为低分辨率的同时，对显示分辨率进行放大处理。这对性能较差的电脑比较友好。
[^3]: 该脚本会将所有 Unity 的 Debug 相关函数封装至 其中来调用，这些函数仅在编辑器内执行。
[^4]: 通过接入Ink语言而实现。
[^5]: 该场景内会列出所有文本，与游戏内显示方式一致，便于快速校对。
[^6]: 即允许游戏内多次战斗和多个战斗。
[^7]: 本地化人员可以在翻译完成但未正式嵌入的阶段，通过发布CC字幕预览版本，提前为玩家提供本地化支持。
[^8]: KR系统通过Buff实现。

