# Localization System

Vampire Survivors uses a custom localization system for multi-language support and runtime text management. The system provides direct access to localized content through data classes.

## Core Components

### LanguageController
**Location**: `Il2CppVampireSurvivors.LanguageController`

Manages language selection and UI updates:

```csharp
public class LanguageController : MonoBehaviour
{
    // Get current language name
    public static string GetCurrentLanguageName();
    
    // Apply a specific language
    public void ApplyLanguage(string code);
    
    // Update language UI layout
    public void Set();
}
```

### Data-Specific Localization
Each data type handles its own localization through specialized methods:

```csharp
// Weapon localization
public class WeaponData
{
    public string GetLocalizedNameTerm(WeaponType wType);
    public string GetLocalizedDescriptionTerm(WeaponType wType);
    public string GetLocalizedTipsTerm(WeaponType wType);
    public string GetTranslation(string term);
}

// Character localization
public class CharacterData
{
    public string GetFullName(CharacterType t, bool ignoreSkinPrefixSuffix = false, bool splitDualCharacterNames = true);
    public string GetFullNameUntranslated();
}

// Item localization
public class ItemData
{
    public string GetLocalizedName(ItemType type);
}

// Stage localization
public class StageData
{
    public string GetLocalizedName(StageType sType);
    public string GetLocalizedTips(StageType sType);
}
```

## Localization Access Patterns

### Direct Data Access
Localization is accessed directly through data classes without term-based lookups:

```csharp
// Get localized weapon name
var weaponData = // ... get weapon data reference
string weaponName = weaponData.GetLocalizedNameTerm(WeaponType.WHIP);

// Get localized character name
var characterData = // ... get character data reference
string characterName = characterData.GetFullName(CharacterType.ANTONIO);

// Get localized item name
var itemData = // ... get item data reference
string itemName = itemData.GetLocalizedName(ItemType.SPINACH);
```

### Language UI Components
Language selection uses dedicated UI components:

```csharp
public class LanguageButtonUI : MonoBehaviour
{
    public string Code;      // Language code
    public string Name;      // Display name
    
    public void SetLanguage(LanguageController controller, string name, string code);
    public void ApplyLanguage();
}
```

## Custom Localization Integration

### Hooking Language Changes

```csharp
// Hook language controller for custom localization
[HarmonyPatch(typeof(LanguageController), "ApplyLanguage")]
[HarmonyPostfix]
public static void OnLanguageChanged(string code)
{
    // Called when language is changed
    // Update custom localization here
    UpdateCustomLocalization(code);
}

// Monitor language selection
[HarmonyPatch(typeof(LanguageController), "GetCurrentLanguageName")]
[HarmonyPostfix]
public static void OnGetCurrentLanguage(ref string __result)
{
    // Access current language name
    string currentLang = __result;
}
```

### Custom Translation Helper

```csharp
public static class CustomLocalization
{
    private static Dictionary<string, Dictionary<string, string>> customTerms = new();
    
    public static void AddCustomTerm(string key, string language, string value)
    {
        if (!customTerms.ContainsKey(key))
            customTerms[key] = new Dictionary<string, string>();
            
        customTerms[key][language] = value;
    }
    
    public static string GetCustomTranslation(string key, string fallback = "")
    {
        string currentLang = LanguageController.GetCurrentLanguageName();
        
        if (customTerms.ContainsKey(key) && customTerms[key].ContainsKey(currentLang))
            return customTerms[key][currentLang];
            
        return fallback;
    }
}
```

## Language System Types

### Language Support
The system supports multiple languages through language codes:

```csharp
// Language button configuration
public class LanguageButtonUI
{
    public string Code;  // Language code (e.g., "en", "it", "es")
    public string Name;  // Display name (e.g., "English", "Italiano")
}
```

### Platform Integration
**Location**: `Il2CppVampireSurvivors.App.Scripts.Framework.Initialisation.PlatformIntegration`

```csharp
public static class PlatformIntegration
{
    // Set current language code based on platform
    public static void SetCurrentLanguageCode();
}
```

## Integration with Game Systems

### WeaponData Localization
Weapons provide direct localization methods:

```csharp
public class WeaponData
{
    // Get localized weapon name
    public string GetLocalizedNameTerm(WeaponType wType);
    
    // Get localized description
    public string GetLocalizedDescriptionTerm(WeaponType wType);
    
    // Get localized tips
    public string GetLocalizedTipsTerm(WeaponType wType);
    
    // Direct translation access
    public string GetTranslation(string term);
    
    // Get formatted description with parameters
    public string GetDescription(string term, float value);
    public string GetDescriptionPercent(string term, float value);
}
```

### Character Localization
Characters use name-based localization:

```csharp
public class CharacterData
{
    // Get full localized character name
    public string GetFullName(CharacterType t, bool ignoreSkinPrefixSuffix = false, bool splitDualCharacterNames = true);
    
    // Get untranslated name
    public string GetFullNameUntranslated();
}
```

### Other Game Systems
Other data types follow similar patterns:

```csharp
// Achievements
public class AchievementData
{
    public virtual string GetLocalizedName();
}

// Arcana
public class ArcanaData
{
    public string GetLocalizedDescriptionTerm(ArcanaType t);
}

// Enemies
public class EnemyData
{
    public string GetLocalizedDescriptionTerm(EnemyType type);
    public string GetLocalizedTipsTerm(EnemyType type);
}

// Power-ups
public class PowerUpData
{
    public string GetLocalizedName(PowerUpType type);
}

// Secrets
public class SecretData
{
    public string GetLocalizedDescriptionTerm(SecretType t);
}
```

## Language Support

### Accessing Current Language

```csharp
// Get current language name
string currentLanguage = LanguageController.GetCurrentLanguageName();

// Change language through controller
var languageController = // ... get controller reference
languageController.ApplyLanguage("it");  // Switch to Italian

// Access through platform integration
PlatformIntegration.SetCurrentLanguageCode();
```

### Multi-Language Custom Implementation

```csharp
public static class MultiLanguageSupport
{
    // Language code mapping
    private static readonly Dictionary<string, string> languageCodes = new()
    {
        ["English"] = "en",
        ["Italiano"] = "it",
        ["Español"] = "es",
        ["Français"] = "fr",
        ["Deutsch"] = "de",
        ["Português"] = "pt",
        ["Русский"] = "ru",
        ["日本語"] = "ja",
        ["한국어"] = "ko",
        ["简体中文"] = "zh-cn",
        ["繁體中文"] = "zh-tw"
    };
    
    public static string GetLanguageCode(string languageName)
    {
        return languageCodes.TryGetValue(languageName, out string code) ? code : "en";
    }
    
    public static string GetLanguageName(string code)
    {
        return languageCodes.FirstOrDefault(kvp => kvp.Value == code).Key ?? "English";
    }
}
```

## UI Localization Components

The system uses Unity components for UI localization:

```csharp
// Force parse escape characters in localized text
public class ForceParseEscapeCharacters : MonoBehaviour
{
    public TextMeshProUGUI _tmp;      // Text component
    public Localize _localize;        // Localization component
    
    public void Parse();  // Parse escape sequences
}
```

## Dynamic Parameter Replacement

The system supports parameter injection through description methods:

```csharp
// WeaponData parameter replacement
public class WeaponData
{
    // Get description with numeric value
    public string GetDescription(string term, float value);
    
    // Get description with percentage value
    public string GetDescriptionPercent(string term, float value);
    
    // Get custom description with value
    public string GetCustomDescription(WeaponType t, float value);
}

// Usage example
var weaponData = // ... get weapon data
string description = weaponData.GetDescription("damage_description", 25.5f);
string percentDesc = weaponData.GetDescriptionPercent("crit_chance", 0.15f);
```

## Best Practices

### Data-Driven Localization
- Use data class methods for consistent access patterns
- Leverage type-specific localization methods
- Access localization through proper data references

### Performance
- Cache data class references
- Use direct localization methods instead of string lookups
- Avoid repeated calls to static methods in update loops

### Custom Localization
- Hook language change events for custom content
- Implement fallback strategies for missing translations
- Use language codes for consistent identification

### Fallback Strategy
```csharp
public static string GetLocalizedTextSafe<T>(Func<T, string> localizationMethod, T data, string fallback = "")
{
    try
    {
        return localizationMethod(data) ?? fallback;
    }
    catch
    {
        return fallback;
    }
}

// Usage
string weaponName = GetLocalizedTextSafe(
    data => data.GetLocalizedNameTerm(WeaponType.WHIP), 
    weaponData, 
    "Unknown Weapon"
);
```

## Common Modding Scenarios

### Intercepting Weapon Localization
```csharp
[HarmonyPatch(typeof(WeaponData), "GetLocalizedNameTerm")]
[HarmonyPostfix]
public static void CustomWeaponNames(WeaponType wType, ref string __result)
{
    // Override weapon names for custom weapons
    if (wType == WeaponType.CUSTOM_WEAPON)
    {
        __result = CustomLocalization.GetCustomTranslation("custom_weapon_name", "Custom Weapon");
    }
}

[HarmonyPatch(typeof(WeaponData), "GetLocalizedDescriptionTerm")]
[HarmonyPostfix]
public static void CustomWeaponDescriptions(WeaponType wType, ref string __result)
{
    if (wType == WeaponType.CUSTOM_WEAPON)
    {
        __result = CustomLocalization.GetCustomTranslation("custom_weapon_desc", "A powerful custom weapon");
    }
}
```

### Custom Character Localization
```csharp
[HarmonyPatch(typeof(CharacterData), "GetFullName")]
[HarmonyPostfix]
public static void CustomCharacterNames(CharacterType t, ref string __result)
{
    if (t == CharacterType.CUSTOM_CHARACTER)
    {
        __result = CustomLocalization.GetCustomTranslation("custom_character_name", "Custom Hero");
    }
}
```

### Account Page Translation Hook
```csharp
[HarmonyPatch(typeof(AccountPage), "GetTranslation")]
[HarmonyPostfix]
public static void CustomAccountTranslations(string key, ref string __result)
{
    // Override specific account page translations
    string customTranslation = CustomLocalization.GetCustomTranslation(key, null);
    if (!string.IsNullOrEmpty(customTranslation))
    {
        __result = customTranslation;
    }
}
```

### Language Change Detection
```csharp
[HarmonyPatch(typeof(LanguageController), "ApplyLanguage")]
[HarmonyPrefix]
public static void OnLanguageChanging(string code)
{
    // Prepare for language change
    Debug.Log($"Language changing to: {code}");
}

[HarmonyPatch(typeof(LanguageController), "ApplyLanguage")]
[HarmonyPostfix]
public static void OnLanguageChanged(string code)
{
    // React to language change
    CustomLocalization.RefreshTranslations(code);
}
```