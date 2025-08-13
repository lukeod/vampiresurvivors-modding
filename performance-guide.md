# Performance Guide Documentation

## Overview
Vampire Survivors is built with performance-critical systems that handle hundreds of entities simultaneously. Understanding these systems and their performance characteristics is essential for creating mods that don't degrade the gameplay experience.

## Core Performance Architecture

### Hybrid Tick/Update System Architecture
The game uses both Unity's standard Update() pattern and a custom tick-based system for different components:

```csharp
// Performance-critical systems inherit from GameTickable
public class GameTickable : Il2CppSystem.Object
{
    public virtual void Tick();     // Entry point for tick processing
    public virtual void OnTick();   // Override point for tick logic
}

// Main tick coordinator
GameManager.OnTickerCallback()  // Orchestrates GameTickable system ticks
```

**Critical Performance Note**: The tick system processes performance-critical systems like EnemiesManager every frame. Standard Unity Update() is still used for UI and less critical components. Only core framework systems inherit from GameTickable (EnemiesManager, ArcanaManager, PixelFontManager, GoldFeverController). Hooking into `OnTick()` methods should be avoided at all costs.

**GameTickable System Details**: 
- Located in PauseSystem.dll namespace
- Provides `Tick()` and `OnTick()` virtual methods
- Used only for core game systems, not weapons or individual entities
- Integrates with the pause system for coordinated updates

### Unity Job System Integration
The game leverages Unity's Job System for heavy computational tasks:

```csharp
// Enemy processing using Unity Jobs
EnemyVelocityCalcJob : Il2CppSystem.ValueType
{
    NativeArray<float3> _positionArray       // Enemy positions
    NativeArray<float3> _velocityArray       // Calculated velocities
    NativeArray<float> _speedArray           // Movement speeds
    NativeArray<bool> _fixedDirectionArray   // Fixed direction flags
    NativeArray<float3> _currentDirectionArray // Current movement directions
    NativeArray<float3> _targetArray         // Target positions
    void Execute(int index)                  // Job execution per enemy
}
```

**Extreme Performance Warning**: Job System operations process thousands of entities per frame using native arrays for bulk data processing. These are the most expensive operations in the game and should never be hooked.

### Object Pooling System
Extensive use of `Il2CppObjectPool` throughout the game for managing IL2CPP object references:
- All game objects use Il2CppObjectPool.Get<T>() for safe reference retrieval
- Enemies, projectiles, weapons, and characters
- Visual effects and UI components
- Data management objects and collections

**Best Practice**: The Il2CppObjectPool is primarily for IL2CPP interop safety, not traditional object pooling. Avoid creating unnecessary new objects during gameplay, but understand this pattern is for safe IL2CPP object access.

## Performance-Critical Methods (NEVER HOOK)

### Extremely Expensive Operations
These methods are called every frame for hundreds of objects:

```csharp
// NEVER HOOK THESE - WILL DESTROY PERFORMANCE
EnemiesManager.OnTick()           // Master enemy processing tick
EnemiesManager.RunMovementJob()   // Unity Job System enemy movement
GameManager.OnTickerCallback()   // Master game tick coordinator
Weapon.InternalUpdate()           // Per-weapon processing
Projectile.OnUpdate()             // Per-projectile movement
CharacterController.OnUpdate()   // Per-character processing
```

### Moderately Expensive Operations
Use extreme caution when hooking these:

```csharp
// USE WITH EXTREME CAUTION
Stage.HandleSpawning()            // Enemy spawn calculations (has ProfilerMarker)
Stage.FindClosestEnemy()          // Distance calculations across all enemies (has ProfilerMarker)
Weapon.DealDamage()              // Damage calculation pipeline
UI component Update() methods     // UI updates and text rendering
Physics and collision methods     // Unity physics system calls
```

### Safe Operations
These can be hooked with minimal performance impact:

```csharp
// SAFE TO HOOK
DataManager.LoadBaseJObjects()    // One-time data loading
GameManager.AddStartingWeapon()   // Game initialization
Character selection methods       // Menu interactions
Save/load operations             // File I/O (not frame-dependent)
Achievement unlock methods       // Infrequent events
```

## Performance Monitoring Tools

### Unity Profiler Markers
The game includes built-in profiler markers for performance analysis:

```csharp
// Stage-related performance markers (in Stage.cs)
ProfilerMarker MarkerSpawnEnemy
ProfilerMarker MarkerFindClosestEnemy
ProfilerMarker MarkerHandleSpawning
ProfilerMarker MarkerSpawnEnemyUnit
ProfilerMarker MarkerSpawnEnemyResolve
ProfilerMarker MarkerUpdateCulling
ProfilerMarker MarkerDespawnEnemyIfOutsideRect

// System-specific markers
EnemiesManager.s_onTickMarker        // EnemiesManager tick performance
PixelFontManager.MarkerOnTextChanged // Text rendering performance
DataManager.MarkerReloadAllData      // Data loading operations
DataManager.MarkerLoadDataFromJson   // JSON parsing performance
DataManager.MarkerBuildConvertedData // Data conversion performance
DataManager.MarkerLoadBaseJObjects   // Base object loading
Blitter.s_updateMarker              // Graphics blitting performance
AdventureManager.MarkerInitAdventure // Adventure initialization
AdventureManager.MarkerInitDataManager // DataManager initialization
```

**Usage**: Monitor these markers to understand performance impact of modifications.

## Memory Management

### IL2CPP Garbage Collection
IL2CPP has different garbage collection characteristics than standard .NET:
- Reduced GC pressure compared to Mono
- Still sensitive to excessive allocations
- String operations can be particularly expensive

### Memory-Efficient Patterns
```csharp
// GOOD: Reuse objects and cache references
private static readonly Dictionary<WeaponType, WeaponData> _weaponCache = new();

// BAD: Repeated allocations in hot paths
public void BadUpdate()
{
    var weapons = new List<WeaponData>();  // Allocates every frame
    foreach (var weapon in GetWeapons())   // Multiple allocations
    {
        // Processing
    }
}

// GOOD: Cache and reuse collections
private static readonly List<WeaponData> _reusableWeaponList = new();

public void GoodUpdate()
{
    _reusableWeaponList.Clear();  // Reuse existing list
    GetWeapons(_reusableWeaponList);  // Fill existing list
    // Processing
}
```

## Recommended Caching Strategies

### 1. Cache Expensive Calculations
```csharp
public static class WeaponStatCache
{
    private static readonly Dictionary<(WeaponType, int), float> _powerCache = new();
    
    public static float GetCachedPower(WeaponType weapon, int level)
    {
        var key = (weapon, level);
        if (!_powerCache.TryGetValue(key, out float power))
        {
            power = CalculateExpensivePower(weapon, level);
            _powerCache[key] = power;
        }
        return power;
    }
}
```

### 2. Cache Frequently Accessed References
```csharp
public static class GameReferences
{
    private static GameManager _gameManager;
    private static DataManager _dataManager;
    
    public static GameManager GameManager
    {
        get
        {
            if (_gameManager == null)
                _gameManager = GM.Core;
            return _gameManager;
        }
    }
    
    public static DataManager DataManager
    {
        get
        {
            if (_dataManager == null)
                _dataManager = GameManager?.DataManager;
            return _dataManager;
        }
    }
}
```

### 3. Pre-compute Lookup Tables
```csharp
public static class DamageCalculator
{
    private static readonly float[,] _damageTable = new float[500, 8];  // [WeaponType, Level]
    
    static DamageCalculator()
    {
        // Pre-compute all damage values during initialization
        PrecomputeDamageTable();
    }
    
    public static float GetDamage(WeaponType weapon, int level)
    {
        return _damageTable[(int)weapon, level - 1];  // O(1) lookup
    }
}
```

## Batch Operation Patterns

### 1. Group Similar Modifications
```csharp
// BAD: Individual modifications
foreach (var weapon in weapons)
{
    ModifyWeapon(weapon);  // Multiple separate operations
}

// GOOD: Batched modifications
var modifications = new List<WeaponModification>();
foreach (var weapon in weapons)
{
    modifications.Add(CreateModification(weapon));
}
ApplyBatchedModifications(modifications);  // Single batch operation
```

### 2. Minimize IL2CPP Boundary Crossings
```csharp
// BAD: Frequent crossings between managed and IL2CPP
foreach (var enemy in il2cppEnemyList)
{
    var health = enemy.Health;        // IL2CPP call
    var maxHealth = enemy.MaxHealth;  // IL2CPP call
    ProcessEnemy(health, maxHealth);
}

// GOOD: Bulk data extraction
var healthData = ExtractHealthData(il2cppEnemyList);  // Single IL2CPP interaction
foreach (var data in healthData)
{
    ProcessEnemy(data.Health, data.MaxHealth);  // Pure managed code
}
```

## Job System Integration Guidelines

### Working with Unity Jobs
If you need to integrate with the existing job system:

```csharp
// Respect existing job patterns
public struct CustomEnemyJob : IJobParallelFor
{
    [ReadOnly] public NativeArray<float3> positions;
    public NativeArray<float3> results;
    
    public void Execute(int index)
    {
        // Simple, fast operations only
        results[index] = ProcessPosition(positions[index]);
    }
}
```

### Job System Best Practices
1. **Keep jobs simple**: Complex logic should be in managed code
2. **Use burst compilation**: Mark jobs with `[BurstCompile]` when possible
3. **Minimize dependencies**: Avoid complex job chains
4. **Proper disposal**: Always dispose NativeArrays

## Performance Testing Guidelines

### Measuring Performance Impact
```csharp
public static class PerformanceTracker
{
    private static readonly Stopwatch _stopwatch = new();
    
    public static void MeasureOperation(string operationName, Action operation)
    {
        _stopwatch.Restart();
        operation();
        _stopwatch.Stop();
        
        if (_stopwatch.ElapsedMilliseconds > 1)  // Only log expensive operations
        {
            MelonLogger.Warning($"{operationName} took {_stopwatch.ElapsedMilliseconds}ms");
        }
    }
}
```

### Frame Time Monitoring
```csharp
public class FrameTimeMonitor : MelonMod
{
    private float _lastFrameTime;
    private readonly Queue<float> _frameTimes = new(60);
    
    public override void OnUpdate()
    {
        float currentTime = Time.time;
        float deltaTime = currentTime - _lastFrameTime;
        
        _frameTimes.Enqueue(deltaTime);
        if (_frameTimes.Count > 60)
            _frameTimes.Dequeue();
            
        if (deltaTime > 0.033f)  // 30 FPS threshold
        {
            MelonLogger.Warning($"Frame spike: {deltaTime * 1000:F1}ms");
        }
        
        _lastFrameTime = currentTime;
    }
}
```

## Common Performance Anti-Patterns

### 1. String Operations in Hot Paths
```csharp
// BAD: String concatenation in Update loops
public void BadUpdate()
{
    string status = "Health: " + health + "/" + maxHealth;  // Allocates every frame
    UpdateUI(status);
}

// GOOD: StringBuilder or cached strings
private readonly StringBuilder _stringBuilder = new();

public void GoodUpdate()
{
    _stringBuilder.Clear();
    _stringBuilder.Append("Health: ").Append(health).Append("/").Append(maxHealth);
    UpdateUI(_stringBuilder.ToString());
}
```

### 2. Unnecessary LINQ Operations
```csharp
// BAD: LINQ in performance-critical code
public void BadProcessing()
{
    var activeEnemies = enemies.Where(e => e.IsActive).ToList();  // Allocates every call
    var closestEnemy = activeEnemies.OrderBy(e => Vector3.Distance(player.position, e.position)).First();
}

// GOOD: Direct iteration and comparison
public EnemyController GoodProcessing()
{
    EnemyController closest = null;
    float closestDistance = float.MaxValue;
    
    foreach (var enemy in enemies)
    {
        if (!enemy.IsActive) continue;
        
        float distance = Vector3.SqrMagnitude(player.position - enemy.position);  // Cheaper than Distance
        if (distance < closestDistance)
        {
            closestDistance = distance;
            closest = enemy;
        }
    }
    
    return closest;
}
```

### 3. Excessive Null Checking
```csharp
// BAD: Repeated null checks
public void BadNullHandling()
{
    if (GM.Core != null && GM.Core.DataManager != null && GM.Core.DataManager.GetConvertedWeapons() != null)
    {
        // Process weapons
    }
}

// GOOD: Cached references with single validation
private static DataManager _cachedDataManager;

public static DataManager GetDataManager()
{
    if (_cachedDataManager == null)
        _cachedDataManager = GM.Core?.DataManager;
    return _cachedDataManager;
}
```

## Advanced Performance Techniques

### 1. Lazy Initialization Patterns
```csharp
public class LazyGameData
{
    private Dictionary<WeaponType, WeaponData> _weaponCache;
    
    public Dictionary<WeaponType, WeaponData> WeaponCache
    {
        get
        {
            if (_weaponCache == null)
            {
                _weaponCache = InitializeWeaponCache();
            }
            return _weaponCache;
        }
    }
}
```

### 2. Update Frequency Reduction
```csharp
public class ThrottledUpdater
{
    private float _lastUpdateTime;
    private const float UPDATE_INTERVAL = 0.1f;  // Update every 100ms instead of every frame
    
    public void ConditionalUpdate()
    {
        if (Time.time - _lastUpdateTime < UPDATE_INTERVAL)
            return;
            
        _lastUpdateTime = Time.time;
        PerformActualUpdate();
    }
}
```

### 3. Spatial Partitioning for Large Datasets
```csharp
public class SpatialGrid<T>
{
    private readonly Dictionary<int, List<T>> _grid = new();
    private readonly float _cellSize;
    
    public void AddItem(T item, Vector2 position)
    {
        int cellKey = GetCellKey(position);
        if (!_grid.TryGetValue(cellKey, out var cell))
        {
            cell = new List<T>();
            _grid[cellKey] = cell;
        }
        cell.Add(item);
    }
    
    public List<T> GetNearbyItems(Vector2 position, float radius)
    {
        // Only check nearby cells instead of all items
        // Dramatically reduces search complexity
    }
}
```

## Performance Debugging Tools

### Custom Profiling
```csharp
public static class CustomProfiler
{
    private static readonly Dictionary<string, ProfilerData> _profiles = new();
    
    public static IDisposable Profile(string name)
    {
        return new ProfilerScope(name);
    }
    
    private class ProfilerScope : IDisposable
    {
        private readonly string _name;
        private readonly Stopwatch _stopwatch;
        
        public ProfilerScope(string name)
        {
            _name = name;
            _stopwatch = Stopwatch.StartNew();
        }
        
        public void Dispose()
        {
            _stopwatch.Stop();
            RecordProfile(_name, _stopwatch.ElapsedTicks);
        }
    }
}

// Usage
using (CustomProfiler.Profile("Weapon Processing"))
{
    ProcessWeapons();
}
```

## Architecture-Specific Performance Considerations

### IL2CPP Object Reference Management
The game uses extensive IL2CPP interop patterns that affect performance:

```csharp
// Standard IL2CPP object access pattern
public unsafe GameManager _gameManager
{
    get
    {
        nint num = (nint)IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this) + 
                   (int)IL2CPP.il2cpp_field_get_offset(NativeFieldInfoPtr__gameManager);
        System.IntPtr intPtr = *(System.IntPtr*)num;
        return (intPtr != (System.IntPtr)0) ? Il2CppObjectPool.Get<GameManager>(intPtr) : null;
    }
}
```

**Performance Impact**: Every property access involves IL2CPP native calls and pointer arithmetic. Cache frequently accessed references in managed code.

### Update vs InternalUpdate vs OnTick Patterns
The game uses different update patterns for different systems:

```csharp
// GameTickable systems (performance-critical)
EnemiesManager : GameTickable -> OnTick()
ArcanaManager : GameTickable -> OnTick() 
PixelFontManager : GameTickable -> OnTick()

// GameMonoBehaviour systems (standard Unity)
Weapon : GameMonoBehaviour -> InternalUpdate()
Projectile : GameMonoBehaviour -> OnUpdate()
CharacterController : GameMonoBehaviour -> OnUpdate()

// Standard MonoBehaviour (UI and non-critical)
Various UI components -> Update()
```

**Guideline**: Respect these patterns. GameTickable is reserved for core game systems that need coordinated updates.

### Decompiled Code Limitations
**Important**: This guide is based on decompiled IL2CPP code which has limitations:
- Method implementations are not fully visible (only signatures and IL2CPP interop code)
- Actual performance characteristics may differ from apparent complexity
- Always profile actual performance rather than assuming from decompiled signatures

Remember: Performance optimization should be data-driven. Always measure before and after changes to validate improvements.