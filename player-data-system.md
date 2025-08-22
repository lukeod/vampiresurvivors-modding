# Player Data System

Comprehensive documentation of player data structures, save data, and runtime player state management in Vampire Survivors (Unity 6000.0.36f1).

## Overview

The player data system consists of three main components:
- **PlayerOptionsData** - Serializable save data structure containing persistent player progress
- **PlayerOptions** - Runtime manager class that handles player data operations
- **Save System** - Comprehensive save/load infrastructure with backup and cloud support

## PlayerOptionsData

**Location**: `Il2CppVampireSurvivors.Data.PlayerOptionsData`

The primary save data structure containing all persistent player information.

### Currency and Economy Fields

```csharp
public float Coins                    // Persistent gold balance (saved between sessions)
public float RunCoins                 // Gold earned in current run only
public float LifetimeCoins           // Total gold ever collected
public float TotalCoins               // Total coins tracking (including spent)
public int RunPickups_Coins          // Count of coin pickups in current run
public int LibraryMerchantGoldSpent  // Gold spent at library merchant
public int BeginnersLuck              // Beginner's luck counter
```

### Run Statistics

```csharp
public int RunEnemies                              // Enemies killed in current run
public int RunBossesCount                          // Bosses defeated in current run
public List<EnemyType> RunBossesTypes              // Types of bosses encountered
public List<ItemType> RunPickups                   // Items collected in current run
public List<WeaponType> RunWeapons                 // Weapons obtained in current run
public List<CharacterType> RunCoffins              // Coffins opened in current run
public Dictionary<EnemyType, int> RunKillCount     // Kill count per enemy type in run
public Dictionary<PropType, int> RunDestroyedProps // Props destroyed in run
public Dictionary<ItemType, int> RunItemsPickupCount // Item pickup counts in run
public int RunStarryHeavnes                        // Starry Heaven pickups
public int RunWeirdSoulsPurifier                   // Weird Souls Purifier count
public float RunFever                              // Fever time in current run
public int RunHunger                               // Hunger counter
public float RawRunHeal                            // Total healing in run
```

### Lifetime Statistics

```csharp
public float LifetimeSurvived                      // Total time survived across all runs
public float LifetimeHeal                          // Total healing across all runs
public float OwO                                   // Special achievement counter
public int CompletedHurries                        // Number of Hurry mode completions
public float TrainHazardEnemiesHit                 // Train hazard enemy hits
public float LongestFever                          // Longest fever duration
public float HighestFever                          // Highest fever multiplier
public Dictionary<EnemyType, int> KillCount        // Lifetime kill counts per enemy
public Dictionary<ItemType, int> PickupCount       // Lifetime pickup counts per item
public Dictionary<PropType, int> DestroyedCount    // Lifetime destroyed props count
public int TopLapsCarlo                            // Best lap count on Monte Carlo stage
public int TotalLapsCarlo                          // Total laps on Monte Carlo stage
public int TopLapsHighway                          // Best lap count on Highway stage
public int TotalLapsHighway                        // Total laps on Highway stage
```

### Game Mode Settings

```csharp
public CharacterType SelectedCharacter    // Currently selected character
public StageType SelectedStage           // Currently selected stage
public StageType NextAutoSelectStage     // Next auto-selected stage
public bool SelectedHyper                // Hyper mode enabled
public bool SelectedHurry                // Hurry mode enabled
public bool SelectedMazzo                // Endless mode enabled
public bool SelectedLimitBreak           // Limit Break mode enabled
public bool SelectedInverse              // Inverse mode enabled
public bool SelectedReapers              // Reapers enabled
public bool SelectedGoldenEggs           // Golden Eggs enabled
public bool SelectedSharePassives        // Share Passives enabled
public int SelectedArcana                // Selected Arcana card
public bool SelectedRandomEvents         // Random events enabled
public bool SelectedRandomLevels         // Random level-ups enabled
public int SelectedMaxWeapons           // Maximum weapon slots
public bool SelectedOnlineFreeRoam      // Online free roam mode
public bool SequentialChestMode         // Sequential chest mode
```

### Audio/Visual Settings

```csharp
// Audio Settings
public bool SoundsEnabled                 // Sound effects enabled
public bool MusicEnabled                  // Music enabled
public float SoundsVolume                 // Sound effects volume (0-1)
public float MusicVolume                  // Music volume (0-1)
public bool ClassicMusic                  // Use classic music
public BgmType SelectedBGM               // Selected background music
public BgmModType SelectedBGMMod         // Background music modification
public BgmPlaybackType SelectedBGMPlayback // Music playback type
public bool SelectedBGMSave              // Save BGM selection
public bool PlayBGMOnlyDuringRun         // Play music only during runs
public Dictionary<StageType, BgmType> MusicSelectionPerStage // Music per stage

// Visual Settings
public bool FlashingVFXEnabled           // Flashing visual effects enabled
public bool DamageNumbersEnabled         // Damage numbers display
public bool GlimmerCarouselEnabled       // Glimmer carousel enabled
public bool StreamSafeEnabled            // Stream-safe mode
public bool VisuallyInvertStages         // Visually invert stages
public bool HideProgress                 // Hide progress indicators
public bool hideXPBar                    // Hide experience bar
public bool ShowPickups                  // Show pickup indicators
public bool ShowSmallMapIcons            // Show small map icons
public bool ReducePhysics                // Reduce physics calculations
public bool DisableMovingBackground      // Disable moving backgrounds
public bool DisableBlood                 // Disable blood effects
public BorderType BorderType             // Screen border type
public bool PixelFont                    // Use pixel font
public bool DisplayDefangedEnemies       // Show defanged enemies
public bool StageLighting                // Stage lighting effects
public bool ScreenShakeEnabled           // Screen shake enabled
public bool ControllerVibrationEnabled   // Controller vibration
public bool Fullscreen                   // Fullscreen mode
```

### Collections and Unlocks

```csharp
// Characters and Skins
public List<CharacterType> BoughtCharacters         // Purchased characters
public List<CharacterType> UnlockedCharacters       // Unlocked characters
public List<CharacterType> HostOnlyUnlockedCharacters // Host-only unlocked characters
public List<CharacterType> OpenedCoffins            // Coffins opened
public List<SkinType> BoughtSkins                   // Purchased skins
public Dictionary<CharacterType, List<SkinType>> UnlockedSkins // Skins per character
public Dictionary<CharacterType, List<SkinType>> UnlockedSkinsV2 // Skins V2 per character
public Dictionary<CharacterType, int> SelectedSkins // Selected skins per character
public Dictionary<CharacterType, SkinType> SelectedSkinsV2 // Selected skins V2

// Weapons and Items
public List<WeaponType> CollectedWeapons            // Collected weapons
public List<WeaponType> UnlockedWeapons             // Unlocked weapons
public List<ItemType> CollectedItems                // Items collected
public List<ItemType> SealedItems                   // Sealed items
public List<WeaponType> SealedWeapons               // Sealed weapons
public List<ItemType> ContentGroupSealedItems       // Content group sealed items
public List<WeaponType> ContentGroupSealedWeapons   // Content group sealed weapons

// Stages and Modes
public List<StageType> UnlockedStages               // Unlocked stages
public List<StageType> UnlockedHypers               // Unlocked hyper stages
public List<ArcanaType> UnlockedArcanas             // Unlocked arcana cards

// Power-ups
public List<PowerUpLevel> BoughtPowerups            // Purchased power-up levels
public List<PowerUpType> UnlockedPowerUpRanks       // Unlocked power-up ranks
public List<PowerUpType> DisabledPowerups           // Disabled power-ups

// Achievements and Secrets
public List<AchievementType> Achievements           // Completed achievements
public List<SecretType> Secrets                     // Discovered secrets
public int itemInCollection                         // Items in collection count
public int itemInUnlocks                            // Items in unlocks count
public int itemInSecrets                            // Items in secrets count
```

### Golden Egg System

```csharp
public bool SelectedGoldenEggs                                           // Golden eggs enabled
public float TotalEggCount                                              // Total eggs purchased
public Dictionary<CharacterType, float> CharacterEggCount               // Eggs per character
public Dictionary<CharacterType, Dictionary<string, float>> CharacterEggInfo  // Detailed egg stats per character

// Egg stat categories (stored as string keys in CharacterEggInfo):
// "MaxHp" - Maximum Health
// "Armor" - Armor
// "Regen" - Health Regeneration
// "MoveSpeed" - Movement Speed
// "Power" - Might/Power
// "Cooldown" - Cooldown Reduction
// "Area" - Area of Effect
// "Speed" - Projectile Speed
// "Duration" - Duration
// "Amount" - Amount/Quantity
// "Luck" - Luck
// "Growth" - Growth
// "Greed" - Greed
// "Curse" - Curse
// "Magnet" - Magnet Range
// "Revivals" - Revival Count
// "Rerolls" - Reroll Count
// "Skips" - Skip Count
// "Banish" - Banish Count
```

### Stage Progression and Completion

```csharp
public Dictionary<CharacterType, List<StageType>> StageCompletionLog    // Stages completed per character
public Dictionary<CharacterType, List<CharacterStageData>> CharacterStageData  // Detailed stage data per character
public Dictionary<CharacterType, int> CharacterEnemiesKilled            // Enemies killed per character
public Dictionary<CharacterType, float> CharacterSurvivedMinutes        // Survival time per character
```

### Adventure Mode Data

```csharp
public Nullable<AdventureType> SelectedAdventureType                   // Selected adventure
public float AdventureStars                                            // Adventure currency
public int AdventureCompletionCount                                    // Adventures completed
public List<AdventureAchievementType> AdventureProgress                // Adventure achievements
public Dictionary<AdventureType, PlayerOptionsData> AdventuresSaveData // Nested save data per adventure
public List<AdventureType> CompletedAdventures                         // Completed adventures list
public float TotalAdventurePlaytime                                    // Total time in adventures
public float AllTimeAdventurePlaytime                                  // All-time adventure playtime
public bool HasSeenAdventuresIntroTutorial                            // Tutorial viewed flag
public bool HasSeenAdventureReveal                                    // Seen adventure mode reveal
public bool ShouldPlayAdventureReveal                                 // Should play adventure reveal
public bool HideUnavailableAdventures                                 // UI preference
public bool HasSeenMerchantTutorial                                   // Seen merchant tutorial
public List<AdventureType> SeenAscensionPopups                        // Seen ascension popups

// Adventure Types:
// ADV_LMS_001 - Legacy of Moonspell
// ADV_POES_001 - Tides of Foscari
// ADV_LIB_001 - Legacy of the Library
// ADV_IMEL_001 - Ode to Castlevania
// ADV_EMER_001 - Emergency Meeting
// ADV_FBLOOD_001 - Forbidden Blood
// ADV_SHEMOON_001 - She-Moon
// ADV_TIDES_001 - Tides of Foscari
```

### PowerUp and Ascension System

```csharp
public Dictionary<PowerUpType, int> AscensionPointsAllocation          // Ascension points spent
// Note: Ascension focuses on four main stats:
// - Luck (critical chance/item drops)
// - Growth (experience gain)
// - Greed (gold gain)
// - Curse (difficulty/enemy spawns)
```

### Character Performance Data

```csharp
public Dictionary<CharacterType, List<StageType>> StageCompletionLog   // Stage completions per character
public Dictionary<CharacterType, List<CharacterStageData>> CharacterStageData // Detailed stage data per character
public Dictionary<CharacterType, int> CharacterEnemiesKilled           // Enemies killed per character
public Dictionary<CharacterType, int> CharacterSurvivedMinutes         // Minutes survived per character
```

### Platform and Save Metadata

```csharp
public string saveDate                           // Last save date/time
public string Platform                           // Platform identifier
public Nullable<SystemPlatformTypes> SaveOriginalPlatform // Original platform where save was created
public List<SystemPlatformTypes> SaveTouchedPlatforms // All platforms that have touched this save
public int Version                               // Save data version
public string checksum                           // Save file checksum
public bool CheatCodeUsed                       // Whether cheat codes have been used
public bool SaveSyncPlatformAchievements        // Sync platform achievements
public bool AutoEnableCloudSavesMobile          // Auto-enable cloud saves on mobile
public bool AcceptedEULA                        // EULA acceptance status
```

### Special Progress Flags

```csharp
public bool HasKilledTheFinalBoss    // Defeated final boss
public bool HasSeenFinalFireworks    // Viewed ending fireworks
public bool HasUsedMirror            // Used mirror item
public bool HasUsedTrumpet           // Used trumpet item
public bool HasPlayedStage3          // Played stage 3
public bool HasSeenDarkanaTransition // Seen Darkana transition
public int PlayedRNJ                  // Played "Return To New Jerusalem"
public bool Didit                    // Achievement flag
public int Seals                     // Seal count
public bool HasFixedSkinIds          // Fixed skin IDs
public bool ShowTPCredits            // Show third-party credits
public bool PassedGaeaEvent          // Passed Gaea event
public int EME_NextBossBiome         // Emergency Meeting next boss biome
public int WW_ZoneProgress           // Whiteout zone progress
public int TP_FrozenShadesCount      // Third-party frozen shades count
public int TP_AxeArmorCount          // Third-party axe armor count
public int TP_SniperCount            // Third-party sniper count
public int TP_PortraitsCount         // Third-party portraits count
```

## PlayerOptions

**Location**: `Il2CppVampireSurvivors.Objects.PlayerOptions`

Runtime manager class that handles player data operations and provides access to PlayerOptionsData.

### Key Properties

```csharp
public PlayerOptionsData MainGameConfig { get; }         // Read-only main game configuration
public PlayerOptionsData Config { get; set; }            // Primary configuration data accessor
public PlayerOptionsData CurrentAdventureSaveData { get; set; } // Adventure mode save data
public bool IsConfigReady { get; }                      // Configuration initialization status
public bool IsInitialized { get; set; }                 // Overall initialization state
public bool IsInvertedWithVisuals { get; }              // Visual inversion state
public PlayerStats PlayerStats { get; }                 // Player statistics accessor

// Special item flags
public bool JustGotTrumpet { get; set; }                // Trumpet acquisition flag
public bool JustGotMirror { get; set; }                 // Mirror acquisition flag
public bool JustGotJubilee { get; set; }                // Jubilee acquisition flag
```

### Coin Management Methods

```csharp
// Add coins with Greed multiplier applied (affects both persistent and run coins)
public float AddCoins(float value, CharacterController player = null)

// Add coins without any multipliers
public void AddCoinsFlat(float value)
public static void AddCoinsFlat(float value, PlayerOptionsData config)

// Add coins to persistent total only (not to run coins)
public void AddCoinsNoRun(float value, CharacterController player = null)

// Remove coins (for purchases)
public void RemoveCoins(int value, bool removeFromLifetime = false)
public float RemoveCoinsFlat(float value)  // Returns actual amount removed

// Utility
public void FixCoinOverflow()  // Corrects coin overflow issues
```

### Save Operations

```csharp
// Save current data
public void Save(bool commitImmediately = true, bool createBackup = false)

// Clear save data
public void ClearSaveData(bool deleteAdventureData = false)

// Apply loaded configuration
public void ApplyLoadedOptions()
public void ApplyConfig(PlayerOptionsData config, bool adventureMode = false, 
                       bool hostConfig = false, bool onlineClientWithRunData = false)

// Utility
public void FixPlayerOptionsData()  // Repairs/validates player options data
public void ApplyUnlocksToData()    // Applies pending unlocks to data
public void ClearRunData()          // Clears current run data
```

### Unlock and Purchase Methods

```csharp
// Characters
public void UnlockCharacter(CharacterType characterType)
public void UnlockCharacter(CharacterType characterType, PlayerOptionsData config)
public void BuyCharacter(CharacterType characterType)
public void BuyCharacter(CharacterType characterType, PlayerOptionsData config)
public void RevealCharacter(CharacterType characterType, PlayerOptionsData config)

// Skins
public void UnlockSkin(CharacterType c, SkinType t, PlayerOptionsData config = null)
public void BuySkin(SkinType skinType)
public SkinType FixSkinMapping(CharacterType characterType, SkinType id)

// Other unlocks
public void UnlockArcana(ArcanaType arcanaType)
public void UnlockWeapon(WeaponType weaponType, PlayerOptionsData config)
public void UnlockStage(StageType stageType, PlayerOptionsData config)
public void UnlockHyper(StageType stageType, PlayerOptionsData config)
public void UnlockItem(ItemType itemType, PlayerOptionsData config)
public void UnlockPowerUp(PowerUpType powerUpType, PlayerOptionsData config)
public bool UnlockSecret(SecretType secretType, PlayerOptionsData config)
```

### Utility Methods

```csharp
// Check unlock status
public bool IsUnlocked(CharacterType characterType)
public HashSet<AchievementType> GetUnlockedAchievements()

// Track progress
public void TrackItemPickup(ItemType itemType, PlayerOptionsData config, bool trackRunPickup = true)
public void AwardAdventureStar()

// Online/multiplayer support
public PlayerOptionsData GetClientPlayerOptionsWithRunDataApplied()
public void ApplyClientConfigWithRunProgress()
public void BuildHostPlayerConfig(HostPlayerOptions hostPlayerOptions)
public void DestroyOnlineConfigs()
public void RemoveOnlineClientRunDataConfig()
```

## Save System Infrastructure

### Core Save Classes

**PhaserSaveDataUtils** (`Il2CppVampireSurvivors.Framework.Saves.PhaserSaveDataUtils`)
```csharp
public static void SaveDataLocal(PlayerOptionsData pod)
public static PlayerOptionsData LoadDataLocal()
public static void DeleteLocalSave()
public static bool HasLocalSave()
public static bool HasValidChecksum(string rawData, string checksum)
```

**SaveSystem** (`Il2CppVampireSurvivors.Framework.Saves.SaveSystem`)
```csharp
public Task SaveDataAsync(PlayerOptionsData pod, bool createBackup = false)
public Task<PlayerOptionsData> LoadDataAsync()
public Task DeleteSaveAsync()
```

**SaveUtils** (`Il2CppVampireSurvivors.Framework.Saves.SaveUtils`)
```csharp
public static Il2CppStructArray<byte> GetSerializedPlayerData(PlayerOptionsData data)
public static PlayerOptionsData TryParseData(Il2CppStructArray<byte> data)
public static SaveSummary GetSaveSummary(PlayerOptionsData pod)
public static bool AreIdentical(PlayerOptionsData saveA, PlayerOptionsData saveB)
public static string GenerateChecksum(string data)
public static bool ChecksumIsValid(string rawData, string checksum)
```

### Backup and Cloud Save

**SaveBackupService** (`Il2CppVampireSurvivors.App.Scripts.Framework.Platforms.Backend.Service.SaveBackupService`)
```csharp
public void Backup(PlayerOptionsData pod)
public PlayerOptionsData GetBackup()
public void ClearBackup()
public bool HasBackup()
```

**MultiSlotSaveStorage** (`Il2CppVampireSurvivors.App.Scripts.Framework.Platforms.Backend.Storage.MultiSlotSaveStorage`)
```csharp
public Task<bool> SetSlotData(int slot, PlayerOptionsData value)
public Task<PlayerOptionsData> GetSlotData(int slot)
public Task<PlayerOptionsData> GetMergeConflictSlotData()
```

## Usage Examples

### Reading Player Data

```csharp
// During gameplay (when GM.Core is available)
var gameManager = GM.Core;
if (gameManager != null && gameManager._playerOptions != null)
{
    var playerOptions = gameManager._playerOptions;
    float currentGold = playerOptions.Config.Coins;
    float runGold = playerOptions.Config.RunCoins;
    
    MelonLogger.Msg($"Persistent Gold: {currentGold}");
    MelonLogger.Msg($"Run Gold: {runGold}");
}

// From save file directly
PlayerOptionsData saveData = PhaserSaveDataUtils.LoadDataLocal();
if (saveData != null)
{
    float totalCoins = saveData.Coins;
    var unlockedCharacters = saveData.UnlockedCharacters;
}
```

### Modifying Coins

```csharp
// Add coins during gameplay (applies Greed multiplier)
var playerOptions = GM.Core?._playerOptions;
if (playerOptions != null)
{
    float actualAdded = playerOptions.AddCoins(100.0f, GM.Core.Player);
    MelonLogger.Msg($"Added {actualAdded} coins (after Greed multiplier)");
}

// Add coins without multiplier
playerOptions.AddCoinsFlat(100.0f);

// Add to persistent only (not run coins)
playerOptions.AddCoinsNoRun(100.0f);

// Remove coins for purchase
playerOptions.RemoveCoins(500, removeFromLifetime: false);
```

### Hook Points

```csharp
// Hook coin additions
[HarmonyPatch(typeof(PlayerOptions), "AddCoins")]
[HarmonyPrefix]
public static void OnAddCoins(ref float value, CharacterController player)
{
    // Modify coin value before it's added
    value *= 2.0f; // Double all coin gains
}

// Hook coin UI updates
[HarmonyPatch(typeof(MainGamePage), "UpdateCoins")]
[HarmonyPostfix]
public static void OnUpdateCoinsUI(MainGamePage __instance)
{
    // Coins UI has been updated
}
```

### Working with Golden Eggs

```csharp
// Check golden egg data
var saveData = PhaserSaveDataUtils.LoadDataLocal();
if (saveData != null)
{
    float totalEggs = saveData.TotalEggCount;
    var eggsByCharacter = saveData.CharacterEggCount;
    
    // Check eggs for specific character
    if (eggsByCharacter != null && eggsByCharacter.ContainsKey(CharacterType.ANTONIO))
    {
        float antonioEggs = eggsByCharacter[CharacterType.ANTONIO];
        MelonLogger.Msg($"Antonio has {antonioEggs} golden eggs");
    }
    
    // Access detailed egg stats
    var eggInfo = saveData.CharacterEggInfo;
    if (eggInfo != null && eggInfo.ContainsKey(CharacterType.ANTONIO))
    {
        var antonioStats = eggInfo[CharacterType.ANTONIO];
        foreach (var kvp in antonioStats)
        {
            // Keys like "MaxHp", "Power", "Luck", etc.
            MelonLogger.Msg($"  {kvp.Key}: {kvp.Value}");
        }
    }
}

// Using EggManager during gameplay
var eggManager = GM.Core?.Egg;
if (eggManager != null)
{
    // Add random egg to character
    var result = eggManager.AddGoldenEgg(CharacterType.ANTONIO, new Random());
    MelonLogger.Msg($"Added {result.Key} with value {result.Value}");
    
    // Apply bonuses to player
    eggManager.ApplyBonuses(GM.Core.Player);
}
```

### Checking Stage Completion

```csharp
// Check which stages a character has completed
var saveData = GM.Core?._playerOptions?.Data;
if (saveData != null && saveData.StageCompletionLog != null)
{
    if (saveData.StageCompletionLog.ContainsKey(CharacterType.ANTONIO))
    {
        var completedStages = saveData.StageCompletionLog[CharacterType.ANTONIO];
        foreach (var stage in completedStages)
        {
            MelonLogger.Msg($"Antonio completed: {stage}");
        }
    }
}

// Get detailed stage data
if (saveData.CharacterStageData != null)
{
    foreach (var kvp in saveData.CharacterStageData)
    {
        CharacterType character = kvp.Key;
        var stageDataList = kvp.Value;
        // Process stage data...
    }
}
```

### Adventure Mode Data Access

```csharp
// Check adventure progress
var saveData = GM.Core?._playerOptions?.Config;
if (saveData != null)
{
    float stars = saveData.AdventureStars;
    var progress = saveData.AdventureProgress;
    
    MelonLogger.Msg($"Adventure Stars: {stars}");
    
    if (progress != null)
    {
        foreach (var achievement in progress)
        {
            MelonLogger.Msg($"Completed: {achievement}");
        }
    }
    
    // Access nested adventure saves
    var adventureSaves = saveData.AdventuresSaveData;
    if (adventureSaves != null)
    {
        foreach (var kvp in adventureSaves)
        {
            AdventureType adventure = kvp.Key;
            PlayerOptionsData adventureData = kvp.Value;
            // Each adventure has its own complete save data
            MelonLogger.Msg($"Adventure {adventure}: {adventureData.Coins} coins");
        }
    }
}

// Using AdventureManager
var adventureManager = GM.Core?.AdventureManager;
if (adventureManager != null)
{
    bool isInAdventure = adventureManager.IsInAdventureMode;
    var currentAdventure = adventureManager.CurrentAdventure;
    
    if (adventureManager.IsAdventureCompleted(AdventureType.ADV_LMS_001))
    {
        MelonLogger.Msg("Legacy of Moonspell completed!");
    }
    
    if (adventureManager.CanAscend(AdventureType.ADV_LMS_001))
    {
        adventureManager.AscendAdventure(AdventureType.ADV_LMS_001, true);
    }
}
```

### PowerUp and Collection Management

```csharp
// Check purchased power-ups
var saveData = GM.Core?._playerOptions?.Config;
if (saveData != null)
{
    // Check bought power-up levels
    var boughtPowerups = saveData.BoughtPowerups;
    if (boughtPowerups != null)
    {
        foreach (var powerupLevel in boughtPowerups)
        {
            MelonLogger.Msg($"Bought: {powerupLevel.PowerUp} level {powerupLevel.Level}");
        }
    }
    
    // Check unlocked power-up ranks
    var unlockedRanks = saveData.UnlockedPowerUpRanks;
    if (unlockedRanks != null)
    {
        foreach (var powerupType in unlockedRanks)
        {
            MelonLogger.Msg($"Unlocked rank: {powerupType}");
        }
    }
    
    // Check ascension points (mainly Luck, Growth, Greed, Curse)
    var ascensionAllocation = saveData.AscensionPointsAllocation;
    if (ascensionAllocation != null)
    {
        foreach (var kvp in ascensionAllocation)
        {
            PowerUpType powerup = kvp.Key;
            int points = kvp.Value;
            MelonLogger.Msg($"{powerup}: {points} ascension points");
        }
    }
    
    // Check disabled power-ups
    var disabledPowerups = saveData.DisabledPowerups;
    if (disabledPowerups != null)
    {
        foreach (var disabled in disabledPowerups)
        {
            MelonLogger.Msg($"Disabled: {disabled}");
        }
    }
}
```

## Gold/Coin System Flow

### Two-Tier System

1. **Persistent Gold** (`PlayerOptionsData.Coins`)
   - Saved between sessions
   - Used for purchases in menus
   - Persists across game restarts

2. **Run Gold** (`PlayerOptionsData.RunCoins`)
   - Tracks earnings in current run
   - Displayed in gameplay UI
   - Added to persistent gold on run completion
   - Reset at start of each run

### Coin Addition Flow

1. Coin pickup collected in game
2. `PlayerOptions.AddCoins()` called
3. Greed multiplier applied (if player has Greed stat)
4. Value added to both `Coins` and `RunCoins`
5. `OnAfterCoinsAddedSignal` fired
6. UI updated via `MainGamePage.UpdateCoins()`

### UI Display

- **Menu**: `CoinsUI` displays `PlayerOptionsData.Coins`
- **Gameplay**: `MainGamePage` displays `PlayerOptionsData.RunCoins`
- **Gold Fever**: Special UI mode via `GoldFeverUIManager`

## Important Notes

### Null Safety
Always check for null when accessing PlayerOptions during gameplay:
```csharp
var playerOptions = GM.Core?._playerOptions;
if (playerOptions != null && playerOptions.Config != null)
{
    // Safe to access
}
```

### Save Data Persistence
- Coins are automatically saved as part of PlayerOptionsData
- Use `PlayerOptions.Save(commitImmediately: true, createBackup: true)` to force immediate save with backup
- Save system includes checksum validation for data integrity
- Multiple save slots supported through MultiSlotSaveStorage
- Cloud save integration available through platform-specific implementations

### Greed Stat Integration
The Greed stat (`PlayerModifierStats.Greed`) acts as a multiplier:
- Base pickup value × (1 + Greed/100) = actual coins gained
- Only applies when using `AddCoins()`, not `AddCoinsFlat()`

### Thread Safety
All modifications must occur on the main Unity thread.