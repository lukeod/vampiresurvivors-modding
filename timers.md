# Timers

The pause-aware timer system that tracks game time accurately through weapon selections, merchant interactions, and other pause states.

**Core Namespace**: `Il2CppVampireSurvivors.Framework.TimerSystem`  
**Formatting Utility**: `Il2CppVampireSurvivors.Tools.VSUtils`  
**Timer Helper**: `Il2CppVampireSurvivors.Tools.TimerHelper`

## Overview

Vampire Survivors uses a sophisticated timer framework that properly handles pause states rather than simple time accumulation. The system ensures timer accuracy when players open weapon selection, interact with merchants, access healers, or trigger other pause events. Game time is tracked through `Timer` objects managed by specialized `TimerManager` classes.

## Quick Reference

### Key Classes
```csharp
// Core timer classes
Il2CppVampireSurvivors.Framework.TimerSystem.Timer                 // Individual timer instance
Il2CppVampireSurvivors.Framework.TimerSystem.TimerManager          // Base manager class
Il2CppVampireSurvivors.Framework.TimerSystem.TimerManagerGame      // Game timers (pause-aware)
Il2CppVampireSurvivors.Framework.TimerSystem.TimerManagerUI        // UI timers (ignore pause)
Il2CppVampireSurvivors.Framework.TimerSystem.TimerManagerAutomation // Automation timers
Il2CppVampireSurvivors.Framework.TimerSystem.Timers                // Global timer registry

// Utility functions
Il2CppVampireSurvivors.Tools.VSUtils.FormatTime          // Formats seconds to MM:SS
Il2CppVampireSurvivors.Tools.TimerHelper                 // Helper methods for timer creation
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
    float _lastUpdateTime;                     // Last update timestamp
    float _Duration_k__BackingField;          // Timer duration
    bool _IsLooped_k__BackingField;           // Whether timer loops
    bool _IsCompleted_k__BackingField;        // Whether timer completed
    bool _UsesRealTime_k__BackingField;       // Real-time vs game-time flag
    bool _canPause;                           // Whether timer can be paused
    bool _isOnlineTimer;                      // Whether timer is for online play
    bool _hasAutoDestroyOwner;                // Whether has auto-destroy owner
    int _repeat;                              // Number of repeats
    Nullable<float> _timeElapsedBeforePause;  // Stores time when paused
    Nullable<float> _timeElapsedBeforeCancel; // Stores time when cancelled
    MonoBehaviour _autoDestroyOwner;          // Owner for auto-destroy
    Il2CppSystem.Action _onComplete;          // Completion callback
    Il2CppSystem.Action<float> _onUpdate;     // Update callback
    static TimerManager _manager;             // Global timer manager
}
```

### Timer Properties and Methods

```csharp
// Timer state properties
public float Duration { get; set; }           // Timer duration
public bool IsLooped { get; set; }            // Whether timer loops
public bool IsCompleted { get; protected set; } // Whether timer completed
public bool UsesRealTime { get; protected set; } // Real-time vs game-time
public bool IsPaused { get; }                 // Whether currently paused
public bool IsCancelled { get; }              // Whether cancelled
public bool IsDone { get; }                   // Whether done (completed or cancelled)
public int RepeatCount { get; }               // Current repeat count
public bool IsOwnerDestroyed { get; }         // Whether owner destroyed

// Timer control methods
public void Cancel()                          // Cancel the timer
public void Complete(bool runAllRepeats = false) // Force completion
public void Pause()                           // Pause the timer
public void Resume()                          // Resume the timer
public float GetTimeElapsed()                 // Get elapsed time
public float GetTimeRemaining()               // Get remaining time
public float GetRatioComplete()               // Get completion ratio (0-1)
public float GetRatioRemaining()              // Get remaining ratio (1-0)
public void Update()                          // Update timer (called by manager)
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

## Timer Helper Utility

### Creating Timers with TimerHelper

The new `TimerHelper` class provides convenient static methods for creating different types of timers:

```csharp
using Il2CppVampireSurvivors.Tools;
using Il2CppVampireSurvivors.Framework.TimerSystem;

// Game timers (pause-aware)
Timer gameTimer = TimerHelper.RegisterSecs(
    duration: 5.0f,                    // Duration in seconds
    onComplete: () => { /* callback */ },
    onUpdate: (elapsed) => { /* update */ },
    isLooped: false,                   // Whether to loop
    useRealTime: false,                // Use game time (pause-aware)
    autoDestroyOwner: this,            // Auto-destroy when owner destroyed
    repeat: 0,                         // Number of repeats (0 = no repeat)
    isOnlineTimer: false,              // Whether for online play
    canPause: true                     // Whether timer can be paused
);

// UI timers (ignore pause)
Timer uiTimer = TimerHelper.RegisterSecsUI(
    duration: 2.0f,
    onComplete: () => { /* callback */ },
    onUpdate: null,                    // Optional update callback
    isLooped: false,
    useRealTime: true,                 // UI timers often use real time
    autoDestroyOwner: uiComponent,
    repeat: 0
);

// Automation timers
Timer automationTimer = TimerHelper.RegisterSecsAutomation(
    duration: 10.0f,
    onComplete: () => { /* callback */ },
    onUpdate: null,
    isLooped: true,                    // Often used for periodic tasks
    useRealTime: false,
    autoDestroyOwner: null,
    repeat: 0
);

// Millisecond precision timers
Timer preciseTiming = TimerHelper.RegisterMillis(
    duration: 1500f,                   // 1.5 seconds in milliseconds
    onComplete: () => { /* callback */ }
);
```

### Timer Types

- **RegisterSecs/RegisterMillis**: Game timers that respect pause states
- **RegisterSecsUI/RegisterMillisUI**: UI timers that ignore pause states  
- **RegisterSecsAutomation/RegisterMillisAutomation**: Automation timers for background tasks

### Timer Constructor (Advanced Usage)

For direct timer creation without TimerHelper:

```csharp
Timer timer = new Timer(
    duration: 5.0f,                    // Duration in seconds
    onComplete: completeAction,        // Completion callback
    onUpdate: updateAction,            // Update callback
    isLooped: false,                   // Whether to loop
    usesRealTime: false,               // Use real time vs game time
    autoDestroyOwner: ownerObject,     // Auto-destroy when owner destroyed
    repeat: 0,                         // Number of repeats
    isMultiplayer: false,              // Whether for multiplayer
    canPause: true                     // Whether timer can be paused
);
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
        
        // Hook sync pause state (new in Unity 6000)
        harmony.Patch(
            typeof(TimerManagerGame).GetMethod("SyncPauseState"),
            postfix: new HarmonyMethod(typeof(AccurateTimerMod).GetMethod(nameof(OnSyncPauseState)))
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
    
    public static void OnSyncPauseState(TimerManagerGame __instance)
    {
        // This method synchronizes pause state across timer systems
        MelonLogger.Msg($"[Timer] Pause state synchronized");
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
    
    // Example of using TimerHelper for custom timers
    public static void CreateCustomTimer()
    {
        // Create a pause-aware game timer that runs for 30 seconds
        Timer customTimer = TimerHelper.RegisterSecs(
            duration: 30.0f,
            onComplete: () => MelonLogger.Msg("Custom timer completed!"),
            onUpdate: (elapsed) => {
                if (elapsed % 5.0f < 0.1f) // Log every 5 seconds
                {
                    MelonLogger.Msg($"Custom timer: {elapsed:F1}s elapsed");
                }
            },
            isLooped: false,
            useRealTime: false,  // Respects game pause
            autoDestroyOwner: null,
            repeat: 0,
            isOnlineTimer: false,
            canPause: true
        );
    }
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