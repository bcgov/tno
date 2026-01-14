/// <summary>
/// Provides report section types.
/// </summary>
export enum ReportSectionTypeName {
  /// <summary>
  /// This section displays content.
  /// </summary>
  Content = 'Content',

  /// <summary>
  /// This section displays a table of contents for the whole report.
  /// </summary>
  TableOfContents = 'TableOfContents',

  /// <summary>
  /// This section displays text only.
  /// </summary>
  Text = 'Text',

  /// <summary>
  /// This section displays charts.
  /// </summary>
  MediaAnalytics = 'MediaAnalytics',

  /// <summary>
  /// This section displays content in a gallery (i.e. front page images).
  /// </summary>
  Gallery = 'Gallery',

  /// <summary>
  /// This section displays an image.
  /// </summary>
  Image = 'Image',

  /// <summary>
  /// This section displays data from JSON, XML, or other 3rd party data.
  /// </summary>
  Data = 'Data',
}
