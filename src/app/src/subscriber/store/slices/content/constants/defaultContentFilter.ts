import { IFilterSettingsModel } from '@/shared';

export const defaultContentFilter: IFilterSettingsModel = {
  contentTypes: [],
  mediaTypeIds: [],
  otherSource: '',
  searchUnpublished: false,
  size: 500,
  sort: [],
  sourceIds: [],
  userId: 0,
};
