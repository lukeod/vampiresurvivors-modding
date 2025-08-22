# Save System

Handles player progress, character unlocks, achievements, and game settings with multi-platform support, backup functionality, and cloud synchronization.

## Core Save Architecture

### SaveSerializer
Handles converting game data to and from storage formats:

```csharp
// Generic serialization methods for different data types
public void SerializeEnumArray<T>(List<T> array, List<T> exclude = null)
public void SerializeEnumArrayAsIntArray<T>(List<T> array, List<T> exclude = null)
public void SerializeObjectEnumInt<T>(Dictionary<T, int> obj)
public void SerializeObjectEnumEnum<T1, T2>(Dictionary<T1, T2> obj)
```

### Data Serialization Patterns
Enum arrays, dictionary mappings, nested data structures, and JSON-based serialization with version compatibility.

### Serialization Methods
```csharp
// Generic serialization for enum collections
public void SerializeEnumArray<T>(List<T> array, List<T> exclude = null)
public void SerializeEnumArrayAsIntArray<T>(List<T> array, List<T> exclude = null)
public void SerializeEnumValArray<T>(List<T> array)

// Dictionary serialization
public void SerializeObjectEnumInt<T>(Dictionary<T, int> obj)
public void SerializeObjectEnumEnum<T1, T2>(Dictionary<T1, T2> obj)
public void SerializeObjectEnumEnumArray<T, T2>(Dictionary<T, List<T2>> obj)
public void SerializeObjectEnumIntArray<T, T2>(Dictionary<T, List<T2>> obj)
```

### Static Serialization
```csharp
// In SaveUtils class - comprehensive serialization methods
public static string GetSerializedPlayerDataAsString(PlayerOptionsData data)
public static Il2CppStructArray<byte> GetSerializedPlayerData(PlayerOptionsData data)
public static PlayerOptionsData TryParseData(Il2CppStructArray<byte> data)
public static string JsonFromBytes(Il2CppStructArray<byte> data)
public static Il2CppStructArray<byte> JsonToBytes(string data)
```

## File System Integration

### PhaserSaveDataUtils
Manages save file operations and paths:

```csharp
// Core file/folder constants
public static string SaveDataFolderName;
public static string SavesFolderName;
public static string BackupsFolderName;
public static string SaveFileName;
public static string SaveBackupFileName;
public static string LastRunBackupFileName;
public static string ElectronDataFolderName;
public static string LastRunBackupBakFileName;
public static string DeletedSaveFileName;
```

### Path Management Methods
```csharp
// Primary save data paths
public static string GetSaveDataPath()
public static string GetSaveDataPathWithSave()
public static string InitSaveDataPath()

// Save validation
public static bool SaveDataHasSave()

// Backup management
public static string GetBackupsPath()
public static bool LastRunBackupExists()
public static string GetLastRunBackupPath()
public static string GetLastRunBackupBakPath()

// Platform-specific paths
public static string GetBaseDataPath()
public static Il2CppStringArray GetTempFolders()

// Temporary data management
public static string GetTempDataPath(string tempFolderName)
public static string GetTempDataPathWithSavesFolder(string tempFolderName)
```

## Backup and Recovery System

### Automatic Backup Creation
Multiple backup layers: Save Backup (regular backup of current save), Last Run Backup (previous game session backup), and Local Backups (timestamped backups).

### Backup Management Methods
```csharp
// Backup operations
public static Il2CppReferenceArray<Il2CppSystem.Object> GetLocalBackupsList()
public static void RestoreLocalBackup(string filename)
public static bool HasBackup()
public static void RestoreLastRunBackup(bool bypassReload = false)

// Save file loading
public static PlayerOptionsData LoadSaveFiles()
```

### Recovery Scenarios
Protection against game crashes, corrupted saves, accidental deletions, and version compatibility issues.

## Platform Integration

### Electron (Desktop) Support
Special handling for desktop versions through Electron framework:

```csharp
// Electron-specific paths and validation
public static string GetElectronDataPath()
public static string GetElectronDataSavesPath()
public static bool ElectronDataHasSave()
```

### Multi-Platform Save Storage
The `MultiSlotSaveStorage` class provides multiple save slots with async operations, cloud synchronization (Steam Cloud, etc.), platform-specific save validation, cross-platform save compatibility, async task-based save/load operations, save data compression and decompression, and conflict resolution for cloud saves.

### Platform Detection
```csharp
public static bool IPCRENDERER;  // Indicates Electron/desktop environment
```

## Data Compression and Processing

### Save Data Compression
The save system includes compression capabilities for efficient storage:

```csharp
// GZipSaveDataCompressor for save data optimization
public class GZipSaveDataCompressor
{
    public virtual string Compress(string input)
    public virtual string Decompress(string input)
}
```

### Temporary Data Management
Temporary directories support save file processing and validation, backup operations, cross-platform data migration, and recovery operations.

## Save Data Structure

### Player Options Integration
```csharp
public static PlayerOptions _playerOptions;  // Current player preferences
```

The `PlayerOptionsData` class contains character and stage selections, game mode preferences (Hyper, Hurry, Mazzo, LimitBreak, etc.), audio and visual settings, control configurations, game progress and unlocks, platform-specific data and achievements, and statistical data (coins, survival time, etc.).

For detailed information about PlayerOptionsData fields and PlayerOptions methods, see [Player Data System](player-data-system.md).

### Game Progress Data
Save files contain character unlocks and levels, weapon and item discoveries, achievement progress, stage completions, and statistical data (kills, time played, etc.).

## High-Level Save System

### SaveSystem Class
The `SaveSystem` static class provides the main interface for save operations:

```csharp
// Core save operations
public static void Save(PlayerOptionsData data, bool commitImmediately = true, 
                       bool createBackup = false, CommitOptions options = CommitOptions.Default)
public static void LoadAsync(PlayerOptions playerOptions, Action<StorageResult> onComplete)
public static void DeleteSave()
public static bool BackupExists()
public static void TryRestoreBackup(PlayerOptions playerOptions, Action<bool> onComplete)

// Conflict resolution for cloud saves
public static void HandleConflictResolution(byte[] dataA, byte[] dataB, 
                                           Action<byte[]> onComplete)
```

### CommitOptions Enum
```csharp
[Flags]
public enum CommitOptions
{
    Default = 0,
    TrySynchronously = 1
}
```

## Save Operation Workflow

### Save Process
1. **Data Collection**: Gather current game state into PlayerOptionsData
2. **Serialization**: Convert data to JSON format via SaveSerializer
3. **Backup Creation**: Create backup of existing save (optional)
4. **File Writing**: Write new save data through platform storage
5. **Validation**: Verify save integrity
6. **Cleanup**: Remove temporary files

### Load Process
1. **File Detection**: Check for valid save files
2. **Async Loading**: Load save data asynchronously
3. **Deserialization**: Parse JSON save data into PlayerOptionsData
4. **Data Validation**: Check data integrity and version compatibility
5. **State Restoration**: Apply loaded data to PlayerOptions
6. **Error Handling**: Fallback to backups if needed

## Error Handling and Recovery

### Save Corruption Recovery
Corruption recovery: load last run backup, try previous backups chronologically, offer manual selection, or create new save.

### Validation Methods
```csharp
// Validation checks (implementation-specific)
bool ValidateSaveIntegrity(string savePath)
bool VerifyBackupCompatibility(string backupPath)
```

## Storage Result System

The save system uses `StorageResult` enum to indicate operation outcomes:
- **Successful**: Operation completed successfully
- **Failed**: Operation failed (file errors, corruption, etc.)
- **NotFound**: Save file not found
- **SDKNotInitialized**: Platform SDK not initialized
- **StorageNotInitialized**: Storage system not initialized
- **StorageIsReinitializing**: Storage is currently reinitializing
- **InvalidArg**: Invalid argument provided
- **AnotherOperationInProgress**: Another save operation is in progress
- **NothingToCommit**: No changes to save
- **DataCorrupted**: Save data is corrupted
- **TargetLocked**: Save file is locked
- **NoFreeSpace**: Insufficient disk space

## Common Modding Scenarios

### Reading Save Data
```csharp
// Load save data using the official API
PlayerOptionsData saveData = PhaserSaveDataUtils.LoadSaveFiles();
if (saveData != null)
{
    // Access specific save properties
    int coins = saveData.Coins;
    var unlockedCharacters = saveData.itemInCollection;
    // Process save data
}

// Alternative: Access raw save file
string savePath = PhaserSaveDataUtils.GetSaveDataPathWithSave();
if (File.Exists(savePath))
{
    string saveContent = File.ReadAllText(savePath);
    // Parse JSON content manually if needed
}
```

### Creating Custom Backups
```csharp
// Create mod-specific backup
string modBackupPath = Path.Combine(
    PhaserSaveDataUtils.GetBackupsPath(), 
    $"mod_backup_{DateTime.Now:yyyyMMdd_HHmmss}.save"
);

if (PhaserSaveDataUtils.SaveDataHasSave())
{
    File.Copy(PhaserSaveDataUtils.GetSaveDataPathWithSave(), modBackupPath);
}
```

### Monitoring Save Operations
```csharp
[HarmonyPatch(typeof(PhaserSaveDataUtils), "LoadSaveFiles")]
[HarmonyPostfix]
public static void OnSaveLoad(PlayerOptionsData __result)
{
    MelonLogger.Msg("Save files loaded successfully");
    // __result contains the loaded PlayerOptionsData
    // Custom post-load logic
}
```

### Using High-Level Save System
```csharp
// Load save data asynchronously
SaveSystem.LoadAsync(playerOptions, (result) =>
{
    if (result == StorageResult.Successful)
    {
        MelonLogger.Msg("Save loaded successfully");
        // Process loaded data
    }
    else
    {
        MelonLogger.Error($"Save load failed: {result}");
    }
});

// Save with backup creation
PlayerOptionsData modifiedData = GetModifiedSaveData();
SaveSystem.Save(modifiedData, commitImmediately: true, createBackup: true);
```

### Working with Compressed Save Data
```csharp
// Using compression for custom save operations
var compressor = new GZipSaveDataCompressor();
string saveJson = SaveUtils.GetSerializedPlayerDataAsString(playerData);
string compressedData = compressor.Compress(saveJson);

// Decompress when loading
string decompressedJson = compressor.Decompress(compressedData);
byte[] saveBytes = SaveUtils.JsonToBytes(decompressedJson);
PlayerOptionsData loadedData = SaveUtils.TryParseData(saveBytes);
```

### Advanced Binary Serialization
```csharp
// Work with binary save data for performance
PlayerOptionsData saveData = PhaserSaveDataUtils.LoadSaveFiles();
byte[] binaryData = SaveUtils.GetSerializedPlayerData(saveData);

// Store or transmit binary data efficiently
File.WriteAllBytes(backupPath, binaryData);

// Load from binary data
byte[] loadedBytes = File.ReadAllBytes(backupPath);
PlayerOptionsData restoredData = SaveUtils.TryParseData(loadedBytes);
```

### Safe Save Modification with Compression
```csharp
// Enhanced save modification with compression support
public static void SafeModifySave(Action<PlayerOptionsData> modifyAction)
{
    PlayerOptionsData saveData = PhaserSaveDataUtils.LoadSaveFiles();
    if (saveData == null) return;
    
    try
    {
        // Create compressed backup before modification
        var compressor = new GZipSaveDataCompressor();
        string saveJson = SaveUtils.GetSerializedPlayerDataAsString(saveData);
        string compressedBackup = compressor.Compress(saveJson);
        
        // Store compressed backup
        string backupPath = Path.Combine(
            PhaserSaveDataUtils.GetBackupsPath(), 
            $"mod_backup_{DateTime.Now:yyyyMMdd_HHmmss}.save.gz"
        );
        File.WriteAllText(backupPath, compressedBackup);
        
        // Apply modifications
        modifyAction(saveData);
        
        // Save modified data with official system
        SaveSystem.Save(saveData, commitImmediately: true, createBackup: true);
    }
    catch (Exception ex)
    {
        MelonLogger.Error($"Save modification failed: {ex.Message}");
        
        // Attempt to restore from backup
        if (SaveSystem.BackupExists())
        {
            SaveSystem.TryRestoreBackup(playerOptions, (success) =>
            {
                if (success)
                    MelonLogger.Msg("Backup restored successfully");
                else
                    MelonLogger.Error("Backup restoration failed");
            });
        }
    }
}
```

## Performance Considerations

### Optimization Strategies
- Async Operations: Use SaveSystem.LoadAsync() to avoid blocking gameplay
- Compression: Leverage GZipSaveDataCompressor for smaller save files
- Binary Serialization: Use SaveUtils.GetSerializedPlayerData() for performance
- Caching: Cache validation results and minimize file operations
- Buffering: Use buffered streams for large save operations

### Save Data Size Management
```csharp
// Monitor save data sizes
PlayerOptionsData saveData = PhaserSaveDataUtils.LoadSaveFiles();
byte[] uncompressed = SaveUtils.GetSerializedPlayerData(saveData);
string json = SaveUtils.GetSerializedPlayerDataAsString(saveData);
string compressed = new GZipSaveDataCompressor().Compress(json);

MelonLogger.Msg($"Save sizes - Binary: {uncompressed.Length} bytes, " +
               $"JSON: {json.Length} bytes, Compressed: {compressed.Length} bytes");
```

## Security and Integrity

### Save File Validation
Implement checks for file format corruption, version compatibility, data consistency, and malicious modifications.

### Backup Verification
```csharp
// Verify backup integrity before restoration
bool IsValidBackup(string backupPath)
{
    try
    {
        // Basic file existence and size checks
        if (!File.Exists(backupPath) || new FileInfo(backupPath).Length == 0)
            return false;
            
        // Additional validation logic
        return true;
    }
    catch
    {
        return false;
    }
}
```

## Testing and Debugging

### Save System Testing
Save system testing requires thorough save/load cycle testing, backup creation and restoration verification, corrupted save file testing, cross-platform compatibility checking, and game version migration validation.

### Common Issues
Common issues include save corruption (incomplete writes or format errors), backup failures (insufficient disk space or permissions), version incompatibility (changes breaking old saves), performance impact (save operations causing frame drops), and data loss (failed save operations without proper backups).

## Advanced Save Features

### Incremental Saves and Compression
For large game states, the system provides automatic save data compression via GZipSaveDataCompressor, efficient binary serialization with SaveUtils.GetSerializedPlayerData(), JSON conversion utilities for cross-platform compatibility, and delta-based saving for performance optimization.

### Cloud Synchronization Integration
Vampire Survivors includes built-in cloud save support through multiple platforms:

```csharp
// Platform storage access through SaveSystem.SaveUtil
IPlatformSaveUtils platformStorage = SaveSystem.SaveUtil;
```

#### Platform Storage Types
- Steam Cloud: Automatic synchronization via Steamworks integration
- Multi-slot storage: Support for multiple save slots with conflict resolution
- PlayFab integration: Backend service for cross-platform saves
- Local fallback: Offline functionality when cloud services unavailable

#### Conflict Resolution
When cloud conflicts occur, the system provides a resolution interface:
```csharp
SaveSystem.HandleConflictResolution(localData, cloudData, (resolvedData) =>
{
    // User or automatic resolution completed
    // System will use resolvedData as the final save
});
```

#### Advanced Save Features
Automatic backup creation before cloud operations, conflict detection based on save timestamps and platform metadata, graceful degradation to local saves when cloud unavailable, cross-platform save compatibility through standardized serialization, multi-slot save management with SaveSlotMetadata, integrated save data compression for reduced storage footprint, and save backup service for automated backup management.