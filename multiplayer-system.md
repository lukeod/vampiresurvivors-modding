# Vampire Survivors Multiplayer System

This document contains technical reference for the multiplayer system implementation in Vampire Survivors, covering the architecture, components, and modding considerations across single-player, local cooperative, and online multiplayer modes.

**Source**: Decompiled IL2CPP code analysis  
**Game Version**: v1.0+ (Unity 6000.0.36f1)  
**Network Framework**: Unity Coherence Toolkit  

## Architecture Overview

Vampire Survivors supports three distinct gameplay modes:

- **Single Player**: Direct entity control, no synchronization requirements
- **Local Cooperative**: Up to 4 players on shared screen, no network layer
- **Online Multiplayer**: Network-synchronized gameplay via Coherence Toolkit

**Core File Locations:**
- `Il2CppVampireSurvivors.Framework.MultiplayerManager` - Primary multiplayer controller
- `Il2CppVampireSurvivors.Framework.CoopConfig` - Configuration system
- `Il2CppVampireSurvivors.Framework.GameManager` - Mode detection properties

## Core Components

### MultiplayerManager

**Access**: `GM.Core._multiplayerManager`  
**Injection**: Via Zenject dependency injection into GameManager

```csharp
public class MultiplayerManager
{
    // Mode Detection
    public bool IsMultiplayer { get; }                    // Any multiplayer mode active
    
    // Configuration
    public CoopConfig CoopConfig { get; }                 // 738 references - central config
    
    // Player Management
    public List<Player> _players { get; set; }            // All active players
    public List<Player> _playersToRemove { get; set; }    // Players queued for removal
    public List<Player> _disconnectedPlayers { get; set; } // Disconnected players
    
    // AI Character Management
    public List<FollowerData> AICharacters { get; }       // AI-controlled characters
    
    // Event System
    public event OnPlayerStateChange PlayerAdded;
    public event OnPlayerStateChange PlayerRemoved;
    public event OnPlayerStateChange PlayerSelected;
    public event OnPlayerStateChange PlayerSettingChanged;
    public event OnControllerStateChange ControllerDisconnected;
}
```

**Key Methods:**
```csharp
// Player Access
public List<Player> GetPlayers();
public int GetPlayerCount();
public Player GetPlayerFromID(int id);
public Player GetPlayerFromIndex(int playerIndex);
public Player GetSelectedPlayer();
public Player GetPlayerOne();

// Character Management
public CharacterController GetCharacter(int playerIndex);
public CharacterController GetCharacter(Player player);
public List<CharacterController> GetAllCharacters();
public CharacterController GetSelectedCharacter();

// Visual Systems
public Color GetPlayerColour(Player player);
```

### Game Mode Detection

Mode detection properties are located in **GameManager**, not MultiplayerManager.

```csharp
public class GameManager
{
    public bool IsOnlineMultiplayer { get; }         // 318 references - Network multiplayer
    public bool IsLocalMultiplayer { get; }          // Local cooperative play
    
    // Internal backing fields
    public bool _isOnlineMultiplayer { get; set; }
    public bool _isLocalMultiplayer { get; set; }
}
```

**Usage Pattern:**
```csharp
// Access through GameManager
bool isOnline = GM.Core.IsOnlineMultiplayer;
bool isLocal = GM.Core.IsLocalMultiplayer;
bool isAnyMulti = GM.Core._multiplayerManager.IsMultiplayer;
```

**Mode Logic Implementation:**
```csharp
// Single Player: Both flags false
if (!GM.Core.IsOnlineMultiplayer && !GM.Core.IsLocalMultiplayer)
{
    // Single player characteristics:
    // - One CharacterController
    // - Fixed camera positioning
    // - Direct loot collection
    // - No revival mechanics
}

// Local Multiplayer: IsLocalMultiplayer = true, IsOnlineMultiplayer = false
else if (GM.Core.IsLocalMultiplayer && !GM.Core.IsOnlineMultiplayer)
{
    // Local cooperative characteristics:
    // - Multiple CharacterControllers (up to 4 players)
    // - Shared screen with dynamic camera
    // - Revival system active
    // - Enemy scaling based on player count
}

// Online Multiplayer: IsOnlineMultiplayer = true
else if (GM.Core.IsOnlineMultiplayer)
{
    // Network multiplayer characteristics:
    // - Coherence Toolkit synchronization
    // - Latency compensation systems
    // - Host authority model
    // - Network-aware revival system
}
```

## Configuration System (CoopConfig)

**Type**: ScriptableObject  
**Access**: `GM.Core._multiplayerManager.CoopConfig`  
**Usage Count**: 738 references throughout codebase (based on code analysis)

### Enumerations

```csharp
public enum CameraMode
{
    AveragePosition,              // Center camera on average player position
    PositionExtentsCenter,        // Center on position bounds
    VisualBoundsExtentsCenter     // Center on visual bounds
}

public enum AccessoryBonusMode
{
    OwnerOnly,                    // Accessories only affect their owner
    MatchingDescription,          // Share with players who have matching accessories
    AllPlayers                    // All players benefit from all accessories
}
```

### Experience and Progression

```csharp
// XP System Configuration
public float _xpGainModifier;                     // Global XP multiplier
public float _xpGainDivisionPerPlayer;            // XP split factor per additional player
public bool _globalLevelNumber;                   // Share level progression globally

// Enemy Scaling
public float _enemyHealthModifierPerMinute;       // Health increase per minute
public int _extraCharmPerPlayer;                  // Charm scaling per player
public int _extraCharmCutoffMinute;               // When to stop adding charm

// Loot Distribution
public AccessoryBonusMode _accessoryBonusMode;    // How accessory bonuses are shared
public bool _blockWeaponsOwnedByOtherPlayers;     // Prevent weapon overlap
public bool _blockAccessoriesOwnedByOtherPlayers; // Prevent accessory overlap
public bool _limitAccessoriesLikeWeapons;         // Apply weapon limits to accessories
public bool _shareEvolutionPassives;              // Share evolution requirements
public float _goldBonusForNotSharingPassives;     // Gold bonus when not sharing
```

### Revival System

```csharp
// Revival Mechanics
public float _revivalSpeedWithRevive;             // Revival speed with Revive powerup
public float _revivalSpeedWithoutRevive;          // Revival speed without Revive
public float _revivalLossSpeed;                   // Speed of losing revival progress
public float _revivalRange;                       // Range for revival interactions
public bool _reviveAllRatherThanClosest;          // Revive all nearby vs closest
public bool _immediateRevivalUsage;               // Instant revival consumption
public float _decompositionTimeSeconds;           // Time before body disappears

// Death and Ghost Mechanics
public bool _ghostUsesCharacterSprite;            // Ghost appearance setting
public float _removeDeadPlayerFromCameraDuration; // Camera adjustment delay
public bool _removeDeadPlayersFromCamera;         // Remove dead from camera bounds
```

### Camera System

```csharp
// Camera Configuration
public CameraMode _cameraMode;                    // Camera positioning mode
public float _screenBoundsTopOffsetPixels;        // Top screen boundary offset
public float _screenBoundsBottomOffsetPixels;     // Bottom screen boundary offset
public float _fixedCameraOffsetPixels;            // Fixed camera offset
public bool _usePhysicalCameraBounds;             // Use physical screen bounds
public float _physicalScreenBoundsTopOffsetPixels; // Physical boundary offset

// UI Elements
public MultiplayerRevivalUI _multiplayerRevivalUIPrefab;  // Revival UI prefab
public PlayerIndicator _playerIndicatorUIPrefab;          // Player indicator prefab
public Material _navigationUIMaterial;                    // UI navigation material
public float _multiplayerIndicatorDuration;              // Indicator display time
```

### Treasure and Enemy Behavior

```csharp
// Treasure Chest Behavior
public float _chestRandomisationSpeedMultiplier;  // Chest spin speed multiplier
public float _chestRandomisationLength;           // Duration of chest randomization
public AnimationCurve _chestRandomisationSpinPositionCurve; // Chest spin animation
public int _chestRandomnessSetSize;               // Size of random item pool
public bool _chestRandomPrioritiseEvolvablePlayers; // Favor players who can evolve

// Enemy Mechanics
public int _enemyChompMaxCount;                   // Max enemies that can chomp
public float _enemyChompHPGainProportionPerChomp; // HP gain per chomp
public float _enemyChompScaleGainProportionPerChomp; // Scale gain per chomp
public HitVfxType _enemyChompEffect;              // Visual effect for chomping
public bool _spawningEnemiesTargetDeadPlayersAlso; // Target dead players too

// Special Items
public int _amuletsInAmuletBag;                   // Amulets per bag
public int _amuletBagSize;                        // Maximum amulet bag size
```

## Player Management

### Player vs CharacterController Distinction

```csharp
// Player (Il2CppRewired namespace) - Input and Identity
public class Player
{
    // Represents the input player (controller/keyboard)
    // Handles input mapping, controller assignment, and identity
    // Used for UI navigation and control assignment
}

// CharacterController - Game Entity
public class CharacterController
{
    // The actual in-game character that moves, fights, and collects items
    // Each Player is associated with one CharacterController during gameplay
    // Handles movement, combat, inventory, and game mechanics
}
```

### AI Characters (FollowerData)

```csharp
public class FollowerData
{
    public CharacterType _FollowerCharacter;         // Which character type
    public AIType _FollowerAI;                       // AI behavior type
    public bool _IsFollowerInvinceable;              // Cannot be damaged
    public bool _CountsAsMainCharacterForRevivals;   // Can perform revivals
    public bool _ManualLevelUps;                     // Manual vs auto level ups
    public bool _TrackedByCamera;                    // Include in camera bounds
    public bool _ShouldFollowMainPlayer;             // Follow behavior
    public bool _AllowDuplicates;                    // Allow multiple of same type
}
```

### Player Lifecycle Events

```csharp
// Event subscription pattern
GM.Core._multiplayerManager.PlayerAdded += (player) =>
{
    // Player joined - CharacterController created separately
};

GM.Core._multiplayerManager.PlayerRemoved += (player) =>
{
    // Player removed - CharacterController cleanup handled
};

GM.Core._multiplayerManager.ControllerDisconnected += (controller) =>
{
    // Hardware controller disconnected
    // Player moved to _disconnectedPlayers list
};
```

## Network System Integration

### Network Items Classification

```csharp
public static class NetworkItems
{
    public static HashSet<ItemType> _networkItems;    // Items that sync over network
    
    public static bool IsNetworkItem(ItemType type);  // 37 references
}

// Usage pattern for online synchronization
if (GM.Core.IsOnlineMultiplayer && NetworkItems.IsNetworkItem(itemType))
{
    // Item requires network synchronization
    // Handle with network authority considerations
}
```

### Data Change Tracking

Located in `DataManager` class for online synchronization:

```csharp
public class DataManager
{
    // Online synchronization flags - appears to track modifications
    public bool _characterDataChangedForOnline;    // Character modifications
    public bool _powerUpDataChangedForOnline;      // Power-up changes
    public bool _stageDataChangedForOnline;        // Stage modifications
    public bool _weaponDataChangedForOnline;       // Weapon changes
    public bool _enemyDataChangedForOnline;        // Enemy modifications
}
```

### Authority System (Online Mode)

Based on code analysis, online multiplayer appears to use Coherence Toolkit's authority model (inferred from network system integration):

- **Host Authority**: Host maintains authoritative game state
- **Entity Authority**: Individual entities may have distributed authority
- **Input Authority**: Separate from state authority for control

## Modding Considerations

### Mode Detection for Mods

```csharp
// Mode detection pattern for mod development
[HarmonyPatch(typeof(SomeGameClass), "SomeMethod")]
public static class SomeGameClassPatch
{
    public static void Postfix(SomeGameClass __instance)
    {
        var gm = GM.Core;
        if (gm == null) return; // GM.Core appears null in menu contexts
        
        if (gm.IsOnlineMultiplayer)
        {
            // Online multiplayer - network authority considerations appear required
            // Modifications appear to need host permission or entity authority
            // Synchronization delays and network latency require consideration
        }
        else if (gm.IsLocalMultiplayer)
        {
            // Local cooperative - no network considerations based on code analysis
            // Multiple players share game state directly
            // Immediate state updates appear possible
        }
        else
        {
            // Single player - control available based on code structure
            // No multiplayer considerations needed
        }
    }
}
```

### Modification Patterns

Based on the decompiled code structure, modification patterns vary by target system:

**Data Modifications:**
```csharp
// Modifying data structures appears to affect all modes
// Based on code analysis, these affect all players
[HarmonyPatch(typeof(DataManager), "ReloadAllData")]
public static void ModifyGameData(DataManager __instance)
{
    // Weapon data, character data, stage data modifications
    // Inferred from code: loaded by all clients/players
}
```

**Entity Modifications:**
```csharp
// Direct entity modifications appear to require authority checking in online mode
// Based on NetworkItems system and data change tracking analysis
if (GM.Core.IsOnlineMultiplayer)
{
    // Authority checking appears required based on code structure
    // Inferred need to verify entity ownership or host status
}
```

### Player Enumeration Pattern

```csharp
// Player enumeration based on API analysis
var players = GM.Core._multiplayerManager.GetPlayers();
foreach (var player in players)
{
    var character = GM.Core._multiplayerManager.GetCharacter(player);
    if (character != null)
    {
        // Process each player's character
        // Authority requirements appear necessary for modifications
    }
}
```

## Configuration Access Examples

```csharp
// Access configuration settings
var coopConfig = GM.Core._multiplayerManager.CoopConfig;

// Check revival configuration
if (coopConfig._reviveAllRatherThanClosest)
{
    // Revival affects all nearby players vs closest only
}

// Check camera mode
switch (coopConfig._cameraMode)
{
    case CoopConfig.CameraMode.AveragePosition:
        // Camera follows average player position
        break;
    case CoopConfig.CameraMode.PositionExtentsCenter:
        // Camera centers on position bounds
        break;
    case CoopConfig.CameraMode.VisualBoundsExtentsCenter:
        // Camera centers on visual bounds
        break;
}

// Check accessory sharing mode
switch (coopConfig._accessoryBonusMode)
{
    case CoopConfig.AccessoryBonusMode.OwnerOnly:
        // Accessories only affect their owner
        break;
    case CoopConfig.AccessoryBonusMode.MatchingDescription:
        // Share bonuses with players who have matching accessories
        break;
    case CoopConfig.AccessoryBonusMode.AllPlayers:
        // All players benefit from all accessories
        break;
}
```

## Reference Information

### Key File References
- `MultiplayerManager.cs` - Lines 22-1976+ (complete class implementation)
- `CoopConfig.cs` - Lines 14-784+ (configuration system)
- `GameManager.cs` - Lines 7432 (IsLocalMultiplayer), 7446 (IsOnlineMultiplayer)
- `FollowerData.cs` - Lines 13-30+ (AI character configuration)
- `NetworkItems.cs` - Lines 12-50+ (network synchronization items)

### Usage Statistics
- `CoopConfig`: 738 references throughout codebase (based on code analysis)
- `IsOnlineMultiplayer`: 318 references (based on decompiled code)
- `IsNetworkItem`: 37 references for network synchronization (based on code analysis)
- Player management events have multiple subscribers across systems (inferred from code structure)

### Dependencies
- **Unity Coherence Toolkit**: Network synchronization framework (30+ assemblies based on decompiled code)
- **Rewired**: Input management system providing Player class (inferred from code analysis)
- **Zenject**: Dependency injection system for MultiplayerManager (based on code structure)
- **Unity 6000.0.36f1**: Base engine with networking capabilities (version detected in code)

## Summary

Based on code analysis, Vampire Survivors appears to implement a multiplayer system that supports single-player, local cooperative (up to 4 players based on decompiled code), and online multiplayer modes. The system appears to use event-driven architecture with configuration options managed through CoopConfig.

For mod developers, understanding the mode detection system and authority patterns appears necessary for creating compatible modifications based on the code structure. The CoopConfig system appears to offer configuration points, while the network integration appears to require consideration of synchronization requirements in online modes.

All behavior descriptions are inferred from decompiled code analysis and have not been validated through runtime testing.