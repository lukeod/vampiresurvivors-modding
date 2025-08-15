# Common Pitfalls Reference

Quick solutions for common Vampire Survivors modding issues.

## Data System

### Weapon Level Delta Values
Weapon levels 2-8 contain incremental deltas, not absolute values.

```csharp
// Use converted data for absolute values
var weapons = dataManager.GetConvertedWeapons();
var whipLevel3Power = weapons[WeaponType.WHIP][2].power;
```

### IL2CPP Type Prefixes
All game types require the `Il2Cpp` prefix.

```csharp
using Il2CppVampireSurvivors.Framework;
Il2CppVampireSurvivors.Framework.GameManager gameManager;
```

### GM.Core Null Reference
`GM.Core` is null in menu screens.

```csharp
var dataManager = GM.Core?.DataManager;
if (dataManager == null) return;
```

### Runtime Assembly Reference
Add VampireSurvivors.Runtime.dll reference for core game classes.

```xml
<Reference Include="Il2CppVampireSurvivors.Runtime">
  <HintPath>F:\vampire\managed\Il2CppVampireSurvivors.Runtime.dll</HintPath>
</Reference>
```

### JSON Modification Reload
Call `ReloadAllData()` after direct JSON modifications.

```csharp
dataManager._allWeaponDataJson["WHIP"][0]["power"] = 100;
dataManager.ReloadAllData();
```

## IL2CPP Specifics

### Collection Null Validation
IL2CPP collections can contain null entries.

```csharp
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

### PlayerModifierStats Wrapper Types
Different stats use different wrapper types.

```csharp
playerStats.Revivals.SetValue(5.0);  // EggDouble - requires double
playerStats.Shroud = 0.5f;           // Raw float
playerStats.Power.SetValue(100f);    // EggFloat
```

### String Handling
Use defensive string handling with IL2CPP objects.

```csharp
string name = il2cppObject.name?.ToString();
if (!string.IsNullOrEmpty(name) && name.Equals("SomeName", StringComparison.OrdinalIgnoreCase))
```

## Performance

### High-Frequency Method Hooks
Avoid hooking methods that execute every frame.

```csharp
// Hook initialization or event-driven methods instead
[HarmonyPatch(typeof(GameManager), "AddStartingWeapon")]
[HarmonyPostfix]
public static void SafeHook() { }
```

### String Allocations
Minimize string operations in frequently called code.

```csharp
private static readonly StringBuilder _stringBuilder = new();

public static void OptimizedStringHandling()
{
    if (DebugMode)
    {
        _stringBuilder.Clear();
        _stringBuilder.Append("Message");
        MelonLogger.Msg(_stringBuilder.ToString());
    }
}
```

## Harmony Hooks

### Exception Handling
Wrap all hook logic in try-catch blocks.

```csharp
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

### Hook Priority
Use explicit priority for execution order dependencies.

```csharp
[HarmonyPatch(typeof(WeaponData), "power", MethodType.Getter)]
[HarmonyPostfix]
[HarmonyPriority(Priority.High)]
public static void HighPriorityHook(ref float __result) { }
```

## Save System

### Safe Save Modification
Create backups before modifying save files.

```csharp
public static void SafeModifySave(string newData)
{
    string savePath = PhaserSaveDataUtils.GetSaveDataPathWithSave();
    string backupPath = savePath + ".backup";
    
    try
    {
        File.Copy(savePath, backupPath);
        File.WriteAllText(savePath, newData);
        
        if (ValidateSaveFile(savePath))
        {
            File.Delete(backupPath);
        }
        else
        {
            File.Copy(backupPath, savePath);
            throw new Exception("Save validation failed");
        }
    }
    catch (Exception ex)
    {
        MelonLogger.Error($"Save modification failed: {ex.Message}");
        if (File.Exists(backupPath))
        {
            File.Copy(backupPath, savePath);
        }
    }
}
```

## File I/O

### Mod Data Directory
Use MelonLoader directories for mod data storage.

```csharp
string userDataPath = MelonEnvironment.UserDataDirectory;
string modDataPath = Path.Combine(userDataPath, "MyMod");
Directory.CreateDirectory(modDataPath);
```

### File Permission Validation
Check permissions before file operations.

```csharp
public static bool SafeWriteFile(string filePath, string data)
{
    try
    {
        string directory = Path.GetDirectoryName(filePath);
        if (!Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }
        
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

## UI and Threading

### Main Thread UI Updates
Use coroutines for UI updates from background threads.

```csharp
Task.Run(() =>
{
    string result = ProcessData();
    MelonCoroutines.Start(UpdateUICoroutine(result));
});

private static IEnumerator UpdateUICoroutine(string text)
{
    yield return null;
    textComponent.text = text;
}
```

## Build Configuration

### Required Dependencies
Include all necessary DLL references.

```xml
<ItemGroup>
  <Reference Include="MelonLoader">
    <HintPath>$(MelonLoaderPath)\MelonLoader.dll</HintPath>
  </Reference>
  <Reference Include="Il2CppVampireSurvivors.Runtime">
    <HintPath>$(GamePath)\Il2CppVampireSurvivors.Runtime.dll</HintPath>
  </Reference>
</ItemGroup>
```

### Target Framework
Match game's framework requirements.

```xml
<TargetFramework>netcoreapp6.0</TargetFramework>
<LangVersion>10.0</LangVersion>
<AllowUnsafeBlocks>True</AllowUnsafeBlocks>
```

## EggFloat/EggDouble Access

### Proper Value Access
Use `GetValue()` and `SetValue()` methods.

```csharp
float power = playerStats.Power.GetValue();
double revivals = playerStats.Revivals.GetValue();

playerStats.Power.SetValue(150f);
playerStats.Revivals.SetValue(3.0);
```

## Namespace Resolution

### Correct Type References
Use game-specific types, not Unity equivalents.

```csharp
using Il2CppVampireSurvivors.Objects.Characters;
[HarmonyPatch(typeof(CharacterController), "LevelUp")]
```

## Testing Checklist

- [ ] IL2CPP objects null-checked
- [ ] Exception handling in all hooks
- [ ] No high-frequency method hooks
- [ ] Proper file operation directories
- [ ] Correct wrapper types for PlayerModifierStats
- [ ] GetConvertedWeapons() for weapon data
- [ ] GM.Core null checks
- [ ] GetValue()/SetValue() for EggFloat/EggDouble
- [ ] Menu and gameplay state testing
- [ ] Save/load compatibility testing