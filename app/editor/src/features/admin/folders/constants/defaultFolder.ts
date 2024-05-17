import { IFolderForm } from '../interfaces';

export const defaultFolder: IFolderForm = {
  id: 0,
  name: '',
  description: '',
  ownerId: undefined,
  sortOrder: 0,
  isEnabled: true,
  filterId: undefined,
  settings: {
    searchUnpublished: false,
    addAgeLimit: 0,
    keepAgeLimit: 0,
    autoPopulate: false,
  },
  content: [],
  reports: [],
  events: [],
};
