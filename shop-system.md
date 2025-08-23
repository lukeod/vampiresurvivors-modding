# Shop System

The shop system manages merchant inventories and item availability through the ShopFactory.

## ShopFactory

Intelligent inventory generation with multiplayer synchronization support.

### Properties

```csharp
public class ShopFactory : Il2CppSystem.Object
{
    public DataManager _data;                    // Game data access
    public PlayerOptions _playerOptions;         // Player configuration
    public List<WeaponType> _availableWeapons;   // Current weapon inventory
    public List<ItemType> _availableItems;       // Current item inventory
}
```

### Public Properties

- `List<WeaponType> AvailableWeapons { get; }` - Current weapon inventory (738 references in codebase)
- `List<ItemType> AvailableItems { get; }` - Current item inventory (30 references)

### Methods

#### Core Generation
- `GenerateShopInventory(CharacterController player)` - Main inventory generation (2 callers)
- `InjectRemoteShop(List<WeaponType> weapons, List<ItemType> items)` - Multiplayer shop injection

#### Validation
- `DoesPlayerAlreadyHaveWeapon(WeaponType t)` - Weapon ownership check (8 callers)

#### Static Helpers
- `GetValidAdventureWeaponsForMerchant(List<WeaponType> merchantInventory, PlayerOptions playerOptions)` - Adventure mode filtering (2 callers)
- `GetValidCustomMerchantWeapons(List<WeaponType> merchantInventory, PlayerOptions playerOptions)` - Custom merchant filtering
- `GetValidCustomMerchantItems(List<ItemType> merchantInventoryItems, PlayerOptions playerOptions)` - Custom item filtering

#### Internal Generation
- `MakeCustomInventory()` - Custom shop generation
- `MakeStandardInventory(CharacterController player)` - Standard shop generation  
- `MakeArcanaInventory()` - Arcana shop generation
- `MakeEggsInventory(CharacterController player)` - Egg shop generation

## Shop Types

### Standard Shop
Generated through `MakeStandardInventory()`, considers:
- Player level and progress
- Unlocked weapons and items
- Current character capabilities

### Custom Shop
Generated through `MakeCustomInventory()`, uses:
- Predefined merchant inventories
- Special event items
- Limited-time offerings

### Arcana Shop
Generated through `MakeArcanaInventory()`, includes:
- Arcana cards
- Special power-ups
- End-game content

### Egg Shop
Generated through `MakeEggsInventory()`, offers:
- Character eggs
- Secret unlocks
- Easter egg content

## Multiplayer Integration

### Shop Synchronization

The `InjectRemoteShop()` method enables shop synchronization in online multiplayer:

```csharp
// Host generates shop inventory
shopFactory.GenerateShopInventory(hostPlayer);

// Host sends inventory to other players
shopFactory.InjectRemoteShop(hostWeapons, hostItems);
```

This ensures:
- All players see the same shop inventory
- Prevents desynchronization issues
- Maintains game balance in multiplayer

### Multiplayer Considerations

- Shop generation is host-authoritative
- Clients receive shop data via `InjectRemoteShop()`
- Validation occurs on both client and server

## Integration with UI

### MerchantUIPage

The shop system integrates directly with the merchant UI:

```csharp
public void Construct(
    SignalBus signalBus,
    DataManager data,
    PlayerOptions playerOptions,
    GameSessionData session,
    EggManager egg,
    AdventureManager adventureManager,
    ShopFactory shopFactory // Injected dependency
)
```

### Usage Flow

1. Player enters merchant area
2. ShopFactory generates inventory based on game state
3. MerchantUIPage displays available items
4. Player makes selections
5. ShopFactory validates purchases

## Adventure Mode

Special handling for adventure mode merchants:

```csharp
var validWeapons = ShopFactory.GetValidAdventureWeaponsForMerchant(
    merchantInventory,
    playerOptions
);
```

Adventure mode features:
- Restricted item pools
- Progressive unlocks
- Quest-specific items

## Modding Guidelines

### Custom Shop Items

```csharp
[HarmonyPatch(typeof(ShopFactory), "GenerateShopInventory")]
public static void Postfix(ShopFactory __instance, CharacterController player)
{
    // Only modify in single player
    if (!GM.Core.IsMultiplayer)
    {
        // Add custom weapons
        if (!__instance.DoesPlayerAlreadyHaveWeapon(CustomWeaponType))
        {
            __instance.AvailableWeapons.Add(CustomWeaponType);
        }
    }
}
```

### Multiplayer-Safe Modifications

```csharp
public class CustomShopMod : MelonMod
{
    [HarmonyPatch(typeof(ShopFactory), "GenerateShopInventory")]
    public static void Postfix(ShopFactory __instance)
    {
        if (GM.Core.IsOnlineMultiplayer)
        {
            // Don't modify - host controls shop
            return;
        }
        
        // Safe to modify in single player/local coop
        AddCustomItems(__instance);
    }
}
```

### Shop Validation

```csharp
[HarmonyPatch(typeof(ShopFactory), "DoesPlayerAlreadyHaveWeapon")]
public static void Postfix(
    ShopFactory __instance,
    WeaponType t,
    ref bool __result)
{
    // Custom validation logic
    if (IsCustomWeapon(t))
    {
        __result = CheckCustomWeaponOwnership(t);
    }
}
```

## Access Patterns

```csharp
// Via GameManager
ShopFactory shopFactory = GM.Core.ShopFactory;

// Via dependency injection in UI pages
public void Construct(..., ShopFactory shopFactory)
```

## Best Practices

- Always check multiplayer state before modifying shop inventory
- Use `DoesPlayerAlreadyHaveWeapon()` to prevent duplicates
- Respect host authority in online multiplayer
- Test shop modifications in all game modes
- Consider adventure mode restrictions
- Validate custom items against game balance

## Common Patterns

### Adding Conditional Items

```csharp
private void AddConditionalItems(ShopFactory factory)
{
    var data = GM.Core._data;
    
    // Add items based on achievements
    if (data._unlockedAchievements.Contains("BeatBoss"))
    {
        factory.AvailableWeapons.Add(WeaponType.BossKiller);
    }
    
    // Add items based on character
    if (GM.Core.Character.CharacterType == CharacterType.CRISTINA)
    {
        factory.AvailableItems.Add(ItemType.HolyWater);
    }
}
```

### Dynamic Pricing

```csharp
[HarmonyPatch(typeof(MerchantUIPage), "GetItemPrice")]
public static void Postfix(ref int __result, ItemType item)
{
    // Apply custom pricing logic
    if (IsOnSale(item))
    {
        __result = Mathf.RoundToInt(__result * 0.5f);
    }
}
```
