# Hook Points Reference

## Data Hooks

### Data Loading
```csharp
// Raw JSON modification
[HarmonyPatch(typeof(DataManager), "LoadBaseJObjects")]
[HarmonyPostfix]
public static void OnDataLoad(DataManager __instance)
{
    // Modify raw JSON before conversion
}

// Post-conversion processing
[HarmonyPatch(typeof(DataManager), "ReloadAllData")]
[HarmonyPostfix]
public static void OnDataReload(DataManager __instance)
{
    // Called 7+ times during startup (base + DLCs)
}
```

### DLC Content Injection
```csharp
// Optimal point for custom DLC injection
[HarmonyPatch(typeof(LoadingManager), "ValidateVersion")]
[HarmonyPostfix]
public static void OnValidateVersion(LoadingManager __instance)
{
    // All base game and official DLCs are loaded
    // Data is complete but gameplay hasn't started
    // Safe to inject custom DLC content here
}

// Alternative hook for manifest loading
[HarmonyPatch(typeof(ManifestLoader), "ApplyBundleCore")]
[HarmonyPostfix]
public static void OnBundleApplied(DlcType dlcType, BundleManifestData manifest)
{
    // Called when each DLC bundle is applied
    // Can modify or extend DLC content
}
```

## Game Session Hooks

### Session Initialization
```csharp
[HarmonyPatch(typeof(GameManager), "Awake")]
[HarmonyPostfix]
public static void OnGameManagerAwake(GameManager __instance)
{
    // GM.Core available, all systems initialized
}

[HarmonyPatch(typeof(GameManager), "AddStartingWeapon")]
[HarmonyPostfix]
public static void OnGameplayStart(GameManager __instance, CharacterController character)
{
    // Game fully started, player ready
}
```

## Player Hooks

### Character Stats
```csharp
[HarmonyPatch(typeof(CharacterController), "PlayerStatsUpgrade")]
[HarmonyPostfix]
public static void OnPlayerStatsUpgrade(CharacterController __instance, ModifierStats other, bool multiplicativeMaxHp)
{
    // Stat progression modifications
}
```

### Weapon Management
```csharp
[HarmonyPatch(typeof(GameManager), "AddWeaponToPlayer")]
[HarmonyPostfix]
public static void OnWeaponAdded(GameManager __instance, GameplaySignals.AddWeaponToCharacterSignal signal)
{
    // Weapon addition events
}
```

## UI Hooks

### Main Menu
```csharp
// Manual patching required
var mainMenuType = typeof(Il2CppVampireSurvivors.AppMainMenuState);
var onEnterMethod = mainMenuType.GetMethod("OnEnter");
harmony.Patch(onEnterMethod, postfix: new HarmonyMethod(typeof(MyHooks).GetMethod("OnMainMenu")));
```

## Timing

### Startup Sequence
1. MelonLoader initialization
2. LoadBaseJObjects (raw JSON loading)
3. ReloadAllData (7+ calls: base + DLCs)
4. Main menu active

### Game Session
1. GameManager.Awake (session start)
2. AddStartingWeapon (gameplay begins)

## Performance Guidelines

### High-Frequency Methods (Avoid)
```csharp
// Do not hook - called every frame
EnemiesManager.OnTick()
GameManager.OnTickerCallback()
Projectile.OnUpdate()
CharacterController.OnUpdate()
```

### Expensive Operations (Use Caution)
```csharp
Stage.HandleSpawning()
Weapon.DealDamage()
MainGamePage.Update()
```

## Best Practices

### Exception Safety
```csharp
[HarmonyPostfix]
public static void SafeHook()
{
    try { /* logic */ }
    catch (Exception ex) { MelonLogger.Error(ex.Message); }
}
```

### Conditional Execution
```csharp
[HarmonyPostfix]
public static void ConditionalHook()
{
    if (!ModEnabled) return;
    // Hook logic
}
```

### Method Replacement
```csharp
[HarmonyPrefix]
public static bool ReplaceMethod()
{
    // Custom implementation
    return false; // Skip original
}
```

## Data Access

### Converted Data (Post-ReloadAllData)
```csharp
var weaponData = dataManager.AllWeaponData;
var characterData = dataManager.AllCharacters;
```

### Raw JSON (Post-LoadBaseJObjects)
```csharp
var weaponJson = dataManager._allWeaponDataJson;
var characterJson = dataManager._allCharactersJson;
```

### Player Stats
```csharp
character.PlayerStats.Power.SetValue(150f);
float power = character.PlayerStats.Power.GetValue();
```