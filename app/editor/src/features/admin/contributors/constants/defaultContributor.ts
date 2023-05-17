import { IContributorForm } from '../interfaces';

export const defaultContributor: IContributorForm = {
  id: 0,
  name: '',
  description: '',
  sourceId: '',
  isEnabled: true,
  sortOrder: 0,
  autoTranscribe: false,
};
