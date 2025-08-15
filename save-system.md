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
Supports enum arrays, dictionary mappings, nested data structures, and JSON-based serialization with version compatibility.

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
// In SaveUtils class (not SaveSerializer)
public static string GetSerializedPlayerDataAsString(PlayerOptionsData data)
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
```

## Backup and Recovery System

### Automatic Backup Creation
Creates multiple backup layers:
- **Save Backup**: Regular backup of current save
- **Last Run Backup**: Previous game session backup
- **Local Backups**: Timestamped backups

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
Protects against game crashes, corrupted saves, accidental deletions, and version compatibility issues.

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
The `MultiSlotSaveStorage` class handles:
- Multiple save slots with async operations
- Cloud synchronization (Steam Cloud, etc.)
- Platform-specific save validation
- Cross-platform save compatibility
- Async task-based save/load operations

### Platform Detection
```csharp
public static bool IPCRENDERER;  // Indicates Electron/desktop environment
```

## Temporary and Working Directories

### Temporary Data Management
```csharp
// Temporary working directories
public static string GetTempDataPath(string tempFolderName)
public static string GetTempDataPathWithSavesFolder(string tempFolderName)
```

These methods create temporary locations for:
- Save file processing and validation
- Backup operations
- Cross-platform data migration
- Recovery operations

## Save Data Structure

### Player Options Integration
```csharp
public static PlayerOptions _playerOptions;  // Current player preferences
```

The `PlayerOptionsData` class contains comprehensive save data including:
- Character and stage selections
- Game mode preferences (Hyper, Hurry, Mazzo, LimitBreak, etc.)
- Audio and visual settings
- Control configurations
- Game progress and unlocks
- Platform-specific data and achievements
- Statistical data (coins, survival time, etc.)

### Game Progress Data
Save files contain various types of progress data:
- Character unlocks and levels
- Weapon and item discoveries
- Achievement progress
- Stage completions
- Statistical data (kills, time played, etc.)

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
When corruption detected: load last run backup, try previous backups chronologically, offer manual selection, or create new save.

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

### Safe Save Modification
```csharp
// Always create backup before modifying saves
public static void SafeModifySave(Action<PlayerOptionsData> modifyAction)
{
    PlayerOptionsData saveData = PhaserSaveDataUtils.LoadSaveFiles();
    if (saveData == null) return;
    
    try
    {
        // Create backup before modification
        SaveSystem.Save(saveData, createBackup: true);
        
        // Apply modifications
        modifyAction(saveData);
        
        // Save modified data
        SaveSystem.Save(saveData, commitImmediately: true);
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

Save operations should not block gameplay. Minimize file operations, use buffered streams, prefer async operations, and cache validation checks.

## Security and Integrity

### Save File Validation
Implement checks for:
- File format corruption
- Version compatibility
- Data consistency
- Malicious modifications

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
When modifying save systems:
1. Test save/load cycles thoroughly
2. Verify backup creation and restoration
3. Test with corrupted save files
4. Check cross-platform compatibility
5. Validate migration between game versions

### Common Issues
1. **Save corruption**: Incomplete writes or format errors
2. **Backup failures**: Insufficient disk space or permissions
3. **Version incompatibility**: Changes breaking old saves
4. **Performance impact**: Save operations causing frame drops
5. **Data loss**: Failed save operations without proper backups

## Advanced Save Features

### Incremental Saves
For large game states, consider incremental saving:
- Only save changed data
- Maintain delta logs for rollback
- Compress save data for storage efficiency

### Cloud Synchronization Integration
Vampire Survivors includes built-in cloud save support through multiple platforms:

```csharp
// Platform storage access through SaveSystem.SaveUtil
IPlatformSaveUtils platformStorage = SaveSystem.SaveUtil;
```

#### Platform Storage Types
- **Steam Cloud**: Automatic synchronization via Steamworks integration
- **Multi-slot storage**: Support for multiple save slots with conflict resolution
- **PlayFab integration**: Backend service for cross-platform saves
- **Local fallback**: Offline functionality when cloud services unavailable

#### Conflict Resolution
When cloud conflicts occur, the system provides a resolution interface:
```csharp
SaveSystem.HandleConflictResolution(localData, cloudData, (resolvedData) =>
{
    // User or automatic resolution completed
    // System will use resolvedData as the final save
});
```

#### Key Features
- Automatic backup creation before cloud operations
- Conflict detection based on save timestamps and platform metadata
- Graceful degradation to local saves when cloud unavailable
- Cross-platform save compatibility through standardized serialization