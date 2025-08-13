# Data Manager

## Overview

The DataManager is the central repository for all game data in Vampire Survivors. It handles loading, storing, and providing access to JSON-based configuration data for weapons, characters, enemies, stages, and more.

**Location**: `Il2CppVampireSurvivors.Data.DataManager`

## JSON Data Storage

DataManager stores game data as JObject instances (from Newtonsoft.Json):

### Core Data Properties

```csharp
public JObject _allWeaponDataJson;     // Weapon configurations
public JObject _allCharactersJson;     // Character definitions
public JObject _allEnemiesJson;        // Enemy data
public JObject _allItemsJson;          // Item definitions
public JObject _allPowerUpsJson;       // PowerUp definitions
public JObject _allPropsJson;          // Environment props
public JObject _allStagesJson;         // Stage configurations
public JObject _allArcanasJson;        // Arcana system data
public JObject _allHitVfxDataJson;     // Hit visual effects
public JObject _allMusicDataJson;      // Music configurations
public JObject _allLimitBreakDataJson; // Limit break data
public JObject _allAchievementsJson;   // Achievement definitions
public JObject _allSecretsJson;        // Secret unlocks
public JObject _allAdventuresJson;     // Adventure mode data
public JObject _allStageSetJson;       // Stage set configurations
public JObject _allAdventureMerchantsJson; // Adventure merchant data
public JObject _allCustomMerchantsJson;   // Custom merchant configurations
public JObject _allAlbumData;             // Album/music collection data
```

## DLC Data Storage

DLC content is stored separately in typed dictionaries:

```csharp
Dictionary<DlcType, Dictionary<WeaponType, List<WeaponData>>> _dlcWeaponData;
Dictionary<DlcType, Dictionary<CharacterType, List<CharacterData>>> _dlcCharacterData;
Dictionary<DlcType, Dictionary<PowerUpType, List<PowerUpData>>> _dlcPowerUpData;
Dictionary<DlcType, Dictionary<StageType, List<StageData>>> _dlcStageData;
Dictionary<DlcType, Dictionary<EnemyType, List<EnemyData>>> _dlcEnemyData;
Dictionary<DlcType, Dictionary<BgmType, MusicData>> _dlcMusicData;
Dictionary<DlcType, HashSet<string>> _dlcSfxData;
```

## Adventure Data Storage

Adventure mode content is stored in separate dictionaries:

```csharp
Dictionary<CharacterType, List<CharacterData>> _adventureCharacterData;
Dictionary<StageType, List<StageData>> _adventureStageData;
```

## Access Patterns

### Getting DataManager Instance

```csharp
// Primary method - through GameManager
var dataManager = GM.Core?.DataManager;

// Alternative - through SoundManager static field
var dataManager = SoundManager._dataManager;
```

### GetConverted Methods (Recommended)

DataManager provides converted data methods that return strongly-typed objects with **absolute values** (not incremental deltas):

```csharp
// Get all weapons with absolute values
Dictionary<WeaponType, List<WeaponData>> weapons = dataManager.GetConvertedWeapons();

// Get specific weapon levels
List<WeaponData> whipLevels = weapons[WeaponType.WHIP];
WeaponData level3Whip = whipLevels[2]; // 0-indexed

// Other data types
var characters = dataManager.GetConvertedCharacterData();
var enemies = dataManager.GetConvertedEnemyData();
var powerups = dataManager.GetConvertedPowerUpData();
var stages = dataManager.GetConvertedStages();

// Access item and arcana data through properties (not GetConverted methods)
var items = dataManager.AllItems;         // Dictionary<ItemType, ItemData>
var arcanas = dataManager.AllArcanas;     // Dictionary<ArcanaType, ArcanaData>

// DLC-specific data
var dlcWeapons = dataManager.GetConvertedDlcWeaponData(DlcType.Foscari);
var dlcCharacters = dataManager.GetConvertedDlcCharacterData(DlcType.Moonspell);
```

### Alternative Access via Properties

Some data types are also accessible through properties that return single data instances:

```csharp
// Access complete data dictionaries via properties
var items = dataManager.AllItems;       // Dictionary<ItemType, ItemData>
var arcanas = dataManager.AllArcanas;   // Dictionary<ArcanaType, ArcanaData>
var props = dataManager.AllProps;       // Dictionary<PropType, PropData>

// Adventure-specific data access
var adventures = dataManager.AllAdventures;              // Dictionary<AdventureType, AdventureData>
var adventureMerchants = dataManager.AllAdventureMerchantsData; // Dictionary<CharacterType, CustomMerchantData>
var adventureCharacters = dataManager.AdventureCharacterData;   // Dictionary<CharacterType, List<CharacterData>>
var adventureStages = dataManager.AdventureStageData;           // Dictionary<StageType, List<StageData>>
var adventureBestiary = dataManager.AdventureBestiaryData;      // Dictionary<EnemyType, List<EnemyData>>

// Or get individual prop data
PropData specificProp = dataManager.GetPropData(PropType.CANDLE);
```

**Benefits of GetConverted Methods:**
- Returns strongly-typed objects (not JObject/JArray)
- Values are already absolute (no delta calculation needed)
- Works with IL2Cpp collections naturally
- Includes all base game and DLC content

## Direct JSON Manipulation

For advanced modifications, you can directly manipulate the JSON data:

### Reading JSON Data

```csharp
// Get weapon JSON data
JObject weaponJson = dataManager._allWeaponDataJson;
JArray whipLevels = weaponJson["WHIP"] as JArray;

// Read specific values
float level1Power = whipLevels[0]["power"].Value<float>();
string weaponName = whipLevels[0]["name"].Value<string>();
```

### Modifying JSON Data

```csharp
// CRITICAL: Remember levels 2-8 are incremental deltas!
JArray whipLevels = weaponJson["WHIP"] as JArray;

// Level 1: Absolute values
whipLevels[0]["power"] = 50;  // Sets base power to 50

// Levels 2-8: Incremental deltas
whipLevels[1]["power"] = 20;  // Adds 20 to level 1 (total: 70)
whipLevels[2]["power"] = 15;  // Adds 15 to level 2 (total: 85)

// Apply changes
dataManager.ReloadAllData();
```

## Incremental Level System

**CRITICAL**: Weapon and PowerUp levels use an incremental delta system:

- **Level 1**: Contains absolute base values
- **Levels 2-8**: Contain incremental changes from the previous level

### Example JSON Structure

```json
"MAGIC_MISSILE": [
    { "level": 1, "power": 10, "speed": 100 },  // Base: power=10, speed=100
    { "level": 2, "power": 10 },                  // Adds: power+10 (total: 20)
    { "level": 3, "power": 10, "speed": 10 }     // Adds: power+10, speed+10 (total: 30, 110)
]
```

### Calculating Total Stats

```csharp
public static float CalculatePowerAtLevel(JArray levels, int targetLevel)
{
    float totalPower = levels[0]["power"].Value<float>();  // Base value
    
    for (int i = 1; i < targetLevel && i < levels.Count; i++)
    {
        var powerToken = levels[i]["power"];
        if (powerToken != null)
        {
            totalPower += powerToken.Value<float>();  // Add delta
        }
    }
    
    return totalPower;
}
```

## Data Loading and Reload

### Initial Load

```csharp
// Called during game initialization (note: this is a private method)
private void LoadBaseJObjects()
{
    // Loads all JSON data from game files
    // Populates _all*Json properties
}
```

### Reload Data

```csharp
// Force reload all data (applies JSON modifications)
public void ReloadAllData()
{
    // Re-processes all JSON data
    // Updates converted data caches
    // Triggers data reload events
}
```

### Hook Points

```csharp
// Hook after initial data load
[HarmonyPatch(typeof(DataManager), "LoadBaseJObjects")]
[HarmonyPostfix]
public static void OnDataLoad(DataManager __instance)
{
    // Data is loaded and ready for modification
}

// Hook after data reload
[HarmonyPatch(typeof(DataManager), "ReloadAllData")]
[HarmonyPostfix]
public static void OnDataReload(DataManager __instance)
{
    // Data has been reloaded
    // Safe to access GetConverted methods
}
```

## Working with IL2Cpp Collections

The GetConverted methods return IL2Cpp collections:

```csharp
// IL2Cpp Dictionary (from game)
using Il2CppSystem.Collections.Generic;
Dictionary<WeaponType, List<WeaponData>> weapons = dataManager.GetConvertedWeapons();

// Iterate with foreach
foreach (var kvp in weapons)
{
    WeaponType type = kvp.Key;
    List<WeaponData> levels = kvp.Value;
    
    // Always null-check IL2Cpp objects
    if (levels != null && levels.Count > 0)
    {
        // Process weapon data
        MelonLogger.Msg($"Weapon {type} has {levels.Count} levels");
    }
}

// Convert to System collections for complex operations
var systemDict = new System.Collections.Generic.Dictionary<string, object>();
foreach (var kvp in weapons)
{
    systemDict[kvp.Key.ToString()] = ConvertWeaponData(kvp.Value);
}
```

## Best Practices

### 1. Use GetConverted Methods
Prefer `GetConverted*` methods over direct JSON manipulation:
- Strongly typed
- Absolute values
- Easier to work with

### 2. Remember the Delta System
When modifying JSON directly, always remember:
- Level 1 = absolute values
- Levels 2+ = incremental deltas

### 3. Null Check IL2Cpp Objects
```csharp
var weapons = dataManager?.GetConvertedWeapons();
if (weapons != null && weapons.Count > 0)
{
    // Safe to use
}
```

### 4. Call ReloadAllData After Modifications
```csharp
// Modify JSON
weaponJson["WHIP"][0]["power"] = 100;

// Must reload for changes to take effect
dataManager.ReloadAllData();
```

### 5. Cache Expensive Lookups
```csharp
// Cache converted data if using multiple times
private static Dictionary<WeaponType, List<WeaponData>> _cachedWeapons;

public static void RefreshCache()
{
    _cachedWeapons = GM.Core?.DataManager?.GetConvertedWeapons();
}
```

## Common Data Types

### Enumerations
- `WeaponType` - Weapon identifiers
- `CharacterType` - Character identifiers
- `EnemyType` - Enemy identifiers
- `StageType` - Stage identifiers
- `PowerUpType` - PowerUp identifiers
- `ItemType` - Item/pickup identifiers
- `ArcanaType` - Arcana identifiers
- `PropType` - Environment prop identifiers
- `BgmType` - Background music identifiers
- `AdventureType` - Adventure mode identifiers
- `DlcType` - DLC pack identifiers (Moonspell, Foscari, Chalcedony, FirstBlood, Emeralds, ThosePeople)

### Data Classes
- `WeaponData` - Weapon configuration
- `CharacterData` - Character stats and info
- `EnemyData` - Enemy configuration
- `StageData` - Stage settings
- `PowerUpData` - PowerUp effects
- `ItemData` - Item/pickup properties
- `ArcanaData` - Arcana effects
- `PropData` - Environment prop configuration
- `MusicData` - Music/BGM configuration
- `AdventureData` - Adventure mode configuration
- `CustomMerchantData` - Merchant configuration data