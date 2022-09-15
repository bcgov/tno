import { ISourceModel } from 'hooks/api-editor';

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
  actions: [],
  metrics: [],
};
