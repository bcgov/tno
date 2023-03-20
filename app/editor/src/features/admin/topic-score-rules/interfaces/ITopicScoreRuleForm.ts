import { IAuditColumnsModel } from 'tno-core';

export interface ITopicScoreRuleForm extends IAuditColumnsModel {
  id: number;
  sourceId: number | '';
  seriesId: number | '';
  section: string;
  pageMin: number | '';
  pageMax: number | '';
  hasImage?: boolean;
  timeMin: string;
  timeMax: string;
  characterMin?: number | '';
  characterMax?: number | '';
  score: number | '';
  sortOrder: number;
  remove?: boolean;
}
