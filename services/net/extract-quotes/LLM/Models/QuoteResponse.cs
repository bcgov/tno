namespace TNO.Services.ExtractQuotes.LLM.Models;

/// <summary>
/// Quote response from the LLM
/// </summary>
public class QuoteResponse
{
    public List<QuoteItem> quotes { get; set; } = new List<QuoteItem>();
}

public class QuoteItem
{
    public int Id { get; set; }
    public string? Text { get; set; }
    public string? CanonicalSpeaker { get; set; }
    public int BeginSentence { get; set; }
}
