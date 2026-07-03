# Level Blockout Spec

For the level designer to build independently without branching from the programmer's scene.

## Physics & Movement

| Property | Value | Notes |
|----------|-------|-------|
| Gravity scale | 10 | High — fast falling |
| Move speed | 5 | Horizontal on ground |
| Jump force | 30 | Impulse, ~4.5 units high |
| Rope max length | 6 | In scene (RopeSystem), NOT from PlayerConfig |
| Swing force | 12 | Tangent push when rope taut + airborne |
| Human mass | 1 | |
| Anchor mass | 1000 | Effectively immovable |

**Jump reach**: ~4.5 units vertical, ~2.5 units horizontal with running start.
**Rope span**: max 6 units between players. Solo jump gap = ~2.5. Rope-assisted gap = up to ~6 (one anchors at edge, other swings).

## Layer 8 = Ground

All static platforms MUST be on layer 8. Player's ground check and wall check use this layer.
Set `m_Layer: 8` on the GameObject.

## Required Scene Objects

Create a new scene, add these in order:

### 1. Main Camera
```
- Orthographic, size 6
- Position: (0, 1, -10)
- BG color: dark (0.1, 0.12, 0.18)
```

### 2. Platforms (Ground)
Each platform is one GameObject with 3 components:
```
- SpriteRenderer: any color, DrawMode=Tiled, Size=(1,1)
- BoxCollider2D: size matches platform dimensions
- Transform: position + scale defines the platform
```

Scale X stretches the platform. Example: `scale=(8, 0.5, 1)` = 8 wide, 0.5 tall.
Use distinct colors per platform type (start=gray, mid=brown, goal=green).

### 3. Players (2x)
Each player:
```
- SpriteRenderer: human color, DrawMode=Simple
- Rigidbody2D: Dynamic, mass=1, gravity=10, freeze rotation
- BoxCollider2D
- PlayerController script (guid: 4aca1e9a703b8413db949dc8565023c2)
  - Player 0 config: WASD + Space + E, playerIndex=0
  - Player 1 config: Arrows + Up/Down, playerIndex=1
  - Both: assign PlayerConfig asset, set groundLayer=8
- Child GameObject "GroundCheck": empty, placed at player's feet center
```
Assign the GroundCheck transform to each player's `groundCheck` field.

### 4. RopeSystem
```
- Empty GameObject
- LineRenderer: material=Sprites-Default, color=gold (0.8, 0.65, 0.2), width=0.06
- RopeSystem script (guid: d5f33e2479b444da4abb64b7e1f127e7)
  - playerA: Player0
  - playerB: Player1
  - maxLength: 6
```

### 5. DeathZone
```
- Empty GameObject at bottom of play area
- BoxCollider2D: IsTrigger=true, very wide (cover full fall zone)
- DeathZone script (guid: 871f85b8a9d4d4c1cb49f6fca1ce6135)
```

### 6. WinZone
```
- Empty GameObject at level goal
- BoxCollider2D: IsTrigger=true
- WinZone script (guid: 80e109714eeca44d1986c2d127633737)
```

### 7. Checkpoints
```
- Empty GameObject at respawn point
- Collider2D: IsTrigger=true
- Checkpoint script (guid: ed9339262696947d0a165752fe572ae1)
```

Place at least one at the start area.

### 8. LevelManager
```
- Empty GameObject
- LevelManager script (guid: 58e11529e6e774b5880c6ee455da0ee5)
  - playerA: Player0 transform
  - playerB: Player1 transform
  - winZone: WinZone transform
  - config: PlayerConfig asset
```

### 9. CameraSetup (for runtime CM wiring)
```
Attach to LevelManager GameObject:
- CameraSetup script (guid: c7b8d9e0f1a2b3c4d5e6f7a8b9c0d1e2)
  - playerA: Player0 transform
  - playerB: Player1 transform
```

### 10. HUD (for lives display)
```
Also attach to LevelManager GameObject:
- HUD script (guid: a3e4f5d6c7b8a9e0f1d2c3b4e5f6a7b8)
```

### 11. Enemy (optional)
```
- SpriteRenderer (red)
- Rigidbody2D: Dynamic, mass=2, freeze rotation
- Collider2D (not trigger)
- Enemy script (guid: b2c0f8fa3559e41ed9a5c9dce902c8d9)
```

## Level 1 Layout Reference (current game scene)

```
HighPlatform  (4, 1.5)  scale 4x0.5  green  — GOAL
               |
               |  gap ~3 units (must rope-swing)
               |
MidPlatform   (-1, -1.5) scale 3x0.5  brown  — anchor point
               |
               |  gap ~2.5 units
               |
Ground        (-7.3, -4.37) scale 10.7x1  gray  — START
Ground(1)     (10.56, -4.37) scale 10.7x1  gray

Players start at (-5, -2.5) and (-3, -2.5) on Ground
WinZone at (4, 3) above HighPlatform
DeathZone at (0, -8)
```

## Design Rules

- Gaps ≤2.5 units = solo jumpable
- Gaps 2.5–5 units = need one player to anchor, other swings
- Gaps >5 units = not passable (rope max is 6)
- Don't put platforms >6 units apart vertically (can't reach)
- DeathZone should be at least 2 units below lowest platform
- Every distinct section needs a checkpoint
- Use layer 8 for ALL walkable surfaces

## Key Mappings

| Action | Player 1 (left) | Player 2 (right) |
|--------|-----------------|-------------------|
| Move | WASD | Arrow keys |
| Jump | Space | Up arrow |
| Anchor toggle | E | Down arrow |
