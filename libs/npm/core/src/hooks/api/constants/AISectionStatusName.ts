export enum AISectionStatusName {
  /// <summary>
  /// The section has its output.
  /// </summary>
  Ready = 'Ready',

  /// <summary>
  /// The section could not be generated.
  /// </summary>
  Failed = 'Failed',

  /// <summary>
  /// No generator has started the section.
  /// </summary>
  NotStarted = 'NotStarted',

  /// <summary>
  /// A generator is producing the section.
  /// </summary>
  Generating = 'Generating',
}
