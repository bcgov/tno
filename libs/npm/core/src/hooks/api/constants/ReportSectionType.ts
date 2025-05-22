/// <summary>
/// Provides report section types.
/// </summary>
export enum ReportSectionType {
  /// <summary>
  /// This section displays content.
  /// </summary>
  Content = 0,

  /// <summary>
  /// This section displays a table of contents for the whole report.
  /// </summary>
  TableOfContents = 1,

  /// <summary>
  /// This section displays text only.
  /// </summary>
  Text = 2,

  /// <summary>
  /// This section displays charts.
  /// </summary>
  MediaAnalytics = 3,

  /// <summary>
  /// This section displays content in a gallery (i.e. front page images).
  /// </summary>
  Gallery = 4,

  /// <summary>
  /// This section displays an image.
  /// </summary>
  Image = 5,

  /// <summary>
  /// This section displays data from JSON, XML, or other 3rd party data.
  /// </summary>
  Data = 5,
}
