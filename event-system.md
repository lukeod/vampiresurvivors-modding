# Event System Documentation

Vampire Survivors uses Zenject's SignalBus for event-driven communication between game systems.

## Signal System Overview

### Core Components

#### SignalBus (Zenject)
**Location**: `Il2CppZenject.SignalBus`

The central event bus that handles all signal subscriptions and firing. Accessible via `GameManager._signalBus` property.

#### SignalsInstaller
**Location**: `Il2CppVampireSurvivors.Signals.SignalsInstaller`

Declares all signal types during game initialization:
- `DeclareUISignals()` - UI event signals
- `DeclareOptionsSignals()` - Settings/options signals  
- `DeclareCharacterSignals()` - Character event signals
- `DeclareLevelUpFactorySignals()` - Level-up system signals
- `DeclareAutomationSignals()` - Automation/testing signals

## Signal Types

### Total Signal Count: ~60+
- **Blittable (struct) signals**: ~37 - Can be subscribed to directly with delegates
- **Non-blittable (class) signals**: ~23 - Contain IL2CPP object references, require special handling

### GameplaySignals

#### Blittable Signals (Structs)
```csharp
// Session management
InitializeGameSessionSignal
PreInitializeGameSessionSignal
GameSessionInitializedSignal
ResetGameSessionSignal
ReturnToAppSignal

// Character events  
CharacterDiedSignal
CharacterXpChangedSignal { float CurrentXp; float MaxXp; }
SetCharacterInvincibilityForMillisSignal { float DurationMillis; }
SetCharacterInvincibilityForMillisNonCumulativeSignal { float DurationMillis; }
ReviveCharacterSignal

// Combat events
TimeStopSignal
FireEnemyBulletSignal

// Weapons & items
WeaponLevelledUpSignal
RemoveAccessoryFromCharacterSignal
WeaponSelectionSignal
BanishWeaponSignal

// Level up
LevelUpSignal
AutoLevelUpSignal
LevelUpCompletedSignal
SkipLevelUpSignal

// Treasure & rewards
OpenTreasureCompletedSignal
OnAfterCoinsAddedSignal
```

#### Non-Blittable Signals (Classes with IL2CPP References)
```csharp
// Character damage
CharacterReceivedDamageSignal { CharacterController Character; }
CharacterLostShieldSignal { float DamageAmount; CharacterController Character; }

// Weapon management
AddWeaponToCharacterSignal { CharacterController Character; WeaponType Weapon; }
WeaponAddedToCharacterSignal { CharacterController Character; WeaponType Weapon; WeaponData Data; }
RemoveWeaponFromCharacterSignal { CharacterController Character; WeaponType Weapon; }
WeaponRemovedFromCharacterSignal

// Hidden weapons
AddHiddenWeaponToCharacterSignal
HiddenWeaponAddedToCharacterSignal
RemoveHiddenWeaponFromCharacterSignal
HiddenWeaponRemovedFromCharacterSignal

// Accessories
AddAccessoryToCharacterSignal
AccessoryAddedToCharacterSignal
AccessoryRemovedFromCharacterSignal

// Skills
AddSkillToCharacterSignal { CharacterController Character; }

// Game state
GamePausedSignal { CharacterController pausingPlayer; }

// Pickups & enemies
PlayerPickedUpNewItemSignal
CharacterFoundSignal
RemoveEnemyFromStageSignal
EnemyKilledImmediateSignal

// Treasure
OpenTreasureSignal
OpenSeasonFanSignal
```

### UISignals

#### Blittable Signals
```csharp
LandingScreenCompletedSignal
IntroAnimCompletedSignal
ShowOptionsScreenSignal
ShowAchievementsScreenSignal
ShowCollectionsScreenSignal
ShowCreditsScreenSignal
OpenGameWeaponSelectionSignal
ShowDLCStoreSignal
```

#### Non-Blittable Signals
```csharp
OpenTPWeaponSelectionSignal { CharacterController Character; TPWeaponGroup WeaponGroup; }
```

### AutomationSignals

All automation signals are blittable structs:
```csharp
RedDamageSignal
BlueDamageSignal
CancelAutomationSignal
AutomationGameSessionInitializedSignal
AutomationSplashScreenInitializedSignal
AutomationIntroWarningScreenInitializedSignal
TestFinished
```

## IL2CPP Signal Limitations

### The Non-Blittable Problem
Signals containing IL2CPP object references (CharacterController, WeaponType, etc.) cannot be marshaled through normal C# delegates. This causes errors like:
```
Delegate has parameter of type CharacterReceivedDamageSignal (non-blittable struct) which is not supported
```

### Solution: InternalFire Interception
Instead of subscribing with delegates, intercept signals at the method level:

```csharp
[HarmonyPatch(typeof(SignalBus), "InternalFire")]
[HarmonyPrefix]
public static void OnInternalFire(Il2CppSystem.Type signalType, Il2CppSystem.Object signal, 
    Il2CppSystem.Object identifier, bool requireDeclaration)
{
    var signalTypeName = signalType?.Name ?? "Unknown";
    
    if (signalTypeName == "CharacterReceivedDamageSignal")
    {
        var damageSignal = signal.Cast<GameplaySignals.CharacterReceivedDamageSignal>();
        // Access damageSignal.Character directly - no marshaling needed
    }
}
```

## Subscribing to Signals

### For Blittable Signals (Direct Subscription)
```csharp
var signalBus = GM.Core._signalBus;

// Simple signal
signalBus.Subscribe<GameplaySignals.InitializeGameSessionSignal>(
    (Action)(() => MelonLogger.Msg("Session started!"))
);

// Signal with data
signalBus.Subscribe<GameplaySignals.CharacterXpChangedSignal>(
    (Action<GameplaySignals.CharacterXpChangedSignal>)((signal) => {
        MelonLogger.Msg($"XP: {signal.CurrentXp}/{signal.MaxXp}");
    })
);
```

### For Non-Blittable Signals (Use Harmony Patches)
```csharp
// Cannot subscribe directly - use InternalFire interception as shown above
```

## Firing Signals

### Basic Firing
```csharp
// Empty signal
signalBus.Fire<GameplaySignals.InitializeGameSessionSignal>();

// Signal with data
var xpSignal = new GameplaySignals.CharacterXpChangedSignal {
    CurrentXp = 100f,
    MaxXp = 200f
};
signalBus.Fire(xpSignal);

// TryFire (won't throw if signal not declared)
signalBus.TryFire(xpSignal);
```

### Firing with Identifier
The identifier parameter is optional and rarely used. It allows targeted signal delivery:
```csharp
signalBus.FireId<SomeSignal>(identifier, signalData);
```

## SignalBus Methods

### Core Methods
```csharp
// InternalFire - All signals go through this
void InternalFire(Type signalType, object signal, object identifier, bool requireDeclaration)

// Subscription
void Subscribe<TSignal>(Action callback)
void Subscribe<TSignal>(Action<TSignal> callback)
void SubscribeId<TSignal>(object identifier, Action<TSignal> callback)

// Unsubscription
void Unsubscribe<TSignal>(Action callback)
void TryUnsubscribe<TSignal>(Action callback)

// Signal firing
void Fire<TSignal>()
void Fire<TSignal>(TSignal signal)
void TryFire<TSignal>(TSignal signal)
void FireId<TSignal>(object identifier, TSignal signal)

// Properties
int NumSubscribers { get; }
```

## Signal Flow Examples

### XP Change Flow
1. Enemy killed → `EnemyController.Die()`
2. XP pickup created → `LootManager.CreateXP()`
3. Player collects → `CharacterController.AddXP()`
4. Signal fired → `signalBus.Fire<CharacterXpChangedSignal>()`
5. UI updates → `MainGamePage.UpdateExperienceProgress(signal)`

### Damage Flow
1. Enemy attacks → Collision detected
2. Damage calculated → `CharacterController.TakeDamage()`
3. Signal fired → `signalBus.Fire<CharacterReceivedDamageSignal>()`
4. Systems react → Shield loss, invincibility frames, UI update

## Best Practices

### Memory Management
- Store action references for unsubscribing
- Unsubscribe in cleanup methods
- Be careful with lambda captures

### Performance
- Signals are synchronous - handlers execute immediately
- Keep handlers lightweight
- Execution order follows subscription order

### Hook Points
1. **GameManager._signalBus property** - Best access point
2. **GameManager.Awake** - Early subscription point
3. **After GM.Core is set** - Safe for all subscriptions

## Common Patterns

### Global Signal Monitor
```csharp
[HarmonyPatch(typeof(SignalBus), "InternalFire")]
[HarmonyPrefix]
public static void MonitorAllSignals(Il2CppSystem.Type signalType, Il2CppSystem.Object signal)
{
    MelonLogger.Msg($"Signal: {signalType.Name}");
}
```

### Accessing SignalBus
```csharp
// Via GameManager property (preferred)
var signalBus = GM.Core._signalBus;

// Via GameManager field search
var gmType = gameManager.GetType();
var properties = gmType.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
foreach (var prop in properties)
{
    if (prop.PropertyType == typeof(SignalBus) && prop.Name == "_signalBus")
    {
        signalBus = prop.GetValue(gameManager) as SignalBus;
    }
}
```

## Limitations

1. **Non-blittable struct marshaling** - Cannot create delegates for signals with IL2CPP references
2. **Signal declaration required** - Must use TryFire for undeclared signals
3. **Synchronous only** - All signals execute immediately
4. **No priority system** - Handlers execute in subscription order
5. **Memory leaks** - Forgetting to unsubscribe causes leaks

## Summary

The Vampire Survivors event system:
- Uses Zenject SignalBus for all game events
- Has ~60+ different signal types
- Splits between blittable (simple) and non-blittable (complex) signals
- Requires special handling for IL2CPP object references
- All signals pass through InternalFire method
- Identifiers are optional and rarely used (usually null)
- Synchronous execution in subscription order