# Arcana System Documentation

## Overview
The Arcana system in Vampire Survivors provides powerful passive effects that significantly alter gameplay mechanics. Arcanas are unlockable tarot-themed modifiers that trigger on specific game events and can dramatically change how weapons, characters, and game mechanics function.

## Arcana Types

### Core Arcana Enumeration
Arcanas are defined in the `ArcanaType` enum with tarot-inspired naming:

```csharp
public enum ArcanaType
{
    VOID = -1,
    T00_KILLER,
    T01_AQUARIUS,
    T02_TWILIGHT,
    T03_TRAGIC,
    T04_AWAKE,
    T05_CRASH,
    T06_SARABANDE,
    T07_IRON_BLUE,
    T08_MAD_FOREST,
    T09_DIVINE,
    T10_BEGINNING,
    T11_PEARLS,
    T12_OUT_OF_TIME,
    T13_WICKED,
    T14_JEWELS,
    T15_GOLD,
    T16_SLASH,
    T17_PAINTING,
    T18_ILLUSIONS,
    T19_FIRE,
    T20_SINKING,
    T21_BLOODY,
    D00_STAKE_TO_YOUR_HEART,
    D01_SAPPHIRE_MIST,
    D02_EMERALD_ELEGY,
    D03_BREAD_ANATHEMA,
    D04_TOWERING_DOLL,
    D05_PALE_DIAMOND,
    D06_BOLERO,
    D07_tbd_bouncy,
    D08_EDGE_OF_THE_EARTH,
    D09_HEIR_OF_FATE,
    D10_FROM_THE_FUTURE,
    D11_HIDDEN_IN_SMOKE,
    D12_CRYSTAL_CRIES,
    D13_MAD_MOON,
    D14_JAWS_SCORCHED_SKY,
    D15_EVERYTHING_TO_LOSE,
    D16_TINKER_PARTITA,
    D17_SIGN_OF_BLOOD,
    D18_VICTORIAN_HORROR,
    D19_RIPPING_SILENCE,
    D20_FANTASIA,
    D21_JETBLACK
}
```

### Arcana Categories

**Base Game Arcanas (T00-T21)**:
- Cover core gameplay mechanics
- Generally available in all game modes
- Include fundamental stat modifications and special effects

**DLC Arcanas (D00-D21)**:
- Introduced with expansion content
- May have DLC-specific requirements or interactions
- Often tied to DLC characters or weapons

## Arcana Manager System

The `ArcanaManager` class handles all arcana-related functionality, including tracking active arcanas and responding to game events.

### Active Arcana Tracking
The ArcanaManager maintains a list of currently active arcanas:

```csharp
public List<ArcanaType> ActiveArcanas;  // Currently active arcanas
```

The manager also tracks specific arcana states through boolean properties:
```csharp
public bool _hasWickedSeason;     // T13_WICKED active
public bool _hasSilentSanctuary;  // T19_FIRE active  
public bool _hasAstronomia;       // T20_SINKING active
public bool _hasSapphireMist;     // D01_SAPPHIRE_MIST active
public bool _hasBreadAnathema;    // D03_BREAD_ANATHEMA active
public bool _hasMoonlightBolero;  // D06_BOLERO active
public bool _hasHailFromTheFuture; // D10_FROM_THE_FUTURE active
public bool _hasJetBlackWeapon;   // D21_JETBLACK active
public bool _hasCrystalCries;     // D12_CRYSTAL_CRIES active

// Additional effect states
public bool _HealOnCoins_k__BackingField;
public bool _CoinFever_k__BackingField;
public bool _MadGroove_k__BackingField;
public bool _CanGather_k__BackingField;
public bool _HasDivineBloodline_k__BackingField;
public bool _PewPew_k__BackingField;
```

### Core Event Handling
The `ArcanaManager` responds to various game events and triggers appropriate arcana effects:

```csharp
// Weapon-related events
public void OnWeaponFired(Weapon weapon)

// Player health-related events
public void OnPlayerHPDamage(CharacterController character, float rawValue)
public void OnPlayerHPRecovery(CharacterController character, float rawValue)
public void OnPlayerHPRecovery(CharacterController character, float rawValue, float actualRecovery)
public void OnPlayerCriticalHPTreshold(CharacterController character, float rawValue)

// Character progression events
public void OnPlayerLevelUp(CharacterController character)
```

### Event-Driven Architecture
Arcanas operate on an event-driven system where:
1. Game events trigger arcana checks
2. Active arcanas respond to relevant events
3. Effects are applied based on arcana-specific logic

## Weapon Integration

### Arcana Detection in Weapons
Weapons can check for and respond to active arcanas:

```csharp
// Check if a specific arcana is currently active
public bool HasActiveArcanaOfType(ArcanaType type)

// General arcana checking (called by weapon systems)
public virtual void CheckArcanas()

// Specific arcana implementations
public void CheckBeginningArcana()  // For T10_BEGINNING (Waltz of Pearls)
```

### Weapon Behavior Modification
Arcanas can fundamentally change how weapons behave:
- Modify damage calculations
- Alter firing patterns
- Change visual effects
- Add special interactions with enemies or pickups

## Arcana Effects Categories

### Stat Modification Arcanas
Some arcanas provide direct stat bonuses or penalties:
- Increased damage, area, speed, or other weapon stats
- Character stat improvements (health, movement, etc.)
- Multiplicative vs. additive bonuses

### Behavior Modification Arcanas
Others change fundamental game mechanics:
- Weapon evolution requirements
- Pickup effects and spawning
- Enemy behavior and interactions
- Time manipulation effects

### Event-Triggered Arcanas
Many arcanas activate on specific conditions:
- Taking damage below certain thresholds
- Leveling up characters
- Killing specific enemy types
- Time-based triggers

## Data Storage and Configuration

### Arcana Data Structure
Arcana configurations are stored in the DataManager:

```csharp
public JObject _allArcanasJson;  // Raw JSON data for all arcanas
public Dictionary<ArcanaType, ArcanaData> AllArcanas; // Converted arcana data dictionary
```

The `ArcanaData` class contains the following properties:
```csharp
public class ArcanaData
{
    public int arcanaType;                    // Enum value cast to int
    public string name;                       // Display name
    public string description;                // Effect description
    public List<Object> weapons;             // Associated weapons
    public List<Object> items;               // Associated items
    public string texture;                   // UI texture name
    public string frameName;                 // UI frame name
    public bool enabled;                     // Whether arcana is enabled
    public bool unlocked;                    // Whether arcana is unlocked
    public bool major;                       // Whether it's a major arcana
    public bool hidden;                      // Whether hidden from UI
    public bool alwaysHidden;               // Whether permanently hidden
    public ContentGroupType contentGroup;   // DLC/content group association
    
    // Localization methods
    public string GetLocalizedNameTerm(ArcanaType t);
    public string GetLocalizedDescriptionTerm(ArcanaType t);
    public string GetLocalPrefix(ArcanaType t);
}
```

### Arcana Data Access
Access arcana data through the DataManager's AllArcanas property:

```csharp
var allArcanas = dataManager.AllArcanas; // Dictionary<ArcanaType, ArcanaData>
var specificArcana = dataManager.AllArcanas[ArcanaType.T00_KILLER];
```

## Arcana Selection and Unlocking

### Unlock Requirements
Arcanas typically have unlock requirements:
- Specific achievements
- Character level thresholds
- Game progression milestones
- DLC ownership for DLC arcanas

### Selection Process
Players choose arcanas during character selection or through special in-game events.

## Integration with Other Systems

### Character System
Arcanas can affect character abilities and stats:
- Modify base character statistics
- Change character-specific abilities
- Alter character evolution paths

### Evolution System
Some arcanas modify weapon evolution requirements:
- Change required synergy items
- Alter evolution conditions
- Enable special evolution paths

### Stage and Enemy Interactions
Arcanas can affect stage mechanics:
- Enemy spawn patterns
- Environmental effects
- Stage-specific bonuses or penalties

## Common Modding Scenarios

### Creating Custom Arcana Effects
```csharp
// Hook into arcana checking
[HarmonyPatch(typeof(Weapon), "CheckArcanas")]
[HarmonyPostfix]
public static void CustomArcanaEffects(Weapon __instance)
{
    if (__instance.HasActiveArcanaOfType(ArcanaType.T00_KILLER))
    {
        // Custom effect for Gemini arcana
        // Modify weapon behavior based on kill count
    }
}
```

### Modifying Arcana Trigger Conditions
```csharp
// Hook into health damage events
[HarmonyPatch(typeof(ArcanaManager), "OnPlayerHPDamage")]
[HarmonyPostfix]
public static void CustomHealthArcanaEffects(CharacterController character, float rawValue)
{
    // Custom logic for health-based arcana triggers
    if (character._currentHp < character.MaxHp * 0.5f)
    {
        // Trigger custom low-health effects
    }
}

// Hook into weapon firing events
[HarmonyPatch(typeof(ArcanaManager), "OnWeaponFired")]
[HarmonyPostfix]
public static void CustomWeaponArcanaEffects(Weapon weapon)
{
    // Custom logic for weapon-based arcana triggers
    if (weapon.HasActiveArcanaOfType(ArcanaType.T00_KILLER))
    {
        // Custom effect when Gemini is active and weapon fires
    }
}
```

### Adding Arcana-Weapon Interactions
```csharp
// Custom weapon behavior with arcana integration
public class ModdedWeapon : Weapon
{
    public override void CheckArcanas()
    {
        base.CheckArcanas();
        
        if (HasActiveArcanaOfType(ArcanaType.T02_TWILIGHT))
        {
            // Custom interaction with Magical Arsenal arcana
            // Modify weapon stats or behavior
        }
    }
}
```

## Performance Considerations

### Event Frequency
Arcana checks can occur frequently during gameplay:
- Every weapon attack for combat arcanas
- Every frame for continuous effect arcanas
- Every game event for trigger-based arcanas

### Optimization Guidelines
1. **Cache arcana states**: Avoid repeated lookups for the same arcana
2. **Efficient condition checking**: Use fast comparisons for trigger conditions
3. **Batch arcana effects**: Group similar modifications together
4. **Avoid complex calculations**: Keep arcana effect calculations simple

## Advanced Arcana Patterns

### Conditional Arcana Stacking
Some arcanas may interact with each other:
- Multiplicative effects when multiple arcanas are active
- Exclusive effects that override each other
- Synergistic combinations that create new behaviors

### Dynamic Arcana Effects
Advanced arcanas may have effects that change over time:
- Scaling effects based on game progression
- Time-limited powerful effects
- Effects that trigger under specific combinations of conditions

## Testing and Validation

### Arcana Effect Testing
When modifying arcana systems:
1. Test with different character combinations
2. Verify effects persist across game sessions
3. Check for conflicts with weapon evolutions
4. Ensure effects scale appropriately with difficulty

### Common Issues
1. **Effect stacking**: Multiple instances of the same effect
2. **Save compatibility**: Modified arcana effects affecting save games
3. **Performance impact**: Too many active arcana checks causing lag
4. **Balance concerns**: Overpowered arcana combinations

## Debugging Arcana Systems

### Arcana State Checking
```csharp
// Check current active arcanas through ArcanaManager
var arcanaManager = GameManager.Instance.ArcanaManager;
var activeArcanas = arcanaManager.ActiveArcanas; // List<ArcanaType>
foreach (var arcana in activeArcanas)
{
    MelonLogger.Msg($"Active Arcana: {arcana}");
}

// Check specific arcana states
if (arcanaManager._hasWickedSeason)
{
    MelonLogger.Msg("Wicked Season arcana is active");
}

// Access arcana data
var dataManager = GameManager.Instance.DataManager;
var arcanaData = dataManager.AllArcanas[ArcanaType.T00_KILLER];
MelonLogger.Msg($"Arcana Name: {arcanaData.name}");
MelonLogger.Msg($"Arcana Description: {arcanaData.description}");
```

### Effect Validation
```csharp
// Validate arcana effects are being applied
[HarmonyPatch(typeof(Weapon), "HasActiveArcanaOfType")]
[HarmonyPostfix]
public static void LogArcanaChecks(bool __result, ArcanaType type)
{
    if (__result)
    {
        MelonLogger.Msg($"Arcana {type} is active");
    }
}
```