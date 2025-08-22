using MelonLoader;
using HarmonyLib;
using Il2CppVampireSurvivors.Framework;
using Il2CppVampireSurvivors.Framework.TimerSystem;
using Il2CppVampireSurvivors.Tools;
using Il2CppVampireSurvivors.Objects;
using Il2CppVampireSurvivors.UI;
using System;
using System.Diagnostics;
using UnityEngine;
using Il2CppTMPro;

[assembly: MelonInfo(typeof(TimerSystemMod), "Timer System Monitor", "1.0.0", "VS Modding Community")]
[assembly: MelonGame("poncle", "Vampire Survivors")]

public class TimerSystemMod : MelonMod
{
    private static float currentGameTime = 0f;
    private static float lastLoggedMinute = -1f;
    private static bool isGamePaused = false;
    private static bool hasStartedGame = false;
    private static readonly Stopwatch debugTimer = new Stopwatch();
    
    private static int pauseCount = 0;
    private static int resumeCount = 0;
    
    public override void OnInitializeMelon()
    {
        debugTimer.Start();
        MelonLogger.Msg("=== Timer System Monitor Initialized ===");
        MelonLogger.Msg("This mod tracks the accurate, pause-aware game timer");
        MelonLogger.Msg("Press F2 to display current timer statistics");
        MelonLogger.Msg("Press F3 to test timer accuracy against UI");
        
        InstallHooks();
    }
    
    private void InstallHooks()
    {
        try
        {
            var harmony = new HarmonyLib.Harmony("TimerSystemMod");
            
            var vsUtilsType = typeof(VSUtils);
            if (vsUtilsType != null)
            {
                var formatTimeMethod = vsUtilsType.GetMethod("FormatTime");
                if (formatTimeMethod != null)
                {
                    harmony.Patch(formatTimeMethod,
                        prefix: new HarmonyMethod(typeof(TimerSystemMod).GetMethod(nameof(OnFormatTime))));
                    MelonLogger.Msg("Successfully hooked VSUtils.FormatTime");
                }
                else
                {
                    MelonLogger.Warning("VSUtils.FormatTime method not found");
                }
            }
            
            var timerManagerGameType = typeof(TimerManagerGame);
            if (timerManagerGameType != null)
            {
                var onPauseMethod = timerManagerGameType.GetMethod("OnPause");
                if (onPauseMethod != null)
                {
                    harmony.Patch(onPauseMethod,
                        postfix: new HarmonyMethod(typeof(TimerSystemMod).GetMethod(nameof(OnGamePause))));
                    MelonLogger.Msg("Successfully hooked TimerManagerGame.OnPause");
                }
                
                var onResumeMethod = timerManagerGameType.GetMethod("OnResume");
                if (onResumeMethod != null)
                {
                    harmony.Patch(onResumeMethod,
                        postfix: new HarmonyMethod(typeof(TimerSystemMod).GetMethod(nameof(OnGameResume))));
                    MelonLogger.Msg("Successfully hooked TimerManagerGame.OnResume");
                }
            }
            
            var gameManagerType = typeof(GameManager);
            if (gameManagerType != null)
            {
                var awakeMethod = gameManagerType.GetMethod("Awake");
                if (awakeMethod != null)
                {
                    harmony.Patch(awakeMethod,
                        postfix: new HarmonyMethod(typeof(TimerSystemMod).GetMethod(nameof(OnGameManagerAwake))));
                    MelonLogger.Msg("Successfully hooked GameManager.Awake");
                }
            }
            
            var stageType = typeof(Stage);
            if (stageType != null)
            {
                var startTimersMethod = stageType.GetMethod("StartTimers");
                if (startTimersMethod != null)
                {
                    harmony.Patch(startTimersMethod,
                        postfix: new HarmonyMethod(typeof(TimerSystemMod).GetMethod(nameof(OnStageStartTimers))));
                    MelonLogger.Msg("Successfully hooked Stage.StartTimers");
                }
                
                var cancelTimersMethod = stageType.GetMethod("CancelTimers");
                if (cancelTimersMethod != null)
                {
                    harmony.Patch(cancelTimersMethod,
                        prefix: new HarmonyMethod(typeof(TimerSystemMod).GetMethod(nameof(OnStageCancelTimers))));
                    MelonLogger.Msg("Successfully hooked Stage.CancelTimers");
                }
            }
            
            MelonLogger.Msg("Timer System hooks installation complete");
        }
        catch (Exception ex)
        {
            MelonLogger.Error($"Failed to install timer hooks: {ex.Message}");
            MelonLogger.Error(ex.StackTrace);
        }
    }
    
    public static bool OnFormatTime(float seconds, ref string __result)
    {
        // Log first few calls to debug
        if (debugTimer.ElapsedMilliseconds < 30000) // First 30 seconds
        {
            MelonLogger.Msg($"[Debug] FormatTime called with: {seconds:F2}s");
        }
        
        // Only update if this is a reasonable game time value (not UI element times)
        if (seconds >= 0 && seconds < 7200) // Less than 2 hours
        {
            currentGameTime = seconds;
            
            float currentMinute = Mathf.Floor(seconds / 60f);
            if (currentMinute > lastLoggedMinute && hasStartedGame)
            {
                lastLoggedMinute = currentMinute;
                int mins = (int)currentMinute;
                int secs = (int)(seconds % 60f);
                MelonLogger.Msg($"[Timer] Game Time: {mins:00}:{secs:00} ({seconds:F2}s total)");
            }
        }
        
        // Let the original method run
        return true;
    }
    
    public static void OnGamePause(TimerManagerGame __instance)
    {
        if (!isGamePaused)
        {
            isGamePaused = true;
            pauseCount++;
            MelonLogger.Msg($"[Timer] Game PAUSED at {FormatTime(currentGameTime)} (Pause #{pauseCount})");
        }
    }
    
    public static void OnGameResume(TimerManagerGame __instance)
    {
        if (isGamePaused)
        {
            isGamePaused = false;
            resumeCount++;
            MelonLogger.Msg($"[Timer] Game RESUMED at {FormatTime(currentGameTime)} (Resume #{resumeCount})");
        }
    }
    
    public static void OnGameManagerAwake(GameManager __instance)
    {
        MelonLogger.Msg("[Timer] GameManager.Awake - New game session starting");
        ResetTimerStats();
    }
    
    public static void OnStageStartTimers(Stage __instance)
    {
        hasStartedGame = true;
        MelonLogger.Msg("[Timer] Stage timers started - Game timer is now active");
        
        if (__instance != null)
        {
            MelonLogger.Msg($"  - Current Minute: {__instance._currentMinute}");
            
            if (__instance._spawnTimer != null)
            {
                try
                {
                    float spawnTime = __instance._spawnTimer.GetTimeElapsed();
                    MelonLogger.Msg($"  - Spawn Timer: {spawnTime:F2}s");
                }
                catch { }
            }
        }
    }
    
    public static void OnStageCancelTimers(Stage __instance)
    {
        if (hasStartedGame)
        {
            MelonLogger.Msg($"[Timer] Stage timers cancelled - Final time: {FormatTime(currentGameTime)}");
            MelonLogger.Msg($"  - Total pauses: {pauseCount}");
            hasStartedGame = false;
        }
    }
    
    public override void OnUpdate()
    {
        // Try to capture game time from UI directly
        var gm = GM.Core;
        if (gm?.MainUI?._TimeText != null)
        {
            try
            {
                string timeText = gm.MainUI._TimeText.text;
                if (!string.IsNullOrEmpty(timeText) && timeText.Contains(":"))
                {
                    string[] parts = timeText.Split(':');
                    if (parts.Length == 2)
                    {
                        if (int.TryParse(parts[0], out int minutes) && int.TryParse(parts[1], out int seconds))
                        {
                            float newTime = minutes * 60f + seconds;
                            
                            // Log when time changes to a new minute
                            if (newTime > currentGameTime && hasStartedGame)
                            {
                                float currentMinute = Mathf.Floor(newTime / 60f);
                                if (currentMinute > lastLoggedMinute)
                                {
                                    lastLoggedMinute = currentMinute;
                                    MelonLogger.Msg($"[Timer] Game Time: {FormatTime(newTime)} ({newTime:F2}s total)");
                                }
                            }
                            
                            currentGameTime = newTime;
                        }
                    }
                }
            }
            catch { }
        }
        
        if (Input.GetKeyDown(KeyCode.F2))
        {
            DisplayTimerStatistics();
        }
        
        if (Input.GetKeyDown(KeyCode.F3))
        {
            TestTimerAccuracy();
        }
    }
    
    private void DisplayTimerStatistics()
    {
        MelonLogger.Msg("=== Timer System Statistics (F2) ===");
        MelonLogger.Msg($"Current Game Time: {FormatTime(currentGameTime)} ({currentGameTime:F2}s)");
        MelonLogger.Msg($"Game Active: {hasStartedGame}");
        MelonLogger.Msg($"Currently Paused: {isGamePaused}");
        MelonLogger.Msg($"Pause Count: {pauseCount}");
        MelonLogger.Msg($"Resume Count: {resumeCount}");
        
        var gm = GM.Core;
        if (gm != null)
        {
            MelonLogger.Msg("\nGame State:");
            MelonLogger.Msg($"  - GM.Core: Available");
            
            if (gm._stage != null)
            {
                MelonLogger.Msg($"  - Stage: Active");
                MelonLogger.Msg($"  - Stage Current Minute: {gm._stage._currentMinute}");
                
                // Check all available timers
                try
                {
                    if (gm._stage._spawnTimer != null)
                    {
                        float spawnElapsed = gm._stage._spawnTimer.GetTimeElapsed();
                        MelonLogger.Msg($"  - Spawn Timer: {spawnElapsed:F2}s (Paused: {gm._stage._spawnTimer.IsPaused})");
                    }
                    
                    if (gm._stage._pauseTimer != null)
                    {
                        float pauseElapsed = gm._stage._pauseTimer.GetTimeElapsed();
                        MelonLogger.Msg($"  - Pause Timer: {pauseElapsed:F2}s (Paused: {gm._stage._pauseTimer.IsPaused})");
                    }
                    
                    if (gm._stage._destructibleTimer != null)
                    {
                        float destElapsed = gm._stage._destructibleTimer.GetTimeElapsed();
                        MelonLogger.Msg($"  - Destructible Timer: {destElapsed:F2}s");
                    }
                    
                    if (gm._stage._checkPizzasTimer != null)
                    {
                        float pizzaElapsed = gm._stage._checkPizzasTimer.GetTimeElapsed();
                        MelonLogger.Msg($"  - Pizza Timer: {pizzaElapsed:F2}s");
                    }
                    
                    if (gm._stage._noShadowsTimer != null)
                    {
                        float shadowElapsed = gm._stage._noShadowsTimer.GetTimeElapsed();
                        MelonLogger.Msg($"  - Shadow Timer: {shadowElapsed:F2}s");
                    }
                }
                catch (Exception ex)
                {
                    MelonLogger.Msg($"  - Error reading timers: {ex.Message}");
                }
            }
            else
            {
                MelonLogger.Msg($"  - Stage: Not Active");
            }
        }
        else
        {
            MelonLogger.Msg("\nGame State: Not in game session");
        }
        
        MelonLogger.Msg("=== End Statistics ===");
    }
    
    private void TestTimerAccuracy()
    {
        MelonLogger.Msg("=== Timer Accuracy Test (F3) ===");
        
        var gm = GM.Core;
        if (gm?.MainUI != null)
        {
            try
            {
                var mainGamePage = gm.MainUI;
                if (mainGamePage != null && mainGamePage._TimeText != null)
                {
                    string uiTimeText = mainGamePage._TimeText.text;
                    MelonLogger.Msg($"UI Display Time: {uiTimeText}");
                    MelonLogger.Msg($"Captured Time: {FormatTime(currentGameTime)}");
                    MelonLogger.Msg($"Raw Seconds: {currentGameTime:F3}");
                    
                    if (uiTimeText == FormatTime(currentGameTime))
                    {
                        MelonLogger.Msg("✓ Timer is ACCURATE - Matches UI exactly!");
                    }
                    else
                    {
                        MelonLogger.Msg("⚠ Timer mismatch - May be due to update timing");
                    }
                }
                else
                {
                    MelonLogger.Msg("MainGamePage or TimeText not available");
                }
            }
            catch (Exception ex)
            {
                MelonLogger.Msg($"Error accessing UI timer: {ex.Message}");
            }
        }
        else
        {
            MelonLogger.Msg("Not in active game session - cannot test accuracy");
        }
        
        MelonLogger.Msg($"\nUnity Time.time: {Time.time:F2}");
        MelonLogger.Msg($"Unity Time.realtimeSinceStartup: {Time.realtimeSinceStartup:F2}");
        MelonLogger.Msg($"Unity Time.timeScale: {Time.timeScale}");
        
        MelonLogger.Msg("=== End Accuracy Test ===");
    }
    
    private static void ResetTimerStats()
    {
        currentGameTime = 0f;
        lastLoggedMinute = -1f;
        isGamePaused = false;
        pauseCount = 0;
        resumeCount = 0;
    }
    
    private static string FormatTime(float seconds)
    {
        int minutes = (int)(seconds / 60f);
        int secs = (int)(seconds % 60f);
        return $"{minutes:00}:{secs:00}";
    }
    
    public static float GetGameSeconds() => currentGameTime;
    public static bool IsPaused() => isGamePaused;
    public static string GetFormattedTime() => FormatTime(currentGameTime);
    public static int GetPauseCount() => pauseCount;
}