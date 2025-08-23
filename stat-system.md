# Stat System

This document describes the stat modification system in Vampire Survivors, based on analysis of decompiled IL2CPP code. The system appears to use wrapper types and incremental calculation methods.

## PlayerModifierStats Structure

Based on code analysis, this stat container appears to store character stat modifiers using wrapper types.

### EggFloat Properties
Based on the decompiled code, most stats appear to use the `EggFloat` wrapper type:

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
public unsafe float Recycle { get; set; }
```

**Raw Int (No Wrapper)**:
```csharp
public unsafe int Charm { get; set; }
```

### Implementation Notes

Based on code analysis, most stats use `EggFloat`, while `Revivals` uses `EggDouble`, and several use raw types. Properties appear to use specific casing: `ReRolls`, `MaxHp`, `InvulTimeBonus`. Assignments need to use the appropriate wrapper type.

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
Based on the decompiled code, weapons appear to have virtual methods that calculate final stats by combining base values with modifiers:

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
Based on code analysis, weapons appear to override these methods to implement calculations with specific formulas and bonuses.

## Character Base Stats

Based on the decompiled code, character base stats appear to be defined in `CharacterData` with the following property names:

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
Based on code analysis, the method for applying stat modifications appears to be:

```csharp
public unsafe void Upgrade(ModifierStats other, bool multiplicativeMaxHp = false)
```

This method appears to combine stat modifiers, with MaxHp handling that may be multiplicative.

## Incremental Level System

Based on the decompiled code, weapon data appears to use an incremental delta system:

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

## Data Access

Based on code analysis, DataManager converted methods appear to provide absolute values:

```csharp
var weapons = dataManager.GetConvertedWeaponData();
var characters = dataManager.GetConvertedCharacterData();
var powerups = dataManager.GetConvertedPowerUpData();
```

## Modding Examples

Based on the decompiled code structure, the following patterns appear to be valid for modifying stats:

### Modifying Character Base Stats
```csharp
var characterData = dataManager.GetConvertedCharacterData();
var myCharacter = characterData[CharacterType.ANTONIO][0];
myCharacter.power = 200;
myCharacter.maxHp = 150;
```

### Modifying Player Runtime Stats
```csharp
var player = GM.Core.Player;
if (player?.PlayerStats != null)
{
    player.PlayerStats.Power.SetValue(100);
    player.PlayerStats.MaxHp.SetValue(200);
    player.PlayerStats.Revivals.SetValue(5);
    player.PlayerStats.Recycle = 10f;
}
```

### Applying Changes
Based on code analysis, changes appear to require reloading data:
```csharp
dataManager.ReloadAllData();
```