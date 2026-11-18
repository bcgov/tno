/** The content to test the rules against: a saved content item, or entered values. */
export interface ITopicScoreTestRequestModel {
  contentId?: number;
  sourceId?: number;
  seriesId?: number;
  section?: string;
  page?: string;
  hasImage: boolean;
  /** ISO date, UTC. */
  publishedOn?: string;
  characterCount: number;
}

/** Whether one rule matched and, if not, the first condition that failed. */
export interface ITopicScoreRuleEvaluationModel {
  ruleId: number;
  sortOrder: number;
  score: number;
  isMatch: boolean;
  failedCondition?: string;
  reason?: string;
}

/** The score the rules give the tested content. */
export interface ITopicScoreTestResultModel {
  isEligible: boolean;
  score: number;
  ruleId?: number;
  isSourceDefault: boolean;
  input: ITopicScoreTestRequestModel;
  evaluations: ITopicScoreRuleEvaluationModel[];
}
