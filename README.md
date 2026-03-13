# Dice x Multiplier - Spirit Cards

Small Unity prototype built around a physics-driven dice roll, spirit card effects, and an event-driven UI/effects flow.

## Unity Version
- Unity 6 LTS (`6000.3.8f1`)

## Setup Steps
1. Open Unity Hub.
2. Add this project folder: `d:\Dev\Unity\DiceRoll`
3. Open the project in Unity 6 LTS.
4. Open the gameplay scene in `Assets/Scenes/GamePlay.unity`.
5. Press Play.

If the scene is missing references, the runtime scripts log setup errors in the Console for the main manager/UI dependencies.

## Controls
- `ROLL DICE`: starts a new roll
- Roll button auto-disables while the die is rolling
- Debug panel:
  - `Force 3`: forces the next result to 3
  - `Force 6`: forces the next result to 6
  - `Clear`: returns to random rolls

The debug panel is intended for Editor and development builds only.

## Implementation Notes
- The dice uses a `Rigidbody` and `BoxCollider` for the roll, then snaps to a clean final orientation after settling.
- `DiceFaceBuilder` generates pip geometry procedurally, so the die does not depend on face textures.
- The project uses a simple event architecture:
  - `EventManager` exposes a single `OnGameEvent` event
  - systems publish typed `GameEvent` objects such as `RollStartedEvent`, `RollCompletedEvent`, and `EquationChangedEvent`
  - UI and effects subscribe once and react only to the event types they need
- `GameCalculator` owns `Points`, `Multiplier`, and `Total`
- `GameManager` handles roll sequencing and delayed spirit card evaluation
- `SpiritCardData` is a `ScriptableObject`, which keeps card rules/data separate from runtime code
- Several scripts validate required Inspector references in `Awake()` to make scene wiring problems easier to spot

## Third-Party Assets
- Unity built-in systems
- TextMesh Pro

No gameplay code depends on a third-party gameplay framework. If the project includes imported art, materials, UI packs, or VFX assets under `Assets/Plugins`, those are used as content/resources rather than as core architecture dependencies.
