using MelonLoader;
using HarmonyLib;
using Il2CppVampireSurvivors.Framework;
using Il2CppVampireSurvivors.Data;
using Il2CppNewtonsoft.Json.Linq;
using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Il2CppInterop.Runtime.InteropTypes.Arrays;

[assembly: MelonInfo(typeof(AssetDumper), "Asset Dumper", "1.0.0", "VS Modding Community")]
[assembly: MelonGame("poncle", "Vampire Survivors")]

/// <summary>
/// AssetDumper - Comprehensive tool for extracting game assets and data
/// 
/// Features:
/// - Export all sprite names and metadata
/// - Extract game text and strings
/// - Extract achievement data
/// - List all audio/music references
/// - Export collection unlockables
/// - Dump stage tileset information
/// 
/// Controls:
/// - F5: Export sprite/texture metadata
/// - F6: Dump game text/strings data
/// - F7: Export collections and unlockables
/// - F8: Dump audio/music data
/// - F9: Export all data (comprehensive dump)
/// </summary>
public class AssetDumper : MelonMod
{
    private string exportBasePath;
    
    public override void OnInitializeMelon()
    {
        MelonLogger.Msg("=====================================");
        MelonLogger.Msg("   ASSET DUMPER v1.0.0 LOADED");
        MelonLogger.Msg("=====================================");
        MelonLogger.Msg("");
        MelonLogger.Msg("HOTKEYS (use in-game, not in menus):");
        MelonLogger.Msg("  F5 - Export Sprites & Textures");
        MelonLogger.Msg("       (metadata, dimensions, formats)");
        MelonLogger.Msg("  F6 - Export Game Text/Strings");
        MelonLogger.Msg("       (weapon, character, stage names/descriptions)");
        MelonLogger.Msg("  F7 - Export Collections");
        MelonLogger.Msg("       (achievements, unlocks, evolutions)");
        MelonLogger.Msg("  F8 - Export Audio Data");
        MelonLogger.Msg("       (music, sound clips, metadata)");
        MelonLogger.Msg("  F9 - EXPORT EVERYTHING");
        MelonLogger.Msg("       (comprehensive dump of all data)");
        MelonLogger.Msg("  F1 - Show this help again");
        MelonLogger.Msg("");
        MelonLogger.Msg("Output Location:");
        
        // Set up export path
        string gameDirectory = Path.GetDirectoryName(Application.dataPath);
        exportBasePath = Path.Combine(gameDirectory, "UserData", "AssetDumps");
        MelonLogger.Msg($"  {exportBasePath}");
        MelonLogger.Msg("");
        MelonLogger.Msg("NOTE: Must be in an active game session!");
        MelonLogger.Msg("=====================================");
    }
    
    public override void OnUpdate()
    {
        if (Input.GetKeyDown(KeyCode.F1))
        {
            ShowHelp();
        }
        
        if (Input.GetKeyDown(KeyCode.F5))
        {
            DumpSpriteData();
        }
        
        if (Input.GetKeyDown(KeyCode.F6))
        {
            DumpLocalizationData();
        }
        
        if (Input.GetKeyDown(KeyCode.F7))
        {
            DumpCollectionsData();
        }
        
        if (Input.GetKeyDown(KeyCode.F8))
        {
            DumpAudioData();
        }
        
        if (Input.GetKeyDown(KeyCode.F9))
        {
            DumpAllData();
        }
    }
    
    private void ShowHelp()
    {
        MelonLogger.Msg("=== ASSET DUMPER HELP ===");
        MelonLogger.Msg("F5: Export Sprites & Textures");
        MelonLogger.Msg("F6: Export Game Text/Strings");
        MelonLogger.Msg("F7: Export Collections");
        MelonLogger.Msg("F8: Export Audio Data");
        MelonLogger.Msg("F9: Export Everything");
        MelonLogger.Msg("F1: Show this help");
        MelonLogger.Msg($"Output: {exportBasePath}");
        MelonLogger.Msg("=========================");
    }
    
    private void DumpSpriteData()
    {
        MelonLogger.Msg("=== Dumping Sprite Data ===");
        
        try
        {
            var exportPath = Path.Combine(exportBasePath, DateTime.Now.ToString("yyyyMMdd_HHmmss"), "sprites");
            Directory.CreateDirectory(exportPath);
            
            // Get all loaded textures
            var allTextures = Resources.FindObjectsOfTypeAll<Texture2D>();
            if (allTextures.Length > 0)
            {
                MelonLogger.Msg($"Found {allTextures.Length} loaded textures");
                
                var spritesFile = Path.Combine(exportPath, "sprite_textures.json");
                var spriteData = new JObject();
                
                var textureList = new JArray();
                foreach (var texture in allTextures)
                {
                    if (texture != null && !string.IsNullOrEmpty(texture.name))
                    {
                        var texObj = new JObject
                        {
                            ["name"] = texture.name,
                            ["width"] = texture.width,
                            ["height"] = texture.height,
                            ["format"] = texture.format.ToString(),
                            ["mipmapCount"] = texture.mipmapCount,
                            ["isReadable"] = texture.isReadable
                        };
                        textureList.Add(texObj);
                    }
                }
                
                spriteData["textures"] = textureList;
                spriteData["count"] = allTextures.Length;
                
                File.WriteAllText(spritesFile, spriteData.ToString(Il2CppNewtonsoft.Json.Formatting.Indented));
                MelonLogger.Msg($"Exported texture data to: {spritesFile}");
            }
            
            // Dump sprite metadata
            var allSprites = Resources.FindObjectsOfTypeAll<Sprite>();
            if (allSprites.Length > 0)
            {
                MelonLogger.Msg($"Found {allSprites.Length} sprites");
                
                var spriteMetaFile = Path.Combine(exportPath, "sprite_metadata.json");
                var spriteArray = new JArray();
                
                foreach (var sprite in allSprites)
                {
                    if (sprite != null && !string.IsNullOrEmpty(sprite.name))
                    {
                        var spriteObj = new JObject
                        {
                            ["name"] = sprite.name,
                            ["rect"] = $"{sprite.rect.x},{sprite.rect.y},{sprite.rect.width},{sprite.rect.height}",
                            ["pivot"] = $"{sprite.pivot.x},{sprite.pivot.y}",
                            ["pixelsPerUnit"] = sprite.pixelsPerUnit,
                            ["texture"] = sprite.texture?.name ?? "null"
                        };
                        spriteArray.Add(spriteObj);
                    }
                }
                
                File.WriteAllText(spriteMetaFile, spriteArray.ToString(Il2CppNewtonsoft.Json.Formatting.Indented));
                MelonLogger.Msg($"Exported {allSprites.Length} sprite metadata to: {spriteMetaFile}");
            }
            
            MelonLogger.Msg("=== Sprite Data Export Complete ===");
        }
        catch (Exception ex)
        {
            MelonLogger.Error($"Failed to dump sprite data: {ex.Message}");
        }
    }
    
    private void DumpLocalizationData()
    {
        MelonLogger.Msg("=== Dumping Game Text/Strings Data ===");
        
        try
        {
            var exportPath = Path.Combine(exportBasePath, DateTime.Now.ToString("yyyyMMdd_HHmmss"), "text_data");
            Directory.CreateDirectory(exportPath);
            
            var dataManager = GM.Core?.DataManager;
            if (dataManager == null)
            {
                MelonLogger.Warning("DataManager not available");
                return;
            }
            
            // Just dump all the raw JSON data that contains text/localization
            if (dataManager != null)
            {
                // Export all raw JSON data files that contain text
                if (dataManager._allWeaponDataJson != null)
                {
                    var weaponsFile = Path.Combine(exportPath, "weapons_raw.json");
                    File.WriteAllText(weaponsFile, dataManager._allWeaponDataJson.ToString());
                    MelonLogger.Msg($"Exported raw weapons JSON to: {weaponsFile}");
                }
                
                if (dataManager._allCharactersJson != null)
                {
                    var charactersFile = Path.Combine(exportPath, "characters_raw.json");
                    File.WriteAllText(charactersFile, dataManager._allCharactersJson.ToString());
                    MelonLogger.Msg($"Exported raw characters JSON to: {charactersFile}");
                }
                
                if (dataManager._allStagesJson != null)
                {
                    var stagesFile = Path.Combine(exportPath, "stages_raw.json");
                    File.WriteAllText(stagesFile, dataManager._allStagesJson.ToString());
                    MelonLogger.Msg($"Exported raw stages JSON to: {stagesFile}");
                }
                
                if (dataManager._allAchievementsJson != null)
                {
                    var achievementsFile = Path.Combine(exportPath, "achievements_raw.json");
                    File.WriteAllText(achievementsFile, dataManager._allAchievementsJson.ToString());
                    MelonLogger.Msg($"Exported raw achievements JSON to: {achievementsFile}");
                }
                
                if (dataManager._allSecretsJson != null)
                {
                    var secretsFile = Path.Combine(exportPath, "secrets_raw.json");
                    File.WriteAllText(secretsFile, dataManager._allSecretsJson.ToString());
                    MelonLogger.Msg($"Exported raw secrets JSON to: {secretsFile}");
                }
                
                // Try to find UI text elements currently visible
                var allUITexts = UnityEngine.Object.FindObjectsOfType<Il2CppTMPro.TextMeshProUGUI>();
                if (allUITexts != null && allUITexts.Length > 0)
                {
                    MelonLogger.Msg($"Found {allUITexts.Length} UI text elements");
                    
                    var uiTextFile = Path.Combine(exportPath, "ui_texts_current.json");
                    var uniqueTexts = new HashSet<string>();
                    
                    foreach (var textComponent in allUITexts)
                    {
                        if (textComponent != null && !string.IsNullOrEmpty(textComponent.text))
                        {
                            uniqueTexts.Add(textComponent.text);
                        }
                    }
                    
                    var textArray = new JArray();
                    foreach (var text in uniqueTexts.OrderBy(t => t))
                    {
                        textArray.Add(text);
                    }
                    
                    File.WriteAllText(uiTextFile, textArray.ToString(Il2CppNewtonsoft.Json.Formatting.Indented));
                    MelonLogger.Msg($"Exported {uniqueTexts.Count} unique UI texts to: {uiTextFile}");
                }
            }
            
            MelonLogger.Msg("=== Text/Strings Data Export Complete ===");
        }
        catch (Exception ex)
        {
            MelonLogger.Error($"Failed to dump localization data: {ex.Message}");
        }
    }
    
    private void DumpCollectionsData()
    {
        MelonLogger.Msg("=== Dumping Collections Data ===");
        
        try
        {
            var exportPath = Path.Combine(exportBasePath, DateTime.Now.ToString("yyyyMMdd_HHmmss"), "collections");
            Directory.CreateDirectory(exportPath);
            
            var dataManager = GM.Core?.DataManager;
            if (dataManager == null)
            {
                MelonLogger.Warning("DataManager not available");
                return;
            }
            
            // Export achievements
            if (dataManager._allAchievementsJson != null)
            {
                var achievementsFile = Path.Combine(exportPath, "achievements.json");
                File.WriteAllText(achievementsFile, dataManager._allAchievementsJson.ToString());
                MelonLogger.Msg($"Exported achievements to: {achievementsFile}");
            }
            
            // Export secrets/unlockables
            if (dataManager._allSecretsJson != null)
            {
                var secretsFile = Path.Combine(exportPath, "secrets.json");
                File.WriteAllText(secretsFile, dataManager._allSecretsJson.ToString());
                MelonLogger.Msg($"Exported secrets to: {secretsFile}");
            }
            
            // Export character unlock conditions
            var characters = dataManager.GetConvertedCharacterData();
            if (characters != null && characters.Count > 0)
            {
                var unlockFile = Path.Combine(exportPath, "character_unlocks.json");
                var unlockData = new JObject();
                
                foreach (var charKvp in characters)
                {
                    var charType = charKvp.Key;
                    var charDataList = charKvp.Value;
                    
                    if (charDataList != null && charDataList.Count > 0)
                    {
                        var charData = charDataList[0];
                        var charObj = new JObject
                        {
                            ["name"] = charData.charName,
                            ["description"] = charData.description,
                            ["price"] = charData.price,
                            ["hidden"] = charData.hidden,
                            ["exLevels"] = charData.exLevels,
                            ["hidden"] = charData.hidden
                        };
                        
                        unlockData[charType.ToString()] = charObj;
                    }
                }
                
                File.WriteAllText(unlockFile, unlockData.ToString(Il2CppNewtonsoft.Json.Formatting.Indented));
                MelonLogger.Msg($"Exported character unlock data to: {unlockFile}");
            }
            
            // Export weapon evolution chains
            var weapons = dataManager.GetConvertedWeapons();
            if (weapons != null && weapons.Count > 0)
            {
                var evolutionFile = Path.Combine(exportPath, "weapon_evolutions.json");
                var evolutionData = new JObject();
                
                foreach (var weaponKvp in weapons)
                {
                    var weaponType = weaponKvp.Key;
                    var weaponDataList = weaponKvp.Value;
                    
                    if (weaponDataList != null && weaponDataList.Count > 0)
                    {
                        // Store basic weapon info (evolution data may not be directly accessible)
                        var weaponObj = new JObject
                        {
                            ["baseWeapon"] = weaponType.ToString(),
                            ["maxLevel"] = weaponDataList.Count,
                            ["name"] = weaponDataList[0].name
                        };
                        
                        evolutionData[weaponType.ToString()] = weaponObj;
                    }
                }
                
                File.WriteAllText(evolutionFile, evolutionData.ToString(Il2CppNewtonsoft.Json.Formatting.Indented));
                MelonLogger.Msg($"Exported weapon evolution data to: {evolutionFile}");
            }
            
            MelonLogger.Msg("=== Collections Data Export Complete ===");
        }
        catch (Exception ex)
        {
            MelonLogger.Error($"Failed to dump collections data: {ex.Message}");
        }
    }
    
    private void DumpAudioData()
    {
        MelonLogger.Msg("=== Dumping Audio Data ===");
        
        try
        {
            var exportPath = Path.Combine(exportBasePath, DateTime.Now.ToString("yyyyMMdd_HHmmss"), "audio");
            Directory.CreateDirectory(exportPath);
            
            var dataManager = GM.Core?.DataManager;
            if (dataManager == null)
            {
                MelonLogger.Warning("DataManager not available");
                return;
            }
            
            // Export music data
            if (dataManager._allMusicDataJson != null)
            {
                var musicFile = Path.Combine(exportPath, "music_data.json");
                File.WriteAllText(musicFile, dataManager._allMusicDataJson.ToString());
                MelonLogger.Msg($"Exported music data to: {musicFile}");
            }
            
            // Get all audio clips
            var audioClips = Resources.FindObjectsOfTypeAll<AudioClip>();
            if (audioClips.Length > 0)
            {
                MelonLogger.Msg($"Found {audioClips.Length} audio clips");
                
                var audioFile = Path.Combine(exportPath, "audio_clips.json");
                var audioArray = new JArray();
                
                foreach (var clip in audioClips)
                {
                    if (clip != null && !string.IsNullOrEmpty(clip.name))
                    {
                        var audioObj = new JObject
                        {
                            ["name"] = clip.name,
                            ["length"] = clip.length,
                            ["frequency"] = clip.frequency,
                            ["channels"] = clip.channels,
                            ["samples"] = clip.samples,
                            ["loadType"] = clip.loadType.ToString(),
                            ["loadState"] = clip.loadState.ToString()
                        };
                        audioArray.Add(audioObj);
                    }
                }
                
                File.WriteAllText(audioFile, audioArray.ToString(Il2CppNewtonsoft.Json.Formatting.Indented));
                MelonLogger.Msg($"Exported audio clip data to: {audioFile}");
            }
            
            // Try to get sound manager data through GM.Core
            if (GM.Core != null)
            {
                // SoundManager might be accessible through GM.Core or other means
                // For now, we'll skip the detailed sound manager extraction
                MelonLogger.Msg("Sound manager data extraction skipped (requires different access method)");
            }
            
            MelonLogger.Msg("=== Audio Data Export Complete ===");
        }
        catch (Exception ex)
        {
            MelonLogger.Error($"Failed to dump audio data: {ex.Message}");
        }
    }
    
    private void DumpAllData()
    {
        MelonLogger.Msg("=== Starting Comprehensive Data Dump ===");
        
        DumpSpriteData();
        DumpLocalizationData();
        DumpCollectionsData();
        DumpAudioData();
        
        // Additional comprehensive dumps
        try
        {
            var exportPath = Path.Combine(exportBasePath, DateTime.Now.ToString("yyyyMMdd_HHmmss"), "comprehensive");
            Directory.CreateDirectory(exportPath);
            
            var dataManager = GM.Core?.DataManager;
            if (dataManager == null)
            {
                MelonLogger.Warning("DataManager not available for comprehensive dump");
                return;
            }
            
            // Dump all enemy data
            if (dataManager._allEnemiesJson != null)
            {
                var enemiesFile = Path.Combine(exportPath, "all_enemies.json");
                File.WriteAllText(enemiesFile, dataManager._allEnemiesJson.ToString());
                MelonLogger.Msg($"Exported enemies to: {enemiesFile}");
            }
            
            // Dump all item data
            if (dataManager._allItemsJson != null)
            {
                var itemsFile = Path.Combine(exportPath, "all_items.json");
                File.WriteAllText(itemsFile, dataManager._allItemsJson.ToString());
                MelonLogger.Msg($"Exported items to: {itemsFile}");
            }
            
            // Dump all props data
            if (dataManager._allPropsJson != null)
            {
                var propsFile = Path.Combine(exportPath, "all_props.json");
                File.WriteAllText(propsFile, dataManager._allPropsJson.ToString());
                MelonLogger.Msg($"Exported props to: {propsFile}");
            }
            
            // Dump all arcanas
            if (dataManager._allArcanasJson != null)
            {
                var arcanasFile = Path.Combine(exportPath, "all_arcanas.json");
                File.WriteAllText(arcanasFile, dataManager._allArcanasJson.ToString());
                MelonLogger.Msg($"Exported arcanas to: {arcanasFile}");
            }
            
            // Dump VFX data
            if (dataManager._allHitVfxDataJson != null)
            {
                var vfxFile = Path.Combine(exportPath, "all_hit_vfx.json");
                File.WriteAllText(vfxFile, dataManager._allHitVfxDataJson.ToString());
                MelonLogger.Msg($"Exported VFX data to: {vfxFile}");
            }
            
            // Get GameObject hierarchy from active scene
            var activeScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
            var rootObjects = activeScene.GetRootGameObjects();
            if (rootObjects.Length > 0)
            {
                var hierarchyFile = Path.Combine(exportPath, "scene_hierarchy.json");
                var hierarchyData = new JArray();
                
                foreach (var obj in rootObjects)
                {
                    if (obj != null)
                    {
                        var objData = new JObject
                        {
                            ["name"] = obj.name,
                            ["active"] = obj.activeSelf,
                            ["tag"] = obj.tag,
                            ["layer"] = LayerMask.LayerToName(obj.layer),
                            ["childCount"] = obj.transform.childCount
                        };
                        hierarchyData.Add(objData);
                    }
                }
                
                File.WriteAllText(hierarchyFile, hierarchyData.ToString(Il2CppNewtonsoft.Json.Formatting.Indented));
                MelonLogger.Msg($"Exported scene hierarchy to: {hierarchyFile}");
            }
            
            MelonLogger.Msg($"=== Comprehensive Dump Complete ===");
            MelonLogger.Msg($"All data exported to: {exportBasePath}");
        }
        catch (Exception ex)
        {
            MelonLogger.Error($"Failed during comprehensive dump: {ex.Message}");
        }
    }
}