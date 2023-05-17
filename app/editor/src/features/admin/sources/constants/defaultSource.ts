import { ISourceModel } from 'tno-core';

export const defaultSource: ISourceModel = {
  id: 0,
  name: '',
  code: '',
  shortName: '',
  description: '',
  isEnabled: false,
  sortOrder: 0,
  licenseId: 0,
  autoTranscribe: false,
  disableTranscribe: false,
  useInTopics: false,
  configuration: {},
  actions: [],
  metrics: [],
};
