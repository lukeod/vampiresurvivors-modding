# Damage System

Comprehensive damage calculation pipeline that flows from character base power through weapon-specific calculations to final damage application.

## Core Damage Flow

### Character Power Calculation
Foundation of damage calculation:

```csharp
public virtual float PPower()      // Base character power
public virtual float PPowerFinal() // Final power with modifiers and cap
```

Character power is capped at 10.0 in the final calculation.

### Weapon Power Calculation
Weapons calculate effective power using virtual methods:

```csharp
public virtual float PPower()               // Primary power
public virtual float SecondaryPPower()      // Secondary attack power  
public virtual float SecondaryCursePPower() // Curse-based secondary power
```

## Damage Application Methods

### Weapon Damage Methods
```csharp
public virtual void DealDamage(IDamageable other)
public virtual void DealDamageRetaliation(IDamageable other)
public virtual void DealDamage(IDamageable other, float damageOverride)
public void DamageAllEnemies(float value)
```

### Damage Formula
`Final Damage = Weapon.PPower × CritMultiplier`

## Critical Hit System

### Critical Hit Calculation
```csharp
public virtual float CalcCritMul()  // Calculate critical multiplier
```

### Critical Hit Stats
```csharp
public float critChance;  // Critical hit probability (0-1)
public float critMul;     // Critical hit damage multiplier
```

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

### Accessing Game Data
```csharp
var gameManager = GM.Core;
var dataManager = gameManager.DataManager;
var player = gameManager.Player;

if (player?.PlayerStats != null)
{
    var ownedPowerUps = player.PlayerStats.GetOwnedPowerUps();
}
```

### Modifying Weapon Damage
```csharp
var dataManager = GM.Core.DataManager;
var weaponData = dataManager.GetWeaponData(WeaponType.WHIP);
if (weaponData != null)
{
    weaponData.power *= 2.0f;
}
```

### Custom Damage Multipliers
```csharp
[HarmonyPatch(typeof(Weapon), "DealDamage")]
[HarmonyPrefix]
public static void ModifyDamage(ref float damageOverride)
{
    damageOverride *= 1.5f;
}
```

### Character Damage Bonuses
```csharp
var player = GM.Core.Player;
if (player?.PlayerStats != null)
{
    var powerStats = player.PlayerStats.GetOwnedPowerUps();
    if (powerStats.ContainsKey(PowerUpType.POWER))
    {
        // Modify power stat
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

### PowerUp Types
PowerUpType exists as a separate enum, but WeaponType also includes these values starting at 50:
- `WeaponType.POWER = 50` - Base damage multiplier
- `WeaponType.AREA = 51` - Weapon area/size
- `WeaponType.SPEED = 52` - Projectile speed
- `WeaponType.COOLDOWN = 53` - Cooldown reduction
- `WeaponType.DURATION = 54` - Effect duration
- `WeaponType.AMOUNT = 55` - Projectile/effect count

## Common Issues
- **Performance**: Too many damage modifications impact frame rate
- **Balance**: Excessive damage trivializes gameplay  
- **Overflow**: Very high damage values may wrap around
- **Compatibility**: Modifications may affect save game balance