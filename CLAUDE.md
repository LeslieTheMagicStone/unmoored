# CLAUDE.md — CGJ2026

## 环境

- Unity 2022.3.22f1 LTS，URP 14.0.10
- macOS 25.3.0（darwin）
- DOTween (免费版, Plugins/Demigiant/DOTween)
- TextMeshPro (3.0.6 内置)
- Cinemachine (需自行导入 Package Manager)
- FMOD (Plugins/FMOD)

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
ScoreChanged  PlayerDied  LevelComplete  GameRestart  EnemyKilled  PickupCollected
```

### 模块化原则

- 每个类一个职责。PlayerController 只管输入和移动，HealthSystem 只管血量，ScoreManager 只管分数。
- 跨系统通信用 EventBus。A 调 B 的方法是直接耦合——A 发事件、B 自己订阅，才是解耦的。
- 数据和逻辑分离。参数放 ScriptableObject (`Scripts/Config/`)，状态放纯数据类，逻辑留在 MonoBehaviour。
- 纯 C# 逻辑（算法、状态机、数据转换）不继承 MonoBehaviour，独立成 class。面试官真正会读的就是这些。
- MonoBehaviour 在 `OnEnable` 订阅事件、`OnDisable` 取消订阅。不要漏掉取消。
- Prefab 的 Inspector 暴露接口用 `[SerializeField]` + 清晰的变量名。

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

## AI 使用策略

- 写骨架/模板/系统设计 → 问 AI
- 写简单 Lerp/赋值/单行逻辑 → 自己写
- 解不开的 bug → 把相关代码文件路径贴给 AI
- 新会话开头贴本文档路径，即可获取完整项目上下文
