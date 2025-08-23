# UI System

Based on code analysis, this UI system appears to use Unity's UI framework with TextMeshPro integration. The decompiled IL2CPP code indicates handling of in-game HUD elements, menus, character selection, and gameplay interfaces.

## Core UI Architecture

### BaseUIPage System
Based on the decompiled code, this appears to be a page-based UI architecture where screens extend `BaseUIPage`:

**Core Game Pages:**
- Character selection (`CharacterSelectionPage`)
- Main game HUD (`MainGamePage`)  
- Collections (`CollectionsPage`)
- Director mode (`DirectorPage : GameWindowedUIPage : BaseUIPage`)
- Level selection (`LevelBonusSelectionPage`)
- Arcana UI (`ArcanaUIPage`)
- Piano interface (`PianoPage`)
- Pause menu (`PausePage`)
- Bestiary (`BestiaryPage`)
- Power-ups display (`PowerUpsPage`)

**Menu Pages:**
- Main menu (`MainMenuPage`)
- Account page (`AccountPage`)
- Options (`OptionsPage`)
- Languages (`LanguagesPage`)
- Warning dialogs (`WarningPage`)
- Menu banner (`MenuBannerPage`)
- Credits (`MenuCreditsPage`)
- Connection error (`ConnectionErrorPage`)
- Online error (`OnlineErrorPage`)
- Online lobby (`OnlineLobbyPage`)
- Room selection (`RoomSelectionPage`)

**Game State Pages:**
- Game over (`GameOverPage`)
- Enhanced game over (`GameoverinoPage`)
- Results recap (`RecapPage`) - See [RecapPage Details](#recappage)
- Results display (`ResultPage`)
- Final credits (`FinalCreditsPage`)
- Final fireworks (`FinalFireworksPage`)

**Item/Treasure Pages:**
- Item found (`ItemFoundPage`)
- Character found (`CharacterFoundPage`)
- Open treasure (`OpenTreasurePage`)

**Store/Achievement Pages:**
- DLC store (`DLCStorePage`)
- Achievements (`AchievementsPage`)
- Secrets (`SecretsPage`)

**Additional UI Pages:**
- Stage selection (`StageSelectPage`)
- Arcana main selection (`ArcanaMainSelectionPage`)
- Arcana small selection (`ArcanaSmallSelectionPage`)
- TP credits (`TPCreditsPage`)
- TP weapon selection (`TPWeaponSelectionPage`)
- Background selection (`BackgroundPage`)
- Base weapon selection (`BaseWeaponSelectionPage`)
- Weapon selection (`WeaponSelectionPage`)
- Select adventures (`SelectAdventuresPage`)

**GameWindowedUIPage Derivatives:**
- Healer interface (`HealerPage`)
- Merchant UI (`MerchantUIPage`)

### MainGamePage
Main in-game UI controller:

```csharp
var mainUI = GM.Core?.MainUI;
```

Based on code analysis, appears to manage experience bar, kill counters, coin display, time, level, equipment panels, and game controls.

## Font and Text Systems

### FontFactory

Based on decompiled code analysis, this appears to be a centralized font asset management system using Unity's Addressable system.

```csharp
public class FontFactory : SerializedScriptableObject
{
    public UnityFontRefDictionary _Fonts;        // Unity font references
    public TMPFontRefDictionary _TMPFonts;       // TextMeshPro font references
}
```

#### Font Access Methods
- `Font GetFont(string fontName)` - Retrieve Unity font by name
- `TMP_FontAsset GetTMPFont(string fontName)` - Retrieve TextMeshPro font by name

#### Data Structures
```csharp
public class UnityFontRefData : Il2CppSystem.Object
{
    public AssetReferenceT<Font> UnityFontRef { get; set; }
}

public class TMPFontRefData : Il2CppSystem.Object
{
    public AssetReferenceT<TMP_FontAsset> TMPFontRef { get; set; }
}
```

#### Access Pattern
```csharp
// Via GameManager
FontFactory fontFactory = GM.Core.FontFactory;

// Get fonts
Font myFont = fontFactory.GetFont("MenuFont");
TMP_FontAsset tmpFont = fontFactory.GetTMPFont("UIFont");
```

### TextMeshPro Integration

Multiple UI classes extensively use TextMeshPro for text rendering:

**Key UI Classes with TextMeshPro**:
- `CoinsUI`, `CharacterSelectionPage`, `CollectionsPage`
- `MainGamePage`, `LevelUpItemUI`, `CustomDropDown`

Based on the code structure, TextMeshPro appears to provide text rendering, formatting, and font management functionality.

## HUD Update System

### Real-Time UI Updates
The MainGamePage implements several update methods for UI synchronization:

```csharp
// Main update methods
public override void Update()           // Main update loop (inherits from BaseUIPage)
public void UpdateKills()              // Kill counter updates
public void UpdateExperienceProgress(GameplaySignals.CharacterXpChangedSignal sig) // XP updates via signals
public void UpdateCoins()              // Coin display updates
```

### Performance Considerations
Based on code analysis, update methods appear to run every frame. You should avoid hooking high-frequency methods, cache UI references, and minimize string allocations.

## UI Component Categories

### Status Displays
- **Health Bar**: Shows current and maximum HP
- **Experience Bar**: Displays XP progress to next level
- **Timer**: Game time and survival duration
- **Kill Counter**: Enemy elimination statistics

### Resource Displays
- **Coin Counter**: Current currency amount
- **Gem Display**: Special currency or resources
- **Achievement Progress**: Real-time achievement tracking

### Equipment Displays
- **Weapon Icons**: Current equipped weapons with levels
- **Accessory/Item Icons**: Passive items and their effects
- **Evolution Indicators**: Shows available weapon evolutions
- **Equipment Panel Container**: Dynamic equipment UI (`_EquipmentPanelContainer`)
- **Equipment Panel Prefabs**: Instantiated from `_PlayerEquipmentPanelPrefab`

### Interactive Elements
- **Level Up Panel**: Weapon/item selection on level up (`LevelUpPage`)
- **Pause Menu**: Game settings and controls
- **Character Selection**: Pre-game character choice (`CharacterSelectionPage`)
- **Twitch Integration**: Real-time Twitch event display (`_TwitchStageEventsPanel`)
- **Gold Fever UI**: Special game mode interface (`_GoldFever` - GoldFeverUIManager)
- **Glimmer Technique Carousel**: Special technique display (`_GlimmerTechniqueCarousel`)
- **Online Cheats Panel**: Online cheating interface (`_OnlineCheatsPanel`)
- **Spectate Mode Container**: Spectator mode UI container (`_SpectateModeContainer`)
- **Spectate Mode Icon**: Icon for spectate mode (`_SpectateModeIcon`)
- **Spectate Mode Player Name**: Player name display in spectate mode (`_SpectateModePlayerName`)

## UI Event System

### Signal-Based Architecture
Based on decompiled code, the UI system appears to use a signal-based event architecture with `SignalBus`:
- XP changes trigger `GameplaySignals.CharacterXpChangedSignal`
- UI toggles use `UISignals.ToggleXPBarSignal` and `UISignals.ToggleWeaponSlotsSignal`
- Glimmer techniques fire `UISignals.FireNewGlimmerTechnique`

### User Input Handling
UI components respond to various input events:
- Click/tap interactions
- Keyboard navigation
- Controller input support (via Rewired integration)
- Touch gestures (mobile platforms)

### UI State Management
The UI system manages various states:
- In-game active state
- Paused state
- Menu navigation states
- Transition animations via scene transition fader

## Custom UI Components

### CustomDropDown
Based on code analysis, this appears to be a dropdown component for game-specific needs:
- Custom styling and theming
- Game data integration
- Performance optimization for large lists

### Tutorial System
`TutorialPopup` provides contextual help:
- Dynamic tutorial content
- Progressive disclosure
- Context-sensitive tips

## UI Styling and Theming

### Visual Consistency
Based on code analysis, the UI system appears to maintain styling across:
- Color schemes and palettes
- Font choices and sizing
- Icon styles and formats
- Animation patterns

### Dynamic Theming
Based on the code structure, UI elements appear to support:
- Day/night themes
- Character-specific styling
- Unlockable UI customizations

## Performance Optimization

### UI Update Efficiency
Potential optimization strategies based on code patterns: conditional updates (only update UI when values change), object pooling (reuse UI elements for dynamic content), batched operations (group multiple UI updates together), and cached references (store frequently accessed UI components).

### Text Rendering Optimization
Potential TextMeshPro optimization techniques based on code structure: font atlas management (character set loading), string pooling (formatted string reuse), and text culling (off-screen text elements).

## Debug and Development UI

### CheatUIHider and DebugUIHider
Based on decompiled code analysis, the game includes multiple debug/cheat UI management classes: `CheatUIHider` (appears to manage cheat interface visibility and functionality), `DebugUIHider` (appears to provide minimal debug UI functionality for hiding debug elements), and limited debug console features for development-time UI management.

### Debug Features
Based on code analysis, debug and cheat UI functionality appears to include standard debug UI with `DebugUIHider` class, cheat interfaces managed by `CheatUIHider`, local cheats panel (`_CheatsPanel`) in MainGamePage, and online cheats panel (`_OnlineCheatsPanel`) for multiplayer features.

## Common Modding Scenarios

### Custom HUD Elements
```csharp
[HarmonyPatch(typeof(MainGamePage), "Update")]
[HarmonyPostfix]
public static void AddCustomHUD(MainGamePage __instance)
{
    // Add custom UI elements to the main game HUD
    // Consider performance impact based on update frequency
}
```

### Modifying UI Text
```csharp
// Hook into text updates to modify displayed values
[HarmonyPatch(typeof(MainGamePage), "UpdateCoins")]
[HarmonyPostfix]
public static void CustomCoinDisplay()
{
    // Modify coin display logic
    // Update TextMeshPro components with custom formatting
}
```

### Custom Level Up Options
```csharp
// Modify level up selection interface
[HarmonyPatch(typeof(LevelUpFactory), "GetAvailableEquipmentForEvolution")]
[HarmonyPostfix]
public static void CustomLevelUpOptions(ref List<Equipment> __result)
{
    // Add custom weapons/items to level up choices
    // Note: Method name is GetAvailableEquipmentForEvolution based on decompiled code
    // Modify the UI to display additional options
}
```

### UI Theme Modifications
```csharp
// Change UI colors or styling
[HarmonyPatch(typeof(CharacterSelectionPage), "Awake")]
[HarmonyPostfix]
public static void CustomUITheme(CharacterSelectionPage __instance)
{
    // Modify UI component colors, fonts, or layouts
    // Access TextMeshPro components for custom styling
}
```

### Font Replacement
```csharp
[HarmonyPatch(typeof(FontFactory), "GetTMPFont")]
[HarmonyPostfix]
public static void CustomFontOverride(string fontName, ref TMP_FontAsset __result)
{
    // Replace specific fonts
    switch (fontName)
    {
        case "DamageFont":
            if (ModSettings.UseHighContrastDamage)
                __result = LoadCustomFont("HighContrastDamage");
            break;
        case "UIFont":
            if (ModSettings.UseDyslexiaFriendlyFont)
                __result = LoadCustomFont("DyslexiaFriendly");
            break;
    }
}
```

## UI Data Binding

### Dependency Injection Architecture
Based on decompiled code analysis, the UI system appears to use Zenject for dependency injection. Key dependencies include:
- `SignalBus` - For event-driven UI updates
- `GameSessionData` - Current game session information
- `PlayerOptions` - Player configuration and settings
- Various game managers (LootManager, WeaponsFacade, etc.)

### Game State Integration
Based on code analysis, UI elements appear to be bound to game state through:
- Direct property access to game managers via dependency injection
- Signal-driven updates from game systems (primary pattern)
- Session data access through `_session` (GameSessionData)
- Player options through `_playerOptions` (PlayerOptions)

### Data Flow Patterns
```csharp
// Signal-based UI update pattern (preferred)
public void UpdateExperienceProgress(GameplaySignals.CharacterXpChangedSignal sig)
{
    // UI updates triggered by game events rather than polling
    _ExperienceProgress.fillAmount = sig.normalizedXP;
}

// Direct access pattern
void UpdateUIElement()
{
    var gameData = GM.Core?.SomeGameSystem;
    if (gameData != null)
    {
        uiComponent.text = gameData.SomeValue.ToString();
    }
}
```

## RecapPage

**Location**: `Il2CppVampireSurvivors.UI.RecapPage`

The RecapPage displays end-of-game statistics including weapon performance, collected items, and run summary data.

### StatsDisplay Structure

The page uses a `StatsDisplay` value type to represent weapon statistics:

```csharp
public sealed class StatsDisplay : Il2CppSystem.ValueType
{
    public string Name;               // Weapon name
    public int Level;                 // Weapon level achieved
    public string WeaponFrameName;    // UI frame reference
    public string WeaponTextureName;  // Texture reference
    public float InflictedDamage;     // Total damage dealt
    public float Lifetime;            // Time weapon was active
    public float Dps;                 // Calculated DPS
    public bool IsBestDps;           // Highest DPS weapon
    public bool IsBestRaw;           // Highest total damage
    public CharacterType Owner;       // Character who used weapon
    public string NameColor;          // UI color for weapon name
}
```

### Data Collection

The `AddWeapons()` method collects statistics from all equipped weapons:

1. Iterates through `WeaponsManager.Weapons`, `PassiveWeapons`, and `Evolutions`
2. Reads `StatsInflictedDamage` and `StatsLifetime` from each weapon instance
3. Calculates DPS as `InflictedDamage / Lifetime`
4. Determines best performers for highlighting
5. Generates UI elements for display

### Accessing Recap Data

```csharp
// Hook into recap display
[HarmonyPatch(typeof(RecapPage), "OnShowStart")]
[HarmonyPostfix]
public static void OnRecapShow(RecapPage __instance)
{
    // Recap page is now active with collected stats
}

// Access weapon statistics during gameplay
var weapon = player.weaponsManager.Weapons[0] as Weapon;
float totalDamage = weapon.StatsInflictedDamage;
float activeTime = weapon.StatsLifetime;
```

### Integration with PlayerOptionsData

The recap page also displays run statistics from `PlayerOptionsData`:
- `RunCoins` - Gold earned
- `RunEnemies` - Enemies killed
- `RunPickups` - Items collected
- `RunBossesCount` - Bosses defeated

## Accessibility and Localization

### Text Scaling and Formatting
Based on code analysis, UI components appear to support dynamic text scaling for accessibility, various screen resolutions and aspect ratios, and different font sizes and styles.

### Localization Considerations
Based on decompiled code analysis, the game appears to use I2 Localization system for internationalization with LocalizedString structures for text content (`_levelString` in MainGamePage) and BaseUIPage text parsing for localized content. UI text modifications require considering text length variations in different languages, maintaining UI layout flexibility, using proper text encoding for international characters, and respecting the existing localization framework.

## Common Issues
- **Performance**: Too many UI updates per frame
- **Text overflow**: Long text not fitting containers
- **Missing references**: Components not initialized
- **Input conflicts**: Custom UI interfering with controls
- **Memory leaks**: Elements not cleaned up

## Additional UI Features

### Dynamic UI Generation
Based on code analysis, mods appear able to use runtime UI element creation using equipment panel prefabs (`_PlayerEquipmentPanelPrefab`), data-driven UI configuration, modular UI component systems, and force layout rebuilds with `ForceEquipmentLayoutRebuild()`.

### Animation and Effects
Based on code analysis, UI animations appear to support transitions between states, visual feedback for user actions, and particle effects integrated with UI elements.

### Custom Input Handling
Based on code analysis, UI modifications appear to require custom input event processing, integration with game input systems, and support for various input devices.

## UI Asset Management

### Sprite and Icon Management
Based on code analysis, UI elements appear to use character portraits and icons, weapon and item sprites, UI decoration and background elements, and status effect indicators.

### Asset Loading and Caching
Based on code structure, asset management appears to include lazy loading of UI assets, asset disposal and cleanup, and shared asset pools for common elements.