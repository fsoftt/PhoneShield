using System.Collections;
using System.Globalization;
using System.Reflection;
using System.Resources;
using System.Text.RegularExpressions;
using Tranqui.App.Core.Resources;

namespace Tranqui.App.Core.Tests.Resources;

/// <summary>Every user-facing text exists, with the same placeholders, in Spanish (default) and English.</summary>
public sealed partial class LocalizationTests
{
    private static readonly CultureInfo english = CultureInfo.GetCultureInfo("en");

    public static TheoryData<string> TextKeys { get; } =
        [.. typeof(Texts).GetProperties(BindingFlags.Public | BindingFlags.Static)
            .Where(property => property.PropertyType == typeof(string))
            .Select(property => property.Name)];

    [Fact]
    public void SpanishAndEnglish_HaveTheSameKeys()
    {
        Keys(CultureInfo.InvariantCulture).Should().BeEquivalentTo(Keys(english));
    }

    [Theory]
    [MemberData(nameof(TextKeys))]
    public void EveryText_ExistsInBothLanguagesWithTheSamePlaceholders(string key)
    {
        var spanish = Texts.ResourceManager.GetString(key, CultureInfo.InvariantCulture);
        var translated = Texts.ResourceManager.GetString(key, english);

        spanish.Should().NotBeNullOrWhiteSpace();
        translated.Should().NotBeNullOrWhiteSpace();
        Placeholders(translated!).Should().Equal(Placeholders(spanish!));
    }

    [Fact]
    public void Texts_FollowTheDeviceLanguage()
    {
        var original = CultureInfo.CurrentUICulture;
        try
        {
            CultureInfo.CurrentUICulture = CultureInfo.GetCultureInfo("en-US");
            Texts.Cancel.Should().Be("Cancel");

            CultureInfo.CurrentUICulture = CultureInfo.GetCultureInfo("es-CO");
            Texts.Cancel.Should().Be("Cancelar");

            CultureInfo.CurrentUICulture = CultureInfo.GetCultureInfo("fr-FR");
            Texts.Cancel.Should().Be("Cancelar", "Spanish is the fallback for languages without a translation");
        }
        finally
        {
            CultureInfo.CurrentUICulture = original;
        }
    }

    private static HashSet<string> Keys(CultureInfo culture)
    {
        // Not disposed: the ResourceManager caches and shares this set.
        var set = Texts.ResourceManager.GetResourceSet(culture, createIfNotExists: true, tryParents: false)
            ?? throw new MissingManifestResourceException(culture.Name);

        return set.Cast<DictionaryEntry>().Select(entry => (string)entry.Key).ToHashSet();
    }

    private static List<string> Placeholders(string text) =>
        PlaceholderPattern().Matches(text).Select(match => match.Value).Order(StringComparer.Ordinal).ToList();

    [GeneratedRegex(@"\{\d+(:[^}]*)?\}")]
    private static partial Regex PlaceholderPattern();
}
