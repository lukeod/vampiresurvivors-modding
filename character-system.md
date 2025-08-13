# Character System

## Overview

The character system in Vampire Survivors manages both player characters and enemies through the CharacterController class and associated data structures.

**Important Note**: This documentation reflects the IL2CPP interop layer structure. When working with mods, be aware that:
- Properties may use wrapper types like `EggFloat` and `EggDouble` instead of raw primitives
- Some properties shown as `{ get; set; }` in decompiled code may require specific accessor methods
- Actual runtime behavior may differ from the decompiled IL2CPP representation

## CharacterController

**Location**: `Il2CppVampireSurvivors.Objects.Characters.CharacterController`

The main class that manages character behavior, stats, and interactions.

### Core Properties

#### Identity and State

```csharp
public CharacterType _characterType;          // Character identifier
public int _level;                           // Current character level
public float _xp;                            // Current experience points
public float _currentHp;                     // Current health
```

#### Stat and Equipment Management

```csharp
public PlayerModifierStats PlayerStats;      // All stat modifiers
public CharacterWeaponsManager _weaponsManager;  // Weapon inventory
public CharacterAccessoriesManager _accessoriesManager;   // Accessories/items
```

### Initialization Methods

```csharp
// Character initialization and setup
public void InitCharacter(CharacterType characterType, int playerIndex, 
                         bool dontGetCharacterDataForCurrentLevel = false)

// Character spotlight initialization 
public void InitCharacterSpotlight()
```

**Note**: Additional private initialization methods exist but are not exposed in the public API.

### Experience and Leveling

```csharp
// Add experience points
public void AddXp(float value, XPMultiplierMode multiplierMode = XPMultiplierMode.Normal)
```

**Note**: Level up methods and related callbacks exist but their exact signatures may vary. The AddXp method handles growth multipliers and triggers level up when thresholds are reached.

### Utility Methods

```csharp
// Force character position
public void ForceSetPosition(Vector2 position)

// Upgrade player stats
public void PlayerStatsUpgrade(ModifierStats other, bool multiplicativeMaxHp)
```

## CharacterData

**Location**: `Il2CppVampireSurvivors.Data.Characters.CharacterData`

Defines the base configuration for a character.

### Identity Properties

```csharp
public string charName;                      // Character display name
public string description;                   // Character description text
public int level;                           // Character unlock level
```

### Starting Configuration

```csharp
public float price;                         // Character unlock cost
```

**Note**: Starting weapons and base character stats are handled through different systems in the actual implementation and may not be directly exposed as simple properties on CharacterData.

### Visual Properties

```csharp
public string textureName;       // Main texture reference
public string spriteName;        // Sprite identifier  
public string charSelTexture;    // Character selection texture
public string portraitName;      // Portrait image reference
```

## Character Types

Characters are identified by the `CharacterType` enum:

```csharp
public enum CharacterType
{
    ANTONIO,
    IMELDA,
    PASQUALINA,
    GENNARO,
    CHRISTINE,
    POPPEA,
    CONCETTA,
    // ... many more including DLC characters
}
```

## Accessing Character Data

### Get All Characters

```csharp
var dataManager = GM.Core?.DataManager;
if (dataManager != null)
{
    var characters = dataManager.GetConvertedCharacterData();
    foreach (var kvp in characters)
    {
        CharacterType type = kvp.Key;
        List<CharacterData> levels = kvp.Value;
        // Usually only one level per character
        CharacterData data = levels[0];
    }
}
```

### Get Current Player

```csharp
var player = GM.Core?.Player;
if (player != null)
{
    CharacterType charType = player._characterType;
    int level = player._level;
    float hp = player._currentHp;
    PlayerModifierStats stats = player.PlayerStats;
}
```

### Get All Players

```csharp
var allPlayers = GM.Core?.AllPlayers;
if (allPlayers != null)
{
    foreach (var player in allPlayers)
    {
        // Process each player
    }
}
```

## Character Weapons Manager

**Location**: `Il2CppVampireSurvivors.Objects.Characters.CharacterWeaponsManager`

Manages the character's equipped weapons:

```csharp
var weaponsManager = player._weaponsManager;
if (weaponsManager != null)
{
    // Access weapon inventory
    var weapons = weaponsManager.GetWeapons();
}
```

## Character Accessories Manager

**Location**: `Il2CppVampireSurvivors.Objects.Characters.CharacterAccessoriesManager`

Manages passive items and accessories:

```csharp
var accessoriesManager = player._accessoriesManager;
if (accessoriesManager != null)
{
    // Access equipped accessories
}
```

## Enemy Controllers

Enemies use the same CharacterController base but with specific enemy implementations:

```csharp
public class EnemyController : CharacterController
{
    // Enemy-specific behavior
}
```

## Common Operations

### Modifying Character Stats

```csharp
// Proper stat modification using wrapper types
var player = GM.Core?.Player;
if (player != null && player.PlayerStats != null)
{
    // EggFloat stats - use SetValue()
    player.PlayerStats.Power.SetValue(100.0f);
    player.PlayerStats.MoveSpeed.SetValue(2.0f);
    
    // EggDouble stats - use SetValue()
    player.PlayerStats.Revivals.SetValue(3.0);
    
    // Raw primitive stats - direct assignment
    player.PlayerStats.Shields = 2.0f;
    player.PlayerStats.Charm = 1;
}
```

### Adding Experience

```csharp
var player = GM.Core?.Player;
if (player != null)
{
    // Add 100 XP with normal growth multiplier
    player.AddXp(100, XPMultiplierMode.Normal);
}
```

### Healing/Damaging

```csharp
// Damage
player.GetDamaged(50.0f, HitVfxType.Default);

// Heal (add to current HP)
player._currentHp += 20.0f;
// Ensure not exceeding max - note: MaxHp is EggFloat
var maxHp = player.PlayerStats.MaxHp.Value;
if (player._currentHp > maxHp)
    player._currentHp = maxHp;
```

## EggFloat and EggDouble Wrappers

Most stats in PlayerModifierStats use specialized wrapper types:

### EggFloat Properties
```csharp
// These return EggFloat objects, not raw floats:
player.PlayerStats.Power      // EggFloat
player.PlayerStats.MoveSpeed  // EggFloat
player.PlayerStats.Area       // EggFloat
player.PlayerStats.Duration   // EggFloat
player.PlayerStats.Speed      // EggFloat
player.PlayerStats.Amount     // EggFloat
player.PlayerStats.Luck       // EggFloat
player.PlayerStats.Growth     // EggFloat
player.PlayerStats.Greed      // EggFloat
player.PlayerStats.Armor      // EggFloat
player.PlayerStats.Regen      // EggFloat
player.PlayerStats.MaxHp      // EggFloat
player.PlayerStats.Magnet     // EggFloat
player.PlayerStats.Curse      // EggFloat
player.PlayerStats.Banish     // EggFloat
player.PlayerStats.ReRolls    // EggFloat
player.PlayerStats.Cooldown   // EggFloat
```

### EggDouble Properties
```csharp
player.PlayerStats.Revivals   // EggDouble
```

### Raw Primitive Properties
```csharp
player.PlayerStats.Shields    // float
player.PlayerStats.Shroud     // float
player.PlayerStats.Defang     // float
player.PlayerStats.InvulTimeBonus // float
player.PlayerStats.Fever      // float
player.PlayerStats.Charm      // int
```

### Working with EggFloat/EggDouble
```csharp
// Getting values
float powerValue = player.PlayerStats.Power.Value;
double revivalsValue = player.PlayerStats.Revivals.Value;

// Setting values
player.PlayerStats.Power.SetValue(150.0f);
player.PlayerStats.Revivals.SetValue(5.0);

// Direct assignment for raw types
player.PlayerStats.Shields = 3.0f;
player.PlayerStats.Charm = 2;
```

## Best Practices

### 1. Null Check Everything

```csharp
var player = GM.Core?.Player;
if (player != null && player.PlayerStats != null)
{
    // Safe to use
}
```

### 2. Use Proper Stat Wrappers

Most stats use EggFloat, but some use different types:

```csharp
// EggFloat stats (Most stats)
player.PlayerStats.Power.SetValue(10);
player.PlayerStats.MoveSpeed.SetValue(2.0f);
player.PlayerStats.Area.SetValue(1.5f);

// EggDouble stats (Only Revivals)
player.PlayerStats.Revivals.SetValue(3);

// Raw float stats (Shields, Shroud, Defang, InvulTimeBonus, Fever)
player.PlayerStats.Shields = 2.0f;
player.PlayerStats.Shroud = 0.5f;

// Raw int stats (Charm)
player.PlayerStats.Charm = 1;
```

### 3. Handle Nullable Types

```csharp
if (characterData.startingWeapon.HasValue)
{
    WeaponType weapon = characterData.startingWeapon.Value;
}
```

### 4. Character vs Enemy

Remember that both players and enemies use CharacterController:

```csharp
if (character is EnemyController enemy)
{
    // Enemy-specific logic
}
else
{
    // Player logic
}
```

## Hook Points

### Character Creation

```csharp
[HarmonyPatch(typeof(CharacterController), "InitCharacter")]
[HarmonyPostfix]
public static void OnCharacterInit(CharacterController __instance, 
                                  CharacterType characterType)
{
    // Character has been initialized
}
```

### Level Up

```csharp
[HarmonyPatch(typeof(CharacterController), "LevelUp")]
[HarmonyPrefix]
public static void OnLevelUp(CharacterController __instance)
{
    // Character is about to level up
}
```

### Experience Gain

```csharp
[HarmonyPatch(typeof(CharacterController), "AddXp")]
[HarmonyPrefix]
public static void OnAddXp(CharacterController __instance, 
                          ref float value, 
                          XPMultiplierMode multiplierMode)
{
    // Can modify XP value before it's added
    value *= 2.0f; // Double XP gain
}
```