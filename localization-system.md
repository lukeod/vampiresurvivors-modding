# Localization System

Vampire Survivors uses the I2 Localization system for multi-language support and runtime text management.

## Core Components

### LocalizationManager
**Location**: `Il2CppI2.Loc.LocalizationManager`

Static manager providing global access to localization:

```csharp
// Get source containing a specific term
public static LanguageSourceData GetSourceContaining(string term, bool fallbackToFirst = true);

// Access all language sources
public static List<LanguageSourceData> Sources;

// Update all sources
public static bool UpdateSources();
```

### LanguageSourceData
**Location**: `Il2CppI2.Loc.LanguageSourceData`

Core data container for localization terms:

```csharp
public class LanguageSourceData
{
    // Term management
    public TermData AddTerm(string term);
    public TermData AddTerm(string NewTerm, eTermType termType, bool SaveSource = true);
    public bool ContainsTerm(string term);
    
    // Language configuration
    public List<LanguageData> mLanguages;
    public string[] mLanguageNames;
    
    // Update settings
    public eGoogleUpdateFrequency GoogleUpdateFrequency;
    public MissingTranslationAction OnMissingTranslation;
}
```

### TermData Structure
Individual localization term configuration:

```csharp
public class TermData
{
    public string Term;                    // Unique identifier
    public eTermType TermType;            // Type (Text, Font, etc.)
    public string Description;            // Optional description
    public Il2CppStringArray Languages;  // Translations per language
    public Il2CppStructArray<byte> Flags; // Term-specific flags
    public Il2CppStringArray Languages_Touch; // Touch-specific translations
}
```

## Term Format Patterns

### Standard Term Structure
Terms follow hierarchical naming conventions:

```
category/{identifier}suffix
```

Common patterns:
- `weaponLang/{WEAPONID}name` - Weapon display names
- `weaponLang/{WEAPONID}description` - Weapon descriptions
- `weaponLang/{WEAPONID}tips` - Weapon usage tips
- `characterLang/{CHARACTERID}name` - Character names
- `characterLang/{CHARACTERID}description` - Character descriptions
- `stageLang/{STAGEID}name` - Stage names
- `itemLang/{ITEMID}name` - Item names

### LocalizedString
**Location**: `Il2CppI2.Loc.LocalizedString`

Runtime string localization wrapper:

```csharp
public sealed class LocalizedString
{
    public string mTerm;                    // Term identifier
    public bool mRTL_IgnoreArabicFix;      // RTL text handling
    public int mRTL_MaxLineLength;         // RTL line length
    public bool mRTL_ConvertNumbers;       // Number conversion for RTL
    public bool m_DontLocalizeParameters;  // Parameter localization
}
```

## Runtime Localization Injection

### Adding Custom Terms

```csharp
// Get the appropriate language source
var source = LocalizationManager.GetSourceContaining("weaponLang/{HELLFIRE}name");
if (source == null)
{
    source = LocalizationManager.Sources[0]; // Fallback to first source
}

// Add new weapon localization
string weaponId = "CUSTOM_WEAPON";
string nameKey = $"weaponLang/{{{weaponId}}}name";
string descKey = $"weaponLang/{{{weaponId}}}description";
string tipsKey = $"weaponLang/{{{weaponId}}}tips";

// Add terms if they don't exist
if (!source.ContainsTerm(nameKey))
{
    var nameTerm = source.AddTerm(nameKey, eTermType.Text);
    nameTerm.Languages[0] = "Custom Weapon";  // English
    // Add other language translations as needed
}

if (!source.ContainsTerm(descKey))
{
    var descTerm = source.AddTerm(descKey, eTermType.Text);
    descTerm.Languages[0] = "A powerful custom weapon";
}

if (!source.ContainsTerm(tipsKey))
{
    var tipsTerm = source.AddTerm(tipsKey, eTermType.Text);
    tipsTerm.Languages[0] = "Use wisely for maximum effect";
}
```

### Hooking Localization Updates

```csharp
[HarmonyPatch(typeof(LocalizationManager), "UpdateSources")]
[HarmonyPostfix]
public static void OnLocalizationUpdate()
{
    // Called when localization sources are updated
    // Ideal time to inject custom terms
    InjectCustomLocalization();
}
```

## Enums and Configuration

### eTermType
Term categories:

```csharp
public enum eTermType
{
    Text,          // General text
    Font,          // Font assets
    Texture,       // Texture references
    AudioClip,     // Audio localization
    GameObject,    // GameObject references
    Sprite,        // Sprite assets
    Material,      // Material assets
    Child,         // Child objects
    Mesh,          // 3D text meshes
    TextMeshPFont, // TextMeshPro fonts
    Object,        // Generic objects
    Video          // Video assets
}
```

### MissingTranslationAction
Behavior for missing translations:

```csharp
public enum MissingTranslationAction
{
    Empty,        // Show empty string
    Fallback,     // Use fallback language
    ShowWarning,  // Display warning message
    ShowTerm      // Show the term key itself
}
```

### eGoogleUpdateFrequency
Google Translate sync frequency:

```csharp
public enum eGoogleUpdateFrequency
{
    Always,
    Never,
    Daily,
    Weekly,
    Monthly,
    OnlyOnce,
    EveryOtherDay
}
```

## Integration with Game Systems

### WeaponData Localization
Weapons use localized strings for display:

```csharp
public class WeaponData
{
    // Gets localized weapon name
    public string GetTranslation()
    {
        string term = $"weaponLang/{{{weaponType}}}name";
        return LocalizationManager.GetTranslation(term);
    }
}
```

### Character Localization
Similar pattern for characters:

```csharp
public class CharacterData
{
    public string GetLocalizedName()
    {
        string term = $"characterLang/{{{characterType}}}name";
        return LocalizationManager.GetTranslation(term);
    }
}
```

## Language Support

### Accessing Current Language

```csharp
// Get current language
string currentLanguage = LocalizationManager.CurrentLanguage;

// Get all available languages
var languages = LocalizationManager.GetAllLanguages();

// Change language
LocalizationManager.CurrentLanguage = "Italian";
```

### Multi-Language Term Addition

```csharp
public static void AddMultiLanguageTerm(string termKey, Dictionary<string, string> translations)
{
    var source = LocalizationManager.Sources[0];
    var term = source.AddTerm(termKey, eTermType.Text);
    
    // Map language names to indices
    for (int i = 0; i < source.mLanguageNames.Length; i++)
    {
        string langName = source.mLanguageNames[i];
        if (translations.ContainsKey(langName))
        {
            term.Languages[i] = translations[langName];
        }
    }
}

// Usage
var translations = new Dictionary<string, string>
{
    ["English"] = "Fire Sword",
    ["Italian"] = "Spada di Fuoco",
    ["Spanish"] = "Espada de Fuego",
    ["French"] = "Épée de Feu"
};
AddMultiLanguageTerm("weaponLang/{FIRESWORD}name", translations);
```

## RTL Language Support

The system includes built-in support for right-to-left languages:

```csharp
var localizedString = new LocalizedString
{
    mTerm = "arabicTerm",
    mRTL_IgnoreArabicFix = false,  // Apply Arabic text fixes
    mRTL_MaxLineLength = 50,       // Maximum line length for RTL
    mRTL_ConvertNumbers = true     // Convert numbers to Arabic numerals
};
```

## Dynamic Parameter Replacement

Localized strings support parameter injection:

```csharp
// Term definition with parameter
"itemLang/{GOLD}description" = "Collect {0} gold coins"

// Runtime replacement
string description = LocalizationManager.GetTranslation("itemLang/{GOLD}description", 100);
// Result: "Collect 100 gold coins"
```

## Best Practices

### Term Naming
- Use consistent category prefixes (weaponLang, characterLang, etc.)
- Include content type in suffix (name, description, tips)
- Use curly braces for IDs in term keys

### Performance
- Cache frequently accessed translations
- Batch term additions during loading
- Avoid runtime term lookups in update loops

### Organization
- Group related terms by category
- Maintain consistent translation quality
- Document custom term patterns

### Fallback Strategy
```csharp
public static string GetLocalizedTextSafe(string termKey, string fallback = "")
{
    var source = LocalizationManager.GetSourceContaining(termKey);
    if (source != null && source.ContainsTerm(termKey))
    {
        return LocalizationManager.GetTranslation(termKey);
    }
    return fallback;
}
```

## Common Modding Scenarios

### Adding Weapon Localization
```csharp
public static void AddWeaponLocalization(WeaponType weaponType, string name, string description)
{
    var source = LocalizationManager.Sources[0];
    
    // Add name
    string nameKey = $"weaponLang/{{{weaponType}}}name";
    if (!source.ContainsTerm(nameKey))
    {
        var term = source.AddTerm(nameKey, eTermType.Text);
        term.Languages[0] = name;
    }
    
    // Add description
    string descKey = $"weaponLang/{{{weaponType}}}description";
    if (!source.ContainsTerm(descKey))
    {
        var term = source.AddTerm(descKey, eTermType.Text);
        term.Languages[0] = description;
    }
}
```

### Localizing Custom UI
```csharp
public static void LocalizeCustomUI(string uiElementId, string text)
{
    string termKey = $"ui/custom/{uiElementId}";
    var source = LocalizationManager.Sources[0];
    
    var term = source.AddTerm(termKey, eTermType.UI);
    term.Languages[0] = text;
    
    // Apply to UI element
    uiElement.GetComponent<Localize>().SetTerm(termKey);
}
```