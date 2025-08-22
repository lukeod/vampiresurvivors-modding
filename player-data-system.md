# Player Data System

Comprehensive documentation of player data structures, save data, and runtime player state management in Vampire Survivors.

## Overview

The player data system consists of two main components:
- **PlayerOptionsData** - Serializable save data structure containing persistent player progress
- **PlayerOptions** - Runtime manager class that handles player data operations

## PlayerOptionsData

**Location**: `Il2CppVampireSurvivors.Data.PlayerOptionsData`

The primary save data structure containing all persistent player information.

### Currency and Economy Fields

```csharp
public float Coins                    // Persistent gold balance (saved between sessions)
public float RunCoins                 // Gold earned in current run only
public float LifetimeCoins           // Total gold ever collected
public float TotalCoins               // Another total gold tracking field
public int RunPickups_Coins          // Count of coin pickups in current run
public int BeginnersLuck              // Beginner's luck counter
```

### Run Statistics

```csharp
public int RunEnemies                 // Enemies killed in current run
public int RunBossesCount             // Bosses defeated in current run
public List<EnemyType> RunBossesTypes // Types of bosses encountered
public List<ItemType> RunPickups      // Items collected in current run
public int RunStarryHeavnes          // Starry Heaven pickups
public int RunWeirdSoulsPurifier     // Weird Souls Purifier count
public float RunFever                 // Fever time in current run
public int RunHunger                  // Hunger counter
public float RawRunHeal               // Total healing in run
```

### Lifetime Statistics

```csharp
public float LifetimeSurvived        // Total time survived across all runs
public float LifetimeHeal            // Total healing across all runs
public float OwO                     // Special statistic
public int CompletedHurries          // Number of Hurry mode completions
public float TrainHazardEnemiesHit   // Train hazard enemy hits
public float LongestFever            // Longest fever duration
public float HighestFever            // Highest fever value
```

### Game Mode Settings

```csharp
public CharacterType SelectedCharacter    // Currently selected character
public StageType SelectedStage           // Currently selected stage
public bool SelectedHyper                // Hyper mode enabled
public bool SelectedHurry                // Hurry mode enabled
public bool SelectedMazzo                // Mazzo (randomizer) mode enabled
public bool SelectedLimitBreak           // Limit Break mode enabled
public bool SelectedInverse              // Inverse mode enabled
public bool SelectedReapers              // Reapers enabled
public bool SelectedGoldenEggs           // Golden Eggs enabled
public bool SelectedSharePassives        // Share Passives enabled
public int SelectedArcana                // Selected Arcana card
public bool SelectedRandomEvents         // Random events enabled
public bool SelectedRandomLevels         // Random level-ups enabled
public int SelectedMaxWeapons           // Maximum weapon slots
```

### Audio/Visual Settings

```csharp
public bool SoundsEnabled            // Sound effects enabled
public bool MusicEnabled             // Music enabled
public float SoundsVolume            // Sound effects volume (0-1)
public float MusicVolume             // Music volume (0-1)
public bool FlashingVFXEnabled       // Flashing visual effects enabled
public bool DamageNumbersEnabled     // Damage numbers display
public bool StreamSafeEnabled        // Stream-safe mode
public bool hideXPBar                // Hide experience bar
public bool HideProgress             // Hide progress indicators
public bool ClassicMusic             // Use classic music
public bool VisuallyInvertStages     // Visually invert stages
```

### Collections and Unlocks

```csharp
public List<CharacterType> BoughtCharacters      // Purchased characters
public List<CharacterType> UnlockedCharacters    // Unlocked characters
public List<SkinType> BoughtSkins               // Purchased skins
public List<SkinType> UnlockedSkins             // Unlocked skins
public List<PowerUpLevel> BoughtPowerups        // Purchased power-up levels
public List<PowerUpType> UnlockedPowerUpRanks   // Unlocked power-up ranks
public List<PowerUpType> DisabledPowerups       // Disabled power-ups
public List<WeaponType> CollectedWeapons        // Collected weapons
public List<WeaponType> UnlockedWeapons         // Unlocked weapons
public List<StageType> UnlockedStages           // Unlocked stages
public List<StageType> UnlockedHypers           // Unlocked hyper stages
public List<ArcanaType> UnlockedArcanas         // Unlocked arcana cards
public List<CollectionEntry> itemInCollection    // Collection items
public List<UnlockType> itemInUnlocks           // Unlocked items
public List<SecretType> itemInSecrets           // Discovered secrets
public List<AchievementData> Achievements       // Achievement progress
public List<SecretData> Secrets                 // Secret progress
```

### Golden Egg System

```csharp
public bool SelectedGoldenEggs                                           // Golden eggs enabled
public float TotalEggCount                                              // Total eggs purchased
public Dictionary<CharacterType, float> CharacterEggCount               // Eggs per character
public Dictionary<CharacterType, Dictionary<string, float>> CharacterEggInfo  // Detailed egg stats per character
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
public AdventureType SelectedAdventureType                              // Selected adventure
public float AdventureStars                                            // Adventure currency
public int AdventureCompletionCount                                    // Adventures completed
public List<AdventureAchievementType> AdventureProgress                // Adventure achievements
public Dictionary<AdventureType, PlayerOptionsData> AdventuresSaveData // Nested save data per adventure
public float TotalAdventurePlaytime                                    // Total time in adventures
public bool HasSeenAdventuresIntroTutorial                            // Tutorial viewed flag
public bool HideUnavailableAdventures                                 // UI preference
public bool ShouldPlayAdventureReveal                                 // Animation flag
```

### PowerUp and Ascension System

```csharp
public Dictionary<PowerUpType, int> AscensionPointsAllocation          // Ascension points spent
public int AscensionPoints                                            // Available ascension points
```

### Prop and Environment Interaction

```csharp
public Dictionary<PropType, int> PickupCount                          // Pickups collected by type
public Dictionary<PropType, int> DestroyedCount                       // Props destroyed by type
```

### Platform and Save Metadata

```csharp
public string saveDate                // Last save date/time
public string Platform                // Platform identifier
public string SaveOriginalPlatform    // Original platform where save was created
public List<string> SaveTouchedPlatforms // All platforms that have touched this save
public int Version                    // Save data version
public bool CheatCodeUsed            // Whether cheat codes have been used
```

### Special Progress Flags

```csharp
public bool HasKilledTheFinalBoss    // Defeated final boss
public bool HasSeenFinalFireworks    // Viewed ending fireworks
public bool HasUsedMirror            // Used mirror item
public bool HasUsedTrumpet           // Used trumpet item
public bool HasPlayedStage3          // Played stage 3
public bool HasSeenDarkanaTransition // Seen Darkana transition
```

## PlayerOptions

**Location**: `Il2CppVampireSurvivors.Objects.PlayerOptions`

Runtime manager class that handles player data operations and provides access to PlayerOptionsData.

### Key Properties

```csharp
public PlayerOptionsData Data { get; set; }  // The underlying save data
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
```

### Save Operations

```csharp
// Save current data
public void Save(bool local = false, bool cloud = true, bool createBackup = false)

// Load save data
public void Load()

// Reset to defaults
public void ResetToDefault()
```

## Usage Examples

### Reading Player Data

```csharp
// During gameplay (when GM.Core is available)
var gameManager = GM.Core;
if (gameManager != null && gameManager._playerOptions != null)
{
    var playerOptions = gameManager._playerOptions;
    float currentGold = playerOptions.Data.Coins;
    float runGold = playerOptions.Data.RunCoins;
    
    MelonLogger.Msg($"Persistent Gold: {currentGold}");
    MelonLogger.Msg($"Run Gold: {runGold}");
}

// From save file directly
PlayerOptionsData saveData = PhaserSaveDataUtils.LoadSaveFiles();
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
var saveData = PhaserSaveDataUtils.LoadSaveFiles();
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
            MelonLogger.Msg($"  {kvp.Key}: {kvp.Value}");
        }
    }
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
var saveData = GM.Core?._playerOptions?.Data;
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
```

### PowerUp and Collection Management

```csharp
// Check purchased power-ups
var saveData = GM.Core?._playerOptions?.Data;
if (saveData != null)
{
    // Check bought power-up levels
    var boughtPowerups = saveData.BoughtPowerups;
    if (boughtPowerups != null)
    {
        foreach (var powerupLevel in boughtPowerups)
        {
            MelonLogger.Msg($"Bought: {powerupLevel}");
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
    
    // Check ascension points
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
if (playerOptions != null && playerOptions.Data != null)
{
    // Safe to access
}
```

### Save Data Persistence
- Coins are automatically saved as part of PlayerOptionsData
- Use `PlayerOptions.Save()` to force immediate save
- Save system handles backup creation automatically

### Greed Stat Integration
The Greed stat (`PlayerModifierStats.Greed`) acts as a multiplier:
- Base pickup value × (1 + Greed/100) = actual coins gained
- Only applies when using `AddCoins()`, not `AddCoinsFlat()`

### Thread Safety
All modifications must occur on the main Unity thread.