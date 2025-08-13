# IL2CPP Guide Documentation

## Overview
Vampire Survivors is built using Unity's IL2CPP backend, which converts C# IL code to optimized C++ for better performance. Understanding IL2CPP's limitations and patterns is crucial for effective modding with MelonLoader.

## IL2CPP Architecture

### Code Compilation Process
1. **C# Source** → **IL (Intermediate Language)** → **C++ Code** → **Native Binary**
2. Original method implementations are replaced with `IL2CPP.il2cpp_runtime_invoke()` calls
3. Actual logic executes in compiled C++ code, not visible in decompiled output

### Method Implementation Visibility
**Critical Limitation**: IL2CPP decompilation shows method bodies that call IL2CPP runtime functions:

```csharp
// What you actually see in decompiled code
public unsafe override float PPower()
{
    IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this);
    System.IntPtr* ptr = null;
    Unsafe.SkipInit(out System.IntPtr intPtr2);
    System.IntPtr intPtr = IL2CPP.il2cpp_runtime_invoke(
        IL2CPP.il2cpp_object_get_virtual_method(
            IL2CPP.Il2CppObjectBaseToPtr((Il2CppObjectBase)(object)this), 
            NativeMethodInfoPtr_PPower_Public_Virtual_Single_0), 
        IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this), 
        (void**)ptr, ref intPtr2);
    Il2CppException.RaiseExceptionIfNecessary(intPtr2);
    return *(float*)IL2CPP.il2cpp_object_unbox(intPtr);
}
```

**Impact on Modding**:
- Cannot see actual mathematical operations (*, +, -, /)
- Cannot determine if stats are additive or multiplicative from code
- Cannot identify hard-coded caps, limits, or special conditions
- Need runtime debugging or empirical testing for formulas

## Type System Differences

### IL2CPP Type Prefixes
All game types require the `Il2Cpp` prefix when referenced in mods:

```csharp
// Correct IL2CPP type references
Il2CppVampireSurvivors.Framework.GameManager
Il2CppVampireSurvivors.Data.DataManager
Il2CppVampireSurvivors.Objects.Characters.CharacterController

// IL2CPP System types (with Il2Cpp prefix)
Il2CppSystem.Collections.Generic.Dictionary<TKey, TValue>
Il2CppSystem.String
Il2CppSystem.Single

// Standard .NET types (no prefix needed)
System.Collections.Generic.Dictionary<TKey, TValue>
System.String
System.Single
```

### Mixed Type Environments
Mods operate in a mixed environment with both IL2CPP and System types:

```csharp
// IL2CPP collection from game
Il2CppSystem.Collections.Generic.Dictionary<WeaponType, List<WeaponData>> il2cppDict;

// Convert to System collection for easier manipulation
var systemDict = new System.Collections.Generic.Dictionary<string, object>();

foreach (var kvp in il2cppDict)
{
    // Bridge between IL2CPP and System types
    systemDict[kvp.Key.ToString()] = ProcessWeaponData(kvp.Value);
}
```

## Collection Handling

### IL2CPP Collections
Game collections use IL2CPP variants of standard .NET collections:

```csharp
// IL2CPP collections from game data
Il2CppSystem.Collections.Generic.List<WeaponData>
Dictionary<WeaponType, List<WeaponData>>  // Note: Often no Il2CppSystem prefix
Il2CppStructArray<WeaponType>  // IL2CPP array type
Il2CppInterop.Runtime.InteropTypes.Arrays.Il2CppStructArray<Vector3>
```

### Safe Iteration Patterns
```csharp
// Safe iteration of IL2CPP collections
if (il2cppDict != null && il2cppDict.Count > 0)
{
    foreach (var entry in il2cppDict)
    {
        // Always null-check IL2CPP objects
        if (entry.Value != null && entry.Value.Count > 0)
        {
            foreach (var item in entry.Value)
            {
                if (item != null)
                {
                    ProcessItem(item);
                }
            }
        }
    }
}
```

### Collection Conversion Utilities
```csharp
public static class Il2CppConverters
{
    public static List<T> ToSystemList<T>(Il2CppSystem.Collections.Generic.List<T> il2cppList)
    {
        var systemList = new List<T>();
        if (il2cppList != null)
        {
            foreach (var item in il2cppList)
            {
                if (item != null)
                    systemList.Add(item);
            }
        }
        return systemList;
    }
    
    public static Dictionary<TKey, TValue> ToSystemDictionary<TKey, TValue>(
        Il2CppSystem.Collections.Generic.Dictionary<TKey, TValue> il2cppDict)
    {
        var systemDict = new Dictionary<TKey, TValue>();
        if (il2cppDict != null)
        {
            foreach (var kvp in il2cppDict)
            {
                if (kvp.Key != null && kvp.Value != null)
                    systemDict[kvp.Key] = kvp.Value;
            }
        }
        return systemDict;
    }
}
```

## Exception Handling

### IL2CPP Exception Characteristics
IL2CPP uses `Il2CppException.RaiseExceptionIfNecessary()` pattern throughout:

```csharp
// Typical IL2CPP method pattern with exception handling
public unsafe SomeType GetSomeData()
{
    IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this);
    System.IntPtr* ptr = null;
    Unsafe.SkipInit(out System.IntPtr intPtr2);
    System.IntPtr intPtr = IL2CPP.il2cpp_runtime_invoke(
        NativeMethodInfoPtr_GetSomeData_Public_SomeType_0, 
        IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this), 
        (void**)ptr, ref intPtr2);
    Il2CppException.RaiseExceptionIfNecessary(intPtr2);  // Key exception handling
    return (intPtr != (System.IntPtr)0) ? Il2CppObjectPool.Get<SomeType>(intPtr) : null;
}

// Safe IL2CPP exception handling pattern in mods
try 
{
    var result = il2cppObject.GetSomeData();
    // Work with result
}
catch (System.NullReferenceException ex)
{
    MelonLogger.Error($"IL2CPP null reference: {ex.Message}");
}
catch (Il2CppSystem.Exception il2cppEx)
{
    MelonLogger.Error($"IL2CPP exception: {il2cppEx.Message}");
}
catch (System.Exception ex)
{
    MelonLogger.Error($"General exception: {ex.Message}");
}
```

### Recovery Strategies
1. **Null Checks**: Always validate IL2CPP objects before use
2. **Fallback Values**: Have defaults for missing or corrupted data
3. **Defensive Programming**: Validate inputs from game systems
4. **State Recovery**: Reset to known-good state on error
5. **Logging**: Use MelonLogger for debugging IL2CPP issues

## Memory Management

### IL2CPP Garbage Collection
IL2CPP uses a different GC strategy than standard Mono:

```csharp
// Good practices for IL2CPP memory management
public static class MemoryManager
{
    // Cache frequently accessed IL2CPP objects
    private static WeakReference<GameManager> _gameManagerRef;
    
    public static GameManager GetGameManager()
    {
        if (_gameManagerRef?.TryGetTarget(out var gm) == true)
            return gm;
            
        gm = GM.Core;
        _gameManagerRef = new WeakReference<GameManager>(gm);
        return gm;
    }
    
    // Minimize object creation in hot paths
    private static readonly List<WeaponData> _reusableList = new();
    
    public static List<WeaponData> GetWeaponDataSafely()
    {
        _reusableList.Clear();
        
        try
        {
            var weapons = GetGameManager()?.DataManager?.GetConvertedWeapons();
            if (weapons != null)
            {
                foreach (var weaponList in weapons.Values)
                {
                    if (weaponList != null)
                        _reusableList.AddRange(weaponList);
                }
            }
        }
        catch (Exception ex)
        {
            MelonLogger.Error($"Error getting weapon data: {ex.Message}");
        }
        
        return _reusableList;
    }
}
```

## IL2CPP Interop Layer

### Core IL2CPP Runtime Components
The decompiled code reveals several key IL2CPP interop components that mod developers should understand:

```csharp
// Key imports found in every IL2CPP file
using Il2CppInterop.Runtime;
using Il2CppInterop.Runtime.InteropTypes;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using Il2CppInterop.Runtime.Runtime;
```

### Native Class Pointer Management
Every IL2CPP class uses a static constructor pattern for native class setup:

```csharp
static GameManager()
{
    Il2CppClassPointerStore<GameManager>.NativeClassPtr = 
        IL2CPP.GetIl2CppClass("VampireSurvivors.Runtime.dll", "VampireSurvivors.Framework", "GameManager");
    IL2CPP.il2cpp_runtime_class_init(Il2CppClassPointerStore<GameManager>.NativeClassPtr);
    NativeMethodInfoPtr_SomeMethod_Public_ReturnType_0 = 
        IL2CPP.GetIl2CppMethodByToken(Il2CppClassPointerStore<GameManager>.NativeClassPtr, 100683855);
}
```

### Field Access Patterns
IL2CPP field access goes through native field pointers:

```csharp
public unsafe float SomeFloatProperty
{
    get
    {
        nint ptr = (nint)IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this) + 
                   (int)IL2CPP.il2cpp_field_get_offset(NativeFieldInfoPtr_someField);
        return *(float*)ptr;
    }
    set
    {
        *(float*)((nint)IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this) + 
                  (int)IL2CPP.il2cpp_field_get_offset(NativeFieldInfoPtr_someField)) = value;
    }
}
```

### Object Pool Usage
IL2CPP uses object pooling for performance:

```csharp
// Common pattern for returning IL2CPP objects
return (intPtr != (System.IntPtr)0) ? Il2CppObjectPool.Get<SomeType>(intPtr) : null;
```

## Runtime Type Resolution

### Dynamic Type Access
Some IL2CPP types may require runtime resolution:

```csharp
public static class TypeResolver
{
    private static readonly Dictionary<string, Type> _typeCache = new();
    
    public static Type GetIl2CppType(string typeName)
    {
        if (_typeCache.TryGetValue(typeName, out var cachedType))
            return cachedType;
            
        try
        {
            // Try direct resolution first
            var type = Type.GetType(typeName);
            if (type == null)
            {
                // Search in loaded assemblies
                foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
                {
                    type = assembly.GetType(typeName);
                    if (type != null) break;
                }
            }
            
            _typeCache[typeName] = type;
            return type;
        }
        catch (Exception ex)
        {
            MelonLogger.Error($"Failed to resolve type {typeName}: {ex.Message}");
            return null;
        }
    }
}
```

## Unsafe Code Considerations

### Prevalent Unsafe Patterns
IL2CPP decompiled code heavily uses unsafe operations that mod developers should understand:

```csharp
// Common unsafe patterns you'll see everywhere
public unsafe SomeMethod()
{
    // Pointer arithmetic for field access
    nint fieldPtr = (nint)IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this) + 
                    (int)IL2CPP.il2cpp_field_get_offset(NativeFieldInfoPtr_field);
    
    // Stack allocation for parameters
    System.IntPtr* ptr = stackalloc System.IntPtr[1];
    ptr[0] = IL2CPP.Il2CppObjectBaseToPtr((Il2CppObjectBase)(object)parameter);
    
    // Skip initialization for performance
    Unsafe.SkipInit(out System.IntPtr intPtr2);
    
    // Copy blocks for value types
    Unsafe.CopyBlock(destinationPtr, sourcePtr, sizeInBytes);
}
```

### Working With Unsafe IL2CPP Code
When interacting with IL2CPP objects, be aware of these patterns:

```csharp
// Safe wrapper for unsafe IL2CPP operations
public static T SafeGetValue<T>(object il2cppObject, string fieldName, T defaultValue = default(T))
{
    try
    {
        if (il2cppObject == null) return defaultValue;
        
        // IL2CPP objects often require pointer validation
        var ptr = IL2CPP.Il2CppObjectBaseToPtr((Il2CppObjectBase)il2cppObject);
        if (ptr == System.IntPtr.Zero) return defaultValue;
        
        // Get field using reflection (safer than direct pointer access)
        var field = il2cppObject.GetType().GetField(fieldName);
        if (field == null) return defaultValue;
        
        var value = field.GetValue(il2cppObject);
        return value is T typedValue ? typedValue : defaultValue;
    }
    catch (Exception ex)
    {
        MelonLogger.Error($"Error accessing IL2CPP field {fieldName}: {ex.Message}");
        return defaultValue;
    }
}
```

## IL2CPP-Specific Modding Patterns

### Safe Property Access
```csharp
public static class SafeAccess
{
    public static T GetProperty<T>(object il2cppObject, string propertyName, T defaultValue = default(T))
    {
        try
        {
            if (il2cppObject == null) return defaultValue;
            
            var property = il2cppObject.GetType().GetProperty(propertyName);
            if (property == null) return defaultValue;
            
            var value = property.GetValue(il2cppObject);
            return value is T typedValue ? typedValue : defaultValue;
        }
        catch (Exception ex)
        {
            MelonLogger.Error($"Error accessing property {propertyName}: {ex.Message}");
            return defaultValue;
        }
    }
    
    public static void SetProperty<T>(object il2cppObject, string propertyName, T value)
    {
        try
        {
            if (il2cppObject == null) return;
            
            var property = il2cppObject.GetType().GetProperty(propertyName);
            if (property?.CanWrite == true)
            {
                property.SetValue(il2cppObject, value);
            }
        }
        catch (Exception ex)
        {
            MelonLogger.Error($"Error setting property {propertyName}: {ex.Message}");
        }
    }
}
```

### Method Invocation Patterns
```csharp
public static class MethodInvoker
{
    public static T InvokeMethod<T>(object il2cppObject, string methodName, params object[] parameters)
    {
        try
        {
            if (il2cppObject == null) return default(T);
            
            var method = il2cppObject.GetType().GetMethod(methodName);
            if (method == null) return default(T);
            
            var result = method.Invoke(il2cppObject, parameters);
            return result is T typedResult ? typedResult : default(T);
        }
        catch (Exception ex)
        {
            MelonLogger.Error($"Error invoking method {methodName}: {ex.Message}");
            return default(T);
        }
    }
}
```

## Performance Considerations

### IL2CPP Performance Characteristics
1. **Faster execution**: Compiled C++ code runs faster than interpreted IL
2. **Slower startup**: More overhead during game initialization
3. **Memory efficiency**: Better memory layout and GC performance
4. **Interop costs**: Crossing IL2CPP/managed boundaries has overhead

### Optimization Strategies
```csharp
// Minimize boundary crossings
public static class BoundaryOptimizer
{
    // BAD: Multiple IL2CPP calls
    public static void BadPattern(Il2CppObject obj)
    {
        var prop1 = obj.Property1;  // IL2CPP call
        var prop2 = obj.Property2;  // IL2CPP call
        var prop3 = obj.Property3;  // IL2CPP call
        ProcessData(prop1, prop2, prop3);
    }
    
    // GOOD: Batch IL2CPP data extraction
    public static void GoodPattern(Il2CppObject obj)
    {
        var data = ExtractAllData(obj);  // Single IL2CPP interaction
        ProcessData(data.Prop1, data.Prop2, data.Prop3);  // Pure managed code
    }
}
```

## Common IL2CPP Issues

### 1. Null Reference Patterns
```csharp
// IL2CPP objects can become null unexpectedly
public static bool IsValidIl2CppObject(object obj)
{
    try
    {
        return obj != null && obj.ToString() != null;
    }
    catch
    {
        return false;  // Object is invalid/destroyed
    }
}
```

### 2. String Handling
```csharp
// IL2CPP strings need special handling
public static string SafeToString(object il2cppObject)
{
    try
    {
        return il2cppObject?.ToString() ?? string.Empty;
    }
    catch
    {
        return string.Empty;
    }
}
```

### 3. Array and Collection Issues
```csharp
// Safe array access pattern
public static T SafeGetArrayElement<T>(Il2CppStructArray<T> array, int index, T defaultValue = default(T))
{
    try
    {
        if (array == null || index < 0 || index >= array.Length)
            return defaultValue;
            
        return array[index];
    }
    catch
    {
        return defaultValue;
    }
}
```

## Debugging IL2CPP Code

### Logging Strategies
```csharp
public static class Il2CppDebugger
{
    public static void LogObjectInfo(object il2cppObject, string context)
    {
        try
        {
            if (il2cppObject == null)
            {
                MelonLogger.Msg($"{context}: Object is null");
                return;
            }
            
            var type = il2cppObject.GetType();
            MelonLogger.Msg($"{context}: Type = {type.Name}");
            
            // Log properties
            foreach (var prop in type.GetProperties())
            {
                try
                {
                    var value = prop.GetValue(il2cppObject);
                    MelonLogger.Msg($"  {prop.Name} = {value}");
                }
                catch (Exception ex)
                {
                    MelonLogger.Msg($"  {prop.Name} = <Error: {ex.Message}>");
                }
            }
        }
        catch (Exception ex)
        {
            MelonLogger.Error($"Error logging object info: {ex.Message}");
        }
    }
}
```

### Runtime Validation
```csharp
public static class Validator
{
    public static void ValidateGameState()
    {
        try
        {
            var gm = GM.Core;
            MelonLogger.Msg($"GameManager valid: {gm != null}");
            
            var dm = gm?.DataManager;
            MelonLogger.Msg($"DataManager valid: {dm != null}");
            
            var player = gm?.Player;
            MelonLogger.Msg($"Player valid: {player != null}");
            
            // Validate critical game objects
        }
        catch (Exception ex)
        {
            MelonLogger.Error($"Game state validation failed: {ex.Message}");
        }
    }
}
```

## Best Practices Summary

1. **Always use Il2Cpp prefixes** for game types
2. **Understand the unsafe context** - IL2CPP code is heavily pointer-based
3. **Null-check everything** - IL2CPP objects can become invalid unexpectedly
4. **Handle exceptions gracefully** - Use `Il2CppException.RaiseExceptionIfNecessary()` pattern awareness
5. **Minimize boundary crossings** - Batch IL2CPP operations to reduce interop overhead
6. **Cache frequently accessed objects** - Use `Il2CppObjectPool` patterns when possible
7. **Validate pointer integrity** - Check `IL2CPP.Il2CppObjectBaseToPtr()` results
8. **Test thoroughly** - IL2CPP runtime behavior differs significantly from standard .NET
9. **Log extensively** - Debug information is crucial for IL2CPP interop issues
10. **Use safe wrappers** - Encapsulate unsafe IL2CPP access in try-catch blocks
11. **Be aware of object lifecycle** - IL2CPP objects may be garbage collected differently
12. **Understand native method tokens** - Method calls go through `IL2CPP.GetIl2CppMethodByToken()`

## Vampire Survivors Specific IL2CPP Patterns

### Core Game Access
The game uses these specific access patterns confirmed in the decompiled source:

```csharp
// Primary game manager access
var gameManager = GM.Core;  // Static property, not GM.Instance

// Data manager access through game manager
var dataManager = gameManager?.DataManager;

// Converted weapon data access (confirmed method signature)
var weapons = dataManager?.GetConvertedWeapons();  // Returns Dictionary<WeaponType, List<WeaponData>>
```

### Common IL2CPP Assembly References
Based on the decompiled code structure, mods typically need these assemblies:

```csharp
// Core game assemblies
Il2CppVampireSurvivors.Runtime.dll      // Main game logic
Il2CppSystem.dll                        // IL2CPP system types
Il2CppInterop.Runtime.dll              // IL2CPP interop layer

// Key namespaces to import
using Il2CppVampireSurvivors.Framework;
using Il2CppVampireSurvivors.Data;
using Il2CppVampireSurvivors.Data.Weapons;
using Il2CppVampireSurvivors.Objects.Characters;
using Il2CppVampireSurvivors.Objects.Weapons;
```

### Observed IL2CPP Method Patterns
From analyzing the decompiled source, these patterns are consistently used:

```csharp
// Method signature pattern for weapon power calculation
public unsafe override float PPower()
{
    // Native pointer validation
    IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this);
    System.IntPtr* ptr = null;
    Unsafe.SkipInit(out System.IntPtr intPtr2);
    
    // Virtual method call through IL2CPP
    System.IntPtr intPtr = IL2CPP.il2cpp_runtime_invoke(
        IL2CPP.il2cpp_object_get_virtual_method(
            IL2CPP.Il2CppObjectBaseToPtr((Il2CppObjectBase)(object)this), 
            NativeMethodInfoPtr_PPower_Public_Virtual_Single_0), 
        IL2CPP.Il2CppObjectBaseToPtrNotNull((Il2CppObjectBase)(object)this), 
        (void**)ptr, ref intPtr2);
    
    // Exception handling and result extraction
    Il2CppException.RaiseExceptionIfNecessary(intPtr2);
    return *(float*)IL2CPP.il2cpp_object_unbox(intPtr);
}
```