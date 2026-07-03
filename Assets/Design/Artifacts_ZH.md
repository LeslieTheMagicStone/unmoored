# 机关说明 — 关卡设计参考

每个机关是独立组件。只需将脚本拖到带 Collider2D 的 GameObject 上即可。

---

## 门 + 踩踏开关

**机制：** 玩家踩在开关上 → 门瞬间打开。所有玩家离开 → 门瞬间关闭。DOTween 动画，开合即发。

**配置：**
- 门：BoxCollider2D（实体） + Gate.cs。设置 `openHeight`、`duration`。
- 开关：BoxCollider2D（IsTrigger=true） + PressureSwitch.cs。将门拖入 `targetGate`。

**设计提示：** 门关闭时压到着地玩家会致死。把开关放在只有一名玩家能碰到的位置——另一人必须在开关松开前通过。

---

## 碎裂平台

**机制：** 玩家踩上顶面 → 平台摇晃 `holdDuration` 时长，然后碎裂消失。`respawnDelay` 秒后重生。

**配置：** BoxCollider2D（实体） + CrumblingPlatform.cs。可选：将子 SpriteRenderer 的 Transform 拖入 `visualTransform`。

**设计提示：** 逼迫玩家快速行动。可故意触发堵路，另一人必须赶在消失前通过。设为 `oneShot = true` 可做一次性跑道。

---

## 移动平台

**机制：** 在路径点间循环巡逻。任意玩家在 `anchorZone` 内锚定即暂停移动，全部释放后继续。

**配置：** BoxCollider2D（实体） + MovingPlatform.cs + 一个子 trigger collider 作为 `anchorZone`。创建若干空 GameObject 作为路径点，拖入 `waypoints` 数组（最少 2 个）。

**设计提示：** 经典"一人锚住，另一人搭车"谜题。也可以反过来：必须全部松锚平台才能带你穿越危险区。

---

## 滑面

**机制：** 锚定在侧面的玩家缓慢下滑。站在顶面不受影响。

**配置：** BoxCollider2D（实体） + SlipperySurface.cs。调节 `slideSpeed`。

**设计提示：** 放在垂直墙上阻止无限挂锚。玩家必须在滑落前规划下一步。结合移动平台制造时间压力。

---

## 弹跳表面

**机制：** 玩家落在顶面 → 向上弹飞。冷却时间防止无限弹跳。视觉有挤压/拉伸动画。

**进阶技巧：** 接触弹面的瞬间按下跳跃键，弹力会与跳跃力叠加，跳到远超正常的高度。这是判定机制的自然结果而非 bug，建议保留并作为可掌握的技巧向玩家传达。

**配置：** BoxCollider2D（实体） + BounceSurface.cs。调节 `bounceForce`。可选 `visualTransform` 做动画。

**设计提示：** 放在缺口下方提供额外高度。"超级跳"可用作隐藏路径的入口条件——只有掌握了踩跳时机的玩家才够得到。多个串联可做节奏跑酷。

---

## 风区

**机制：** 玩家进入触发区域后被 `windForce` 持续推动。白蓝色粒子沿风向飘过。

**配置：** BoxCollider2D（IsTrigger=true） + WindZone.cs。调节 `windForce`（方向+强度）、`particleRate`、`particleLifetime`。

**设计提示：** 用来偏转摆荡弧线、把玩家推下窄平台、或抬高到跳不到的高度。与滑面搭配——风推水平、滑面拉垂直，形成不可逆路径。

---

## 存档点

**机制：** 玩家通过触发器 → 存档点记录。激活后变绿。死亡从此处重生。

**配置：** BoxCollider2D（IsTrigger=true） + SpriteRenderer + Checkpoint.cs。

---

## 死亡区域

**机制：** 任何玩家进入触发器立即死亡。

**配置：** BoxCollider2D（IsTrigger=true） + DeathZone.cs。拉伸至关卡底部全覆盖。

---

## 胜利区域

**机制：** 两名玩家都进入触发器即通关。

**配置：** BoxCollider2D（IsTrigger=true） + WinZone.cs。

---

### 速查表

| 机关 | 触发方式 | 效果 | 关键参数 |
|------|---------|------|---------|
| 门+开关 | 玩家踩开关 | 门开/关 | `duration` |
| 碎裂平台 | 踩上 / 锚侧面 | 摇晃、碎裂、重生 | `holdDuration` |
| 移动平台 | 玩家在锚定区内 | 巡逻暂停 | `moveSpeed` |
| 滑面 | 锚定在侧面 | 缓慢下滑 | `slideSpeed` |
| 弹跳表面 | 落在顶面 | 向上弹飞 | `bounceForce` |
| 风区 | 进入触发器 | 持续推力+粒子 | `windForce` |
| 存档点 | 穿过触发器 | 保存重生点 | — |
| 死亡区域 | 进入触发器 | 死亡 | — |
| 胜利区域 | 两人都进入 | 通关 | — |
