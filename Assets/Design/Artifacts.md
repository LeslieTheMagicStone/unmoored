# Artifacts — Level Design Reference

Each artifact is a standalone component. Drag the script onto a GameObject with a Collider2D.

---

## Gate + PressureSwitch

**How it works:** A player stands on the PressureSwitch → Gate snaps open. All players leave → Gate snaps shut. DOTween-animated, instant open/close.

**Setup:**
- Gate: BoxCollider2D (solid) + Gate.cs. Set `openHeight`, `duration`.
- PressureSwitch: BoxCollider2D (IsTrigger=true) + PressureSwitch.cs. Drag Gate into `targetGate`.

**Design notes:** If the Gate closes on a grounded player, they die. Place the PressureSwitch where only one player can reach it — the other must pass through before the switch is released.

---

## CrumblingPlatform

**How it works:** Player steps on top OR anchors to the side → platform shakes for `holdDuration`, then shatters and fades out. Respawns after `respawnDelay`.

**Setup:** BoxCollider2D (solid) + CrumblingPlatform.cs. Optionally drag a child SpriteRenderer Transform into `visualTransform`.

**Design notes:** Force players to move fast. Can block a path until someone deliberately triggers it, then the other player must cross before it's gone. Set `oneShot = true` for gauntlets that don't come back.

---

## MovingPlatform

**How it works:** Patrols a loop of waypoints. Stops moving when any player anchors inside the `anchorZone`. Resumes when all release.

**Setup:** BoxCollider2D (solid) + MovingPlatform.cs + a child trigger collider for `anchorZone`. Create empty GameObjects as waypoints, drag into the `waypoints` array (min 2).

**Design notes:** Classic "one anchors, other rides" puzzle. Can also reverse: everyone must release for the platform to carry you past a hazard.

---

## SlipperySurface

**How it works:** Anchored players touching the side slowly slide downward. Standing on top has no effect.

**Setup:** BoxCollider2D (solid) + SlipperySurface.cs. Tune `slideSpeed`.

**Design notes:** Place on vertical walls to prevent indefinite anchoring. Players must plan their next move before sliding off. Combine with MovingPlatform for timing pressure.

---

## BounceSurface

**How it works:** Player lands on top → launched upward with `bounceForce`. Cooldown prevents infinite bounce loops. Squash/stretch animation on visual.

**Advanced:** Press Jump the moment you contact the surface to stack jump force on top of the bounce, reaching far greater height. This is a natural result of the input system, not a bug — treat it as a skill mechanic.

**Setup:** BoxCollider2D (solid) + BounceSurface.cs. Tune `bounceForce`. Optional `visualTransform` for animation.

**Design notes:** Place under gaps for extra height. The "super bounce" can gate secret paths — only players who nail the timing can reach them. Chain multiple for rhythm platforming.

---

## Checkpoint

**How it works:** Player walks through trigger → checkpoint set. Turns green. Death respawns here.

**Setup:** BoxCollider2D (IsTrigger=true) + SpriteRenderer + Checkpoint.cs.

---

## DeathZone

**How it works:** Any player entering the trigger dies immediately.

**Setup:** BoxCollider2D (IsTrigger=true) + DeathZone.cs. Stretch wide across the bottom of the level.

---

## WinZone

**How it works:** Both players must reach the trigger to win.

**Setup:** BoxCollider2D (IsTrigger=true) + WinZone.cs.

---

### Quick Reference Table

| Artifact | Trigger | Effect | Key Param |
|----------|---------|--------|-----------|
| Gate + Switch | Player on switch | Door opens/closes | `duration` |
| CrumblingPlatform | Step on / anchor side | Shakes, breaks, respawns | `holdDuration` |
| MovingPlatform | Player in anchor zone | Patrol pauses | `moveSpeed` |
| SlipperySurface | Anchor on side | Slides down | `slideSpeed` |
| BounceSurface | Land on top | Launched upward | `bounceForce` |
| Checkpoint | Walk through | Respawn point saved | — |
| DeathZone | Enter trigger | Death | — |
| WinZone | Both players enter | Level complete | — |
