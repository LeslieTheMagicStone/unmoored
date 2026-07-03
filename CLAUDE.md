# CLAUDE.md — CGJ2026

## 环境

- Unity 2022.3.22f1 LTS，URP 14.0.10
- macOS 25.3.0（darwin）
- DOTween (免费版, Plugins/Demigiant/DOTween)
- TextMeshPro (3.0.6 内置)
- Cinemachine (2.9.7, Package Manager)
- Input System (1.7.0, Package Manager)

## 项目结构

```
Assets/
├── Scenes/          # 场景文件
├── Scripts/         # 所有 C# 脚本
├── Prefabs/         # 预制体（后续手动创建）
├── Art/             # 美术资产（后续手动创建）
├── Audio/           # 音效（后续手动创建）
├── Resources/       # 动态加载资源
├── Settings/        # URP Pipeline Asset / Volume Profile
├── Plugins/         # DOTween, FMOD
├── TextMesh Pro/    # TMP 资产
└── TutorialInfo/    # 模板自带，可删
```

## 代码约定

这个项目的代码也是你程序开发实习的作品集素材——写干净、写模块化、写值得给面试官看的东西。不要因为是 jam 就写成屎山。

- 命名空间：`CGJ2026`
- `[SerializeField]` 暴露给 Inspector，不用 `public` 字段
- 禁止在 Awake/Start 中依赖其他脚本的初始化顺序
- 核心循环逻辑放 Update，配置数据放 ScriptableObject
- 单例模式：`GameManager.Instance` 用 `Awake()` + `DontDestroyOnLoad`
- 不要写 region
- 所有注释用英文写，遵循国际规范。不用中文注释
- 不要沉默 fallback——null 引用、缺 key、缺资源直接抛异常，宁可 crash 也不要静默吞掉

## 架构骨架

### EventBus —— 全局事件系统

`Scripts/EventBus.cs`：静态类，Dictionary 存 `Action<object>`。`On(id, handler)` 注册、`Off(id)` 移除、`Emit(id, data)` 发送。一个 key 只存一个 handler（jam 够用，需要多个时改用 `event`）。

`Scripts/E.cs`：事件 ID 常量类，所有跨系统事件名集中在这里：

```
ScoreChanged  LevelComplete  GameRestart
```

### 模块化原则

- 每个类一个职责。PlayerController 只管输入和移动，HealthSystem 只管血量，ScoreManager 只管分数。
- 跨系统通信用 EventBus。A 调 B 的方法是直接耦合——A 发事件、B 自己订阅，才是解耦的。
- 数据和逻辑分离。参数放 ScriptableObject (`Scripts/Config/`)，状态放纯数据类，逻辑留在 MonoBehaviour。
- 纯 C# 逻辑（算法、状态机、数据转换）不继承 MonoBehaviour，独立成 class。面试官真正会读的就是这些。
- MonoBehaviour 在 `OnEnable` 订阅事件、`OnDisable` 取消订阅。不要漏掉取消。
- Prefab 的 Inspector 暴露接口用 `[SerializeField]` + 清晰的变量名。

## 游戏概念

2D 垂直爬塔游戏，双人合作（本地同屏），两人用一根绳子相连。

方向：玩家向上爬，地图整体向下滚动（类似 Getting Over It 的方向感）。

**核心循环**

```
A 变锚 → B 借力摆荡上去 → B 变锚 → A 释放锚、荡上去 → 重复
```

**核心机制：锚/人形态切换（精确定义）**

- **触发方式**：每个玩家有独立的锚化键，按一下 Toggle（按下即切换，非长按）。空中也可以变锚，但锚形态下受重力影响会下落——必须在平台边缘/可挂点处锚定才有意义。
- **锚形态**：完全不能移动、不能跳跃。自身成为队友摆荡的支点。可随时解除锚定恢复人形态。程序上变为 Kinematic Rigidbody。
- **人形态**：正常移动、跳跃。靠近锚点时无法替队友解除锚——只有锚定者本人能解除。
- **绳子**：固定最大长度，不可主动伸缩。双方距离达到绳长时拉紧，无法继续远离。摆荡时绳长保持不变。
- **两人同时锚定**：允许。两人分别固定在不同位置，绳子拉紧，游戏暂停。至少一人解除才能继续。不会死锁——也可以作为某些机关的解法。

**死亡与重生**

- 掉落到底部边界 / 碰到敌人 / 被地图带到最底部 → 死亡
- 死亡后扣除生命值，从掉落位置上空重新生成往下落

**战斗（简版）**

- 摆荡过程中用身体撞击敌人可将其撞飞
- 从上方踩敌人头顶可踩死
- 无复杂战斗系统，保持核心在移动和锚点策略

**待定问题**

- 是否需要二段跳？（暂不实现，等核心循环确定后再判断）
- 跳跃高度是否应达到绳长？（倾向：略低于绳长，保证必须靠摆荡才能跨大距离）

**核心承诺**

> 我们的游戏是**一根绳两根命**。两个人通过一条绳子和锚定来交替攀爬高塔。我们想让玩家感受到策略默契与互相托付的紧张感。

**参考作品**

| 游戏 | 连接方式 | 锚点机制 | 与本作差异 |
|------|---------|---------|-----------|
| Bread & Fred | 绳子拴住 | 体重+物理当锚 | 无形态切换 |
| Unravel Two | 毛线连接 | 绕环境物体当锚 | 锚在环境，不在角色 |
| Chained Together | 铁链 | 无锚点 | 纯链条物理拉扯 |
| PEAK | 绳索+攀登钉 | 布设固定锚点 | 锚是装备，不是人 |

**差异化亮点**：现有作品都没有"角色本身主动变为锚"这一机制。

## 关卡设计（三层塔）

### 第一层：教学 — 认识锚

目的：玩家自然理解"锚不是让自己过去，是帮队友过去"。

```
────────── 高台（终点）
          □
      空缺（单人跳不过去）
────────── 起点
```

流程：A 跳到平台边缘 → A 变锚 → B 借 A 摆荡过去 → B 到达高台 → A 解除锚继续前进。

### 第二层：风险 — 错误的锚点

目的：理解"锚点位置决定成败"。

```
────────── □（正确锚点，能荡到终点）
────□─────（错误锚点，挂太低荡不过去）
  尖刺区
──────────
```

新增：多个可挂边缘（诱导错误选择）、尖刺区（摆荡角度太低时撞刺）。

### 第三层：高潮 — 快速交替锚

目的：需要连续快速轮流锚才可通过。

```
■■■■■■ 移动平台→
        ←移动平台
■■■■■■
```

流程：A 挂 → B 荡 → B 立即挂另一侧 → A 解除 → A 荡 → 循环。加入巡逻敌人，摆荡时可用身体撞飞。

### 存档/重生点

- 每层起始位置设重生点
- 掉落到底部或死亡后从当前层重生点重新落下

## 双语配置

所有游戏内文本通过 XML 表格管理，位于 `Assets/Resources/Localization.xml`。

格式如下，每条一行 `<string>`：

```xml
<?xml version="1.0" encoding="UTF-8"?>
<strings>
  <string key="mainmenu_play">Play / 开始游戏</string>
  <string key="mainmenu_title">My Game / 我的游戏</string>
  <string key="gameover_score">Score: {0} / 得分：{0}</string>
  <string key="gameover_retry">Retry / 重试</string>
</strings>
```

读取方式：`LocalizationManager` 单例在 `Awake` 时加载 XML，提供 `Get(string key, params object[] args)` 方法。key 不存在时抛出 `KeyNotFoundException`，禁止沉默 fallback。

规则：
- key 用 snake_case 英文，见名知义
- 中英文用 ` / ` （空格斜杠空格）分隔
- `{0}` `{1}` 做可替换参数，两种语言共用同一占位符
- 新增文本必须加到这个 XML，禁止硬编码字符串到场景/脚本里

## 场景流

MainMenu → Game → GameOver（三场景闭环）

## 开发约束

- 48h Game Jam，3人队（程序/策划/美术）
- 程序不阻塞等美术资产——用方块/圆圈做 placeholder
- 策划在 12h 后只许砍不许加功能
- 每完成一个功能 commit 一次
- 40h 内容锁死，之后只做 polish + build

## Jam 时间线

```
开始: 2026-07-03 15:00 CST (+08:00)
结束: 2026-07-05 14:00 CST (+08:00)
总时长: 47h
```

| 阶段 | 时间窗 | 对应现实时间 | 目标 | 死线 |
|------|--------|-------------|------|------|
| 1. 审题 & 头脑风暴 | 0h–2h | 7/3 15:00–17:00 | 获主题，三层解读，锁定1个方向 | 17:00 |
| 2. 纸上原型 & 分工 | 2h–6h | 7/3 17:00–21:00 | 核心循环纸上跑通，美术出风格指南，程序搭事件+骨架 | 21:00 |
| 3. 垂直接片 | 6h–11h | 7/3 21:00–7/4 02:00 | 核心循环在引擎可玩，placeholder 完整闭环 | 02:00 |
| 4. 内容铺量 | 11h–23h | 7/4 02:00–14:00 | 关卡/内容批量产出，手感迭代，美术批量出资产 | |
| 5. 强制休息 | 23h–29h | 7/4 14:00–20:00 | 睡觉 4–6h，醒后 review polish list | 20:00前起床 |
| 6. 内容收尾 | 29h–39h | 7/4 20:00–7/5 06:00 | 所有内容就位，不再加新东西 | 06:00 锁死 |
| 7. Juice & Polish | 39h–46h | 7/5 06:00–13:00 | 粒子/震屏/音效/UI动画。出第一个 build 验证 | |
| 8. 打包 & 提交 | 46h–47h | 7/5 13:00–14:00 | 录 gif，写描述，上传 | 14:00 提交 |

### 每条新对话开始时，请计算当前阶段并提醒我

请根据上述开始时间（2026-07-03 14:00 CST）和当前对话的 system date 计算已过小时数，告诉我：
1. 当前处于哪个阶段（1–8）
2. 离下一个死线还有多久（如果有）
3. 该阶段的一个关键提醒（一行）

格式示例：
> ⏰ **Jam 第 8h — 阶段 3: 垂直接片** | 离死线还有 4h
> 核心循环必须在引擎里跑起来。用方块圆圈。不要让美术阻塞你。

## 配置文件

- `.editorconfig`：缩进/换行规范，IDE 自动读取
- `.gitignore`：Unity 标准模板，Library/Temp/Builds 不入库
- `Force Text` serialization 已开（`m_SerializationMode: 2`），`.prefab` 和 `.unity` 是 YAML，Claude Code 可直接读写
- Input System 设为 `Both`（新旧 API 共存，`Input.GetKeyDown` 和 `InputAction` 都能用）

## AI 使用策略

- 写骨架/模板/系统设计 → 问 AI
- 写简单 Lerp/赋值/单行逻辑 → 自己写
- 解不开的 bug → 把相关代码文件路径贴给 AI
- 新会话开头贴本文档路径，即可获取完整项目上下文
