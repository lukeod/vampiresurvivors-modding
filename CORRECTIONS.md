# Corrections to Vampire Survivors Modding Documentation

## Overview
This document lists corrections to previously documented findings based on verified analysis of the GameAssembly.dll.c native code.

## Critical Corrections

### 1. ❌ 100.0f Damage Capping
**Previous Documentation**: "100.0f capping found in multiple locations"
**Correction**: Only 3 instances found, none directly related to damage capping
- The patterns found are mathematical comparisons, not damage caps
- No evidence of systematic 100.0f damage limiting
- Actual damage caps (if they exist) are hidden in compiled code

### 2. ✅ GM.Core vs GM.Instance
**Previous Documentation**: Correctly stated as `GM.Core`
**Verification**: CONFIRMED - All 20+ references use `Core`, not `Instance`
```c
// Verified pattern
VampireSurvivors_Framework_GM_TypeInfo->static_fields->Core
```

### 3. ✅ EggFloat/EggDouble Wrappers
**Previous Documentation**: Correctly identified wrapper types
**Verification**: CONFIRMED - Complete type definitions found
- `VampireSurvivors_Framework_NumberTypes_EggFloat_o`
- `VampireSurvivors_Framework_NumberTypes_EggDouble_o`
- Both have VTables and field structures

### 4. ⚠️ Memory Offset Patterns
**Previous Documentation**: Listed offsets 0x10, 0x14, 0x18, 0x1c, 0x20, 0x24
**Correction**: CharacterController only shows 3 offsets in actual code:
- 0x10 - Confirmed
- 0x20 - Confirmed  
- 0x50 - Confirmed
- Others were assumptions, not verified

### 5. ❌ Damage Calculation Visibility
**Previous Documentation**: Implied formulas could be seen
**Correction**: NO actual damage formulas visible
- Only field assignments like `InflictedDamage = fVar10`
- Actual calculations hidden in unnamed functions
- IL2CPP compilation obscures all algorithm logic

### 6. ⚠️ PPower Implementation Details
**Previous Documentation**: Suggested PPower calculations could be analyzed
**Correction**: Only VTable entries visible, no implementations
```c
// What we see
struct VirtualInvokeData _45_PPower;  // Just a declaration

// What we DON'T see
// Actual PPower calculation code
```

### 7. ✅ Virtual Method Indices
**Previous Documentation**: PPower at index 45
**Verification**: CONFIRMED
- `_45_PPower`
- `_46_SecondaryPPower`
- `_47_SecondaryCursePPower`

## Important Clarifications

### What IL2CPP Actually Shows

#### We CAN See:
- ✅ Type definitions and structures
- ✅ Virtual method table layouts
- ✅ Some mathematical operations (but without context)
- ✅ Memory access patterns
- ✅ Static field accessors

#### We CANNOT See:
- ❌ Actual method implementations
- ❌ Damage calculation formulas
- ❌ Algorithm logic
- ❌ Original variable names
- ❌ Business logic flow

### Mathematical Operations Reality
**Found**: Basic float operations
```c
fVar17 = fVar1 * fVar17;  // Simple multiplication
fVar5 = fVar5 * fVar1 * fVar6;  // Compound multiplication
```
**NOT Found**: Contextualized damage formulas or stat calculations

## Updated Best Practices

### For Modders
1. **Don't assume formulas are visible** - They're compiled away
2. **Use GetConverted methods** - Still correct advice
3. **Hook at known points** - VTable indices are reliable
4. **Test empirically** - Can't rely on reading the formula

### For Documentation
1. **Verify with tools** - Don't rely on partial grep results
2. **Distinguish between "found" and "assumed"**
3. **IL2CPP hides more than expected** - Update expectations
4. **Native code ≠ readable code** - It's still compiled

## Unchanged Recommendations

These previous recommendations remain valid:
1. ✅ Use `GM.Core` for GameManager access
2. ✅ Use `DataManager.GetConvertedWeapons()` for absolute values
3. ✅ Weapon levels 2-8 use incremental deltas
4. ✅ Avoid hooking OnTick/OnUpdate methods
5. ✅ Use proper IL2Cpp type prefixes
6. ✅ Always null-check IL2CPP objects

## New Insights

### Reality of IL2CPP Analysis
1. **Function names are lost** - Everything is FUN_[address]
2. **Logic is opaque** - Even in native C code
3. **Patterns exist but lack context** - Math without meaning
4. **Structure preserved, behavior hidden** - Can see data, not algorithms

### Improved Analysis Method
Use the `llm_search_helper.py` tool instead of grep:
```python
from llm_search_helper import LLMGameAssemblyHelper
helper = LLMGameAssemblyHelper()

# Efficient, targeted searches
results = helper.quick_search(pattern, "vs", max_lines=20)
func = helper.get_function_by_address(address)
```

## Summary

The core architecture documentation remains largely correct, but claims about visibility into calculation logic were overstated. IL2CPP compilation creates a more opaque system than initially understood - we can see the structure but not the behavior.

**Key Lesson**: Even with access to native C code, IL2CPP effectively obscures implementation details. Modders must rely on:
- Empirical testing
- Hook points
- Data structure manipulation
- Method replacement (not modification)

---
*Document created to correct and clarify previous findings*
*Based on systematic verification using purpose-built analysis tools*