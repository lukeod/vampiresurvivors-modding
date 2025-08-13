# Common Pitfalls Documentation

## Overview
This document outlines the most common issues encountered when modding Vampire Survivors and provides solutions to avoid or resolve them. Learning from these pitfalls can save significant development time and prevent game instability.

## Data System Pitfalls

### 1. Weapon Level Delta Confusion
**Problem**: Forgetting that weapon levels 2-8 contain incremental deltas, not absolute values.

```csharp
// WRONG: Treating all levels as absolute values
var whipLevel3 = weaponJson["WHIP"][2]["power"].Value<float>();  // This is a delta!
// If level 1 = 10, level 2 = +10, level 3 = +10, this returns 10, not 30

// CORRECT: Use GetConverted methods or calculate cumulative values
var weapons = dataManager.GetConvertedWeapons();
var whipLevel3Power = weapons[WeaponType.WHIP][2].power;  // Returns absolute value (30)
```

**Solution**: Always use `DataManager.GetConvertedWeapons()` for absolute values, or implement proper delta calculation.

### 2. Missing IL2CPP Type Prefixes
**Problem**: Using standard .NET type names instead of IL2CPP variants.

```csharp
// WRONG: Missing Il2Cpp prefix
using VampireSurvivors.Framework;  // Won't compile
GameManager gameManager;            // Won't compile

// CORRECT: Proper IL2CPP type references
using Il2CppVampireSurvivors.Framework;
Il2CppVampireSurvivors.Framework.GameManager gameManager;
```

**Solution**: All game types must include the `Il2Cpp` prefix. Use IDE auto-completion to ensure correct naming.

### 3. GM.Core Null Reference in Menus
**Problem**: Assuming `GM.Core` is always available, especially in menu screens.

```csharp
// WRONG: No null checking
var dataManager = GM.Core.DataManager;  // NullReferenceException in menus

// CORRECT: Proper null checking
var dataManager = GM.Core?.DataManager;
if (dataManager == null)
{
    MelonLogger.Warning("DataManager not available (likely in menu)");
    return;
}
```

**Solution**: Always use null-conditional operators and validate game state before accessing core systems.

### 4. Runtime Assembly Reference Missing
**Problem**: Compilation errors when working with `CharacterController` or other core game classes due to missing assembly reference.

```csharp
// Error: Cannot find type CharacterController
[HarmonyPatch(typeof(CharacterController), "LevelUp")]
// CS0234: The type or namespace name 'CharacterController' could not be found
```

**Solution**: Add the VampireSurvivors.Runtime.dll reference to your project:
```xml
<Reference Include="Il2CppVampireSurvivors.Runtime">
  <HintPath>F:\vampire\managed\Il2CppVampireSurvivors.Runtime.dll</HintPath>
</Reference>
```

### 5. Direct JSON Modification Without Reload
**Problem**: Modifying JSON data directly but forgetting to trigger data reload.

```csharp
// WRONG: Modifying data without reload
dataManager._allWeaponDataJson["WHIP"][0]["power"] = 100;
// Changes won't take effect!

// CORRECT: Reload data after modifications
dataManager._allWeaponDataJson["WHIP"][0]["power"] = 100;
dataManager.ReloadAllData();  // Apply changes
```

**Solution**: Always call `dataManager.ReloadAllData()` after direct JSON modifications.

## IL2CPP Specific Pitfalls

### 6. Collection Iteration Without Null Checks
**Problem**: IL2CPP collections can contain null entries or become invalid.

```csharp
// WRONG: No null validation
foreach (var weapon in il2cppWeaponList)
{
    var power = weapon.power;  // Potential NullReferenceException
}

// CORRECT: Defensive iteration
if (il2cppWeaponList != null)
{
    foreach (var weapon in il2cppWeaponList)
    {
        if (weapon != null)
        {
            var power = weapon.power;
        }
    }
}
```

**Solution**: Always validate IL2CPP objects and collections before use.

### 7. Mixed Wrapper Type Confusion
**Problem**: Using wrong wrapper types for `PlayerModifierStats` properties.

```csharp
// WRONG: Assuming all stats use EggFloat
playerStats.Revivals.Value = 5;  // ERROR: Revivals uses EggDouble!
playerStats.Shroud.Value = 0.5f; // ERROR: Shroud is raw float!

// CORRECT: Use proper types
playerStats.Revivals.SetValue(5.0);  // EggDouble - note: requires double
playerStats.Shroud = 0.5f;           // Raw float
playerStats.Power.SetValue(100f);    // EggFloat
```

**Solution**: Consult the stat system documentation for exact property types and use appropriate access methods.

### 8. String Handling in IL2CPP
**Problem**: IL2CPP string operations can be unreliable.

```csharp
// WRONG: Direct string comparison
if (il2cppObject.name == "SomeName")  // May fail due to IL2CPP string handling

// CORRECT: Safe string handling
string name = il2cppObject.name?.ToString();
if (!string.IsNullOrEmpty(name) && name.Equals("SomeName", StringComparison.OrdinalIgnoreCase))
```

**Solution**: Use defensive string handling with null checks and explicit conversion.

## Performance Pitfalls

### 9. Hooking High-Frequency Methods
**Problem**: Hooking into methods that execute every frame or multiple times per frame.

```csharp
// WRONG: Hooking performance-critical methods
[HarmonyPatch(typeof(EnemiesManager), "OnTick")]
[HarmonyPostfix]
public static void BadHook()  // Called every frame for hundreds of objects!
{
    // Will destroy game performance
}

// CORRECT: Hook initialization or event-driven methods
[HarmonyPatch(typeof(GameManager), "AddStartingWeapon")]
[HarmonyPostfix]
public static void GoodHook()  // Called once at game start
{
    // Safe for modifications
}
```

**Solution**: Avoid hooking `OnTick`, `OnUpdate`, `RunMovementJob`, and other high-frequency methods.

### 10. Excessive String Allocations
**Problem**: Creating strings in hot code paths, especially in hooks.

```csharp
// WRONG: String creation in frequently called code
[HarmonyPatch(typeof(Weapon), "PPower")]
[HarmonyPostfix]
public static void BadStringHandling(float __result)
{
    string message = "Power: " + __result;  // Allocates every call
    MelonLogger.Msg(message);
}

// CORRECT: Conditional logging and cached strings
private static readonly StringBuilder _stringBuilder = new();

[HarmonyPatch(typeof(GameManager), "AddStartingWeapon")]
[HarmonyPostfix]
public static void GoodStringHandling()
{
    if (DebugMode)  // Only when needed
    {
        _stringBuilder.Clear();
        _stringBuilder.Append("Weapon added");
        MelonLogger.Msg(_stringBuilder.ToString());
    }
}
```

**Solution**: Minimize string operations in hooks, use StringBuilder for complex formatting, and implement conditional logging.

## Harmony Hooking Pitfalls

### 11. Missing Exception Handling in Hooks
**Problem**: Unhandled exceptions in Harmony patches can crash the game.

```csharp
// WRONG: No exception handling
[HarmonyPatch(typeof(CharacterController), "LevelUp")]
[HarmonyPostfix]
public static void UnsafeHook(CharacterController __instance)
{
    __instance.PlayerStats.Power.SetValue(999);  // Could throw exceptions
}

// CORRECT: Defensive exception handling
[HarmonyPatch(typeof(CharacterController), "LevelUp")]
[HarmonyPostfix]
public static void SafeHook(CharacterController __instance)
{
    try
    {
        if (__instance?.PlayerStats?.Power != null)
        {
            __instance.PlayerStats.Power.SetValue(999);
        }
    }
    catch (Exception ex)
    {
        MelonLogger.Error($"Error in LevelUp hook: {ex.Message}");
    }
}
```

**Solution**: Wrap hook logic in try-catch blocks and validate all parameters.

### 12. Hook Order Dependencies
**Problem**: Multiple mods hooking the same method with dependencies on execution order.

```csharp
// PROBLEMATIC: Mod A expects to run before Mod B
[HarmonyPatch(typeof(WeaponData), "power", MethodType.Getter)]
[HarmonyPostfix]
public static void ModifyPower(ref float __result)
{
    __result *= 2;  // What if another mod also multiplies by 2?
}
```

**Solution**: Design hooks to be independent when possible, or use explicit priority if needed:
```csharp
[HarmonyPatch(typeof(WeaponData), "power", MethodType.Getter)]
[HarmonyPostfix]
[HarmonyPriority(Priority.High)]
public static void ModifyPowerFirst(ref float __result)
{
    // This runs before lower priority hooks
}
```

## Save System Pitfalls

### 13. Save Corruption from Direct Modification
**Problem**: Modifying save files without proper backup or validation.

```csharp
// WRONG: Direct save modification without backup
string savePath = PhaserSaveDataUtils.GetSaveDataPathWithSave();
File.WriteAllText(savePath, modifiedSaveData);  // Could corrupt save!

// CORRECT: Safe save modification with backup
public static void SafeModifySave(string newData)
{
    string savePath = PhaserSaveDataUtils.GetSaveDataPathWithSave();
    string backupPath = savePath + ".backup";
    
    try
    {
        // Create backup
        File.Copy(savePath, backupPath);
        
        // Modify save
        File.WriteAllText(savePath, newData);
        
        // Validate
        if (ValidateSaveFile(savePath))
        {
            File.Delete(backupPath);  // Success
        }
        else
        {
            File.Copy(backupPath, savePath);  // Restore backup
            throw new Exception("Save validation failed");
        }
    }
    catch (Exception ex)
    {
        MelonLogger.Error($"Save modification failed: {ex.Message}");
        if (File.Exists(backupPath))
        {
            File.Copy(backupPath, savePath);  // Restore backup
        }
    }
}
```

**Solution**: Always create backups before modifying save files and implement validation.

## File I/O Pitfalls

### 14. Using Game Directory for Mod Data
**Problem**: Writing mod files to the game installation directory instead of proper mod directories.

```csharp
// WRONG: Writing to game directory
string gameDir = Directory.GetCurrentDirectory();
string modFile = Path.Combine(gameDir, "mod_data.json");  // Bad practice

// CORRECT: Using MelonLoader directories
string userDataPath = MelonEnvironment.UserDataDirectory;
string modDataPath = Path.Combine(userDataPath, "MyMod");
Directory.CreateDirectory(modDataPath);
string modFile = Path.Combine(modDataPath, "mod_data.json");
```

**Solution**: Use `MelonEnvironment.UserDataDirectory` for mod data storage.

### 15. File Access Without Permissions Check
**Problem**: Attempting file operations without verifying permissions.

```csharp
// WRONG: No permission validation
File.WriteAllText(filePath, data);  // May fail due to permissions

// CORRECT: Permission and existence validation
public static bool SafeWriteFile(string filePath, string data)
{
    try
    {
        // Ensure directory exists
        string directory = Path.GetDirectoryName(filePath);
        if (!Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }
        
        // Test write permissions
        File.WriteAllText(filePath, data);
        return true;
    }
    catch (UnauthorizedAccessException)
    {
        MelonLogger.Error($"No write permission for: {filePath}");
        return false;
    }
    catch (Exception ex)
    {
        MelonLogger.Error($"File write error: {ex.Message}");
        return false;
    }
}
```

**Solution**: Implement proper error handling and permission checking for file operations.

## UI and Threading Pitfalls

### 16. UI Updates from Background Threads
**Problem**: Attempting to update UI elements from non-main threads.

```csharp
// WRONG: UI update from background thread
Task.Run(() =>
{
    // Background processing
    textComponent.text = "Updated";  // Will fail - not on main thread
});

// CORRECT: Marshal to main thread
Task.Run(() =>
{
    // Background processing
    string result = ProcessData();
    
    // Update UI on main thread
    MelonCoroutines.Start(UpdateUICoroutine(result));
});

private static IEnumerator UpdateUICoroutine(string text)
{
    yield return null;  // Wait for next frame
    textComponent.text = text;  // Safe - on main thread
}
```

**Solution**: Use coroutines or invoke UI updates on the main thread.

## Build and Deployment Pitfalls

### 17. Missing Dependencies
**Problem**: Forgetting to include required references or dependencies.

```csharp
// Common missing references that cause compilation errors
// - Il2CppVampireSurvivors.Runtime.dll (main game classes)
// - Il2CppPhaserPort.dll (save system utilities)
// - Il2CppNewtonsoft.Json.dll (JSON handling)
// - MelonLoader.dll (mod framework)
// - Il2CppInterop.Runtime.dll (IL2CPP interop)
// - UnityEngine.CoreModule.dll (Unity core types)
```

**Solution**: Ensure all required DLLs are referenced in your project file:
```xml
<ItemGroup>
  <Reference Include="MelonLoader">
    <HintPath>$(MelonLoaderPath)\MelonLoader.dll</HintPath>
  </Reference>
  <!-- Add all other required references -->
</ItemGroup>
```

### 18. Incorrect Build Configuration
**Problem**: Using wrong target framework or build settings.

```xml
<!-- WRONG: Incorrect target framework -->
<TargetFramework>net472</TargetFramework>

<!-- CORRECT: Match game's framework -->
<TargetFramework>netcoreapp6.0</TargetFramework>
<LangVersion>10.0</LangVersion>
<AllowUnsafeBlocks>True</AllowUnsafeBlocks>
```

**Solution**: Match the target framework to the game's requirements (.NET Core 6.0) and enable unsafe blocks for IL2CPP interop.

## Debugging Pitfalls

### 19. Insufficient Logging
**Problem**: Not logging enough information to diagnose issues.

```csharp
// WRONG: Silent failures
if (data == null) return;  // What happened? Why is it null?

// CORRECT: Informative logging
if (data == null)
{
    MelonLogger.Warning("Data is null - possible causes: DataManager not initialized, save not loaded, or data corruption");
    return;
}

MelonLogger.Msg($"Processing {data.Count} items");
```

**Solution**: Implement comprehensive logging with context and diagnostic information.

### 21. Incorrect EggFloat/EggDouble Usage
**Problem**: Using the wrong methods to access or modify EggFloat and EggDouble values.

```csharp
// WRONG: Accessing wrapper values incorrectly
float power = playerStats.Power.Value;  // ERROR: No .Value property
double revivals = playerStats.Revivals.Value;  // ERROR: No .Value property

// CORRECT: Use proper access methods
float power = playerStats.Power.GetValue();
double revivals = playerStats.Revivals.GetValue();

// Setting values
playerStats.Power.SetValue(150f);
playerStats.Revivals.SetValue(3.0);  // Note: double, not float
```

**Solution**: Use `GetValue()` to read and `SetValue()` to modify EggFloat/EggDouble wrapped values.

### 22. Namespace Confusion Between Assemblies
**Problem**: Using types from the wrong namespace when multiple assemblies contain similar class names.

```csharp
// WRONG: Using Unity's CharacterController instead of VS CharacterController
using UnityEngine;
[HarmonyPatch(typeof(CharacterController), "Move")]  // Unity's CharacterController!

// CORRECT: Use the game's CharacterController
using Il2CppVampireSurvivors.Objects.Characters;
[HarmonyPatch(typeof(CharacterController), "LevelUp")]  // VS CharacterController
```

**Solution**: Always use fully qualified type names or verify namespace imports are correct.

### 23. Incorrect Property Access on PlayerModifierStats
**Problem**: Mixing direct field access with property access on PlayerModifierStats.

```csharp
// WRONG: Inconsistent access patterns
float shroud = playerStats.Shroud;  // Property access (correct for Shroud)
float power = playerStats._Power_k__BackingField.GetValue();  // Direct field (wrong)

// CORRECT: Use consistent property access
float shroud = playerStats.Shroud;  // Raw float property
float power = playerStats.Power.GetValue();  // EggFloat property
```

**Solution**: Always use the public properties, not backing fields. Check the decompiled source to understand which properties use wrappers vs raw values.

### 24. Not Testing Edge Cases
**Problem**: Only testing mods under ideal conditions.

**Common untested scenarios**:
- Game in menu vs. gameplay states
- Character with no weapons vs. fully equipped
- New game vs. loaded save game
- Different DLC combinations enabled/disabled
- Very high or very low stat values

**Solution**: Create comprehensive test scenarios covering edge cases and error conditions.

## Prevention Strategies

### Code Review Checklist
Before deploying a mod, verify:
- [ ] All IL2CPP objects are null-checked
- [ ] Exception handling is implemented in all hooks
- [ ] No high-frequency methods are hooked (OnTick, OnUpdate, RunMovementJob)
- [ ] File operations use proper directories and error handling
- [ ] String operations are optimized
- [ ] Correct wrapper types used for PlayerModifierStats (EggFloat, EggDouble, raw float)
- [ ] GetConvertedWeapons() used instead of raw JSON data
- [ ] GM.Core null checks implemented
- [ ] GetValue()/SetValue() used for EggFloat/EggDouble access
- [ ] Memory leaks are prevented
- [ ] Edge cases are tested

### Testing Protocol
1. **Menu testing**: Verify mod works in main menu
2. **Gameplay testing**: Test during active gameplay
3. **Save/load testing**: Ensure compatibility with save system
4. **Performance testing**: Monitor frame rate and memory usage
5. **Error testing**: Test with corrupted or missing data
6. **Compatibility testing**: Test with other mods if possible

### Monitoring and Maintenance
- Regularly check MelonLoader console for errors
- Monitor game performance after mod installation
- Keep backups of working mod versions
- Test mod compatibility with game updates
- Document known issues and workarounds