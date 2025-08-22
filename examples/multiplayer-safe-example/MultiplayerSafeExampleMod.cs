using MelonLoader;
using HarmonyLib;
using Il2CppVampireSurvivors;
using Il2CppVampireSurvivors.Framework;
using Il2CppVampireSurvivors.Data;
using Il2CppVampireSurvivors.Objects.Characters;
using Il2CppVampireSurvivors.Signals;
using Il2CppVampireSurvivors.UI;
using Il2CppCoherence.Toolkit;
using Il2CppRewired;
using System;
using System.Collections.Generic;
using UnityEngine;

[assembly: MelonInfo(typeof(MultiplayerSafeExampleMod), "Multiplayer Safe Example", "1.0.0", "VS Modding Community")]
[assembly: MelonGame("poncle", "Vampire Survivors")]

/// <summary>
/// Example mod demonstrating all multiplayer-safe patterns and best practices.
/// This mod includes:
/// - Proper game mode detection
/// - Authority checking for network entities
/// - Safe hook points and patterns
/// - Error handling
/// - Testing helpers
/// </summary>
public class MultiplayerSafeExampleMod : MelonMod
{
    private static bool _debugMode = false;
    private static float _lastModeCheck = 0f;
    
    // Example mod settings
    private static readonly float POWER_BONUS_PER_LEVEL = 5f;
    private static readonly float WEAPON_DAMAGE_MULTIPLIER = 1.2f;
    private static readonly float ENEMY_HEALTH_MULTIPLIER = 1.3f;

    public override void OnInitializeMelon()
    {
        MelonLogger.Msg("======================================================");
        MelonLogger.Msg("Multiplayer Safe Example Mod v1.0.0 Initialized");
        MelonLogger.Msg("======================================================");
        MelonLogger.Msg("This mod demonstrates safe multiplayer modding patterns:");
        MelonLogger.Msg("- Character stat bonuses (+5 power per level)");
        MelonLogger.Msg("- Weapon damage boost (+20%)");
        MelonLogger.Msg("- Enemy health scaling (+30%)");
        MelonLogger.Msg("- Custom UI indicators");
        MelonLogger.Msg("");
        MelonLogger.Msg("Debug Controls:");
        MelonLogger.Msg("  F1: Toggle debug mode");
        MelonLogger.Msg("  F2: Show game mode info");
        MelonLogger.Msg("  F3: Test authority checks");
        MelonLogger.Msg("  F4: Show player information");
        MelonLogger.Msg("======================================================");
    }

    public override void OnUpdate()
    {
        // Handle debug inputs
        if (Input.GetKeyDown(KeyCode.F1))
        {
            _debugMode = !_debugMode;
            MelonLogger.Msg($"Debug mode: {(_debugMode ? "ON" : "OFF")}");
        }

        if (Input.GetKeyDown(KeyCode.F2))
        {
            ShowGameModeInfo();
        }

        if (Input.GetKeyDown(KeyCode.F3))
        {
            TestAuthorityChecks();
        }

        if (Input.GetKeyDown(KeyCode.F4))
        {
            ShowPlayerInformation();
        }

        // Periodic debug info
        if (_debugMode && Time.time - _lastModeCheck > 10f)
        {
            _lastModeCheck = Time.time;
            LogCurrentGameState();
        }
    }

    #region Authority and Safety Helpers

    /// <summary>
    /// Check if we can safely modify a networked entity's state (health, stats, position).
    /// This checks HasStateAuthority - use HasInputAuthority for input control.
    /// </summary>
    public static bool CanModifyEntity(MonoBehaviour entity)
    {
        var gm = GM.Core;
        if (gm?.IsOnlineMultiplayer != true) return true; // Safe in non-online modes
        
        try
        {
            var sync = entity?.GetComponent<CoherenceSync>();
            return sync?.HasStateAuthority == true; // Use HasStateAuthority for state modifications
        }
        catch (Exception ex)
        {
            MelonLogger.Error($"Authority check failed: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// Check if local client is the host
    /// </summary>
    public static bool IsHost()
    {
        var gm = GM.Core;
        if (gm?.IsOnlineMultiplayer != true) return true; // Always "host" in non-online
        
        try
        {
            var players = gm._multiplayerManager?.GetPlayers();
            return players?.Count > 0 && IsLocalPlayer(players[0]);
        }
        catch (Exception ex)
        {
            MelonLogger.Error($"Host detection failed: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// Check if a player is the local player
    /// </summary>
    public static bool IsLocalPlayer(Player player)
    {
        try
        {
            return player?.isPlayer == true && player.id == 0;
        }
        catch (Exception ex)
        {
            MelonLogger.Error($"Local player check failed: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// Get all active players safely
    /// </summary>
    public static List<Player> GetAllPlayers()
    {
        var gm = GM.Core;
        if (gm?._multiplayerManager == null) return new List<Player>();
        
        try
        {
            var players = gm._multiplayerManager.GetPlayers();
            return players?.ToArray().ToList() ?? new List<Player>();
        }
        catch (Exception ex)
        {
            MelonLogger.Error($"Failed to get players: {ex.Message}");
            return new List<Player>();
        }
    }

    /// <summary>
    /// Get player's character safely
    /// </summary>
    public static CharacterController GetPlayerCharacter(Player player)
    {
        var gm = GM.Core;
        if (gm?._multiplayerManager == null || player == null) return null;
        
        try
        {
            return gm._multiplayerManager.GetCharacter(player);
        }
        catch (Exception ex)
        {
            MelonLogger.Error($"Failed to get character for player {player.id}: {ex.Message}");
            return null;
        }
    }

    #endregion

    #region Safe Hook Examples

    /// <summary>
    /// SAFE: Game initialization - perfect hook point for setup
    /// </summary>
    [HarmonyPatch(typeof(GameManager), "Awake")]
    [HarmonyPostfix]
    public static void OnGameStart(GameManager __instance)
    {
        try
        {
            MelonLogger.Msg("[SAFE HOOK] Game starting, initializing mod features...");
            
            // This is always safe - GM.Core is available, all systems initialized
            LogGameModeInfo(__instance);
            
            // Initialize mode-specific features
            InitializeModeSpecificFeatures(__instance);
        }
        catch (Exception ex)
        {
            MelonLogger.Error($"Game start hook failed: {ex.Message}");
        }
    }

    /// <summary>
    /// SAFE: Data modification - affects all players equally
    /// </summary>
    [HarmonyPatch(typeof(DataManager), "ReloadAllData")]
    [HarmonyPostfix]
    public static void OnDataReload(DataManager __instance)
    {
        try
        {
            MelonLogger.Msg("[SAFE HOOK] Modifying game data...");
            
            // Data modifications are always safe - they affect all players equally
            ModifyWeaponData(__instance);
            
            if (_debugMode)
            {
                MelonLogger.Msg("Applied weapon damage boost (+20%)");
            }
        }
        catch (Exception ex)
        {
            MelonLogger.Error($"Data modification failed: {ex.Message}");
        }
    }

    /// <summary>
    /// SAFE WITH CHECKS: Character level up - needs authority check in online mode
    /// </summary>
    [HarmonyPatch(typeof(CharacterController), "LevelUp")]
    [HarmonyPostfix]
    public static void OnCharacterLevelUp(CharacterController __instance)
    {
        try
        {
            var gm = GM.Core;
            if (gm == null) return; // Menu mode
            
            // Check authority in online mode
            if (gm.IsOnlineMultiplayer && !CanModifyEntity(__instance)) return;
            
            // Apply custom level up bonus
            ApplyCustomLevelUpBonus(__instance);
            
            if (_debugMode)
            {
                MelonLogger.Msg($"Applied level up bonus to {__instance.name}");
            }
        }
        catch (Exception ex)
        {
            MelonLogger.Error($"Level up hook failed: {ex.Message}");
        }
    }

    /// <summary>
    /// SAFE: UI modification - client-local, always safe
    /// </summary>
    [HarmonyPatch(typeof(MainGamePage), "UpdateExperienceProgress")]
    [HarmonyPostfix]
    public static void OnXPProgressUpdate(GameplaySignals.CharacterXpChangedSignal sig)
    {
        try
        {
            // UI modifications are client-local and always safe
            UpdateCustomXPDisplay(sig.CurrentXp, sig.MaxXp);
        }
        catch (Exception ex)
        {
            MelonLogger.Error($"UI update failed: {ex.Message}");
        }
    }

    /// <summary>
    /// HOST ONLY: Enemy initialization - only host should modify enemies in online mode
    /// </summary>
    [HarmonyPatch(typeof(EnemyController), "Initialize")]
    [HarmonyPostfix]
    public static void OnEnemyInitialize(EnemyController __instance)
    {
        try
        {
            var gm = GM.Core;
            if (gm == null) return;
            
            // In online mode, only host should modify enemies
            if (gm.IsOnlineMultiplayer && !IsHost()) return;
            
            // Additional authority check for the specific entity
            if (gm.IsOnlineMultiplayer && !CanModifyEntity(__instance)) return;
            
            // Apply enemy modifications
            ModifyEnemyStats(__instance);
            
            if (_debugMode)
            {
                MelonLogger.Msg($"Modified enemy stats: {__instance.name}");
            }
        }
        catch (Exception ex)
        {
            MelonLogger.Error($"Enemy initialization hook failed: {ex.Message}");
        }
    }

    /// <summary>
    /// SAFE: Menu modifications - always safe
    /// </summary>
    [HarmonyPatch(typeof(AppMainMenuState), "OnEnter")]
    [HarmonyPostfix]
    public static void OnMainMenu(AppMainMenuState __instance)
    {
        try
        {
            // Menu modifications are always safe
            MelonLogger.Msg("[SAFE HOOK] Main menu entered - mod active");
            
            // Could add custom menu elements here
            AddCustomMenuIndicator();
        }
        catch (Exception ex)
        {
            MelonLogger.Error($"Main menu hook failed: {ex.Message}");
        }
    }

    #endregion

    #region Modification Methods

    private static void InitializeModeSpecificFeatures(GameManager gameManager)
    {
        if (gameManager.IsOnlineMultiplayer)
        {
            MelonLogger.Msg("Initializing for ONLINE MULTIPLAYER mode");
            MelonLogger.Msg("- Authority checks enabled");
            MelonLogger.Msg("- Host-only enemy modifications");
            MelonLogger.Msg("- Network-aware features");
        }
        else if (gameManager.IsLocalMultiplayer)
        {
            MelonLogger.Msg("Initializing for LOCAL CO-OP mode");
            MelonLogger.Msg("- Multiple player support");
            MelonLogger.Msg("- Shared screen features");
            var playerCount = gameManager._multiplayerManager?.GetPlayerCount() ?? 1;
            MelonLogger.Msg($"- {playerCount} players detected");
        }
        else
        {
            MelonLogger.Msg("Initializing for SINGLE PLAYER mode");
            MelonLogger.Msg("- Full mod features enabled");
            MelonLogger.Msg("- No multiplayer restrictions");
        }
    }

    private static void ModifyWeaponData(DataManager dataManager)
    {
        try
        {
            var weapons = dataManager.GetConvertedWeapons();
            if (weapons == null) return;
            
            int modifiedWeapons = 0;
            
            foreach (var weaponType in weapons.Keys)
            {
                var weaponLevels = weapons[weaponType];
                if (weaponLevels == null) continue;
                
                foreach (var level in weaponLevels)
                {
                    if (level != null)
                    {
                        level.power *= WEAPON_DAMAGE_MULTIPLIER;
                        modifiedWeapons++;
                    }
                }
            }
            
            if (_debugMode)
            {
                MelonLogger.Msg($"Modified {modifiedWeapons} weapon levels (+{(WEAPON_DAMAGE_MULTIPLIER - 1) * 100:F0}% damage)");
            }
        }
        catch (Exception ex)
        {
            MelonLogger.Error($"Weapon data modification failed: {ex.Message}");
        }
    }

    private static void ApplyCustomLevelUpBonus(CharacterController character)
    {
        if (character?.PlayerStats == null) return;
        
        try
        {
            var stats = character.PlayerStats.GetAllPowerUps();
            if (stats?.ContainsKey(PowerUpType.POWER) == true)
            {
                var powerStat = stats[PowerUpType.POWER];
                var currentPower = powerStat.GetValue();
                powerStat.SetValue(currentPower + POWER_BONUS_PER_LEVEL);
                
                if (_debugMode)
                {
                    MelonLogger.Msg($"Boosted {character.name} power: {currentPower:F1} -> {currentPower + POWER_BONUS_PER_LEVEL:F1}");
                }
            }
        }
        catch (Exception ex)
        {
            MelonLogger.Error($"Level up bonus failed: {ex.Message}");
        }
    }

    private static void ModifyEnemyStats(EnemyController enemy)
    {
        if (enemy == null) return;
        
        try
        {
            // Scale enemy health
            var originalHealth = enemy.health;
            enemy.health *= ENEMY_HEALTH_MULTIPLIER;
            enemy.maxHealth *= ENEMY_HEALTH_MULTIPLIER;
            
            if (_debugMode)
            {
                MelonLogger.Msg($"Enemy {enemy.name} health: {originalHealth:F0} -> {enemy.health:F0}");
            }
        }
        catch (Exception ex)
        {
            MelonLogger.Error($"Enemy modification failed: {ex.Message}");
        }
    }

    private static void UpdateCustomXPDisplay(double currentXp, double maxXp)
    {
        // Custom XP display logic would go here
        // This is just a placeholder showing the pattern
        
        if (_debugMode && maxXp > 0)
        {
            var percentage = (currentXp / maxXp) * 100;
            if (percentage % 25 < 1) // Log every 25%
            {
                MelonLogger.Msg($"XP Progress: {currentXp:F0}/{maxXp:F0} ({percentage:F0}%)");
            }
        }
    }

    private static void AddCustomMenuIndicator()
    {
        // Custom menu modification logic would go here
        if (_debugMode)
        {
            MelonLogger.Msg("Custom menu indicator added");
        }
    }

    #endregion

    #region Debug and Testing Methods

    private static void ShowGameModeInfo()
    {
        var gm = GM.Core;
        
        MelonLogger.Msg("=== GAME MODE INFORMATION ===");
        
        if (gm == null)
        {
            MelonLogger.Msg("GM.Core is null - currently in menu");
            return;
        }
        
        MelonLogger.Msg($"Online Multiplayer: {gm.IsOnlineMultiplayer}");
        MelonLogger.Msg($"Local Multiplayer: {gm.IsLocalMultiplayer}");
        MelonLogger.Msg($"Any Multiplayer: {gm._multiplayerManager?.IsMultiplayer}");
        MelonLogger.Msg($"Player Count: {gm._multiplayerManager?.GetPlayerCount() ?? 0}");
        
        if (gm.IsOnlineMultiplayer)
        {
            MelonLogger.Msg($"Host Status: {IsHost()}");
        }
        
        MelonLogger.Msg("============================");
    }

    private static void TestAuthorityChecks()
    {
        var gm = GM.Core;
        if (gm == null)
        {
            MelonLogger.Msg("Cannot test authority - GM.Core is null");
            return;
        }
        
        MelonLogger.Msg("=== AUTHORITY CHECK TEST ===");
        
        var players = GetAllPlayers();
        MelonLogger.Msg($"Found {players.Count} players");
        
        foreach (var player in players)
        {
            var character = GetPlayerCharacter(player);
            if (character != null)
            {
                var canModify = CanModifyEntity(character);
                var isLocal = IsLocalPlayer(player);
                
                MelonLogger.Msg($"Player {player.id}:");
                MelonLogger.Msg($"  Is Local: {isLocal}");
                MelonLogger.Msg($"  Can Modify: {canModify}");
                MelonLogger.Msg($"  Character: {character.name}");
            }
        }
        
        MelonLogger.Msg("===========================");
    }

    private static void ShowPlayerInformation()
    {
        var gm = GM.Core;
        if (gm == null)
        {
            MelonLogger.Msg("Cannot show player info - GM.Core is null");
            return;
        }
        
        MelonLogger.Msg("=== PLAYER INFORMATION ===");
        
        var players = GetAllPlayers();
        MelonLogger.Msg($"Total Players: {players.Count}");
        
        for (int i = 0; i < players.Count; i++)
        {
            var player = players[i];
            var character = GetPlayerCharacter(player);
            
            MelonLogger.Msg($"Player {i + 1}:");
            MelonLogger.Msg($"  ID: {player.id}");
            MelonLogger.Msg($"  Is Player: {player.isPlayer}");
            MelonLogger.Msg($"  Is Local: {IsLocalPlayer(player)}");
            
            if (character != null)
            {
                MelonLogger.Msg($"  Character: {character.name}");
                MelonLogger.Msg($"  Level: {character.level}");
                MelonLogger.Msg($"  Health: {character.health:F0}/{character.maxHealth:F0}");
                
                var stats = character.PlayerStats?.GetAllPowerUps();
                if (stats?.ContainsKey(PowerUpType.POWER) == true)
                {
                    var power = stats[PowerUpType.POWER].GetValue();
                    MelonLogger.Msg($"  Power: {power:F1}");
                }
            }
            else
            {
                MelonLogger.Msg($"  Character: None");
            }
        }
        
        MelonLogger.Msg("=========================");
    }

    private static void LogGameModeInfo(GameManager gameManager)
    {
        MelonLogger.Msg($"Game Mode Detected:");
        MelonLogger.Msg($"  Online: {gameManager.IsOnlineMultiplayer}");
        MelonLogger.Msg($"  Local: {gameManager.IsLocalMultiplayer}");
        MelonLogger.Msg($"  Multi: {gameManager._multiplayerManager?.IsMultiplayer}");
    }

    private static void LogCurrentGameState()
    {
        var gm = GM.Core;
        if (gm == null) return;
        
        var players = GetAllPlayers();
        MelonLogger.Msg($"[DEBUG] Current state - Mode: {GetGameModeString(gm)}, Players: {players.Count}");
        
        if (gm.IsOnlineMultiplayer)
        {
            MelonLogger.Msg($"[DEBUG] Host: {IsHost()}");
        }
    }

    private static string GetGameModeString(GameManager gm)
    {
        if (gm.IsOnlineMultiplayer) return "Online";
        if (gm.IsLocalMultiplayer) return "Local Co-op";
        return "Single Player";
    }

    #endregion

    #region Testing Patterns for Different Scenarios

    /// <summary>
    /// Example: Process all players safely
    /// </summary>
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
            MelonLogger.Error($"Player processing failed: {ex.Message}");
        }
    }

    /// <summary>
    /// Example: Modify only owned entities
    /// </summary>
    public static void ModifyOwnedEntitiesOnly(Action<CharacterController> modification)
    {
        ProcessAllPlayers((player, character) =>
        {
            if (CanModifyEntity(character))
            {
                try
                {
                    modification(character);
                }
                catch (Exception ex)
                {
                    MelonLogger.Error($"Entity modification failed: {ex.Message}");
                }
            }
        });
    }

    /// <summary>
    /// Example: Host-only game state modification
    /// </summary>
    public static void ModifyGameStateHostOnly(Action action)
    {
        var gm = GM.Core;
        if (gm == null) return;
        
        if (gm.IsOnlineMultiplayer && !IsHost()) return;
        
        try
        {
            action();
        }
        catch (Exception ex)
        {
            MelonLogger.Error($"Game state modification failed: {ex.Message}");
        }
    }

    #endregion
}