# Performance Guide

## Architecture Overview

Vampire Survivors processes hundreds of entities using hybrid tick/update systems and Unity Jobs for computational workloads.

### Core Systems

**GameTickable System**
```csharp
public class GameTickable : Il2CppSystem.Object
{
    public virtual void Tick();     // Entry point for tick processing
    public virtual void OnTick();   // Override point for tick logic
}

GameManager.OnTickerCallback()  // Orchestrates system ticks
```

Core systems: EnemiesManager, ArcanaManager, PixelFontManager, GoldFeverController

**Unity Job System**
```csharp
EnemyVelocityCalcJob : Il2CppSystem.ValueType
{
    NativeArray<float3> _positionArray
    NativeArray<float3> _velocityArray
    NativeArray<float> _speedArray
    NativeArray<bool> _fixedDirectionArray
    NativeArray<float3> _currentDirectionArray
    NativeArray<float3> _targetArray
    void Execute(int index)
}
```

**Object Pooling**
All game objects use `Il2CppObjectPool.Get<T>()` for IL2CPP interop safety.

## Method Performance Classification

**Avoid Hooking - High Impact**
```csharp
EnemiesManager.OnTick()           // Master enemy processing
EnemiesManager.RunMovementJob()   // Unity Job System movement
GameManager.OnTickerCallback()   // Master game tick
Weapon.InternalUpdate()           // Per-weapon processing
Projectile.OnUpdate()             // Per-projectile movement
CharacterController.OnUpdate()   // Per-character processing
```

**Caution - Moderate Impact**
```csharp
Stage.HandleSpawning()            // Enemy spawn calculations
Stage.FindClosestEnemy()          // Distance calculations
Weapon.DealDamage()              // Damage pipeline
```

**Safe - Low Impact**
```csharp
DataManager.LoadBaseJObjects()    // One-time data loading
GameManager.AddStartingWeapon()   // Game initialization
Character selection methods       // Menu interactions
Save/load operations             // File I/O operations
Achievement unlock methods       // Infrequent events
```

## Performance Monitoring

**Built-in Profiler Markers**
```csharp
// Stage operations
ProfilerMarker MarkerSpawnEnemy
ProfilerMarker MarkerFindClosestEnemy
ProfilerMarker MarkerHandleSpawning
ProfilerMarker MarkerUpdateCulling

// System operations
EnemiesManager.s_onTickMarker
PixelFontManager.MarkerOnTextChanged
DataManager.MarkerReloadAllData
DataManager.MarkerLoadDataFromJson
Blitter.s_updateMarker
AdventureManager.MarkerInitAdventure
```

## Memory Management

**IL2CPP Characteristics**
- Reduced GC pressure compared to Mono
- Sensitive to excessive allocations
- String operations are expensive

**Memory-Efficient Patterns**
```csharp
// Cache references
private static readonly Dictionary<WeaponType, WeaponData> _weaponCache = new();

// Reuse collections
private static readonly List<WeaponData> _reusableWeaponList = new();

public void ProcessWeapons()
{
    _reusableWeaponList.Clear();
    GetWeapons(_reusableWeaponList);
    // Process reused list
}
```

## Caching Strategies

**Expensive Calculations**
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

**Reference Caching**
```csharp
public static class GameReferences
{
    private static GameManager _gameManager;
    private static DataManager _dataManager;
    
    public static GameManager GameManager => 
        _gameManager ??= GM.Core;
    
    public static DataManager DataManager => 
        _dataManager ??= GameManager?.DataManager;
}
```

**Lookup Tables**
```csharp
public static class DamageCalculator
{
    private static readonly float[,] _damageTable = new float[500, 8];
    
    static DamageCalculator() => PrecomputeDamageTable();
    
    public static float GetDamage(WeaponType weapon, int level) =>
        _damageTable[(int)weapon, level - 1];
}
```

## Batch Operations

**Group Modifications**
```csharp
var modifications = new List<WeaponModification>();
foreach (var weapon in weapons)
{
    modifications.Add(CreateModification(weapon));
}
ApplyBatchedModifications(modifications);
```

**Minimize IL2CPP Boundary Crossings**
```csharp
// Extract data in bulk
var healthData = ExtractHealthData(il2cppEnemyList);
foreach (var data in healthData)
{
    ProcessEnemy(data.Health, data.MaxHealth);  // Pure managed code
}
```

## Unity Jobs Integration

**Job Structure**
```csharp
public struct CustomEnemyJob : IJobParallelFor
{
    [ReadOnly] public NativeArray<float3> positions;
    public NativeArray<float3> results;
    
    public void Execute(int index)
    {
        results[index] = ProcessPosition(positions[index]);
    }
}
```

**Best Practices**
- Keep jobs simple - complex logic in managed code
- Use `[BurstCompile]` when possible
- Minimize job dependencies
- Always dispose NativeArrays

## Performance Testing

**Operation Measurement**
```csharp
public static class PerformanceTracker
{
    private static readonly Stopwatch _stopwatch = new();
    
    public static void MeasureOperation(string operationName, Action operation)
    {
        _stopwatch.Restart();
        operation();
        _stopwatch.Stop();
        
        if (_stopwatch.ElapsedMilliseconds > 1)
        {
            MelonLogger.Warning($"{operationName} took {_stopwatch.ElapsedMilliseconds}ms");
        }
    }
}
```

**Frame Time Monitoring**
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

## Optimized Patterns

**String Operations**
```csharp
private readonly StringBuilder _stringBuilder = new();

public void UpdateHealthDisplay()
{
    _stringBuilder.Clear();
    _stringBuilder.Append("Health: ").Append(health).Append("/").Append(maxHealth);
    UpdateUI(_stringBuilder.ToString());
}
```

**Enemy Processing**
```csharp
public EnemyController FindClosestEnemy()
{
    EnemyController closest = null;
    float closestDistance = float.MaxValue;
    
    foreach (var enemy in enemies)
    {
        if (!enemy.IsActive) continue;
        
        float distance = Vector3.SqrMagnitude(player.position - enemy.position);
        if (distance < closestDistance)
        {
            closestDistance = distance;
            closest = enemy;
        }
    }
    
    return closest;
}
```

**Reference Validation**
```csharp
private static DataManager _cachedDataManager;

public static DataManager GetDataManager() =>
    _cachedDataManager ??= GM.Core?.DataManager;
```

## Advanced Techniques

**Lazy Initialization**
```csharp
public class LazyGameData
{
    private Dictionary<WeaponType, WeaponData> _weaponCache;
    
    public Dictionary<WeaponType, WeaponData> WeaponCache =>
        _weaponCache ??= InitializeWeaponCache();
}
```

**Update Throttling**
```csharp
public class ThrottledUpdater
{
    private float _lastUpdateTime;
    private const float UPDATE_INTERVAL = 0.1f;
    
    public void ConditionalUpdate()
    {
        if (Time.time - _lastUpdateTime < UPDATE_INTERVAL)
            return;
            
        _lastUpdateTime = Time.time;
        PerformActualUpdate();
    }
}
```

**Spatial Partitioning**
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
        // Check nearby cells only - reduces search complexity
        return ProcessNearbyCells(position, radius);
    }
}
```

## Custom Profiling

```csharp
public static class CustomProfiler
{
    private static readonly Dictionary<string, ProfilerData> _profiles = new();
    
    public static IDisposable Profile(string name) => new ProfilerScope(name);
    
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

## IL2CPP Architecture

**Object Reference Pattern**
```csharp
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

Property access involves IL2CPP native calls and pointer arithmetic. Cache frequently accessed references.

**Update Pattern Hierarchy**
```csharp
// GameTickable (performance-critical)
EnemiesManager : GameTickable -> OnTick()
ArcanaManager : GameTickable -> OnTick() 
PixelFontManager : GameTickable -> OnTick()

// GameMonoBehaviour (standard Unity)
Weapon : GameMonoBehaviour -> InternalUpdate()
Projectile : GameMonoBehaviour -> OnUpdate()
CharacterController : GameMonoBehaviour -> OnUpdate()

// MonoBehaviour (UI and non-critical)
UI components -> Update()
```

GameTickable is reserved for core systems requiring coordinated updates.

## Measurement Guidelines

Performance optimization requires data-driven decisions. Always profile before and after changes to validate improvements. Decompiled IL2CPP code has visibility limitations - actual performance may differ from apparent complexity.