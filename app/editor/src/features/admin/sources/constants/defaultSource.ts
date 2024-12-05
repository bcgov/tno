import { ISourceForm } from '../interfaces';

export const defaultSource: ISourceForm = {
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
  ownerId: '',
  mediaTypeId: '',
  mediaTypeSearchMappings: [],
  configuration: {
    timeZone: '',
  },
  actions: [],
  metrics: [],
  isCBRASource: false,
};
