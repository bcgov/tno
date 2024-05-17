export interface IFolderSettingsModel extends Record<string, any> {
  /// <summary>
  /// get/set - The age limit of content that remains in the folder when cleaned.
  /// 0 days will clean out the folder entirely when schedule runs.
  /// </summary>
  keepAgeLimit: number;

  /// <summary>
  /// get/set - If the folder auto populates.
  /// </summary>
  autoPopulate: boolean;
}
