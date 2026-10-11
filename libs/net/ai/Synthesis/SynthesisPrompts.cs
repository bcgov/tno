namespace TNO.AI.Synthesis;

/// <summary>
/// SynthesisPrompts class, the fixed text of each synthesis step. The map and reduce steps fix the
/// findings shape the pipeline parses; the final step only describes the data it carries, and the
/// section's own prompt decides the output's content and format.
/// </summary>
public static class SynthesisPrompts
{
    /// <summary>
    /// The version of the prompts and pipeline. Changing it invalidates stored results.
    /// </summary>
    public const string PipelineVersion = "8";

    /// <summary>
    /// The shape every map and reduce step returns.
    /// </summary>
    public const string FindingsSchema = """
        Respond with JSON only, in this shape:
        {"findings":[{"topic":"a short topic label","statement":"one self-contained statement","sources":["S1","S4"]}]}
        """;

    /// <summary>
    /// Extract findings from a batch of stories.
    /// </summary>
    /// <param name="sectionInstructions">The section's own prompts.</param>
    /// <returns></returns>
    public static string Map(string sectionInstructions) => $"""
        You read a batch of news stories for one section of a media monitoring report and extract the
        findings that section needs. The section's instructions decide what matters:

        <section-instructions>
        {sectionInstructions}
        </section-instructions>

        Rules:
        - Use only the stories in this batch. Do not add outside knowledge or invent details.
        - Each story starts with an evidence handle in brackets, e.g. [S12]. Cite the handles of every
          story that supports a statement in "sources". Never cite a handle that is not in the batch.
        - A story may arrive in parts; treat its parts as one story.
        - The metadata line under a story's handle names the report section(s) it belongs to as
          "section: ...". Use it when the instructions refer to sections.
        - Merge statements that say the same thing, and keep distinct facts separate.
        - Leave out stories that are not relevant to the section.
        {FindingsSchema}
        """;

    /// <summary>
    /// Merge findings into fewer, consolidated findings.
    /// </summary>
    /// <param name="sectionInstructions">The section's own prompts.</param>
    /// <returns></returns>
    public static string Reduce(string sectionInstructions) => $"""
        You consolidate findings for one section of a media monitoring report. The section's
        instructions decide what matters:

        <section-instructions>
        {sectionInstructions}
        </section-instructions>

        Each finding starts with its handle in brackets, e.g. [F3], then its topic in parentheses.

        Rules:
        - Combine findings that say the same thing into one; in "sources" cite the handles of every
          finding it combines, e.g. ["F3","F8"]. A finding kept as it is cites its own handle.
        - Keep distinct facts; never drop a finding the section needs.
        - Use consistent topic labels; merge labels that name the same topic.
        - Do not add facts that are not in the findings. Only cite handles that appear here.
        {FindingsSchema}
        """;

    /// <summary>
    /// What the final free-text request carries. It describes the data and the post-processing and
    /// gives no formatting or citation directions: the section's prompt, which follows the data under
    /// "## Instructions", controls the output entirely.
    /// </summary>
    public const string FinalDataGuide = """
        This message carries the data for one section of a media monitoring report, then the
        instructions that decide the section's content and format. The data is described here; the
        instructions are under "## Instructions" at the end.
        Coverage by topic counts the stories behind each topic's findings. A previous report, when
        given in an earlier message, lists the same counts and its findings for comparison.
        Findings for this report were synthesized from the report's stories. Each finding starts with
        its topic in parentheses, states one fact, and ends with the references of the stories that
        support it, e.g. [S3].
        The Report sections array lists every section of this report in order: its "label", its
        "anchor" (a link to the section within this report, or null when the section is not rendered)
        and "stories", the references of the stories it holds.
        The Story data array maps each reference to its "url" (the subscriber story page, or null),
        its "anchor" (the story within this report, or null), its "section" (the section(s) it belongs
        to) and the metadata fields that were provided (any of "source", "mediaType", "series",
        "publishedOn", "byline", "contributor"). A field that is absent, null or empty has no value.
        In the response, a bracketed reference such as [S3] or [S3][S7] is replaced by a link to that
        story, an HTML link whose href is not one of the url or anchor values above is replaced by its
        text, a link to an anchor loses any target, and the HTML is sanitized.
        """;
}
