/** How Content-Analysis assigns topics to content. */
export enum TopicPopulationModeName {
  /** Assign existing, active staff topics only. */
  ExistingOnly = 'ExistingOnly',
  /** Assign existing topics, and create a topic when none matches. */
  AllowCreate = 'AllowCreate',
}

/** The topic population settings on the topics admin page. */
export interface ITopicPopulationSettingsModel {
  mode: TopicPopulationModeName;
}
