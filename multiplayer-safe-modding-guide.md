# Vampire Survivors Multiplayer-Safe Modding Guide

*The complete practical reference for developing mods that work across single-player, local co-op, and online multiplayer modes*

**Last Updated:** January 2025  
**Game Version:** v1.0+ (Unity 6000.0.36f1)  
**Coherence Toolkit:** Integrated  

## Table of Contents

1. [Overview & Essential Knowledge](#overview--essential-knowledge)
2. [Critical Multiplayer Patterns](#critical-multiplayer-patterns)
3. [Safe Hook Points & Patterns](#safe-hook-points--patterns)
4. [Dangerous Patterns to Avoid](#dangerous-patterns-to-avoid)
5. [Authority & Synchronization](#authority--synchronization)
6. [Common Modding Scenarios](#common-modding-scenarios)
7. [Testing Methodology](#testing-methodology)
8. [Troubleshooting Guide](#troubleshooting-guide)
9. [Quick Reference](#quick-reference)

---

## Overview & Essential Knowledge

### Why Multiplayer Awareness Matters

Vampire Survivors v1.0+ supports three distinct game modes:

- **Single Player**: Full control, no network concerns
- **Local Co-op**: Up to 4 players, shared screen, no network
- **Online Multiplayer**: Network-synchronized via Coherence Toolkit

**⚠️ CRITICAL**: All mods MUST handle these three modes or risk:
- Crashes in multiplayer sessions
- Desynchronization between players
- Host/client conflicts
- Broken game state

### The Golden Rule

```csharp
// ALWAYS check game mode before modifying anything
var gm = GM.Core;
if (gm == null) return; // GM.Core is null in menus

if (gm.IsOnlineMultiplayer)
{
    // Network multiplayer - EXTREME CAUTION required
    // Check authority, respect host/client roles
}
else if (gm.IsLocalMultiplayer)
{
    // Local co-op - moderate caution
    // Consider multiple players, shared state
}
else
{
    // Single player - full control available
}
```

## Critical Multiplayer Patterns

### 1. Game Mode Detection

```csharp
[HarmonyPatch(typeof(GameManager), "Awake")]
[HarmonyPostfix]
public static void OnGameStart(GameManager __instance)
{
    // Triple-check pattern for all mods
    var isOnline = __instance.IsOnlineMultiplayer;     // Network play
    var isLocal = __instance.IsLocalMultiplayer;      // Local co-op
    var isMulti = __instance._multiplayerManager.IsMultiplayer; // Any multiplayer
    
    LogInfo($"Game Mode - Online: {isOnline}, Local: {isLocal}, Multi: {isMulti}");
    
    // Your initialization logic here based on mode
    if (isOnline)
    {
        InitializeNetworkMode(__instance);
    }
    else if (isLocal)
    {
        InitializeLocalCoopMode(__instance);
    }
    else
    {
        InitializeSinglePlayerMode(__instance);
    }
}
```

### 2. Player Management

```csharp
// Get all active players safely
public static List<Player> GetAllPlayers()
{
    var gm = GM.Core;
    if (gm?.MultiplayerManager == null) return new List<Player>();
    
    try
    {
        return gm._multiplayerManager.GetPlayers()?.ToList() ?? new List<Player>();
    }
    catch (Exception ex)
    {
        LogError($"Failed to get players: {ex.Message}");
        return new List<Player>();
    }
}

// Get player's character safely
public static CharacterController GetPlayerCharacter(Player player)
{
    var gm = GM.Core;
    if (gm?.MultiplayerManager == null || player == null) return null;
    
    try
    {
        return gm._multiplayerManager.GetCharacter(player);
    }
    catch (Exception ex)
    {
        LogError($"Failed to get character for player {player.id}: {ex.Message}");
        return null;
    }
}
```

### 3. Network Authority Checking

#### Authority Types

Coherence Toolkit provides two types of authority:
- **State Authority (`HasStateAuthority`)**: Permission to modify entity state (health, stats, position)
- **Input Authority (`HasInputAuthority`)**: Permission to send input commands for entity control

**Important**: There is NO simple `HasAuthority` property. Always use the specific authority type you need.

```csharp
// Check if we can safely modify a networked entity's state
public static bool CanModifyEntity(CharacterController character)
{
    var gm = GM.Core;
    if (gm == null) return false;
    
    // Single player and local co-op - always safe
    if (!gm.IsOnlineMultiplayer) return true;
    
    // Online multiplayer - check network authority
    try
    {
        var coherenceSync = character?.Sync;
        if (coherenceSync == null) return false;
        
        // Only modify if we have state authority
        return coherenceSync.HasStateAuthority;
    }
    catch (Exception ex)
    {
        LogError($"Authority check failed: {ex.Message}");
        return false;
    }
}

// Alternative authority check pattern
public static bool IsLocallyControlled(CharacterController character)
{
    var gm = GM.Core;
    if (gm?.IsOnlineMultiplayer != true) return true; // Always local in non-online modes
    
    // Check if this character belongs to the local player
    var players = GetAllPlayers();
    foreach (var player in players)
    {
        var playerChar = GetPlayerCharacter(player);
        if (playerChar == character)
        {
            // Check if this player is local
            return IsLocalPlayer(player);
        }
    }
    
    return false;
}
```

## Safe Hook Points & Patterns

### Initialization Hooks (SAFE)

```csharp
// Game Manager initialization - always safe
[HarmonyPatch(typeof(GameManager), "Awake")]
[HarmonyPostfix]
public static void OnGameManagerAwake(GameManager __instance)
{
    // GM.Core is available, all systems initialized
    // Safe to query multiplayer state and setup mode-specific logic
}

// Gameplay start - always safe
[HarmonyPatch(typeof(GameManager), "AddStartingWeapon")]
[HarmonyPostfix]
public static void OnGameplayStart(GameManager __instance, CharacterController character)
{
    // Game fully started, player ready
    // Safe to modify character stats, add custom behaviors
}
```

### Data Loading Hooks (SAFE)

```csharp
// Data modification - safe across all modes
[HarmonyPatch(typeof(DataManager), "ReloadAllData")]
[HarmonyPostfix]
public static void OnDataReload(DataManager __instance)
{
    // Modify weapon data, character stats, etc.
    // This affects all players equally in all modes
    ModifyWeaponData(__instance);
    ModifyCharacterData(__instance);
}

private static void ModifyWeaponData(DataManager dataManager)
{
    try
    {
        var weapons = dataManager.GetConvertedWeaponData();
        if (weapons?.ContainsKey(WeaponType.WHIP) == true)
        {
            var whipLevels = weapons[WeaponType.WHIP];
            foreach (var level in whipLevels)
            {
                if (level != null)
                {
                    level.power *= 1.5f; // 50% damage boost
                }
            }
        }
    }
    catch (Exception ex)
    {
        LogError($"Failed to modify weapon data: {ex.Message}");
    }
}
```

### UI Hooks (MOSTLY SAFE)

```csharp
// UI modifications - generally safe but check for multiplayer-specific UI
[HarmonyPatch(typeof(MainGamePage), "UpdateExperienceProgress")]
[HarmonyPostfix]
public static void OnXPProgressUpdate(GameplaySignals.CharacterXpChangedSignal sig)
{
    // UI updates are local to each client
    // Safe to modify visual elements, add overlays
    if (CustomXPDisplay.isEnabled)
    {
        CustomXPDisplay.UpdateProgress(sig.CurrentXp, sig.MaxXp);
    }
}

// Main menu modifications - always safe
[HarmonyPatch(typeof(AppMainMenuState), "OnEnter")]
[HarmonyPostfix]
public static void OnMainMenu(AppMainMenuState __instance)
{
    // Menu modifications are always safe
    AddCustomMenuButtons(__instance);
}
```

### Player Event Hooks (SAFE WITH CHECKS)

```csharp
// Player lifecycle events - safe with proper checks
[HarmonyPatch(typeof(CharacterController), "LevelUp")]
[HarmonyPostfix]
public static void OnCharacterLevelUp(CharacterController __instance)
{
    try
    {
        var gm = GM.Core;
        if (gm == null) return;
        
        // Check if we should handle this level up
        if (gm.IsOnlineMultiplayer && !CanModifyEntity(__instance)) return;
        
        // Apply custom level up effects
        ApplyCustomLevelUpBonus(__instance);
        
        // Log for all modes
        LogInfo($"Character leveled up: {__instance.name}");
    }
    catch (Exception ex)
    {
        LogError($"Level up hook failed: {ex.Message}");
    }
}

private static void ApplyCustomLevelUpBonus(CharacterController character)
{
    if (character?.PlayerStats == null) return;
    
    // Safe stat modifications
    var powerStat = character.PlayerStats.GetAllPowerUps()?.GetValueOrDefault(PowerUpType.POWER);
    if (powerStat != null)
    {
        var currentPower = powerStat.GetValue();
        powerStat.SetValue(currentPower + 5f); // +5 power per level
    }
}
```

## Dangerous Patterns to Avoid

### HIGH FREQUENCY HOOKS (NEVER DO THIS)

```csharp
// ❌ WRONG - These methods are called every frame and will destroy performance
[HarmonyPatch(typeof(CharacterController), "OnUpdate")]
[HarmonyPostfix]
public static void OnCharacterUpdate() { /* DON'T DO THIS */ }

[HarmonyPatch(typeof(GameManager), "OnTickerCallback")]
[HarmonyPostfix]  
public static void OnGameTick() { /* DON'T DO THIS */ }

[HarmonyPatch(typeof(EnemiesManager), "OnTick")]
[HarmonyPostfix]
public static void OnEnemyTick() { /* DON'T DO THIS */ }

// ✅ CORRECT - Use these alternatives instead
[HarmonyPatch(typeof(CharacterController), "LevelUp")]
[HarmonyPostfix]
public static void OnLevelUp() { /* Event-driven, safe */ }

[HarmonyPatch(typeof(GameManager), "AddStartingWeapon")]
[HarmonyPostfix]
public static void OnGameStart() { /* Initialization, safe */ }
```

### UNSAFE NETWORK ENTITY MODIFICATION

```csharp
// ❌ WRONG - Direct modification without authority check
[HarmonyPatch(typeof(CharacterController), "TakeDamage")]
[HarmonyPrefix]
public static bool PreventDamage(CharacterController __instance)
{
    __instance.health = __instance.maxHealth; // DANGEROUS in online mode
    return false; // Skip original method
}

// ✅ CORRECT - Authority-aware modification
[HarmonyPatch(typeof(CharacterController), "TakeDamage")]
[HarmonyPrefix]
public static bool SafePreventDamage(CharacterController __instance)
{
    var gm = GM.Core;
    if (gm?.IsOnlineMultiplayer == true)
    {
        // Online mode - check authority
        if (!CanModifyEntity(__instance)) return true; // Let original method run
        
        // Only host or entity owner can modify health
        var sync = __instance.Sync;
        if (sync?.HasStateAuthority != true) return true;
    }
    
    // Safe to modify in single player or if we have authority
    __instance.health = __instance.maxHealth;
    return false;
}
```

### UNSAFE SIGNAL INTERFERENCE

```csharp
// ❌ WRONG - Blocking critical signals can break multiplayer
[HarmonyPatch(typeof(SignalBus), "Fire")]
[HarmonyPrefix]
public static bool BlockAllSignals() 
{
    return false; // This will break multiplayer synchronization
}

// ✅ CORRECT - Selective signal interception
[HarmonyPatch(typeof(SignalBus), "Fire")]
[HarmonyPrefix]
public static bool SafeSignalIntercept(Il2CppSystem.Object signal)
{
    var gm = GM.Core;
    if (gm?.IsOnlineMultiplayer == true)
    {
        // Don't interfere with online signals
        var signalType = signal?.GetType().Name;
        if (signalType?.Contains("Online") == true) return true;
    }
    
    // Safe to intercept non-critical signals
    if (signal is GameplaySignals.TimeStopSignal)
    {
        LogInfo("Time stop signal intercepted");
        // Custom logic here
    }
    
    return true; // Always let original signal through
}
```

## Authority & Synchronization

### Understanding Network Authority

```csharp
public static class NetworkAuthority
{
    // Check if local client has authority over an entity
    public static bool HasStateAuthority(MonoBehaviour entity)
    {
        var gm = GM.Core;
        if (gm?.IsOnlineMultiplayer != true) return true; // Always have authority in non-online modes
        
        var coherenceSync = entity?.GetComponent<CoherenceSync>();
        return coherenceSync?.HasStateAuthority == true;
    }
    
    // Check if local client is the host
    public static bool IsHost()
    {
        var gm = GM.Core;
        if (gm?.IsOnlineMultiplayer != true) return true; // Always "host" in non-online modes
        
        // Host detection logic - this is game-specific
        // You may need to inspect the networking implementation for exact detection
        try
        {
            // Check if we're the first player (usually the host)
            var players = gm._multiplayerManager?.GetPlayers();
            if (players?.Count > 0)
            {
                var firstPlayer = players[0];
                return IsLocalPlayer(firstPlayer);
            }
        }
        catch (Exception ex)
        {
            LogError($"Host detection failed: {ex.Message}");
        }
        
        return false;
    }
    
    // Check if a player is the local player
    public static bool IsLocalPlayer(Player player)
    {
        if (player == null) return false;
        
        try
        {
            // This is Rewired-specific logic
            return player.isPlayer && player.id == 0; // Local player is typically ID 0
        }
        catch (Exception ex)
        {
            LogError($"Local player check failed: {ex.Message}");
            return false;
        }
    }
}
```

### Safe Entity Modification Patterns

```csharp
// Pattern 1: Host-Only Modifications
public static void ModifyEntityHostOnly(CharacterController character, Action<CharacterController> modification)
{
    var gm = GM.Core;
    if (gm == null) return;
    
    if (gm.IsOnlineMultiplayer)
    {
        // Only host can modify in online mode
        if (!NetworkAuthority.IsHost()) return;
        
        // Additional authority check
        if (!NetworkAuthority.HasStateAuthority(character)) return;
    }
    
    try
    {
        modification(character);
    }
    catch (Exception ex)
    {
        LogError($"Entity modification failed: {ex.Message}");
    }
}

// Pattern 2: Owner-Only Modifications
public static void ModifyOwnEntityOnly(CharacterController character, Action<CharacterController> modification)
{
    var gm = GM.Core;
    if (gm == null) return;
    
    if (gm.IsOnlineMultiplayer)
    {
        // Only the entity owner can modify
        if (!NetworkAuthority.HasStateAuthority(character)) return;
    }
    
    try
    {
        modification(character);
    }
    catch (Exception ex)
    {
        LogError($"Entity modification failed: {ex.Message}");
    }
}

// Pattern 3: Visual-Only Modifications (Always Safe)
public static void ModifyVisualOnly(CharacterController character, Action<CharacterController> visualChange)
{
    // Visual changes are always safe as they don't affect game state
    try
    {
        visualChange(character);
    }
    catch (Exception ex)
    {
        LogError($"Visual modification failed: {ex.Message}");
    }
}
```

## Common Modding Scenarios

### Scenario 1: Character Stat Modifications

```csharp
[HarmonyPatch(typeof(CharacterController), "PlayerStatsUpgrade")]
[HarmonyPostfix]
public static void EnhanceCharacterStats(CharacterController __instance, ModifierStats other, bool multiplicativeMaxHp)
{
    var gm = GM.Core;
    if (gm == null || __instance?.PlayerStats == null) return;
    
    // This is safe in all modes because it affects stat calculation uniformly
    try
    {
        var stats = __instance.PlayerStats.GetAllPowerUps();
        if (stats == null) return;
        
        // Apply custom stat bonuses
        if (stats.ContainsKey(PowerUpType.POWER))
        {
            var powerStat = stats[PowerUpType.POWER];
            var currentPower = powerStat.GetValue();
            powerStat.SetValue(currentPower * 1.1f); // 10% power boost
        }
        
        LogInfo($"Enhanced stats for character: {__instance.name}");
    }
    catch (Exception ex)
    {
        LogError($"Stat enhancement failed: {ex.Message}");
    }
}
```

### Scenario 2: Weapon Behavior Changes

```csharp
[HarmonyPatch(typeof(GameManager), "AddWeaponToPlayer")]
[HarmonyPostfix]
public static void OnWeaponAdded(GameManager __instance, GameplaySignals.AddWeaponToCharacterSignal signal)
{
    try
    {
        var gm = GM.Core;
        if (gm == null) return;
        
        // Log weapon addition for all modes
        LogInfo($"Weapon added: {signal.WeaponType} to character");
        
        // Apply custom weapon behavior
        if (signal.WeaponType == WeaponType.WHIP)
        {
            ApplyCustomWhipBehavior(__instance, signal);
        }
    }
    catch (Exception ex)
    {
        LogError($"Weapon addition hook failed: {ex.Message}");
    }
}

private static void ApplyCustomWhipBehavior(GameManager gameManager, GameplaySignals.AddWeaponToCharacterSignal signal)
{
    // This is safe because we're not modifying networked entities directly
    // We're just setting up behavior that will be consistent across all clients
    
    LogInfo("Applied custom whip behavior");
    // Custom weapon behavior logic here
}
```

### Scenario 3: Enemy Behavior Alterations

```csharp
[HarmonyPatch(typeof(EnemyController), "Initialize")]
[HarmonyPostfix]
public static void ModifyEnemyBehavior(EnemyController __instance)
{
    var gm = GM.Core;
    if (gm == null) return;
    
    // Enemy modifications need authority checking in online mode
    if (gm.IsOnlineMultiplayer)
    {
        // Only host should modify enemy behavior in online mode
        if (!NetworkAuthority.IsHost()) return;
        
        // Check if we have authority over this enemy
        if (!NetworkAuthority.HasStateAuthority(__instance)) return;
    }
    
    try
    {
        // Apply custom enemy modifications
        ModifyEnemyStats(__instance);
    }
    catch (Exception ex)
    {
        LogError($"Enemy modification failed: {ex.Message}");
    }
}

private static void ModifyEnemyStats(EnemyController enemy)
{
    if (enemy == null) return;
    
    // Example: Make enemies 20% stronger
    enemy.health *= 1.2f;
    enemy.maxHealth *= 1.2f;
    
    LogInfo($"Modified enemy stats: {enemy.name}");
}
```

### Scenario 4: UI Additions

```csharp
// UI modifications are generally safe in all modes
[HarmonyPatch(typeof(MainGamePage), "Awake")]
[HarmonyPostfix]
public static void AddCustomUI(MainGamePage __instance)
{
    try
    {
        // Add custom UI elements
        CreateCustomStatsDisplay(__instance);
    }
    catch (Exception ex)
    {
        LogError($"UI addition failed: {ex.Message}");
    }
}

private static void CreateCustomStatsDisplay(MainGamePage mainGamePage)
{
    // UI creation logic - safe in all modes
    LogInfo("Created custom stats display");
}
```

### Scenario 5: Game Mode Changes

```csharp
// Game mode modifications should be data-driven and safe
[HarmonyPatch(typeof(DataManager), "ReloadAllData")]
[HarmonyPostfix]
public static void ModifyGameMode(DataManager __instance)
{
    try
    {
        // Modify stage data to create custom game modes
        ModifyStageData(__instance);
    }
    catch (Exception ex)
    {
        LogError($"Game mode modification failed: {ex.Message}");
    }
}

private static void ModifyStageData(DataManager dataManager)
{
    try
    {
        var stageData = dataManager.AllStageData;
        if (stageData == null) return;
        
        // Example: Modify enemy spawn rates
        foreach (var stage in stageData.Values)
        {
            if (stage?._spawnEvents != null)
            {
                foreach (var spawnEvent in stage._spawnEvents)
                {
                    if (spawnEvent != null)
                    {
                        spawnEvent._density *= 1.5f; // 50% more enemies
                    }
                }
            }
        }
        
        LogInfo("Modified stage data for custom game mode");
    }
    catch (Exception ex)
    {
        LogError($"Stage data modification failed: {ex.Message}");
    }
}
```

## Testing Methodology

### Test Environment Setup

```csharp
public static class ModTestHelper
{
    public static void LogGameModeInfo()
    {
        var gm = GM.Core;
        if (gm == null)
        {
            LogInfo("GM.Core is null - probably in menu");
            return;
        }
        
        LogInfo("=== GAME MODE INFORMATION ===");
        LogInfo($"Is Online Multiplayer: {gm.IsOnlineMultiplayer}");
        LogInfo($"Is Local Multiplayer: {gm.IsLocalMultiplayer}");
        LogInfo($"Is Any Multiplayer: {gm._multiplayerManager?.IsMultiplayer}");
        LogInfo($"Player Count: {gm._multiplayerManager?.GetPlayerCount() ?? 0}");
        
        if (gm.IsOnlineMultiplayer)
        {
            LogInfo($"Host Status: {NetworkAuthority.IsHost()}");
            LogNetworkInfo();
        }
        
        LogInfo("============================");
    }
    
    private static void LogNetworkInfo()
    {
        try
        {
            var players = GM.Core._multiplayerManager?.GetPlayers();
            if (players != null)
            {
                LogInfo($"Network Players ({players.Count}):");
                for (int i = 0; i < players.Count; i++)
                {
                    var player = players[i];
                    var isLocal = NetworkAuthority.IsLocalPlayer(player);
                    var character = GM.Core._multiplayerManager.GetCharacter(player);
                    var hasAuthority = character != null && NetworkAuthority.HasStateAuthority(character);
                    
                    LogInfo($"  Player {i}: ID={player.id}, Local={isLocal}, Authority={hasAuthority}");
                }
            }
        }
        catch (Exception ex)
        {
            LogError($"Failed to log network info: {ex.Message}");
        }
    }
    
    public static void TestModInAllModes()
    {
        LogInfo("=== MOD FUNCTIONALITY TEST ===");
        LogGameModeInfo();
        
        // Test your mod's functionality here
        TestCustomFeatures();
        
        LogInfo("==============================");
    }
    
    private static void TestCustomFeatures()
    {
        // Add your mod's test logic here
        LogInfo("Testing custom mod features...");
        
        // Example: Test stat modifications
        var players = GetAllPlayers();
        foreach (var player in players)
        {
            var character = GetPlayerCharacter(player);
            if (character != null)
            {
                TestCharacterModifications(character);
            }
        }
    }
    
    private static void TestCharacterModifications(CharacterController character)
    {
        try
        {
            var canModify = CanModifyEntity(character);
            LogInfo($"Can modify {character.name}: {canModify}");
            
            if (canModify)
            {
                // Test safe modifications
                var stats = character.PlayerStats?.GetAllPowerUps();
                if (stats?.ContainsKey(PowerUpType.POWER) == true)
                {
                    var power = stats[PowerUpType.POWER].GetValue();
                    LogInfo($"Character power: {power}");
            }
        }
        catch (Exception ex)
        {
            LogError($"Character test failed: {ex.Message}");
        }
    }
}

// Add this to your mod's OnUpdate for testing
[HarmonyPatch(typeof(GameManager), "Awake")]
[HarmonyPostfix]
public static void OnGameStart(GameManager __instance)
{
    // Test the mod in all detected modes
    ModTestHelper.TestModInAllModes();
}
```

### Testing Checklist

```csharp
public static class TestingChecklist
{
    public static void RunAllTests()
    {
        LogInfo("=== MULTIPLAYER MOD TESTING CHECKLIST ===");
        
        TestGameModeDetection();
        TestPlayerManagement();
        TestAuthorityChecking();
        TestDataModifications();
        TestUIModifications();
        TestErrorHandling();
        
        LogInfo("==========================================");
    }
    
    private static void TestGameModeDetection()
    {
        LogInfo("✓ Testing game mode detection...");
        var gm = GM.Core;
        
        if (gm != null)
        {
            LogInfo($"  - Online: {gm.IsOnlineMultiplayer}");
            LogInfo($"  - Local: {gm.IsLocalMultiplayer}");
            LogInfo($"  - Multi: {gm._multiplayerManager?.IsMultiplayer}");
        }
        else
        {
            LogInfo("  - GM.Core is null (menu mode)");
        }
    }
    
    private static void TestPlayerManagement()
    {
        LogInfo("✓ Testing player management...");
        var players = GetAllPlayers();
        LogInfo($"  - Found {players.Count} players");
        
        foreach (var player in players)
        {
            var character = GetPlayerCharacter(player);
            LogInfo($"  - Player {player.id}: {(character != null ? "has character" : "no character")}");
        }
    }
    
    private static void TestAuthorityChecking()
    {
        LogInfo("✓ Testing authority checking...");
        var gm = GM.Core;
        
        if (gm?.IsOnlineMultiplayer == true)
        {
            LogInfo($"  - Is Host: {NetworkAuthority.IsHost()}");
            
            var players = GetAllPlayers();
            foreach (var player in players)
            {
                var character = GetPlayerCharacter(player);
                if (character != null)
                {
                    var hasAuthority = NetworkAuthority.HasStateAuthority(character);
                    LogInfo($"  - Player {player.id} authority: {hasAuthority}");
                }
            }
        }
        else
        {
            LogInfo("  - Not in online mode, authority checks N/A");
        }
    }
    
    private static void TestDataModifications()
    {
        LogInfo("✓ Testing data modifications...");
        var gm = GM.Core;
        
        if (gm?.DataManager != null)
        {
            var weapons = gm.DataManager.GetConvertedWeaponData();
            LogInfo($"  - Weapon data available: {weapons != null}");
            
            var characters = gm.DataManager.AllCharacters;
            LogInfo($"  - Character data available: {characters != null}");
        }
        else
        {
            LogInfo("  - DataManager not available");
        }
    }
    
    private static void TestUIModifications()
    {
        LogInfo("✓ Testing UI modifications...");
        // Test UI-related functionality
        LogInfo("  - UI modifications are client-local and generally safe");
    }
    
    private static void TestErrorHandling()
    {
        LogInfo("✓ Testing error handling...");
        try
        {
            // Deliberately cause a handled error to test error handling
            var nullRef = (CharacterController)null;
            var canModify = CanModifyEntity(nullRef);
            LogInfo($"  - Null reference handled: {!canModify}");
        }
        catch (Exception ex)
        {
            LogError($"  - Error handling failed: {ex.Message}");
        }
    }
}
```

## Troubleshooting Guide

### Common Issues & Solutions

#### 1. "Mod works in single player but breaks in multiplayer"

**Symptoms:**
- Crashes when joining online sessions
- Desynchronization between players
- Features only work for host

**Solution:**
```csharp
// Add multiplayer checks to all hook points
[HarmonyPatch(typeof(SomeClass), "SomeMethod")]
[HarmonyPostfix]
public static void SafeHook(SomeClass __instance)
{
    var gm = GM.Core;
    if (gm == null) return;
    
    // Add mode-specific logic
    if (gm.IsOnlineMultiplayer)
    {
        // Check authority before modifying
        if (!CanModifyEntity(__instance)) return;
    }
    
    // Your mod logic here
}
```

#### 2. "Players see different game states"

**Symptoms:**
- Host sees different stats than clients
- Items appear for some players but not others
- Inconsistent enemy behavior

**Solution:**
```csharp
// Ensure modifications are deterministic and authority-based
public static void ModifyGameState()
{
    var gm = GM.Core;
    if (gm?.IsOnlineMultiplayer == true)
    {
        // Only host should modify shared game state
        if (!NetworkAuthority.IsHost()) return;
    }
    
    // Apply modifications that affect all players equally
}
```

#### 3. "Authority errors in online mode"

**Symptoms:**
- "No authority to modify" errors
- Changes not synchronizing
- Inconsistent entity states

**Solution:**
```csharp
// Always check authority before modifying networked entities
public static void SafeEntityModification(MonoBehaviour entity)
{
    var gm = GM.Core;
    if (gm?.IsOnlineMultiplayer == true)
    {
        var sync = entity.GetComponent<CoherenceSync>();
        if (sync?.HasStateAuthority != true)
        {
            LogWarning($"No authority to modify {entity.name}");
            return;
        }
    }
    
    // Safe to modify
}
```

#### 4. "Performance issues in multiplayer"

**Symptoms:**
- Frame rate drops in multiplayer
- Network lag increases
- High CPU usage

**Solution:**
```csharp
// Avoid high-frequency hooks and expensive operations
// Use event-driven patterns instead of per-frame checks

// ❌ WRONG
[HarmonyPatch(typeof(CharacterController), "Update")]
[HarmonyPostfix]
public static void ExpensiveHook() { /* Heavy computation */ }

// ✅ CORRECT
[HarmonyPatch(typeof(CharacterController), "LevelUp")]
[HarmonyPostfix]
public static void EfficientHook() { /* Event-driven, efficient */ }
```

### Debug Information Collection

```csharp
public static class DebugCollector
{
    public static void CollectDebugInfo()
    {
        LogInfo("=== DEBUG INFORMATION COLLECTION ===");
        
        CollectGameModeInfo();
        CollectPlayerInfo();
        CollectNetworkInfo();
        CollectPerformanceInfo();
        
        LogInfo("====================================");
    }
    
    private static void CollectGameModeInfo()
    {
        var gm = GM.Core;
        LogInfo("Game Mode Info:");
        LogInfo($"  GM.Core available: {gm != null}");
        
        if (gm != null)
        {
            LogInfo($"  IsOnlineMultiplayer: {gm.IsOnlineMultiplayer}");
            LogInfo($"  IsLocalMultiplayer: {gm.IsLocalMultiplayer}");
            LogInfo($"  MultiplayerManager available: {gm._multiplayerManager != null}");
            
            if (gm._multiplayerManager != null)
            {
                LogInfo($"  IsMultiplayer: {gm._multiplayerManager.IsMultiplayer}");
                LogInfo($"  Player count: {gm._multiplayerManager.GetPlayerCount()}");
            }
        }
    }
    
    private static void CollectPlayerInfo()
    {
        LogInfo("Player Info:");
        var players = GetAllPlayers();
        LogInfo($"  Total players: {players.Count}");
        
        for (int i = 0; i < players.Count; i++)
        {
            var player = players[i];
            var character = GetPlayerCharacter(player);
            var isLocal = NetworkAuthority.IsLocalPlayer(player);
            
            LogInfo($"  Player {i}:");
            LogInfo($"    ID: {player.id}");
            LogInfo($"    Is Local: {isLocal}");
            LogInfo($"    Has Character: {character != null}");
            
            if (character != null)
            {
                var hasAuthority = NetworkAuthority.HasStateAuthority(character);
                LogInfo($"    Has State Authority: {hasAuthority}");
                LogInfo($"    Character Name: {character.name}");
            }
        }
    }
    
    private static void CollectNetworkInfo()
    {
        var gm = GM.Core;
        if (gm?.IsOnlineMultiplayer != true)
        {
            LogInfo("Network Info: Not in online mode");
            return;
        }
        
        LogInfo("Network Info:");
        LogInfo($"  Is Host: {NetworkAuthority.IsHost()}");
        
        // Collect Coherence-specific information if available
        var characters = GameObject.FindObjectsOfType<CharacterController>();
        LogInfo($"  Networked Characters: {characters.Length}");
        
        foreach (var character in characters)
        {
            var sync = character.GetComponent<CoherenceSync>();
            if (sync != null)
            {
                LogInfo($"    {character.name}: State Authority={sync.HasStateAuthority}, Input Authority={sync.HasInputAuthority}");
            }
        }
    }
    
    private static void CollectPerformanceInfo()
    {
        LogInfo("Performance Info:");
        LogInfo($"  Frame Rate: {1f / Time.deltaTime:F1} FPS");
        LogInfo($"  Delta Time: {Time.deltaTime * 1000f:F1}ms");
        LogInfo($"  Time Scale: {Time.timeScale}");
        
        // Collect memory info if needed
        var memoryUsage = System.GC.GetTotalMemory(false);
        LogInfo($"  Memory Usage: {memoryUsage / 1024 / 1024}MB");
    }
}
```

## Quick Reference

### Essential Checks (Copy-Paste Ready)

```csharp
// Game mode detection
var gm = GM.Core;
if (gm == null) return; // Menu mode

var isOnline = gm.IsOnlineMultiplayer;     // Network multiplayer
var isLocal = gm.IsLocalMultiplayer;      // Local co-op
var isMulti = gm._multiplayerManager.IsMultiplayer; // Any multiplayer

// Authority checking
public static bool CanModifyEntity(MonoBehaviour entity)
{
    var gm = GM.Core;
    if (gm?.IsOnlineMultiplayer != true) return true;
    
    var sync = entity?.GetComponent<CoherenceSync>();
    return sync?.HasStateAuthority == true;
}

// Safe player iteration
var players = gm._multiplayerManager?.GetPlayers();
if (players != null)
{
    foreach (var player in players)
    {
        var character = gm._multiplayerManager.GetCharacter(player);
        if (character != null && CanModifyEntity(character))
        {
            // Safe to modify this character
        }
    }
}

// Safe hook pattern
[HarmonyPatch(typeof(SomeClass), "SomeMethod")]
[HarmonyPostfix]
public static void SafeHook(SomeClass __instance)
{
    try
    {
        var gm = GM.Core;
        if (gm == null) return;
        
        if (gm.IsOnlineMultiplayer)
        {
            if (!CanModifyEntity(__instance)) return;
        }
        
        // Your mod logic here
    }
    catch (Exception ex)
    {
        LogError($"Hook failed: {ex.Message}");
    }
}
```

### Safe Hook Points

| Hook Point | Safety Level | Notes |
|------------|-------------|--------|
| `GameManager.Awake` | ✅ SAFE | Initialization, all modes |
| `DataManager.ReloadAllData` | ✅ SAFE | Data modifications, affects all players |
| `CharacterController.LevelUp` | ⚠️ CHECK AUTHORITY | Player events, needs authority check |
| `MainGamePage.UpdateExperienceProgress` | ✅ SAFE | UI updates, client-local |
| `SignalBus.Fire` | ⚠️ DANGEROUS | Can break multiplayer if misused |
| `CharacterController.Update` | ❌ NEVER | High frequency, performance killer |

### Authority Check Quick Reference

| Scenario | Check Required | Pattern |
|----------|----------------|---------|
| Data modifications | ❌ NO | Affects all players equally |
| UI changes | ❌ NO | Client-local |
| Character stats | ⚠️ ONLINE ONLY | `CanModifyEntity(character)` |
| Enemy behavior | ⚠️ ONLINE ONLY | `IsHost() && HasStateAuthority(enemy)` |
| Game state | ⚠️ ONLINE ONLY | `IsHost()` |

### Testing Modes

1. **Single Player**: Full control, test basic functionality
2. **Local Co-op**: Multiple players, shared screen, test scaling
3. **Online Multiplayer**: Network mode, test authority and synchronization

### Error Handling Template

```csharp
try
{
    var gm = GM.Core;
    if (gm == null) return;
    
    // Null checks
    if (someObject?.someProperty == null) return;
    
    // Authority checks
    if (gm.IsOnlineMultiplayer && !CanModifyEntity(entity)) return;
    
    // Your logic here
}
catch (Exception ex)
{
    LogError($"Operation failed: {ex.Message}");
}
```

---

## Summary

Creating multiplayer-safe mods for Vampire Survivors requires:

1. **Always check game mode** before making modifications
2. **Respect network authority** in online multiplayer
3. **Use safe hook points** and avoid high-frequency methods
4. **Test in all three modes** (single, local co-op, online)
5. **Handle errors gracefully** with proper exception handling
6. **Follow the patterns** shown in this guide

Remember: **When in doubt, check authority and test in online mode!**

The multiplayer system is robust, but it requires mods to be respectful of the network architecture. Following these patterns will ensure your mod works seamlessly across all game modes and provides a great experience for all players.