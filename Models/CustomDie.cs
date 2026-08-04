using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;
using System.Text.RegularExpressions;

// A single face of a custom die. The face's numeric Value is what a formula sees;
// Text and/or Image are optional cosmetic overrides shown when the die is rolled on its own.
public class DieFace
{
    public int Value { get; set; }
    public string? Text { get; set; }
    public string? Image { get; set; }

    public bool HasOverride => !string.IsNullOrWhiteSpace(Text) || !string.IsNullOrWhiteSpace(Image);
}

// A campaign-scoped custom die (e.g. Fate/Fudge dice, symbol dice) for systems that use
// dice with their own faces. Mechanically it behaves like a standard N-sided die: in a
// formula it is treated as a number, but individual faces can be overridden with text or
// an image, which is displayed when the die is rolled by itself.
public class CustomDie
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    // Token used to reference the die in a formula, e.g. "fate" -> written as "1dfate".
    // Must start with a letter so it never collides with standard numeric dice (d20, 2d6, ...).
    public string Name { get; set; } = "";

    public int Sides { get; set; } = 6;

    [Column("faces")]
    public string FacesJson { get; set; } = "[]";

    [NotMapped]
    public List<DieFace> Faces
    {
        get => JsonSerializer.Deserialize<List<DieFace>>(FacesJson ?? "[]") ?? new();
        set => FacesJson = JsonSerializer.Serialize(value);
    }

    public int CampaignId { get; set; }
    public Campaign? Campaign { get; set; }

    // The override (if any) for a rolled value.
    public DieFace? FaceFor(int value) => Faces.FirstOrDefault(f => f.Value == value && f.HasOverride);

    private static Regex TokenRegex(string name) =>
        new(@"\b(\d*)d" + Regex.Escape(name) + @"\b", RegexOptions.IgnoreCase);

    // Replaces custom-die references (e.g. "1dfate") with their underlying numeric die
    // (e.g. "1d6") so the roll can be evaluated by the standard roll service.
    public static string TranslateFormula(string? formula, IEnumerable<CustomDie> dice)
    {
        if (string.IsNullOrWhiteSpace(formula)) return formula ?? "";
        var result = formula;
        foreach (var die in dice)
        {
            if (string.IsNullOrWhiteSpace(die.Name)) continue;
            result = TokenRegex(die.Name).Replace(result, m => $"{m.Groups[1].Value}d{die.Sides}");
        }
        return result;
    }

    // If the whole formula is a single custom die used on its own (e.g. "dfate" or "1dfate"),
    // returns that die so its rolled face can be displayed. Anything more (extra dice, modifiers,
    // or a count > 1) means it is being used as a number, so this returns null.
    public static CustomDie? MatchLone(string? formula, IEnumerable<CustomDie> dice)
    {
        if (string.IsNullOrWhiteSpace(formula)) return null;
        var trimmed = formula.Trim();
        foreach (var die in dice)
        {
            if (string.IsNullOrWhiteSpace(die.Name)) continue;
            var m = Regex.Match(trimmed, @"^(\d*)d" + Regex.Escape(die.Name) + @"$", RegexOptions.IgnoreCase);
            if (m.Success && (m.Groups[1].Value is "" or "1"))
                return die;
        }
        return null;
    }
}
