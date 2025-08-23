# Unity Version Upgrade Guide: 2021.3 → 6000.0.36f1
## Reference for Vampire Survivors v1.0+ Unity Engine Changes

*Last updated: January 2025*

## Overview

Vampire Survivors v1.0+ upgraded from Unity 2021.3.44f1 to Unity 6000.0.36f1. Based on analysis of the decompiled IL2CPP code, this upgrade appears to enable networking capabilities, Universal Render Pipeline (URP) support, IL2CPP optimizations, and multiplayer architecture while maintaining backward compatibility for most modding patterns.

## 1. Unity Engine Upgrade Details

### Version Change
- **Previous**: Unity 2021.3.44f1 (LTS)
- **Current**: Unity 6000.0.36f1 (Latest)

### Key Upgrade Motivations
1. **Networking Infrastructure**: Unity 6000 provides networking support required for Coherence Toolkit integration
2. **Render Pipeline Modernization**: URP support for visual effects and performance
3. **IL2CPP Improvements**: Code generation and interop patterns
4. **Collection Handling**: Memory management and collection performance
5. **Modern Platform Support**: Compatibility with current platform SDKs

## 2. Universal Render Pipeline (URP) Implementation

### Evidence from Source Code

The decompiled IL2CPP code reveals URP integration:

#### URP Assemblies Present
- `Unity.RenderPipelines.Universal.Runtime` (confirmed in project references)
- `Unity.RenderPipelines.Universal.2D.Runtime` (confirmed in project references)  
- `Unity.RenderPipelines.Core.Runtime` (present in assembly structure)

#### URP-Specific Components
- **DisableURPDebugUpdater**: Custom component to manage URP debug systems
- **Universal Renderer**: Full URP rendering pipeline implementation
- **URP Shader Support**: Trail and effect shaders adapted for URP

```csharp
// Evidence from DisableURPDebugUpdater.cs
public class DisableURPDebugUpdater : MonoBehaviour
{
    public unsafe void Awake()
    {
        // Appears to disable URP debug features based on code analysis
        // IL2CPP interop calls to native URP debug system
    }
}
```

#### Shader Adaptations
The game includes URP-compatible shaders for effects:
- `s_atlasRectTrailShader` - URP trail rendering
- `s_atlasRectTrailAdditiveShader` - URP additive blending trails

### What URP Means for Modders

1. **Rendering Pipeline Changes**:
   - Shaders must be URP-compatible
   - Built-in render pipeline shaders no longer work
   - Post-processing effects use URP Volume system

2. **Performance Implications**:
   - GPU performance on modern hardware
   - Batching and culling
   - Lighting calculations

3. **Visual Effects**:
   - Particle systems
   - 2D lighting support
   - Post-processing capabilities

## 3. IL2CPP Changes and Compatibility

### IL2CPP Patterns

Unity 6000 introduces IL2CPP code generation patterns evident in the decompiled code:

#### Object Pool Management
```csharp
// Object pooling with memory management, inferred from decompiled code
return (intPtr != (System.IntPtr)0) ? Il2CppObjectPool.Get<T>(intPtr) : null;
```

#### Interop Calls
```csharp
// Native method invocation patterns based on code analysis
System.IntPtr intPtr = IL2CPP.il2cpp_runtime_invoke(methodPtr, objectPtr, args, ref exception);
Il2CppException.RaiseExceptionIfNecessary(exception);
```

#### Collection Handling
- Native array support with `Unity.Collections.NativeArray`
- Memory layout for collections
- Garbage collection integration

### Breaking Changes for Mods

1. **Method Signature Changes**:
   - Some Unity API methods have updated signatures
   - Physics callback parameters may differ
   - Event handling patterns updated

2. **Interop Patterns**:
   - IL2CPP object creation patterns refined
   - Memory management calls optimized
   - Exception handling improved

3. **Assembly References**:
   - New URP assemblies must be referenced
   - Coherence networking assemblies added
   - Some Unity module assemblies restructured

## 4. Networking and Multiplayer Infrastructure

### Coherence Toolkit Integration

Unity 6000 enables the Coherence Toolkit networking implementation:

#### Coherence Assemblies (30+ assemblies)
- `Il2CppCoherence.Core` - Core networking functionality
- `Il2CppCoherence.Toolkit` - Unity integration layer
- `Il2CppCoherence.Transport` - Network transport layer
- `Il2CppCoherence.Runtime` - Runtime networking services

#### Network Time Synchronization
```csharp
public class NetworkTime : Il2CppSystem.Object
{
    // Network time synchronization based on code analysis
    // Ping compensation and client prediction
    // Multi-client mode support
}
```

#### Entity Synchronization
- 512+ generated CoherenceSync files for entity replication
- Network instantiators for different entity types
- Authority management system

### Impact on Modding

1. **Multiplayer Awareness Required**:
   - All mods must check multiplayer state
   - Network entity modifications need special handling
   - Authority-based modifications

2. **Latency Considerations**:
   - Network lag affects timing-sensitive mods
   - Prediction and rollback systems in place
   - Client-server synchronization requirements

## 5. Performance and Asset Considerations

### Unity 6000 Performance Improvements

1. **GPU Utilization**:
   - URP provides rendering
   - Batching reduces draw calls
   - Occlusion culling

2. **Memory Management**:
   - Garbage collection
   - Native collection support
   - Object pooling

3. **CPU Performance**:
   - IL2CPP code generation
   - Method inlining
   - Overhead in interop calls

### Asset Loading Changes

1. **Addressable Assets**:
   - Addressable system integration
   - Asset reference management
   - Loading performance

2. **Shader Compilation**:
   - URP shaders compile differently
   - Shader variants managed by URP
   - Shader stripping

## 6. Modding Migration Guide

### Updating Existing Mods

#### Project References
Add URP references to your mod project:
```xml
<Reference Include="Unity.RenderPipelines.Universal.Runtime" />
<Reference Include="Unity.RenderPipelines.Universal.2D.Runtime" />
<Reference Include="Unity.RenderPipelines.Core.Runtime" />
```

#### Multiplayer Compatibility
Update mod initialization:
```csharp
[HarmonyPatch(typeof(GameManager), "Awake")]
public static void Postfix(GameManager __instance)
{
    // Always check multiplayer state
    if (__instance.IsOnlineMultiplayer)
    {
        // Network-aware modifications only
        return;
    }
    
    if (__instance.IsLocalMultiplayer)
    {
        // Local coop compatible modifications
    }
    
    // Single player modifications
}
```

#### Rendering Updates
Update shader-related code:
```csharp
// Old: Built-in pipeline shader
var shader = Shader.Find("Legacy Shaders/Transparent/Diffuse");

// New: URP shader
var shader = Shader.Find("Universal Render Pipeline/2D/Sprite-Lit-Default");
```

#### Physics Callback Updates
Update collision callbacks for new signatures:
```csharp
// Verify callback parameters match Unity 6000 signatures
private void OnCollisionEnter2D(Collision2D collision)
{
    // Updated collision handling
}
```

### Testing Your Mods

1. **Multi-Mode Testing**:
   - Test in single player
   - Test in local coop (up to 4 players)
   - Test in online multiplayer (if applicable)

2. **Performance Testing**:
   - Monitor performance with URP enabled
   - Check for rendering issues
   - Verify no memory leaks

3. **Compatibility Testing**:
   - Test with other mods
   - Verify IL2CPP interop works correctly
   - Check for Unity API compatibility

## 7. Common Issues and Solutions

### Shader Compatibility
**Issue**: Old shaders don't render correctly
**Solution**: Update to URP-compatible shaders or use URP Converter

### Network Entity Modifications
**Issue**: Modifications don't sync in multiplayer
**Solution**: Check for network authority and use synchronization

### IL2CPP Interop Failures
**Issue**: IL2CPP method calls fail or crash
**Solution**: Verify method signatures match Unity 6000 IL2CPP patterns

### Performance Issues
**Issue**: Mod causes performance issues in URP
**Solution**: Optimize rendering calls and use URP-specific optimizations

## 8. Unity 6000 Specific Features

### New Capabilities Available

1. **Graphics**:
   - Particle systems integration
   - Post-processing effects
   - 2D lighting

2. **Networking Features**:
   - Coherence Toolkit integration
   - Network synchronization
   - Multi-client support

3. **Development Tools**:
   - Debugging capabilities
   - Profiling tools
   - Unity Editor integration

### Features to Avoid in Mods

1. **URP Debug Systems**: Disabled by game for performance
2. **Direct Network Modifications**: Use proper authority checks
3. **Legacy Shader Features**: No longer supported in URP

## 9. Future Considerations

### Unity Version Stability
- Unity 6000.0.36f1 is expected to remain stable for Vampire Survivors
- Future Unity updates unlikely to be as disruptive
- URP is now the standard rendering pipeline

### Modding Ecosystem Evolution
- Focus on multiplayer compatibility
- Networking capabilities for mods
- Performance characteristics for modifications

## 10. Resources and References

### Documentation Links
- [Unity 6000 Release Notes](https://docs.unity3d.com/2023.3/Documentation/Manual/UpgradeGuides.html)
- [URP Documentation](https://docs.unity3d.com/Packages/com.unity.render-pipelines.universal@latest)
- [IL2CPP Scripting Backend](https://docs.unity3d.com/Manual/IL2CPP.html)

### Vampire Survivors Specific Documentation
- `core-architecture.md` - Updated architecture patterns
- `multiplayer-system.md` - Comprehensive multiplayer documentation
- `networking-coherence-toolkit.md` - Coherence integration details
- `performance-guide.md` - URP performance optimization

## Summary

The Unity 6000.0.36f1 upgrade represents a technological advancement in Vampire Survivors' development. While maintaining compatibility with existing modding patterns, it introduces new capabilities through URP rendering, networking via Coherence Toolkit, and IL2CPP performance.

**Key Takeaways for Modders:**
1. **Multiplayer awareness is required** for all mods
2. **URP compatibility** required for rendering-related modifications  
3. **Performance** available through Unity features
4. **Network considerations** for online gameplay
5. **IL2CPP patterns** refined but fundamentally compatible

The upgrade enables Vampire Survivors to support multiplayer gaming while providing modders with tools and performance characteristics.