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
- `DeclareOnlineSignals()` - Online/multiplayer signals
- `DeclareOptionsSignals()` - Settings/options signals  
- `DeclareCharacterSignals()` - Character event signals
- `DeclareLevelUpFactorySignals()` - Level-up system signals
- `DeclareAutomationSignals()` - Automation/testing signals

## Signal Types

### Total Signal Count: 163
- **Blittable (struct) signals**: 128 - Can be subscribed to directly with delegates
- **Non-blittable (class) signals**: 35 - Contain IL2CPP object references, require special handling

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
TimeStopSignal { bool IgnoreMovementFreezeFromTimeStop; bool SkipStandardVFX; }
ChangeSpectateSignal { bool GoStraightToRecapPage; }
SummonWhiteHandSignal
FireEnemyBulletSignal { Vector2 SpawnPos; EnemyType BulletType; }

// Weapons & items
WeaponLevelledUpSignal
RemoveAccessoryFromCharacterSignal
WeaponSelectionSignal
BanishWeaponSignal

// Level up
LevelUpSignal
LevelUpWithoutScreenSignal
AutoLevelUpSignal
LevelUpCompletedSignal

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
ConnectionErrorSignal { ConnectionException ConnectionException; }

// Pickups & enemies
PlayerPickedUpNewItemSignal
CharacterFoundSignal
RemoveEnemyFromStageSignal
EnemyKilledImmediateSignal
DestructibleDestroyed

// Treasure
OpenTreasureSignal
OpenSeasonFanSignal

// Level up (non-blittable)
SkipLevelUpSignal
```

### UISignals

#### Blittable Signals
```csharp
// Screen navigation
OnEnteredUISignal
LandingScreenCompletedSignal
IntroAnimCompletedSignal
ShowOptionsScreenSignal
ShowAchievementsScreenSignal
ShowCollectionsScreenSignal
ShowCreditsScreenSignal
ShowDLCStoreSignal
ShowPowerUpsScreenSignal
ShowSecretsScreenSignal
ShowCharacterSelectScreenSignal
ShowOnlineScreenSignal
ShowOnlineLobbyScreenSignal
GoBackOnlineSignal
ShowAdventuresSelectionViewSignal
ShowAdventuresAscensionSignal

// Game setup
OpenGameWeaponSelectionSignal
LaunchGameplaySignal
QuickStartGameSignal
ConfirmCharacterSignal
ConfirmStageSelectionSignal
SelectOnlineStageSignal

// Healer and Arcana
OpenHealerSignal
CloseHealerSignal
OpenArcanaSignal
CloseArcanaSignal
ArcanaSelectedSignal
ArcanaSkippedSignal

// Piano and interactions
OpenPianoSignal
ClosePianoSignal
PianoTuneCompleteSignal

// UI toggles and settings
ToggleGuidesSignal
TogglePickupsSignal
ToggleMovingBackgroundSignal
SetFullscreenSignal
ToggleStageProgressionSignal
ToggleHideDebugUISignal
ToggleHideGameUISignal
ToggleXPBarSignal
ToggleWeaponSlotsSignal
SetVisibleJoysticksSignal
SetDamageNumbersSignal
SetGlimmerCarouselSignal
SetFlashingVFXSignal
SetStreamerSafeMusicSignal
SetSFXVolumeSignal
SetMusicVolumeSignal

// Items and purchasing
ReceivedNewItemSignal
DiscardNewItemSignal
ShowItemFoundScreenSignal
BuyPowerUpSignal
RefundPowerUpsSignal
CharacterBoughtSignal
SkinBoughtSignal

// Game events
RecapPageCompletedSignal
SkipWeaponSelectionSignal
BackButtonPressedSignal
ShowBackButtonSignal
HideBackButtonSignal
QuitGameSignal

// Unlocks and achievements
CharacterUnlockedSignal
StageUnlockedSignal
WeaponUnlockedSignal
CharacterCollectedSignal
WarningShownSignal
SyncSteamAchievementsSignal

// Gold Fever
GoldFeverStartedSignal
GoldFeverEndedSignal
EmitGoldFeverParticleSignal
GoldFeverCoinCollectedSignal

// VFX and UI effects
CreateDamageNumberSignal
CreateSpecialDamageNumberSignal
CreateImpactVFXSignal
SpawnMinorDoilieSignal
StartTPUnlockSequenceSignal
ShowFinalFireworksSignal
CloseFinalFireworksSignal

// Miscellaneous
OpenLanguagePageSignal
OpenBestiarySignal
ShowEndCreditsSignal
ShowGameOverinoSceneSignal
ShowAccountPageSignal
ShowLevelBonusSelectionSignal
LevelUpBonusSelectedSignal
SkipLevelUpBonusSignal
CharacterFoundPageClosedSignal
RefreshCursorsSignal
BanishWeaponLevelUpSignal
MerchantClosedSignal
HideAllCursorsSignal
UnhideCursorsSignal
ForceSelectionOnCharacterSelectionPageSignal
```

#### Non-Blittable Signals
```csharp
OpenTPWeaponSelectionSignal { CharacterController Character; TPWeaponGroup WeaponGroup; }
AddNewCharactersToSelectionPageSignal
ShowOnlineErrorScreenSignal
ForceBackButtonNavigation
FadeScreenSignal
FireNewGlimmerTechnique
LanguageSelectedSignal
TreasureChestSpawnedSignal
TreasureChestCollectedSignal
SpawnOffScreenCursorSignal
ShowCursorSignal
HideCursorSignal
RemoveOffScreenCursorSignal
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
```

#### Non-Blittable Signals
```csharp
TestFinished { string TestName; }
```

### OnlineSignals

The OnlineSignals category is new in Unity 6000.0.36f1, containing signals for multiplayer and online functionality.

#### Blittable Signals (Structs)
```csharp
// Level up related
OnlineLevelUpReRollRequested
RequestOnlineLevelUpPass
OnlineLevelUpPass { bool ShowStats; }
OnlineLevelUpSkip

// Item selection
OnlineCloseItemFoundPage { bool Discard; }
SelectCandyBoxWeapon { WeaponType Weapon; }
SelectTPWeapon { WeaponType Weapon; }
SkipTpWeapon
SkipCandyBox
SelectLevelUpBonus { PowerUpType LevelUpBonus; }
SkipLevelBonus

// Arcana system
OnlineSelectedArcana { int Arcana; }
OnlineReRolledArcanas
ArcanaModeTransition

// Stage interactions
SuccessfulPianoSignal
ExitPianoSignal
TouchedPianoKeySignal { int Key; }
RightCoffinOpened

// Character collection
RevealCharacter
CollectCharacter

// Director feedback
DirecterTooEasy
DirecterTooHard
DirecterOkayButton
OnDirecterStageSwitch { int Stage; }

// Miscellaneous
OnlineSkipTreasureAnim
ForceCloseUi
WestwoodsSpin { int _seed; }
```

#### Non-Blittable Signals (Classes with IL2CPP References)
```csharp
// Level up with items
OnlineLevelUpReRoll { List<WeaponType> ChosenWeapons; }
OnlineLevelUpWithItem { ItemType ItemType; CharacterController ReceivingCharacter; }
OnlineLevelUpWithLimitBreak { int ChosenLimitBreakIndex; bool AlwaysRandomLimitBreak; CharacterController ReceivingCharacter; }

// Purchase system
OnlinePurchase { WeaponType Weapon; ItemType Item; int Price; CharacterController PurchasingPlayer; }

// Connection events
CharacterDisconnected { CharacterController DisconnectedPlayer; }
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
    else if (signalTypeName == "OnlineLevelUpWithItem")
    {
        var itemSignal = signal.Cast<OnlineSignals.OnlineLevelUpWithItem>();
        MelonLogger.Msg($"Online item level up: {itemSignal.ItemType} for {itemSignal.ReceivingCharacter?.name}");
    }
    else if (signalTypeName == "ConnectionErrorSignal")
    {
        var errorSignal = signal.Cast<GameplaySignals.ConnectionErrorSignal>();
        MelonLogger.Msg($"Connection error: {errorSignal.ConnectionException}");
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

// Online signals (new in Unity 6000.0.36f1)
signalBus.Subscribe<OnlineSignals.OnlineLevelUpPass>(
    (Action<OnlineSignals.OnlineLevelUpPass>)((signal) => {
        MelonLogger.Msg($"Online level up pass - show stats: {signal.ShowStats}");
    })
);

signalBus.Subscribe<OnlineSignals.TouchedPianoKeySignal>(
    (Action<OnlineSignals.TouchedPianoKeySignal>)((signal) => {
        MelonLogger.Msg($"Piano key pressed: {signal.Key}");
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
- Has 163 different signal types (significantly expanded in Unity 6000.0.36f1)
- Splits between blittable (128 signals) and non-blittable (35 signals)
- Includes new OnlineSignals category for multiplayer functionality
- Requires special handling for IL2CPP object references
- All signals pass through InternalFire method
- Identifiers are optional and rarely used (usually null)
- Synchronous execution in subscription order