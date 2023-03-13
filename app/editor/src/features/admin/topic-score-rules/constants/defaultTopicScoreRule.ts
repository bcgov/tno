import { ITopicScoreRuleForm } from '../interfaces';

export const defaultTopicScoreRule: ITopicScoreRuleForm = {
  id: 0,
  sourceId: 0,
  section: '',
  pageMin: '',
  pageMax: '',
  timeMin: '',
  timeMax: '',
  characterMin: '',
  characterMax: '',
  score: 0,
  sortOrder: 0,
};
