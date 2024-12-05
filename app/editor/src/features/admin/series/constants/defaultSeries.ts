import { ISeriesForm } from '../interfaces';

export const defaultSeries: ISeriesForm = {
  id: 0,
  name: '',
  description: '',
  sourceId: '',
  isEnabled: true,
  isOther: false,
  sortOrder: 0,
  autoTranscribe: false,
  useInTopics: false,
  isCBRASource: false,
};
