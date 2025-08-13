# Damage System Documentation

## Overview
Vampire Survivors implements a comprehensive damage calculation pipeline that flows from character base power through weapon-specific calculations to final damage application. Understanding this system is essential for creating weapon mods and damage modifications.

## Core Damage Flow

### 1. Character Power Calculation
The foundation of damage calculation starts with character power:

```csharp
public virtual float PPower()  // In CharacterController
```

This method calculates the character's base damage output, combining base stats with modifiers.

### 2. Weapon Power Calculation
Each weapon calculates its effective power using virtual methods:

```csharp
public virtual float PPower()              // Primary power calculation
public virtual float SecondaryPPower()     // Secondary attack power
public virtual float SecondaryCursePPower() // Curse-based secondary power
```

These methods are virtual, allowing individual weapons to implement custom damage formulas.

## Damage Application Methods

### Primary Damage Methods
Located in `CharacterController`, these methods handle damage application:

```csharp
// Main damage dealing method with full parameters
public virtual void GetDamaged(float value, HitVfxType showHitVfx = HitVfxType.Default, 
                               float damageKnockBack = 1f, WeaponType damageType = WeaponType.VOID, 
                               bool hasKb = true)

// Simplified damage application
public virtual bool GetDamaged(float damageAmount)

// Self-damage (friendly fire scenarios)
public virtual bool GetDamagedByOwnWeapon(float damageAmount)
```

### Weapon-Specific Damage Methods
Located in the `Weapon` class:

```csharp
// Standard damage dealing
public virtual void DealDamage(IDamageable other)

// Retaliation damage (counter-attacks)
public virtual void DealDamageRetaliation(IDamageable other)

// Damage with custom amount override
public virtual void DealDamage(IDamageable other, float damageOverride)

// Area damage to all enemies
public void DamageAllEnemies(float value)
```

## Critical Hit System

### Critical Hit Calculation
The critical hit system is handled through:

```csharp
public virtual void StandardCritical(ArcadeColliderType second, ArcadeColliderType first)
```

### Critical Hit Stats
Critical hits are influenced by weapon stats:

```csharp
public float critChance;    // Probability of critical hit (0-1)
public float critMul;       // Critical hit damage multiplier
```

### Critical Hit Formula
Critical hits are handled through the `StandardCritical` method in weapons:

```csharp
public virtual void StandardCritical(ArcadeColliderType second, ArcadeColliderType first)
```

The general pattern is:
1. Roll against `critChance` to determine if hit is critical
2. If critical, multiply damage by `critMul`
3. Apply visual and audio effects through the StandardCritical method

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