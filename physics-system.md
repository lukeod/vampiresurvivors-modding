# Physics System

Based on code analysis, Vampire Survivors appears to use a custom physics engine (ArcadePhysics) for collision detection and movement, integrated with Unity's component system. All information in this document is inferred from decompiled IL2CPP code.

**Note**: This documentation reflects the Unity 6000.0.36f1 implementation observed in the decompiled code. Collision callback signatures and some method parameters appear to have been updated from previous versions.

## Core Components

### PhysicsManager (Singleton)
**Location**: `Il2CppVampireSurvivors.Framework.PhysicsManager`

Appears to function as a central physics management singleton based on code analysis:

```csharp
// Singleton access
PhysicsManager._sInstance
PhysicsManager.Instance // Property accessor
```

#### Physics Groups
Based on the decompiled code, manages different collision groups for game objects:
- `_playerGroup` - Player characters
- `_playersWithWallCollisionGroup` - Players that collide with walls
- `_enemyGroup` - Enemy characters
- `_bulletGroup` - Projectiles and bullets
- `_pickupGroup` - Collectible items
- `_goToPlayerPickupGroup` - Attracted pickups (magnet effect)
- `_destructiblesGroup` - Destructible objects
- `_magnetGroup` - Magnet collision detection
- `_doorGroup` - Door objects

#### Key Methods
- `InitPhysicsGroups(GameManager gameManager)` - Initializes all physics groups
- `InitPhysicsColliders()` - Sets up collision detection between groups

#### Collision Callbacks
- `OnPlayerOverlapsEnemy(CallbackContext context, ArcadeColliderType first, ArcadeColliderType second) : bool` - Player-enemy collision
- `OnPlayerOverlapsPickup(CallbackContext context, ArcadeColliderType player, ArcadeColliderType pickup) : bool` - Player-pickup collection
- `OnMagnetOverlapsPickup(CallbackContext context, ArcadeColliderType magnet, ArcadeColliderType pickup) : bool` - Magnet attraction
- `OnBulletOverlapsDoor(CallbackContext context, ArcadeColliderType bullet, ArcadeColliderType door) : bool` - Projectile-door interaction

#### Additional Properties
- `PickupImmaterial : bool` - Controls whether pickup collision is enabled

### ArcadePhysics System
**Location**: `Il2Cpp.ArcadePhysics`

Based on code analysis, appears to be a custom physics engine implementation:

```csharp
// Static access points
ArcadePhysics.Instance       // Singleton instance property
ArcadePhysics.scene          // PhaserScene reference property
ArcadePhysics.s_world        // Physics world static field
ArcadePhysics.Config         // Physics configuration property
```

#### Collision Detection Methods
- `OverlapRect(float x, float y, float width, float height, bool includeDynamic = true, bool includeStatic = false, Group specificGroup = null) : List<BaseBody>` - Rectangle collision detection
- `OverlapCirc(float x, float y, float radius, bool includeDynamic = true, bool includeStatic = false, Group specificGroup = null) : List<BaseBody>` - Circle collision detection
- `OverlapLine(float2 lineStart, float2 lineEnd, float lineWidth, bool includeDynamic = true, bool includeStatic = false, Group specificGroup = null) : List<BaseBody>` - Line collision detection
- `CircleToCircle(ArcadeCircle a, ArcadeCircle b) : bool` - Circle-to-circle collision test
- `CircleToRectangle(ArcadeCircle circle, ArcadeRect rect) : bool` - Circle-to-rectangle collision test

## Physics Body Hierarchy

### BaseBody
**Location**: `Il2CppPhaserPort.BaseBody`

Based on the decompiled code, serves as the base physics body with core properties:

```csharp
public class BaseBody : RBush.IRectangular
{
    // Physics type
    public PhysicsType _physicsType;  // DYNAMIC_BODY, STATIC_BODY, UNDEFINED
    
    // Movement
    public float2 _velocity;
    public float2 _position;
    public float2 _offset;
    
    // Collision shape
    public float2 _size;
    public float2 _halfSize;
    public float2 _center;
    public bool _isCircle;
    public float _radius;
    
    // Physics properties
    public float _mass;
    public bool _immovable;
    public bool _pushable;
    public bool _collideWorldBounds;
    public bool _embedded;
    public bool _enable;
    
    // Gravity and bounce
    public bool _allowGravity;
    public float2 _gravity;
    public float2 _bounce;
    
    // Collision state
    public ArcadeBodyCollision _checkCollision;
    public ArcadeBodyCollision _blocked;
    
    // Callbacks
    public bool _onWorldBounds;
    public bool _onCollide;
    public bool _onOverlap;
    
    // Movement deltas
    public float _dx;
    public float _dy;
}
```

### ArcadeSprite
**Location**: `Il2Cpp.ArcadeSprite`

Based on code analysis, extends PhaserGameObject with physics integration:

```csharp
public class ArcadeSprite : PhaserGameObject
{
    public float2 position;  // Position property with getter/setter
    
    // Movement methods
    public void setVelocity(float xVel, Il2CppSystem.Nullable<float> yVel = null);
    public void setVelocity(Vector2 velocity);
    public void setBounce(float2 bounce);
    public void setCollideWorldBounds(bool value, Il2CppSystem.Nullable<float> bounceX = null, Il2CppSystem.Nullable<float> bounceY = null);
}
```

### PhysicsType Enum
```csharp
public enum PhysicsType
{
    DYNAMIC_BODY,    // Affected by physics
    STATIC_BODY,     // Static collision
    UNDEFINED        // Uninitialized
}
```

### Collision System Types

#### ArcadeColliderType
**Location**: `Il2Cpp.ArcadeColliderType`

Based on the decompiled code, appears to be an abstract base class for collision objects in callbacks:

```csharp
public abstract class ArcadeColliderType
{
    public virtual bool isParent { get; }
    public virtual BaseBody body { get; }
    public virtual bool isTilemap { get; }
    public virtual GameObject gameObject { get; }
}
```

#### CallbackContext
**Location**: `Il2Cpp.CallbackContext`

Based on code analysis, appears to be a context object passed to collision callbacks:

```csharp
public class CallbackContext
{
    // Empty context class for collision callbacks
}
```

## Physics Groups

### PhysicsGroup Class
**Location**: `Il2Cpp.PhysicsGroup`

Based on the decompiled code, functions as a container for physics objects:

```csharp
public class PhysicsGroup : Group
{
    public PhysicsGroup(int capacity);  // Constructor with capacity
    
    // Add/remove objects
    public void add(ArcadeSprite sprite);
    public void remove(ArcadeSprite sprite);
}
```

### BulletPool (Projectile Management)
**Location**: `Il2CppVampireSurvivors.Objects.Pools.BulletPool`

Based on code analysis, appears to be a specialized pool for projectiles:

```csharp
public class BulletPool : PhysicsGroup
{
    // Spawning
    public Projectile SpawnAt(float x, float y, Weapon weapon, int index);
    public Projectile SpawnAt(float2 pos, Weapon weapon, int index);
    
    // Pooling
    public void Return(Projectile projectile);
    
    // Properties
    public int AliveObjectsCount;
    public bool IsUncapped;
    public int UpperLimit;
}
```

## Projectile Integration

### Projectile Class
**Location**: `Il2CppVampireSurvivors.Objects.Projectiles.Projectile`

Based on the decompiled code, extends ArcadeSprite for weapon projectiles:

```csharp
public class Projectile : ArcadeSprite
{
    // Core properties
    public Weapon _weapon;
    public BulletPool _pool;
    public int _indexInWeapon;
    public float _speed;
    public int _penetrating;
    public int _bounces;
    
    // Properties
    public int IndexInWeapon { get; }
    public Weapon Weapon { get; }
    public float ProjectileSpeed { get; }
    
    // Initialization
    public virtual void InitProjectile(BulletPool pool, Weapon weapon, int index);
    
    // Lifecycle
    public virtual void InternalUpdate();
    // Note: Collision handling methods appear to have been updated in Unity 6 based on code analysis
}
```

### Integration Flow

Based on code analysis, the integration appears to follow this pattern:

```
1. Weapon.Fire() creates projectile
2. BulletPool.SpawnAt() retrieves from pool
3. Projectile.InitProjectile() sets up physics
4. PhysicsManager._bulletGroup.add() registers with physics
5. Physics world handles movement and collision
6. Collision callbacks trigger game logic
7. BulletPool.Return() recycles projectile
```

## Custom Physics Objects

### Creating Physics-Enabled Objects

```csharp
// 1. Extend ArcadeSprite
public class CustomPhysicsObject : ArcadeSprite
{
    public void Initialize()
    {
        // Access physics body through the body property
        var physicsBody = this.body;
        
        // Set physics type
        physicsBody._physicsType = PhysicsType.DYNAMIC_BODY;
        
        // Configure collision shape
        physicsBody._isCircle = false;
        physicsBody._size = new float2(32, 32);
        
        // Set physics properties
        physicsBody._mass = 1.0f;
        physicsBody._immovable = false;
        physicsBody._collideWorldBounds = true;
    }
}

// 2. Register with PhysicsManager
var customObject = new CustomPhysicsObject();
customObject.Initialize();
PhysicsManager.Instance._bulletGroup.add(customObject);

// 3. Set movement
customObject.setVelocity(100, 0);  // Move right at 100 units/sec

// 4. Handle collisions - Note: collision callback signatures appear to have changed
// Override appropriate collision methods as needed based on your requirements
```

### Custom Projectile Pool

```csharp
public class CustomProjectilePool : BulletPool
{
    public CustomProjectile SpawnCustom(float2 pos, Weapon weapon)
    {
        var projectile = GetFromPool() ?? CreateNew();
        
        // Initialize physics
        projectile.InitProjectile(this, weapon, GetNextIndex());
        
        // Set position
        projectile.position = pos;
        
        // Register with physics
        PhysicsManager.Instance._bulletGroup.add(projectile);
        
        return projectile;
    }
}
```

## Collision Detection

### Overlap Queries

```csharp
// Rectangle overlap
var hits = ArcadePhysics.Instance.OverlapRect(x, y, width, height, includeDynamic: true, includeStatic: false);

// Circle overlap
var hits = ArcadePhysics.Instance.OverlapCirc(centerX, centerY, radius, includeDynamic: true, includeStatic: false);

// Line overlap using float2
var hits = ArcadePhysics.Instance.OverlapLine(new float2(x1, y1), new float2(x2, y2), lineWidth: 1.0f);
```

### Collision Callbacks

```csharp
// Register collision callback - Note: callbacks appear to use different signatures based on code analysis
// These appear to be registered internally by the game
bool OnPlayerOverlapsEnemy(CallbackContext context, ArcadeColliderType player, ArcadeColliderType enemy)
{
    // Handle player-enemy collision
    var playerBody = player.body;
    var enemyBody = enemy.body;
    // Return true to continue processing, false to stop
    return true;
}

// Initialize collision detection between groups
PhysicsManager.Instance.InitPhysicsColliders();
```

## Performance Considerations

### Object Pooling
Based on code analysis:
- All projectiles appear to use BulletPool for memory efficiency
- Enemies appear to use similar pooling systems
- Pickups appear to be recycled through object pools

### Spatial Indexing
Based on the decompiled code:
- Appears to use RBush spatial tree for queries
- Appears to be automatically managed by ArcadePhysics system
- Appears to reduce collision check complexity

### Update Patterns
Based on code analysis:
- Physics updates appear to happen in fixed timestep
- Movement integration appears to be handled by physics world
- Collision detection appears to run after movement

## Implementation Patterns

### Physics Object Creation
Based on code analysis:
1. Extend appropriate base class (ArcadeSprite for moving objects)
2. Set physics type during initialization
3. Register with physics group
4. Configure collision shape

### Memory Management
Based on the decompiled code patterns:
1. Use object pooling for frequently created/destroyed objects
2. Return objects to pools when done
3. Clear references in pooled objects

### Collision Configuration
Based on observed patterns:
1. Enable necessary collision pairs
2. Use appropriate collision shapes (circle vs rectangle)
3. Consider using layers for scenarios

### Performance
Based on code analysis:
1. Minimize physics body count
2. Use static bodies for non-moving colliders
3. Batch physics operations when possible
4. Profile collision detection areas

### Unity 6 Migration Notes
Based on the decompiled Unity 6 code:
1. Collision callbacks appear to use `CallbackContext` and `ArcadeColliderType` parameters
2. All collision callbacks appear to return boolean values
3. Access physics bodies through `.body` property on collider types
4. Use property accessors for ArcadePhysics static access (`Instance`, `scene`, `Config`)
5. Check for `PickupImmaterial` flag in PhysicsManager for pickup collision state