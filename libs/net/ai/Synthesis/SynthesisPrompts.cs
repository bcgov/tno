namespace TNO.AI.Synthesis;

/// <summary>
/// SynthesisPrompts class, the instructions for each synthesis step. The section's own prompts
/// decide what matters; these instructions fix the response shape and the evidence rules.
/// </summary>
public static class SynthesisPrompts
{
    /// <summary>
    /// The version of the prompts and pipeline. Changing it invalidates stored results.
    /// </summary>
    public const string PipelineVersion = "4";

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
    /// The citation rule for the final free-text step.
    /// </summary>
    public const string FinalCitationRule = """
        The findings below were synthesized from the report's stories. Write the section from them,
        following the instructions. Cite the stories behind each point with their handles in square
        brackets, e.g. [S3] or [S3][S7]; they are replaced with links to the stories. Use these
        handles even when the instructions ask for URLs or HTML links: URLs are not part of the
        findings. Never construct a URL or wrap a handle in an HTML link. Do not invent handles or facts.
        """;
}
