# Dice x Multiplier - Spirit Cards

This is a small Unity prototype where the player rolls a 3D die, updates a score equation, and triggers spirit card effects based on the result.

## Unity Version
This project was built with:

- Unity 6 LTS (`6000.3.8f1`)

## Quick Start
1. Open Unity Hub.
2. Add the project folder: `d:\Dev\Unity\DiceRoll`
3. Open the project in Unity 6 LTS.
4. Open the scene: `Assets/Scenes/GamePlay.unity`
5. Press Play.

If something is not wired correctly in the scene, the scripts log clear setup errors in the Unity Console.

## How to Play
- Press `ROLL DICE` to roll the die.
- The roll button becomes disabled while the die is rolling.
- After the die settles:
  - the rolled value becomes the base points
  - the multiplier is applied
  - spirit cards are evaluated
  - the UI and effects update automatically

## Debug Controls
The debug panel is available in the Unity Editor and development builds.

- `Force 3`: forces the next roll result to be 3
- `Force 6`: forces the next roll result to be 6
- `Clear`: returns the die to random results

## Implementation Overview
The project is organized into a few simple systems:

- `DiceRoller`
  - Handles physics-based rolling
  - Detects the final face value
  - Publishes roll events

- `GameCalculator`
  - Owns `Points`, `Multiplier`, and `Total`
  - Recalculates the equation when the roll or spirit card effects change it

- `GameManager`
  - Coordinates the roll flow
  - Delays spirit card evaluation until after the die result is known

- `SpiritCardData`
  - ScriptableObject-based card data
  - Stores trigger conditions and effect values

- `SpiritCardManager`
  - Evaluates all cards after each roll
  - Applies card effects through `GameCalculator`

- UI and effects
  - Roll button
  - Equation display
  - Roll history
  - Result popup
  - Camera shake
  - Trail effect

## Event Flow
The project uses a simple event-driven architecture.

- `EventManager` exposes one shared event stream: `OnGameEvent`
- Systems publish typed events such as:
  - `RollStartedEvent`
  - `RollCompletedEvent`
  - `EquationChangedEvent`
  - `SpiritCardActivatedEvent`
  - `SpiritCardIdleEvent`
- UI and effect scripts subscribe once and respond only to the event types they care about

This keeps the systems loosely coupled and easier to extend.

## Technical Notes
- The die uses a `Rigidbody` and `BoxCollider` for rolling.
- The final face is detected from the die's world orientation after it settles.
- `DiceFaceBuilder` creates the dice pips procedurally, so no face textures are required.
- Several scripts validate required Inspector references in `Awake()` to make scene setup issues easier to catch.

## Third-Party Assets
Core implementation uses:

- Unity built-in systems
- TextMesh Pro

If there are imported art packs, UI packs, or VFX assets inside `Assets/Plugins`, they are used as content only and are not required by the gameplay architecture itself.
