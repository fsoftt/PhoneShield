using System.Xml.Linq;

namespace Tranqui.BackOffice.Tests;

public sealed class LocalizationTests
{
    [Fact]
    public void SpanishAndEnglish_HaveTheSameKeys()
    {
        Keys("Strings.resx").Should().BeEquivalentTo(Keys("Strings.en.resx"));
    }

    private static IEnumerable<string> Keys(string file)
    {
        var path = Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "..", "src", "BackOffice", "Resources", file);

        return XDocument.Load(path).Root!.Elements("data").Select(data => data.Attribute("name")!.Value);
    }
}
