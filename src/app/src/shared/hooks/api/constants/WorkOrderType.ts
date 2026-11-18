export enum WorkOrderType {
  /// <summary>
  /// A request for content to te sent for transcription.
  /// </summary>
  Transcription = 0,

  /// <summary>
  /// A request for content to be sent for natural language processing.
  /// </summary>
  NaturalLanguageProcess = 1,

  /// <summary>
  /// A request for a remote file.
  /// </summary>
  FileRequest = 2,

  /// <summary>
  /// A request for content to be sent for FFmpeg actions.
  /// </summary>
  FFmpeg = 3,

  /// <summary>
  /// A request for content to be sent for auto clipping and transcription.
  /// </summary>
  AutoClip = 4,
}
