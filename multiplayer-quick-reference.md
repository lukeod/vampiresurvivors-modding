# Vampire Survivors Multiplayer Modding Quick Reference

*Essential patterns and checks for multiplayer-safe modding*

## The Three Sacred Checks

```csharp
// 1. GM.Core null check (always first)
var gm = GM.Core;
if (gm == null) return; // We're in a menu

// 2. Game mode detection
bool isOnline = gm.IsOnlineMultiplayer;   // Network multiplayer
bool isLocal = gm.IsLocalMultiplayer;     // Local co-op
bool isMulti = gm._multiplayerManager.IsMultiplayer; // Any multiplayer

// 3. Authority check (online only)
if (isOnline && !CanModifyEntity(entity)) return; // No authority
```

## Copy-Paste Authority Checker

```csharp
public static bool CanModifyEntity(MonoBehaviour entity)
{
    var gm = GM.Core;
    if (gm?.IsOnlineMultiplayer != true) return true; // Safe in non-online modes
    
    try
    {
        var sync = entity?.GetComponent<CoherenceSync>();
        return sync?.HasAuthority == true;
    }
    catch (Exception ex)
    {
        LogError($"Authority check failed: {ex.Message}");
        return false;
    }
}

public static bool IsHost()
{
    var gm = GM.Core;
    if (gm?.IsOnlineMultiplayer != true) return true; // Always "host" in non-online
    
    try
    {
        var players = gm._multiplayerManager?.GetPlayers();
        return players?.Count > 0 && IsLocalPlayer(players[0]);
    }
    catch { return false; }
}

public static bool IsLocalPlayer(Player player)
{
    try
    {
        return player?.isPlayer == true && player.id == 0;
    }
    catch { return false; }
}
```

## Safe Hook Templates

### Universal Safe Hook (Copy This!)

```csharp
[HarmonyPatch(typeof(TargetClass), "TargetMethod")]
[HarmonyPostfix]
public static void SafeModification(TargetClass __instance)
{
    try
    {
        var gm = GM.Core;
        if (gm == null) return; // Menu mode
        
        if (gm.IsOnlineMultiplayer)
        {
            // Online multiplayer - check authority
            if (!CanModifyEntity(__instance)) return;
        }
        
        // Your modification logic here
        ApplyModification(__instance);
    }
    catch (Exception ex)
    {
        LogError($"Modification failed: {ex.Message}");
    }
}
```

### Host-Only Modification

```csharp
[HarmonyPatch(typeof(GameManager), "SomeGameStateMethod")]
[HarmonyPostfix]
public static void HostOnlyModification(GameManager __instance)
{
    try
    {
        var gm = GM.Core;
        if (gm == null) return;
        
        if (gm.IsOnlineMultiplayer && !IsHost()) return; // Only host modifies
        
        // Game state modification
        ModifyGameState(__instance);
    }
    catch (Exception ex)
    {
        LogError($"Host modification failed: {ex.Message}");
    }
}
```

### Data-Only Modification (Always Safe)

```csharp
[HarmonyPatch(typeof(DataManager), "ReloadAllData")]
[HarmonyPostfix]
public static void ModifyData(DataManager __instance)
{
    try
    {
        // Data modifications are safe in all modes
        var weapons = __instance.GetConvertedWeapons();
        if (weapons?.ContainsKey(WeaponType.WHIP) == true)
        {
            foreach (var level in weapons[WeaponType.WHIP])
            {
                if (level != null) level.power *= 1.5f; // 50% boost
            }
        }
    }
    catch (Exception ex)
    {
        LogError($"Data modification failed: {ex.Message}");
    }
}
```

### UI-Only Modification (Always Safe)

```csharp
[HarmonyPatch(typeof(MainGamePage), "UpdateExperienceProgress")]
[HarmonyPostfix]
public static void ModifyUI(GameplaySignals.CharacterXpChangedSignal sig)
{
    try
    {
        // UI modifications are client-local and always safe
        UpdateCustomXPDisplay(sig.CurrentXp, sig.MaxXp);
    }
    catch (Exception ex)
    {
        LogError($"UI modification failed: {ex.Message}");
    }
}
```

## Player Iteration Pattern

```csharp
public static void ProcessAllPlayers(Action<Player, CharacterController> action)
{
    var gm = GM.Core;
    if (gm?._multiplayerManager == null) return;
    
    try
    {
        var players = gm._multiplayerManager.GetPlayers();
        if (players == null) return;
        
        foreach (var player in players)
        {
            var character = gm._multiplayerManager.GetCharacter(player);
            if (character != null)
            {
                action(player, character);
            }
        }
    }
    catch (Exception ex)
    {
        LogError($"Player iteration failed: {ex.Message}");
    }
}

// Usage:
ProcessAllPlayers((player, character) =>
{
    if (CanModifyEntity(character))
    {
        // Safe to modify this character
        ModifyCharacter(character);
    }
});
```

## Dangerous Methods to Avoid

```csharp
// ❌ NEVER HOOK THESE (Performance killers)
CharacterController.Update()
CharacterController.OnUpdate()
GameManager.OnTickerCallback()
EnemiesManager.OnTick()
Projectile.OnUpdate()

// ❌ DANGEROUS WITHOUT CHECKS
CharacterController.TakeDamage()    // Needs authority check
EnemyController.Initialize()        // Needs host check
GameManager.SpawnEnemy()           // Needs host check

// ❌ NEVER BLOCK THESE
SignalBus.Fire() // Can break multiplayer synchronization
```

## Safe Hook Points

```csharp
// ✅ ALWAYS SAFE
GameManager.Awake                  // Initialization
DataManager.ReloadAllData          // Data modifications
AppMainMenuState.OnEnter          // Menu modifications
MainGamePage.UpdateExperienceProgress // UI updates

// ✅ SAFE WITH CHECKS
CharacterController.LevelUp        // Player events
GameManager.AddWeaponToPlayer      // Weapon events
CharacterController.PlayerStatsUpgrade // Stat events

// ✅ SAFE FOR HOST ONLY
EnemyController.Initialize         // Enemy spawn
GameManager.PauseGame             // Game state
Stage.HandleSpawning              // Stage events
```

## Emergency Debugging

```csharp
// Add this to any hook for instant debug info
public static void DebugGameState()
{
    var gm = GM.Core;
    MelonLogger.Msg("=== DEBUG INFO ===");
    MelonLogger.Msg($"GM.Core: {gm != null}");
    
    if (gm != null)
    {
        MelonLogger.Msg($"Online: {gm.IsOnlineMultiplayer}");
        MelonLogger.Msg($"Local: {gm.IsLocalMultiplayer}");
        MelonLogger.Msg($"Players: {gm._multiplayerManager?.GetPlayerCount() ?? 0}");
        MelonLogger.Msg($"Host: {IsHost()}");
    }
    MelonLogger.Msg("=================");
}
```

## Mode-Specific Patterns

### Single Player Mode
```csharp
if (!gm.IsOnlineMultiplayer && !gm.IsLocalMultiplayer)
{
    // Full control - do anything
    ModifyAnything();
}
```

### Local Co-op Mode
```csharp
if (gm.IsLocalMultiplayer && !gm.IsOnlineMultiplayer)
{
    // Multiple players, shared state, no network
    // Consider all players when modifying
    var playerCount = gm._multiplayerManager.GetPlayerCount();
    ModifyForLocalCoop(playerCount);
}
```

### Online Multiplayer Mode
```csharp
if (gm.IsOnlineMultiplayer)
{
    // Network mode - extreme caution
    if (IsHost())
    {
        // Host can modify game state
        ModifyAsHost();
    }
    else
    {
        // Client - only modify owned entities
        ModifyOwnedEntitiesOnly();
    }
}
```

## Error Handling Template

```csharp
try
{
    // Null checks first
    var gm = GM.Core;
    if (gm == null) return;
    
    if (someObject?.someProperty == null) return;
    
    // Authority checks for online mode
    if (gm.IsOnlineMultiplayer)
    {
        if (!CanModifyEntity(entity)) return;
    }
    
    // Your modification logic
    DoModification();
}
catch (Exception ex)
{
    MelonLogger.Error($"[ModName] Operation failed: {ex.Message}");
    // Never rethrow in hook methods
}
```

## Stat Modification Template

```csharp
public static void ModifyCharacterStats(CharacterController character, float powerBonus)
{
    if (character?.PlayerStats == null) return;
    
    try
    {
        var stats = character.PlayerStats.GetAllPowerUps();
        if (stats?.ContainsKey(PowerUpType.POWER) == true)
        {
            var powerStat = stats[PowerUpType.POWER];
            var currentPower = powerStat.GetValue();
            powerStat.SetValue(currentPower + powerBonus);
        }
    }
    catch (Exception ex)
    {
        LogError($"Stat modification failed: {ex.Message}");
    }
}
```

## Testing Checklist

- [ ] ✅ Tested in single player
- [ ] ✅ Tested in local co-op (2+ players)
- [ ] ✅ Tested in online multiplayer (host)
- [ ] ✅ Tested in online multiplayer (client)
- [ ] ✅ No high-frequency hooks used
- [ ] ✅ All hooks have try-catch blocks
- [ ] ✅ Authority checks where needed
- [ ] ✅ Null checks everywhere
- [ ] ✅ No direct Signal.Fire blocking

## When Things Go Wrong

### Mod works in single player but crashes in multiplayer
- Add GM.Core null check
- Add authority checks for entity modifications
- Remove high-frequency hooks

### Players see different states
- Move logic to host-only
- Use data modifications instead of entity modifications
- Check for deterministic behavior

### "No authority" errors
- Add `CanModifyEntity()` checks
- Only modify owned entities
- Use host-only pattern for game state

### Performance issues in multiplayer
- Remove Update/OnTick hooks
- Use event-driven patterns
- Reduce network traffic

## Remember

1. **GM.Core can be null** (always check first)
2. **Authority matters in online mode** (check before modifying)
3. **Data changes are safe** (affect all players equally)
4. **UI changes are safe** (client-local)
5. **When in doubt, test online** (most issues appear there)

**Golden Rule**: If it modifies game state, check authority first!