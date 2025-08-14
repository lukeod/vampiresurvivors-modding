using MelonLoader;
using HarmonyLib;
using Il2CppVampireSurvivors.Framework;
using Il2CppVampireSurvivors.Objects.Weapons;
using Il2CppVampireSurvivors.Objects.Projectiles;
using Il2CppVampireSurvivors.Objects.Pools;
using Il2CppVampireSurvivors.Data;
using System;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

[assembly: MelonInfo(typeof(ProjectileManipulator), "Projectile Manipulator", "1.0.0", "VS Modding Community")]
[assembly: MelonGame("poncle", "Vampire Survivors")]

/// <summary>
/// ProjectileManipulator - Demonstrates various approaches to manipulating projectiles.
/// 
/// This mod showcases:
/// - Hooking InitProjectile to modify all projectiles as they're created
/// - Hooking BulletPool.SpawnAt to catch projectiles from the pool
/// - Tracking multi-projectile weapons using indexInWeapon
/// - Modifying projectile properties at runtime
/// - Handling async projectile spawning
/// 
/// Controls:
/// - F5: Toggle projectile tracking
/// - F6: Show projectile statistics
/// - F7: Toggle projectile modifications (double size, rainbow colors)
/// - F8: Show weapon fire patterns
/// </summary>
public class ProjectileManipulator : MelonMod
{
    // Tracking data
    private static bool trackingEnabled = false;
    private static bool modificationsEnabled = false;
    private static bool eventLoggingEnabled = false;
    private static bool hooksInstalled = false;
    private static readonly Dictionary<WeaponType, ProjectileStats> weaponStats = new();
    private static readonly Dictionary<int, ProjectileInfo> activeProjectiles = new();
    private static int totalProjectilesSpawned = 0;
    private static int framesSinceLastSpawn = 0;
    
    // For tracking burst patterns
    private static WeaponType? lastWeaponFired = null;
    private static float lastFireTime = 0;
    private static int burstIndex = 0;
    
    // Event log
    private static readonly List<string> eventLog = new();
    private static int maxLogEntries = 100;
    
    // Harmony instance
    private static HarmonyLib.Harmony harmony;
    
    private class ProjectileStats
    {
        public int TotalSpawned { get; set; }
        public int MaxBurstSize { get; set; }
        public float LastFireTime { get; set; }
        public List<float> SpawnDelays { get; } = new();
    }
    
    private class ProjectileInfo
    {
        public WeaponType WeaponType { get; set; }
        public int IndexInBurst { get; set; }
        public float SpawnTime { get; set; }
        public Vector3 InitialPosition { get; set; }
    }
    
    public override void OnInitializeMelon()
    {
        MelonLogger.Msg("=== Projectile Manipulator Initialized ===");
        MelonLogger.Msg("F5: Toggle Tracking | F6: Show Stats | F7: Toggle Mods | F8: Fire Patterns");
        MelonLogger.Msg("F9: Toggle Event Logging | F10: Show Event Log | F11: Clear Event Log");
        MelonLogger.Msg("Waiting for game session to install hooks...");
        
        harmony = new HarmonyLib.Harmony("ProjectileManipulator");
    }
    
    private void InstallHooks()
    {
        if (hooksInstalled) return;
        
        // Extra safety check - don't try to hook if we're not in gameplay
        if (GM.Core == null || GM.Core.Player == null)
        {
            MelonLogger.Warning("Attempted to install hooks outside of gameplay - aborting");
            return;
        }
        
        try
        {
            MelonLogger.Msg("Starting hook installation...");
            
            // Hook 1: InitProjectile - Called for every projectile initialization
            var initMethod = typeof(Projectile).GetMethod("InitProjectile");
            if (initMethod != null)
            {
                harmony.Patch(initMethod,
                    postfix: new HarmonyMethod(typeof(ProjectileManipulator).GetMethod(nameof(OnInitProjectile))));
                MelonLogger.Msg("Hooked Projectile.InitProjectile");
            }
            else
            {
                MelonLogger.Warning("Could not find Projectile.InitProjectile method");
            }
            
            // Hook 2: BulletPool.SpawnAt - Called when retrieving from pool
            // We need to hook both overloads
            var spawnMethod1 = typeof(BulletPool).GetMethod("SpawnAt", 
                new Type[] { typeof(float), typeof(float), typeof(Weapon), typeof(int) });
            if (spawnMethod1 != null)
            {
                harmony.Patch(spawnMethod1,
                    postfix: new HarmonyMethod(typeof(ProjectileManipulator).GetMethod(nameof(OnSpawnAtXY))));
                MelonLogger.Msg("Hooked BulletPool.SpawnAt(x,y)");
            }
            else
            {
                MelonLogger.Warning("Could not find BulletPool.SpawnAt(x,y) method");
            }
            
            var spawnMethod2 = typeof(BulletPool).GetMethod("SpawnAt",
                new Type[] { typeof(float2), typeof(Weapon), typeof(int) });
            if (spawnMethod2 != null)
            {
                harmony.Patch(spawnMethod2,
                    postfix: new HarmonyMethod(typeof(ProjectileManipulator).GetMethod(nameof(OnSpawnAtFloat2))));
                MelonLogger.Msg("Hooked BulletPool.SpawnAt(float2)");
            }
            else
            {
                MelonLogger.Warning("Could not find BulletPool.SpawnAt(float2) method");
            }
            
            // Hook 3: Weapon.Fire to track firing patterns
            // There are two Fire methods, we want the parameterless one
            var fireMethod = typeof(Weapon).GetMethod("Fire", Type.EmptyTypes);
            if (fireMethod != null)
            {
                harmony.Patch(fireMethod,
                    prefix: new HarmonyMethod(typeof(ProjectileManipulator).GetMethod(nameof(OnWeaponFire))));
                MelonLogger.Msg("Hooked Weapon.Fire()");
            }
            else
            {
                // Try the overload with bool parameter
                fireMethod = typeof(Weapon).GetMethod("Fire", new Type[] { typeof(bool) });
                if (fireMethod != null)
                {
                    harmony.Patch(fireMethod,
                        prefix: new HarmonyMethod(typeof(ProjectileManipulator).GetMethod(nameof(OnWeaponFireWithParam))));
                    MelonLogger.Msg("Hooked Weapon.Fire(bool)");
                }
                else
                {
                    MelonLogger.Warning("Could not find any Weapon.Fire method");
                }
            }
            
            // Hook 4: Projectile.OnUpdate for runtime modifications
            var updateMethod = typeof(Projectile).GetMethod("OnUpdate");
            if (updateMethod != null)
            {
                harmony.Patch(updateMethod,
                    postfix: new HarmonyMethod(typeof(ProjectileManipulator).GetMethod(nameof(OnProjectileUpdate))));
                MelonLogger.Msg("Hooked Projectile.OnUpdate");
            }
            else
            {
                MelonLogger.Warning("Could not find Projectile.OnUpdate method");
            }
            
            hooksInstalled = true;
            MelonLogger.Msg("All hooks installed successfully!");
        }
        catch (Exception ex)
        {
            MelonLogger.Error($"Failed to install hooks: {ex.Message}");
            MelonLogger.Error(ex.StackTrace);
        }
    }
    
    public override void OnUpdate()
    {
        // Check if we need to install hooks (following TracerMod pattern)
        if (!hooksInstalled && GM.Core != null && GM.Core.Player != null)
        {
            // We're in a game session with a player, safe to install hooks
            MelonLogger.Msg("Game session detected - installing projectile hooks...");
            InstallHooks();
        }
        else if (hooksInstalled && GM.Core == null)
        {
            // We've returned to main menu, mark hooks as needing reinstall
            MelonLogger.Msg("Returned to main menu - hooks will be reinstalled next game");
            hooksInstalled = false;
            // Clear tracking data
            weaponStats.Clear();
            activeProjectiles.Clear();
            totalProjectilesSpawned = 0;
        }
        
        framesSinceLastSpawn++;
        
        // Handle input
        if (Input.GetKeyDown(KeyCode.F5))
        {
            trackingEnabled = !trackingEnabled;
            MelonLogger.Msg($"Projectile tracking: {(trackingEnabled ? "ENABLED" : "DISABLED")}");
            if (!trackingEnabled)
            {
                weaponStats.Clear();
                activeProjectiles.Clear();
            }
        }
        
        if (Input.GetKeyDown(KeyCode.F6))
        {
            ShowProjectileStats();
        }
        
        if (Input.GetKeyDown(KeyCode.F7))
        {
            modificationsEnabled = !modificationsEnabled;
            MelonLogger.Msg($"Projectile modifications: {(modificationsEnabled ? "ENABLED" : "DISABLED")}");
        }
        
        if (Input.GetKeyDown(KeyCode.F8))
        {
            ShowFirePatterns();
        }
        
        if (Input.GetKeyDown(KeyCode.F9))
        {
            eventLoggingEnabled = !eventLoggingEnabled;
            MelonLogger.Msg($"Event logging: {(eventLoggingEnabled ? "ENABLED" : "DISABLED")}");
            if (eventLoggingEnabled)
            {
                LogEvent("=== Event Logging Started ===");
            }
        }
        
        if (Input.GetKeyDown(KeyCode.F10))
        {
            ShowEventLog();
        }
        
        if (Input.GetKeyDown(KeyCode.F11))
        {
            eventLog.Clear();
            MelonLogger.Msg("Event log cleared");
        }
        
        // Clean up old projectile tracking data
        if (trackingEnabled && Time.time % 5 < 0.02f) // Every 5 seconds
        {
            var toRemove = new List<int>();
            foreach (var kvp in activeProjectiles)
            {
                if (Time.time - kvp.Value.SpawnTime > 10f) // Remove after 10 seconds
                {
                    toRemove.Add(kvp.Key);
                }
            }
            foreach (var key in toRemove)
            {
                activeProjectiles.Remove(key);
            }
        }
    }
    
    /// <summary>
    /// Logs an event with timestamp
    /// </summary>
    private static void LogEvent(string message)
    {
        if (!eventLoggingEnabled) return;
        
        string timestampedMessage = $"[{Time.time:F3}] {message}";
        eventLog.Add(timestampedMessage);
        
        // Keep log size manageable
        if (eventLog.Count > maxLogEntries)
        {
            eventLog.RemoveAt(0);
        }
        
        // Also log to MelonLogger for immediate visibility
        MelonLogger.Msg($"[EVENT] {timestampedMessage}");
    }
    
    /// <summary>
    /// Hook: Called when a projectile is initialized.
    /// This is THE BEST HOOK for modifying projectiles, as it provides the index parameter.
    /// </summary>
    public static void OnInitProjectile(Projectile __instance, BulletPool pool, Weapon weapon, int index)
    {
        try
        {
            // Get weapon type from CurrentWeaponData
            var weaponData = weapon.CurrentWeaponData;
            if (weaponData == null) return;
            var weaponType = weaponData.bulletType;
            
            // Log the event
            LogEvent($"InitProjectile: {weaponType} [index={index}] at {__instance.transform.position}");
            
            if (!trackingEnabled) return;
            
            totalProjectilesSpawned++;
            
            // Track statistics
            if (!weaponStats.ContainsKey(weaponType))
            {
                weaponStats[weaponType] = new ProjectileStats();
            }
            
            var stats = weaponStats[weaponType];
            stats.TotalSpawned++;
            
            // Track burst patterns
            if (lastWeaponFired == weaponType && Time.time - lastFireTime < 0.5f)
            {
                burstIndex++;
                if (burstIndex > stats.MaxBurstSize)
                {
                    stats.MaxBurstSize = burstIndex;
                }
                
                // Track delay between projectiles in burst
                if (stats.LastFireTime > 0)
                {
                    stats.SpawnDelays.Add(Time.time - stats.LastFireTime);
                }
            }
            else
            {
                burstIndex = 0;
            }
            
            stats.LastFireTime = Time.time;
            lastWeaponFired = weaponType;
            lastFireTime = Time.time;
            
            // Store projectile info
            var projectileId = __instance.GetInstanceID();
            activeProjectiles[projectileId] = new ProjectileInfo
            {
                WeaponType = weaponType,
                IndexInBurst = index,
                SpawnTime = Time.time,
                InitialPosition = __instance.transform.position
            };
            
            // Log interesting multi-projectile weapons
            if (index > 0)
            {
                MelonLogger.Msg($"Multi-projectile: {weaponType} spawned projectile #{index}");
            }
            
            // Apply modifications if enabled
            if (modificationsEnabled)
            {
                ApplyProjectileModifications(__instance, weapon, index);
            }
        }
        catch (Exception ex)
        {
            MelonLogger.Error($"Error in OnInitProjectile: {ex.Message}");
        }
    }
    
    /// <summary>
    /// Hook: Called when BulletPool spawns a projectile at x,y position.
    /// This happens BEFORE InitProjectile.
    /// </summary>
    public static void OnSpawnAtXY(BulletPool __instance, ref Projectile __result, 
        float x, float y, Weapon weapon, int index)
    {
        if (__result == null) return;
        
        if (!trackingEnabled && !eventLoggingEnabled) return;
        
        // Log the event AFTER we return from this method (SpawnAt happens first)
        var weaponData = weapon?.CurrentWeaponData;
        if (weaponData != null)
        {
            LogEvent($"SpawnAt(x,y): {weaponData.bulletType} [index={index}] at ({x:F1}, {y:F1})");
        }
        else
        {
            LogEvent($"SpawnAt(x,y): Unknown weapon [index={index}] at ({x:F1}, {y:F1})");
        }
        
        if (!trackingEnabled) return;
        
        framesSinceLastSpawn = 0;
        
        if (modificationsEnabled)
        {
            // Early modifications before initialization
            // Note: Some properties might get overwritten during InitProjectile
            __result.transform.localScale *= 2f; // Double size
        }
    }
    
    /// <summary>
    /// Hook: Called when BulletPool spawns a projectile at float2 position.
    /// This happens BEFORE InitProjectile.
    /// </summary>
    public static void OnSpawnAtFloat2(BulletPool __instance, ref Projectile __result,
        float2 pos, Weapon weapon, int index)
    {
        if (__result == null) return;
        
        if (!trackingEnabled && !eventLoggingEnabled) return;
        
        // Log the event
        var weaponData = weapon?.CurrentWeaponData;
        if (weaponData != null)
        {
            LogEvent($"SpawnAt(float2): {weaponData.bulletType} [index={index}] at ({pos.x:F1}, {pos.y:F1})");
        }
        else
        {
            LogEvent($"SpawnAt(float2): Unknown weapon [index={index}] at ({pos.x:F1}, {pos.y:F1})");
        }
        
        if (!trackingEnabled) return;
        
        framesSinceLastSpawn = 0;
        
        if (modificationsEnabled)
        {
            // Early modifications before initialization
            __result.transform.localScale *= 2f; // Double size
        }
    }
    
    /// <summary>
    /// Hook: Called when a weapon fires (parameterless version).
    /// This happens BEFORE projectiles are spawned.
    /// </summary>
    public static void OnWeaponFire(Weapon __instance)
    {
        try
        {
            var weaponData = __instance.CurrentWeaponData;
            if (weaponData == null) return;
            var weaponType = weaponData.bulletType;
            var amount = __instance.PAmount(); // Get the calculated amount
            
            LogEvent($"Weapon.Fire(): {weaponType} preparing to fire {amount} projectiles");
            
            if (!trackingEnabled) return;
            
            // The actual projectile spawning happens asynchronously via delegates
            // This is why we need to track them in InitProjectile
        }
        catch (Exception ex)
        {
            MelonLogger.Error($"Error in OnWeaponFire: {ex.Message}");
        }
    }
    
    /// <summary>
    /// Hook: Called when a weapon fires (with skipTriggers parameter).
    /// This happens BEFORE projectiles are spawned.
    /// </summary>
    public static void OnWeaponFireWithParam(Weapon __instance, bool skipTriggers)
    {
        try
        {
            var weaponData = __instance.CurrentWeaponData;
            if (weaponData == null) return;
            var weaponType = weaponData.bulletType;
            var amount = __instance.PAmount(); // Get the calculated amount
            
            LogEvent($"Weapon.Fire(skipTriggers={skipTriggers}): {weaponType} preparing to fire {amount} projectiles");
            
            if (!trackingEnabled) return;
            
            // The actual projectile spawning happens asynchronously via delegates
            // This is why we need to track them in InitProjectile
        }
        catch (Exception ex)
        {
            MelonLogger.Error($"Error in OnWeaponFireWithParam: {ex.Message}");
        }
    }
    
    /// <summary>
    /// Hook: Called every frame for active projectiles.
    /// WARNING: This is performance-critical! Keep it lightweight!
    /// </summary>
    public static void OnProjectileUpdate(Projectile __instance)
    {
        if (!modificationsEnabled) return;
        
        // Only apply visual effects every few frames to reduce performance impact
        if (Time.frameCount % 5 != 0) return;
        
        try
        {
            var projectileId = __instance.GetInstanceID();
            if (activeProjectiles.ContainsKey(projectileId))
            {
                // Rainbow color effect based on time
                var renderer = __instance._renderer;
                if (renderer != null)
                {
                    float hue = (Time.time * 0.5f + projectileId * 0.1f) % 1f;
                    renderer.color = Color.HSVToRGB(hue, 1f, 1f);
                }
            }
        }
        catch
        {
            // Silently ignore errors in update to avoid spam
        }
    }
    
    /// <summary>
    /// Applies modifications to a projectile based on its properties.
    /// </summary>
    private static void ApplyProjectileModifications(Projectile projectile, Weapon weapon, int index)
    {
        try
        {
            // Modify based on index in burst
            if (index > 0)
            {
                // Make later projectiles in burst larger
                float scaleFactor = 1f + (index * 0.2f);
                projectile.transform.localScale *= scaleFactor;
            }
            
            // Modify projectile speed
            if (projectile._speed > 0)
            {
                projectile._speed *= 1.5f; // 50% faster
            }
            
            // Modify penetration
            if (projectile._penetrating < 10)
            {
                projectile._penetrating += 2; // Extra penetration
            }
            
            // Add visual indicator
            var renderer = projectile._renderer;
            if (renderer != null)
            {
                // Tint based on weapon type
                var weaponData = weapon.CurrentWeaponData;
                if (weaponData == null) return;
                var weaponType = weaponData.bulletType;
                if (weaponType.ToString().Contains("FIRE"))
                {
                    renderer.color = new Color(1f, 0.5f, 0f); // Orange for fire weapons
                }
                else if (weaponType.ToString().Contains("ICE"))
                {
                    renderer.color = new Color(0.5f, 0.8f, 1f); // Light blue for ice
                }
                else if (weaponType.ToString().Contains("LIGHTNING"))
                {
                    renderer.color = new Color(1f, 1f, 0.5f); // Yellow for lightning
                }
            }
        }
        catch (Exception ex)
        {
            MelonLogger.Error($"Error applying modifications: {ex.Message}");
        }
    }
    
    private void ShowProjectileStats()
    {
        MelonLogger.Msg("=== Projectile Statistics ===");
        MelonLogger.Msg($"Total projectiles spawned: {totalProjectilesSpawned}");
        MelonLogger.Msg($"Active tracked projectiles: {activeProjectiles.Count}");
        MelonLogger.Msg($"Frames since last spawn: {framesSinceLastSpawn}");
        
        if (weaponStats.Count > 0)
        {
            MelonLogger.Msg("\nWeapon Statistics:");
            foreach (var kvp in weaponStats)
            {
                var weapon = kvp.Key;
                var stats = kvp.Value;
                MelonLogger.Msg($"  {weapon}:");
                MelonLogger.Msg($"    Total spawned: {stats.TotalSpawned}");
                MelonLogger.Msg($"    Max burst size: {stats.MaxBurstSize + 1}");
                
                if (stats.SpawnDelays.Count > 0)
                {
                    float avgDelay = 0;
                    foreach (var delay in stats.SpawnDelays)
                    {
                        avgDelay += delay;
                    }
                    avgDelay /= stats.SpawnDelays.Count;
                    MelonLogger.Msg($"    Avg delay between projectiles: {avgDelay * 1000:F1}ms");
                }
            }
        }
        
        MelonLogger.Msg("=== End Statistics ===");
    }
    
    private void ShowFirePatterns()
    {
        MelonLogger.Msg("=== Weapon Fire Patterns ===");
        
        var gm = GM.Core;
        if (gm?.Player == null)
        {
            MelonLogger.Msg("Not in game");
            return;
        }
        
        var weaponsManager = gm.Player.WeaponsManager;
        if (weaponsManager != null)
        {
            var weapons = weaponsManager.ActiveEquipment;
            if (weapons != null && weapons.Count > 0)
            {
                MelonLogger.Msg($"Player has {weapons.Count} weapons:");
                foreach (var equipment in weapons)
                {
                    if (equipment == null) continue;
                    
                    // Cast Equipment to Weapon
                    var weapon = equipment.TryCast<Weapon>();
                    if (weapon == null) continue;
                    
                    var weaponData = weapon.CurrentWeaponData;
                    if (weaponData == null) continue;
                    var weaponType = weaponData.bulletType;
                    var amount = weapon.PAmount();
                    var interval = weapon.PInterval();
                    var duration = weapon.PDuration();
                    
                    MelonLogger.Msg($"  {weaponType}:");
                    MelonLogger.Msg($"    Amount: {amount}");
                    MelonLogger.Msg($"    Interval: {interval:F0}ms ({interval/1000f:F2}s)");
                    MelonLogger.Msg($"    Duration: {duration:F0}ms ({duration/1000f:F2}s)");
                    
                    // Check if this weapon has fired multiple projectiles
                    if (weaponStats.ContainsKey(weaponType))
                    {
                        var stats = weaponStats[weaponType];
                        if (stats.MaxBurstSize > 0)
                        {
                            MelonLogger.Msg($"    Observed burst size: {stats.MaxBurstSize + 1}");
                        }
                    }
                }
            }
        }
        else
        {
            MelonLogger.Msg("No weapons equipped");
        }
        
        MelonLogger.Msg("=== End Fire Patterns ===");
    }
    
    private void ShowEventLog()
    {
        MelonLogger.Msg("=== Projectile Event Log ===");
        
        if (eventLog.Count == 0)
        {
            MelonLogger.Msg("No events logged. Press F9 to enable event logging.");
        }
        else
        {
            MelonLogger.Msg($"Showing last {eventLog.Count} events:");
            
            // Show the last 20 events or all if less than 20
            int startIndex = Math.Max(0, eventLog.Count - 20);
            for (int i = startIndex; i < eventLog.Count; i++)
            {
                MelonLogger.Msg(eventLog[i]);
            }
            
            if (eventLog.Count > 20)
            {
                MelonLogger.Msg($"... {eventLog.Count - 20} earlier events not shown (see full log with larger display)");
            }
        }
        
        MelonLogger.Msg("=== End Event Log ===");
    }
    
    public override void OnApplicationQuit()
    {
        MelonLogger.Msg($"Projectile Manipulator shutting down");
        MelonLogger.Msg($"Total projectiles tracked this session: {totalProjectilesSpawned}");
    }
}