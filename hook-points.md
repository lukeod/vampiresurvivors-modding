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
    // Called 7 times during startup (base + DLCs)
}
```

### DLC Content Injection
```csharp
// Custom DLC injection point
[HarmonyPatch(typeof(LoadingManager), "ValidateVersion")]
[HarmonyPostfix]
public static void OnValidateVersion(LoadingManager __instance, int index, Il2CppStructArray<DlcType> dlcs, Il2CppSystem.Action callback)
{
    // All base game and official DLCs loaded
    // Data complete, gameplay not started
    // Safe for custom DLC content injection
}

// Alternative hook for manifest loading
[HarmonyPatch(typeof(ManifestLoader), "ApplyBundleCore")]
[HarmonyPostfix]
public static void OnBundleApplied(DlcType dlcType, BundleManifestData manifest, Il2CppSystem.Action<BundleManifestData> onComplete)
{
    // Called when each DLC bundle is applied
    // Modify or extend DLC content
}
```

## Game Session Hooks

### Session Initialization
```csharp
[HarmonyPatch(typeof(GameManager), "Awake")]
[HarmonyPostfix]
public static void OnGameManagerAwake(GameManager __instance)
{
    // GM.Core available, systems initialized
}

[HarmonyPatch(typeof(GameManager), "AddStartingWeapon")]
[HarmonyPostfix]
public static void OnGameplayStart(GameManager __instance, CharacterController character)
{
    // Game started, player ready
}
```

## Player Hooks

### Character Stats
```csharp
[HarmonyPatch(typeof(CharacterController), "PlayerStatsUpgrade")]
[HarmonyPostfix]
public static void OnPlayerStatsUpgrade(CharacterController __instance, ModifierStats other, bool multiplicativeMaxHp)
{
    // Stat progression modification
}
```

### Weapon Management
```csharp
[HarmonyPatch(typeof(GameManager), "AddWeaponToPlayer")]
[HarmonyPostfix]
public static void OnWeaponAdded(GameManager __instance, GameplaySignals.AddWeaponToCharacterSignal signal)
{
    // Weapon addition event
}
```

## UI Hooks

### Main Menu
```csharp
[HarmonyPatch(typeof(AppMainMenuState), "OnEnter")]
[HarmonyPostfix]
public static void OnMainMenu(AppMainMenuState __instance)
{
    // Main menu initialized
}
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
// Access stats via dictionary lookup
var powerStat = character.PlayerStats.GetAllPowerUps()[PowerUpType.POWER];
powerStat._value = 150.0;
double power = powerStat._value;

// Alternative direct field access to stats dictionary
var stats = character.PlayerStats._stats;
if (stats.ContainsKey(PowerUpType.POWER))
{
    stats[PowerUpType.POWER]._value = 150.0;
}
```