import { type ITopicScoreRuleForm } from '../interfaces';

export const defaultTopicScoreRule = (sourceId: number): ITopicScoreRuleForm => ({
  id: 0,
  sourceId,
  seriesId: '',
  section: '',
  pagePrefix: '',
  pageMin: '',
  pageMax: '',
  hasImage: '',
  timeMin: '',
  timeMax: '',
  characterMin: '',
  characterMax: '',
  score: 0,
  sortOrder: 0,
});
