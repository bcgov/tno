import { ITopicModel } from '.';

export interface IContentTopicModel extends ITopicModel {
  score: number;
  /** The score was set by an editor or arrived with ingest, so it is not recalculated. */
  isScoreOverridden?: boolean;
  /** The topic score rule that produced a calculated score. */
  scoreRuleId?: number;
}
