using MelonLoader;
using HarmonyLib;
using Il2CppVampireSurvivors;
using Il2CppVampireSurvivors.Framework;
using Il2CppVampireSurvivors.Signals;
using Il2CppVampireSurvivors.Objects.Characters;
using Il2CppVampireSurvivors.UI;
using Il2CppZenject;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

[assembly: MelonInfo(typeof(SignalInspectorMod), "Signal Inspector", "3.2.0", "VS Modding Community")]
[assembly: MelonGame("poncle", "Vampire Survivors")]

public class SignalInspectorMod : MelonMod
{
    private static SignalBus _signalBus;
    private static DiContainer _container;
    
    private static readonly Dictionary<string, int> _signalFireCounts = new Dictionary<string, int>();
    private static readonly List<string> _signalLog = new List<string>();
    private static bool _loggingEnabled = true;
    private static bool _verboseMode = false;
    
    private static readonly HashSet<string> _declaredSignals = new HashSet<string>();
    
    private static bool _traceXpFlow = false;
    private static bool _interceptDamage = false;
    private static bool _interceptWeapons = false;

    public override void OnInitializeMelon()
    {
        MelonLogger.Msg("========================================");
        MelonLogger.Msg("Signal Inspector Mod v3.2.0 Initialized");
        MelonLogger.Msg("========================================");
        MelonLogger.Msg("Inspector Controls:");
        MelonLogger.Msg("  F9: Toggle signal logging");
        MelonLogger.Msg("  F10: Dump signal statistics");
        MelonLogger.Msg("  F11: Toggle verbose mode");
        MelonLogger.Msg("  F12: Show signal system info");
        MelonLogger.Msg("  Tab: Show current state");
        MelonLogger.Msg("");
        MelonLogger.Msg("Flow Tracing:");
        MelonLogger.Msg("  Numpad 1: Toggle XP flow tracing (working via subscription)");
        MelonLogger.Msg("  Numpad 2: Toggle Damage interception (NEW!)");
        MelonLogger.Msg("  Numpad 3: Toggle Weapon interception (NEW!)");
        MelonLogger.Msg("  Numpad 0: Toggle ALL interception");
        MelonLogger.Msg("");
        MelonLogger.Msg("Analysis:");
        MelonLogger.Msg("  F8: Show known signals");
        MelonLogger.Msg("  F7: Signal declaration info");
        MelonLogger.Msg("  F6: Signal system analysis");
        MelonLogger.Msg("========================================");
        
        // Initialize the signal interceptor for non-blittable signals
        SignalInterceptor.Initialize();
        
        // Register callback to track ALL signals that fire
        SignalInterceptor.OnSignalFired = (signalName) => {
            if (!_signalFireCounts.ContainsKey(signalName))
                _signalFireCounts[signalName] = 0;
            _signalFireCounts[signalName]++;
            
            if (_loggingEnabled && _verboseMode)
            {
                MelonLogger.Msg($"[ALL SIGNALS] {signalName} fired");
            }
        };
        
        InstallPatches();
    }
    
    private void InstallPatches()
    {
        try
        {
            var harmony = new HarmonyLib.Harmony("SignalInspectorHarmony");
            harmony.PatchAll();
            MelonLogger.Msg("Harmony patches installed successfully");
        }
        catch (Exception ex)
        {
            MelonLogger.Error($"Failed to install patches: {ex.Message}");
        }
    }
    
    public override void OnSceneWasLoaded(int buildIndex, string sceneName)
    {
        MelonLogger.Msg($"Scene loaded: {sceneName} (index: {buildIndex})");
        
        if (sceneName == "Gameplay" && _signalBus == null)
        {
            MelonLogger.Msg("[Signal Inspector] Gameplay scene loaded, checking for GM.Core");
            var gameManager = GM.Core;
            if (gameManager != null)
            {
                MelonLogger.Msg("[Signal Inspector] GM.Core is available!");
                TryGetSignalBusFromGameManager(gameManager);
            }
        }
    }
    
    private static void TryGetSignalBusFromGameManager(GameManager gameManager)
    {
        if (gameManager == null || _signalBus != null) return;
        
        MelonLogger.Msg("[Signal Inspector] Searching for SignalBus in GameManager");
        
        var gmType = gameManager.GetType();
        var fields = gmType.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);
        
        MelonLogger.Msg($"[Signal Inspector] Found {fields.Length} fields in GameManager");
        
        // Log ALL fields when verbose
        foreach (var field in fields)
        {
            try
            {
                var value = field.IsStatic ? field.GetValue(null) : field.GetValue(gameManager);
                MelonLogger.Msg($"  Field: {field.Name} ({field.FieldType.Name}) = {value?.GetType().Name ?? "null"}");
            }
            catch (Exception ex)
            {
                MelonLogger.Msg($"  Field: {field.Name} - Error reading: {ex.Message}");
            }
            
            if (field.FieldType == typeof(SignalBus))
            {
                var signalBus = field.IsStatic ? field.GetValue(null) as SignalBus : field.GetValue(gameManager) as SignalBus;
                if (signalBus != null)
                {
                    _signalBus = signalBus;
                    MelonLogger.Msg($"[Signal Inspector] Found SignalBus in field: {field.Name}");
                    MelonLogger.Msg($"SignalBus captured! NumSubscribers: {signalBus.NumSubscribers}");
                    
                    SubscribeToAllSignals(signalBus);
                    SetupFlowTracers(signalBus);
                    InspectSignalBusInternals(signalBus);
                    break;
                }
            }
            
            if (field.FieldType == typeof(DiContainer) && _container == null)
            {
                var container = field.IsStatic ? field.GetValue(null) as DiContainer : field.GetValue(gameManager) as DiContainer;
                if (container != null)
                {
                    _container = container;
                    MelonLogger.Msg($"[Signal Inspector] Found DiContainer in field: {field.Name}");
                    ExploreContainer(container);
                }
            }
        }
        
        // If not found in fields, try properties
        if (_signalBus == null)
        {
            MelonLogger.Msg("[Signal Inspector] Checking properties...");
            var properties = gmType.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            MelonLogger.Msg($"[Signal Inspector] Found {properties.Length} properties");
            
            foreach (var prop in properties)
            {
                try
                {
                    if (prop.PropertyType == typeof(SignalBus) && prop.CanRead)
                    {
                        var signalBus = prop.GetValue(gameManager) as SignalBus;
                        if (signalBus != null)
                        {
                            _signalBus = signalBus;
                            MelonLogger.Msg($"[Signal Inspector] Found SignalBus in property: {prop.Name}");
                            MelonLogger.Msg($"SignalBus captured! NumSubscribers: {signalBus.NumSubscribers}");
                            
                            SubscribeToAllSignals(signalBus);
                            SetupFlowTracers(signalBus);
                            InspectSignalBusInternals(signalBus);
                            break;
                        }
                    }
                }
                catch { }
            }
        }
        
        // Try to find SignalBus through DiContainer if we have one
        if (_signalBus == null && _container != null)
        {
            MelonLogger.Msg("[Signal Inspector] Trying to resolve SignalBus from DiContainer...");
            try
            {
                var signalBus = _container.Resolve<SignalBus>();
                if (signalBus != null)
                {
                    _signalBus = signalBus;
                    MelonLogger.Msg("[Signal Inspector] Found SignalBus via DiContainer.Resolve!");
                    MelonLogger.Msg($"SignalBus captured! NumSubscribers: {signalBus.NumSubscribers}");
                    
                    SubscribeToAllSignals(signalBus);
                    SetupFlowTracers(signalBus);
                    InspectSignalBusInternals(signalBus);
                }
            }
            catch (Exception ex)
            {
                MelonLogger.Msg($"[Signal Inspector] Failed to resolve SignalBus: {ex.Message}");
            }
        }
        
        if (_signalBus == null)
        {
            MelonLogger.Warning("[Signal Inspector] SignalBus not found - will retry on next opportunity");
        }
    }

    [HarmonyPatch(typeof(GM), "set_Core")]
    [HarmonyPostfix]
    public static void OnGMCoreSet(GameManager value)
    {
        if (value != null && _signalBus == null)
        {
            MelonLogger.Msg("[Signal Inspector] GM.Core was set!");
            TryGetSignalBusFromGameManager(value);
        }
    }
    
    [HarmonyPatch(typeof(GameManager), "Awake")]
    [HarmonyPostfix]
    public static void OnGameManagerAwake(GameManager __instance)
    {
        MelonLogger.Msg("[Signal Inspector] GameManager.Awake called");
        TryGetSignalBusFromGameManager(__instance);
    }
    
    [HarmonyPatch(typeof(GameManager), "AddStartingWeapon")]
    [HarmonyPostfix]
    public static void OnAddStartingWeapon(GameManager __instance)
    {
        if (_signalBus == null)
        {
            MelonLogger.Msg("[Signal Inspector] Attempting to get SignalBus from AddStartingWeapon hook");
            TryGetSignalBusFromGameManager(__instance);
        }
    }

    private static void SubscribeToAllSignals(SignalBus signalBus)
    {
        if (signalBus == null) return;

        MelonLogger.Msg("Subscribing to signals for monitoring...");
        int successCount = 0;
        int failCount = 0;
        
        // Simple signals without parameters
        try
        {
            signalBus.Subscribe<GameplaySignals.InitializeGameSessionSignal>((Action)(() => LogSignal("InitializeGameSessionSignal")));
            successCount++;
        }
        catch (Exception ex)
        {
            MelonLogger.Msg($"  Failed to subscribe to InitializeGameSessionSignal: {ex.Message}");
            failCount++;
        }
        
        try
        {
            signalBus.Subscribe<GameplaySignals.PreInitializeGameSessionSignal>((Action)(() => LogSignal("PreInitializeGameSessionSignal")));
            successCount++;
        }
        catch (Exception ex)
        {
            MelonLogger.Msg($"  Failed to subscribe to PreInitializeGameSessionSignal: {ex.Message}");
            failCount++;
        }
        
        try
        {
            signalBus.Subscribe<GameplaySignals.GameSessionInitializedSignal>((Action)(() => LogSignal("GameSessionInitializedSignal")));
            successCount++;
        }
        catch (Exception ex)
        {
            MelonLogger.Msg($"  Failed to subscribe to GameSessionInitializedSignal: {ex.Message}");
            failCount++;
        }
        
        // Signals with parameters - these may fail due to non-blittable structs
        try
        {
            signalBus.Subscribe<GameplaySignals.CharacterXpChangedSignal>((Action<GameplaySignals.CharacterXpChangedSignal>)((signal) => 
            {
                LogSignal($"CharacterXpChangedSignal - XP: {signal.CurrentXp}/{signal.MaxXp}");
            }));
            successCount++;
        }
        catch (Exception ex)
        {
            MelonLogger.Msg($"  Failed to subscribe to CharacterXpChangedSignal: {ex.Message}");
            failCount++;
        }
        
        // Skip complex signals that cause non-blittable errors
        // Instead, we'll rely on the Harmony patches to catch these
        
        MelonLogger.Msg($"Subscribed to {successCount} signals, {failCount} failed (this is normal for complex signals)");
    }

    private static void SetupFlowTracers(SignalBus signalBus)
    {
        if (signalBus == null) return;

        MelonLogger.Msg("Setting up flow tracers...");
        
        // XP Flow - this one works
        try
        {
            signalBus.Subscribe<GameplaySignals.CharacterXpChangedSignal>((Action<GameplaySignals.CharacterXpChangedSignal>)((signal) => 
            {
                if (_traceXpFlow)
                {
                    MelonLogger.Msg("=== XP SIGNAL FLOW ===");
                    MelonLogger.Msg($"[1] Signal Fired: CharacterXpChangedSignal");
                    MelonLogger.Msg($"    Current XP: {signal.CurrentXp}");
                    MelonLogger.Msg($"    Max XP: {signal.MaxXp}");
                    MelonLogger.Msg($"    Progress: {(signal.CurrentXp / signal.MaxXp * 100f):F1}%");
                    MelonLogger.Msg("======================");
                }
            }));
            MelonLogger.Msg("  ✓ XP flow tracer subscribed");
        }
        catch (Exception ex)
        {
            MelonLogger.Msg($"  ✗ XP flow tracer failed: {ex.Message}");
        }

        // Note: Weapon and Damage flow tracers don't work due to non-blittable struct issues
        // Only XP tracing works via signal subscription
    }

    private static void InspectSignalBusInternals(SignalBus signalBus)
    {
        if (signalBus == null) return;

        try
        {
            var signalBusType = signalBus.GetType();
            var fields = signalBusType.GetFields(BindingFlags.NonPublic | BindingFlags.Instance);
            
            MelonLogger.Msg($"SignalBus has {fields.Length} private fields:");
            foreach (var field in fields.Take(10))
            {
                try
                {
                    var value = field.GetValue(signalBus);
                    MelonLogger.Msg($"  - {field.Name}: {value?.GetType().Name ?? "null"}");
                }
                catch
                {
                    MelonLogger.Msg($"  - {field.Name}: <inaccessible>");
                }
            }

            var subscriptionField = fields.FirstOrDefault(f => 
                f.Name.Contains("subscription", StringComparison.OrdinalIgnoreCase) ||
                f.Name.Contains("_subscriptions", StringComparison.OrdinalIgnoreCase));
            
            if (subscriptionField != null)
            {
                var subscriptions = subscriptionField.GetValue(signalBus);
                MelonLogger.Msg($"Found subscriptions field: {subscriptionField.Name}");
            }
        }
        catch (Exception ex)
        {
            MelonLogger.Error($"Error inspecting SignalBus internals: {ex.Message}");
        }
    }

    private static void ExploreContainer(DiContainer container)
    {
        if (container == null) return;

        try
        {
            var containerType = container.GetType();
            var fields = containerType.GetFields(BindingFlags.NonPublic | BindingFlags.Instance);
            
            foreach (var field in fields)
            {
                if (field.Name.Contains("binding", StringComparison.OrdinalIgnoreCase) ||
                    field.Name.Contains("signal", StringComparison.OrdinalIgnoreCase))
                {
                    try
                    {
                        var value = field.GetValue(container);
                        if (value != null)
                        {
                            MelonLogger.Msg($"Container field: {field.Name} = {value.GetType().Name}");
                        }
                    }
                    catch { }
                }
            }

            var bindingsField = fields.FirstOrDefault(f => 
                f.Name == "_bindings" || 
                f.Name.Contains("allBindings", StringComparison.OrdinalIgnoreCase));
            
            if (bindingsField != null)
            {
                var bindings = bindingsField.GetValue(container);
                MelonLogger.Msg($"Found bindings collection: {bindings?.GetType().Name}");
            }
        }
        catch (Exception ex)
        {
            MelonLogger.Error($"Error exploring container: {ex.Message}");
        }
    }

    private static void LogSignal(string signalInfo)
    {
        if (!_loggingEnabled) return;

        var timestamp = Time.time.ToString("F2");
        var logEntry = $"[{timestamp}] {signalInfo}";
        
        _signalLog.Add(logEntry);
        if (_signalLog.Count > 100)
            _signalLog.RemoveAt(0);

        var signalName = signalInfo.Split(' ')[0].Split('-')[0].Trim();
        if (!_signalFireCounts.ContainsKey(signalName))
            _signalFireCounts[signalName] = 0;
        _signalFireCounts[signalName]++;

        if (_verboseMode)
        {
            MelonLogger.Msg($"[SIGNAL] {logEntry}");
        }
    }

    // Disabled - using SignalInterceptor's InternalFire patch instead
    // [HarmonyPatch(typeof(SignalBus), "InternalFire")]
    // [HarmonyPrefix]
    // public static void OnSignalFire(Il2CppSystem.Type signalType, Il2CppSystem.Object signal, 
    //     Il2CppSystem.Object identifier, bool requireDeclaration)
    // {
    //     if (!_loggingEnabled) return;

    //     var signalTypeName = signalType?.Name ?? "Unknown";
    //     var identifierStr = identifier?.ToString() ?? "null";
    //     
    //     LogSignal($"[InternalFire] Type: {signalTypeName}, ID: {identifierStr}, Required: {requireDeclaration}");
    // }

    [HarmonyPatch(typeof(SignalBus), "TryFire", new Type[] { typeof(Il2CppSystem.Object) })]
    [HarmonyPrefix]
    public static void OnTryFireNonGeneric(Il2CppSystem.Object signal)
    {
        LogSignal($"[TryFire] Attempted to fire signal: {signal?.GetType().Name ?? "null"}");
    }

    private static float _lastDebugTime = 0f;
    
    public override void OnUpdate()
    {
        if (Time.time - _lastDebugTime > 5f)
        {
            _lastDebugTime = Time.time;
            if (_verboseMode)
            {
                MelonLogger.Msg($"[DEBUG] OnUpdate running, time: {Time.time:F1}");
            }
        }
        
        if (Input.GetKeyDown(KeyCode.F9))
        {
            _loggingEnabled = !_loggingEnabled;
            MelonLogger.Msg($"Signal logging: {(_loggingEnabled ? "ENABLED" : "DISABLED")}");
        }

        if (Input.GetKeyDown(KeyCode.F10))
        {
            DumpSignalStatistics();
        }

        if (Input.GetKeyDown(KeyCode.F11))
        {
            _verboseMode = !_verboseMode;
            SignalInterceptor.SetVerboseMode(_verboseMode);
            MelonLogger.Msg($"Verbose mode: {(_verboseMode ? "ON" : "OFF")}");
        }

        if (Input.GetKeyDown(KeyCode.F12))
        {
            TestFireSignals();
        }
        
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            MelonLogger.Msg("=== TAB KEY PRESSED - MOD IS WORKING ===");
            MelonLogger.Msg($"Current state - Logging: {_loggingEnabled}, Verbose: {_verboseMode}");
            MelonLogger.Msg($"SignalBus available: {_signalBus != null}");
            MelonLogger.Msg($"Signals logged: {_signalLog.Count}");
            MelonLogger.Msg($"Flow Tracing - XP: {_traceXpFlow}");
        }
        
        if (Input.GetKeyDown(KeyCode.Keypad1))
        {
            _traceXpFlow = !_traceXpFlow;
            MelonLogger.Msg($"XP Flow Tracing: {(_traceXpFlow ? "ON" : "OFF")}");
        }
        
        if (Input.GetKeyDown(KeyCode.Keypad2))
        {
            SignalInterceptor.EnableDamageInterception(!_interceptDamage);
            _interceptDamage = !_interceptDamage;
            MelonLogger.Msg($"Damage Signal Interception: {(_interceptDamage ? "ON" : "OFF")}");
        }
        
        if (Input.GetKeyDown(KeyCode.Keypad3))
        {
            SignalInterceptor.EnableWeaponInterception(!_interceptWeapons);
            _interceptWeapons = !_interceptWeapons;
            MelonLogger.Msg($"Weapon Signal Interception: {(_interceptWeapons ? "ON" : "OFF")}");
        }
        
        if (Input.GetKeyDown(KeyCode.Keypad0))
        {
            bool newState = !(_interceptDamage && _interceptWeapons);
            SignalInterceptor.EnableAllInterception(newState);
            _interceptDamage = newState;
            _interceptWeapons = newState;
            MelonLogger.Msg($"ALL Signal Interception: {(newState ? "ON" : "OFF")}");
        }
        
        if (Input.GetKeyDown(KeyCode.F8))
        {
            ExploreDeclaredSignals();
        }

        if (Input.GetKeyDown(KeyCode.F7))
        {
            TestCustomSignalDeclaration();
        }

        if (Input.GetKeyDown(KeyCode.F6))
        {
            TestAsyncVsSync();
        }
    }

    private static void DumpSignalStatistics()
    {
        MelonLogger.Msg("========== SIGNAL STATISTICS ==========");
        MelonLogger.Msg($"Total unique signals fired: {_signalFireCounts.Count}");
        MelonLogger.Msg($"SignalBus subscribers: {_signalBus?.NumSubscribers ?? 0}");
        MelonLogger.Msg($"Declared signal categories: {_declaredSignals.Count}");
        
        MelonLogger.Msg("\n--- Signal Fire Counts ---");
        foreach (var kvp in _signalFireCounts.OrderByDescending(x => x.Value))
        {
            MelonLogger.Msg($"  {kvp.Key}: {kvp.Value} times");
        }

        MelonLogger.Msg("\n--- Recent Signal Log (last 10) ---");
        var recentLogs = _signalLog.TakeLast(10);
        foreach (var log in recentLogs)
        {
            MelonLogger.Msg($"  {log}");
        }

        MelonLogger.Msg("=======================================");
    }

    private static void TestFireSignals()
    {
        if (_signalBus == null)
        {
            MelonLogger.Warning("SignalBus not available");
            return;
        }

        MelonLogger.Msg("=== Signal System Information ===");
        MelonLogger.Msg($"SignalBus Status: Active");
        MelonLogger.Msg($"Total Subscribers: {_signalBus.NumSubscribers}");
        MelonLogger.Msg($"Signals Tracked: {_signalFireCounts.Count}");
        MelonLogger.Msg($"Signal Log Entries: {_signalLog.Count}");
        
        if (_signalFireCounts.Count > 0)
        {
            MelonLogger.Msg("\nMost Frequent Signals:");
            foreach (var kvp in _signalFireCounts.OrderByDescending(x => x.Value).Take(5))
            {
                MelonLogger.Msg($"  - {kvp.Key}: {kvp.Value} times");
            }
        }
        
        MelonLogger.Msg("=================================");
    }

    private static void ExploreDeclaredSignals()
    {
        MelonLogger.Msg("========== DECLARED SIGNALS ==========");
        MelonLogger.Msg($"Signal categories declared: {_declaredSignals.Count}");
        foreach (var category in _declaredSignals)
        {
            MelonLogger.Msg($"  - {category}");
        }

        if (_signalBus != null)
        {
            MelonLogger.Msg($"\nSignalBus state:");
            MelonLogger.Msg($"  Total subscribers: {_signalBus.NumSubscribers}");
            
            MelonLogger.Msg($"\nKnown Signal Types (from tracking):");
            if (_signalFireCounts.Count > 0)
            {
                foreach (var signal in _signalFireCounts.Keys.OrderBy(x => x))
                {
                    MelonLogger.Msg($"  - {signal}");
                }
            }
            else
            {
                MelonLogger.Msg("  No signals captured yet");
            }
        }
        
        MelonLogger.Msg("=======================================");
    }

    private static void TestCustomSignalDeclaration()
    {
        MelonLogger.Msg("=== Signal Declaration Info ===");

        if (_container == null)
        {
            MelonLogger.Warning("DiContainer not available for declaration info");
            return;
        }

        MelonLogger.Msg("Signal Declaration System:");
        MelonLogger.Msg("  - Signals must be declared at container build time");
        MelonLogger.Msg("  - Runtime declaration is not supported");
        MelonLogger.Msg("  - TryFire works for undeclared signals");
        MelonLogger.Msg("  - Fire requires declaration");
        
        if (_declaredSignals.Count > 0)
        {
            MelonLogger.Msg($"\nDeclared Signal Categories: {_declaredSignals.Count}");
            foreach (var category in _declaredSignals)
            {
                MelonLogger.Msg($"  - {category}");
            }
        }
        
        MelonLogger.Msg("================================");
    }

    private static void TestAsyncVsSync()
    {
        MelonLogger.Msg("=== Signal System Analysis ===");

        if (_signalBus == null)
        {
            MelonLogger.Warning("SignalBus not available");
            return;
        }

        MelonLogger.Msg($"SignalBus Analysis:");
        MelonLogger.Msg($"  - Active Subscribers: {_signalBus.NumSubscribers}");
        MelonLogger.Msg($"  - Signals are processed synchronously");
        MelonLogger.Msg($"  - Signal order is preserved during execution");
        
        if (_signalFireCounts.Count > 0)
        {
            var totalSignals = _signalFireCounts.Values.Sum();
            var avgSignalsPerType = totalSignals / (float)_signalFireCounts.Count;
            MelonLogger.Msg($"\nSignal Statistics:");
            MelonLogger.Msg($"  - Total Signals Fired: {totalSignals}");
            MelonLogger.Msg($"  - Unique Signal Types: {_signalFireCounts.Count}");
            MelonLogger.Msg($"  - Average per Type: {avgSignalsPerType:F1}");
        }
        
        MelonLogger.Msg("===============================");
    }

    public override void OnApplicationQuit()
    {
        DumpSignalStatistics();
    }

    [HarmonyPatch(typeof(SignalBus), "SubscribeInternal")]
    [HarmonyPostfix]
    public static void OnSubscribeInternal(Il2CppSystem.Type signalType)
    {
        if (_verboseMode)
        {
            MelonLogger.Msg($"[SUBSCRIBE] New subscription to: {signalType?.Name ?? "Unknown"}");
        }
    }

    [HarmonyPatch(typeof(SignalBus), "UnsubscribeInternal")]
    [HarmonyPostfix]
    public static void OnUnsubscribeInternal(Il2CppSystem.Type signalType)
    {
        if (_verboseMode)
        {
            MelonLogger.Msg($"[UNSUBSCRIBE] Removed subscription from: {signalType?.Name ?? "Unknown"}");
        }
    }
    
    [HarmonyPatch(typeof(MainGamePage), "UpdateExperienceProgress")]
    [HarmonyPrefix]
    public static void TraceUIXpUpdate(GameplaySignals.CharacterXpChangedSignal sig)
    {
        if (_traceXpFlow)
        {
            MelonLogger.Msg("[3] UI Received Signal -> MainGamePage.UpdateExperienceProgress");
            MelonLogger.Msg($"    Normalized XP: {sig.CurrentXp / sig.MaxXp:F3}");
        }
    }

    [HarmonyPatch(typeof(CharacterController), "AddExperience")]
    [HarmonyPostfix]
    public static void TraceXpSource(float experience)
    {
        if (_traceXpFlow)
        {
            MelonLogger.Msg($"[0] XP Source: CharacterController.AddExperience({experience})");
        }
    }
    
    [HarmonyPatch(typeof(SignalsInstaller), "DeclareUISignals")]
    [HarmonyPostfix]
    public static void TrackUISignals()
    {
        MelonLogger.Msg("[Declaration] UI Signals declared");
        _declaredSignals.Add("UISignals");
    }

    [HarmonyPatch(typeof(SignalsInstaller), "DeclareCharacterSignals")]
    [HarmonyPostfix]
    public static void TrackCharacterSignals()
    {
        MelonLogger.Msg("[Declaration] Character Signals declared");
        _declaredSignals.Add("CharacterSignals");
    }

    [HarmonyPatch(typeof(SignalsInstaller), "DeclareOptionsSignals")]
    [HarmonyPostfix]
    public static void TrackOptionsSignals()
    {
        MelonLogger.Msg("[Declaration] Options Signals declared");
        _declaredSignals.Add("OptionsSignals");
    }

    [HarmonyPatch(typeof(SignalsInstaller), "DeclareLevelUpFactorySignals")]
    [HarmonyPostfix]
    public static void TrackLevelUpSignals()
    {
        MelonLogger.Msg("[Declaration] LevelUp Factory Signals declared");
        _declaredSignals.Add("LevelUpFactorySignals");
    }

    [HarmonyPatch(typeof(SignalsInstaller), "DeclareAutomationSignals")]
    [HarmonyPostfix]
    public static void TrackAutomationSignals()
    {
        MelonLogger.Msg("[Declaration] Automation Signals declared");
        _declaredSignals.Add("AutomationSignals");
    }
    
    public class TestCustomSignal
    {
        public string Message { get; set; }
    }
}