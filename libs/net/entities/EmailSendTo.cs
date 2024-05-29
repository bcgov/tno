namespace TNO.Entities;

/// <summary>
/// EmailSentTo enum, provides a way to control how email is sent to subscribers.
/// </summary>
public enum EmailSentTo
{
    /// <summary>
    /// Who to send the email to.
    /// </summary>
    To = 0,
    /// <summary>
    /// Carbon copy
    /// </summary>
    CC = 1,
    /// <summary>
    /// Blind carbon copy
    /// </summary>
    BCC = 2,
}
