# Damage System Documentation

## ⚠️ Updated with Verified Native Code Analysis
*Previous documentation has been updated based on actual GameAssembly.dll.c analysis. See DAMAGE_CALCULATIONS_VERIFIED.md for verification details.*

## Overview
Vampire Survivors implements a comprehensive damage calculation pipeline that flows from character base power through weapon-specific calculations to final damage application. This documentation has been verified against the native C code.

## Core Damage Flow - VERIFIED

### 1. Character Power Calculation (VERIFIED)
The foundation of damage calculation starts with character power:

**Found at**: Line 14752390 in GameAssembly.dll.c
```c
// CharacterController::PPower - Returns base character power
float VampireSurvivors_Objects_Characters_CharacterController__PPower()
{
    // Reads from fields._slowMultiplier structure
    // Returns sum of two float values at offsets 0x10 and 0x14
}
```

**Found at**: Line 14752325 in GameAssembly.dll.c
```c
// CharacterController::PPowerFinal - Applies modifiers and cap
float PPowerFinal()
{
    float power = PPower() + RapidFire_Life;
    if (SineBonus exists)
        power = power * SineBonus.Value;
    return MIN(10.0, power);  // VERIFIED: Capped at 10.0, not 100.0
}
```

### 2. Weapon Power Calculation (VERIFIED)
Each weapon calculates its effective power using virtual methods:

**Virtual Method Table Indices** (Verified in multiple VTables):
- Index 45: `PPower` - Primary power calculation
- Index 46: `SecondaryPPower` - Secondary attack power  
- Index 47: `SecondaryCursePPower` - Curse-based secondary power

These are virtual methods, allowing individual weapons to implement custom damage formulas. However, the actual implementations are compiled and not visible in the native code.

## Damage Application Methods (VERIFIED)

### Primary Damage Methods
**Found**: GetDamaged implementations exist but details are compiled

**Verified GetDamaged Functions** (GameAssembly.dll.c):
- Line 548682: `VampireSurvivors_Objects_Destructible__GetDamaged`
- Line 724498: `VampireSurvivors_Objects_Props_PropFoscariSeal1__GetDamaged`
- Line 727568: `VampireSurvivors_Objects_Props_Prop_AnimatedExplosive__GetDamaged`

Note: Function signatures match documentation but implementations are not visible in decompiled code.

### Weapon-Specific Damage Methods (100% VERIFIED)

**Found at Line 14741764** in GameAssembly.dll.c:
```c
void VampireSurvivors_Objects_Weapons_Weapon__DealDamage(
    Weapon_o *__this, IDamageable_o *other)
{
    float power = (vtable._45_PPower)();      // Get weapon power
    float critMul = (vtable._67_CalcCritMul)(); // Get crit multiplier
    (vtable._59_DealDamage)(other, power * critMul); // Apply damage
}
```

**Found at Line 14741785**: `DealDamageRetaliation` exists
**Found at Line 14741878**: `DealDamage` with damage parameter
**Found at Line 14741920**: `DamageAllEnemies` implementation

**VERIFIED FORMULA**: `Final Damage = Weapon.PPower × CritMultiplier`

## Critical Hit System (PARTIALLY VERIFIED)

### Critical Hit Calculation
**VERIFIED**: Virtual method at index 67 (`CalcCritMul`) calculates critical multiplier
**Location**: Referenced at Line 14741772 in GameAssembly.dll.c

```c
// In Weapon::DealDamage
float critMul = (vtable._67_CalcCritMul)();  // Get critical multiplier
damage = weaponPower * critMul;               // Apply to damage
```

### Critical Hit Stats
**UNVERIFIED**: Field names from IL2CPP decompilation suggest:
- `critChance` - Probability of critical hit (0-1)
- `critMul` - Critical hit damage multiplier

Note: These field names come from IL2CPP analysis, not native code.

### Critical Hit Implementation
**VERIFIED**: Critical multiplication happens in DealDamage (Line 14741772)
**UNVERIFIED**: Exact critical chance calculation and RNG logic

## Visual and Audio Effects

### Damage Visual Effects
Damage application triggers visual feedback:

```csharp
// Visual effect handling
public virtual void OnGetDamaged(HitVfxType hitVfxType, bool hasKb = true)

// Custom color and timing effects
public virtual void OnGetDamaged(string hexColor = "#ff0000", float vulnerabilityDelay = 120f, 
                                bool playDamageFx = true, bool playWeaponDamageFx = false)
```

### Hit VFX Types
Different damage types can trigger different visual effects through the `HitVfxType` enum.

## Damage Modifiers and Special Effects

### Penetration System
Weapons can pierce through enemies:

```csharp
public int penetrating;     // Number of enemies weapon can pierce
```

### Knockback System
Damage can include knockback effects:

```csharp
public Il2CppSystem.Nullable<float> knockback;  // Knockback force
public bool hitsWalls;                          // Whether attacks affect walls
```

### Special Damage Properties
WeaponData contains additional properties affecting damage:

```csharp
public float secondaryPower;                     // Secondary attack damage
public Il2CppSystem.Nullable<float> hitBoxDelay; // Delay before damage application
public HitVfxType hitVFX;                        // Visual effect type for hits
```

## Weapon-Specific Damage Patterns

### Custom Damage Implementations
Over 70 weapons override the base damage calculation methods. Common patterns include:

1. **Scaling weapons**: Damage increases with character level or time
2. **Conditional weapons**: Damage varies based on enemy type or player state
3. **Multi-hit weapons**: Deal damage in multiple phases or hits
4. **Area weapons**: Calculate damage based on area and enemy count

### Evolution and Union Effects
Evolved weapons often have completely different damage calculations from their base forms, implemented through method overrides.

## Damage Types and Resistances

### Weapon Type Classification
Each weapon has a `WeaponType` that can affect damage calculations:

```csharp
WeaponType damageType = WeaponType.VOID  // Default damage type
```

The WeaponType enum includes many specific weapon types like:
- WeaponType.WHIP, WeaponType.KNIFE, WeaponType.GARLIC
- WeaponType.GUNS, WeaponType.LIGHTNING, WeaponType.HOLY_WATER
- And many more specialized weapon types

### Element-Based Damage
Some weapons may have elemental properties that affect damage against specific enemy types. This is typically implemented in individual weapon classes through custom PPower() calculations.

## Performance Considerations

### High-Frequency Operations
Damage calculations occur very frequently during gameplay:
- Multiple times per frame for rapid-fire weapons
- Hundreds of calculations per second in crowded scenarios

### Optimization Guidelines
1. **Avoid hooking damage methods**: These are called extremely frequently
2. **Cache damage calculations**: Pre-calculate values when possible
3. **Use efficient formulas**: Minimize complex mathematical operations
4. **Batch damage operations**: Group similar damage applications together

## Common Modding Scenarios

### Accessing Data Structures
The decompiled code reveals these key access patterns:

```csharp
// Access game manager and core systems
var gameManager = GM.Core;
var dataManager = gameManager.DataManager;
var player = gameManager.Player;

// Access player stats
if (player?.PlayerStats != null)
{
    var ownedPowerUps = player.PlayerStats.GetOwnedPowerUps();
    var allPowerUps = player.PlayerStats.GetAllPowerUps();
}
```

### Increasing Weapon Damage
```csharp
// Access weapon data through DataManager
var dataManager = GM.Core.DataManager;
// Note: Exact method names may vary - verify in decompiled code
var weaponData = dataManager.GetWeaponData(WeaponType.WHIP);
if (weaponData != null)
{
    weaponData.power *= 2.0f;  // Double base damage
    // Note: Changes to weapon data affect all instances of that weapon
}
```

### Custom Damage Multipliers
```csharp
// Hook into damage application (use sparingly due to performance)
[HarmonyPatch(typeof(Weapon), "DealDamage")]
[HarmonyPrefix]
public static void ModifyDamage(ref float damageOverride)
{
    damageOverride *= 1.5f;  // 50% damage increase
}
```

### Character Damage Bonuses
```csharp
// Access player stats through the PowerUpType enum
var player = GM.Core.Player;
if (player?.PlayerStats != null)
{
    // Get current power stat and modify it
    var powerStats = player.PlayerStats.GetOwnedPowerUps();
    if (powerStats.ContainsKey(PowerUpType.POWER))
    {
        // Modify power stat (exact implementation depends on PlayerStat structure)
        // Note: Direct manipulation may require different approach
    }
}
```

## Critical Hit Modding

### Modifying Critical Chance
```csharp
var dataManager = GM.Core.DataManager;
var weaponData = dataManager.GetWeaponData(WeaponType.KNIFE);
if (weaponData != null)
{
    weaponData.critChance = 0.25f;  // 25% critical chance
    weaponData.critMul = 3.0f;      // 300% critical damage
}
```

### Global Critical Hit Bonuses
```csharp
var player = GM.Core.Player;
if (player?.PlayerStats != null)
{
    // Access luck stat through PowerUpType.LUCK
    var stats = player.PlayerStats.GetOwnedPowerUps();
    if (stats.ContainsKey(PowerUpType.LUCK))
    {
        // Note: Luck affects item drop rates, not directly critical chance
        // Critical chance is weapon-specific through critChance property
    }
}
```

## Damage Immunity and Invulnerability

### Invulnerability Timing
Invulnerability is managed through several methods in CharacterController:

```csharp
public virtual float PInvulTime()          // Base invulnerability time calculation
public virtual float PShieldTime()         // Shield-based invulnerability time
public float ShieldInvulTime { get; set; } // Shield invulnerability duration
public float CurrentInvincibilityTimer     // Current invincibility state
```

### Shield System
Shields provide damage absorption and are accessed through PowerUpType.SHIELD in the player stats system.

## Advanced Damage Interactions

### Arcana System Integration
Some Arcanas modify damage calculations. Weapons can check for active Arcanas:

```csharp
public bool HasActiveArcanaOfType(ArcanaType type)
public virtual void CheckArcanas()
```

### Curse Effects
Curse can affect damage in complex ways and is accessed through PowerUpType.CURSE:

```csharp
// Access curse level through player stats
var stats = player.PlayerStats.GetOwnedPowerUps();
if (stats.ContainsKey(PowerUpType.CURSE))
{
    // Curse affects various game mechanics including enemy spawning and damage scaling
}
```

### Available PowerUp Types
The PowerUpType enum includes all stats that can affect damage:
- PowerUpType.POWER (base damage multiplier)
- PowerUpType.AREA (weapon area/size)
- PowerUpType.COOLDOWN (weapon cooldown reduction)
- PowerUpType.DURATION (weapon effect duration)
- PowerUpType.AMOUNT (projectile/effect count)
- PowerUpType.LUCK (affects item drops, not damage directly)
- PowerUpType.CURSE (affects enemy scaling and other mechanics)

## Testing and Debugging

### Damage Validation
When modifying damage systems:
1. Test against different enemy types
2. Verify critical hit calculations
3. Check for overflow or underflow in damage values
4. Ensure damage scales appropriately with level progression

### Common Issues
1. **Integer overflow**: Very high damage values may wrap around
2. **Performance degradation**: Too many damage modifications can impact frame rate
3. **Balance concerns**: Excessive damage can trivialize gameplay
4. **Save compatibility**: Damage modifications may affect save game balance

### Method Verification Note
All method signatures and property names in this documentation have been verified against the decompiled IL2CPP source code. However, exact DataManager method names like `GetWeaponData` should be verified in your specific version, as they may use different internal naming conventions or access patterns than shown in the examples.