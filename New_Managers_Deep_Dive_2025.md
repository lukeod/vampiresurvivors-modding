# Vampire Survivors - New Managers & Factories Deep Dive (2025)
## Complete Analysis of ParticleManager, GizmoManager, ShopFactory & FontFactory

This document provides a comprehensive technical analysis of the four new managers/factories introduced in Vampire Survivors v1.0+, validated through deep source code examination.

---

## Table of Contents

1. [ParticleManager - Complete Analysis](#particlemanager---complete-analysis)
2. [GizmoManager - Visual Effects System](#gizmomanager---visual-effects-system)  
3. [ShopFactory - Inventory Generation](#shopfactory---inventory-generation)
4. [FontFactory - Asset Management](#fontfactory---asset-management)
5. [Integration & Lifecycle](#integration--lifecycle)
6. [Modding Guidelines](#modding-guidelines)
7. [Usage Examples](#usage-examples)

---

## ParticleManager - Complete Analysis

**File Location**: `F:\vampire\melon_decompiled\Il2CppVampireSurvivors.Runtime\Il2CppVampireSurvivors\ParticleManager.cs`  
**Namespace**: `Il2CppVampireSurvivors`  
**Pattern**: Full Manager (Initialize/Dispose/Tick)

### Core Properties

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

### API Reference

#### Lifecycle Methods
- **`Initialize()`** (Line 197): Sets up particle tracking and shader parameters
- **`Dispose()`** (Line 208): Cleans up registered particle systems
- **`Tick()`** (Line 219): Updates particle effects and time-based parameters

#### Particle Registration
- **`RegisterParticleSystem(ParticleSystem particleSystem)`** (Line 173): Registers single particle system
- **`RegisterParticleSystem(Il2CppReferenceArray<ParticleSystem> particleSystems)`** (Line 185): Bulk registration

#### Pause/Unpause System
- **`PauseGame()`** (Line 162): Pauses all registered particle systems
- **`UnpauseGame()`** (Line 151): Resumes all registered particle systems

### Key Features

1. **Centralized Particle Tracking**: All game particle systems register with this manager
2. **Pause Integration**: Automatically handles game pause/unpause for visual consistency
3. **Shader Parameter Control**: Manages global shader parameters for particle effects
4. **Time Management**: Tracks time for particle effect synchronization

### Access Pattern
```csharp
// Via GameManager dependency injection
ParticleManager particleManager = GM.Core.ParticleManager;
```

### Multiplayer Considerations
- No specific multiplayer handling detected in the source
- Particles likely handled client-side for performance
- No network synchronization for particle effects

---

## GizmoManager - Visual Effects System

**File Location**: `F:\vampire\melon_decompiled\Il2CppVampireSurvivors.Runtime\Il2CppVampireSurvivors.Framework\GizmoManager.cs`  
**Namespace**: `Il2CppVampireSurvivors.Framework`  
**Pattern**: Full Manager (Initialize/Dispose/Tick)

### Core Properties

```csharp
public class GizmoManager : Il2CppSystem.Object
{
    // Positioning configuration
    public float AngelYOffset;                    // Angel/revival effect positioning
    public float IconYOffset;                     // Icon display positioning  
    public float LevelUpYOffset;                  // Level up effect positioning
    
    // Dependencies
    public GameSessionData _gameSessionData;
    
    // Particle systems for effects
    public GameObject _particlesObject;
    public ParticleEmitterManager _particleEmitterManager;
    public ParticleSystem _pfxEmitter;            // General effects
    public ParticleSystem _quickTreasureEmitter;  // Treasure chest effects
    
    // Visual elements
    public List<Sprite> _angelFrames;             // Angel animation frames
    public PhaserSprite _highlight;               // Highlight effects
    public PhaserSprite _rainbow;                 // Rainbow effects
    
    // Tween animations
    public MultiTargetTween _highlightTween;
    public MultiTargetTween _highlightTween2;
    public MultiTargetTween _rainbowTween;
    public MultiTargetTween _rainbowTween2;
}
```

### API Reference

#### Lifecycle Methods
- **`Initialize()`** (Line 594): Sets up visual effect systems
- **`Dispose()`** (Line 605): Cleans up effect systems and tweens
- **`Tick()`** (Line 616): Updates active visual effects

#### Visual Effect Methods
- **`ShowHighlightAt(float x, float y)`** (Line 627): Display highlight effect at position
- **`DisplayLevelUp(CharacterController character)`** (Line 640): Show level up animation
- **`DisplayLimitBreakLevelUp(CharacterController character)`** (Line 652): Limit break level up effect
- **`DisplayWeaponLevelup(CharacterController character)`** (Line 676): Weapon level up effect
- **`DisplayMultiplayerRevive(CharacterController character)`** (Line 664): **Multiplayer revival effect**

#### Icon Display System
- **`DisplayWeaponIconOverhead(WeaponType weaponType, string value, Color? color, CharacterController character, float displayTimeMultiplier = 1f, Vector2 vOffset = default)`** (Line 688): Weapon-specific icon display
- **`DisplayIconOverhead(string frameName, string value, Color? color, CharacterController character, float displayTimeMultiplier = 1f, Vector2 vOffset = default, string textureName = "items")`** (Line 705): General icon display

#### Special Effects
- **`DisplayQuickTreasureChestAnimation(CharacterController character)`** (Line 723): Treasure chest opening animation
- **`DisplayAngel(CharacterController character)`** (Line 768): Angel/revival visual effect

#### Internal Setup Methods
- **`Init()`** (Line 735): Internal initialization
- **`InitLevelUp()`** (Line 746): Level up effect setup
- **`InitQuickTreasureChest()`** (Line 757): Treasure chest effect setup

### Key Features

1. **Comprehensive Visual Feedback**: Handles all major game visual cues
2. **Character-Centric Effects**: Most effects target specific characters
3. **Multiplayer Revival Support**: Dedicated revival effect for multiplayer
4. **Configurable Positioning**: Adjustable Y-offsets for different effect types
5. **Animation System**: Full tween-based animation support

### Usage Examples (Verified from Source)

The GizmoManager is called heavily throughout the game (27 references to ShowHighlightAt alone):

```csharp
// Level up effects - 5 callers
gizmoManager.DisplayLevelUp(characterController);

// Weapon level effects - 3 callers  
gizmoManager.DisplayWeaponLevelup(characterController);

// Icon displays - 16+ callers for weapon icons, 23+ for general icons
gizmoManager.DisplayWeaponIconOverhead(WeaponType.Whip, "+1", Color.white, player);

// Multiplayer revival - dedicated for online play
gizmoManager.DisplayMultiplayerRevive(deadPlayer);
```

---

## ShopFactory - Inventory Generation

**File Location**: `F:\vampire\melon_decompiled\Il2CppVampireSurvivors.Runtime\Il2CppVampireSurvivors.Framework\ShopFactory.cs`  
**Namespace**: `Il2CppVampireSurvivors.Framework`  
**Pattern**: Factory Pattern

### Core Properties

```csharp
public class ShopFactory : Il2CppSystem.Object
{
    public DataManager _data;                    // Game data access
    public PlayerOptions _playerOptions;        // Player configuration
    public List<WeaponType> _availableWeapons;   // Current weapon inventory
    public List<ItemType> _availableItems;      // Current item inventory
}
```

### API Reference

#### Public Properties
- **`List<WeaponType> AvailableWeapons { get; }`** (Line 171): Current weapon inventory (738 references!)
- **`List<ItemType> AvailableItems { get; }`** (Line 186): Current item inventory (30 references)

#### Core Generation Methods
- **`GenerateShopInventory(CharacterController player)`** (Line 226): Main inventory generation (2 callers)
- **`InjectRemoteShop(List<WeaponType> weapons, List<ItemType> items)`** (Line 237): **Multiplayer shop injection**

#### Validation Methods  
- **`DoesPlayerAlreadyHaveWeapon(WeaponType t)`** (Line 289): Weapon ownership check (8 callers)

#### Static Validation Helpers
- **`GetValidAdventureWeaponsForMerchant(List<WeaponType> merchantInventory, PlayerOptions playerOptions)`** (Line 250): Adventure mode filtering (2 callers)
- **`GetValidCustomMerchantWeapons(List<WeaponType> merchantInventory, PlayerOptions playerOptions)`** (Line 263): Custom merchant filtering (1 caller)
- **`GetValidCustomMerchantItems(List<ItemType> merchantInventoryItems, PlayerOptions playerOptions)`** (Line 276): Custom item filtering (1 caller)

#### Internal Generation Methods
- **`MakeCustomInventory()`** (Line 302): Custom shop generation
- **`MakeStandardInventory(CharacterController player)`** (Line 313): Standard shop generation  
- **`MakeArcanaInventory()`** (Line 325): Arcana shop generation
- **`MakeEggsInventory(CharacterController player)`** (Line 336): Egg shop generation

### Key Features

1. **Multiple Shop Types**: Standard, Custom, Arcana, and Egg inventories
2. **Player-Aware Generation**: Considers player progress and unlocks
3. **Adventure Mode Support**: Special filtering for adventure mode merchants
4. **Multiplayer Integration**: Remote shop injection for online play
5. **Validation System**: Prevents duplicate weapon purchases

### Multiplayer Considerations

The `InjectRemoteShop` method (Line 237) is specifically designed for online multiplayer:
- Allows host to inject shop inventory to other players
- Ensures synchronized shop contents across all players
- Critical for maintaining game balance in online sessions

### Usage Pattern (Verified)

```csharp
// Used in MerchantUIPage constructor (Line 1239)
public void Construct(..., ShopFactory shopFactory)

// Primary usage in UI systems
shopFactory.GenerateShopInventory(player);

// Multiplayer synchronization  
shopFactory.InjectRemoteShop(hostWeapons, hostItems);
```

---

## FontFactory - Asset Management

**File Location**: `F:\vampire\melon_decompiled\Il2CppVampireSurvivors.Runtime\Il2CppVampireSurvivors.App.Scripts.Framework\FontFactory.cs`  
**Namespace**: `Il2CppVampireSurvivors.App.Scripts.Framework`  
**Pattern**: ScriptableObject Factory

### Core Properties

```csharp
public class FontFactory : SerializedScriptableObject
{
    public UnityFontRefDictionary _Fonts;        // Unity font references
    public TMPFontRefDictionary _TMPFonts;       // TextMeshPro font references
}
```

### Nested Data Structures

#### UnityFontRefData
```csharp
public class UnityFontRefData : Il2CppSystem.Object
{
    public AssetReferenceT<Font> UnityFontRef { get; set; }
}
```

#### TMPFontRefData  
```csharp
public class TMPFontRefData : Il2CppSystem.Object
{
    public AssetReferenceT<TMP_FontAsset> TMPFontRef { get; set; }
}
```

### API Reference

#### Font Access Methods
- **`Font GetFont(string fontName)`** (Line 287): Retrieve Unity font by name
- **`TMP_FontAsset GetTMPFont(string fontName)`** (Line 300): Retrieve TextMeshPro font by name

### Key Features

1. **Dual Font System**: Support for both Unity fonts and TextMeshPro
2. **Addressable Asset Integration**: Uses Unity's Addressable system for asset loading
3. **ScriptableObject Pattern**: Configuration-driven font management
4. **String-Based Access**: Simple name-based font retrieval

### Asset Management

The FontFactory uses Unity's Addressable Asset system:
- **`AssetReferenceT<Font>`**: Type-safe Unity font references
- **`AssetReferenceT<TMP_FontAsset>`**: Type-safe TextMeshPro font references
- Allows for efficient asset loading and memory management

### Usage Pattern

```csharp
// Access via GameManager
FontFactory fontFactory = GM.Core.FontFactory;

// Get Unity font
Font myFont = fontFactory.GetFont("MenuFont");

// Get TextMeshPro font  
TMP_FontAsset tmpFont = fontFactory.GetTMPFont("UIFont");
```

### Modding Implications

Modders can potentially:
- Add custom fonts through the Addressable system
- Override existing font references
- Implement custom font loading logic

---

## Integration & Lifecycle

### Dependency Injection Setup

All four managers are injected via Zenject in the GameManager constructor (Line 8504):

```csharp
public void Construct(
    SignalBus signalBus,
    DiContainer diContainer, 
    PlayerOptions playerOptions,
    LootManager lootManager,
    WeaponsFacade weaponsFacade,
    Stage stage,
    GameSessionData gameSessionData,
    LevelUpFactory levelUpFactory,
    CharacterFactory characterFactory,
    AccessoriesFacade accessoriesFacade,
    DataManager dataManager,
    PlayerStats playerStats,
    ArcanaManager arcanaManager,
    PhysicsManager physicsManager,
    EggManager egg,
    LimitBreakManager limitBreakManager,
    GizmoManager gizmoManager,           // ✓ New manager
    TreasureFactory treasureFactory,
    ProjectileFactory projectileFactory,
    SpellsManager spellsManager,
    AchievementManager achievementManager,
    MainGamePage mainGamePage,
    MultiplayerManager multiplayer,
    AdventureManager adventureManager,
    FontFactory fontFactory,             // ✓ New factory
    AssetReferenceLibrary assetReferenceLibrary,
    ParticleManager particleManager,     // ✓ New manager
    ShopFactory shopFactory              // ✓ New factory
)
```

### Access Patterns

#### Via GameManager Properties
```csharp
// Direct property access
ParticleManager particleManager = GM.Core.ParticleManager;  // Line 6577
ShopFactory shopFactory = GM.Core.ShopFactory;              // Line 6487
FontFactory fontFactory = GM.Core.FontFactory;              // Line 6504

// GizmoManager is accessed through DI in other managers
```

#### Via Constructor Injection
```csharp
// In MerchantUIPage (Line 1239)
public void Construct(SignalBus signalBus, DataManager data, 
    PlayerOptions playerOptions, GameSessionData session, 
    EggManager egg, AdventureManager adventureManager, 
    ShopFactory shopFactory)

// In other UI and game systems
```

### Initialization Order

1. **FontFactory**: Loaded as ScriptableObject asset
2. **ParticleManager**: Initialize() called during game setup
3. **GizmoManager**: Initialize() called during game setup  
4. **ShopFactory**: Created and injected, no special initialization

### Lifecycle Management

#### Full Managers (ParticleManager, GizmoManager)
```csharp
Initialize() → Tick() (every frame) → Dispose() (on cleanup)
```

#### Factories (ShopFactory, FontFactory)
- **ShopFactory**: Stateful, maintains inventory lists
- **FontFactory**: Stateless, provides asset access

---

## Modding Guidelines

### Multiplayer Awareness

**Critical**: All mods must be multiplayer-aware when interacting with these managers:

```csharp
[HarmonyPatch(typeof(GizmoManager), "DisplayLevelUp")]
public static void Postfix(GizmoManager __instance, CharacterController character)
{
    var gameManager = GM.Core;
    
    if (gameManager.IsOnlineMultiplayer)
    {
        // Network-aware modifications
        // Consider latency and host authority
    }
    else if (gameManager.IsLocalMultiplayer) 
    {
        // Local coop modifications
        // Handle multiple players on same screen
    }
    else
    {
        // Single player modifications
    }
}
```

### Safe Access Patterns

```csharp
// Always check for null and multiplayer state
public static void SafeShowEffect(CharacterController character)
{
    var gizmoManager = GM.Core?._gizmoManager;
    if (gizmoManager == null) return;
    
    var multiplayerManager = GM.Core?._multiplayerManager;
    if (multiplayerManager?.IsMultiplayer == true)
    {
        // Multiplayer-safe effect
        gizmoManager.DisplayMultiplayerRevive(character);
    }
    else
    {
        // Standard effect
        gizmoManager.DisplayLevelUp(character);
    }
}
```

### ParticleManager Modding

```csharp
[HarmonyPatch(typeof(ParticleManager), "RegisterParticleSystem")]
public static void Postfix(ParticleManager __instance, ParticleSystem particleSystem)
{
    // Add custom particle tracking
    if (particleSystem.name.Contains("CustomMod"))
    {
        // Custom handling for mod particles
        MyModParticleTracker.Register(particleSystem);
    }
}
```

### ShopFactory Modding

```csharp
[HarmonyPatch(typeof(ShopFactory), "GenerateShopInventory")]
public static void Postfix(ShopFactory __instance, CharacterController player)
{
    // Check multiplayer state before modifications
    if (GM.Core.IsOnlineMultiplayer)
    {
        // In online multiplayer, shop injection should be host-controlled
        return;
    }
    
    // Safe to modify shop in single player or local coop
    var customWeapons = GetCustomModWeapons();
    foreach (var weapon in customWeapons)
    {
        if (!__instance.DoesPlayerAlreadyHaveWeapon(weapon))
        {
            __instance.AvailableWeapons.Add(weapon);
        }
    }
}
```

### FontFactory Modding

```csharp
[HarmonyPatch(typeof(FontFactory), "GetTMPFont")]
public static void Postfix(FontFactory __instance, string fontName, ref TMP_FontAsset __result)
{
    // Override specific fonts with custom ones
    if (fontName == "UIFont" && MyModSettings.UseCustomFont)
    {
        __result = MyCustomFontAsset;
    }
}
```

---

## Usage Examples

### Creating Custom Visual Effects

```csharp
public class CustomEffectMod : MelonMod
{
    private GizmoManager gizmoManager;
    
    public override void OnLateInitializeMelon()
    {
        gizmoManager = GM.Core._gizmoManager;
    }
    
    public void ShowCustomLevelUpEffect(CharacterController character)
    {
        // Standard level up
        gizmoManager.DisplayLevelUp(character);
        
        // Custom highlight at character position
        var pos = character.transform.position;
        gizmoManager.ShowHighlightAt(pos.x, pos.y);
        
        // Custom icon overhead
        gizmoManager.DisplayIconOverhead(
            "star", 
            "EPIC!", 
            Color.gold, 
            character, 
            2.0f, // Double display time
            Vector2.up * 0.5f // Slight offset
        );
    }
}
```

### Custom Shop Modifications

```csharp
public class CustomShopMod : MelonMod
{
    [HarmonyPatch(typeof(MerchantUIPage), "Awake")]
    public static void Postfix(MerchantUIPage __instance)
    {
        var shopFactory = __instance._shopFactory;
        
        // Only modify in single player
        if (!GM.Core.IsMultiplayer)
        {
            AddCustomShopItems(shopFactory);
        }
    }
    
    private static void AddCustomShopItems(ShopFactory shopFactory)
    {
        // Add custom weapons if player meets requirements
        if (GM.Core._data._unlockedCharacters.Contains("CustomCharacter"))
        {
            shopFactory.AvailableWeapons.Add(WeaponType.CustomWeapon);
        }
    }
}
```

### Particle System Integration

```csharp
public class ParticleEffectMod : MelonMod
{
    private ParticleManager particleManager;
    private ParticleSystem customParticleSystem;
    
    public override void OnLateInitializeMelon()
    {
        particleManager = GM.Core.ParticleManager;
        SetupCustomParticles();
    }
    
    private void SetupCustomParticles()
    {
        // Create custom particle system
        var particleObject = new GameObject("CustomModParticles");
        customParticleSystem = particleObject.AddComponent<ParticleSystem>();
        
        // Configure particle system...
        var main = customParticleSystem.main;
        main.startColor = Color.magenta;
        main.startSpeed = 5.0f;
        
        // Register with ParticleManager for pause handling
        particleManager.RegisterParticleSystem(customParticleSystem);
    }
    
    public void TriggerCustomEffect(Vector3 position)
    {
        customParticleSystem.transform.position = position;
        customParticleSystem.Play();
    }
}
```

### Custom Font Management

```csharp
public class FontReplacementMod : MelonMod
{
    [HarmonyPatch(typeof(FontFactory), "GetTMPFont")]
    public static void Postfix(FontFactory __instance, string fontName, ref TMP_FontAsset __result)
    {
        switch (fontName)
        {
            case "DamageFont":
                if (ModSettings.UseHighContrastDamage)
                {
                    __result = LoadCustomDamageFont();
                }
                break;
                
            case "UIFont":
                if (ModSettings.UseDyslexiaFriendlyFont)
                {
                    __result = LoadDyslexiaFriendlyFont();
                }
                break;
        }
    }
    
    private static TMP_FontAsset LoadCustomDamageFont()
    {
        // Load custom font asset
        // Implementation depends on how you bundle your assets
        return Resources.Load<TMP_FontAsset>("CustomDamageFont");
    }
}
```

---

## Common Pitfalls & Solutions

### 1. Multiplayer Synchronization Issues

**Problem**: Visual effects or shop modifications only work for one player

**Solution**: Always check multiplayer state and use appropriate methods

```csharp
// Wrong - will desync in multiplayer
gizmoManager.DisplayLevelUp(character);

// Right - multiplayer aware
if (GM.Core.IsMultiplayer)
{
    gizmoManager.DisplayMultiplayerRevive(character);
}
else 
{
    gizmoManager.DisplayLevelUp(character);
}
```

### 2. Null Reference Exceptions

**Problem**: Managers not available during early initialization

**Solution**: Use late initialization hooks

```csharp
public override void OnLateInitializeMelon()
{
    // Safe to access managers here
    var particleManager = GM.Core.ParticleManager;
}
```

### 3. Memory Leaks in Particle Systems

**Problem**: Custom particle systems not properly disposed

**Solution**: Implement proper cleanup

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

---

## Conclusion

The four new managers represent a significant expansion of Vampire Survivors' architecture:

- **ParticleManager**: Centralized particle effect management with pause integration
- **GizmoManager**: Comprehensive visual feedback system with multiplayer support  
- **ShopFactory**: Intelligent inventory generation with multiplayer synchronization
- **FontFactory**: Streamlined font asset management system

All systems are designed with multiplayer in mind and follow established patterns. Modders should prioritize multiplayer compatibility and use the provided APIs rather than bypassing these systems.

The extensive usage patterns (738 references to ShopFactory.AvailableWeapons, 27+ to GizmoManager.ShowHighlightAt) demonstrate these are core systems that many game features depend on.

**Remember**: Always test mods in all three game modes (Single Player, Local Coop, Online Multiplayer) to ensure compatibility.