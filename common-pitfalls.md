# Common Pitfalls Reference

Reference for Vampire Survivors modding issues based on decompiled IL2CPP code analysis. All behavior patterns are inferred from static code analysis, not runtime testing.

## Data System

### Weapon Level Delta Values
Based on code analysis, weapon levels 2-8 appear to contain incremental deltas rather than absolute values.

```csharp
// Use converted data for absolute values
var weapons = dataManager.GetConvertedWeaponData();
var whipLevel3Power = weapons[WeaponType.WHIP][2].power;
```

### IL2CPP Type Prefixes
Game types require the `Il2Cpp` prefix based on IL2CPP compilation patterns.

```csharp
using Il2CppVampireSurvivors.Framework;
Il2CppVampireSurvivors.Framework.GameManager gameManager;
```

### GM.Core Null Reference
`GM.Core` appears to be null in menu screens based on code analysis.

```csharp
var dataManager = GM.Core?.DataManager;
if (dataManager == null) return;
```

### Runtime Assembly Reference
You need VampireSurvivors.Runtime.dll reference for core game classes.

```xml
<Reference Include="Il2CppVampireSurvivors.Runtime">
  <HintPath>F:\vampire\managed\Il2CppVampireSurvivors.Runtime.dll</HintPath>
</Reference>
```

### JSON Modification Reload
Based on code analysis, you need to call `ReloadAllData()` after direct JSON modifications.

```csharp
dataManager._allWeaponDataJson["WHIP"][0]["power"] = 100;
dataManager.ReloadAllData();
```

## IL2CPP Specifics

### Collection Null Validation
IL2CPP collections can contain null entries based on decompiled code patterns.

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
Different stats appear to use different wrapper types based on code analysis.

```csharp
playerStats.Revivals.SetValue(5.0);  // EggDouble - requires double
playerStats.Shroud = 0.5f;           // Raw float
playerStats.Power.SetValue(100f);    // EggFloat
```

### String Handling
Defensive string handling appears necessary with IL2CPP objects based on decompiled code patterns.

```csharp
string name = il2cppObject.name?.ToString();
if (!string.IsNullOrEmpty(name) && name.Equals("SomeName", StringComparison.OrdinalIgnoreCase))
```

## Performance

### High-Frequency Method Hooks
Avoid hooking methods that execute every frame based on performance considerations.

```csharp
// Hook initialization or event-driven methods
[HarmonyPatch(typeof(GameManager), "AddStartingWeapon")]
[HarmonyPostfix]
public static void SafeHook() { }
```

### String Allocations
Minimizing string operations in frequently called code appears beneficial for performance.

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
Wrapping hook logic in try-catch blocks appears necessary based on IL2CPP interop patterns.

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
Explicit priority appears necessary for execution order dependencies based on Harmony patterns.

```csharp
[HarmonyPatch(typeof(WeaponData), "power", MethodType.Getter)]
[HarmonyPostfix]
[HarmonyPriority(Priority.High)]
public static void HighPriorityHook(ref float __result) { }
```

## Save System

### Safe Save Modification

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
Based on Unity patterns, UI updates require main thread execution.

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

```xml
<TargetFramework>netcoreapp6.0</TargetFramework>
<LangVersion>10.0</LangVersion>
<AllowUnsafeBlocks>True</AllowUnsafeBlocks>
```

## EggFloat/EggDouble Access

### Proper Value Access
Based on decompiled code analysis, you use `GetValue()` and `SetValue()` methods for wrapper types.

```csharp
float power = playerStats.Power.GetValue();
double revivals = playerStats.Revivals.GetValue();

playerStats.Power.SetValue(150f);
playerStats.Revivals.SetValue(3.0);
```

## Namespace Resolution

### Correct Type References
Based on namespace analysis from decompiled code:

```csharp
using Il2CppVampireSurvivors.Objects.Characters;
[HarmonyPatch(typeof(CharacterController), "LevelUp")]
```

## Verification Points

- IL2CPP objects null-checked
- Exception handling in hooks
- Avoid high-frequency method hooks
- File operations use appropriate directories
- Wrapper types match PlayerModifierStats patterns
- GetConvertedWeaponData() for weapon data access
- GM.Core null validation
- GetValue()/SetValue() for EggFloat/EggDouble types
- Menu and gameplay state compatibility
- Save/load operation compatibility