# Visual Effects System

The visual effects system manages particle effects, animations, and visual feedback through specialized managers.

## ParticleManager

Centralized particle effect management with automatic pause handling.

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

- Centralized tracking of all game particle systems
- Automatic pause/unpause handling for visual consistency
- Global shader parameter management for particle effects
- Time synchronization for particle animations

## GizmoManager

Comprehensive visual feedback system for level-ups, icons, and special effects.

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

- Character-centric visual feedback
- Multiplayer-aware revival effects
- Configurable Y-offsets for different effect types
- Tween-based animations for smooth transitions
- Extensive usage throughout the game (50+ call sites)

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

- `DisplayMultiplayerRevive()` specifically for online play
- Visual effects are client-side for performance
- No network synchronization for particle effects

### ParticleManager

- Particle effects handled locally on each client
- Pause state managed independently per client
- No network traffic for visual effects

## Modding Guidelines

### Custom Particle Effects

```csharp
[HarmonyPatch(typeof(ParticleManager), "RegisterParticleSystem")]
public static void Postfix(ParticleManager __instance, ParticleSystem particleSystem)
{
    // Track custom mod particles
    if (particleSystem.name.Contains("CustomMod"))
    {
        MyModParticleTracker.Register(particleSystem);
    }
}
```

### Custom Visual Feedback

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
        // Standard level up
        gizmoManager.DisplayLevelUp(character);
        
        // Custom highlight
        var pos = character.transform.position;
        gizmoManager.ShowHighlightAt(pos.x, pos.y);
        
        // Custom icon
        gizmoManager.DisplayIconOverhead(
            "star", 
            "EPIC!", 
            Color.gold, 
            character, 
            2.0f, // Double display time
            Vector2.up * 0.5f
        );
    }
}
```

### Multiplayer-Aware Effects

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

```csharp
public override void OnDeinitializeMelon()
{
    if (customParticleSystem != null)
    {
        // Unregister from manager
        particleManager.UnregisterParticleSystem(customParticleSystem);
        
        // Destroy GameObject
        UnityEngine.Object.Destroy(customParticleSystem.gameObject);
    }
}
```

## Common Patterns

### Creating Custom Particles

```csharp
private void SetupCustomParticles()
{
    var particleObject = new GameObject("CustomModParticles");
    customParticleSystem = particleObject.AddComponent<ParticleSystem>();
    
    // Configure particle system
    var main = customParticleSystem.main;
    main.startColor = Color.magenta;
    main.startSpeed = 5.0f;
    
    // Register for pause handling
    GM.Core.ParticleManager.RegisterParticleSystem(customParticleSystem);
}
```

### Safe Effect Access

```csharp
public static void SafeShowEffect(CharacterController character)
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

## Best Practices

- Always register custom particle systems with ParticleManager for proper pause handling
- Check multiplayer state before applying visual modifications
- Use late initialization hooks to ensure managers are available
- Properly dispose of custom effects to prevent memory leaks
- Visual effects are client-side only - don't rely on them for gameplay logic
- Test effects in all three game modes (single player, local coop, online multiplayer)