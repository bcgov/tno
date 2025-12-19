namespace TNO.Services.AutoClipper.LLM;

/// <summary>
/// Represents a suggested clip from the LLM response.
/// </summary>
public record ClipDefinition(string Title, string Summary, TimeSpan Start, TimeSpan End)
{
    public bool IsValid => End > Start;
}
