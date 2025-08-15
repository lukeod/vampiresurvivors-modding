# DLC System

Vampire Survivors uses a sophisticated DLC system for loading additional content through Unity AssetBundles and manifest data.

## Core Components

### DlcSystem Class
**Location**: `Il2CppVampireSurvivors.Framework.DLC.DlcSystem`

Central DLC management with static properties:
- `MountedPaths` - `Dictionary<DlcType, string>` tracking file system mount points
- `LoadedDlc` - `Dictionary<DlcType, BundleManifestData>` containing loaded DLC data

Key methods:
- `LoadDlc(Action callback)` - Main entry point for DLC loading
- `MountDlc(DlcType dlcType, Action callback)` - Mounts specific DLC
- `LicenseCheckDlc(Action callback)` - Validates DLC ownership
- `UpdateDlc(Action callback)` - Updates DLC content
- `GetDlcTypesToLoad()` - Returns list of DLCs to load
- `GetMissingDlc()` - Identifies missing DLC content

### BundleManifestData
**Location**: `Il2CppVampireSurvivors.Framework.DLC.BundleManifestData`

ScriptableObject containing all DLC configuration:

```csharp
public class BundleManifestData : ScriptableObject
{
    // Version and metadata
    public string _Version;
    public string name;
    
    // Platform-specific data
    public SwitchManifestData _Switch;
    public PS5ManifestData _PS5;
    public PS4ManifestData _PS4;
    
    // Data configuration
    public DataManagerSettings _DataFiles;
    
    // Content factories
    public WeaponFactory _WeaponFactory;
    public AccessoriesFactory _AccessoriesFactory;
    public CharacterFactory _CharacterFactory;
    public EnemyFactory _EnemyFactory;
    public ProjectileFactory _ProjectileFactory;
    public TilesetFactory _TilesetFactory;
    public DestructibleFactory _DestructibleFactory;
    public BestiaryFactory _BestiaryFactory;
    public MainMenuBackgroundFactory _MainMenuBackgroundFactory;
    public PickupFactory _PickupFactory;
    public HeroVfxFactory _HeroVfxFactory;
    
    // Audio content
    public DynamicSoundGroupCreator _DynamicSoundGroup;
    
    // Asset references
    public AssetReferenceLibrary _AssetReferenceLibrary;
}
```

### LoadingManager
**Location**: `Il2CppVampireSurvivors.Framework.DLC.LoadingManager`

Handles DLC loading orchestration:
- `ValidateVersion(int index, DlcType[] dlcs, Action callback)` - Validates DLC compatibility
- `LoadDlcs(Action callback)` - Loads multiple DLCs sequentially
- `MountDlc(DlcType dlcType, Action callback)` - Mounts DLC to file system
- `LoadManifestDirect(DlcType dlcType, string path, Action<bool> callback)` - Direct manifest loading
- `UnmountAllDlc(Action callback)` - Unmounts all DLCs
- `UnmountDlc(DlcType dlcType, Action callback)` - Unmounts specific DLC

### ManifestLoader
**Location**: `Il2CppVampireSurvivors.Framework.DLC.ManifestLoader`

Low-level manifest operations:
- `LoadAssetBundleFromPath(string bundlePath)` - Loads Unity AssetBundles
- `LoadManifest(BundleManifestData bundleManifestData, DlcType dlcType, Action<BundleManifestData> onComplete)` - Main manifest loading
- `ApplyBundleCore(DlcType dlcType, BundleManifestData manifest, Action<BundleManifestData> onComplete)` - Applies bundle data
- `DoRuntimeReload()` - Performs runtime content refresh

## DLC Types

**Location**: `Il2CppVampireSurvivors.Data.DlcType`

```csharp
public enum DlcType
{
    Moonspell = 0,    // Legacy of the Moonspell
    Foscari = 1,      // Tides of the Foscari
    Chalcedony = 2,   // Chalcedony
    FirstBlood = 3,   // First Blood
    Emeralds = 4,     // Emeralds
    ThosePeople = 5   // Those People
}
```

## Loading Sequence

### Standard DLC Loading Flow

```
1. LicenseCheckDlc() → Validate ownership
2. ValidateVersion() → Check compatibility
3. LoadDlcs() → Load available DLCs
4. MountDlc() → Mount to file system
5. LoadManifest() → Load bundle manifest
6. ApplyBundleCore() → Integrate content
7. DoRuntimeReload() → Refresh game systems
```

### Detailed Call Chain

```
LicenseManager.AddIncludedDlc
  └─> LoadingManager.LoadIncludedDlc
      └─> LoadingManager.LoadDlc
          └─> LoadingManager.LoadManifestDirect
              └─> DlcLoader.LoadDlc
                  └─> DlcLoader.LoadManifest
                      └─> ManifestLoader.LoadManifest
                          └─> ManifestLoader.ApplyBundleCore
                              └─> ManifestLoader.DoRuntimeReload
```

## Custom DLC Injection

### Creating Custom DLC Content

To inject custom content as a DLC:

```csharp
// Define custom DLC type (use high values to avoid conflicts)
DlcType customDlcType = (DlcType)10000;

// Create manifest data
var customManifest = ScriptableObject.CreateInstance<BundleManifestData>();
customManifest._Version = "1.0.0";
customManifest.name = "CustomDLC";
customManifest._DataFiles = new DataManagerSettings();

// Configure data files
JObject weaponJson = JObject.FromObject(customWeaponData);
TextAsset weaponAsset = new TextAsset(weaponJson.ToString());
customManifest._DataFiles._WeaponDataJsonAsset = weaponAsset;

// Create and populate factories
customManifest._WeaponFactory = ScriptableObject.CreateInstance<WeaponFactory>();
foreach (var weapon in customWeapons)
{
    var prefab = CreateWeaponPrefab(weapon);
    customManifest._WeaponFactory._weapons.Add(weapon.Key, prefab);
}

// Mount and load the custom DLC
DlcSystem.MountedPaths.Add(customDlcType, "");
DlcSystem.LoadedDlc.Add(customDlcType, customManifest);

// Apply the bundle
ManifestLoader.ApplyBundleCore(customDlcType, customManifest, (manifest) => 
{
    UnityEngine.Debug.Log("Custom DLC loaded");
});

// Trigger runtime reload
ManifestLoader.DoRuntimeReload();
```

### Optimal Injection Point

The `LoadingManager.ValidateVersion` method is the ideal hook point for DLC injection:

```csharp
[HarmonyPatch(typeof(LoadingManager), "ValidateVersion")]
[HarmonyPostfix]
public static void InjectCustomDLC()
{
    // All base game and official DLCs are loaded
    // Safe to inject custom content here
}
```

## DataManagerSettings Integration

The `DataManagerSettings` class within `BundleManifestData` contains all JSON asset configurations:

```csharp
public class DataManagerSettings
{
    public TextAsset _WeaponDataJsonAsset;
    public TextAsset _CharacterDataJsonAsset;
    public TextAsset _EnemyDataJsonAsset;
    public TextAsset _StageDataJsonAsset;
    public TextAsset _ItemDataJsonAsset;
    public TextAsset _PowerUpDataJsonAsset;
    public TextAsset _ArcanaDataJsonAsset;
    public TextAsset _MusicDataJsonAsset;
    public TextAsset _HitVfxDataJsonAsset;
    public TextAsset _PropsDataJsonAsset;
    public TextAsset _SecretsDataJsonAsset;
    public TextAsset _AchievementDataJsonAsset;
    public TextAsset _LimitBreakDataJsonAsset;
    public TextAsset _AlbumDataJsonAsset;
    public TextAsset _CustomMerchantsDataJsonAsset;
    public TextAsset _AdventureDataJsonAsset;
    public TextAsset _AdventuresStageSetDataJsonAsset;
    public TextAsset _AdventuresMerchantsDataJsonAsset;
}
```

Each TextAsset contains JSON data that gets merged into the main game data through `DataManager.MergeInJsonData()`.

## Factory System Integration

DLC content creation uses the factory pattern:

### Factory Classes
All factories extend `SerializedScriptableObject` and use `UnitySerializedDictionary` for type-safe mappings:

- **WeaponFactory** - Maps `WeaponType` to `Weapon` prefabs
- **CharacterFactory** - Maps `CharacterType` to `CharacterController` prefabs
- **EnemyFactory** - Maps `EnemyType` to enemy prefabs with pooling
- **ProjectileFactory** - Maps weapon types to projectile prefabs
- **AccessoriesFactory** - Maps item types to accessory prefabs

### Linked Factories
Factories support chaining through `_LinkedFactories` lists, allowing DLC factories to extend base game factories.

## Audio Content

DLC audio is managed through `DynamicSoundGroupCreator`:

```csharp
var soundGroup = customManifest._DynamicSoundGroup;
soundGroup.musicPlaylists = new List<Playlist>();
soundGroup.customEventsToCreate = new List<CustomEvent>();

// Add custom music playlist
var playlist = new Playlist() { playlistName = "CustomMusic" };
playlist.MusicSettings.Add(new MusicSetting() 
{ 
    clip = customAudioClip,
    songName = "CustomSong",
    isLoop = true,
    audLocation = AudioLocation.Clip
});
soundGroup.musicPlaylists.Add(playlist);
```

## Platform-Specific Content

Platform-specific manifest data allows conditional content loading:

```csharp
// Check platform and load appropriate data
if (Application.platform == RuntimePlatform.PS5)
{
    var ps5Data = customManifest._PS5;
    // Load PS5-specific content
}
```

## Best Practices

### ID Range Management
Use high ID values for custom content to avoid conflicts:
- Custom DlcType: 100+
- Custom WeaponType: 5000+
- Custom CharacterType: 5000+
- Custom EnemyType: 5000+
- Custom StageType: 5000+

### Content Validation
Always validate custom content before injection:
- Check for ID conflicts
- Verify JSON structure matches expected format
- Ensure prefabs are properly configured
- Test factory creation and retrieval

### Performance Considerations
- Use object pooling for frequently spawned content
- Lazy load assets when possible
- Batch DLC operations to minimize loading overhead
- Cache factory lookups for frequently accessed content