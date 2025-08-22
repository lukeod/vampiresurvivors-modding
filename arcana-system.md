# Arcana System

Provides powerful passive effects that alter gameplay mechanics. Arcanas are unlockable tarot-themed modifiers that trigger on specific game events.

## Arcana Types

### Core Arcana Enumeration
Arcanas are defined in the `ArcanaType` enum with tarot-inspired naming:

```csharp
public enum ArcanaType
{
    VOID = -1,
    T00_KILLER = 0,
    T01_AQUARIUS = 1,
    T02_TWILIGHT = 2,
    T03_TRAGIC = 3,
    T04_AWAKE = 4,
    T05_CRASH = 5,
    T06_SARABANDE = 6,
    T07_IRON_BLUE = 7,
    T08_MAD_FOREST = 8,
    T09_DIVINE = 9,
    T10_BEGINNING = 10,
    T11_PEARLS = 11,
    T12_OUT_OF_TIME = 12,
    T13_WICKED = 13,
    T14_JEWELS = 14,
    T15_GOLD = 15,
    T16_SLASH = 16,
    T17_PAINTING = 17,
    T18_ILLUSIONS = 18,
    T19_FIRE = 19,
    T20_SINKING = 20,
    T21_BLOODY = 21,
    D00_STAKE_TO_YOUR_HEART = 22,
    D01_SAPPHIRE_MIST = 23,
    D02_EMERALD_ELEGY = 24,
    D03_BREAD_ANATHEMA = 25,
    D04_TOWERING_DOLL = 26,
    D05_PALE_DIAMOND = 27,
    D06_BOLERO = 28,
    D07_tbd_bouncy = 29,
    D08_EDGE_OF_THE_EARTH = 30,
    D09_HEIR_OF_FATE = 31,
    D10_FROM_THE_FUTURE = 32,
    D11_HIDDEN_IN_SMOKE = 33,
    D12_CRYSTAL_CRIES = 34,
    D13_MAD_MOON = 35,
    D14_JAWS_SCORCHED_SKY = 36,
    D15_EVERYTHING_TO_LOSE = 37,
    D16_TINKER_PARTITA = 38,
    D17_SIGN_OF_BLOOD = 39,
    D18_VICTORIAN_HORROR = 40,
    D19_RIPPING_SILENCE = 41,
    D20_FANTASIA = 42,
    D21_JETBLACK = 43,
    B001_ANTONIO = 101,
    B002_IMELDA = 102,
    B003_PASQUALINA = 103,
    B004_GENNARO = 104,
    B005_ARCA = 105,
    B006_PORTA = 106,
    B007_LAMA = 107,
    B008_POE = 108,
    B009_CLERICI = 109,
    B010_DOMMARIO = 110,
    B011_KROCHI = 111,
    B012_CHRISTINE = 112,
    B013_PUGNALA = 113,
    B014_GIOVANNA = 114,
    B015_POPPEA = 115,
    B016_CONCETTA = 116,
    B017_MORTACCIO = 117,
    B018_CAVALLO = 118,
    B019_RAMBA = 119,
    B020_OSOLE = 120,
    B021_AMBROJOE = 121,
    B022_GALLO = 122,
    B023_DIVANO = 123,
    B024_ZIASSUNTA = 124
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

**Character Arcanas (B001-B024)**:
- Character-specific arcanas tied to individual characters
- Named after specific characters (Antonio, Imelda, Pasqualina, etc.)
- Provide character-specific bonuses or unlock requirements

## Arcana Manager System

Handles arcana functionality, tracking active arcanas and responding to game events.

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
public bool _hasMadMoon;          // D13_MAD_MOON active
public bool _hasVictorianHorror;  // D18_VICTORIAN_HORROR active

// Additional effect states
public bool HealOnCoins;          // Heal when collecting coins
public bool CoinFever;            // Coin fever effect active
public bool MadGroove;            // Mad groove effect active
public bool CanGather;            // Can gather items effect
public bool HasDivineBloodline;   // Divine bloodline effect active
public bool PewPew;               // Pew pew effect active
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
Game events trigger arcana checks, active arcanas respond to relevant events, and effects are applied based on arcana-specific logic.

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
Arcanas can modify damage calculations, alter firing patterns, change visual effects, and add special interactions.

## Arcana Effects

- **Stat Modification**: Direct stat bonuses or penalties
- **Behavior Modification**: Changes to game mechanics
- **Event-Triggered**: Activate on specific conditions

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
    public List<Il2CppSystem.Object> weapons; // Associated weapons
    public List<Il2CppSystem.Object> items;   // Associated items
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
    
    // Check for character-specific arcanas
    if (__instance.HasActiveArcanaOfType(ArcanaType.B001_ANTONIO))
    {
        // Custom effect for Antonio's character arcana
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
    
    // Check for Mad Moon or Victorian Horror arcanas
    if (weapon.HasActiveArcanaOfType(ArcanaType.D13_MAD_MOON))
    {
        // Custom effect for Mad Moon arcana
    }
    
    if (weapon.HasActiveArcanaOfType(ArcanaType.D18_VICTORIAN_HORROR))
    {
        // Custom effect for Victorian Horror arcana
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
        
        // Handle character-specific arcana interactions
        if (HasActiveArcanaOfType(ArcanaType.B005_ARCA))
        {
            // Custom interaction with Arca's character arcana
        }
    }
}
```

## Performance Considerations

Arcana checks occur frequently. Optimize by caching states, using efficient condition checking, batching effects, and avoiding complex calculations.

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

## Common Issues
- **Effect stacking**: Multiple instances of same effect
- **Save compatibility**: Modified effects affecting save games
- **Performance**: Too many active checks causing lag
- **Balance**: Overpowered combinations

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

if (arcanaManager._hasMadMoon)
{
    MelonLogger.Msg("Mad Moon arcana is active");
}

if (arcanaManager._hasVictorianHorror)
{
    MelonLogger.Msg("Victorian Horror arcana is active");
}

// Access arcana data
var dataManager = GameManager.Instance.DataManager;
var arcanaData = dataManager.AllArcanas[ArcanaType.T00_KILLER];
MelonLogger.Msg($"Arcana Name: {arcanaData.name}");
MelonLogger.Msg($"Arcana Description: {arcanaData.description}");

// Access character arcana data
var characterArcana = dataManager.AllArcanas[ArcanaType.B001_ANTONIO];
MelonLogger.Msg($"Character Arcana: {characterArcana.name}");
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