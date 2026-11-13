/// <summary>
/// Provides a way to control how email is sent to subscribers.
/// </summary>
export enum EmailSendTo {
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
