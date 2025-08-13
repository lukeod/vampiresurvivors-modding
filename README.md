# Vampire Survivors Modding Reference

Comprehensive technical reference for modding Vampire Survivors using MelonLoader and IL2CPP decompiled source code.

## Overview

This documentation provides detailed technical information about the internal structure and systems of Vampire Survivors, gathered through IL2CPP decompilation and analysis. It serves as a reference for mod developers working with MelonLoader to create modifications for the game.

## Documentation Structure

### Core Systems
- [Core Architecture](core-architecture.md) - Static accessors, GameManager, and dependency injection
- [Data Manager](data-manager.md) - Central data repository and JSON storage system
- [Character System](character-system.md) - CharacterController and CharacterData structures
- [Weapon System](weapon-system.md) - Weapons, evolution, unions, and limit breaks
- [Stat System](stat-system.md) - Stat calculations, modifiers, and EggFloat/EggDouble wrappers
- [Damage System](damage-system.md) - Damage calculation pipeline and combat mechanics
- [Arcana System](arcana-system.md) - Arcana types and integration with game systems
- [Stage System](stage-system.md) - Stage management, enemy spawning, and events
- [Save System](save-system.md) - Save data persistence and file management
- [UI System](ui-system.md) - UI architecture and HUD updates

### Modding Guides
- [Hook Points](hook-points.md) - Validated entry points for mod initialization
- [Performance Guide](performance-guide.md) - Optimization tips and performance considerations
- [IL2CPP Guide](il2cpp-guide.md) - IL2CPP specifics, limitations, and patterns
- [Common Pitfalls](common-pitfalls.md) - Troubleshooting and solutions to common issues

## Quick Start

For modders looking to get started quickly:

1. Start with [Core Architecture](core-architecture.md) to understand the basic game structure
2. Review [Hook Points](hook-points.md) to find the best entry point for your mod
3. Check [Data Manager](data-manager.md) to learn how to access and modify game data
4. Read [IL2CPP Guide](il2cpp-guide.md) for important IL2CPP-specific considerations
5. Consult [Common Pitfalls](common-pitfalls.md) to avoid common mistakes

## Key Technical Insights

1. **GM.Core** is the official static accessor for GameManager
2. **DataManager** accessible through `GameManager.DataManager` property
3. **GetConverted Methods** - Use these for accessing game data with absolute values
4. **Weapon levels 2-8** store incremental deltas in JSON, but GetConvertedWeapons() returns absolute values
5. **ReloadAllData Hook** - Best place to know when all data is loaded and ready
6. **IL2Cpp Collections** - Can be iterated with foreach, always null-check
7. **MelonEnvironment** - Use UserDataDirectory for safe file I/O
8. **Zenject DI** - No singleton pattern, uses dependency injection
9. **AddStartingWeapon** is the best hook for gameplay mods
10. **Tick-based architecture** - Performance-critical system, avoid hooking OnTick methods

## Prerequisites

- MelonLoader installed and configured
- Basic understanding of C# and Unity
- IL2CPP decompiled assemblies (for reference)
- Visual Studio or compatible IDE for mod development

## Build Configuration

### Required References
- `Il2CppVampireSurvivors.Runtime.dll`
- `Il2CppVampireSurvivors.PhaserPort.dll`
- `Il2CppNewtonsoft.Json.dll`
- MelonLoader assemblies
- Unity assemblies (UnityEngine.CoreModule, etc.)

### Namespaces
```csharp
using Il2CppVampireSurvivors.Framework;
using Il2CppVampireSurvivors.Data;
using Il2CppVampireSurvivors.Objects.Characters;
using MelonLoader;
using HarmonyLib;
```

## Contributing

This documentation is based on analysis of the decompiled IL2CPP code and empirical testing. As the game updates, some information may become outdated. Contributions and corrections are welcome.

## Disclaimer

This documentation is for educational and modding purposes only. Always respect the game's EULA and the developers' wishes regarding modifications.