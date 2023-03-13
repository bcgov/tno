import { IAuditColumnsModel, ISourceModel } from '.';

export interface ITopicScoreRuleModel extends IAuditColumnsModel {
  id: number;
  sourceId: number;
  source?: ISourceModel;
  section?: string;
  pageMin?: number;
  pageMax?: number;
  hasImage?: boolean;
  timeMin?: string;
  timeMax?: string;
  characterMin?: number;
  characterMax?: number;
  score: number;
  sortOrder: number;
  remove?: boolean;
}
