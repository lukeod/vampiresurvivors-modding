# Data Manager

Central repository for game data in Vampire Survivors. Handles loading, storing, and providing access to JSON-based configuration data.

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
Dictionary<EnemyType, List<EnemyData>> _adventureBestiaryData;
Dictionary<CharacterType, CustomMerchantData> _adventureMerchantsData;
```

## Access Patterns

### Getting DataManager Instance

```csharp
// Primary method - through GameManager
var dataManager = GM.Core?.DataManager;

// Alternative - through SoundManager static field
var dataManager = SoundManager._dataManager;
```

### GetConverted Methods

DataManager provides converted data methods that return strongly-typed objects with absolute values:

```csharp
Dictionary<WeaponType, List<WeaponData>> weapons = dataManager.GetConvertedWeapons();
var characters = dataManager.GetConvertedCharacterData();
var enemies = dataManager.GetConvertedEnemyData();
var powerups = dataManager.GetConvertedPowerUpData();
var stages = dataManager.GetConvertedStages();

// Properties for other data types
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
JArray whipLevels = weaponJson["WHIP"] as JArray;

// Level 1: Absolute values
whipLevels[0]["power"] = 50;

// Levels 2-8: Incremental deltas
whipLevels[1]["power"] = 20;  // Adds to level 1
whipLevels[2]["power"] = 15;  // Adds to level 2

dataManager.ReloadAllData();
```

## Incremental Level System

Weapon and PowerUp levels use an incremental delta system:

- **Level 1**: Absolute base values
- **Levels 2-8**: Incremental changes from previous level

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
// Called during game initialization
public void LoadBaseJObjects()
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
    // Called multiple times during startup:
    // - Once for base game data
    // - Once for each DLC
    // - Final call for data validation
    // Re-processes all JSON data
    // Updates converted data caches
    // Triggers data reload events
}
```

### Hook Points

```csharp
[HarmonyPatch(typeof(DataManager), "LoadBaseJObjects")]
[HarmonyPostfix]
public static void OnDataLoad(DataManager __instance)
{
    // Data loaded and ready for modification
}

[HarmonyPatch(typeof(DataManager), "ReloadAllData")]
[HarmonyPostfix]
public static void OnDataReload(DataManager __instance)
{
    // Called multiple times during startup
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

## Usage Guidelines

### Use GetConverted Methods
Prefer `GetConverted*` methods over direct JSON manipulation for strongly typed, absolute values.

### Delta System
When modifying JSON directly:
- Level 1 = absolute values
- Levels 2+ = incremental deltas

### Null Safety
```csharp
var weapons = dataManager?.GetConvertedWeapons();
if (weapons != null && weapons.Count > 0)
{
    // Safe to use
}
```

### Apply Changes
```csharp
weaponJson["WHIP"][0]["power"] = 100;
dataManager.ReloadAllData();
```

## DataManagerSettings Structure

**Location**: `Il2CppVampireSurvivors.App.Data.DataManagerSettings`

Centralized configuration container for all game data JSON assets:

### Core Game Data Assets
- `_AchievementDataJsonAsset` - Achievement definitions and requirements
- `_ArcanaDataJsonAsset` - Arcana cards and their effects
- `_CharacterDataJsonAsset` - Character stats, abilities, and properties
- `_EnemyDataJsonAsset` - Enemy statistics and behaviors
- `_ItemDataJsonAsset` - Item definitions and effects
- `_WeaponDataJsonAsset` - Weapon statistics and behaviors
- `_StageDataJsonAsset` - Stage configurations and layouts
- `_SecretsDataJsonAsset` - Hidden content and unlockables
- `_PropsDataJsonAsset` - Environmental objects and decorations
- `_PowerUpDataJsonAsset` - Power-up effects and statistics
- `_MusicDataJsonAsset` - Audio track configurations
- `_HitVfxDataJsonAsset` - Visual effects for hits and impacts
- `_LimitBreakDataJsonAsset` - Limit break mechanics
- `_AlbumDataJsonAsset` - Music album information
- `_CustomMerchantsDataJsonAsset` - Custom merchant configurations

### Adventure Mode Data Assets
- `_AdventureDataJsonAsset` - Adventure mode configurations
- `_AdventuresStageSetDataJsonAsset` - Adventure stage set definitions
- `_AdventuresMerchantsDataJsonAsset` - Adventure-specific merchant data

### Asset Loading
Each field provides access through IL2CPP runtime invocation:
```csharp
public TextAsset GetWeaponDataJsonAsset()
{
    return Il2CppObjectPool.Get<TextAsset>(intPtr);
}
```

## Common Data Types

### Enumerations with Safe ID Ranges

#### Core Content Types
- `WeaponType` - Weapon identifiers (0-1599 used, **safe: 5000+**)
- `CharacterType` - Character identifiers (0-314 used, **safe: 5000+**)
- `EnemyType` - Enemy identifiers (0-1122 used, **safe: 5000+**)
- `StageType` - Stage identifiers (0-1064 used, **safe: 5000+**)
- `ItemType` - Item/pickup identifiers (0-230 used, **safe: 5000+**)
- `ArcanaType` - Arcana identifiers (0-52 used, **safe: 1000+**)
- `PowerUpType` - PowerUp identifiers (separate enum with symbolic names like POWER, REGEN, etc.)
- `DlcType` - DLC pack identifiers (0-5 used, **safe: 100+**)

#### Additional Types
- `PropType` - Environment prop identifiers
- `BgmType` - Background music identifiers (0-1409 used, **safe: 10000+**)
- `SfxType` - Sound effect identifiers (0-517 used, **safe: 10000+**)
- `AdventureType` - Adventure mode identifiers
- `SkinType` - Character skin identifiers (0-1076 used, **safe: 10000+**)

### Enum ID Assignment Patterns
- **Base Game**: Generally uses lower ranges (0-200)
- **DLC Content**: Uses specific reserved ranges:
  - 300s: Foscari DLC (FB_ prefix)
  - 360s-410s: Emergency Meeting DLC (EME_ prefix)
  - 1400s-1500s: Tides of the Foscari (TP_ prefix)
- **Special Values**: Some enums use -1 for VOID/NULL values
- **Reserved Gaps**: Large gaps suggest space for future content

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