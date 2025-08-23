# Character System

Based on analysis of decompiled IL2CPP code, the character system appears to manage player characters and enemies through the CharacterController class and associated data structures.

## CharacterController

**Location**: `Il2CppVampireSurvivors.Objects.Characters.CharacterController`

Based on code analysis, this class appears to manage character behavior, stats, and interactions.

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
                         bool asRemote, bool dontGetCharacterDataForCurrentLevel = false)

// Character spotlight initialization 
public void InitCharacterSpotlight()
```


### Experience and Leveling

```csharp
// Add experience points
public void AddXp(float value, XPMultiplierMode multiplierMode = XPMultiplierMode.Normal)
```


### Utility Methods

```csharp
// Force character position
public void ForceSetPosition(Vector2 position)

// Upgrade player stats
public void PlayerStatsUpgrade(ModifierStats other, bool multiplicativeMaxHp)
```

## CharacterData

**Location**: `Il2CppVampireSurvivors.Data.Characters.CharacterData`

Inferred from decompiled code, this class appears to define the base configuration for a character.

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


### Visual Properties

```csharp
public string textureName;       // Main texture reference
public string spriteName;        // Sprite identifier  
public string charSelTexture;    // Character selection texture
public string portraitName;      // Portrait image reference
```

## Character Types

Based on code analysis, characters appear to be identified by the `CharacterType` enum:

```csharp
public enum CharacterType
{
    ANTONIO,
    IMELDA,
    PASQUALINA,
    GENNARO,
    CRISTINA,
    POPPEA,
    CONCETTA,
    // Additional character types exist based on code analysis
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
        // Based on code analysis, typically one level per character
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

**Location**: `Il2CppVampireSurvivors.Objects.CharacterWeaponsManager`

Based on code analysis, this class appears to manage the character's equipped weapons:

```csharp
var weaponsManager = player._weaponsManager;
if (weaponsManager != null)
{
    // Inferred from code: inherits from EquipmentManager
    weaponsManager.SetWeaponsActive(true);
    weaponsManager.SetMaxWeaponCount(6, 0);
}
```

## Character Accessories Manager

**Location**: `Il2CppVampireSurvivors.Objects.CharacterAccessoriesManager`

Based on code analysis, this class appears to manage passive items and accessories:

```csharp
var accessoriesManager = player._accessoriesManager;
if (accessoriesManager != null)
{
    // Access equipped accessories
}
```

## Character Skill Cards Manager

**Location**: `Il2CppVampireSurvivors.Objects.Characters.CharacterSkillCardsManager`

Based on code analysis, this class appears to manage character-specific skill cards and abilities:

```csharp
var skillCardsManager = player.CharacterSkillCardsManager;
if (skillCardsManager != null)
{
    // Based on code analysis: access character skill cards
    var cards = skillCardsManager.CharacterCards;
    foreach (var card in cards)
    {
        // Process each skill card
    }
}
```

### Skill Card System Properties

```csharp
public List<CharacterSkillCard_Base> CharacterCards;  // List of equipped skill cards
public float SkillCards_Mult;                        // Skill card multiplier
```

### Skill Card Methods

```csharp
// Add a skill card to the character
public void AddSkillCard(CharacterSkillCard_Base card)

// Called when a skill card is added (virtual, can be overridden)
public virtual void OnSkillCardAdded(CharacterSkillCard_Base card)
```

## Enemy Controllers

Based on code analysis, enemies appear to use the same CharacterController base but with specific enemy implementations:

```csharp
public class EnemyController : CharacterController
{
    // Inferred from code: enemy-specific behavior
}
```

## Common Operations

### Modifying Character Stats

```csharp
// Based on code analysis: stat modification using wrapper types
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
    // Example: add 100 XP with normal growth multiplier
    player.AddXp(100, XPMultiplierMode.Normal);
}
```

### Healing/Damaging

```csharp
// Damage with optional parameters
player.GetDamaged(50.0f, HitVfxType.Default, 1f, WeaponType.VOID, true);

// Based on code analysis: heal by adding to current HP
player._currentHp += 20.0f;
// Inferred from code: ensure not exceeding max (MaxHp is EggFloat)
var maxHp = player.PlayerStats.MaxHp.Value;
if (player._currentHp > maxHp)
    player._currentHp = maxHp;
```

## EggFloat and EggDouble Wrappers

Based on code analysis, PlayerModifierStats appears to use specialized wrapper types for most stats:

### EggFloat Properties
```csharp
// Based on code analysis, these return EggFloat objects, not raw floats:
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
player.PlayerStats.Recycle    // float
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
player.PlayerStats.Recycle = 0.1f;
player.PlayerStats.Charm = 2;
```

## Usage Guidelines

### Null Safety
```csharp
var player = GM.Core?.Player;
if (player != null && player.PlayerStats != null)
{
    // Based on code analysis: appears safe to use
}
```

### Stat Wrapper Types
```csharp
// EggFloat stats
player.PlayerStats.Power.SetValue(10);
player.PlayerStats.MoveSpeed.SetValue(2.0f);

// EggDouble stats
player.PlayerStats.Revivals.SetValue(3);

// Raw types
player.PlayerStats.Shields = 2.0f;
player.PlayerStats.Charm = 1;
```

### Character vs Enemy
```csharp
if (character is EnemyController enemy)
{
    // Inferred from code: enemy-specific logic
}
else
{
    // Inferred from code: player logic
}
```

## Hook Points

### Character Creation

```csharp
[HarmonyPatch(typeof(CharacterController), "InitCharacter")]
[HarmonyPostfix]
public static void OnCharacterInit(CharacterController __instance, 
                                  CharacterType characterType, 
                                  int playerIndex, 
                                  bool asRemote)
{
    // Based on code analysis: character has been initialized
}
```

### Level Up

```csharp
[HarmonyPatch(typeof(CharacterController), "LevelUp")]
[HarmonyPrefix]
public static void OnLevelUp(CharacterController __instance)
{
    // Based on code analysis: character is about to level up
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
    // Example: modify XP value before addition
    value *= 2.0f;
}
```

### Skill Card System

```csharp
[HarmonyPatch(typeof(CharacterController), "AddSkillCard")]
[HarmonyPostfix]
public static void OnSkillCardAdded(CharacterController __instance, 
                                   CharacterSkillCard_Base card)
{
    // Based on code analysis: character skill card has been added
}

[HarmonyPatch(typeof(CharacterController), "OnSkillCardAdded")]
[HarmonyPrefix]
public static void OnSkillCardAddedEvent(CharacterController __instance, 
                                        CharacterSkillCard_Base card)
{
    // Based on code analysis: called when skill card is processed
}
```