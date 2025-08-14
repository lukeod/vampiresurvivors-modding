# Core Architecture

## Overview

Vampire Survivors uses a modular architecture with dependency injection (Zenject) and centralized static accessors. Understanding these core systems is essential for effective modding.

## Static Accessors

### GM.Core - GameManager Access
**Location**: `Il2CppVampireSurvivors.Framework.GM`
**Actual Namespace**: `VampireSurvivors.Framework`

The primary static accessor for accessing the GameManager instance:

```csharp
// Static accessor for GameManager
public static GameManager Core { get; set; }

// Usage
var gameManager = GM.Core;
var dataManager = gameManager?.DataManager;
```

**Important**: GM.Core may be null in certain game states (e.g., main menu). Always use null-conditional operators.

## GameManager - Core Game Controller

**Location**: `Il2CppVampireSurvivors.Framework.GameManager`
**Actual Namespace**: `VampireSurvivors.Framework`

The central controller that manages all game systems.

### Key Properties

```csharp
public DataManager DataManager { get; }           // Data access
public DataManager _dataManager;                   // Data access (field)
public Stage _stage;                              // Current stage
public PlayerOptions _playerOptions;              // Player preferences
public List<CharacterController> AllPlayers;      // All players
public List<CharacterController> MainPlayers;     // Main players only
public CharacterController Player;                // Primary player
public CharacterController PlayerOne;             // Player one specifically
public MainGamePage MainUI;                       // Main UI controller
```

### Key Methods

```csharp
// Constructor injection method
void Construct(SignalBus signalBus, DiContainer diContainer, 
    PlayerOptions playerOptions, LootManager lootManager, WeaponsFacade weaponsFacade, 
    Stage stage, GameSessionData gameSessionData, LevelUpFactory levelUpFactory, 
    CharacterFactory characterFactory, AccessoriesFacade accessoriesFacade, 
    DataManager dataManager, PlayerStats playerStats, ArcanaManager arcanaManager, 
    PhysicsManager physicsManager, EggManager egg, LimitBreakManager limitBreakManager, 
    GizmoManager gizmoManager, TreasureFactory treasureFactory, 
    ProjectileFactory projectileFactory, SpellsManager spellsManager, 
    AchievementManager achievementManager, MainGamePage mainGamePage, 
    MultiplayerManager multiplayer, AdventureManager adventureManager, 
    FontFactory fontFactory, AssetReferenceLibrary assetReferenceLibrary, 
    ParticleManager particleManager)

// Player and game state methods
void StartEnterWeaponSelection(string weaponSelectionType, CharacterController interactingPlayer)
void StartShopScene(CharacterController interactingPlayer, PickupCustomMerchant customMerchant, 
    Il2CppSystem.Nullable<MerchantInventoryType> inventoryType)
CharacterController GetClosestPlayer(float2 position, PlayerInclusionMode inclusionMode, 
    float maxRangeSqrd, bool includeFollowers)
CharacterController GeneratePlayerCharacter(CharacterType characterType, int playerIndex)
```

### Access Pattern

```csharp
// Through GM.Core static accessor
var gameManager = GM.Core;
if (gameManager != null)
{
    var dataManager = gameManager.DataManager;
    var currentStage = gameManager._stage;
    var player = gameManager.Player;
}

// Direct access through static reference
var dataManager = GM.Core?._dataManager;
```

## Dependency Injection (Zenject)

The game uses Zenject for dependency injection rather than traditional singleton patterns.

### GameInstaller
**Location**: `Il2CppVampireSurvivors.Installers.GameInstaller`
**Actual Namespace**: `VampireSurvivors.Installers`

Configures dependency injection bindings for the game.

### GameManager Constructor

Services are injected through the constructor:

```csharp
// Full constructor signature with all injected dependencies
public void Construct(SignalBus signalBus, DiContainer diContainer, 
    PlayerOptions playerOptions, LootManager lootManager, WeaponsFacade weaponsFacade, 
    Stage stage, GameSessionData gameSessionData, LevelUpFactory levelUpFactory, 
    CharacterFactory characterFactory, AccessoriesFacade accessoriesFacade, 
    DataManager dataManager, PlayerStats playerStats, ArcanaManager arcanaManager, 
    PhysicsManager physicsManager, EggManager egg, LimitBreakManager limitBreakManager, 
    GizmoManager gizmoManager, TreasureFactory treasureFactory, 
    ProjectileFactory projectileFactory, SpellsManager spellsManager, 
    AchievementManager achievementManager, MainGamePage mainGamePage, 
    MultiplayerManager multiplayer, AdventureManager adventureManager, 
    FontFactory fontFactory, AssetReferenceLibrary assetReferenceLibrary, 
    ParticleManager particleManager)
```

**Important**: This means there are no static Instance properties. Always use GM.Core or injected references.

## Initialization Flow

### Application Startup Sequence

1. **Application Start**
   - MelonLoader initializes
   - Mod assemblies load
   - Harmony patches apply

2. **Initial Data Loading**
   - DataManager initialization
   - `LoadBaseJObjects()` loads raw JSON data
   - `ReloadAllData()` called multiple times:
     - First call: Base game data conversion
     - Subsequent calls: Each DLC's data loading and merging
     - Final call: Data validation and cross-referencing
   - All data ready within 3-5 seconds of launch

3. **Main Menu**
   - Game waits in main menu for user interaction
   - `GM.Core` remains null during menu state
   - Data is loaded but GameManager not yet instantiated

### Game Session Sequence

1. **Player Initiates Game**
   - Character selection
   - Stage selection
   - Game mode configuration

2. **GameManager Initialization**
   - GameManager instantiated through dependency injection
   - `GameManager.Awake()` called
   - `GM.Core` static accessor set
   - `Construct()` method called with injected dependencies
   - All core systems initialize

3. **Session Start**
   - Player character creation via CharacterFactory
   - Starting weapon initialization through WeaponsFacade
   - Stage and enemy spawning begins
   - Gameplay loop active

## System Architecture

### Core Components

1. **GameManager** - Central game controller
2. **DataManager** - Data repository and loading
3. **Stage** - Level and environment management
4. **CharacterController** - Player/enemy entities
5. **WeaponsFacade** - Weapon system facade
6. **ArcanaManager** - Arcana effects system
7. **StageEventManager** - Scripted events and spawning
8. **SaveSerializer** - Save/load functionality
9. **LootManager** - Loot generation and management
10. **AccessoriesFacade** - Accessories system
11. **SpellsManager** - Spell system management
12. **MultiplayerManager** - Multiplayer functionality
13. **AdventureManager** - Adventure mode management

### Communication Patterns

```csharp
// Direct property access
var dataManager = GM.Core?.DataManager;
var weaponData = dataManager?.WeaponData;

// Event-based (through SignalBus)
signalBus.Fire<PlayerLevelUpSignal>();

// Player queries
var closestPlayer = GM.Core?.GetClosestPlayer(position, PlayerInclusionMode.AliveOrDead, maxRange, true);

// Stage access
var stage = GM.Core?._stage;
var spawnedEnemies = stage?._spawnedEnemies;
```

## Best Practices

### Accessing Core Systems

```csharp
// Always null-check GM.Core
if (GM.Core != null)
{
    var dataManager = GM.Core.DataManager;
    // Safe to use dataManager
}

// Use null-conditional for chains
var weaponData = GM.Core?.DataManager?.GetConvertedWeapons();
if (weaponData != null)
{
    // Process weapon data
}
```

### Timing Considerations

- **During Startup**: GM.Core is null, only DataManager operations available
- **Menu State**: GM.Core remains null, data is loaded but no active game session
- **Game Session Start**: GM.Core becomes available when `GameManager.Awake()` is called
- **In-Game**: Full access to all systems through GM.Core
- **Best Hooks**:
  - `DataManager.ReloadAllData()` for data modifications
  - `GameManager.Awake()` for session-specific setup
  - Monitor GM.Core in `OnUpdate()` for reliable detection

### Memory Management

- Game uses object pooling extensively
- Respect existing pools to avoid performance issues
- Use weak references for Unity objects
- Clean up event subscriptions properly

## Common Integration Points

### For Data Modifications
Hook into DataManager initialization or access data through `GM.Core.DataManager` properties.

### For Gameplay Changes
Hook into `GameManager.Construct()` for initialization or use methods like `StartEnterWeaponSelection()` and `GeneratePlayerCharacter()`.

### For UI Modifications
Access `GameManager.MainUI` for HUD and interface changes.

### For Save System
Hook into `SaveSerializer` methods for save data manipulation.
**Location**: `Il2CppVampireSurvivors.Framework.Saves.SaveSerializer`

### For Player Management
Use `GM.Core.AllPlayers`, `GM.Core.MainPlayers`, or `GM.Core.GetClosestPlayer()` for player queries.

## Thread Safety

Most game systems are **not** thread-safe:
- All modifications should happen on the main Unity thread
- Use Unity's main thread dispatcher for async operations
- Avoid concurrent modifications to game state

## Performance Considerations

- GameManager methods are called frequently
- Avoid expensive operations in per-frame methods
- Cache references when possible
- Use object pooling for spawned entities

## Additional Architecture Notes

### Base Classes
- **GameManager** extends `GameMonoBehaviour` (custom MonoBehaviour base)
- **Stage** extends `GameMonoBehaviour` 
- **ArcanaManager** extends `GameTickable` (for frame-by-frame updates)

### Facade Pattern Usage
The game uses Facade pattern extensively for complex subsystems:
- **WeaponsFacade** - Weapon system interface
- **AccessoriesFacade** - Accessories system interface
- **BackendFacade** - Platform backend abstraction

### Additional Managers
- **LimitBreakManager** - Limit break system
- **GizmoManager** - Gizmo/utility management
- **GlimmerManager** - Visual effects
- **PhysicsManager** - Physics calculations
- **ParticleManager** - Particle effects

### Factory Pattern Usage
- **LevelUpFactory** - Creates level-up components
- **CharacterFactory** - Character instantiation
- **TreasureFactory** - Treasure generation  
- **ProjectileFactory** - Projectile creation
- **FontFactory** - Font management

### Data Architecture
Data is stored in JSON format and converted at runtime:
- Base game data in core files
- DLC data merged separately
- Runtime conversion to C# objects
- Caching for performance