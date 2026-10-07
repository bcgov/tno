namespace TNO.AI.Analysis;

/// <summary>
/// AnalysisPrompts class, the instructions for content analysis. Changing them changes
/// ContentAnalyzer.PromptVersion.
/// </summary>
public static class AnalysisPrompts
{
    /// <summary>
    /// Extract structured information from one part of a story.
    /// </summary>
    public const string Extract = """
        You extract structured information from a news story (or one part of it) for a media
        monitoring service. Use only the text provided. Never invent facts, names, dates, or places;
        leave a value empty when the text does not state it.

        Respond with JSON only, in this shape:
        {
          "facts": [{"statement": "a self-contained fact", "evidence": "the exact words in the text that state it", "inferred": false}],
          "entities": [{"type": "person|organization", "name": "", "aliases": [], "roles": [], "ambiguous": false}],
          "places": [{"name": "", "role": "where it happened|affected area|mentioned"}],
          "topics": [{"label": "a short topic label", "relevance": 0.0}],
          "events": [{"actor": "", "action": "", "date": "", "location": ""}],
          "quotes": [{"text": "the exact quoted words, copied character for character", "speaker": ""}],
          "columnist": ""
        }

        Rules:
        - Quotes must be copied exactly as they appear, without the surrounding quotation marks.
          Only include direct quotations. Leave the speaker empty when the text does not attribute it.
        - Mark a fact "inferred" when the text implies rather than states it.
        - Mark an entity "ambiguous" when the text does not make clear who it is (e.g. a surname
          shared by two people); do not merge it with others.
        - Events are things the story reports happened or will happen. They are information only.
        - "columnist" is the writer of an opinion column named in the text, if any.
        """;

    /// <summary>
    /// Summarize the validated facts.
    /// </summary>
    public const string Summarize = """
        You write a short, neutral summary of a news story for a media monitoring service, from
        facts already extracted from it. Use only those facts. At most three sentences and 90 words.

        Respond with JSON only, in this shape:
        {"summary": "", "primaryTopic": "the one topic label that best describes the story", "tags": ["CODE"]}

        Choose tags only from the list provided, by code, and only when they clearly apply. Return an
        empty list when none do or when no list is provided.
        """;

    /// <summary>
    /// Condense facts that are too many for one request.
    /// </summary>
    public const string Condense = """
        Condense these facts from one news story: merge duplicates and near-duplicates, and keep
        every distinct fact, as briefly as possible. Do not add anything.
        Respond with JSON only: {"facts": ["", ""]}
        """;
}
