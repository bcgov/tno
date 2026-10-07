/** A source that uses topics, with its topic score rule summary. */
export interface ITopicScoreSourceModel {
  id: number;
  code: string;
  name: string;
  /** The source itself uses topics. */
  useInTopics: boolean;
  /** One of the source's series uses topics. */
  seriesUseInTopics: boolean;
  ruleCount: number;
  /** The score used when no rule matches; undefined scores unmatched content 0. */
  topicDefaultScore?: number;
}
