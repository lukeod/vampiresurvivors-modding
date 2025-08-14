# Vampire Survivors Diagnostic & Dumping Tools

A collection of diagnostic and data extraction tools for Vampire Survivors modding with MelonLoader.

## Quick Start

1. Set environment variable: `$env:VAMPIRE_SURVIVORS_PATH = "F:\vampire\VampireSurvivors"`
2. Build all tools: `.\build-all.bat`
3. Tools will be in `bin\Release\net6.0\`

## Tools Overview

### TracerMod v2.0.0
**Purpose:** Understand game loading sequence and lifecycle  
**Key Features:**
- Tracks multiple `ReloadAllData` calls (base + DLCs)
- Monitors `GameManager.Awake` timing
- Shows when `GM.Core` becomes available
- **F1** - Dump current game state

### DLCInspector v1.0.0
**Purpose:** Examine and export DLC content  
**Controls:**
- **F2** - Show DLC summary
- **F3** - Export all JSON data
- **F4** - Show loading sequence

**Exports to:** `UserData/DLCExports/`

### AssetDumper v1.0.0
**Purpose:** Comprehensive asset extraction  
**Controls:**
- **F1** - Show help
- **F5** - Export sprites/textures metadata
- **F6** - Export localization/text
- **F7** - Export collections/unlockables
- **F8** - Export audio data
- **F9** - Export EVERYTHING

**Exports to:** `UserData/AssetDumps/[timestamp]/`

## Key Findings

- **IL2CPP**: Transpilers don't work (C# → C++ → Native)
- **Loading**: `ReloadAllData` called 5+ times for DLCs
- **GM.Core**: Null during menus, only available in-game
- **DLC Types**: Moonspell, Foscari, Chalcedony, FirstBlood, Emeralds, ThosePeople

## Technical Notes

### Manual Harmony Patching (Required for IL2CPP)
```csharp
var harmony = new HarmonyLib.Harmony("ModName");
var method = typeof(TargetClass).GetMethod("MethodName");
harmony.Patch(method, postfix: new HarmonyMethod(typeof(MyClass).GetMethod("MyPatch")));
```

### EggFloat/EggDouble Access
```csharp
// Wrong: stats.Power.Value
// Right: stats.Power.GetValue()
```

## Building

Requires:
- .NET 6.0 SDK
- MelonLoader installed in game
- `VAMPIRE_SURVIVORS_PATH` environment variable

```powershell
# Set path (PowerShell)
$env:VAMPIRE_SURVIVORS_PATH = "F:\vampire\VampireSurvivors"

# Build all
.\build-all.bat

# Or build individual
dotnet build TracerMod.csproj -c Release
```

## License

MIT - Created for the Vampire Survivors modding community