# Anchor — Brainstorm

## 14 Concept Seeds

### A. Anchor & Drift — 资源管理 roguelite
- **Layer**: ②机制 + ③情感（安全感vs风险）
- **Hook**: 投锚→探索→在资源耗尽前返回锚→升级→种更远
- **Feasibility**: 中（噪声地图+方块拼，核心循环简单）
- **Art**: 低（几何图形即可）
- **Emotion**: 锚是唯一的安全感。每次离开都是赌博。

### B. Weight of the Chain — 解谜平台
- **Layer**: ①物理 + ③情感（包袱与放下的选择）
- **Hook**: 拖着一只沉重的锚。有些路必须放下锚才能通过，但后面你会需要它。
- **Feasibility**: 高（平台移动+可抛可捡物体，tilemap 做关卡）
- **Art**: 极低（方块+三角，灰调）
- **Emotion**: 锚是你最大的负担，也是你唯一的工具。

### C. You Are My Anchor — 双角色绑定动作
- **Layer**: ②机制（拴缚）+ ③情感（羁绊）
- **Hook**: 双角色被绳子连在一起。水上风筝+水下输出，绳子长度是硬约束。
- **Feasibility**: 中低（双角色控制手感调试吃时间）
- **Art**: 中（双角色+敌人+水面 shader）
- **Emotion**: 不能太远也不能太近，像一段关系。

### D. Anchor Point — 第一人称抓钩跑酷
- **Layer**: ①物理锚
- **Hook**: 射锚钩墙，摆荡+拉拽移动。Verlet 摆动物理。速通导向。
- **Feasibility**: 中（3D 物理调试需要时间，但核心只是一根绳）
- **Art**: 低（低模风格，灰盒关卡）
- **Emotion**: 纯粹的流动感。锚不是负担，是翅膀。

### E. News Anchor — 文字/叙事
- **Layer**: ①字面（双关）+ ③情感（社会锚）
- **Hook**: 你是末日中的新闻主播。保持冷静、持续播报。你的声音是混乱中唯一的"锚"。
- **Feasibility**: 高（纯 UI/文字，零 gameplay 系统）
- **Art**: 极低（一张主播台背景+文字 UI）
- **Emotion**: 你是社会的锚，但谁来做你的锚？

### F. Anchor Memories — 记忆锚点步行模拟
- **Layer**: ①物理锚（物件）+ ③情感（记忆）
- **Hook**: 空间中物件触发闪回。非线​​性拼凑故事。Gone Home 式。
- **Feasibility**: 中（需要写好叙事+关卡设计来引导）
- **Art**: 中（场景需要氛围感）
- **Emotion**: 那个你放不下的记忆，就是你灵魂的锚。

### G. Drag — 锚之竞速
- **Layer**: ①物理锚
- **Hook**: 每辆赛车拖着一只锚。锚让你慢，但能用来漂移、砸对手、钩翻敌人。终点前割绳冲刺。
- **Feasibility**: 中（物理+AI+赛道设计）
- **Art**: 中（车辆+锚模型+赛道）
- **Emotion**: 什么时候割掉你的负担，换取最后的速度？

### H. Safe Harbor — 浮城建造/塔防
- **Layer**: ①物理 + ②机制（锚=安全区）
- **Hook**: 建造浮城。锚把你拴住。起锚可以移动，但移动时脆弱。锚链断=game over。
- **Feasibility**: 中低（建造+塔防两套系统）
- **Art**: 中（浮城+水面+敌人）
- **Emotion**: 有些东西你必须守住，因为锚链的另一端是你的一切。

### I. Anchorless — 生存恐怖
- **Layer**: ③情感（失去锚的恐惧）
- **Hook**: 你是正在消散的幽灵。找物件/人/记忆暂时锚定自己。每个锚会慢慢腐化。
- **Feasibility**: 中（氛围+计时+资源管理）
- **Art**: 中（昏暗场景+粒子消散效果）
- **Emotion**: 被遗忘的恐惧。你在一点点消失。

### J. Ball & Chain — 双人合作物理
- **Layer**: ①物理 + ②机制（共享拴缚）
- **Hook**: 两个玩家分享一个锚。锚是钟摆、是桥、是配重。配合是唯一的出路。
- **Feasibility**: 中低（需要两台手柄+物理调试）
- **Art**: 低（几何图形双角色+锚）
- **Emotion**: 我们被同一个"锚"拴在一起，但只有一起用力才能前进。

### K. Anchor Drop — 下落式音游
- **Layer**: ①物理 + ②机制（锚=节拍）
- **Hook**: 在节拍上抛锚。锚=重拍。不同锚种=不同节奏型。简约手机音游。
- **Feasibility**: 高（谱面+判定+视觉反馈）
- **Art**: 极低（纯 UI，几何动画）
- **Emotion**: 锚是节奏本身。在混沌中找到你的节拍。

### L. Anchor Rancher — 太空鲸鱼牧场
- **Layer**: ①物理 + ③情感（锚=温柔的羁绊）
- **Hook**: 用锚拴住太空鲸鱼，不然它们飘走。星露谷×太空。治愈系。
- **Feasibility**: 中（AI 行为+收集+升级循环）
- **Art**: 中（可爱的鲸鱼+太空场景）
- **Emotion**: 锚不是束缚，是"我不想让你走"的温柔。

### M. Sea of Anchors — 网格战术策略
- **Layer**: ②机制（锚=安全区=领土）
- **Hook**: 放置锚创建安全区。锚内安全，锚外危险。敌人能摧毁锚。建立安全网络。
- **Feasibility**: 中（网格逻辑+AI+关卡设计）
- **Art**: 低（网格+图标+简单的船/锚模型）
- **Emotion**: 每一只锚都是一句"这里安全了"。但安全从来不是永久的。

### N. Cut the Anchor — 情感叙事对话
- **Layer**: ③情感（锚=有毒关系）
- **Hook**: 与一个你需要离开的人对话。每个选项加强或削弱"锚"。游戏在你割断锚时结束——或者永远不割。
- **Feasibility**: 高（纯对话树+情绪状态机）
- **Art**: 极低（两个角色立绘/剪影+文字 UI）
- **Emotion**: 有些锚是你自己选择戴上的。你能松开手吗？

---

## Quick Comparison

| # | Name | Layer | 48h Risk | Art Need | Core Emotion |
|---|------|-------|----------|----------|--------------|
| A | Anchor & Drift | ②+③ | 中 | 低 | 安全感 vs 风险 |
| B | Weight of the Chain | ①+③ | **低** | **极低** | 负担即工具 |
| C | You Are My Anchor | ②+③ | 中低 | 中 | 羁绊与距离 |
| D | Anchor Point | ① | 中 | 低 | 流动与自由 |
| E | News Anchor | ①+③ | **低** | **极低** | 社会支柱 |
| F | Anchor Memories | ①+③ | 中 | 中 | 放不下的记忆 |
| G | Drag | ① | 中 | 中 | 负担的赌博 |
| H | Safe Harbor | ①+② | 中低 | 中 | 守护与脆弱 |
| I | Anchorless | ③ | 中 | 中 | 消散的恐惧 |
| J | Ball & Chain | ①+② | 中低 | 低 | 共享的羁绊 |
| K | Anchor Drop | ①+② | **低** | **极低** | 节拍中的秩序 |
| L | Anchor Rancher | ①+③ | 中 | 中 | 温柔的牵挂 |
| M | Sea of Anchors | ② | 中 | 低 | 安全不是永久的 |
| N | Cut the Anchor | ③ | **低** | **极低** | 放手的勇气 |

## My Top Picks by Team Type

- **三人都会写码、缺美术**: B (Weight of the Chain) — 方块就能做，puzzle design 是重心
- **有会写 narrative 的人**: N (Cut the Anchor) — 48h 够写一段让人哭的对话
- **有做过音游的**: K (Anchor Drop) — 技术风险极低，juice 空间巨大
- **想赢 innovation 分**: A (Anchor & Drift) — 系统驱动，别人想不到
- **想拿 feel/juice 分**: D (Anchor Point) — 摆荡手感好的 gif 本身就是广告
- **三个人都是二次元**: C (You Are My Anchor) — "不能被绳子分开的两个人"太好画了
