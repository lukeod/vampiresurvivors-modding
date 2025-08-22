# Stage System

Manages enemy spawning, environmental effects, and gameplay progression through dynamic enemy placement and spawn patterns.

## Core Stage Architecture

### Stage Class
Primary controller for stage mechanics and enemy spawning:

```csharp
// Key properties for stage management
public List<EnemyController> SpawnedEnemies;     // Currently active enemies
public SpawnType SpawnType;                      // Current spawn behavior mode
public Rect SpawnOuterRect;                      // Outer boundary for spawning
public Rect SpawnInnerRect;                      // Inner safe zone (no spawning)
public List<Vector2> EnemySpawnLocations;        // Predefined spawn points
public float _effectiveSpawnFrequency;           // Current spawn rate
```

### Stage Data Storage
Stage configurations are stored in the DataManager:

```csharp
public JObject _allStagesJson;      // Raw JSON stage data
public JObject _allStageSetJson;    // Stage set configurations
```

Access processed stage data through:
```csharp
var stages = dataManager.GetConvertedStages();  // Dictionary<StageType, List<StageData>>
```

## Enemy Spawning System

### Core Spawning Methods
The Stage class provides multiple spawning methods for different scenarios:

```csharp
// Primary enemy spawning
public GameObject SpawnEnemy(EnemyType enemyType, Vector2 spawnPos, bool asRemote = false, bool forceSpawn = false)
public T SpawnEnemy<T>(EnemyType enemyType, Vector2 spawnPos, bool asRemote = false, bool forceSpawn = false) where T : EnemyController

// Additional spawn location methods
public GameObject SpawnEnemyInOuterRect(EnemyType enemyType, bool checkWalls = false, bool forceSpawn = false)

// Special enemy spawning
public void SpawnBoss()
public void SpawnBatGoblin()
public void SpawnEnemyBullet(Vector2 spawnPos, EnemyType bulletType = EnemyType.BULLET_1)

// Additional spawn management methods
public void SetSpawnType(SpawnType type)

// Internal spawning logic
public EnemyController SpawnEnemyUnit(ObjectPool pool, EnemyType enemyType, Vector2 spawnPos, bool asRemote)
```

### Spawn Pattern Methods
Different spawn patterns are implemented through specialized methods:

```csharp
// Pattern-based spawning (internal implementation)
private bool SpawnEnemiesInOuterRect()
private void SpawnEnemiesInRandomLocationHorizontal()
private void SpawnEnemiesInRandomLocationVertical()
private void SpawnEnemiesTiled()
private void SpawnEnemiesMapped()

// Main spawning controller
public void HandleSpawning(bool checkMaxEnemyCount = true)
```

### Spawn Types
Different spawn behaviors are controlled through the `SpawnType` enum:
- **STANDARD**: Default spawning behavior at random valid locations
- **HORIZONTAL**: Spawning focused on horizontal patterns
- **VERTICAL**: Spawning focused on vertical patterns
- **SCRIPTED**: Controlled by script events and special conditions
- **TILED**: Grid-based spawning patterns using tilemap data
- **MAPPED**: Using predefined spawn locations from EnemySpawnLocations

## Enemy Query and Management

### Enemy Search Methods
The stage system provides efficient methods for finding and managing enemies:

```csharp
// Distance-based enemy queries
public EnemyController FindClosestEnemy(Vector3 queryPos, bool excludeDead = false, float maxRange = float.MaxValue)
public List<EnemyController> GetClosestEnemiesSorted(Vector3 queryPos, bool excludeDead = false, float maxRange = float.MaxValue)
public List<EnemyController> GetEnemiesInCircle(float2 position, float radius)

// Random enemy selection (all methods now take optional Unity.Mathematics.Random for deterministic results)
public Transform PickRandomEnemy(Il2CppSystem.Nullable<Unity.Mathematics.Random> rng)
public Transform PickRandomEnemyInScreenBounds(Il2CppSystem.Nullable<Unity.Mathematics.Random> rng)
public Transform PickRandomEnemyInRectBounds(Rectangle _rect, Il2CppSystem.Nullable<Unity.Mathematics.Random> rng)
public Transform PickRandomEnemyInCircle(float2 position, float radius, Il2CppSystem.Nullable<Unity.Mathematics.Random> rng)
public EnemyController PickRandomEnemyFromList(IList<EnemyController> enemiesList, Il2CppSystem.Nullable<Unity.Mathematics.Random> rng)

// Screen-based queries
public List<EnemyController> GetAllEnemiesInScreenBounds()
public List<EnemyController> GetAllEnemiesInScreenBounds(float excludedBorderPercentage01)
```

### Performance Monitoring
Includes Unity Profiler markers for performance analysis:

```csharp
public static ProfilerMarker MarkerSpawnEnemy;
public static ProfilerMarker MarkerFindClosestEnemy;
public static ProfilerMarker MarkerHandleSpawning;
public static ProfilerMarker MarkerSpawnEnemyUnit;
public static ProfilerMarker MarkerSpawnEnemyResolve;
public static ProfilerMarker MarkerUpdateCulling;
public static ProfilerMarker MarkerDespawnEnemyIfOutsideRect;
```

## Stage Event Management

### StageEventManager
Handles stage-specific events beyond basic spawning:
- Timed events and special encounters
- Stage transitions and boss triggers
- Dynamic difficulty adjustments
- Environmental effects

### Event-Driven Gameplay
Stages trigger boss spawning, environmental hazards, special enemy waves, and stage-specific mechanics.

## Spawn Boundaries and Zones

### Spawn Rectangle System
The stage uses a dual-rectangle system for spawn management:

```csharp
public Rect SpawnOuterRect;  // Maximum spawn boundary
public Rect SpawnInnerRect;  // Player safe zone (no spawning)
```

### Zone-Based Spawning
- **Outer Zone**: Valid spawning area around the player
- **Inner Zone**: Safe area immediately around the player
- **Off-Screen**: Areas outside the visible screen bounds
- **Mapped Locations**: Predefined strategic spawn points

## Dynamic Spawn Control

### Spawn Frequency Management
```csharp
public float _effectiveSpawnFrequency;  // Current spawn rate
```

The spawn frequency can be modified dynamically based on:
- Game progression and time
- Player performance and difficulty scaling
- Stage-specific requirements
- Special events and boss encounters

### Adaptive Spawning
The system can adapt spawning behavior based on:
- Current enemy count on screen
- Player position and movement patterns
- Performance requirements
- Difficulty curve progression

## Stage Progression

### Stage Transitions
Stages can transition between different phases:
- Early game with basic enemies
- Mid-game with increased complexity
- Late game with boss encounters
- Survival mode with maximum difficulty

### Boss Management
Special handling for boss encounters:
```csharp
public void SpawnBoss()  // Triggers boss spawning logic
```

Boss spawning typically involves:
- Clearing or reducing regular enemy spawns
- Special spawn locations and patterns
- Modified game mechanics during boss fights

## Stage-Specific Data

### Stage Configuration
Each stage has specific configuration data including:
- Available enemy types for spawning through `poolsMapping`
- Spawn pattern preferences via `spawnType` property
- Environmental settings and tileset configuration
- Victory/progression conditions and time limits
- Destructible environment settings (`destructibleType`, `destructibleFreq`, `destructibleChance`)
- Stage-specific events list for timed encounters
- Background music and visual texture assignments
- Lighting configuration (`hasLights`, `disableGlobalLight`)
- Merchant and speedup restrictions (`isMerchantBanned`, `isSpeedupBanned`)

### DLC Stage Support
The system supports DLC stages through the DataManager:
```csharp
public Dictionary<DlcType, Dictionary<StageType, List<StageData>>> _dlcStageData;
public Dictionary<DlcType, Dictionary<StageType, List<StageData>>> AllDlcStageData { get; }
```

## Performance Considerations

Stage systems involve performance-critical operations. Avoid hooking high-frequency methods, use efficient spatial queries, cache calculations, and batch operations.

## Common Modding Scenarios

### Custom Spawn Patterns
```csharp
[HarmonyPatch(typeof(Stage), "HandleSpawning")]
[HarmonyPrefix]
public static bool CustomSpawnPattern(Stage __instance, bool checkMaxEnemyCount)
{
    // Implement custom spawning logic
    // Return false to skip original method
    return false;
}
```

### Modified Spawn Rates
```csharp
// Hook into spawn frequency changes by modifying the property directly
[HarmonyPatch(typeof(Stage), "HandleSpawning")]
[HarmonyPrefix]
public static void ModifySpawnRate(Stage __instance, bool forceSpawn)
{
    __instance._effectiveSpawnFrequency *= 2.0f;  // Double spawn rate
}
```

### Custom Boss Spawning
```csharp
[HarmonyPatch(typeof(Stage), "SpawnBoss")]
[HarmonyPostfix]
public static void CustomBossSpawn(Stage __instance)
{
    // Add custom boss spawn logic
    // Spawn additional enemies or modify boss behavior
}
```

### Stage-Specific Enemy Types
```csharp
// Modify available enemies for specific stages
[HarmonyPatch(typeof(Stage), "SpawnEnemy")]
[HarmonyPrefix]
public static bool CustomEnemySelection(ref EnemyType enemyType, Vector2 spawnPos, ref bool asRemote, ref bool forceSpawn)
{
    // Replace enemy type based on custom logic
    if (ShouldSpawnCustomEnemy())
    {
        enemyType = EnemyType.CUSTOM_ENEMY;
    }
    return true;
}
```

## Enemy Culling and Cleanup

### Automatic Culling
Includes automatic enemy culling through the private `UpdateCulling()` method. Enemies outside boundaries are despawned, off-screen enemies culled for performance, and dead enemies cleaned up automatically.

### Manual Enemy Management
```csharp
// Access spawned enemies for custom management
var activeEnemies = stage.SpawnedEnemies;
foreach (var enemy in activeEnemies)
{
    // Custom enemy logic or cleanup
}

// Additional enemy query methods
public EnemyController ClosestAlive(Vector3 queryPos, float maxRange = float.MaxValue)
public void GetEnemyBodiesInRect(Rectangle _rect, ref List<BaseBody> list)
```

## Testing and Debugging

### Spawn Validation
When modifying stage systems:
1. Test spawn patterns across different stage types
2. Verify enemy counts remain reasonable
3. Check for spawn location conflicts
4. Monitor performance impact of changes

### Common Issues
1. **Spawn overflow**: Too many enemies causing performance issues
2. **Invalid spawn locations**: Enemies spawning inside walls or obstacles
3. **Unbalanced difficulty**: Modified spawn rates breaking game balance
4. **Memory leaks**: Improper enemy cleanup causing memory issues

## Advanced Stage Features

### Stage Data Access Patterns
The stage system uses several access patterns for configuration:

```csharp
// Access stage data through DataManager
var stageData = dataManager.GetConvertedStages(); // Returns Dictionary<StageType, List<StageData>>

// DLC stages have separate access
var dlcStages = dataManager.AllDlcStageData; // Dictionary<DlcType, Dictionary<StageType, List<StageData>>>

// Stage-specific properties from StageData
stageData.poolsMapping;      // Enemy pool configuration
stageData.events;           // List of timed stage events  
stageData.spawnType;        // Default spawn behavior
stageData.destructibleType; // Environmental destructibles
```

### Conditional Spawning
Implement spawning based on game state:
- Player level and equipment
- Time progression
- Current enemy composition
- Special game events

### Environmental Integration
Stages can interact with environmental systems:
- Destructible environments
- Moving platforms or obstacles
- Dynamic lighting and visual effects
- Interactive stage elements

## Key Changes for Modders

### Method Signature Updates
Several important method signatures have been updated:

- `SpawnEnemy()` methods now include optional `asRemote` and `forceSpawn` parameters
- `SpawnEnemyUnit()` now includes an `asRemote` parameter
- All `PickRandomEnemy*()` methods now take an optional `Unity.Mathematics.Random` parameter for deterministic randomization
- `HandleSpawning()` is now public instead of private

### New Methods
- `SpawnEnemyInOuterRect()` provides additional spawn location control
- `PickRandomEnemyFromList()` for selecting from custom enemy lists

### Updated Random System
The stage system now uses Unity.Mathematics.Random for more predictable and controllable randomization. When hooking into random enemy selection methods, you can pass a specific Random instance to ensure deterministic behavior across different game sessions.