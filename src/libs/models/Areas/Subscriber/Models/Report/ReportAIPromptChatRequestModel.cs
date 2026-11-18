using TNO.API.Models.Settings;

namespace TNO.API.Areas.Subscriber.Models.Report;

/// <summary>
/// ReportAIPromptChatRequestModel class, a turn of a conversation with the section's model about
/// how to change the section's prompt. The first turn has no messages and the server composes the
/// context; later turns pass the conversation returned by the previous turn back.
/// </summary>
public class ReportAIPromptChatRequestModel
{
    #region Properties
    /// <summary>get/set - The draft section settings the question is about.</summary>
    public ReportSectionSettingsModel Settings { get; set; } = new();

    /// <summary>get/set - The user's question.</summary>
    public string Question { get; set; } = "";

    /// <summary>get/set - The conversation so far; empty starts a new one.</summary>
    public IEnumerable<ReportAIPromptChatMessageModel> Messages { get; set; } = Array.Empty<ReportAIPromptChatMessageModel>();

    /// <summary>get/set - A test run whose transcript and output give the conversation its context.</summary>
    public Guid? TestId { get; set; }
    #endregion
}
