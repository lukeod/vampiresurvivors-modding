# Physics System

Vampire Survivors uses a custom physics engine (ArcadePhysics) for collision detection and movement, integrated with Unity's component system.

## Core Components

### PhysicsManager (Singleton)
**Location**: `Il2CppVampireSurvivors.Framework.PhysicsManager`

Central physics management singleton:

```csharp
// Singleton access
PhysicsManager._sInstance
PhysicsManager.Instance // Property accessor
```

#### Physics Groups
Manages different collision groups for game objects:
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
- `OnPlayerOverlapsEnemy` - Player-enemy collision
- `OnPlayerOverlapsPickup` - Player-pickup collection
- `OnMagnetOverlapsPickup` - Magnet attraction
- `OnBulletOverlapsDoor` - Projectile-door interaction

### ArcadePhysics System
**Location**: `Il2CppPhaserPort.ArcadePhysics`

Custom physics engine implementation:

```csharp
// Static access points
ArcadePhysics.s_instance    // Singleton instance
ArcadePhysics.s_scene       // PhaserScene reference
ArcadePhysics.s_world       // Physics world
ArcadePhysics.s_currentConfig // Physics configuration
```

#### Collision Detection Methods
- `OverlapRect()` - Rectangle collision detection
- `OverlapCirc()` - Circle collision detection
- `OverlapLine()` - Line collision detection
- `CircleToCircle()` - Circle-to-circle collision test
- `CircleToRectangle()` - Circle-to-rectangle collision test

## Physics Body Hierarchy

### BaseBody
**Location**: `Il2CppPhaserPort.BaseBody`

Base physics body with core properties:

```csharp
public class BaseBody
{
    // Physics type
    public PhysicsType _physicsType;  // DYNAMIC_BODY, STATIC_BODY, UNDEFINED
    
    // Movement
    public float2 _velocity;
    public float2 _position;
    
    // Collision shape
    public float2 _size;
    public float2 _center;
    public bool _isCircle;
    public float _radius;
    
    // Physics properties
    public float _mass;
    public bool _immovable;
    public bool _pushable;
    public bool _collideWorldBounds;
}
```

### ArcadeSprite
**Location**: `Il2CppPhaserPort.ArcadeSprite`

Extends BaseBody with sprite rendering:

```csharp
public class ArcadeSprite : BaseBody
{
    public float2 position;
    
    // Movement methods
    public void setVelocity(float x, float y);
    public void setBounce(float x, float y);
    public void setCollideWorldBounds(bool value);
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

## Physics Groups

### PhysicsGroup Class
**Location**: `Il2CppPhaserPort.PhysicsGroup`

Container for physics objects:

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

Specialized pool for projectiles:

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

Extends ArcadeSprite for weapon projectiles:

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
    
    // Initialization
    public void InitProjectile(BulletPool pool, Weapon weapon, int index);
    
    // Physics callbacks
    public void OnHasHitWallPhaser(PhaserTile tile);
    
    // Lifecycle
    public void Explode();
    public void Despawn();
}
```

### Integration Flow

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
        // Set physics type
        body._physicsType = PhysicsType.DYNAMIC_BODY;
        
        // Configure collision shape
        body._isCircle = false;
        body._size = new float2(32, 32);
        
        // Set physics properties
        body._mass = 1.0f;
        body._immovable = false;
        body._collideWorldBounds = true;
    }
}

// 2. Register with PhysicsManager
var customObject = new CustomPhysicsObject();
customObject.Initialize();
PhysicsManager.Instance._bulletGroup.add(customObject);

// 3. Set movement
customObject.setVelocity(100, 0);  // Move right at 100 units/sec

// 4. Handle collisions
public override void OnHasHitWallPhaser(PhaserTile tile)
{
    // Custom wall collision logic
}
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
var hits = ArcadePhysics.OverlapRect(x, y, width, height);

// Circle overlap
var hits = ArcadePhysics.OverlapCirc(centerX, centerY, radius);

// Line overlap
var hits = ArcadePhysics.OverlapLine(x1, y1, x2, y2);
```

### Collision Callbacks

```csharp
// Register collision callback
PhysicsManager.Instance.OnPlayerOverlapsEnemy += (player, enemy) =>
{
    // Handle player-enemy collision
};

// Custom group collision
PhysicsManager.Instance.InitPhysicsColliders();
// Sets up collision detection between registered groups
```

## Performance Considerations

### Object Pooling
- All projectiles use BulletPool for memory efficiency
- Enemies often use similar pooling systems
- Pickups are recycled through object pools

### Spatial Indexing
- Uses RBush spatial tree for efficient queries
- Automatically managed by ArcadePhysics system
- Reduces collision check complexity

### Update Patterns
- Physics updates happen in fixed timestep
- Movement integration handled by physics world
- Collision detection runs after movement

## Best Practices

### Physics Object Creation
1. Always extend appropriate base class (ArcadeSprite for moving objects)
2. Set physics type explicitly during initialization
3. Register with correct physics group
4. Configure collision shape appropriately

### Memory Management
1. Use object pooling for frequently created/destroyed objects
2. Properly return objects to pools when done
3. Clear references in pooled objects

### Collision Configuration
1. Only enable necessary collision pairs
2. Use appropriate collision shapes (circle vs rectangle)
3. Consider using layers for complex scenarios

### Performance
1. Minimize physics body count
2. Use static bodies for non-moving colliders
3. Batch physics operations when possible
4. Profile collision detection hotspots