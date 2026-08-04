# 2D Landscape Chase Scene Specification

## 1. Overview & Understanding Summary
A 2D landscape side-scrolling chase minigame/level in Unity where the Player runs ahead of a Chaser across 4 fixed horizontal lanes (stacked vertically in landscape view). The level has a finite distance ending at a Finish Line.

### Core Gameplay Mechanics
- **Lanes**: 4 fixed horizontal lanes (`Lane 0` at top to `Lane 3` at bottom). Player switches lanes using Up/Down inputs.
- **Chaser**: Positioned at the far-left screen boundary (`X_Chaser`), playing a running animation, and smoothly interpolates its Y-position to follow the Player's lane with a short delay.
- **Obstacles**: Spawns from the far-right screen boundary (`X_Spawn`), moves left along the lanes, and despawns after passing a trigger line behind the Chaser on the left (`X_Despawn`). Spawn rate and movement speed increase gradually over time.
- **Safety Step Buffer (Player X-Position)**:
  - The Player has a discrete safety step buffer (e.g., 3 steps away from the Chaser).
  - Hitting an obstacle decrements the step count by 1, instantly shifting the Player 1 step back closer to the Chaser.
  - If the step count reaches 0, the Player touches the Chaser, triggering **Game Over / Defeat**.
- **Level Progress & Finish Line**:
  - Distance accumulates over time while running.
  - Reaching the target distance halts obstacle spawning and spawns the **Finish Line**.
  - Touching the Finish Line triggers **Level Complete / Victory**.

---

## 2. Assumptions & Non-Functional Requirements
- **Engine / Pipeline**: Unity 2D (URP or Standard Renderer), 60 FPS target.
- **Memory Management**: Object Pooling is required for obstacles (`ObstaclePool`) to prevent Garbage Collection spikes.
- **Input System**: Flexible controller supporting Keyboard (W/S or Up/Down arrows) and touch/swipe inputs.
- **Clean State Reset**: Level resets clean without lingering objects or corrupted state upon retry.

---

## 3. Decision Log
| Decision Point | Chosen Solution | Rationale / Alternatives Considered |
|---|---|---|
| **Perspective & Orientation** | Landscape 2D, 4 Horizontal Lanes | User preference for landscape side-scroller layout. |
| **Collision Consequence** | Discrete Safety Steps (X-offset buffer) | Simple, predictable feedback for player position relative to chaser. |
| **Chaser Movement** | Delayed smooth Y-lerp | Creates a realistic pursuit feel while keeping Chaser fixed on X. |
| **Obstacle Lifecycle** | Dynamic Speed Spawner + Object Pool | Ramps difficulty smoothly and avoids memory allocations. |
| **Level Goal** | Finite Distance + Finish Line Object | Provides a clear completion goal rather than an endless loop. |
| **Architecture** | Modular Event-Driven Manager Architecture | High testability, decoupled scripts, and clean separation of concerns. |

---

## 4. System Architecture & Components

```
                      +-------------------+
                      | ChaseLevelManager | (Tracks Distance & Game State)
                      +---------+---------+
                                |
         +----------------------+----------------------+
         |                      |                      |
+--------v-------+    +---------v--------+    +--------v--------+
|  LaneManager   |    |  ObstaclePool    |    | ObstacleSpawner |
+--------+-------+    +---------+--------+    +--------+--------+
         |                      |                      |
+--------v-------+    +---------v--------+             |
| PlayerControl  |    |  ObstacleItem    | <-----------+
+--------+-------+    +------------------+
         |
+--------v-------+
| ChaserControl  |
+----------------+
```

### Component Details

#### 1. `LaneManager.cs`
- **Fields**: `int laneCount = 4`, `float laneSpacing`, `float baseLaneY`.
- **Methods**: `float GetLaneY(int index)` returns the exact world Y position for a given lane index.

#### 2. `PlayerChaseController.cs`
- **Fields**: `int currentLane`, `int safetySteps` (default: 3), `float stepWidthX`, `float moveSpeedY`.
- **Logic**:
  - Handles Up/Down inputs to change `currentLane` between `0` and `3`.
  - Lerps Y position to `LaneManager.GetLaneY(currentLane)`.
  - Calculates X position: `baseChaserX + (safetySteps * stepWidthX)`.
  - On obstacle hit: `TakeHit()` -> decrements `safetySteps`. If `0` -> triggers Game Over.

#### 3. `ChaserController.cs`
- **Fields**: `float followDelaySpeed`, `float fixedXPosition`.
- **Logic**:
  - Remains locked to `fixedXPosition`.
  - Reads Player's `currentLane` and smoothly lerps Y position to match with a configured delay.

#### 4. `ObstacleSpawner.cs` & `ObstaclePool.cs`
- **Fields**: `float initialSpawnInterval`, `float minSpawnInterval`, `float currentSpeed`.
- **Logic**:
  - Retrieves `ObstacleItem` from `ObstaclePool`.
  - Spawns at right edge `X_Spawn` on a randomly chosen lane.
  - Recycles obstacle when position `X <= X_Despawn`.

#### 5. `ChaseLevelManager.cs`
- **Fields**: `float targetDistance`, `float currentDistance`, `GameState state` (Ready, Playing, Victory, Defeat).
- **Logic**:
  - Accumulates `currentDistance += speed * Time.deltaTime`.
  - Updates UI Progress Bar.
  - Spawns Finish Line when `currentDistance >= targetDistance`.
