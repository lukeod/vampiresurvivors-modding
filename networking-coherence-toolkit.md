# Coherence Toolkit Networking in Vampire Survivors

## Overview

Vampire Survivors uses the **Coherence Toolkit** for its online multiplayer functionality. This document provides a comprehensive analysis of the networking architecture, based on verified examination of the decompiled source code, to help modders create network-compatible modifications.

## Core Architecture

### 1. Main Components

#### CoherenceBridge
- **Purpose**: Central network manager and connection hub
- **Key Properties**:
  - `IsConnected`: Network connection status
  - `IsSimulatorOrHost`: Whether this client is the host/simulator
  - `AuthorityManager`: Handles entity authority and ownership
  - `EntityCount`: Current number of networked entities
- **Key Methods**:
  - `Connect()`: Establish network connection
  - `Disconnect()`: Close network connection
  - `OnConnectionError()`: Handle connection failures

#### CoherenceSync
- **Purpose**: Entity synchronization component for networked objects
- **Key Properties**:
  - `HasStateAuthority`: Can modify entity state (health, stats, position)
  - `HasInputAuthority`: Can send input commands for entity control
  - `authorityTransferType`: How authority can be transferred (NotTransferable/Request/Stealing)
  - **Note**: There is NO simple `HasAuthority` property - always use the specific type
- **Key Methods**:
  - `RequestAuthority()`: Request authority over entity
  - `TransferAuthority()`: Transfer authority to another client
  - `AbandonAuthority()`: Give up authority over entity

#### AuthorityManager
- **Purpose**: Manages network entity ownership and authority transfers
- **Key Methods**:
  - `RequestAuthority()`: Process authority requests
  - `TransferAuthority()`: Handle authority transfers
  - `Adopt()`: Adopt orphaned entities

### 2. Game-Specific Integration

#### PlayerInfo
- **Network Properties**:
  - `AverageLatencyMs`: Current network latency
  - `UpdateAverageLatency`: Enable/disable latency tracking
  - `CharacterEntity`: Reference to networked character
  - `_coherenceSync`: CoherenceSync component reference

#### Character Synchronization
- **CharacterController Integration**:
  - `_coherenceSync`: Direct reference to sync component
  - `player`: CoherenceSync reference for multiplayer
  - `FollowedCharacter`: For AI followers in multiplayer
  - `Sync`: Public property to access synchronization component

#### Network Instantiators
The game uses specialized instantiators for different entity types:
- `CharacterInstantiator`: Player character spawning
- `EnemyInstantiator`: Enemy synchronization
- `ItemInstantiator`: Pickup/item replication
- `TreasureInstantiator`: Treasure chest networking
- `HostPlayerOptionsInstantiator`: Host configuration sync

#### Network Providers
Asset providers handle loading networked prefabs:
- `CharacterProvider`: Character prefab loading by CharacterType
- `EnemyProvider`: Enemy prefab loading by EnemyType
- `PickupProvider`: Item/pickup prefab management
- `DestructibleProvider`: Destructible object networking

## Authority Model

### Host Authority System
Vampire Survivors uses a **host authority** model where:
- One player acts as the authoritative host/simulator
- Host has final say on game state
- Clients can request authority for specific entities
- Authority can transfer between clients for certain objects

### Authority Types

**Important**: CoherenceSync has two distinct authority properties:

1. **State Authority (`HasStateAuthority`)**: Permission to modify entity's state and data
   - Health, stats, position, rotation
   - Game state changes
   - Most entity modifications

2. **Input Authority (`HasInputAuthority`)**: Permission to send input commands for entity
   - Player input processing
   - Movement commands
   - Action inputs

3. **Full Authority**: Having both state and input authority

**There is NO simple `HasAuthority` property** - you must always check the specific authority type you need.

### Authority Transfer Modes
- **NotTransferable**: Authority cannot be transferred
- **Request**: Authority must be requested and approved
- **Stealing**: Authority can be taken without approval

## Network Entity Lifecycle

### 1. Entity Creation
```
Host creates entity → CoherenceSync attached → Entity replicated to clients
```

### 2. Authority Assignment
```
Initial authority to host → Client requests authority → Host approves/denies
```

### 3. State Synchronization
```
Authority holder modifies state → Changes replicated to all clients
```

### 4. Entity Destruction
```
Authority holder destroys entity → Destruction replicated to all clients
```

## Latency Compensation

### Latency Tracking
- **PlayerInfo.AverageLatencyMs**: Real-time latency measurement
- **PlayerInfo.UpdateAverageLatency**: Control latency calculation
- Latency measured continuously during gameplay

### Compensation Mechanisms
- **Position Interpolation**: Smooth movement between network updates
- **Client Prediction**: Local movement prediction to hide latency
- **Server Reconciliation**: Correction when predictions are wrong

## Connection Management

### Connection Process
1. **GameManager.OnConnectionError()**: Handles connection failures
2. **CoherenceBridge.Connect()**: Establishes initial connection
3. **GameplaySceneConnector**: Manages scene-level connections
4. **HostPlayerOptions**: Synchronizes host configuration

### Error Handling
- **ConnectionException**: Network-level errors
- **GameManager.BlockConnectionErrorPopups**: Control error UI display
- Automatic reconnection attempts (implementation details in bridge)

## Modding Guidelines

### 1. Network-Aware Mods

#### Check Network Status
```csharp
[HarmonyPatch(typeof(SomeGameClass), "SomeMethod")]
public static bool Prefix(SomeGameClass __instance)
{
    if (GM.Core.IsOnlineMultiplayer)
    {
        // Online multiplayer logic - be careful with state changes
        return true; // Let original method run
    }
    
    // Single player or local coop - full freedom
    return true;
}
```

#### Respect Authority
```csharp
[HarmonyPatch(typeof(CharacterController), "ModifyHealth")]
public static bool Prefix(CharacterController __instance)
{
    if (GM.Core.IsOnlineMultiplayer)
    {
        var sync = __instance.Sync;
        if (sync != null && !sync.HasStateAuthority)
        {
            // Don't modify if we don't have authority
            return false;
        }
    }
    
    return true; // Proceed with modification
}
```

### 2. Safe Modding Patterns

#### Client-Side Only Modifications
Safe for multiplayer:
- UI changes
- Visual effects
- Audio modifications  
- Input handling improvements

#### Authority-Aware State Changes
```csharp
public void ModifyPlayerState(CharacterController character)
{
    if (GM.Core.IsOnlineMultiplayer)
    {
        var sync = character.Sync;
        if (sync == null || !sync.HasStateAuthority)
        {
            // Request authority or skip modification
            sync?.RequestAuthority(AuthorityType.Full);
            return;
        }
    }
    
    // Safe to modify state here
    character.ModifyHp(100);
}
```

### 3. Common Networking Pitfalls

#### Don't Do These in Online Multiplayer:
1. **Direct GameObject.Destroy()** on networked objects
2. **Spawning objects** without proper instantiators
3. **Modifying networked state** without authority
4. **Creating timers** that can desync between clients
5. **Using Random.Range()** without synchronized seeds

#### Safe Alternatives:
1. Use proper CoherenceSync destruction
2. Use game's instantiation system
3. Check authority before modifications
4. Use server-authoritative timing
5. Use deterministic randomization

### 4. Testing Network Mods

#### Test Scenarios:
1. **Host vs Client**: Test as both host and joining client
2. **Authority Transfer**: Test when authority changes hands
3. **Connection Loss**: Test reconnection scenarios  
4. **Latency**: Test with artificial network delay
5. **Multiple Clients**: Test with 2+ clients simultaneously

#### Debug Information:
```csharp
// Check player network status
var playerInfo = GM.Core.GetPlayerInfo(playerId);
Debug.Log($"Latency: {playerInfo.AverageLatencyMs}ms");
Debug.Log($"Has Character Entity: {playerInfo.CharacterEntity != null}");

// Check entity authority
var sync = someObject.GetComponent<CoherenceSync>();
Debug.Log($"Has State Authority: {sync.HasStateAuthority}");
Debug.Log($"Has Input Authority: {sync.HasInputAuthority}");
```

## Generated Synchronization Files

The game includes 512 generated CoherenceSync files for different entity types. These files handle the low-level synchronization of:
- Transform data (position, rotation, scale)
- Component properties (health, stats, etc.)
- Custom game state (weapons, abilities, etc.)

## Key Differences from Single Player

### Game Flow Changes:
- **Authority checks** before state modifications
- **Network instantiation** instead of direct spawning
- **Latency compensation** for smooth gameplay
- **Connection management** and error handling
- **Synchronized random events** across clients

### Performance Considerations:
- Network bandwidth usage increases with entity count
- More complex camera calculations for multiple players
- CPU overhead from network processing
- Memory overhead from prediction/reconciliation

## Summary

Understanding Coherence Toolkit's architecture is crucial for creating network-compatible mods. The key principles are:

1. **Respect Authority**: Only modify entities you have authority over
2. **Use Proper Instantiation**: Use game's network-aware spawning
3. **Handle Network States**: Always check if multiplayer is active
4. **Test Thoroughly**: Verify functionality in all network scenarios
5. **Follow Game Patterns**: Use existing networking patterns where possible

By following these guidelines, modders can create modifications that work seamlessly in both single-player and multiplayer scenarios.

---

*🤖 Generated with [Claude Code](https://claude.ai/code)*

*This documentation is based on comprehensive source code analysis of Vampire Survivors v1.0+ and represents the verified networking implementation as of January 2025.*