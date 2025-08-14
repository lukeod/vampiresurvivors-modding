using MelonLoader;
using HarmonyLib;
using Il2CppVampireSurvivors.Framework;
using Il2CppVampireSurvivors.Data;
using Il2CppNewtonsoft.Json.Linq;
using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

[assembly: MelonInfo(typeof(DLCInspector), "DLC Inspector", "1.0.0", "VS Modding Community")]
[assembly: MelonGame("poncle", "Vampire Survivors")]

/// <summary>
/// DLCInspector - Tool for examining DLC data in Vampire Survivors.
/// 
/// Features:
/// - Lists all DLC types and their content
/// - Exports DLC JSON data
/// - Shows DLC-specific weapons, characters, stages
/// - Tracks DLC loading during ReloadAllData
/// 
/// Usage:
/// - F2: Dump DLC summary
/// - F3: Export DLC data to JSON files
/// - F4: Show DLC loading sequence
/// </summary>
public class DLCInspector : MelonMod
{
    private static int reloadCount = 0;
    private static System.Collections.Generic.List<string> loadingSequence = new();
    
    public override void OnInitializeMelon()
    {
        MelonLogger.Msg("=== DLC Inspector Initialized ===");
        MelonLogger.Msg("F2: DLC Summary | F3: Export DLC JSON | F4: Loading Sequence");
        
        InstallHooks();
    }
    
    private void InstallHooks()
    {
        try
        {
            var harmony = new HarmonyLib.Harmony("DLCInspector");
            
            // Hook ReloadAllData to track DLC loading
            var reloadMethod = typeof(DataManager).GetMethod("ReloadAllData");
            if (reloadMethod != null)
            {
                harmony.Patch(reloadMethod, 
                    prefix: new HarmonyMethod(typeof(DLCInspector).GetMethod(nameof(OnReloadAllDataPrefix))),
                    postfix: new HarmonyMethod(typeof(DLCInspector).GetMethod(nameof(OnReloadAllDataPostfix))));
            }
            
            // Hook MergeInJsonData to track DLC merging
            var mergeMethod = typeof(DataManager).GetMethod("MergeInJsonData");
            if (mergeMethod != null)
            {
                harmony.Patch(mergeMethod,
                    postfix: new HarmonyMethod(typeof(DLCInspector).GetMethod(nameof(OnMergeInJsonData))));
            }
        }
        catch (Exception ex)
        {
            MelonLogger.Error($"Failed to install hooks: {ex.Message}");
        }
    }
    
    public override void OnUpdate()
    {
        if (Input.GetKeyDown(KeyCode.F2))
        {
            DumpDLCSummary();
        }
        
        if (Input.GetKeyDown(KeyCode.F3))
        {
            ExportDLCData();
        }
        
        if (Input.GetKeyDown(KeyCode.F4))
        {
            ShowLoadingSequence();
        }
    }
    
    private void DumpDLCSummary()
    {
        MelonLogger.Msg("=== DLC Summary ===");
        
        var dataManager = GM.Core?.DataManager;
        if (dataManager == null)
        {
            MelonLogger.Msg("DataManager not available (not in game)");
            return;
        }
        
        // List all DLC types from the enum
        MelonLogger.Msg("\nKnown DLC Types:");
        foreach (DlcType dlc in Enum.GetValues(typeof(DlcType)))
        {
            MelonLogger.Msg($"  - {dlc}");
        }
        
        // Check DLC weapon data
        if (dataManager._dlcWeaponData != null && dataManager._dlcWeaponData.Count > 0)
        {
            MelonLogger.Msg($"\nDLC Weapon Data ({dataManager._dlcWeaponData.Count} DLCs):");
            foreach (var dlcEntry in dataManager._dlcWeaponData)
            {
                var dlcType = dlcEntry.Key;
                var weaponDict = dlcEntry.Value;
                MelonLogger.Msg($"  {dlcType}: {weaponDict?.Count ?? 0} weapons");
                
                // Show first few weapons as examples
                if (weaponDict != null && weaponDict.Count > 0)
                {
                    int count = 0;
                    var first5 = new List<WeaponType>();
                    foreach (var key in weaponDict.Keys)
                    {
                        first5.Add(key);
                        if (++count >= 5) break;
                    }
                    foreach (var weapon in first5)
                    {
                        MelonLogger.Msg($"    - {weapon}");
                    }
                    if (weaponDict.Count > 5)
                    {
                        MelonLogger.Msg($"    ... and {weaponDict.Count - 5} more");
                    }
                }
            }
        }
        
        // Check DLC character data
        if (dataManager._dlcCharacterData != null && dataManager._dlcCharacterData.Count > 0)
        {
            MelonLogger.Msg($"\nDLC Character Data ({dataManager._dlcCharacterData.Count} DLCs):");
            foreach (var dlcEntry in dataManager._dlcCharacterData)
            {
                var dlcType = dlcEntry.Key;
                var charDict = dlcEntry.Value;
                MelonLogger.Msg($"  {dlcType}: {charDict?.Count ?? 0} characters");
                
                // Show character names
                if (charDict != null && charDict.Count > 0)
                {
                    int count = 0;
                    var first5 = new List<CharacterType>();
                    foreach (var key in charDict.Keys)
                    {
                        first5.Add(key);
                        if (++count >= 5) break;
                    }
                    foreach (var character in first5)
                    {
                        MelonLogger.Msg($"    - {character}");
                    }
                    if (charDict.Count > 5)
                    {
                        MelonLogger.Msg($"    ... and {charDict.Count - 5} more");
                    }
                }
            }
        }
        
        // Check DLC stage data
        if (dataManager._dlcStageData != null && dataManager._dlcStageData.Count > 0)
        {
            MelonLogger.Msg($"\nDLC Stage Data ({dataManager._dlcStageData.Count} DLCs):");
            foreach (var dlcEntry in dataManager._dlcStageData)
            {
                var dlcType = dlcEntry.Key;
                var stageDict = dlcEntry.Value;
                MelonLogger.Msg($"  {dlcType}: {stageDict?.Count ?? 0} stages");
                
                if (stageDict != null && stageDict.Count > 0)
                {
                    foreach (var stage in stageDict.Keys)
                    {
                        MelonLogger.Msg($"    - {stage}");
                    }
                }
            }
        }
        
        // Check DLC powerup data
        if (dataManager._dlcPowerUpData != null && dataManager._dlcPowerUpData.Count > 0)
        {
            MelonLogger.Msg($"\nDLC PowerUp Data ({dataManager._dlcPowerUpData.Count} DLCs):");
            foreach (var dlcEntry in dataManager._dlcPowerUpData)
            {
                var dlcType = dlcEntry.Key;
                var powerDict = dlcEntry.Value;
                MelonLogger.Msg($"  {dlcType}: {powerDict?.Count ?? 0} powerups");
            }
        }
        
        MelonLogger.Msg("=== End DLC Summary ===");
    }
    
    private void ExportDLCData()
    {
        MelonLogger.Msg("=== Exporting DLC Data ===");
        
        var dataManager = GM.Core?.DataManager;
        if (dataManager == null)
        {
            MelonLogger.Msg("DataManager not available");
            return;
        }
        
        // Get the UserData directory from MelonLoader's application path
        string gameDirectory = Path.GetDirectoryName(Application.dataPath);
        string exportPath = Path.Combine(gameDirectory, "UserData", "DLCExports");
        Directory.CreateDirectory(exportPath);
        
        // Export DLC-specific JSON data
        ExportDLCJsonData(dataManager, exportPath);
        
        // Export raw JSON data if available
        try
        {
            // Export main JSON data
            if (dataManager._allWeaponDataJson != null)
            {
                var weaponJsonPath = Path.Combine(exportPath, "all_weapons.json");
                File.WriteAllText(weaponJsonPath, dataManager._allWeaponDataJson.ToString());
                MelonLogger.Msg($"Exported weapon JSON to: {weaponJsonPath}");
            }
            else
            {
                MelonLogger.Msg("No weapon JSON data available");
            }
            
            if (dataManager._allCharactersJson != null)
            {
                var charJsonPath = Path.Combine(exportPath, "all_characters.json");
                File.WriteAllText(charJsonPath, dataManager._allCharactersJson.ToString());
                MelonLogger.Msg($"Exported character JSON to: {charJsonPath}");
            }
            else
            {
                MelonLogger.Msg("No character JSON data available");
            }
            
            if (dataManager._allStagesJson != null)
            {
                var stageJsonPath = Path.Combine(exportPath, "all_stages.json");
                File.WriteAllText(stageJsonPath, dataManager._allStagesJson.ToString());
                MelonLogger.Msg($"Exported stage JSON to: {stageJsonPath}");
            }
            else
            {
                MelonLogger.Msg("No stage JSON data available");
            }
            
            // Check for more JSON data fields
            if (dataManager._allPowerUpsJson != null)
            {
                var powerupJsonPath = Path.Combine(exportPath, "all_powerups.json");
                File.WriteAllText(powerupJsonPath, dataManager._allPowerUpsJson.ToString());
                MelonLogger.Msg($"Exported powerup JSON to: {powerupJsonPath}");
            }
            
            if (dataManager._allEnemiesJson != null)
            {
                var enemiesJsonPath = Path.Combine(exportPath, "all_enemies.json");
                File.WriteAllText(enemiesJsonPath, dataManager._allEnemiesJson.ToString());
                MelonLogger.Msg($"Exported enemies JSON to: {enemiesJsonPath}");
            }
            
            if (dataManager._allItemsJson != null)
            {
                var itemsJsonPath = Path.Combine(exportPath, "all_items.json");
                File.WriteAllText(itemsJsonPath, dataManager._allItemsJson.ToString());
                MelonLogger.Msg($"Exported items JSON to: {itemsJsonPath}");
            }
            
            if (dataManager._allPropsJson != null)
            {
                var propsJsonPath = Path.Combine(exportPath, "all_props.json");
                File.WriteAllText(propsJsonPath, dataManager._allPropsJson.ToString());
                MelonLogger.Msg($"Exported props JSON to: {propsJsonPath}");
            }
            
            if (dataManager._allArcanasJson != null)
            {
                var arcanasJsonPath = Path.Combine(exportPath, "all_arcanas.json");
                File.WriteAllText(arcanasJsonPath, dataManager._allArcanasJson.ToString());
                MelonLogger.Msg($"Exported arcanas JSON to: {arcanasJsonPath}");
            }
            
            if (dataManager._allAchievementsJson != null)
            {
                var achievementsJsonPath = Path.Combine(exportPath, "all_achievements.json");
                File.WriteAllText(achievementsJsonPath, dataManager._allAchievementsJson.ToString());
                MelonLogger.Msg($"Exported achievements JSON to: {achievementsJsonPath}");
            }
            
            if (dataManager._allSecretsJson != null)
            {
                var secretsJsonPath = Path.Combine(exportPath, "all_secrets.json");
                File.WriteAllText(secretsJsonPath, dataManager._allSecretsJson.ToString());
                MelonLogger.Msg($"Exported secrets JSON to: {secretsJsonPath}");
            }
            
            if (dataManager._allLimitBreakDataJson != null)
            {
                var limitBreakJsonPath = Path.Combine(exportPath, "all_limitbreak.json");
                File.WriteAllText(limitBreakJsonPath, dataManager._allLimitBreakDataJson.ToString());
                MelonLogger.Msg($"Exported limit break JSON to: {limitBreakJsonPath}");
            }
            
            if (dataManager._allMusicDataJson != null)
            {
                var musicJsonPath = Path.Combine(exportPath, "all_music.json");
                File.WriteAllText(musicJsonPath, dataManager._allMusicDataJson.ToString());
                MelonLogger.Msg($"Exported music JSON to: {musicJsonPath}");
            }
            
            if (dataManager._allHitVfxDataJson != null)
            {
                var vfxJsonPath = Path.Combine(exportPath, "all_hitvfx.json");
                File.WriteAllText(vfxJsonPath, dataManager._allHitVfxDataJson.ToString());
                MelonLogger.Msg($"Exported hit VFX JSON to: {vfxJsonPath}");
            }
            
            // Export DLC-specific data
            if (dataManager._dlcWeaponData != null)
            {
                foreach (var dlcEntry in dataManager._dlcWeaponData)
                {
                    var dlcType = dlcEntry.Key;
                    // Skip if no weapons for this DLC
                    if (dlcEntry.Value == null || dlcEntry.Value.Count == 0) continue;
                    
                    var dlcPath = Path.Combine(exportPath, $"dlc_{dlcType}_weapons.txt");
                    using (var writer = new StreamWriter(dlcPath))
                    {
                        writer.WriteLine($"DLC: {dlcType}");
                        writer.WriteLine($"Weapon Count: {dlcEntry.Value?.Count ?? 0}");
                        writer.WriteLine("Weapons:");
                        
                        if (dlcEntry.Value != null)
                        {
                            foreach (var weapon in dlcEntry.Value)
                            {
                                writer.WriteLine($"  {weapon.Key}:");
                                if (weapon.Value != null && weapon.Value.Count > 0)
                                {
                                    var firstWeaponData = weapon.Value[0];
                                    writer.WriteLine($"    Name: {firstWeaponData.name}");
                                    writer.WriteLine($"    Description: {firstWeaponData.description}");
                                    // Evolution field may not exist, skip it
                                }
                            }
                        }
                    }
                    MelonLogger.Msg($"Exported {dlcType} weapons to: {dlcPath}");
                }
            }
            
            MelonLogger.Msg($"\nAll exports saved to: {exportPath}");
        }
        catch (Exception ex)
        {
            MelonLogger.Error($"Export failed: {ex.Message}");
        }
    }
    
    private void ShowLoadingSequence()
    {
        MelonLogger.Msg("=== DLC Loading Sequence ===");
        if (loadingSequence.Count == 0)
        {
            MelonLogger.Msg("No loading sequence recorded yet. Start a game to capture.");
        }
        else
        {
            foreach (var entry in loadingSequence)
            {
                MelonLogger.Msg(entry);
            }
        }
        MelonLogger.Msg("=== End Loading Sequence ===");
    }
    
    public static void OnReloadAllDataPrefix(DataManager __instance)
    {
        reloadCount++;
        loadingSequence.Add($"[{reloadCount}] ReloadAllData START");
    }
    
    public static void OnReloadAllDataPostfix(DataManager __instance)
    {
        loadingSequence.Add($"[{reloadCount}] ReloadAllData END");
        
        // Check what DLC data is present after reload
        if (__instance._dlcWeaponData != null && __instance._dlcWeaponData.Count > 0)
        {
            loadingSequence.Add($"  - DLC Weapon Data: {__instance._dlcWeaponData.Count} DLCs loaded");
            foreach (var dlc in __instance._dlcWeaponData.Keys)
            {
                loadingSequence.Add($"    - {dlc}");
            }
        }
    }
    
    public static void OnMergeInJsonData(DataManager __instance, DlcType dlcType)
    {
        loadingSequence.Add($"  MergeInJsonData called for: {dlcType}");
        MelonLogger.Msg($"Merging DLC data: {dlcType}");
    }
    
    private void ExportDLCJsonData(DataManager dataManager, string exportPath)
    {
        MelonLogger.Msg("\nChecking for DLC-specific data...");
        
        // The DataManager has parsed DLC data dictionaries but not raw JSON
        // We can export the DLC data as serialized JSON from the parsed objects
        
        try
        {
            // Export DLC weapon data as JSON
            if (dataManager._dlcWeaponData != null && dataManager._dlcWeaponData.Count > 0)
            {
                MelonLogger.Msg($"\nExporting DLC weapon data ({dataManager._dlcWeaponData.Count} DLCs)...");
                foreach (var dlcEntry in dataManager._dlcWeaponData)
                {
                    var dlcType = dlcEntry.Key;
                    var weaponDict = dlcEntry.Value;
                    if (weaponDict != null && weaponDict.Count > 0)
                    {
                        var fileName = $"dlc_{dlcType}_weapons_parsed.json";
                        var filePath = Path.Combine(exportPath, fileName);
                        
                        // Create a JSON representation of the weapon data
                        var jsonObj = new JObject();
                        foreach (var weaponEntry in weaponDict)
                        {
                            var weaponArray = new JArray();
                            foreach (var weaponData in weaponEntry.Value)
                            {
                                var weaponObj = new JObject
                                {
                                    ["name"] = weaponData.name,
                                    ["description"] = weaponData.description,
                                    ["level"] = weaponData.level,
                                    ["rarity"] = weaponData.rarity
                                };
                                weaponArray.Add(weaponObj);
                            }
                            jsonObj[weaponEntry.Key.ToString()] = weaponArray;
                        }
                        
                        File.WriteAllText(filePath, jsonObj.ToString(Il2CppNewtonsoft.Json.Formatting.Indented));
                        MelonLogger.Msg($"  Exported {dlcType} weapons to {fileName}");
                    }
                }
            }
            
            // Export DLC character data as JSON
            if (dataManager._dlcCharacterData != null && dataManager._dlcCharacterData.Count > 0)
            {
                MelonLogger.Msg($"\nExporting DLC character data ({dataManager._dlcCharacterData.Count} DLCs)...");
                foreach (var dlcEntry in dataManager._dlcCharacterData)
                {
                    var dlcType = dlcEntry.Key;
                    var charDict = dlcEntry.Value;
                    if (charDict != null && charDict.Count > 0)
                    {
                        var fileName = $"dlc_{dlcType}_characters_parsed.json";
                        var filePath = Path.Combine(exportPath, fileName);
                        
                        var jsonObj = new JObject();
                        foreach (var charEntry in charDict)
                        {
                            var charArray = new JArray();
                            foreach (var charData in charEntry.Value)
                            {
                                var charObj = new JObject
                                {
                                    ["charName"] = charData.charName,
                                    ["description"] = charData.description,
                                    ["level"] = charData.level,
                                    ["startingWeapon"] = charData.startingWeapon.ToString()
                                };
                                charArray.Add(charObj);
                            }
                            jsonObj[charEntry.Key.ToString()] = charArray;
                        }
                        
                        File.WriteAllText(filePath, jsonObj.ToString(Il2CppNewtonsoft.Json.Formatting.Indented));
                        MelonLogger.Msg($"  Exported {dlcType} characters to {fileName}");
                    }
                }
            }
            
            // Export DLC stage data as JSON
            if (dataManager._dlcStageData != null && dataManager._dlcStageData.Count > 0)
            {
                MelonLogger.Msg($"\nExporting DLC stage data ({dataManager._dlcStageData.Count} DLCs)...");
                foreach (var dlcEntry in dataManager._dlcStageData)
                {
                    var dlcType = dlcEntry.Key;
                    var stageDict = dlcEntry.Value;
                    if (stageDict != null && stageDict.Count > 0)
                    {
                        var fileName = $"dlc_{dlcType}_stages_parsed.json";
                        var filePath = Path.Combine(exportPath, fileName);
                        
                        var jsonObj = new JObject();
                        foreach (var stageEntry in stageDict)
                        {
                            var stageArray = new JArray();
                            foreach (var stageData in stageEntry.Value)
                            {
                                var stageObj = new JObject
                                {
                                    ["stageName"] = stageData.stageName,
                                    ["description"] = stageData.description,
                                    ["stageNumber"] = stageData.stageNumber
                                };
                                stageArray.Add(stageObj);
                            }
                            jsonObj[stageEntry.Key.ToString()] = stageArray;
                        }
                        
                        File.WriteAllText(filePath, jsonObj.ToString(Il2CppNewtonsoft.Json.Formatting.Indented));
                        MelonLogger.Msg($"  Exported {dlcType} stages to {fileName}");
                    }
                }
            }
        }
        catch (Exception ex)
        {
            MelonLogger.Error($"Failed to export DLC data: {ex.Message}");
        }
    }
}

// Extension to help with DLC names
public static class DLCNames
{
    public static string GetFriendlyName(DlcType dlc)
    {
        return dlc switch
        {
            DlcType.Moonspell => "Legacy of the Moonspell",
            DlcType.Foscari => "Tides of the Foscari", 
            DlcType.Chalcedony => "Chalcedony Bundle",
            DlcType.FirstBlood => "First Blood",
            DlcType.Emeralds => "Emerald Serenade",
            DlcType.ThosePeople => "Those People",
            _ => dlc.ToString()
        };
    }
}