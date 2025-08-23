# Data Loading Sequence

Based on analysis of decompiled IL2CPP code, this document describes the data loading process that appears to occur during game initialization. The sequence inferred from static code analysis loads base game data and DLC content.

## Loading Classes

Based on code analysis, the following classes appear to handle data loading:

**PlatformIntegration**
- Location: `Il2CppVampireSurvivors.App.Scripts.Framework.Initialisation.PlatformIntegration`
- Methods:
  - `Init(PlayerOptions, AchievementManager, Action)` - Entry point
  - `LoadDlc()` - Initiates DLC loading
  - `LicenseCheckDlc()` - DLC license verification
  - `UpdateDlc()` - DLC updates

**DlcSystem**
- Location: `Il2CppVampireSurvivors.Framework.DLC.DlcSystem`
- Callback classes: `__c__DisplayClass25_0`, `__c__DisplayClass27_0`, `__c__DisplayClass38_0`

**LoadingManager**
- Location: `Il2CppVampireSurvivors.Framework.DLC.LoadingManager`
- Methods:
  - `LoadDlcs()` - Loads DLC content
  - `ValidateDlcVersions()` - Validates DLC versions
  - `ValidateVersion(int, Il2CppStructArray<DlcType>, Action)` - Version validation
  - `MountDlc()` / `UnmountDlc()` - DLC mounting
  - `LoadManifestDirect()` - Direct manifest loading

**DataManager**
- `LoadBaseJObjects()` - Loads raw JSON data (private method)
- `ReloadAllData()` - Called multiple times during startup (appears to be 1 base + 6 DLCs based on code analysis)

## Loading Sequence

Based on code analysis, the loading sequence appears to follow this pattern:

1. `PlatformIntegration.Init()` - Initialization entry point
2. `PlatformIntegration.LoadDlc()` - DLC loading initiation
3. `DlcSystem` - DLC coordination
4. `LoadingManager` - DLC orchestration
5. `DataManager.LoadBaseJObjects()` - JSON loading (private method)
6. `DataManager.ReloadAllData()` - Called for base game + each DLC
7. `LoadingManager.ValidateVersion()` - Final validation

## Projectile System

Based on decompiled code analysis, the projectile system appears to use these core classes:

### Core Classes

**IL2CppObjectPool**
- Purpose: IL2CPP interop safety
- Usage: `Il2CppObjectPool.Get<T>(IntPtr)` in property getters

**BulletPool**
- Location: `Il2CppVampireSurvivors.Objects.Pools.BulletPool`
- Inherits: `PhysicsGroup`
- Fields: `_pool`, `IsUncapped`, `UpperLimit`
- Methods: `SpawnAt()`, `Return()`, `Cleanup()`, `Destroy()`

**Projectile**
- Location: `Il2CppVampireSurvivors.Objects.projectiles.Projectile`
- Inherits: `ArcadeSprite`
- Fields: `_weapon`, `_indexInWeapon`, `_pool`, `_objectsHit`
- Methods: `InitProjectile(BulletPool, Weapon, int)`, `OnUpdate()`

**ProjectileFactory**
- Location: `Il2CppVampireSurvivors.Framework.ProjectileFactory`
- Fields: `_Projectiles` (Dictionary<WeaponType, Projectile>), `_LinkedFactories`
- Methods: `GetProjectilePrefab(WeaponType)`

### Creation Flow

Based on code analysis, the projectile creation flow appears to follow this sequence:

1. `Weapon.Fire()` - Creates delegate callbacks
2. `BulletPool.SpawnAt()` - Retrieves/creates projectile
3. `Projectile.InitProjectile()` - Initializes with pool, weapon, index
4. `ProjectileFactory.GetProjectilePrefab()` - Provides prefab template

## Projectile Manipulation

### Hook Points

**InitProjectile Hook**
```csharp
[HarmonyPatch(typeof(Projectile), "InitProjectile")]
[HarmonyPostfix]
public static void OnProjectileInit(Projectile __instance, BulletPool pool, Weapon weapon, int index)
{
    // Called for every projectile initialization
    // index identifies projectile position in burst
}
```

**Weapon Stats Hook**
```csharp
[HarmonyPatch(typeof(Weapon), "PPower")]
[HarmonyPostfix]
public static void ModifyPower(Weapon __instance, ref float __result)
{
    // Affects all projectiles from weapon
}
```

**BulletPool Hook**
- Methods: `SpawnAt(float, float, Weapon, int)`, `SpawnAt(float2, Weapon, int)`
- Timing: Called before `InitProjectile`

**Weapon.Fire Hook**
- Methods: `Fire()`, `Fire(bool)`
- Timing: Called before projectile spawning
- Note: Specify parameter types to avoid ambiguity

### IL2CPP Constraints

Based on IL2CPP transpilation analysis:

- C# compiled to C++ native code
- No runtime method replacement
- Limited reflection capabilities

## DLC Injection Hooks

### ValidateVersion Hook
```csharp
[HarmonyPatch(typeof(LoadingManager), "ValidateVersion")]
[HarmonyPostfix]
public static void OnValidateVersion(/* parameters */)
{
    // Called after all DLC content loaded
}
```

### ReloadAllData Hook
```csharp
private static int reloadCallCount = 0;

[HarmonyPatch(typeof(DataManager), "ReloadAllData")]
[HarmonyPostfix]
public static void OnDataReload(DataManager __instance)
{
    reloadCallCount++;
    if (reloadCallCount >= 7) // 1 base + 6 DLCs based on code analysis
    {
        // Final call - all data loaded
    }
}
```

### Hook Requirements

Based on code analysis, these requirements appear necessary for hooks:

- Install hooks after `GM.Core` available
- Use `weapon._currentWeaponData.bulletType` not `weapon._weaponType`
- Parameter names must match exactly
- Specify parameter types for method overloads
- `PInterval()` and `PDuration()` return milliseconds