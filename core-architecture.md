# Core Architecture

## Overview

Based on decompiled IL2CPP code analysis, Vampire Survivors appears to use dependency injection (Zenject) with centralized static accessors. The v1.0+ architecture includes multiplayer support, Unity 6000, and additional visual systems.

**v1.0+ Components:**
- GameManager injects 28 dependencies
- Multiplayer properties (`IsOnlineMultiplayer`, `IsLocalMultiplayer`)
- Managers: ParticleManager (particle effects), GizmoManager (visual feedback), MultiplayerManager (online/local coop)
- Factories: FontFactory (font assets), ShopFactory (merchant inventories)
- Coherence Toolkit integration for online networking

## Static Accessors

### GM.Core - GameManager Access
**Location**: `Il2CppVampireSurvivors.Framework.GM`

```csharp
public static GameManager Core { get; set; }

// Usage
var gameManager = GM.Core;
var dataManager = gameManager?.DataManager;
```

Based on code analysis, GM.Core appears to be null in menu states. Use null-conditional operators.

## GameManager

**Location**: `Il2CppVampireSurvivors.Framework.GameManager`

### Key Properties

Inferred from decompiled IL2CPP code:

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

// Multiplayer Properties (v1.0+)
public bool IsOnlineMultiplayer { get; }          // Network multiplayer (318 references)
public bool IsLocalMultiplayer { get; }           // Local coop multiplayer
```

### Key Methods

Based on code analysis:

```csharp
// Constructor injection method - 28 dependencies (v1.0+)
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
    ParticleManager particleManager, ShopFactory shopFactory)

// Player and game state methods
void QueueOpenWeaponSelection(CharacterController player, string weaponSelectionType)
// OpenWeaponSelection appears to be private, use QueueOpenWeaponSelection
CharacterController GetClosestPlayer(float2 position, PlayerInclusionMode inclusionMode, 
    float maxRangeSqrd, bool includeFollowers)
CharacterController GeneratePlayerCharacter(CharacterType characterType, int playerIndex)
```

### Access Pattern

Based on code analysis:

```csharp
var gameManager = GM.Core;
if (gameManager != null)
{
    var dataManager = gameManager.DataManager;
    var currentStage = gameManager._stage;
    var player = gameManager.Player;
    
    // Check multiplayer mode
    if (gameManager.IsOnlineMultiplayer)
    {
        // Online multiplayer logic
    }
    else if (gameManager.IsLocalMultiplayer)
    {
        // Local coop logic
        var playerCount = gameManager._multiplayer.GetPlayerCount();
    }
    else
    {
        // Single player logic
    }
}
```

## Dependency Injection (Zenject)

### Installer Architecture

Based on decompiled code, the game appears to use multiple installer classes following Zenject patterns:

#### GameInstaller
**Location**: `Il2CppVampireSurvivors.Installers.GameInstaller`

Main game-level dependency installer with methods (inferred from code):
- `InstallBindings()` - Main binding configuration
- `Install()` - Core installation logic
- `InstallData()` - Data system bindings
- `InstallUI()` - UI component bindings
- `InstallMobile()` - Mobile-specific bindings

#### CoreInstaller
**Location**: `Il2CppVampireSurvivors.Installers.CoreInstaller`

Based on code analysis, handles core system bindings including:
- Debug console and profiling tools
- DLC catalog management
- Base game data configuration
- Main menu background factory

#### FactoriesInstaller
**Location**: `Il2CppVampireSurvivors.Installers.FactoriesInstaller`

Based on code analysis, manages factory pattern bindings for content creation:
- `WeaponFactory` - Weapon prefab management
- `ProjectileFactory` - Projectile creation
- `CharacterFactory` - Character instantiation
- `EnemyFactory` - Enemy spawning with pooling
- `AccessoriesFactory` - Item/accessory creation
- `ShopFactory` - Shop and merchant creation
- Additional factories for destructibles, pickups, VFX, fonts, and assets

#### DataManagerSettingsInstaller
**Location**: `Il2CppVampireSurvivors.Installers.DataManagerSettingsInstaller`

Extends `ScriptableObjectInstaller<DataManagerSettingsInstaller>` for configuration injection.

### GameManager Constructor

Based on decompiled code, services appear to be injected through the constructor (28 total dependencies in v1.0+):

```csharp
// Full constructor signature with all injected dependencies
public void Construct(
    SignalBus signalBus,              // Event system
    DiContainer diContainer,          // Dependency injection
    PlayerOptions playerOptions,      // Player configuration
    LootManager lootManager,          // Loot distribution
    WeaponsFacade weaponsFacade,      // Weapon management
    Stage stage,                      // Stage/level management
    GameSessionData gameSessionData,  // Session state
    LevelUpFactory levelUpFactory,    // Level progression
    CharacterFactory characterFactory, // Character instantiation
    AccessoriesFacade accessoriesFacade, // Accessory management
    DataManager dataManager,          // Game data
    PlayerStats playerStats,          // Player statistics
    ArcanaManager arcanaManager,      // Arcana system
    PhysicsManager physicsManager,    // Physics simulation
    EggManager egg,                   // Egg/secret management
    LimitBreakManager limitBreakManager, // Limit break system
    GizmoManager gizmoManager,        // Visual effects and overlays
    TreasureFactory treasureFactory,  // Treasure generation
    ProjectileFactory projectileFactory, // Projectile management
    SpellsManager spellsManager,      // Spell system
    AchievementManager achievementManager, // Achievement tracking
    MainGamePage mainGamePage,        // Main UI
    MultiplayerManager multiplayer,   // Multiplayer coordination
    AdventureManager adventureManager, // Adventure mode
    FontFactory fontFactory,          // Font asset management
    AssetReferenceLibrary assetReferenceLibrary, // Asset loading
    ParticleManager particleManager,  // Particle system management
    ShopFactory shopFactory           // Shop inventory generation
)
```

**Key Dependencies:**
- **GizmoManager**: Visual effects and UI overlays for level-ups, weapon icons, and special animations. See [Visual Effects System](visual-effects.md)
- **MultiplayerManager**: Handles online and local multiplayer coordination. See [Multiplayer System](multiplayer-system.md)
- **FontFactory**: Centralized font asset management using Unity's Addressable system. See [UI System](ui-system.md#fontfactory)
- **ParticleManager**: Manages particle system lifecycle with automatic pause handling. See [Visual Effects System](visual-effects.md)
- **ShopFactory**: Generates merchant inventories with multiplayer synchronization. See [Shop System](shop-system.md)

The `DiContainer` is stored as `_diContainer` for runtime object creation.

### Signal System

Based on code analysis, the game appears to use Zenject's signal system:

#### SignalsInstaller
**Location**: `Il2CppVampireSurvivors.Signals.SignalsInstaller`

Based on decompiled code, declares signal types for decoupled communication:
- `DeclareUISignals()` - UI event signals
- `DeclareOptionsSignals()` - Settings/options signals
- `DeclareCharacterSignals()` - Character event signals
- `DeclareLevelUpFactorySignals()` - Level-up system signals
- `DeclareAutomationSignals()` - Automation/testing signals

### Runtime Object Creation

Based on code analysis, runtime object creation patterns appear to include:
- Factory classes that manage prefab instantiation
- `ProjectContext.Instance.Container` for runtime access
- Asset reference system with lazy loading

Access appears to be through GM.Core or injected references.

## Initialization Flow

### Application Startup Sequence

Based on code analysis, the startup sequence appears to be:

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
   - Based on code timing, data appears ready within 3-5 seconds of launch

3. **Main Menu**
   - Game waits in main menu for user interaction
   - `GM.Core` remains null during menu state
   - Data is loaded but GameManager not yet instantiated

### Game Session Sequence

Based on decompiled code analysis:

1. **Player Initiates Game**
   - Character selection
   - Stage selection
   - Game mode configuration

2. **GameManager Initialization**
   - GameManager appears to be instantiated through dependency injection
   - `GameManager.Awake()` called
   - `GM.Core` static accessor set
   - `Construct()` method called with injected dependencies
   - Core systems initialize

3. **Session Start**
   - Player character creation appears to use CharacterFactory
   - Starting weapon initialization through WeaponsFacade
   - Stage and enemy spawning begins
   - Gameplay loop becomes active

## System Architecture

### Core Components

Based on decompiled IL2CPP code analysis:

1. **GameManager** - Central game controller with multiplayer awareness
2. **DataManager** - Data repository and loading with online sync tracking
3. **Stage** - Level and environment management
4. **CharacterController** - Player/enemy entities with network synchronization
5. **WeaponsFacade** - Weapon system facade
6. **ArcanaManager** - Arcana effects system
7. **StageEventManager** - Scripted events and spawning
8. **SaveSerializer** - Save/load functionality
9. **LootManager** - Loot generation and management
10. **AccessoriesFacade** - Accessories system
11. **SpellsManager** - Spell system management
12. **MultiplayerManager** - Multiplayer functionality and coordination
13. **AdventureManager** - Adventure mode management
14. **ParticleManager** - Particle system lifecycle and rendering
15. **GizmoManager** - Visual effects and UI overlays
16. **FontFactory** - Font asset management
17. **ShopFactory** - Shop generation with multiplayer support

### Communication Patterns

```csharp
// Direct property access
var dataManager = GM.Core?.DataManager;
var weaponData = dataManager?.WeaponData;

// Event-based (through SignalBus)
signalBus.Fire<PlayerLevelUpSignal>();

// Player queries (multiplayer-aware)
var closestPlayer = GM.Core?.GetClosestPlayer(position, PlayerInclusionMode.AliveOrDead, maxRange, true);

// Stage access
var stage = GM.Core?._stage;
var spawnedEnemies = stage?._spawnedEnemies;

// Multiplayer-specific patterns (v1.0+)
if (GM.Core.IsOnlineMultiplayer)
{
    // Network-aware modifications
    var players = GM.Core._multiplayer.GetPlayers();
    // Host authority and synchronization required
}
```

## Multiplayer Considerations (v1.0+)

### Multiplayer Detection

Based on code analysis:

```csharp
// Check if any multiplayer mode is active
if (GM.Core._multiplayer?.IsMultiplayer == true)
{
    // Multiplayer-specific logic
}

// Check specific multiplayer types
if (GM.Core.IsOnlineMultiplayer)
{
    // Network multiplayer with latency and host authority
    var players = GM.Core._multiplayer.GetPlayers();
    foreach (var player in players)
    {
        var latency = player.PlayerInfo?.AverageLatencyMs ?? 0;
        // Network-aware logic required
    }
}
else if (GM.Core.IsLocalMultiplayer)
{
    // Local coop - shared screen, no network concerns
    var playerCount = GM.Core._multiplayer.GetPlayerCount();
    // Handle local multiplayer logic
}
else
{
    // Single player mode
}
```

### Multiplayer Modding Requirements

Based on code analysis:
- Check multiplayer mode before modifying game state
- Online multiplayer appears to require network synchronization awareness
- Local multiplayer appears to have different camera and UI considerations
- Host authority: Based on code, only the host should modify certain game states in online play
- Test in all modes: Single player, local coop, and online multiplayer

### Multiplayer Manager Access

Based on decompiled code:

```csharp
var multiplayerManager = GM.Core._multiplayer;
if (multiplayerManager != null)
{
    var isMultiplayer = multiplayerManager.IsMultiplayer;
    var players = multiplayerManager.GetPlayers();
    var playerCount = multiplayerManager.GetPlayerCount();
    var coopConfig = multiplayerManager.CoopConfig; // Extensive configuration
}
```

## Timing Considerations

Based on code analysis:
- **Startup**: GM.Core appears to be null
- **Menu**: GM.Core remains null, data loaded
- **Game Session**: GM.Core available after `GameManager.Awake()`
- **In-Game**: System access through GM.Core

## Integration Points

Based on code analysis:
- **Data**: Hook `DataManager` or access through `GM.Core.DataManager`
- **Gameplay**: Hook `GameManager.Construct()` or `AddStartingWeapon()`
- **UI**: Access `GameManager.MainUI`
- **Saves**: Hook `SaveSerializer` methods
- **Players**: Use `GM.Core.AllPlayers` or `GM.Core.GetClosestPlayer()`
- **Multiplayer**: Check `IsOnlineMultiplayer`/`IsLocalMultiplayer` before modifications
- **New Systems**: Access appears to be via dependency injection or through GameManager references

## Architecture Stability

Based on code analysis, core patterns in v1.0+ appear to be:

- **GM.Core** static accessor pattern - Primary access method
- **Zenject** dependency injection - Systems appear to use constructor injection
- **SignalBus** event system - Decoupled communication
- **Manager relationships** - Core manager hierarchy
- **Data loading sequences** - DataManager patterns
- **Single player core mechanics** - Base gameplay systems

Based on code structure, existing mod compatibility appears to require:
1. Adding multiplayer mode checks
2. Handling new dependencies if needed
3. Testing in all game modes (single, local coop, online)

## Thread Safety

Based on code analysis, game systems appear to not be thread-safe. Modify only on main Unity thread.

## Architecture Patterns

### Base Classes
Based on decompiled code:
- `GameMonoBehaviour` - Base for GameManager, Stage
- `GameTickable` - Base for tick-based systems (ArcanaManager)

### Facades
Based on code analysis:
- `WeaponsFacade` - Weapon system
- `AccessoriesFacade` - Accessories
- `BackendFacade` - Platform backend

### Factories
Based on decompiled code:
- `LevelUpFactory` - Level-up components
- `CharacterFactory` - Characters
- `TreasureFactory` - Treasures
- `ProjectileFactory` - Projectiles
- `ShopFactory` - Shops and merchants with multiplayer support
- `FontFactory` - Font asset management