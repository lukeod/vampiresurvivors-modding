// SignalInterceptor.cs - Solution for intercepting non-blittable IL2CPP signals
// Bypasses IL2CPP marshaling limitations by patching SignalBus.InternalFire directly
// Part of Signal Inspector Mod v3.2.0

using MelonLoader;
using HarmonyLib;
using Il2CppVampireSurvivors.Framework;
using Il2CppVampireSurvivors.Signals;
using Il2CppVampireSurvivors.Objects.Characters;
using Il2CppZenject;
using System;
using System.Reflection;

/// <summary>
/// Intercepts non-blittable signals using Harmony patches at their fire points.
/// This bypasses the IL2CPP marshaling limitations for delegates.
/// </summary>
public static class SignalInterceptor
{
    private static bool _interceptDamage = false;
    private static bool _interceptWeapons = false;
    private static bool _interceptAll = false;
    private static bool _verboseMode = false;
    
    // Delegate for signal tracking callback
    public static Action<string> OnSignalFired;
    
    public static void Initialize()
    {
        MelonLogger.Msg("[SignalInterceptor] Initializing signal interception patches");
        
        try
        {
            var harmony = new HarmonyLib.Harmony("SignalInterceptor");
            var signalBusType = typeof(SignalBus);
            
            // Find InternalFire with specific parameters
            var internalFireMethod = signalBusType.GetMethod("InternalFire", 
                BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance,
                null,
                new Type[] { 
                    typeof(Il2CppSystem.Type), 
                    typeof(Il2CppSystem.Object), 
                    typeof(Il2CppSystem.Object), 
                    typeof(bool) 
                },
                null);
            
            if (internalFireMethod != null)
            {
                harmony.Patch(internalFireMethod,
                    prefix: new HarmonyMethod(typeof(SignalInterceptor).GetMethod(nameof(OnInternalFire))));
                MelonLogger.Msg("[SignalInterceptor] Successfully patched SignalBus.InternalFire");
            }
            else
            {
                MelonLogger.Error("[SignalInterceptor] Could not find InternalFire method to patch!");
            }
        }
        catch (Exception ex)
        {
            MelonLogger.Error($"[SignalInterceptor] Failed to initialize: {ex.Message}");
        }
    }
    
    public static bool OnInternalFire(Il2CppSystem.Type signalType, Il2CppSystem.Object signal, 
        Il2CppSystem.Object identifier, bool requireDeclaration)
    {
        var signalTypeName = signalType?.Name ?? "Unknown";
        
        // Always track signal for statistics (not just when intercepting)
        OnSignalFired?.Invoke(signalTypeName);
        
        // Log all signals in verbose mode to see what's firing
        if (_verboseMode)
        {
            // Only show ID if it's not null (rare case)
            if (identifier != null)
            {
                MelonLogger.Msg($"[Signal] {signalTypeName} fired (ID: {identifier})");
            }
            else
            {
                MelonLogger.Msg($"[Signal] {signalTypeName} fired");
            }
        }
        
        // Check if we should intercept specific signals
        if (!_interceptAll) return true;
        
        // Check if this is one of our target signals
        if (signalTypeName == "CharacterReceivedDamageSignal" && _interceptDamage)
        {
            try
            {
                // Cast and extract data
                var damageSignal = signal.Cast<GameplaySignals.CharacterReceivedDamageSignal>();
                if (damageSignal != null && damageSignal.Character != null)
                {
                    var character = damageSignal.Character;
                    MelonLogger.Msg("=== DAMAGE SIGNAL INTERCEPTED ===");
                    MelonLogger.Msg($"  Character Type: {character.CharacterType}");
                    MelonLogger.Msg($"  Health: {character.CurrentHealth()}");
                    MelonLogger.Msg($"  Level: {character.Level}");
                    MelonLogger.Msg("=================================");
                }
            }
            catch (Exception ex)
            {
                MelonLogger.Warning($"[SignalInterceptor] Failed to process damage signal: {ex.Message}");
            }
        }
        else if ((signalTypeName == "AddWeaponToCharacterSignal" || 
                  signalTypeName == "WeaponAddedToCharacterSignal") && _interceptWeapons)
        {
            try
            {
                if (signalTypeName == "AddWeaponToCharacterSignal")
                {
                    var weaponSignal = signal.Cast<GameplaySignals.AddWeaponToCharacterSignal>();
                    if (weaponSignal != null)
                    {
                        MelonLogger.Msg("=== ADD WEAPON SIGNAL INTERCEPTED ===");
                        if (weaponSignal.Character != null)
                            MelonLogger.Msg($"  Character Type: {weaponSignal.Character.CharacterType}");
                        MelonLogger.Msg($"  Weapon Type: {weaponSignal.Weapon}");
                        MelonLogger.Msg("=====================================");
                    }
                }
                else
                {
                    var weaponSignal = signal.Cast<GameplaySignals.WeaponAddedToCharacterSignal>();
                    if (weaponSignal != null)
                    {
                        MelonLogger.Msg("=== WEAPON ADDED SIGNAL INTERCEPTED ===");
                        if (weaponSignal.Character != null)
                            MelonLogger.Msg($"  Character Type: {weaponSignal.Character.CharacterType}");
                        MelonLogger.Msg($"  Weapon Type: {weaponSignal.Weapon}");
                        MelonLogger.Msg("=======================================");
                    }
                }
            }
            catch (Exception ex)
            {
                MelonLogger.Warning($"[SignalInterceptor] Failed to process weapon signal: {ex.Message}");
            }
        }
        
        return true; // Let the original method run
    }
    
    public static void EnableDamageInterception(bool enable)
    {
        _interceptDamage = enable;
        _interceptAll = enable || _interceptWeapons;
        MelonLogger.Msg($"[SignalInterceptor] Damage interception: {(enable ? "ENABLED" : "DISABLED")}");
    }
    
    public static void EnableWeaponInterception(bool enable)
    {
        _interceptWeapons = enable;
        _interceptAll = enable || _interceptDamage;
        MelonLogger.Msg($"[SignalInterceptor] Weapon interception: {(enable ? "ENABLED" : "DISABLED")}");
    }
    
    public static void EnableAllInterception(bool enable)
    {
        _interceptAll = enable;
        _interceptDamage = enable;
        _interceptWeapons = enable;
        MelonLogger.Msg($"[SignalInterceptor] All interception: {(enable ? "ENABLED" : "DISABLED")}");
    }
    
    public static void SetVerboseMode(bool verbose)
    {
        _verboseMode = verbose;
    }
}