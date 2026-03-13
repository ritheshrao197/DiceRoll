# Dice x Multiplier - Spirit Cards

Small Unity prototype built around a physics-driven dice roll, an event bus, and ScriptableObject-powered spirit cards.

## Unity Version
Unity 6 LTS (`6000.0.x`)

## Runtime Overview
- `DiceRoller` launches a 3D die with physics, waits for it to settle, reads the top face, then snaps cleanly to that face.
- `GameEventBus` is the messaging layer between gameplay, UI, and effects.
- `GameCalculator` owns `Points`, `Multiplier`, and `Total`.
- `GameManager` delays spirit card evaluation after each completed roll.
- `SpiritCardManager` evaluates the configured `SpiritCardData` assets.
- UI and effects subscribe to events instead of polling.

## Script Layout
### Core
- `Assets/Scripts/Core/DiceRoller.cs`
- `Assets/Scripts/Core/DiceFaceBuilder.cs`
- `Assets/Scripts/Core/GameCalculator.cs`
- `Assets/Scripts/Core/GameEventBus.cs`
- `Assets/Scripts/Core/GameManager.cs`

### Spirit Cards
- `Assets/Scripts/SpiritCards/SpiritCardData.cs`
- `Assets/Scripts/SpiritCards/SpiritCardManager.cs`
- `Assets/Scripts/SpiritCards/SpiritCardView.cs`

### UI
- `Assets/Scripts/UI/UIRollButton.cs`
- `Assets/Scripts/UI/UIEquationView.cs`
- `Assets/Scripts/UI/UIRollHistory.cs`
- `Assets/Scripts/UI/UIDebugPanel.cs`

### Effects
- `Assets/Scripts/Effects/CameraShake.cs`
- `Assets/Scripts/Effects/DiceTrailEffect.cs`
- `Assets/Scripts/Effects/DiceResultPopup.cs`

## Gameplay Flow
1. `UIRollButton` calls `GameManager.RequestRoll()`.
2. `GameManager` tells `DiceRoller` to roll if the die is idle.
3. `DiceRoller` publishes `GameEventBus.RollStarted()`.
4. Roll listeners respond:
   - `CameraShake` shakes the camera.
   - `DiceTrailEffect` enables the trail.
   - `UIRollButton` disables itself.
5. When the die settles, `DiceRoller` publishes `GameEventBus.RollCompleted(face)`.
6. `GameCalculator` resets equation state to `Points = face`, `Multiplier = 10`, `Total = Points * Multiplier`.
7. `GameManager` waits for `spiritCardDelay`, then asks `SpiritCardManager` to evaluate cards.
8. Matching cards apply effects through `GameCalculator`, and card/result UI updates through bus events.

## Expected Scene Wiring
### Dice
- A GameObject with:
  - `Rigidbody`
  - `BoxCollider`
  - `DiceRoller`
  - `DiceFaceBuilder`
  - `TrailRenderer`
  - `DiceTrailEffect`

### Managers
- A GameObject with:
  - `GameCalculator`
  - `SpiritCardManager`
  - `GameManager`

Inspector assignments:
- `GameManager.diceRoller` -> scene `DiceRoller`
- `GameManager.spiritCardManager` -> scene `SpiritCardManager`
- `SpiritCardManager.calculator` -> scene `GameCalculator`
- `SpiritCardManager.cards` -> assigned spirit card assets

### UI
- `UIRollButton` with a `Button` component and a `GameManager` reference
- `UIEquationView` with `pointsText`, `multiplierText`, and `totalText`
- `UIRollHistory` with `historyLabel`
- `UIDebugPanel` with `DiceRoller`, buttons, and optional status label

### Effects
- `CameraShake` on the camera that should move locally during the roll
- `DiceResultPopup` with a TMP label

## Notes
- `GameEventBus` auto-clears static subscribers at subsystem registration to reduce stale event state across play mode reloads.
- Most runtime scripts now log missing inspector references early to make scene setup mistakes obvious.
- The debug panel only stays active in the editor or development builds.
