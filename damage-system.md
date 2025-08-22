# Damage System

Comprehensive damage calculation pipeline that flows from character base power through weapon-specific calculations to final damage application.

## Core Damage Flow

### Character Power Calculation
Foundation of damage calculation:

```csharp
public virtual float PPower()                    // Base character power
public float PPowerFinal()                       // Final power with modifiers and cap
public float PPowerWithoutSilentMight()          // Power calculation excluding Silent Might modifiers
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

// Extended damage effects with invulnerability control
public void OnGetDamaged(string hexColor = "#ff0000", float vulnerabilityDelay = 120f, 
                        bool playDamageFx = true, bool playWeaponDamageFx = false, 
                        bool ignoreInvulnerabilityForRestoringTint = false)
```

### Hit VFX Types
Damage types trigger different visual effects through the `HitVfxType` enum.

## Damage Modifiers and Special Effects

### Penetration System
Weapon piercing through enemies:

```csharp
public int penetrating;     // Number of enemies weapon can pierce
```

### Knockback System
Damage with knockback effects:

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
Each weapon has a `WeaponType` that affects damage calculations:

```csharp
WeaponType damageType = WeaponType.VOID  // Default damage type
```

The WeaponType enum includes specific weapon types:
- WeaponType.WHIP, WeaponType.KNIFE, WeaponType.GARLIC
- WeaponType.GUNS, WeaponType.LIGHTNING, WeaponType.HOLY_WATER
- Additional specialized weapon types

### Element-Based Damage
Weapons with elemental properties affect damage against specific enemy types through custom PPower() calculations in individual weapon classes.

## Performance Considerations

Damage calculations occur multiple times per frame for rapid-fire weapons and hundreds of times per second in crowded scenarios.

**Optimization Guidelines:**
- Avoid hooking damage methods due to high call frequency
- Cache damage calculations and pre-calculate values when possible
- Minimize complex mathematical operations
- Group similar damage applications together

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
public static void ModifyDamage(ref float damage)
{
    damage *= 1.5f;
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
    var stats = player.PlayerStats.GetOwnedPowerUps();
    if (stats.ContainsKey(PowerUpType.LUCK))
    {
        // Luck affects item drop rates, not critical chance
        // Critical chance is weapon-specific through critChance property
    }
}
```

## Damage Immunity and Invulnerability

### Invulnerability Timing
Invulnerability is managed through several methods in CharacterController:

```csharp
public virtual float PInvulTime()                    // Base invulnerability time calculation
public virtual float PShieldTime()                   // Shield-based invulnerability time
public float ShieldInvulTime { get; set; }           // Shield invulnerability duration
public float CurrentInvincibilityTimer               // Current invincibility state
public void TriggerGetDamagedByOwnWeapon(float damage) // Self-damage from own weapons
public virtual void GetDamagedByOwnWeapon(float damage) // Virtual method for self-damage handling
```

### Shield System
Shields provide damage absorption and are accessed through PowerUpType.SHIELD in the player stats system.

## Advanced Damage Interactions

### Arcana System Integration
Arcanas modify damage calculations. Weapons check for active Arcanas:

```csharp
public bool HasActiveArcanaOfType(ArcanaType type)
public virtual void CheckArcanas()
```

### Curse Effects
Curse affects damage in complex ways and is accessed through PowerUpType.CURSE:

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
- **Performance**: Excessive damage modifications impact frame rate
- **Balance**: High damage values affect gameplay difficulty
- **Overflow**: Very high damage values wrap around
- **Compatibility**: Modifications affect save game balance