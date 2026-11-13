export enum WorkOrderTypeName {
  /// <summary>
  /// A request for content to be sent for transcription.
  /// </summary>
  Transcription = 'Transcription',

  /// <summary>
  /// A request for content to be sent for natural language processing.
  /// </summary>
  NaturalLanguageProcess = 'NaturalLanguageProcess',

  /// <summary>
  /// A request for a remote file.
  /// </summary>
  FileRequest = 'FileRequest',

  /// <summary>
  /// A request for content to be sent for FFmpeg actions.
  /// </summary>
  FFmpeg = 'FFmpeg',

  /// <summary>
  /// A request for content to be sent for auto clipping and transcription.
  /// </summary>
  AutoClip = 'AutoClip',
}
