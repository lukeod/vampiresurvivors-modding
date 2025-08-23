# UI System

Comprehensive UI system built on Unity's UI framework with TextMeshPro integration. Handles in-game HUD elements, menus, character selection, and gameplay interfaces.

## Core UI Architecture

### BaseUIPage System
Page-based UI architecture where screens extend `BaseUIPage`:

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
- Results recap (`RecapPage`)
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

**Additional Specialized Pages:**
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

Manages experience bar, kill counters, coin display, time, level, equipment panels, and game controls.

## Font and Text Systems

### FontFactory

Centralized font asset management system using Unity's Addressable system.

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

TextMeshPro provides high-quality text rendering, rich formatting, and efficient font management.

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
Update methods run every frame. Avoid hooking high-frequency methods, cache UI references, and minimize string allocations.

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
The UI system uses a signal-based event architecture with `SignalBus`:
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
Specialized dropdown component for game-specific needs:
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
The UI system maintains consistent styling across:
- Color schemes and palettes
- Font choices and sizing
- Icon styles and formats
- Animation patterns

### Dynamic Theming
UI elements support:
- Day/night themes
- Character-specific styling
- Unlockable UI customizations

## Performance Optimization

### UI Update Efficiency
Optimization strategies: conditional updates (only update UI when values change), object pooling (reuse UI elements for dynamic content), batched operations (group multiple UI updates together), and cached references (store frequently accessed UI components).

### Text Rendering Optimization
TextMeshPro optimization techniques: font atlas management (efficient character set loading), string pooling (reuse formatted strings), and dynamic text culling (hide off-screen text elements).

## Debug and Development UI

### CheatUIHider and DebugUIHider
The game includes multiple debug/cheat UI management classes: `CheatUIHider` (manages cheat interface visibility and functionality), `DebugUIHider` (provides minimal debug UI functionality for hiding debug elements), and limited debug console features for development-time UI management.

### Debug Features
Debug and cheat UI functionality includes standard debug UI with `DebugUIHider` class, cheat interfaces managed by `CheatUIHider`, local cheats panel (`_CheatsPanel`) in MainGamePage, and online cheats panel (`_OnlineCheatsPanel`) for multiplayer features.

## Common Modding Scenarios

### Custom HUD Elements
```csharp
[HarmonyPatch(typeof(MainGamePage), "Update")]
[HarmonyPostfix]
public static void AddCustomHUD(MainGamePage __instance)
{
    // Add custom UI elements to the main game HUD
    // Be careful about performance impact
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
    // Note: Method name is GetAvailableEquipmentForEvolution, not GetAvailableEquipment
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
The UI system uses Zenject for dependency injection. Key dependencies include:
- `SignalBus` - For event-driven UI updates
- `GameSessionData` - Current game session information
- `PlayerOptions` - Player configuration and settings
- Various game managers (LootManager, WeaponsFacade, etc.)

### Game State Integration
UI elements are bound to game state through:
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

## Accessibility and Localization

### Text Scaling and Formatting
UI components support dynamic text scaling for accessibility, various screen resolutions and aspect ratios, and different font sizes and styles.

### Localization Considerations
The game uses I2 Localization system for internationalization with LocalizedString structures for text content (`_levelString` in MainGamePage) and BaseUIPage text parsing for localized content. UI text modifications require considering text length variations in different languages, maintaining UI layout flexibility, using proper text encoding for international characters, and respecting the existing localization framework.

## Common Issues
- **Performance**: Too many UI updates per frame
- **Text overflow**: Long text not fitting containers
- **Missing references**: Components not initialized
- **Input conflicts**: Custom UI interfering with controls
- **Memory leaks**: Elements not cleaned up

## Advanced UI Features

### Dynamic UI Generation
Complex mods can use runtime UI element creation using equipment panel prefabs (`_PlayerEquipmentPanelPrefab`), data-driven UI configuration, modular UI component systems, and force layout rebuilds with `ForceEquipmentLayoutRebuild()`.

### Animation and Effects
UI animations enhance user experience through smooth transitions between states, visual feedback for user actions, and particle effects integrated with UI elements.

### Custom Input Handling
Advanced UI modifications require custom input event processing, integration with game input systems, and support for various input devices.

## UI Asset Management

### Sprite and Icon Management
UI elements use character portraits and icons, weapon and item sprites, UI decoration and background elements, and status effect indicators.

### Asset Loading and Caching
Efficient asset management includes lazy loading of UI assets, proper asset disposal and cleanup, and shared asset pools for common elements.