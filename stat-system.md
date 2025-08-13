# Stat System Documentation

## Overview
Vampire Survivors uses a comprehensive stat modification system built around wrapper types and incremental calculation methods. Understanding this system is crucial for creating effective mods that modify character and weapon statistics.

## PlayerModifierStats Structure

The core stat container is `PlayerModifierStats`, which stores all character stat modifiers using specialized wrapper types.

### EggFloat Properties (Most Common)
The majority of stats use the `EggFloat` wrapper type:

```csharp
public unsafe EggFloat Power { get; set; }
public unsafe EggFloat Area { get; set; }
public unsafe EggFloat Speed { get; set; }
public unsafe EggFloat MoveSpeed { get; set; }
public unsafe EggFloat Growth { get; set; }
public unsafe EggFloat Luck { get; set; }
public unsafe EggFloat Duration { get; set; }
public unsafe EggFloat Cooldown { get; set; }
public unsafe EggFloat Amount { get; set; }
public unsafe EggFloat Armor { get; set; }
public unsafe EggFloat Greed { get; set; }
public unsafe EggFloat Regen { get; set; }
public unsafe EggFloat ReRolls { get; set; }       // Note exact casing
public unsafe EggFloat Skips { get; set; }
public unsafe EggFloat MaxHp { get; set; }
public unsafe EggFloat Magnet { get; set; }
public unsafe EggFloat Curse { get; set; }
public unsafe EggFloat Banish { get; set; }
```

### Special Type Properties

**EggDouble (Different Wrapper)**:
```csharp
public unsafe EggDouble Revivals { get; set; }
```

**Raw Float (No Wrapper)**:
```csharp
public unsafe float Shroud { get; set; }
public unsafe float Shields { get; set; }
public unsafe float Defang { get; set; }
public unsafe float InvulTimeBonus { get; set; }    // Note exact casing
public unsafe float Fever { get; set; }
```

**Raw Int (No Wrapper)**:
```csharp
public unsafe int Charm { get; set; }
```

### Critical Implementation Notes

1. **Mixed wrapper types**: Most stats use `EggFloat`, but `Revivals` uses `EggDouble`, and several use raw types
2. **Exact casing matters**: Properties use specific casing like `ReRolls`, `MaxHp`, `InvulTimeBonus`
3. **Type safety**: You must use the correct wrapper type or direct assignment will fail

## EggFloat/EggDouble Wrappers

### Working with EggFloat
```csharp
EggFloat power = playerStats.Power;
float currentValue = power.GetValue();
power.SetValue(100);

// Modify through wrapper
playerStats.Power.SetValue(playerStats.Power.GetValue() + 50);
```

### Working with EggDouble
```csharp
EggDouble revivals = playerStats.Revivals;
double currentRevivals = revivals.GetValue();
revivals.SetValue(5);
```

### Working with Raw Types
```csharp
// Direct assignment for raw types
playerStats.Shroud = 0.5f;
playerStats.Charm = 3;
```

## Weapon Stat Calculation

### Virtual Calculation Methods
Weapons have virtual methods that calculate final stats by combining base values with modifiers:

```csharp
public virtual float PPower()          // Final power calculation
public virtual float PArea()           // Final area calculation  
public virtual float PSpeed()          // Final speed calculation
public virtual float PAmount()         // Final amount calculation
public virtual float PDuration()       // Final duration calculation
public virtual float SecondaryPPower() // Secondary power calculation
public virtual float SecondaryPAmount()// Secondary amount calculation
```

### Custom Weapon Overrides
Over 70 weapons override these methods for custom calculations. This means:
- Each weapon can have unique stat calculation formulas
- Not all weapons follow the same mathematical patterns
- Some weapons may have special caps, multipliers, or conditional bonuses

## Character Base Stats

Character base stats are defined in `CharacterData` with exact property names:

```csharp
public unsafe float maxHp { get; set; }
public unsafe float armor { get; set; }
public unsafe float regen { get; set; }
public unsafe float moveSpeed { get; set; }
public unsafe double power { get; set; }    // Note: double type
public unsafe float area { get; set; }
public unsafe float speed { get; set; }
public unsafe float duration { get; set; }
public unsafe float amount { get; set; }
public unsafe float luck { get; set; }
public unsafe float growth { get; set; }
public unsafe float greed { get; set; }
public unsafe float magnet { get; set; }
public unsafe float revivals { get; set; }
public unsafe float curse { get; set; }
public unsafe float shields { get; set; }
public unsafe float reRolls { get; set; }
```

## Stat Upgrade Methods

### Upgrade Method
The primary method for applying stat modifications:

```csharp
public unsafe void Upgrade(ModifierStats other, bool multiplicativeMaxHp = false)
```

This method combines stat modifiers, with special handling for MaxHp which can be applied multiplicatively.

## Incremental Level System for Weapons

**Critical Understanding**: Weapon data uses an incremental delta system:

- **Level 1**: Contains absolute base values
- **Levels 2-8**: Contains incremental changes from the previous level

### Example JSON Structure
```json
"MAGIC_MISSILE": [
    { "level": 1, "power": 10 },      // Base: 10
    { "level": 2, "power": 10 },      // Adds 10 (total: 20)
    { "level": 3, "power": 10 }       // Adds 10 (total: 30)
]
```

### Calculating Total Stats at Level
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

## Recommended Data Access Pattern

### Use GetConverted Methods
Instead of manually calculating deltas, use DataManager's converted methods:

```csharp
// Get weapons with absolute values (no delta calculation needed)
var weapons = dataManager.GetConvertedWeapons();
var characters = dataManager.GetConvertedCharacterData();
var powerups = dataManager.GetConvertedPowerUpData();
```

These methods return strongly-typed objects with absolute values already calculated.

## Common Modding Patterns

### Modifying Character Base Stats
```csharp
var characterData = dataManager.GetConvertedCharacterData();
var myCharacter = characterData[CharacterType.ANTONIO][0];
myCharacter.power = 200;  // Double base power
myCharacter.maxHp = 150;  // Increase base HP
```

### Modifying Player Runtime Stats
```csharp
var player = GM.Core.Player;
if (player?.PlayerStats != null)
{
    player.PlayerStats.Power.SetValue(100);
    player.PlayerStats.MaxHp.SetValue(200);
    player.PlayerStats.Revivals.SetValue(5);
}
```

### Applying Changes
After modifying JSON data directly, always call:
```csharp
dataManager.ReloadAllData();
```

## Performance Considerations

- Stat calculations happen frequently during gameplay
- Use cached values when possible
- Avoid hooking into per-frame calculation methods
- Prefer one-time modifications during initialization