# Hook Points Documentation

## Overview
This document outlines validated and recommended hook points for modding Vampire Survivors. Understanding when and where to hook into the game's execution flow is crucial for creating stable, performant mods.

## Essential Entry Points

### 1. Data Load Hook
**Purpose**: Hook into initial data loading to modify game data before it's processed.

```csharp
// Manual patching recommended for IL2CPP
var harmony = new Harmony("MyMod");
var method = typeof(DataManager).GetMethod("LoadBaseJObjects");
harmony.Patch(method, postfix: new HarmonyMethod(typeof(MyHooks).GetMethod("OnDataLoad")));

public static void OnDataLoad(DataManager __instance)
{
    // Earliest point to access and modify raw JSON data
    // All game data is loaded but not yet converted
    // Perfect for wholesale data modifications
}
```

**Use Cases**:
- Modifying weapon statistics in JSON
- Adding new character data
- Changing item properties
- Custom enemy configurations

### 2. Data Reload Hook
**Purpose**: Hook into data reloading to apply changes when data is refreshed.

```csharp
// Manual patching recommended for IL2CPP
var harmony = new Harmony("MyMod");
var method = typeof(DataManager).GetMethod("ReloadAllData");
harmony.Patch(method, postfix: new HarmonyMethod(typeof(MyHooks).GetMethod("OnDataReload")));

public static void OnDataReload(DataManager __instance)
{
    // Called multiple times during startup (5+ times for DLC loading)
    // Each call processes base game or DLC data
    // Data is fully converted and ready for use
    // Safe to call GetConverted methods here
}
```

**Important**: This method is called multiple times:
- Once for base game data
- Once for each DLC
- Final call for data validation

**Use Cases**:
- Applying changes after JSON modifications
- Validating data integrity after changes
- Initializing mod-specific data structures
- Setting up cross-references between modified data

### 3. GameManager Initialization
**Purpose**: Hook into GameManager when a game session starts.

```csharp
// Manual patching recommended for IL2CPP
var harmony = new Harmony("MyMod");
var method = typeof(GameManager).GetMethod("Awake");
harmony.Patch(method, postfix: new HarmonyMethod(typeof(MyHooks).GetMethod("OnGameManagerAwake")));

public static void OnGameManagerAwake(GameManager __instance)
{
    // Called when entering gameplay after character/stage selection, not during app startup
    // GM.Core is already set at this point
    // DataManager, Player, Stage, and MainUI are available
    // Perfect for gameplay-specific initialization
}
```

**Timing**: Called when entering gameplay after character/stage selection, not during app startup.

**Use Cases**:
- Storing GameManager reference for later use
- Setting up mod configuration
- Initializing global mod state
- Registering mod services

### 4. Gameplay Start Hook
**Purpose**: Best hook for gameplay-related modifications with full context.

```csharp
[HarmonyPatch(typeof(GameManager), "AddStartingWeapon")]
[HarmonyPostfix]
public static void OnGameplayStart(GameManager __instance, CharacterController character)
{
    // Game session is fully started
    // Player character is available and initialized
    // All systems are running
    // Ideal for gameplay mods
    // Note: AddStartingWeapon is actually a private method, but this hook still works
}
```

**Use Cases**:
- Modifying player stats at game start
- Adding custom starting equipment
- Setting up player-specific mod features
- Initializing gameplay tracking systems

### 5. Main Menu Hook
**Purpose**: Hook into main menu for UI modifications and mod setup.

```csharp
// Manual patching recommended due to IL2CPP type resolution issues
var mainMenuType = typeof(Il2CppVampireSurvivors.AppMainMenuState);
var onEnterMethod = mainMenuType.GetMethod("OnEnter");
if (onEnterMethod != null)
{
    harmony.Patch(onEnterMethod, postfix: new HarmonyMethod(typeof(MyHooks).GetMethod("OnMainMenu")));
}

public static void OnMainMenu(AppMainMenuState __instance)
{
    // Main menu is loaded and active
    // Use for UI modifications and menu-based mod features
}
```

**Known Issues**:
- Type accessibility problems with IL2CPP
- May need runtime type resolution
- Consider using OnUpdate monitoring as alternative

## Hook Timing Guide

### Application Startup Sequence
1. **MelonLoader Initialization**: Mods are loaded
2. **LoadBaseJObjects**: Raw JSON data loaded from files
3. **ReloadAllData** (Multiple calls):
   - Call 1: Base game data (~2 seconds)
   - Calls 2-4: DLC data loading (~2.5-3 seconds)
   - Call 5: Final data merge (~3.5 seconds)
4. **Main Menu Active**: Game ready for interaction (~5-10 seconds)

### Game Session Sequence
1. **Character Selection**: Player chooses character
2. **Stage Selection**: Player chooses stage
3. **GameManager.Awake**: Session initialization (when starting a game)
4. **GM.Core Available**: All game systems ready
5. **AddStartingWeapon**: Gameplay officially starts
6. **Game Loop**: Continuous gameplay systems active

### Menu/UI Sequence
1. **AppMainMenuState.OnEnter**: Main menu becomes active
2. **UI Page Transitions**: Various menu screens
3. **Character Selection UI**: Pre-game setup interfaces

## Performance-Critical Areas (AVOID HOOKING)

### Extremely Expensive Operations
**DO NOT HOOK** - These methods are called every frame for hundreds of objects:

```csharp
// NEVER HOOK THESE
EnemiesManager.OnTick()           // Called every frame (protected virtual)
EnemiesManager.RunMovementJob()   // Unity Job System processing (private)
GameManager.OnTickerCallback()   // Main game tick coordinator (private)
Projectile.OnUpdate()             // Hundreds of projectiles per frame (protected virtual)
CharacterController.OnUpdate()   // Per-character per-frame updates (protected virtual)
```

### Moderately Expensive Operations
**USE WITH EXTREME CAUTION** - These can impact performance:

```csharp
// MINIMIZE HOOKING THESE
Stage.HandleSpawning()            // Enemy spawn calculations (private)
Stage.FindClosestEnemy()          // Distance calculations (public)
Weapon.DealDamage()              // Damage calculations (public virtual)
MainGamePage.Update()            // UI updates (protected virtual)
```

## Safe Hook Categories

### One-Time Setup Hooks
Safe to hook as they execute infrequently:
- Data initialization methods
- Game state setup methods
- UI initialization methods
- Configuration loading methods

### Event-Driven Hooks
Safe when used appropriately:
- User input handlers
- Game state transition methods
- Achievement unlock methods
- Save/load operations

### Data Access Hooks
Generally safe for read-only operations:
- Weapon data getters
- Character data access methods
- Configuration property access

## Hook Implementation Best Practices

### 1. Minimal Processing
```csharp
[HarmonyPatch(typeof(SomeClass), "SomeMethod")]
[HarmonyPostfix]
public static void MinimalHook()
{
    // Do only what's absolutely necessary
    // Cache expensive calculations
    // Avoid complex logic in hooks
}
```

### 2. Exception Handling
```csharp
[HarmonyPatch(typeof(SomeClass), "SomeMethod")]
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
        // Graceful degradation - don't crash the game
    }
}
```

### 3. Conditional Execution
```csharp
[HarmonyPatch(typeof(SomeClass), "SomeMethod")]
[HarmonyPostfix]
public static void ConditionalHook()
{
    if (!ModEnabled || GameState != TargetState)
        return;
        
    // Only execute when conditions are met
}
```

### 4. Resource Cleanup
```csharp
public class ModClass : MelonMod
{
    public override void OnApplicationQuit()
    {
        // Clean up any resources
        // Unregister event handlers
        // Save mod configuration
    }
}
```

## Additional Useful Hook Points

### 5. Game Initialization Hooks
**Purpose**: Hook into various game initialization phases for setup-specific modifications.

```csharp
[HarmonyPatch(typeof(GameManager), "InitializeGame")]
[HarmonyPostfix]
public static void OnInitializeGame(GameManager __instance)
{
    // Called when a new game session is initialized
    // Good for session-specific setup
}

[HarmonyPatch(typeof(GameManager), "InitializeGameSession")]
[HarmonyPostfix]
public static void OnInitializeGameSession(GameManager __instance)
{
    // Called when game session begins
    // Player character and stage are being set up
}

[HarmonyPatch(typeof(GameManager), "InitializeGameSessionPostLoad")]
[HarmonyPostfix]
public static void OnInitializeGameSessionPostLoad(GameManager __instance)
{
    // Called after game session is fully loaded
    // All systems are ready and running
}
```

**Use Cases**:
- Setting up session-specific mod state
- Initializing custom game systems
- Applying mod configuration to new sessions
- Setting up player-specific data

### 6. Character Controller Hooks
**Purpose**: Hook into character-specific functionality and stat management.

```csharp
[HarmonyPatch(typeof(CharacterController), "PlayerStatsUpgrade")]
[HarmonyPostfix]
public static void OnPlayerStatsUpgrade(CharacterController __instance, ModifierStats other, bool multiplicativeMaxHp)
{
    // Called when player stats are upgraded
    // Perfect for modifying stat progression
}
```

**Use Cases**:
- Custom stat scaling
- Achievement tracking
- Dynamic difficulty adjustment
- Custom progression systems

### 7. Weapon Management Hooks
**Purpose**: Hook into weapon system for custom weapon behavior.

```csharp
[HarmonyPatch(typeof(GameManager), "AddWeaponToPlayer")]
[HarmonyPostfix]
public static void OnWeaponAdded(GameManager __instance, GameplaySignals.AddWeaponToCharacterSignal signal)
{
    // Called when a weapon is added to a player character
    // Signal contains Character and Weapon information
    // Good for weapon-specific modifications
}
```

**Use Cases**:
- Custom weapon effects
- Weapon synergy systems
- Dynamic weapon modifications
- Weapon acquisition tracking

## Common Hook Patterns

### Data Modification Pattern
```csharp
[HarmonyPatch(typeof(DataManager), "LoadBaseJObjects")]
[HarmonyPostfix]
public static void ModifyData(DataManager __instance)
{
    // Modify _allWeaponDataJson, _allCharactersJson, etc.
    ModifyWeaponData(__instance._allWeaponDataJson);
    ModifyCharacterData(__instance._allCharactersJson);
}

[HarmonyPatch(typeof(DataManager), "ReloadAllData")]
[HarmonyPostfix]
public static void OnDataReady(DataManager __instance)
{
    // Data is now converted and ready for use
    ValidateModifications(__instance);
}
```

### Runtime Modification Pattern
```csharp
[HarmonyPatch(typeof(GameManager), "AddStartingWeapon")]
[HarmonyPostfix]
public static void ModifyGameplay(CharacterController character)
{
    // Modify player stats or equipment at runtime
    if (character?.PlayerStats != null)
    {
        character.PlayerStats.Power.SetValue(100);
    }
}
```

### UI Modification Pattern
```csharp
[HarmonyPatch(typeof(MainGamePage), "Awake")]
[HarmonyPostfix]
public static void ModifyUI(MainGamePage __instance)
{
    // Modify UI elements after they're initialized
    // Add custom UI components
    // Change existing UI behavior
}
```

## Debugging and Validation

### Hook Execution Logging
```csharp
[HarmonyPatch(typeof(SomeClass), "SomeMethod")]
[HarmonyPostfix]
public static void LoggedHook()
{
    MelonLogger.Msg("Hook executed successfully");
    // Helps confirm hooks are working
}
```

### State Validation
```csharp
[HarmonyPatch(typeof(GameManager), "AddStartingWeapon")]
[HarmonyPostfix]
public static void ValidateState(GameManager __instance, CharacterController character)
{
    if (__instance == null)
    {
        MelonLogger.Warning("GameManager is null in hook");
        return;
    }
    
    if (character == null)
    {
        MelonLogger.Warning("Character is null in hook");
        return;
    }
    
    // Proceed with hook logic
}
```

## Common Pitfalls

### 1. Hooking High-Frequency Methods
**Problem**: Causes severe performance degradation
**Solution**: Use initialization hooks or cached values instead

### 2. Missing Null Checks
**Problem**: NullReferenceException crashes
**Solution**: Always validate IL2CPP object references

### 3. Exception Propagation
**Problem**: Unhandled exceptions can crash the game
**Solution**: Wrap hook logic in try-catch blocks

### 4. Resource Leaks
**Problem**: Hooks that allocate resources without cleanup
**Solution**: Implement proper cleanup in mod shutdown methods

### 5. Hook Order Dependencies
**Problem**: Multiple mods hooking the same method with order dependencies
**Solution**: Design hooks to be independent when possible

## Advanced Hook Techniques

### Prefix Hooks for Method Replacement
```csharp
[HarmonyPatch(typeof(SomeClass), "SomeMethod")]
[HarmonyPrefix]
public static bool ReplaceMethod()
{
    // Custom implementation
    // Return false to skip original method
    return false;
}
```

### Transpiler Hooks (NOT SUPPORTED)
**Important**: Transpiler hooks do not work with IL2CPP games. The C# code is compiled to C++, eliminating the IL that transpilers modify. Use Prefix/Postfix hooks instead.

### Multiple Hook Coordination
```csharp
// Use static flags to coordinate between hooks
public static bool ModificationComplete = false;

[HarmonyPatch(typeof(DataManager), "LoadBaseJObjects")]
[HarmonyPostfix]
public static void LoadHook()
{
    // Prepare modifications
    ModificationComplete = false;
}

[HarmonyPatch(typeof(DataManager), "ReloadAllData")]
[HarmonyPostfix]
public static void ReloadHook()
{
    // Finalize modifications
    ModificationComplete = true;
}
```

## Data Access Patterns

### Accessing Game Data
**Important**: Always access converted data after `ReloadAllData` has been called.

```csharp
[HarmonyPatch(typeof(DataManager), "ReloadAllData")]
[HarmonyPostfix]
public static void AccessGameData(DataManager __instance)
{
    // Access converted weapon data
    var weaponData = __instance.AllWeaponData; // Dictionary<WeaponType, JArray>
    
    // Access converted character data  
    var characterData = __instance.AllCharacters; // Dictionary<CharacterType, JArray>
    
    // Access raw JSON data (from LoadBaseJObjects hook)
    var rawWeaponJson = __instance._allWeaponDataJson; // JObject
    var rawCharacterJson = __instance._allCharactersJson; // JObject
}
```

### Player Stats Access
```csharp
[HarmonyPatch(typeof(GameManager), "AddStartingWeapon")]
[HarmonyPostfix]
public static void ModifyPlayerStats(CharacterController character)
{
    if (character?.PlayerStats != null)
    {
        // PlayerStats properties are EggFloat objects
        character.PlayerStats.Power.SetValue(150f);
        character.PlayerStats.Area.SetValue(2.0f);
        character.PlayerStats.Speed.SetValue(1.5f);
        
        // Get current values
        float currentPower = character.PlayerStats.Power.GetValue();
    }
}
```

### Key Data Properties
- `DataManager.AllWeaponData`: Dictionary<WeaponType, JArray> - Converted weapon data
- `DataManager.AllCharacters`: Dictionary<CharacterType, JArray> - Converted character data  
- `DataManager._allWeaponDataJson`: JObject - Raw weapon JSON data
- `DataManager._allCharactersJson`: JObject - Raw character JSON data
- `CharacterController.PlayerStats`: PlayerModifierStats - Character's current stats
- `GameManager.Instance`: Static access to the main GameManager instance