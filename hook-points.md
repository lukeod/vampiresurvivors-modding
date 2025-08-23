# Hook Points Reference

Based on analysis of decompiled IL2CPP source code. All method signatures and behavior patterns are inferred from static code analysis without runtime validation.

## Data Hooks

### Data Loading
```csharp
// Raw JSON modification - based on decompiled code analysis
[HarmonyPatch(typeof(DataManager), "LoadBaseJObjects")]
[HarmonyPostfix]
public static void OnDataLoad(DataManager __instance)
{
    // Access to raw JSON data before conversion to game objects
}

// Post-conversion processing - appears to be called multiple times
[HarmonyPatch(typeof(DataManager), "ReloadAllData")]
[HarmonyPostfix]
public static void OnDataReload(DataManager __instance)
{
    // Analysis suggests this is called approximately 7 times during startup
}
```

### DLC Content Injection
```csharp
// DLC validation hook - based on code analysis
[HarmonyPatch(typeof(LoadingManager), "ValidateVersion")]
[HarmonyPostfix]
public static void OnValidateVersion(LoadingManager __instance, int index, Il2CppStructArray<DlcType> dlcs, Il2CppSystem.Action callback)
{
    // Appears to be called after all base game and official DLCs are loaded
    // Based on code structure, data appears complete but gameplay not yet active
}

// Bundle manifest processing hook
[HarmonyPatch(typeof(ManifestLoader), "ApplyBundleCore")]
[HarmonyPostfix]
public static void OnBundleApplied(DlcType dlcType, BundleManifestData manifest, Il2CppSystem.Action<BundleManifestData> onComplete)
{
    // Inferred to be called when each DLC bundle is processed
}
```

## Game Session Hooks

### Session Initialization
```csharp
[HarmonyPatch(typeof(GameManager), "Awake")]
[HarmonyPostfix]
public static void OnGameManagerAwake(GameManager __instance)
{
    // Based on Unity lifecycle, GM.Core should be available after Awake
}

[HarmonyPatch(typeof(GameManager), "AddStartingWeapon")]
[HarmonyPostfix]
public static void OnGameplayStart(GameManager __instance, CharacterController character)
{
    // Appears to mark the beginning of active gameplay
}
```

## Player Hooks

### Character Stats
```csharp
[HarmonyPatch(typeof(CharacterController), "PlayerStatsUpgrade")]
[HarmonyPostfix]
public static void OnPlayerStatsUpgrade(CharacterController __instance, ModifierStats other, bool multiplicativeMaxHp)
{
    // Hook point for stat modification events
}
```

### Weapon Management
```csharp
[HarmonyPatch(typeof(GameManager), "AddWeaponToPlayer")]
[HarmonyPostfix]
public static void OnWeaponAdded(GameManager __instance, GameplaySignals.AddWeaponToCharacterSignal signal)
{
    // Called when weapons are added to the player character
}
```

## UI Hooks

### Main Menu
```csharp
[HarmonyPatch(typeof(AppMainMenuState), "OnEnter")]
[HarmonyPostfix]
public static void OnMainMenu(AppMainMenuState __instance)
{
    // Called when entering the main menu state
}
```

## Timing

### Startup Sequence
Based on code analysis, the initialization sequence appears to follow this pattern:
1. MelonLoader initialization
2. LoadBaseJObjects (raw JSON loading)
3. ReloadAllData (multiple calls for base game and DLC content)
4. Main menu state activation

### Game Session
Inferred session flow:
1. GameManager.Awake (session initialization)
2. AddStartingWeapon (gameplay activation)

## Performance Considerations

### High-Frequency Methods
Based on method names and Unity patterns, these methods likely execute frequently:
```csharp
EnemiesManager.OnTick()
GameManager.OnTickerCallback()
Projectile.OnUpdate()
CharacterController.OnUpdate()
```

### Performance-Sensitive Operations
Methods that may impact performance based on code analysis:
```csharp
Stage.HandleSpawning()
Weapon.DealDamage()
MainGamePage.Update()
```

## Implementation Patterns

### Exception Handling
```csharp
[HarmonyPostfix]
public static void SafeHook()
{
    try 
    { 
        // Your hook logic here
    }
    catch (Exception ex) 
    { 
        MelonLogger.Error($"Hook error: {ex.Message}"); 
    }
}
```

### Conditional Execution
```csharp
[HarmonyPostfix]
public static void ConditionalHook()
{
    if (!ModEnabled) return;
    // Execute hook logic only when mod is enabled
}
```

### Method Override
```csharp
[HarmonyPrefix]
public static bool ReplaceMethod()
{
    // Implement replacement behavior
    return false; // Prevents original method execution
}
```

## Data Access

### Converted Data Access
After ReloadAllData completion, data appears available through:
```csharp
var weaponData = dataManager.AllWeaponData;
var characterData = dataManager.AllCharacters;
```

### Raw JSON Access
After LoadBaseJObjects, raw JSON appears accessible via:
```csharp
var weaponJson = dataManager._allWeaponDataJson;
var characterJson = dataManager._allCharactersJson;
```

### Player Statistics
Based on decompiled code structure, player stats appear accessible through:
```csharp
// Dictionary-based stat access
var powerStat = character.PlayerStats.GetAllPowerUps()[PowerUpType.POWER];
powerStat._value = 150.0;
double power = powerStat._value;

// Direct stats dictionary access
var stats = character.PlayerStats._stats;
if (stats.ContainsKey(PowerUpType.POWER))
{
    stats[PowerUpType.POWER]._value = 150.0;
}
```