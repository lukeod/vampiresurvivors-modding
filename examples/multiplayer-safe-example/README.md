# Multiplayer Safe Example Mod

This example mod demonstrates all the essential patterns for creating multiplayer-safe mods for Vampire Survivors. It's designed as a comprehensive reference implementation that you can study, copy from, and adapt for your own mods.

## What This Mod Does

- **Character Stat Bonuses**: +5 power per level up
- **Weapon Damage Boost**: +20% damage to all weapons  
- **Enemy Health Scaling**: +30% health to all enemies
- **Custom UI Indicators**: Shows XP progress in debug mode
- **Comprehensive Debugging**: F1-F4 keys for testing and info

## Key Features Demonstrated

### 1. Proper Game Mode Detection
```csharp
var gm = GM.Core;
if (gm == null) return; // Menu mode

if (gm.IsOnlineMultiplayer)
{
    // Network multiplayer - check authority
}
else if (gm.IsLocalMultiplayer)
{
    // Local co-op - multiple players
}
else
{
    // Single player - full control
}
```

### 2. Authority Checking
```csharp
public static bool CanModifyEntity(MonoBehaviour entity)
{
    var gm = GM.Core;
    if (gm?.IsOnlineMultiplayer != true) return true;
    
    var sync = entity?.GetComponent<CoherenceSync>();
    return sync?.HasAuthority == true;
}
```

### 3. Safe Hook Patterns
- ✅ **Always Safe**: Data modifications, UI changes, menu hooks
- ⚠️ **Check Authority**: Character events, entity modifications
- 🏠 **Host Only**: Enemy spawning, game state changes

### 4. Player Management
```csharp
var players = GetAllPlayers();
foreach (var player in players)
{
    var character = GetPlayerCharacter(player);
    if (character != null && CanModifyEntity(character))
    {
        // Safe to modify this character
    }
}
```

## How to Build

1. Ensure paths in `.csproj` point to your Vampire Survivors installation
2. Build the project using Visual Studio or `dotnet build`
3. Copy the built DLL to `VampireSurvivors/Mods/`

## Testing Checklist

Use the debug controls to test in all modes:

- **F1**: Toggle debug mode for detailed logging
- **F2**: Show current game mode information  
- **F3**: Test authority checks for all players
- **F4**: Show detailed player information

### Test Sequence

1. **Single Player**: Verify basic functionality works
2. **Local Co-op**: Test with 2+ controllers, check player scaling
3. **Online Host**: Test as host in online multiplayer
4. **Online Client**: Test as client joining someone else's game

## Safe Patterns Used

### Data Modifications (Always Safe)
```csharp
[HarmonyPatch(typeof(DataManager), "ReloadAllData")]
[HarmonyPostfix]
public static void OnDataReload(DataManager __instance)
{
    // Safe in all modes - affects all players equally
    ModifyWeaponData(__instance);
}
```

### Character Events (Authority Check Required)
```csharp
[HarmonyPatch(typeof(CharacterController), "LevelUp")]
[HarmonyPostfix]
public static void OnCharacterLevelUp(CharacterController __instance)
{
    var gm = GM.Core;
    if (gm?.IsOnlineMultiplayer == true && !CanModifyEntity(__instance)) return;
    
    // Safe to modify
}
```

### Enemy Events (Host Only)
```csharp
[HarmonyPatch(typeof(EnemyController), "Initialize")]
[HarmonyPostfix]
public static void OnEnemyInitialize(EnemyController __instance)
{
    var gm = GM.Core;
    if (gm?.IsOnlineMultiplayer == true && !IsHost()) return;
    
    // Host-only modification
}
```

### UI Changes (Always Safe)
```csharp
[HarmonyPatch(typeof(MainGamePage), "UpdateExperienceProgress")]
[HarmonyPostfix]
public static void OnXPProgressUpdate(GameplaySignals.CharacterXpChangedSignal sig)
{
    // UI changes are client-local and always safe
}
```

## Common Pitfalls Avoided

### ❌ Never Do This
```csharp
// High-frequency hooks (performance killer)
[HarmonyPatch(typeof(CharacterController), "Update")]

// Direct modification without checks (breaks multiplayer)
character.health = 100; // No authority check!

// Blocking critical signals (breaks synchronization)
[HarmonyPrefix] return false; // On important signals
```

### ✅ Always Do This
```csharp
// Check GM.Core first
var gm = GM.Core;
if (gm == null) return;

// Check authority in online mode
if (gm.IsOnlineMultiplayer && !CanModifyEntity(entity)) return;

// Wrap in try-catch
try { /* modification */ }
catch (Exception ex) { LogError($"Failed: {ex.Message}"); }
```

## Error Handling Pattern

Every hook follows this pattern:
```csharp
[HarmonyPostfix]
public static void SafeHook(SomeClass __instance)
{
    try
    {
        var gm = GM.Core;
        if (gm == null) return; // Null check
        
        // Authority checks
        if (gm.IsOnlineMultiplayer && !CanModifyEntity(__instance)) return;
        
        // Your logic here
        DoModification(__instance);
    }
    catch (Exception ex)
    {
        MelonLogger.Error($"Hook failed: {ex.Message}");
        // Never rethrow in hook methods
    }
}
```

## Performance Considerations

- ✅ **Event-driven hooks**: LevelUp, Initialize, etc.
- ✅ **Cached calculations**: Only compute when needed
- ✅ **Efficient logging**: Debug mode only
- ❌ **Per-frame hooks**: Update, OnTick, etc.
- ❌ **Expensive operations**: Heavy computation in hooks

## Multiplayer Behavior

### Single Player
- All features work normally
- Full modification control
- No network overhead

### Local Co-op
- Features work for all players
- Shared screen considerations
- No network synchronization needed

### Online Multiplayer
- Host controls enemy modifications
- Players control their own characters
- UI changes are per-client
- Network synchronization handled automatically

## Adaptation for Your Mod

1. Copy the authority checking methods
2. Use the safe hook patterns
3. Follow the error handling template
4. Test in all three game modes
5. Add debug controls for testing

This example provides a solid foundation for any multiplayer-safe mod. The patterns demonstrated here will work for character modifications, weapon changes, enemy behavior, UI additions, and more.