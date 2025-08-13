# Weapon System

## Overview

The weapon system in Vampire Survivors handles weapon behavior, stats, evolution, and unions. Understanding this system is crucial for creating weapon mods.

## WeaponType Enum

**Location**: `Il2CppVampireSurvivors.Data.WeaponType`

Weapon identifiers with over 500 entries:

### Categories

- **Base Weapons** (1-49): `WHIP = 3`, `AXE = 5`, `KNIFE = 7`, `CROSS = 4`
- **PowerUps** (50-68): `POWER = 50`, `AREA = 51`, `SPEED = 52`, `DURATION = 53`
- **Evolutions** (100+): Various evolved weapon forms
- **DLC Prefixes**:
  - `C1_` - Among Us DLC (166-200)
  - `FB_` - Foscari DLC (300-350)
  - `EME_` - Emergency Meeting DLC (361-450)
  - `TP_` - Tides of Foscari/Castlevania DLC (1400-1500)

## WeaponData Structure

**Location**: `Il2CppVampireSurvivors.Data.Weapons.WeaponData`

Comprehensive weapon configuration:

### Identity & Metadata

```csharp
public int level;                    // Weapon level (1-8 typically)
public WeaponType bulletType;        // Projectile type if applicable
public string name;                  // Display name
public bool hidden;                  // Hidden from selection
public bool alwaysHidden;           // Never shown
public string description;           // Weapon description
public string tips;                  // Usage tips
public string texture;               // Texture reference
public string frameName;             // Sprite frame
public string collectionFrame;      // Collection UI frame
```

### Evolution System

```csharp
public string evoInto;                           // Target evolution weapon
public Il2CppStructArray<WeaponType> evoSynergy; // Required synergy items
public bool isEvolution;                         // Is this an evolution?
public bool isSpecialOnly;                      // Special conditions only
public List<WeaponType> evolvesFrom;            // What evolves into this
public List<WeaponType> requires;               // Base requirements
public List<WeaponType> requiresMax;            // Items needed at max level
public List<WeaponType> evolutionLine;          // Complete evolution chain
```

### Basic Combat Stats

```csharp
public float power;                              // Damage
public float area;                               // Effect area
public float speed;                              // Projectile speed
public Il2CppSystem.Nullable<float> duration;    // Effect duration
public float interval;                           // Attack interval
public int amount;                               // Projectile count
public float repeatInterval;                     // Multi-hit interval
public float secondaryPower;                     // Secondary damage
```

### Advanced Combat Stats

```csharp
public Il2CppSystem.Nullable<float> knockback;  // Knockback force
public Il2CppSystem.Nullable<float> hitBoxDelay; // Hitbox activation delay
public float critChance;                         // Critical chance
public float critMul;                            // Critical multiplier
public bool hitsWalls;                          // Collides with walls
public int penetrating;                         // Pierce count
public HitVfxType hitVFX;                       // Hit effect type
public float cooldown;                          // Weapon cooldown
public int rarity;                              // Weapon rarity
public int poolLimit;                           // Pool object limit
public int charges;                             // Weapon charges
public bool intervalDependsOnDuration;          // Interval scaling
```

### PowerUp Stats

```csharp
public float maxHp;              // Max health bonus
public float moveSpeed;          // Movement speed bonus
public float growth;             // Experience gain bonus
public float magnet;             // Pickup range bonus
public float luck;               // Luck bonus
public float armor;              // Armor bonus
public float greed;              // Gold gain bonus
public float regen;              // Regeneration bonus
public int revivals;             // Revival count
public int rerolls;              // Reroll count
public int skips;                // Skip count
public float chance;             // Proc chance
public float shieldInvulTime;    // Shield invulnerability time
public float curse;              // Curse level
public float charm;              // Charm effect
public float fever;              // Fever mode bonus
public float invulTimeBonus;     // Invulnerability time bonus
```

### Content & Organization

```csharp
public int price;                              // Purchase price
public bool appliesOnlyToOwner;                // MP-only affects owner
public bool allowDuplicates;                   // Allow duplicate weapons
public bool despawnOnUnavailable;              // Despawn when unavailable
public string contentGroup;                    // DLC content grouping
public WeaponType followerType;                // Follower weapon type
public string followerAI;                      // Follower AI behavior
public bool dropRateAffectedByLuck;            // Luck affects drop rate
public bool sealable;                          // Can be sealed in modes
public bool unexcludeSelf;                     // Don't exclude self
```

## CRITICAL: Incremental Level System

**Weapon levels use incremental deltas:**

- **Level 1**: Absolute base values
- **Levels 2-8**: Incremental changes from previous level

### Example JSON Structure

```json
"MAGIC_MISSILE": [
    { "level": 1, "power": 10 },      // Base: 10
    { "level": 2, "power": 10 },      // Adds 10 (total: 20)
    { "level": 3, "power": 10 }       // Adds 10 (total: 30)
]
```

### Calculating Total Stats

```csharp
public static float CalculatePowerAtLevel(JArray levels, int targetLevel)
{
    float totalPower = levels[0]["power"].Value<float>();  // Base
    
    for (int i = 1; i < targetLevel && i < levels.Count; i++)
    {
        totalPower += levels[i]["power"].Value<float>();   // Add deltas
    }
    
    return totalPower;
}
```

## Weapon Class

**Location**: `Il2CppVampireSurvivors.Objects.Weapons.Weapon`

The runtime weapon instance that handles behavior.

### Stat Calculation Methods

Virtual methods that calculate final weapon stats:

```csharp
public virtual float PPower()                    // Final power calculation
public virtual float PArea()                     // Final area calculation  
public virtual float PSpeed()                    // Final speed calculation
public virtual float PAmount()                   // Final amount calculation
public virtual float PDuration()                 // Final duration calculation
public virtual float PInterval()                 // Final interval calculation
public virtual int PBounces()                    // Final bounce count
public virtual float SecondaryPPower()           // Secondary power calculation
public virtual float SecondaryPAmount()          // Secondary amount calculation
public virtual float SecondaryCursePPower()      // Curse-modified secondary power
public virtual float PSpeedRepeatInterval()      // Repeat interval speed
public virtual float PHitBoxDelayOverSpeed()     // Hitbox delay over speed
```

**Important**: These are virtual methods. 70+ weapons override these for custom calculations.

### Damage Methods

```csharp
public virtual void DealDamage(IDamageable other)
public virtual void DealDamageRetaliation(IDamageable other)
public virtual void DealDamage(IDamageable other, float damageOverride)
public void DamageAllEnemies(float value)
```

## Evolution & Union System

**Location**: `Il2CppVampireSurvivors.Framework.LevelUpFactory`

### Evolution Requirements

Evolutions require:
1. Base weapon at max level (usually level 8)
2. Specific synergy item(s)
3. Opening a treasure chest

### Evolution Data Structure

```csharp
// Example: Whip → Bloody Tear
WeaponData whip = new WeaponData
{
    evoInto = "BLOODY_TEAR",
    evoSynergy = new[] { WeaponType.HOLLOW_HEART }
};
```

### Evolution Logic

```csharp
// In LevelUpFactory
public bool HasPotentialEvolution(CharacterController character)
public WeaponType PullEvolution(CharacterController character)
public bool HasEvolutionRequirements(WeaponData data, List<Equipment> held)
```

### Special Evolution Cases

Some weapons have unique requirements:

```csharp
// Special handlers
private static bool AlucardShieldUniqueRequirements(List<Equipment> held)
private static bool CalamityRingUniqueRequirements(List<Equipment> held)
```

## Limit Break System

**Location**: `Il2CppVampireSurvivors.Data.LimitBreakData`

Allows weapons to exceed normal stat caps:

```csharp
public class LimitBreakData
{
    public int rarity;
    public Il2CppSystem.Nullable<float> power;
    public Il2CppSystem.Nullable<float> area;
    public Il2CppSystem.Nullable<float> speed;
    public Il2CppSystem.Nullable<int> max;
    public Il2CppSystem.Nullable<int> penetrating;
    public Il2CppSystem.Nullable<int> amount;
    public Il2CppSystem.Nullable<float> chance;
    public Il2CppSystem.Nullable<int> duration;
    public Il2CppSystem.Nullable<float> critChance;
    public Il2CppSystem.Nullable<float> cooldown;
    public Il2CppSystem.Nullable<WeaponType> addEvolvedWeapon;
    
    public void AccumulateData(LimitBreakData other)
    public void ApplyDataToWeapon(WeaponData weapon)
}
```

## Accessing Weapon Data

### Get All Weapons

```csharp
var dataManager = GM.Core?.DataManager;
if (dataManager != null)
{
    var weapons = dataManager.GetConvertedWeapons();
    foreach (var kvp in weapons)
    {
        WeaponType type = kvp.Key;
        List<WeaponData> levels = kvp.Value;
        
        // Process each level
        for (int i = 0; i < levels.Count; i++)
        {
            WeaponData levelData = levels[i];
            // Level data has absolute values when using GetConverted
        }
    }
}
```

### Modify Weapon Stats

```csharp
// Direct JSON modification (remember deltas!)
var weaponJson = dataManager._allWeaponDataJson;
JArray whipLevels = weaponJson["WHIP"] as JArray;

// Level 1: absolute values
whipLevels[0]["power"] = 50;

// Levels 2+: deltas
whipLevels[1]["power"] = 20;  // Adds to level 1

// Apply changes
dataManager.ReloadAllData();
```

### Add Weapon to Character

```csharp
var gameManager = GM.Core;
var player = gameManager?.Player;

if (gameManager != null && player != null)
{
    gameManager.AddWeapon(WeaponType.WHIP, player);
}
```

## Common Weapon Operations

### Check for Evolution

```csharp
public static bool CanEvolve(WeaponData weapon, CharacterController character)
{
    if (string.IsNullOrEmpty(weapon.evoInto))
        return false;
    
    // Check if weapon is max level
    if (weapon.level < 8)  // Assuming 8 is max
        return false;
    
    // Check for synergy items
    if (weapon.evoSynergy != null)
    {
        var accessories = character._accessoriesManager;
        // Check if character has required items
    }
    
    return true;
}
```

### Calculate Final Damage

```csharp
public static float CalculateFinalDamage(Weapon weapon, CharacterController owner)
{
    float basePower = weapon.PPower();  // Weapon's calculated power
    float charPower = owner.PlayerStats.Power.Value;  // Character power multiplier
    
    return basePower * (1 + charPower);
}
```

## Best Practices

### 1. Use GetConverted Methods

```csharp
// Preferred: GetConverted returns absolute values
var weapons = dataManager.GetConvertedWeapons();

// Avoid: Direct JSON has deltas
var json = dataManager._allWeaponDataJson;
```

### 2. Remember Level Deltas

When modifying JSON directly:
- Level 1 = absolute values
- Levels 2-8 = incremental deltas

### 3. Check Nullable Values

```csharp
if (weaponData.duration.HasValue)
{
    float duration = weaponData.duration.Value;
}
```

### 4. Virtual Method Overrides

Many weapons override stat calculation methods. Check specific weapon implementations.

### 5. Evolution Timing

Evolutions only trigger when opening treasure chests, not automatically at max level.

## Hook Points

### Weapon Addition

```csharp
[HarmonyPatch(typeof(GameManager), "AddWeapon")]
[HarmonyPrefix]
public static void OnAddWeapon(WeaponType type, CharacterController character)
{
    // Weapon is about to be added
}
```

### Evolution Check

```csharp
[HarmonyPatch(typeof(LevelUpFactory), "HasPotentialEvolution")]
[HarmonyPostfix]
public static void OnEvolutionCheck(ref bool __result, CharacterController character)
{
    // Can modify evolution availability
}
```

### Damage Calculation

```csharp
[HarmonyPatch(typeof(Weapon), "PPower")]
[HarmonyPostfix]
public static void OnPowerCalc(Weapon __instance, ref float __result)
{
    // Can modify final power value
    __result *= 2.0f;  // Double damage
}
```