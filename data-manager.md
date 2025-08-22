# Data Manager - Complete v1.0+ Reference

Central repository for game data in Vampire Survivors. Handles loading, storing, and providing access to JSON-based configuration data with comprehensive online multiplayer synchronization support.

**Location**: `Il2CppVampireSurvivors.Data.DataManager`

**Version**: Updated for v1.0+ with multiplayer and new data types

## JSON Data Storage

DataManager stores game data as JObject instances (from Newtonsoft.Json):

### Core Data Properties (Line 897-983 in DataManager.cs)

```csharp
// Traditional game data (unchanged)
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
public JObject _allAdventureMerchantsJson; // Adventure merchant data
public JObject _allCustomMerchantsJson;   // Custom merchant configurations

// NEW DATA TYPES (v1.0+)
public JObject _allStageSetJson;          // Stage set configurations for adventures
public JObject _allAdventureStagesJson;   // Adventure-specific stage definitions 
public JObject _allAlbumData;             // Music album/collection system
public JObject _allCPUJson;               // AI behavior configurations for multiplayer
```

### Online Synchronization Tracking (Lines 502-564 in DataManager.cs)

**NEW in v1.0+**: DataManager now tracks when data is modified for online multiplayer synchronization.

```csharp
public bool _characterDataChangedForOnline;  // Character data modified for online play
public bool _powerUpDataChangedForOnline;    // PowerUp data modified for online play  
public bool _stageDataChangedForOnline;      // Stage data modified for online play
public bool _weaponDataChangedForOnline;     // Weapon data modified for online play
public bool _enemyDataChangedForOnline;      // Enemy data modified for online play
```

**Status**: These flags are declared but **not currently used** in the codebase. They appear to be preparation for future online synchronization features.

**Purpose**: When implemented, these flags will allow the game to track which data types have been modified by mods and need to be synchronized across online multiplayer clients.

## NEW Data Types Deep Dive (v1.0+)

### CPU/AI Data System (_allCPUJson)

**Purpose**: Defines AI behaviors for multiplayer bots and CPU-controlled characters.

**Access**: `dataManager.AllCPU` → `Dictionary<AIType, AIData>`

**AIType Enum Values** (from AIType.cs):
```csharp
None,                    // No AI behavior
Aggressive,              // Attacks aggressively
Defensive,               // Focuses on survival
ChaoticAF,              // Unpredictable behavior
MirrorInput,            // Mirrors player input
DelayedInputCopy,       // Copies input with delay
DelayedPositionCopy,    // Follows position with delay
GreedyForPickups,       // Prioritizes item collection
StandsStill,            // Stationary behavior
Masochistic,            // Takes damage intentionally
AlwaysRight,            // Always moves right
AngleDistance,          // Movement based on angles
DeathSequence,          // Special death behavior
AngleDistanceMirrorInput, // Combination behavior
Conga                   // Follows in a line
```

**AIData Structure** (from AIData.cs):
```csharp
string AINameLocalTerm;  // Localization key for AI name
string AIIconSprite;     // Sprite name for AI icon
string AIIconTexture;    // Texture path for AI icon
```

### Album Data System (_allAlbumData)

**Purpose**: Music collection/gallery system for tracking unlocked albums.

**Access**: `dataManager.AllAlbumData` → `Dictionary<AlbumType, AlbumData>`

**AlbumType Enum Values** (from AlbumType.cs):
```csharp
VampireSurvivorsA,       // Base game album A
VampireSurvivorsB,       // Base game album B  
VampireSurvivorsC,       // Base game album C
LegacyOfTheMoonspell,    // Moonspell DLC album
TidesOfTheFoscari,       // Foscari DLC album
HOTI,                    // Heart of the Islands
EmergencyMeeting,        // Emergency Meeting DLC
OperationGuns,           // Operation Guns DLC
OdeToCastlevaniaA,       // Castlevania collab A
OdeToCastlevaniaB,       // Castlevania collab B
EmeraldDiorama           // Emerald Diorama content
```

**AlbumData Structure** (from AlbumData.cs):
```csharp
bool isUnlocked;                    // Whether album is unlocked
string title;                       // Album display name
string icon;                        // Album icon reference
List<BgmType> trackList;           // List of tracks in album
ContentGroupType contentGroupType;  // Content group classification
```

### Stage Set Data System (_allStageSetJson)

**Purpose**: Groups stages into sets for adventure mode progression.

**Access**: `dataManager.AllStageSetData` → `Dictionary<StageSetType, JObject>`

**StageSetType Enum Values** (from StageSetType.cs):
```csharp
STAGE_DATA_ADV_M001,     // Adventure mode set M001
STAGE_DATA_ADV_0001,     // Adventure mode set 0001
STAGESET_POE,            // Poe stage set
STAGESET_IMELDA,         // Imelda stage set
STAGESET_CHALCEDONY,     // Chalcedony stage set
STAGESET_FIRSTBLOOD,     // First Blood stage set
STAGESET_SHEMOON,        // Moonspell stage set
STAGESET_FOSCARI         // Foscari stage set
```

**Purpose**: Each stage set defines a collection of stages that form a cohesive adventure experience, typically themed around specific DLC content or character storylines.

### Adventure Stage Data (_allAdventureStagesJson)

**Purpose**: Adventure mode-specific stage configurations that differ from regular stages.

**Access**: Via JSON only (no direct converted access method found)

**Relationship**: Works with `_allStageSetJson` to define complete adventure experiences with stage progression, special rules, and unique configurations not available in standard stages.

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
// Primary method - through GameManager (recommended)
var dataManager = GM.Core?._data;  // Note: _data is the actual field name

// Alternative - through GameManager.DataManager property
var dataManager = GM.Core?.DataManager;

// Legacy - through SoundManager static field (still works)
var dataManager = SoundManager._dataManager;

// Always null-check in multiplayer contexts
if (dataManager == null)
{
    MelonLogger.Error("DataManager not available!");
    return;
}
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

Data is also accessible through strongly-typed properties:

```csharp
// JSON-based data (returns JArray for level-based data)
var weaponData = dataManager.AllWeaponData;      // Dictionary<WeaponType, JArray>
var characterData = dataManager.AllCharacters;   // Dictionary<CharacterType, JArray>
var enemyData = dataManager.AllEnemies;          // Dictionary<EnemyType, JArray>
var powerUpData = dataManager.AllPowerUps;       // Dictionary<PowerUpType, JArray>
var stageData = dataManager.AllStages;           // Dictionary<StageType, JArray>
var limitBreakData = dataManager.AllLimitBreakData; // Dictionary<WeaponType, JArray>

// Converted data (returns single instance objects)
var items = dataManager.AllItems;       // Dictionary<ItemType, ItemData>
var arcanas = dataManager.AllArcanas;   // Dictionary<ArcanaType, ArcanaData>
var props = dataManager.AllProps;       // Dictionary<PropType, PropData>
var hitVfx = dataManager.AllHitVfxData; // Dictionary<HitVfxType, HitVfxData>
var musicData = dataManager.AllMusicData; // Dictionary<BgmType, MusicData>
var secrets = dataManager.AllSecrets;   // Dictionary<SecretType, SecretData>
var achievements = dataManager.AllAchievements; // Dictionary<AchievementType, AchievementData>

// Adventure-specific data access
var adventures = dataManager.AllAdventures;              // Dictionary<AdventureType, AdventureData>
var adventureMerchants = dataManager.AllAdventureMerchantsData; // Dictionary<CharacterType, CustomMerchantData>
var customMerchants = dataManager.AllCustomMerchantsData; // Dictionary<CharacterType, CustomMerchantData>
var adventureCharacters = dataManager.AdventureCharacterData;   // Dictionary<CharacterType, List<CharacterData>>
var adventureStages = dataManager.AdventureStageData;           // Dictionary<StageType, List<StageData>>
var adventureBestiary = dataManager.AdventureBestiaryData;      // Dictionary<EnemyType, List<EnemyData>>

// NEW DATA TYPES (v1.0+) with detailed descriptions
var albumData = dataManager.AllAlbumData;       // Dictionary<AlbumType, AlbumData> - Music collection system
var cpuData = dataManager.AllCPU;               // Dictionary<AIType, AIData> - AI behavior for multiplayer
var stageSetData = dataManager.AllStageSetData; // Dictionary<StageSetType, JObject> - Adventure stage sets

// Individual data access
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

## Data Loading Process (Verified from Source)

### DataManager Lifecycle

**DataManager** implements `IInitializable` and `IDisposable` patterns:

```csharp
// Called during GameManager initialization
public void Initialize()  // Line 2539 in DataManager.cs
{
    // Sets up DataManager for operation
    // Called once during game startup
}

// Called when GameManager is destroyed
public void Dispose()     // Line 2553 in DataManager.cs  
{
    // Cleanup DataManager resources
    // Called during game shutdown
}
```

### Data Loading Methods

**LoadBaseJObjects** (Line 2784 in DataManager.cs):
```csharp
public void LoadBaseJObjects()
{
    // Loads all JSON data from game files
    // Populates _all*Json properties from DataManagerSettings
    // Called: 2 times according to CallCount annotation
}
```

**ReloadAllData** (Line 2564 in DataManager.cs):
```csharp
public void ReloadAllData()
{
    // Re-processes all JSON data into converted objects
    // Updates AllWeaponData, AllCharacters, etc. properties
    // Called: 7 times according to CallCount annotation
    // Timing: Once for base game + once per DLC + validation
}
```

**LoadDataFromJson** (Line 2795 in DataManager.cs):
```csharp
public void LoadDataFromJson()
{
    // Internal method for processing JSON into object collections
    // Called: 3 times according to CallCount annotation
    // Converts JObject data to strongly-typed collections
}
```

### Data Loading Order

1. **Initialize()** - Sets up DataManager
2. **LoadBaseJObjects()** - Loads JSON from files (called 2x)
3. **LoadDataFromJson()** - Converts JSON to objects (called 3x) 
4. **ReloadAllData()** - Final processing (called 7x: base + 6 DLCs)
5. **BuildConvertedData()** - Creates typed object collections

### Profiling Markers

DataManager includes Unity Profiler markers for performance monitoring:

```csharp
ProfilerMarker MarkerReloadAllData;      // Tracks ReloadAllData performance
ProfilerMarker MarkerLoadDataFromJson;   // Tracks JSON processing
ProfilerMarker MarkerBuildConvertedData; // Tracks object conversion
ProfilerMarker MarkerLoadBaseJObjects;   // Tracks initial JSON loading
```

### Hook Points (Updated for v1.0+)

**LoadBaseJObjects Hook** - JSON Data Ready:
```csharp
[HarmonyPatch(typeof(DataManager), "LoadBaseJObjects")]
[HarmonyPostfix]
public static void OnDataLoad(DataManager __instance)
{
    // Raw JSON data loaded and ready for modification
    // Called 2 times during startup
    // Safe to modify _all*Json properties here
    
    // Example: Modify AI data
    if (__instance._allCPUJson != null)
    {
        // Add custom AI behaviors
    }
}
```

**ReloadAllData Hook** - Converted Data Ready:
```csharp
private static int reloadCount = 0;

[HarmonyPatch(typeof(DataManager), "ReloadAllData")]
[HarmonyPostfix] 
public static void OnDataReload(DataManager __instance)
{
    reloadCount++;
    // Called 7 times: base game + 6 DLCs
    // Safe to access GetConverted methods and All* properties
    
    if (reloadCount >= 7) // Final call
    {
        // All data fully loaded and converted
        // Safe to access new data types:
        var cpuData = __instance.AllCPU;
        var albumData = __instance.AllAlbumData;
        var stageSetData = __instance.AllStageSetData;
    }
}
```

**Multiplayer-Safe Modification Hook**:
```csharp
[HarmonyPatch(typeof(DataManager), "LoadDataFromJson")]
[HarmonyPostfix]
public static void OnDataConversion(DataManager __instance)
{
    // Called 3 times during data processing
    // Best place for multiplayer-aware modifications
    
    // Check if in multiplayer mode before modifying
    if (GM.Core?.IsOnlineMultiplayer == true)
    {
        // Handle online multiplayer data constraints
        return; // Skip modifications for online play
    }
    
    // Safe to modify data for single player/local coop
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

## Multiplayer Considerations (NEW in v1.0+)

### Online Multiplayer Data Restrictions

**Critical**: Mods must check multiplayer state before modifying data:

```csharp
[HarmonyPatch(typeof(DataManager), "ReloadAllData")]
[HarmonyPostfix]
public static void SafeDataModification(DataManager __instance)
{
    // ALWAYS check multiplayer mode first
    if (GM.Core?.IsOnlineMultiplayer == true)
    {
        // SKIP data modifications in online multiplayer
        // Modifications could desync clients or cause crashes
        return;
    }
    
    if (GM.Core?.IsLocalMultiplayer == true)
    {
        // Local coop: modifications usually safe
        // But test thoroughly with multiple players
    }
    
    // Single player: all modifications safe
    ModifyGameData(__instance);
}
```

### Data Change Tracking (Future-Ready)

```csharp
// When modifying data, prepare for future synchronization
public static void ModifyWeaponData(DataManager dataManager)
{
    // Modify weapon data
    dataManager._allWeaponDataJson["WHIP"][0]["power"] = 200;
    
    // Mark for future online synchronization
    dataManager._weaponDataChangedForOnline = true;
    
    // Apply changes
    dataManager.ReloadAllData();
}
```

### IsOnline Method

DataManager includes a private `IsOnline()` method (Line 2777) that checks multiplayer state. This suggests internal logic for handling online vs offline data differently.

## Usage Guidelines (Updated)

### 1. Multiplayer Awareness (CRITICAL)

```csharp
// ALWAYS check multiplayer state first
if (GM.Core?.IsOnlineMultiplayer == true)
{
    return; // Skip modifications
}
```

### 2. Use GetConverted Methods
Prefer `GetConverted*` methods over direct JSON manipulation for strongly typed, absolute values.

### 3. Delta System (Unchanged)
When modifying JSON directly:
- Level 1 = absolute values
- Levels 2+ = incremental deltas

### 4. Null Safety with IL2CPP
```csharp
var weapons = dataManager?.GetConvertedWeapons();
if (weapons != null && weapons.Count > 0)
{
    // IL2CPP collections require careful null checking
    foreach (var kvp in weapons)
    {
        if (kvp.Value != null) // Always check IL2CPP objects
        {
            // Safe to use
        }
    }
}
```

### 5. Apply Changes Safely
```csharp
// Check multiplayer state before any modification
if (GM.Core?.IsOnlineMultiplayer != true)
{
    weaponJson["WHIP"][0]["power"] = 100;
    dataManager.ReloadAllData();
}
```

### 6. New Data Type Access
```csharp
// Access new v1.0+ data types
var aiData = dataManager.AllCPU; // Dictionary<AIType, AIData>
foreach (var ai in aiData)
{
    MelonLogger.Msg($"AI: {ai.Key} - {ai.Value?.AINameLocalTerm}");
}

var albums = dataManager.AllAlbumData; // Dictionary<AlbumType, AlbumData>
foreach (var album in albums)
{
    MelonLogger.Msg($"Album: {album.Key} - {album.Value?.title}");
}
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
- `_AdventuresStagesJsonAsset` - Adventure stage data (new)
- `_AdventuresMerchantsDataJsonAsset` - Adventure-specific merchant data

### Additional Data Assets
- `_AllCPUAsset` - CPU/AI behavior configurations (new)

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
- `AIType` - CPU/AI behavior identifiers
- `AlbumType` - Music album identifiers
- `StageSetType` - Stage set identifiers
- `HitVfxType` - Hit visual effect identifiers

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
- `AIData` - CPU/AI behavior configuration (NEW v1.0+)
- `AlbumData` - Music album information (NEW v1.0+)  
- `HitVfxData` - Hit visual effect configuration
- `SecretData` - Secret unlock data
- `AchievementData` - Achievement configuration data

### New Data Classes (v1.0+) - Detailed

**AIData Fields** (from AIData.cs):
```csharp
string AINameLocalTerm;   // Localization key for AI name display
string AIIconSprite;      // Sprite asset name for AI icon
string AIIconTexture;     // Texture asset path for AI icon
```

**AlbumData Fields** (from AlbumData.cs):
```csharp
bool isUnlocked;                      // Album unlock status
string title;                         // Album display name
string icon;                          // Album icon asset reference
List<BgmType> trackList;             // Music tracks in album
ContentGroupType contentGroupType;    // Content group (base/DLC classification)
```

**StageSetType Usage**:
- Used with `AllStageSetData` returning `Dictionary<StageSetType, JObject>`
- Contains raw JSON configuration for adventure stage progression
- Links to specific DLC content (POE, IMELDA, CHALCEDONY, etc.)