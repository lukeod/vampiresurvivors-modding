# Timers

The pause-aware timer system that tracks game time accurately through weapon selections, merchant interactions, and other pause states.

**Core Namespace**: `Il2CppVampireSurvivors.Framework.TimerSystem`  
**Formatting Utility**: `Il2CppVampireSurvivors.Tools.VSUtils`

## Overview

Vampire Survivors uses a sophisticated timer framework that properly handles pause states rather than simple time accumulation. The system ensures timer accuracy when players open weapon selection, interact with merchants, access healers, or trigger other pause events. Game time is tracked through `Timer` objects managed by specialized `TimerManager` classes.

## Quick Reference

### Key Classes
```csharp
// Core timer classes
Il2CppVampireSurvivors.Framework.TimerSystem.Timer         // Individual timer instance
Il2CppVampireSurvivors.Framework.TimerSystem.TimerManager  // Base manager class
Il2CppVampireSurvivors.Framework.TimerSystem.TimerManagerGame  // Game timers (pause-aware)
Il2CppVampireSurvivors.Framework.TimerSystem.TimerManagerUI    // UI timers (ignore pause)
Il2CppVampireSurvivors.Framework.TimerSystem.Timers        // Global timer registry

// Utility functions
Il2CppVampireSurvivors.Tools.VSUtils.FormatTime           // Formats seconds to MM:SS
```

### Stage Timer Properties
```csharp
public class Stage
{
    public int _currentMinute;           // Current minute (integer only)
    public Timer _pauseTimer;           // Handles pause functionality
    public Timer _spawnTimer;           // Enemy spawn timing
    public Timer _destructibleTimer;    // Destructible spawn timing
    public Timer _checkPizzasTimer;     // Pizza check timing
    public Timer _noShadowsTimer;       // Shadow visibility timing
}
```

## Accessing Game Time

### Method 1: Hook VSUtils.FormatTime (Recommended)

The most reliable approach hooks the formatting function that receives accurate, pause-aware game time.

```csharp
[HarmonyPatch(typeof(VSUtils), "FormatTime")]
[HarmonyPrefix]
public static bool CaptureGameTime(float seconds, ref string __result)
{
    // 'seconds' contains the exact pause-aware game time
    currentGameTime = seconds;
    
    // Return true to run original method
    return true;
}
```

**Advantages**:
- Receives already-calculated pause-aware time
- Single float parameter contains exact game seconds
- No pause state management required
- Matches UI display precisely

### Method 2: Read UI Timer Text

Access the displayed timer text directly from the UI component.

```csharp
public static float GetTimeFromUI()
{
    var gm = GM.Core;
    if (gm?.MainUI?._TimeText != null)
    {
        string timeText = gm.MainUI._TimeText.text;
        if (!string.IsNullOrEmpty(timeText) && timeText.Contains(":"))
        {
            string[] parts = timeText.Split(':');
            if (parts.Length == 2 && 
                int.TryParse(parts[0], out int minutes) && 
                int.TryParse(parts[1], out int seconds))
            {
                return minutes * 60f + seconds;
            }
        }
    }
    return 0f;
}
```

### Method 3: Access Stage Timers

Query timer objects directly from the Stage instance.

```csharp
public static float GetStageSpawnTime()
{
    var stage = GM.Core?._stage;
    if (stage?._spawnTimer != null)
    {
        return stage._spawnTimer.GetTimeElapsed();
    }
    return 0f;
}
```

## Pause State Management

### Detecting Pause Events

Hook the timer manager to track pause state changes.

```csharp
[HarmonyPatch(typeof(TimerManagerGame), "OnPause")]
[HarmonyPostfix]
public static void OnGamePause(TimerManagerGame __instance)
{
    isGamePaused = true;
    LogMessage($"Game paused at {currentGameTime:F2} seconds");
}

[HarmonyPatch(typeof(TimerManagerGame), "OnResume")]
[HarmonyPostfix]
public static void OnGameResume(TimerManagerGame __instance)
{
    isGamePaused = false;
    LogMessage($"Game resumed at {currentGameTime:F2} seconds");
}
```

### Pause Triggers

The game pauses automatically during:
- Weapon/item selection (level up)
- Merchant interactions
- Healer interactions
- Game menu access
- Certain scripted events

## Timer Internal Structure

### Timer State Fields

```csharp
public class Timer
{
    float _startTime;                          // When timer started
    float _Duration_k__BackingField;          // Timer duration
    Nullable<float> _timeElapsedBeforePause;  // Stores time when paused
    Nullable<float> _timeElapsedBeforeCancel; // Stores time when cancelled
    bool _UsesRealTime_k__BackingField;       // Real-time vs game-time flag
}
```

### State Machine Logic

```csharp
// Timer.GetTimeElapsed implementation pattern
public float GetTimeElapsed()
{
    if (IsCompleted())
        return _Duration_k__BackingField;
    
    if (_timeElapsedBeforeCancel.HasValue)
        return _timeElapsedBeforeCancel.Value;
    
    if (_timeElapsedBeforePause.HasValue)
        return _timeElapsedBeforePause.Value;  // Frozen during pause
    
    return GetWorldTime() - _startTime;        // Normal running
}
```

## Complete Implementation Example

```csharp
using MelonLoader;
using HarmonyLib;
using Il2CppVampireSurvivors.Framework.TimerSystem;
using Il2CppVampireSurvivors.Tools;
using Il2CppVampireSurvivors.Objects;
using UnityEngine;

[assembly: MelonInfo(typeof(AccurateTimerMod), "Accurate Timer", "1.0.0", "Author")]
[assembly: MelonGame("poncle", "Vampire Survivors")]

public class AccurateTimerMod : MelonMod
{
    private static float currentGameTime = 0f;
    private static bool isGamePaused = false;
    private static int pauseCount = 0;
    
    public override void OnInitializeMelon()
    {
        var harmony = new HarmonyLib.Harmony("AccurateTimerMod");
        
        // Hook timer formatting
        harmony.Patch(
            typeof(VSUtils).GetMethod("FormatTime"),
            prefix: new HarmonyMethod(typeof(AccurateTimerMod).GetMethod(nameof(OnFormatTime)))
        );
        
        // Hook pause events
        harmony.Patch(
            typeof(TimerManagerGame).GetMethod("OnPause"),
            postfix: new HarmonyMethod(typeof(AccurateTimerMod).GetMethod(nameof(OnGamePause)))
        );
        
        harmony.Patch(
            typeof(TimerManagerGame).GetMethod("OnResume"),
            postfix: new HarmonyMethod(typeof(AccurateTimerMod).GetMethod(nameof(OnGameResume)))
        );
        
        MelonLogger.Msg("Accurate Timer initialized");
    }
    
    public static bool OnFormatTime(float seconds, ref string __result)
    {
        // Capture accurate game time
        if (seconds >= 0 && seconds < 7200)  // Reasonable game time range
        {
            currentGameTime = seconds;
        }
        return true;  // Run original method
    }
    
    public static void OnGamePause(TimerManagerGame __instance)
    {
        if (!isGamePaused)
        {
            isGamePaused = true;
            pauseCount++;
            MelonLogger.Msg($"[Timer] Paused at {FormatTime(currentGameTime)} (#{pauseCount})");
        }
    }
    
    public static void OnGameResume(TimerManagerGame __instance)
    {
        if (isGamePaused)
        {
            isGamePaused = false;
            MelonLogger.Msg($"[Timer] Resumed at {FormatTime(currentGameTime)}");
        }
    }
    
    public override void OnUpdate()
    {
        if (Input.GetKeyDown(KeyCode.F2))
        {
            DisplayTimerInfo();
        }
    }
    
    private void DisplayTimerInfo()
    {
        MelonLogger.Msg($"Game Time: {FormatTime(currentGameTime)} ({currentGameTime:F2}s)");
        MelonLogger.Msg($"Paused: {isGamePaused}");
        MelonLogger.Msg($"Total Pauses: {pauseCount}");
        
        // Verify accuracy against UI
        var gm = GM.Core;
        if (gm?.MainUI?._TimeText != null)
        {
            string uiTime = gm.MainUI._TimeText.text;
            MelonLogger.Msg($"UI Display: {uiTime}");
            MelonLogger.Msg($"Match: {(uiTime == FormatTime(currentGameTime) ? "✓" : "✗")}");
        }
    }
    
    private static string FormatTime(float seconds)
    {
        int minutes = (int)(seconds / 60f);
        int secs = (int)(seconds % 60f);
        return $"{minutes:00}:{secs:00}";
    }
    
    // Public API
    public static float GetGameSeconds() => currentGameTime;
    public static bool IsPaused() => isGamePaused;
    public static string GetFormattedTime() => FormatTime(currentGameTime);
}
```

## Testing Timer Accuracy

### Verification Steps

1. **Start Stage** - Timer begins at 00:00
2. **Open Weapon Selection** - Timer freezes at current value
3. **Close Selection** - Timer resumes from exact pause point
4. **Check UI Match** - Hook value equals displayed time
5. **Test Merchant/Healer** - Verify pause during interactions

### Debug Output Example

```
[Timer] Game started - timers active
[Timer] Game Time: 00:12 (12.00s total)
[Timer] Paused at 00:25 (#1)
[Timer] Resumed at 00:25
[Timer] Game Time: 01:00 (60.00s total)
```

## Common Pitfalls

### Using Unity Time Directly

```csharp
// INCORRECT - continues during pauses
float gameTime = Time.time;

// CORRECT - use timer system
float gameTime = VSUtils.FormatTime parameter;
```

### Relying on Stage._currentMinute

```csharp
// INCORRECT - only minute precision
int gameMinutes = stage._currentMinute;
float approxTime = gameMinutes * 60f;  // Missing seconds

// CORRECT - get exact time
float exactTime = capturedFromFormatTime;
```

### Ignoring Timer State

```csharp
// INCORRECT - manual calculation
float elapsed = Time.time - startTime;

// CORRECT - use Timer.GetTimeElapsed()
float elapsed = timer.GetTimeElapsed();  // Handles pause state
```

## Performance Considerations

- VSUtils.FormatTime called multiple times per frame
- Filter by reasonable time ranges (0-7200 seconds)
- Cache timer references to avoid repeated lookups
- Minimize logging in high-frequency hooks

## Related Systems

- [Game Manager](gamemanager.md) - Core game state management
- [Stage](stage.md) - Stage timer integration
- [UI System](ui.md) - Timer display components