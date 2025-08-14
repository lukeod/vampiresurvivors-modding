using MelonLoader;
using HarmonyLib;
using Il2CppVampireSurvivors.Framework;
using Il2CppVampireSurvivors.Data;
using System;
using System.Diagnostics;
using UnityEngine;

[assembly: MelonInfo(typeof(TracerMod), "VS Loading Tracer", "2.0.0", "VS Modding Community")]
[assembly: MelonGame("poncle", "Vampire Survivors")]

/// <summary>
/// TracerMod - A diagnostic tool for understanding Vampire Survivors' loading sequence.
/// 
/// This mod demonstrates:
/// - Manual Harmony patching (recommended for IL2CPP)
/// - Monitoring GM.Core availability
/// - Tracking multiple ReloadAllData calls
/// - Using OnUpdate for state monitoring
/// - Input handling with Unity's Input system
/// </summary>
public class TracerMod : MelonMod
{
    // Tracking variables
    private static int updateFrameCount = 0;
    private static int gameManagerAwakeCount = 0;
    private static int dataReloadCount = 0;
    private static bool hasLoggedGameManager = false;
    private static readonly Stopwatch timer = new Stopwatch();
    
    /// <summary>
    /// Called when the mod is initialized by MelonLoader.
    /// This is where we set up our hooks using manual patching.
    /// </summary>
    public override void OnInitializeMelon()
    {
        timer.Start();
        MelonLogger.Msg("=== TracerMod Initialized ===");
        MelonLogger.Msg("Press F1 in-game to dump current state");
        
        // Use manual patching instead of attributes (more reliable with IL2CPP)
        InstallHooks();
    }
    
    /// <summary>
    /// Installs Harmony hooks using manual patching.
    /// This approach is more reliable than attribute-based patching for IL2CPP games.
    /// </summary>
    private void InstallHooks()
    {
        try
        {
            var harmony = new HarmonyLib.Harmony("TracerMod");
            
            // Hook DataManager.ReloadAllData
            var dataManagerType = typeof(DataManager);
            if (dataManagerType != null)
            {
                var reloadMethod = dataManagerType.GetMethod("ReloadAllData");
                if (reloadMethod != null)
                {
                    harmony.Patch(reloadMethod, 
                        postfix: new HarmonyMethod(typeof(TracerMod).GetMethod(nameof(OnReloadAllData))));
                    MelonLogger.Msg("Successfully hooked DataManager.ReloadAllData");
                }
                else
                {
                    MelonLogger.Warning("DataManager.ReloadAllData method not found");
                }
            }
            
            // Hook GameManager.Awake
            var gameManagerType = typeof(GameManager);
            if (gameManagerType != null)
            {
                var awakeMethod = gameManagerType.GetMethod("Awake");
                if (awakeMethod != null)
                {
                    harmony.Patch(awakeMethod, 
                        postfix: new HarmonyMethod(typeof(TracerMod).GetMethod(nameof(OnGameManagerAwake))));
                    MelonLogger.Msg("Successfully hooked GameManager.Awake");
                }
                else
                {
                    MelonLogger.Warning("GameManager.Awake method not found");
                }
            }
            
            MelonLogger.Msg("Hook installation complete");
        }
        catch (Exception ex)
        {
            MelonLogger.Error($"Failed to install hooks: {ex.Message}");
            MelonLogger.Error(ex.StackTrace);
        }
    }
    
    /// <summary>
    /// Called every frame by MelonLoader.
    /// Used to monitor GM.Core availability and handle input.
    /// </summary>
    public override void OnUpdate()
    {
        updateFrameCount++;
        
        // Check GM.Core availability every second (60 frames)
        if (updateFrameCount % 60 == 0)
        {
            CheckGameManagerAvailability();
        }
        
        // Handle debug key input
        if (Input.GetKeyDown(KeyCode.F1))
        {
            DumpCurrentState();
        }
    }
    
    /// <summary>
    /// Checks if GM.Core has become available and logs relevant information.
    /// This demonstrates the recommended pattern for detecting when the game session starts.
    /// </summary>
    private void CheckGameManagerAvailability()
    {
        var gameManager = GM.Core;
        if (gameManager != null && !hasLoggedGameManager)
        {
            MelonLogger.Msg($"[{timer.ElapsedMilliseconds}ms] GM.Core is now available!");
            hasLoggedGameManager = true;
            
            // Log what's available
            if (gameManager.DataManager != null)
            {
                MelonLogger.Msg("  - DataManager is available");
                
                var weapons = gameManager.DataManager.GetConvertedWeapons();
                if (weapons != null)
                {
                    MelonLogger.Msg($"  - Weapon data loaded: {weapons.Count} weapons");
                }
                
                var characters = gameManager.DataManager.GetConvertedCharacterData();
                if (characters != null)
                {
                    MelonLogger.Msg($"  - Character data loaded: {characters.Count} characters");
                }
            }
            
            if (gameManager.Player != null)
            {
                MelonLogger.Msg($"  - Player is available: {gameManager.Player._characterType}");
            }
            
            if (gameManager._stage != null)
            {
                MelonLogger.Msg("  - Stage is available");
            }
        }
    }
    
    /// <summary>
    /// Hook for DataManager.ReloadAllData.
    /// This is called multiple times during startup (once for base game, once per DLC).
    /// </summary>
    public static void OnReloadAllData(DataManager __instance)
    {
        dataReloadCount++;
        MelonLogger.Msg($"[{timer.ElapsedMilliseconds}ms] DataManager.ReloadAllData called #{dataReloadCount}");
        
        if (__instance != null)
        {
            // Check what data is available
            if (__instance._allWeaponDataJson != null)
            {
                MelonLogger.Msg("  - Weapon JSON data is loaded");
            }
            
            if (__instance._allCharactersJson != null)
            {
                MelonLogger.Msg("  - Character JSON data is loaded");
            }
            
            // Check for DLC data
            if (__instance._dlcWeaponData != null && __instance._dlcWeaponData.Count > 0)
            {
                MelonLogger.Msg($"  - DLC weapon data present: {__instance._dlcWeaponData.Count} DLCs");
            }
        }
    }
    
    /// <summary>
    /// Hook for GameManager.Awake.
    /// This is called when entering gameplay (not at app startup).
    /// </summary>
    public static void OnGameManagerAwake(GameManager __instance)
    {
        gameManagerAwakeCount++;
        MelonLogger.Msg($"[{timer.ElapsedMilliseconds}ms] GameManager.Awake called #{gameManagerAwakeCount}");
        
        if (__instance != null)
        {
            MelonLogger.Msg($"  - Instance type: {__instance.GetType().FullName}");
        }
        
        if (GM.Core != null)
        {
            MelonLogger.Msg("  - GM.Core is already set");
        }
        else
        {
            MelonLogger.Msg("  - GM.Core is still null (will be set soon)");
        }
    }
    
    /// <summary>
    /// Dumps the current game state to the console.
    /// Triggered by pressing F1 during gameplay.
    /// </summary>
    private void DumpCurrentState()
    {
        MelonLogger.Msg("=== Current Game State (F1) ===");
        MelonLogger.Msg($"Time since mod init: {timer.ElapsedMilliseconds}ms");
        MelonLogger.Msg($"Update frames: {updateFrameCount}");
        MelonLogger.Msg($"ReloadAllData calls: {dataReloadCount}");
        MelonLogger.Msg($"GameManager.Awake calls: {gameManagerAwakeCount}");
        
        var gm = GM.Core;
        if (gm != null)
        {
            MelonLogger.Msg("\nGM.Core: Available");
            MelonLogger.Msg($"  - DataManager: {(gm.DataManager != null ? "Yes" : "No")}");
            MelonLogger.Msg($"  - Player: {(gm.Player != null ? "Yes" : "No")}");
            MelonLogger.Msg($"  - Stage: {(gm._stage != null ? "Yes" : "No")}");
            MelonLogger.Msg($"  - MainUI: {(gm.MainUI != null ? "Yes" : "No")}");
            
            if (gm.Player != null)
            {
                MelonLogger.Msg($"\nPlayer Details:");
                MelonLogger.Msg($"  - Character: {gm.Player._characterType}");
                MelonLogger.Msg($"  - Level: {gm.Player._level}");
                MelonLogger.Msg($"  - HP: {gm.Player._currentHp:F0}");
                MelonLogger.Msg($"  - XP: {gm.Player._xp:F0}");
                
                if (gm.Player.PlayerStats != null)
                {
                    MelonLogger.Msg($"  - Power: {gm.Player.PlayerStats.Power.GetValue():F1}");
                    MelonLogger.Msg($"  - MoveSpeed: {gm.Player.PlayerStats.MoveSpeed.GetValue():F1}");
                }
            }
            
            if (gm._stage != null)
            {
                MelonLogger.Msg($"\nStage Details:");
                if (gm._stage._stageData != null)
                {
                    MelonLogger.Msg($"  - Stage Data Available");
                }
                MelonLogger.Msg($"  - Enemies Spawned: {gm._stage._spawnedEnemies?.Count ?? 0}");
            }
        }
        else
        {
            MelonLogger.Msg("\nGM.Core: NULL (Not in game session)");
        }
        
        MelonLogger.Msg("=== End State Dump ===");
    }
}