# Vampire Survivors Multiplayer System

## Overview

Vampire Survivors v1.0+ includes a comprehensive multiplayer system supporting both local cooperative and online multiplayer gameplay. This system is built on Unity Coherence Toolkit for network synchronization and Rewired for input management.

**File Locations:**
- `F:\vampire\melon_decompiled\Il2CppVampireSurvivors.Runtime\Il2CppVampireSurvivors.Framework\MultiplayerManager.cs`
- `F:\vampire\melon_decompiled\Il2CppVampireSurvivors.Runtime\Il2CppVampireSurvivors.Framework\CoopConfig.cs`
- `F:\vampire\melon_decompiled\Il2CppVampireSurvivors.Runtime\Il2CppVampireSurvivors.Framework\GameManager.cs` (Lines 7432, 7446)

## Core Components

### 1. MultiplayerManager

**Location:** `Il2CppVampireSurvivors.Framework.MultiplayerManager`  
**Access:** `GM.Core._multiplayerManager`  
**Dependencies:** Injected into GameManager via Zenject

#### Key Properties

```csharp
public class MultiplayerManager
{
    // Core Multiplayer Detection
    public bool IsMultiplayer { get; }                    // Any multiplayer mode active
    
    // Configuration
    public CoopConfig CoopConfig { get; }                 // 738 references - central config
    public CoopConfig _coopConfig { get; }                // Internal backing field
    
    // Player Management
    public List<Player> _players { get; set; }            // All active players
    public List<Player> _playersToRemove { get; set; }    // Players queued for removal
    public List<Player> _disconnectedPlayers { get; set; } // Disconnected players
    
    // AI Character Management
    public List<FollowerData> AICharacters { get; }       // AI-controlled characters
    
    // Event System
    public event OnPlayerStateChange PlayerAdded;         // Fired when player joins
    public event OnPlayerStateChange PlayerRemoved;       // Fired when player leaves
    public event OnPlayerStateChange PlayerSelected;      // UI selection events
    public event OnPlayerStateChange PlayerSettingChanged; // Player config changes
    public event OnControllerStateChange ControllerDisconnected; // Hardware disconnections
}
```

#### Core Methods

```csharp
// Player Access
public List<Player> GetPlayers();                    // Get all active players
public int GetPlayerCount();                         // Get active player count
public Player GetPlayerFromID(int id);               // Find player by ID
public Player GetPlayerFromIndex(int playerIndex);   // Find player by index
public Player GetSelectedPlayer();                   // Get currently selected player
public Player GetPlayerOne();                        // Get player 1 (host)

// Player Management
public void SelectPlayer(Player player, bool animate = true, bool sound = true, float delay = 0f);
public bool NextPlayer(bool wrapAround = true, bool animate = true);
public bool PreviousPlayer(bool wrapAround = true, bool animate = true);

// Character Management
public void AddCharacter(Player player, CharacterController character);
public void RemoveCharacter(Player player);
public void RemoveAllCharacters();
public CharacterController GetCharacter(int playerIndex);
public CharacterController GetCharacter(Player player);
public List<CharacterController> GetAllCharacters();
public CharacterController GetSelectedCharacter();

// Visual/UI
public Color GetPlayerColour(Player player);         // Get player-specific color
```

### 2. Game Mode Detection

These properties are located in **GameManager**, NOT MultiplayerManager.

```csharp
public class GameManager
{
    // Mode Detection Properties
    public bool IsOnlineMultiplayer { get; }         // 318 references - Network multiplayer
    public bool IsLocalMultiplayer { get; }          // Local cooperative play
    
    // Internal Fields
    public bool _isOnlineMultiplayer { get; set; }   // Backing field
    public bool _isLocalMultiplayer { get; set; }    // Backing field
}

// Access Pattern
bool isOnline = GM.Core.IsOnlineMultiplayer;
bool isLocal = GM.Core.IsLocalMultiplayer;
bool isAnyMulti = GM.Core._multiplayerManager.IsMultiplayer;
```

#### Game Mode Logic

```csharp
// Single Player: Neither flag is true
if (!GM.Core.IsOnlineMultiplayer && !GM.Core.IsLocalMultiplayer)
{
    // Single player mode
    // - One CharacterController
    // - Fixed camera on player
    // - Direct loot collection
    // - No revival mechanics
}

// Local Multiplayer: IsLocalMultiplayer = true, IsOnlineMultiplayer = false
else if (GM.Core.IsLocalMultiplayer && !GM.Core.IsOnlineMultiplayer)
{
    // Local cooperative mode (up to 4 players)
    // - Multiple CharacterControllers
    // - Shared screen with dynamic camera
    // - Revival system active
    // - Enemy scaling based on player count
}

// Online Multiplayer: IsOnlineMultiplayer = true
else if (GM.Core.IsOnlineMultiplayer)
{
    // Network multiplayer mode
    // - Coherence Toolkit synchronization
    // - Latency compensation
    // - Host authority model
    // - Network-aware revival system
}
```

### 3. CoopConfig System

**Location:** `Il2CppVampireSurvivors.Framework.CoopConfig`  
**Type:** ScriptableObject  
**Access:** `GM.Core._multiplayerManager.CoopConfig`

#### Enums

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

#### Experience and Progression Settings

```csharp
// XP System
public float _xpGainModifier;                     // Global XP multiplier
public float _xpGainDivisionPerPlayer;            // XP split factor per additional player
public bool _globalLevelNumber;                   // Share level progression globally

// Enemy Scaling
public float _enemyHealthModifierPerMinute;       // Health increase per minute
public int _extraCharmPerPlayer;                  // Charm scaling per player
public int _extraCharmCutoffMinute;               // When to stop adding charm

// Loot and Upgrades
public AccessoryBonusMode _accessoryBonusMode;    // How accessory bonuses are shared
public bool _blockWeaponsOwnedByOtherPlayers;     // Prevent weapon overlap
public bool _blockAccessoriesOwnedByOtherPlayers; // Prevent accessory overlap
public bool _limitAccessoriesLikeWeapons;         // Apply weapon limits to accessories
public bool _shareEvolutionPassives;              // Share evolution requirements
public float _goldBonusForNotSharingPassives;     // Gold bonus when not sharing
```

#### Revival System Settings

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

#### Camera and Visual Settings

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

#### Treasure and Randomization Settings

```csharp
// Treasure Chest Behavior
public float _chestRandomisationSpeedMultiplier;  // Chest spin speed multiplier
public float _chestRandomisationLength;           // Duration of chest randomization
public AnimationCurve _chestRandomisationSpinPositionCurve; // Chest spin animation
public int _chestRandomnessSetSize;               // Size of random item pool
public bool _chestRandomPrioritiseEvolvablePlayers; // Favor players who can evolve

// Special Items
public int _amuletsInAmuletBag;                   // Amulets per bag
public int _amuletBagSize;                        // Maximum amulet bag size
```

#### Enemy Behavior Settings

```csharp
// Enemy Mechanics
public int _enemyChompMaxCount;                   // Max enemies that can chomp
public float _enemyChompHPGainProportionPerChomp; // HP gain per chomp
public float _enemyChompScaleGainProportionPerChomp; // Scale gain per chomp
public HitVfxType _enemyChompEffect;              // Visual effect for chomping
public bool _spawningEnemiesTargetDeadPlayersAlso; // Target dead players too
```

#### Controller and Feedback Settings

```csharp
// Controller Feedback
public float _levelupVibrationMilliseconds;       // Level up vibration duration
```

### 4. Player Management System

#### Player vs CharacterController

```csharp
// Player (Rewired) - Input and Identity
public class Player // From Il2CppRewired namespace
{
    // This represents the input player (controller/keyboard)
    // Handles input mapping, controller assignment, and identity
    // Used for UI navigation and control assignment
}

// CharacterController - Game Entity
public class CharacterController // Game entity that moves and fights
{
    // This is the actual in-game character that moves, fights, and collects items
    // Each Player is associated with one CharacterController during gameplay
    // Handles movement, combat, inventory, and game mechanics
}
```

#### Player Lifecycle Management

```csharp
// Adding Players
GM.Core._multiplayerManager.PlayerAdded += (player) =>
{
    Debug.Log($"Player {player.id} joined the game");
    // Player added event fired
    // CharacterController will be created and assigned separately
};

// Removing Players
GM.Core._multiplayerManager.PlayerRemoved += (player) =>
{
    Debug.Log($"Player {player.id} left the game");
    // Player removed event fired
    // Associated CharacterController will be cleaned up
};

// Player Disconnection Handling
GM.Core._multiplayerManager.ControllerDisconnected += (controller) =>
{
    // Hardware controller disconnected
    // Player moved to _disconnectedPlayers list
    // Game can pause or show reconnection UI
};
```

#### AI Characters (FollowerData)

```csharp
// AI Character Configuration
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

// Access AI Characters
List<FollowerData> aiCharacters = GM.Core._multiplayerManager.AICharacters;
```

### 5. Network vs Local Differences

#### Network-Specific Features (Online Multiplayer)

```csharp
// Coherence Toolkit Integration
if (GM.Core.IsOnlineMultiplayer)
{
    // Network synchronization active
    // - CoherenceSync components on all networked entities
    // - Host authority for game state
    // - Latency compensation for movement and actions
    // - Network-aware spawning and despawning
    // - Remote player state synchronization
    
    // Example: Check if entity has network sync
    var sync = characterController.GetComponent<CoherenceSync>();
    if (sync != null)
    {
        // This entity is network-synchronized
        // Modifications may need host authority
    }
}
```

#### Local-Specific Features (Local Cooperative)

```csharp
if (GM.Core.IsLocalMultiplayer && !GM.Core.IsOnlineMultiplayer)
{
    // Local cooperative features
    // - Shared screen with dynamic camera positioning
    // - Immediate state updates (no network latency)
    // - Local input handling for all players
    // - Shared game state without synchronization overhead
    // - Split-screen or shared-screen rendering
    
    // Player limit: Up to 4 players typically
    int maxLocalPlayers = 4; // Inferred from typical cooperative games
}
```

#### Network Items System

```csharp
// Network Items System
public static class NetworkItems
{
    public static HashSet<ItemType> _networkItems;    // Items that sync over network
    
    public static bool IsNetworkItem(ItemType type);  // 37 references
}

// Usage in online multiplayer
if (GM.Core.IsOnlineMultiplayer && NetworkItems.IsNetworkItem(itemType))
{
    // This item requires network synchronization
    // Handle with network authority checks
}
```

### 6. Data Change Tracking for Online Synchronization

**Location:** `DataManager` class

```csharp
public class DataManager
{
    // Online Synchronization Flags
    public bool _characterDataChangedForOnline;    // Character modifications
    public bool _powerUpDataChangedForOnline;      // Power-up changes
    public bool _stageDataChangedForOnline;        // Stage modifications
    public bool _weaponDataChangedForOnline;       // Weapon changes
    public bool _enemyDataChangedForOnline;        // Enemy modifications
}
```

## Modding Guidelines and Best Practices

### 1. Multiplayer-Aware Development

```csharp
[HarmonyPatch(typeof(SomeGameClass), "SomeMethod")]
public static class SomeGameClassPatch
{
    public static void Postfix(SomeGameClass __instance)
    {
        // Check multiplayer state first
        var gm = GM.Core;
        
        if (gm.IsOnlineMultiplayer)
        {
            // Online multiplayer requirements:
            // - Check network authority before modifying entities
            // - Respect host/client relationships  
            // - Handle synchronization delays
            // - Test with network latency
            
            if (IsHost()) // Implement host detection
            {
                // Host can safely modify game state
                ModifyGameState();
            }
            else
            {
                // Clients should request changes or only modify local UI
                RequestGameStateChange();
            }
        }
        else if (gm.IsLocalMultiplayer)
        {
            // Local cooperative features:
            // - No network concerns
            // - Immediate state updates
            // - Shared game state
            
            ModifyGameStateDirectly();
        }
        else
        {
            // Single player mode
            ModifyGameStateDirectly();
        }
    }
}
```

### 2. Player Management Examples

```csharp
// Get all active players
var players = GM.Core._multiplayerManager.GetPlayers();
foreach (var player in players)
{
    var character = GM.Core._multiplayerManager.GetCharacter(player);
    if (character != null)
    {
        // Do something with each player's character
        Debug.Log($"Player {player.id} has character: {character.name}");
    }
}

// React to player joins/leaves
GM.Core._multiplayerManager.PlayerAdded += OnPlayerJoined;
GM.Core._multiplayerManager.PlayerRemoved += OnPlayerLeft;

private static void OnPlayerJoined(Player player)
{
    Debug.Log($"Player {player.id} joined - total players: {GM.Core._multiplayerManager.GetPlayerCount()}");
}

private static void OnPlayerLeft(Player player)
{
    Debug.Log($"Player {player.id} left");
}
```

### 3. Configuration Access Examples

```csharp
// Access multiplayer configuration
var coopConfig = GM.Core._multiplayerManager.CoopConfig;

// Check revival settings
if (coopConfig._reviveAllRatherThanClosest)
{
    Debug.Log("Revive all nearby players instead of just the closest");
}

// Check camera mode
switch (coopConfig._cameraMode)
{
    case CoopConfig.CameraMode.AveragePosition:
        Debug.Log("Camera follows average player position");
        break;
    case CoopConfig.CameraMode.PositionExtentsCenter:
        Debug.Log("Camera centers on position bounds");
        break;
    case CoopConfig.CameraMode.VisualBoundsExtentsCenter:
        Debug.Log("Camera centers on visual bounds");
        break;
}

// Check accessory sharing mode
switch (coopConfig._accessoryBonusMode)
{
    case CoopConfig.AccessoryBonusMode.OwnerOnly:
        Debug.Log("Accessories only affect their owner");
        break;
    case CoopConfig.AccessoryBonusMode.MatchingDescription:
        Debug.Log("Share bonuses with players who have matching accessories");
        break;
    case CoopConfig.AccessoryBonusMode.AllPlayers:
        Debug.Log("All players benefit from all accessories");
        break;
}
```

### 4. Testing Considerations

#### Single Player Testing
```csharp
// Verify mod works in single player
Debug.Assert(!GM.Core.IsOnlineMultiplayer && !GM.Core.IsLocalMultiplayer);
```

#### Local Multiplayer Testing
```csharp
// Test with multiple controllers
Debug.Assert(GM.Core.IsLocalMultiplayer && !GM.Core.IsOnlineMultiplayer);
var playerCount = GM.Core._multiplayerManager.GetPlayerCount();
Debug.Log($"Testing with {playerCount} local players");
```

#### Online Multiplayer Testing
```csharp
// Test with network conditions
Debug.Assert(GM.Core.IsOnlineMultiplayer);
// Test with simulated latency, packet loss, etc.
```

### 5. Common Pitfalls and Solutions

#### Pitfall: Modifying Network Entities Without Authority
```csharp
// Incorrect - Direct modification in online mode
if (GM.Core.IsOnlineMultiplayer)
{
    character.health = 100; // Conflicts with network sync
}

// Correct - Check authority first
if (GM.Core.IsOnlineMultiplayer)
{
    var sync = character.GetComponent<CoherenceSync>();
    if (sync != null && sync.HasStateAuthority)
    {
        character.health = 100; // Safe
    }
    else
    {
        // Request change from host or modify locally only
    }
}
else
{
    character.health = 100; // Safe
}
```

#### Pitfall: Ignoring Game Mode Detection
```csharp
// Incorrect - Assuming single player
ModifyGameDirectly();

// Correct - Check mode first
if (GM.Core._multiplayerManager.IsMultiplayer)
{
    HandleMultiplayerModification();
}
else
{
    HandleSinglePlayerModification();
}
```

#### Pitfall: Not Handling Player Count Changes
```csharp
// Incorrect - Caching player count
private static int cachedPlayerCount = 1;

// Correct - Get current count
int currentPlayerCount = GM.Core._multiplayerManager.GetPlayerCount();
```

## Technical Architecture

### Key Files and Line Numbers
- `MultiplayerManager.cs` - Lines 22-1976+ (class definition through GetPlayerFromID method)
- `CoopConfig.cs` - Lines 14-784+ (complete class with all settings)
- `GameManager.cs` - Lines 7432 (IsLocalMultiplayer), 7446 (IsOnlineMultiplayer)
- `FollowerData.cs` - Lines 13-30+ (AI character configuration)
- `NetworkItems.cs` - Lines 12-50+ (network synchronization items)

### Reference Counts
- `CoopConfig`: 738 references throughout codebase
- `IsOnlineMultiplayer`: 318 references
- `PlayerAdded`/`PlayerRemoved`: Event system with multiple subscribers
- `IsNetworkItem`: 37 references for network synchronization

### Dependencies
- **Unity Coherence Toolkit**: Network synchronization (30+ assemblies)
- **Rewired**: Input management and Player class
- **Zenject**: Dependency injection for MultiplayerManager
- **Unity 6000.0.36f1**: Modern networking capabilities

## Summary

The Vampire Survivors multiplayer system handles single player, local cooperative (up to 4 players), and online multiplayer modes. Key requirements for modders:

1. **Always check game mode** using `IsOnlineMultiplayer`, `IsLocalMultiplayer`, and `IsMultiplayer`
2. **Respect network authority** in online modes
3. **Handle player lifecycle events** through the event system
4. **Test in all three modes** to ensure compatibility
5. **Use the extensive CoopConfig** for customization options

The system's modular design allows targeted modifications while maintaining compatibility across all multiplayer modes.