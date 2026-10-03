namespace Tranqui.Domain.Reputation;

/// <summary>
/// Recognizes names that describe a relationship rather than identify a caller ("Mamá", "Mi amor", "Jefe").
/// Those names are never uploaded; the number still counts as saved by someone. Compares canonical forms,
/// so casing and accents do not matter. Extend the word list here as new cases appear.
/// </summary>
public static class PersonalNameFilter
{
    private const int MinimumLetters = 2;

    private static readonly HashSet<string> personalWords =
    [
        "mama", "mami", "mamita", "madre", "papa", "papi", "papito", "padre", "amor", "mi amor", "amorcito", "mi vida",
        "vida", "cielo", "mi cielo", "bebe", "baby", "mi rey", "mi reina", "esposo", "esposa", "mi esposo", "mi esposa",
        "novio", "novia", "mi novio", "mi novia", "hermano", "hermana", "hermanito", "hermanita", "hijo", "hija",
        "abuelo", "abuela", "abue", "tio", "tia", "primo", "prima", "suegro", "suegra", "cunado", "cunada",
        "jefe", "jefa", "mi jefe", "mi jefa", "ex", "mi ex", "vecino", "vecina", "yo", "casa", "mi casa", "trabajo",
        "oficina", "amigo", "amiga", "parcero", "parcera", "parce",
    ];

    public static bool IsPersonal(CallerName name)
    {
        ArgumentNullException.ThrowIfNull(name);

        return personalWords.Contains(name.CanonicalValue)
            || name.CanonicalValue.Count(char.IsLetter) < MinimumLetters;
    }
}
