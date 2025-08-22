# IL2CPP Guide Documentation

## Overview
Vampire Survivors uses Unity's IL2CPP backend, which converts C# IL code to optimized C++. Understanding IL2CPP limitations and patterns is essential for effective MelonLoader modding.

## IL2CPP Architecture

### Code Compilation Process
1. **C# Source** → **IL (Intermediate Language)** → **C++ Code** → **Native Binary**
2. Method implementations are replaced with `IL2CPP.il2cpp_runtime_invoke()` calls
3. Logic executes in compiled C++ code, not visible in decompiled output

### Method Implementation Visibility
IL2CPP decompilation shows method bodies that call IL2CPP runtime functions:

```csharp
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

**Modding Impact**:
- Cannot see mathematical operations (*, +, -, /)
- Cannot determine if stats are additive or multiplicative
- Cannot identify hard-coded caps or limits
- Requires runtime debugging or empirical testing for formulas

## Type System

### IL2CPP Type Prefixes
All game types require the `Il2Cpp` prefix:

```csharp
// Game types
Il2CppVampireSurvivors.Framework.GameManager
Il2CppVampireSurvivors.Data.DataManager
Il2CppVampireSurvivors.Objects.Characters.CharacterController

// IL2CPP System types
Il2CppSystem.Collections.Generic.Dictionary<TKey, TValue>
Il2CppSystem.String
Il2CppSystem.Single

// Standard .NET types (no prefix)
System.Collections.Generic.Dictionary<TKey, TValue>
System.String
System.Single
```

### Mixed Type Environment
```csharp
// IL2CPP collection from game
Il2CppSystem.Collections.Generic.Dictionary<WeaponType, List<WeaponData>> il2cppDict;

// Convert to System collection
var systemDict = new System.Collections.Generic.Dictionary<string, object>();

foreach (var kvp in il2cppDict)
{
    systemDict[kvp.Key.ToString()] = ProcessWeaponData(kvp.Value);
}
```

## Collection Handling

### IL2CPP Collections
```csharp
Il2CppSystem.Collections.Generic.List<WeaponData>
Dictionary<WeaponType, List<WeaponData>>
Il2CppStructArray<WeaponType>
Il2CppInterop.Runtime.InteropTypes.Arrays.Il2CppStructArray<Vector3>
```

### Safe Iteration
```csharp
if (il2cppDict != null && il2cppDict.Count > 0)
{
    foreach (var entry in il2cppDict)
    {
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

### Collection Conversion
```csharp
public static class Il2CppConverters
{
    // List conversions
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
    
    public static Il2CppSystem.Collections.Generic.List<T> ToIl2CppList<T>(List<T> systemList)
    {
        var il2cppList = new Il2CppSystem.Collections.Generic.List<T>();
        if (systemList != null)
        {
            foreach (var item in systemList)
            {
                if (item != null)
                    il2cppList.Add(item);
            }
        }
        return il2cppList;
    }
    
    // Dictionary conversions
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
    
    public static Il2CppSystem.Collections.Generic.Dictionary<TKey, TValue> ToIl2CppDictionary<TKey, TValue>(
        Dictionary<TKey, TValue> systemDict)
    {
        var il2cppDict = new Il2CppSystem.Collections.Generic.Dictionary<TKey, TValue>();
        if (systemDict != null)
        {
            foreach (var kvp in systemDict)
            {
                if (kvp.Key != null && kvp.Value != null)
                    il2cppDict.Add(kvp.Key, kvp.Value);
            }
        }
        return il2cppDict;
    }
    
    // HashSet conversions
    public static HashSet<T> ToHashSet<T>(Il2CppSystem.Collections.Generic.HashSet<T> il2cppSet)
    {
        var systemSet = new HashSet<T>();
        if (il2cppSet != null)
        {
            foreach (var item in il2cppSet)
            {
                if (item != null)
                    systemSet.Add(item);
            }
        }
        return systemSet;
    }
    
    public static Il2CppSystem.Collections.Generic.HashSet<T> ToIl2CppHashSet<T>(HashSet<T> systemSet)
    {
        var il2cppSet = new Il2CppSystem.Collections.Generic.HashSet<T>();
        if (systemSet != null)
        {
            foreach (var item in systemSet)
            {
                if (item != null)
                    il2cppSet.Add(item);
            }
        }
        return il2cppSet;
    }
}
```

### Extension Method Approach
For cleaner code, implement as extension methods:

```csharp
public static class Il2CppExtensions
{
    public static List<T> ToSystemList<T>(this Il2CppSystem.Collections.Generic.List<T> il2cppList)
    {
        return Il2CppConverters.ToSystemList(il2cppList);
    }
    
    public static Il2CppSystem.Collections.Generic.List<T> ToIl2CppList<T>(this List<T> systemList)
    {
        return Il2CppConverters.ToIl2CppList(systemList);
    }
    
    // Usage examples
    var weapons = gameManager.Weapons.ToSystemList();
    var il2cppWeapons = myWeaponList.ToIl2CppList();
}
```

## Exception Handling

### IL2CPP Exception Pattern
```csharp
try 
{
    var result = il2cppObject.GetSomeData();
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
- Validate IL2CPP objects before use
- Provide fallback values for missing data
- Reset to known-good state on error
- Use MelonLogger for debugging IL2CPP issues

## Memory Management

### Object Caching
```csharp
public static class MemoryManager
{
    private static WeakReference<GameManager> _gameManagerRef;
    
    public static GameManager GetGameManager()
    {
        if (_gameManagerRef?.TryGetTarget(out var gm) == true)
            return gm;
            
        gm = GM.Core;
        _gameManagerRef = new WeakReference<GameManager>(gm);
        return gm;
    }
    
    private static readonly List<WeaponData> _reusableList = new();
    
    public static List<WeaponData> GetWeaponDataSafely()
    {
        _reusableList.Clear();
        
        try
        {
            var weapons = GetGameManager()?.DataManager?.GetConvertedWeaponData();
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

### Core Runtime Components
```csharp
using Il2CppInterop.Runtime;
using Il2CppInterop.Runtime.InteropTypes;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using Il2CppInterop.Runtime.Runtime;
```

### Native Class Pointers
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

### Field Access
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
```csharp
return (intPtr != (System.IntPtr)0) ? Il2CppObjectPool.Get<SomeType>(intPtr) : null;
```

## Runtime Type Resolution

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
            var type = Type.GetType(typeName);
            if (type == null)
            {
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

## Unsafe Code Patterns

### Common Unsafe Operations
```csharp
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

### Safe Wrappers
```csharp
public static T SafeGetValue<T>(object il2cppObject, string fieldName, T defaultValue = default(T))
{
    try
    {
        if (il2cppObject == null) return defaultValue;
        
        var ptr = IL2CPP.Il2CppObjectBaseToPtr((Il2CppObjectBase)il2cppObject);
        if (ptr == System.IntPtr.Zero) return defaultValue;
        
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

## Modding Patterns

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

### Method Invocation
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

## Harmony Limitations

### Transpilers Not Supported
Harmony transpilers cannot be used with IL2CPP:
- IL2CPP compiles to C++, eliminating IL
- Game runs as native C++ code
- IL2CPP runtime doesn't maintain IL representation

### Alternative Approaches
```csharp
// Prefix hooks to modify parameters
[HarmonyPrefix]
public static void BeforeMethod(ref int parameter)
{
    parameter = ModifyValue(parameter);
}

// Postfix hooks to modify return values
[HarmonyPostfix]
public static void AfterMethod(ref int __result)
{
    __result = ModifyResult(__result);
}

// Complete method replacement
[HarmonyPrefix]
public static bool ReplaceMethod(ref int __result)
{
    __result = CustomImplementation();
    return false; // Skip original method
}
```

## Performance Optimization

### Boundary Crossing Minimization
```csharp
public static class BoundaryOptimizer
{
    // Batch IL2CPP data extraction
    public static void OptimizedPattern(Il2CppObject obj)
    {
        var data = ExtractAllData(obj);  // Single IL2CPP interaction
        ProcessData(data.Prop1, data.Prop2, data.Prop3);  // Pure managed code
    }
}
```

## Common Issues

### Object Validation
```csharp
public static bool IsValidIl2CppObject(object obj)
{
    try
    {
        return obj != null && obj.ToString() != null;
    }
    catch
    {
        return false;
    }
}
```

### String Handling
```csharp
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

### Array Access
```csharp
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

## Debugging

### Object Information Logging
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
        }
        catch (Exception ex)
        {
            MelonLogger.Error($"Game state validation failed: {ex.Message}");
        }
    }
}
```

## Vampire Survivors Patterns

### Core Game Access
```csharp
// Primary game manager access
var gameManager = GM.Core;

// Data manager access
var dataManager = gameManager?.DataManager;

// Weapon data access
var weapons = dataManager?.GetConvertedWeaponData();  // Returns Dictionary<WeaponType, List<WeaponData>>
```

### Assembly References
```csharp
// Core assemblies
Il2CppVampireSurvivors.Runtime.dll
Il2CppSystem.dll
Il2CppInterop.Runtime.dll

// Key namespaces
using Il2CppVampireSurvivors.Framework;
using Il2CppVampireSurvivors.Data;
using Il2CppVampireSurvivors.Data.Weapons;
using Il2CppVampireSurvivors.Objects.Characters;
using Il2CppVampireSurvivors.Objects.Weapons;
```

## Best Practices

1. Use Il2Cpp prefixes for game types
2. Null-check all IL2CPP objects
3. Handle exceptions gracefully
4. Minimize boundary crossings
5. Cache frequently accessed objects
6. Validate pointer integrity
7. Log extensively for debugging
8. Use safe wrappers for unsafe operations
9. Understand object lifecycle differences
10. Batch IL2CPP operations when possible