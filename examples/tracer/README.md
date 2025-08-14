# TracerMod - Loading Sequence Diagnostic Tool

## Overview

TracerMod is a diagnostic tool that traces the Vampire Survivors loading sequence and provides insights into the game's initialization flow. This mod demonstrates several important patterns for IL2CPP modding with MelonLoader.

## Features

- Traces application startup sequence
- Monitors `DataManager.ReloadAllData` calls (showing multiple DLC loading passes)
- Detects when `GameManager.Awake` is called (game session start)
- Monitors `GM.Core` availability using `OnUpdate`
- Provides F1 hotkey for state dump during gameplay

## Key Concepts Demonstrated

### 1. Manual Harmony Patching
Instead of using attributes (which may fail silently with IL2CPP), the mod uses manual patching:

```csharp
var harmony = new Harmony("TracerMod");
var method = typeof(DataManager).GetMethod("ReloadAllData");
harmony.Patch(method, postfix: new HarmonyMethod(typeof(TracerMod).GetMethod("OnReloadAllData")));
```

### 2. GM.Core Monitoring Pattern
The mod demonstrates the recommended pattern for detecting when the game session starts:

```csharp
public override void OnUpdate()
{
    if (GM.Core != null && !hasLoggedGameManager)
    {
        // Game session has started
        hasLoggedGameManager = true;
    }
}
```

### 3. Multiple ReloadAllData Calls
The mod tracks how `ReloadAllData` is called multiple times during startup:
- Call 1: Base game data
- Calls 2-4: DLC data loading
- Call 5: Final data merge

### 4. Timing Information
Uses `Stopwatch` to provide accurate timing information for all events, helping understand the initialization sequence.

## Building

### Prerequisites
- .NET 6.0 SDK
- MelonLoader installed in Vampire Survivors
- Game assemblies from `MelonLoader/Il2CppAssemblies/`

### Build Steps
1. Navigate to the tracer directory
2. Run `dotnet build`
3. Copy `bin/Debug/net6.0/TracerMod.dll` to your game's `Mods` folder

### Using the Provided Project File
```bash
dotnet build TracerMod.csproj
```

## Usage

1. Install the mod in your `Mods` folder
2. Launch Vampire Survivors with MelonLoader
3. Watch the console for loading sequence information
4. Start a game to see `GameManager.Awake` timing
5. Press F1 during gameplay to dump current state

## Expected Output

### During Startup
```
[2136ms] DataManager.ReloadAllData called #1
  - Weapon JSON data is loaded
[2354ms] DataManager.ReloadAllData called #2
  - Weapon JSON data is loaded
[2545ms] DataManager.ReloadAllData called #3
  - Weapon JSON data is loaded
```

### When Starting Game
```
[25788ms] GameManager.Awake called #1
  - Instance type: Il2CppVampireSurvivors.Framework.GameManager
  - GM.Core is already set
[26641ms] GM.Core is now available!
  - DataManager is available
  - Weapon data loaded: 346 weapons
  - Player is available: ANTONIO
```

### F1 State Dump
```
=== Current Game State (F1) ===
Time since mod init: 31973ms
Update frames: 7710
ReloadAllData calls: 5
GameManager.Awake calls: 1

GM.Core: Available
  - DataManager: Yes
  - Player: Yes
  - Stage: Yes
  - MainUI: Yes

Player Details:
  - Character: ANTONIO
  - Level: 1
  - HP: 100
  - Power: 100.0
  - MoveSpeed: 1.0
```

## Important Notes

- `GM.Core` is null during menus and only becomes available when a game session starts
- `ReloadAllData` is called multiple times - don't assume single execution
- `GameManager.Awake` is called when entering gameplay, not at app startup
- Transpiler hooks do not work with IL2CPP games

## Learning Resources

This example demonstrates concepts from:
- [Hook Points Documentation](../../hook-points.md)
- [Core Architecture](../../core-architecture.md)
- [IL2CPP Guide](../../il2cpp-guide.md)
- [Data Manager](../../data-manager.md)