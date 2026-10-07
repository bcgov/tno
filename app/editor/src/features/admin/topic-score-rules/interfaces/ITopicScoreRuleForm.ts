import { type IAuditColumnsModel } from 'tno-core';

/** The rule drawer's form values. A page range shares one prefix ("A") across min and max. */
export interface ITopicScoreRuleForm extends IAuditColumnsModel {
  id: number;
  sourceId: number;
  seriesId: number | '';
  section: string;
  pagePrefix: string;
  pageMin: number | '';
  pageMax: number | '';
  /** '' is any, 'true' requires an image, 'false' requires none. */
  hasImage: '' | 'true' | 'false';
  timeMin: string;
  timeMax: string;
  characterMin: number | '';
  characterMax: number | '';
  score: number | '';
  sortOrder: number;
}
