# Visual Effects System

Based on analysis of decompiled IL2CPP code, the visual effects system appears to manage particle effects, animations, and visual feedback through specialized managers.

## ParticleManager

The decompiled code indicates particle effect management with automatic pause handling.

### Properties

```csharp
public class ParticleManager : Il2CppSystem.Object
{
    public SignalBus _signalBus;                           // Event system integration
    public List<ParticleSystem> _registeredParticleSystems; // All tracked particles
    public List<ParticleSystem> _pausedParticleSystems;     // Paused during game pause
    public bool _wasPaused;                                 // Pause state tracking
    public float _time;                                     // Time tracking for effects
    public int _shaderParam;                                // Shader parameter control
}
```

### Methods

#### Lifecycle
- `Initialize()` - Sets up particle tracking and shader parameters
- `Dispose()` - Cleans up registered particle systems
- `Tick()` - Updates particle effects and time-based parameters

#### Registration
- `RegisterParticleSystem(ParticleSystem particleSystem)` - Registers single particle system
- `RegisterParticleSystem(Il2CppReferenceArray<ParticleSystem> particleSystems)` - Bulk registration

#### Pause System
- `PauseGame()` - Pauses all registered particle systems
- `UnpauseGame()` - Resumes all registered particle systems

### Access Pattern

```csharp
ParticleManager particleManager = GM.Core.ParticleManager;
```

### Features

Based on the decompiled code, this system appears to provide:

- Tracking of game particle systems
- Automatic pause/unpause handling for visual consistency
- Shader parameter management for particle effects
- Time synchronization for particle animations

## GizmoManager

Based on code analysis, this appears to be a visual feedback system for level-ups, icons, and special effects.

### Properties

```csharp
public class GizmoManager : Il2CppSystem.Object
{
    // Positioning configuration
    public float AngelYOffset;                    // Angel/revival effect positioning
    public float IconYOffset;                     // Icon display positioning  
    public float LevelUpYOffset;                  // Level up effect positioning
    
    // Dependencies
    public GameSessionData _gameSessionData;
    
    // Particle systems
    public GameObject _particlesObject;
    public ParticleEmitterManager _particleEmitterManager;
    public ParticleSystem _pfxEmitter;            // General effects
    public ParticleSystem _quickTreasureEmitter;  // Treasure chest effects
    
    // Visual elements
    public List<Sprite> _angelFrames;             // Angel animation frames
    public PhaserSprite _highlight;               // Highlight effects
    public PhaserSprite _rainbow;                 // Rainbow effects
    
    // Animations
    public MultiTargetTween _highlightTween;
    public MultiTargetTween _highlightTween2;
    public MultiTargetTween _rainbowTween;
    public MultiTargetTween _rainbowTween2;
}
```

### Methods

#### Lifecycle
- `Initialize()` - Sets up visual effect systems
- `Dispose()` - Cleans up effect systems and tweens
- `Tick()` - Updates active visual effects

#### Visual Effects
- `ShowHighlightAt(float x, float y)` - Display highlight effect at position (27+ callers)
- `DisplayLevelUp(CharacterController character)` - Show level up animation (5 callers)
- `DisplayLimitBreakLevelUp(CharacterController character)` - Limit break level up effect
- `DisplayWeaponLevelup(CharacterController character)` - Weapon level up effect (3 callers)
- `DisplayMultiplayerRevive(CharacterController character)` - Multiplayer revival effect

#### Icon Display
- `DisplayWeaponIconOverhead(WeaponType weaponType, string value, Color? color, CharacterController character, float displayTimeMultiplier = 1f, Vector2 vOffset = default)` - Weapon icon display (16+ callers)
- `DisplayIconOverhead(string frameName, string value, Color? color, CharacterController character, float displayTimeMultiplier = 1f, Vector2 vOffset = default, string textureName = "items")` - General icon display (23+ callers)

#### Special Effects
- `DisplayQuickTreasureChestAnimation(CharacterController character)` - Treasure chest animation
- `DisplayAngel(CharacterController character)` - Angel/revival visual effect

### Features

Based on code analysis, this system appears to provide:

- Character-centric visual feedback
- Multiplayer-aware revival effects
- Configurable Y-offsets for different effect types
- Tween-based animations for transitions
- Usage throughout the game (50+ call sites identified in decompiled code)

## Integration with Game Systems

### Dependency Injection

Both managers are injected via GameManager's constructor:

```csharp
public void Construct(
    // ... other dependencies ...
    GizmoManager gizmoManager,
    ParticleManager particleManager,
    // ... other dependencies ...
)
```

### Access Patterns

```csharp
// Direct property access
ParticleManager particleManager = GM.Core.ParticleManager;

// GizmoManager typically accessed through dependency injection
```

### Initialization Order

1. ParticleManager.Initialize() - Set up particle tracking
2. GizmoManager.Initialize() - Set up visual effect systems
3. Both managers receive Tick() calls every frame
4. Dispose() called on cleanup

## Multiplayer Considerations

### GizmoManager

Based on decompiled code analysis:

- `DisplayMultiplayerRevive()` appears to be for online play
- Visual effects appear to be client-side
- No apparent network synchronization for particle effects

### ParticleManager

Inferred from code structure:

- Particle effects appear to be handled locally on each client
- Pause state appears to be managed independently per client
- No apparent network traffic for visual effects

## Modding Reference

### Custom Particle Effects

Based on the decompiled code structure, you can patch particle system registration:

```csharp
[HarmonyPatch(typeof(ParticleManager), "RegisterParticleSystem")]
public static void Postfix(ParticleManager __instance, ParticleSystem particleSystem)
{
    if (particleSystem.name.Contains("CustomMod"))
    {
        MyModParticleTracker.Register(particleSystem);
    }
}
```

### Custom Visual Feedback

The decompiled code suggests this pattern for custom effects:

```csharp
public class CustomEffectMod : MelonMod
{
    private GizmoManager gizmoManager;
    
    public override void OnLateInitializeMelon()
    {
        gizmoManager = GM.Core._gizmoManager;
    }
    
    public void ShowCustomEffect(CharacterController character)
    {
        gizmoManager.DisplayLevelUp(character);
        
        var pos = character.transform.position;
        gizmoManager.ShowHighlightAt(pos.x, pos.y);
        
        gizmoManager.DisplayIconOverhead(
            "star", 
            "EPIC!", 
            Color.gold, 
            character, 
            2.0f,
            Vector2.up * 0.5f
        );
    }
}
```

### Multiplayer-Aware Effects

The decompiled code indicates multiplayer awareness:

```csharp
[HarmonyPatch(typeof(GizmoManager), "DisplayLevelUp")]
public static void Postfix(GizmoManager __instance, CharacterController character)
{
    if (GM.Core.IsOnlineMultiplayer)
    {
        // Network-aware modifications
    }
    else if (GM.Core.IsLocalMultiplayer) 
    {
        // Local coop modifications
    }
    else
    {
        // Single player modifications
    }
}
```

### Memory Management

Based on code analysis, cleanup appears necessary:

```csharp
public override void OnDeinitializeMelon()
{
    if (customParticleSystem != null)
    {
        particleManager.UnregisterParticleSystem(customParticleSystem);
        UnityEngine.Object.Destroy(customParticleSystem.gameObject);
    }
}
```

## Implementation Patterns

### Creating Custom Particles

The decompiled code suggests this approach:

```csharp
private void SetupCustomParticles()
{
    var particleObject = new GameObject("CustomModParticles");
    customParticleSystem = particleObject.AddComponent<ParticleSystem>();
    
    var main = customParticleSystem.main;
    main.startColor = Color.magenta;
    main.startSpeed = 5.0f;
    
    GM.Core.ParticleManager.RegisterParticleSystem(customParticleSystem);
}
```

### Accessing Effects

Based on the decompiled structure:

```csharp
public static void AccessEffect(CharacterController character)
{
    var gizmoManager = GM.Core?._gizmoManager;
    if (gizmoManager == null) return;
    
    if (GM.Core.IsMultiplayer)
    {
        gizmoManager.DisplayMultiplayerRevive(character);
    }
    else
    {
        gizmoManager.DisplayLevelUp(character);
    }
}
```

## Implementation Notes

Based on decompiled IL2CPP code analysis:

- Register custom particle systems with ParticleManager for pause handling
- Check multiplayer state before applying visual modifications
- Use late initialization hooks to ensure managers are available
- Dispose of custom effects to prevent memory leaks
- Visual effects appear to be client-side only
- Test effects in different game modes (single player, local coop, online multiplayer)