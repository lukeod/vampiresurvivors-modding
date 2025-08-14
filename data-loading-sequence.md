# Data Loading Sequence and Projectile System

## Data Loading Call Chain

### Data Loading Classes

**PlatformIntegration Class**
- Located at: `Il2CppVampireSurvivors.App.Scripts.Framework.Initialisation.PlatformIntegration`
- Contains the following methods:
  - `Init(PlayerOptions, AchievementManager, Action)` - Public static initialization method
  - `LoadDlc()` - Private static method for loading DLC
  - `LicenseCheckDlc()` - Private static method for DLC license verification
  - `UpdateDlc()` - Private static method for updating DLC

**DlcSystem Class**
- Located at: `Il2CppVampireSurvivors.Framework.DLC.DlcSystem`
- Contains nested classes for callbacks:
  - `__c__DisplayClass30_0` - For LicenseCheckDlc callbacks
  - `__c__DisplayClass32_0` - For LoadDlc callbacks

**LoadingManager Class**
- Located at: `Il2CppVampireSurvivors.Framework.DLC.LoadingManager`
- Contains the following methods:
  - `LoadDlcs()` - Loads DLC content
  - `ValidateDlcVersions()` - Public method that validates DLC versions
  - `ValidateVersion(int index, Il2CppStructArray<DlcType> dlcs, Action callback)` - Private method for version validation
  - `MountDlc()` - Mounts individual DLC
  - `UnmountDlc()` - Unmounts DLC
  - `LoadManifestDirect()` - Loads DLC manifest directly

**DataManager Loading Methods** (from documentation)
- `LoadBaseJObjects()` - Private method that loads raw JSON data
- `ReloadAllData()` - Public method called multiple times during startup:
  - Once for base game data
  - Once for each DLC (6 DLCs: Moonspell, Foscari, Chalcedony, FirstBlood, Emeralds, ThosePeople)
  - Final call for data validation

### Loading Sequence

**Call Chain**:
1. PlatformIntegration.Init() - Entry point for initialization
2. PlatformIntegration.LoadDlc() - Initiates DLC loading
3. DlcSystem handles DLC licensing and loading coordination
4. LoadingManager orchestrates actual DLC loading
5. DataManager.LoadBaseJObjects() loads raw JSON
6. DataManager.ReloadAllData() called 7+ times (1 base + 6 DLCs)
7. LoadingManager.ValidateVersion() ensures all content is loaded

## Projectile and Object Pool System

### Core Components

**IL2CppObjectPool**
- **Purpose**: IL2CPP interop safety, NOT traditional object pooling
- **Usage**: Throughout the codebase via `Il2CppObjectPool.Get<T>(IntPtr)`
- **Location**: Used in property getters for safe IL2CPP object reference retrieval

**BulletPool Class**
- Located at: `Il2CppVampireSurvivors.Objects.Pools.BulletPool`
- Inherits from: `PhysicsGroup`
- Key fields:
  - `_pool` - Reference to ObjectPool (QFSW.MOP2 library)
  - `IsUncapped` - Boolean for unlimited pool size
  - `UpperLimit` - Maximum pool size
- Key methods:
  - `BulletPool(Projectile projectilePrefab, int capacity = 50)` - Constructor
  - `SpawnAt(float x, float y, Weapon weapon, int index = 0)` - Spawn projectile at position
  - `SpawnAt(float2 pos, Weapon weapon, int index = 0)` - Spawn projectile at position
  - `Return(Projectile projectile)` - Return projectile to pool
  - `Cleanup()` - Clean up pool
  - `Destroy()` - Destroy pool

**Projectile Class Structure**
- Located at: `Il2CppVampireSurvivors.Objects.Projectiles.Projectile`
- Inherits from: `ArcadeSprite`
- Key fields:
  - `_weapon` - Reference to the owning weapon
  - `_indexInWeapon` - Index of this projectile in the weapon's burst
  - `_pool` - Reference to the BulletPool
  - `_objectsHit` - HashSet tracking hit targets
- Key methods:
  - `InitProjectile(BulletPool pool, Weapon weapon, int index)` - Initialization method (parameter is `index` not `indexInWeapon`)
  - `OnUpdate()` - Protected virtual update method

**ProjectileFactory Class**
- Located at: `Il2CppVampireSurvivors.Framework.ProjectileFactory`
- Purpose: Maps WeaponType to Projectile prefabs
- Key components:
  - `_Projectiles` - Dictionary<WeaponType, Projectile> storing prefab mappings
  - `_LinkedFactories` - List of additional ProjectileFactory instances for fallback
  - `GetProjectilePrefab(WeaponType)` - Returns the projectile prefab for a weapon type

**Weapon Fire System**
- Weapon class has nested class `__c__DisplayClass145_0` with:
  - `localIndex` field - tracks projectile index
  - `_Fire_b__0()` method - Anonymous delegate for firing
- The Fire method creates delegates for spawning projectiles with delays

### Projectile Creation Flow

1. **Weapon.Fire()** creates delegate callbacks for each projectile
2. **BulletPool.SpawnAt()** retrieves/creates projectile from object pool (called first)
3. **Projectile.InitProjectile()** called immediately after with pool, weapon, and index parameters
4. **Timing**: ~100ms delay between each projectile in a burst (observed via event logging)
5. **ProjectileFactory.GetProjectilePrefab()** provides the prefab template when BulletPool needs to create new instances

## Solutions for Projectile Manipulation

### Primary Approaches

**Hook InitProjectile**
```csharp
// This method exists and is hookable
[HarmonyPatch(typeof(Projectile), "InitProjectile")]
[HarmonyPostfix]
public static void OnProjectileInit(Projectile __instance, BulletPool pool, Weapon weapon, int index)
{
    // The index parameter identifies which projectile in a burst
    // This is called for EVERY projectile initialization
}
```

### Additional Approaches

**Hook Weapon Stats**
```csharp
// These methods exist and are virtual
// Speculation: Modifying these affects all projectiles from the weapon
[HarmonyPatch(typeof(Weapon), "PPower")]
[HarmonyPostfix]
public static void ModifyPower(Weapon __instance, ref float __result)
{
    // Affects calculated power for all projectiles
}
```

**BulletPool SpawnAt Hooks**
- BulletPool.SpawnAt methods can be hooked
- Successfully hooked both overloads (x,y and float2)
- Timing: Called BEFORE InitProjectile, when projectile is retrieved from pool

**Weapon.Fire Hooks**
- Weapon.Fire() can be hooked (both overloads)
- Note: Must specify parameter types to avoid ambiguity
- Timing: Called BEFORE projectile spawning begins

## Why Type-Level Method Replacement Doesn't Work

### Constraints

1. **IL2CPP Compilation**: C# is compiled to C++, eliminating traditional .NET reflection capabilities
2. **Native Code**: Each projectile type (TrapanoProjectile, FlowerProjectile, etc.) has methods compiled to native code
3. **No Runtime Method Replacement**: Cannot dynamically replace method implementations like in pure .NET

### Recommended Approach

**Most Reliable**:
1. Hook `InitProjectile` to catch all projectile instances
2. Use the `index` parameter to identify specific projectiles in bursts
3. Modify projectile properties at initialization time

**Alternative** (Speculative effectiveness):
1. Modify weapon stat calculation methods (PPower, PAmount, etc.)
2. These modifications cascade to all projectiles from that weapon

## Hook Point Recommendation

### For Custom DLC Injection

**Current Approach** (User's Implementation):
- Hooking `LoadingManager.ValidateVersion`
- Advantage: Guaranteed to run after all base game and DLC content is loaded
- Timing: Called during the DLC loading sequence

**Alternative Approach - Detecting Final ReloadAllData**:
```csharp
private static int reloadCallCount = 0;
private const int EXPECTED_DLC_COUNT = 6; // DLC count

[HarmonyPatch(typeof(DataManager), "ReloadAllData")]
[HarmonyPostfix]
public static void OnDataReload(DataManager __instance)
{
    reloadCallCount++;
    
    // Called 7+ times during startup (1 base + 6 DLCs)
    // The final call is typically after all DLCs are loaded
    if (reloadCallCount >= EXPECTED_DLC_COUNT + 1)
    {
        // This is likely the final call
        // All DLC data should be loaded at this point
    }
}
```

**Important Notes on Hooking**:
- **Deferred Installation**: Install hooks only after GM.Core is available (check in OnUpdate)
- **Access Weapon Type**: Use `weapon.CurrentWeaponData.bulletType` not `weapon._weaponType`
- **Parameter Names**: Must match exactly (e.g., `index` not `indexInWeapon`)
- **Method Overloads**: Specify parameter types to avoid ambiguity errors (e.g., `Fire()` vs `Fire(bool)`)
- **Weapon Stats Units**: `PInterval()` and `PDuration()` return milliseconds, not seconds

The user's approach of hooking `ValidateVersion` for DLC injection appears sound based on the timing, and the `InitProjectile` hook with `index` parameter is the most reliable solution for the projectile manipulation challenge.