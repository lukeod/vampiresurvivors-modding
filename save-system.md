# Save System

Handles player progress, character unlocks, achievements, and game settings. Based on analysis of decompiled IL2CPP code, the system supports multi-platform storage, backup functionality, and cloud synchronization.


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
Supports enum arrays, dictionary mappings, nested data structures, and JSON-based serialization. Version compatibility handling is inferred from code analysis.

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
// In SaveUtils class - serialization methods
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
// Primary save data paths - INTERNAL METHODS (not accessible for modding)
// These methods use private static accessibility:
private static string GetSaveDataPath()        // Internal path resolution
private static string GetSaveDataPathWithSave() // Internal path with save file
private static string InitSaveDataPath()        // Internal path initialization

// Save validation - INTERNAL METHOD (not accessible for modding)
private static bool SaveDataHasSave()           // Internal save validation

// Backup management - MIXED ACCESSIBILITY
private static string GetBackupsPath()          // Internal backup path
private static bool LastRunBackupExists()       // Internal backup check
private static string GetLastRunBackupPath()    // Internal backup path
private static string GetLastRunBackupBakPath() // Internal backup path

// Platform-specific paths - INTERNAL METHODS
private static string GetBaseDataPath()         // Internal base path
private static Il2CppStringArray GetTempFolders() // Internal temp folders

// Temporary data management - INTERNAL METHODS
private static string GetTempDataPath(string tempFolderName)              // Internal temp path
private static string GetTempDataPathWithSavesFolder(string tempFolderName) // Internal temp path with saves
```

## Backup and Recovery System

### Automatic Backup Creation
Based on code analysis, the system appears to maintain multiple backup layers: Save Backup (regular backup of current save), Last Run Backup (previous game session backup), and Local Backups (timestamped backups).

### Backup Management Methods
```csharp
// Backup operations - PUBLIC METHODS (accessible for modding)
public static Il2CppReferenceArray<Il2CppSystem.Object> GetLocalBackupsList() // Get available backups
public static void RestoreLocalBackup(string filename)                        // Restore specific backup
public static bool HasBackup()                                                // Check backup existence
public static void RestoreLastRunBackup(bool bypassReload = false)           // Restore last run backup

// Save file loading - PUBLIC METHOD (accessible for modding)
public static PlayerOptionsData LoadSaveFiles()                               // Load current save data
```

### Recovery Scenarios
Based on code analysis, the system appears to provide protection against game crashes, corrupted saves, accidental deletions, and version compatibility issues.

## Platform Integration

### Electron (Desktop) Support
Special handling for desktop versions through Electron framework:

```csharp
// Electron-specific paths and validation - INTERNAL METHODS
private static string GetElectronDataPath()     // Internal Electron data path
private static string GetElectronDataSavesPath() // Internal Electron saves path
private static bool ElectronDataHasSave()       // Internal Electron save validation
```

### Multi-Platform Save Storage
Based on code analysis, the `MultiSlotSaveStorage` class appears to provide multiple save slots with async operations, cloud synchronization integration, platform-specific save validation, cross-platform save compatibility, async task-based save/load operations, save data compression and decompression, and conflict resolution for cloud saves.

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
Based on code analysis, temporary directories appear to support save file processing and validation, backup operations, cross-platform data migration, and recovery operations.

## Save Data Structure

### Player Options Integration
```csharp
public static PlayerOptions _playerOptions;  // Current player preferences
```

Based on code analysis, the `PlayerOptionsData` class appears to contain character and stage selections, game mode preferences (Hyper, Hurry, Mazzo, LimitBreak, and others), audio and visual settings, control configurations, game progress and unlocks, platform-specific data and achievements, and statistical data (coins, survival time, and similar metrics).

For additional information about PlayerOptionsData fields and PlayerOptions methods, see [Player Data System](player-data-system.md).

### Game Progress Data
Based on code analysis, save files appear to contain character unlocks and levels, weapon and item discoveries, achievement progress, stage completions, and statistical data (kills, time played, and similar metrics).

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
Based on code analysis, corruption recovery appears to follow this process: load last run backup, try previous backups chronologically, offer manual selection, or create new save.

### Validation Methods
```csharp
// Validation checks (implementation-specific)
bool ValidateSaveIntegrity(string savePath)
bool VerifyBackupCompatibility(string backupPath)
```

## Storage Result System

The save system uses `StorageResult` enum to indicate operation outcomes:
- **Successful**: Operation completed successfully
- **Failed**: Operation failed (file errors, corruption, and similar items)
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

## Method Accessibility Reference

### Public Methods (Accessible for Modding)

These methods are accessible to mods and provide safe interfaces for save system interaction:

**PhaserSaveDataUtils - Public Methods:**
```csharp
// Backup management (safe for modding use)
public static Il2CppReferenceArray<Il2CppSystem.Object> GetLocalBackupsList()
public static void RestoreLocalBackup(string filename)
public static bool HasBackup()
public static void RestoreLastRunBackup(bool bypassReload = false)

// Save file operations (primary modding interface)
public static PlayerOptionsData LoadSaveFiles()
```

**SaveUtils - Public Methods:**
```csharp
// Serialization utilities (safe for modding use)
public static string GetSerializedPlayerDataAsString(PlayerOptionsData data)
public static Il2CppStructArray<byte> GetSerializedPlayerData(PlayerOptionsData data)
public static PlayerOptionsData TryParseData(Il2CppStructArray<byte> data)
public static string JsonFromBytes(Il2CppStructArray<byte> data)
public static Il2CppStructArray<byte> JsonToBytes(string data)

// Save validation and management
public static bool SaveExists(string basePath)
public static string GetSaveFilePath(string basePath)
public static string GetSaveFolderPath(string basePath)
```

**SaveSystem - Public Methods:**
```csharp
// High-level save operations (recommended for modding)
public static void Save(PlayerOptionsData data, bool commitImmediately = true, bool createBackup = false)
public static void LoadAsync(PlayerOptions playerOptions, Action<StorageResult> onComplete)
public static void DeleteSave()
public static bool BackupExists()
public static void TryRestoreBackup(PlayerOptions playerOptions, Action<bool> onComplete)
```

### Private Methods (Internal Only)

These methods are internal to the save system and not accessible for direct modding use:

**PhaserSaveDataUtils - Private Methods:**
```csharp
// Path resolution (internal use only - use alternative approaches)
private static string GetSaveDataPath()
private static string GetSaveDataPathWithSave()
private static string InitSaveDataPath()
private static bool SaveDataHasSave()
private static string GetBackupsPath()
private static string GetBaseDataPath()
private static string GetElectronDataPath()
private static string GetElectronDataSavesPath()
private static bool ElectronDataHasSave()
private static string GetTempDataPath(string tempFolderName)
private static string GetTempDataPathWithSavesFolder(string tempFolderName)

// Utility methods (internal use only)
private static bool LastRunBackupExists()
private static string GetLastRunBackupPath()
private static string GetLastRunBackupBakPath()
private static Il2CppStringArray GetTempFolders()
```

### Modding Recommendations

**Recommended - Use Public Methods:**
- Use `PhaserSaveDataUtils.LoadSaveFiles()` to access save data
- Use `SaveUtils` serialization methods for data conversion
- Use `SaveSystem` methods for high-level save operations
- Use public backup management methods for backup functionality

**Avoid - Private Methods:**
- Do not attempt to call private path resolution methods
- Do not rely on internal validation methods
- Use alternative approaches for path discovery (see examples below)

**Alternative Approaches:**
```csharp
// Instead of private GetSaveDataPath(), use:
string savePath = SaveUtils.GetSaveFilePath(Application.persistentDataPath);

// Instead of private SaveDataHasSave(), use:
bool hasSave = PhaserSaveDataUtils.LoadSaveFiles() != null;

// Instead of private GetBackupsPath(), use:
var backupsList = PhaserSaveDataUtils.GetLocalBackupsList();
```

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

// Alternative: Access raw save file using public methods
string savePath = SaveUtils.GetSaveFilePath(Application.persistentDataPath);
if (File.Exists(savePath))
{
    string saveContent = File.ReadAllText(savePath);
    // Parse JSON content manually if needed
}
```

### Creating Custom Backups
```csharp
// Create mod-specific backup using public methods
PlayerOptionsData saveData = PhaserSaveDataUtils.LoadSaveFiles();
if (saveData != null)
{
    string basePath = Application.persistentDataPath;
    string sourcePath = SaveUtils.GetSaveFilePath(basePath);
    string modBackupPath = Path.Combine(
        Path.GetDirectoryName(sourcePath), 
        $"mod_backup_{DateTime.Now:yyyyMMdd_HHmmss}.save"
    );
    
    if (File.Exists(sourcePath))
    {
        File.Copy(sourcePath, modBackupPath);
    }
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
        
        // Store compressed backup (using alternative path approach)
        string basePath = Application.persistentDataPath;
        string saveDir = Path.GetDirectoryName(SaveUtils.GetSaveFilePath(basePath));
        string backupPath = Path.Combine(
            saveDir, 
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
Based on code analysis, the system appears to implement checks for file format corruption, version compatibility, data consistency, and malicious modifications.

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
Save system testing would typically require thorough save/load cycle testing, backup creation and restoration verification, corrupted save file testing, cross-platform compatibility checking, and game version migration validation.

### Common Issues
Common issues may include save corruption (incomplete writes or format errors), backup failures (insufficient disk space or permissions), version incompatibility (changes breaking old saves), performance impact (save operations causing frame drops), and data loss (failed save operations without appropriate backups).

## Advanced Save Features

### Incremental Saves and Compression
Based on code analysis, for large game states, the system appears to provide automatic save data compression via GZipSaveDataCompressor, binary serialization with SaveUtils.GetSerializedPlayerData(), JSON conversion utilities for cross-platform compatibility, and delta-based saving for performance optimization.

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
Based on code analysis, the system appears to provide automatic backup creation before cloud operations, conflict detection based on save timestamps and platform metadata, graceful degradation to local saves when cloud unavailable, cross-platform save compatibility through standardized serialization, multi-slot save management with SaveSlotMetadata, integrated save data compression for reduced storage footprint, and save backup service for automated backup management.