# Vampire Survivors Modding Reference

Technical reference for modding Vampire Survivors using MelonLoader.

## Overview

Documentation of Vampire Survivors' internal structure and systems for mod development.

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

1. [Core Architecture](core-architecture.md) - Game structure and systems
2. [Hook Points](hook-points.md) - Mod entry points
3. [Data Manager](data-manager.md) - Data access and modification
4. [IL2CPP Guide](il2cpp-guide.md) - IL2CPP specifics
5. [Common Pitfalls](common-pitfalls.md) - Common issues and solutions

## Key Points

- `GM.Core` - Static accessor for GameManager
- `DataManager.GetConverted*()` - Returns absolute values (levels 2-8 store deltas)
- `ReloadAllData` - Hook point for data loading completion
- `AddStartingWeapon` - Recommended gameplay hook point
- Avoid hooking `OnTick` methods (performance-critical)

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

